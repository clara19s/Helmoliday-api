using HELMoliday.Contracts.Authentication;
using HELMoliday.Data;
using HELMoliday.Exceptions;
using HELMoliday.Models;
using HELMoliday.Options;
using HELMoliday.Services.Email;
using HELMoliday.Services.JwtToken;
using HELMoliday.Services.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PusherServer;

namespace HELMoliday.Controllers;

[Route("auth")]
[ApiController]
[AllowAnonymous]
public class AuthenticationController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<AuthenticationController> _logger;
    private readonly HELMolidayContext _context;

    public AuthenticationController(UserManager<User> userManager, IJwtTokenGenerator jwtTokenGenerator, ILogger<AuthenticationController> logger, HELMolidayContext context)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Authentifie un utilisateur au moyen de son adresse email et de son mot de passe.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Une réponse contenant les informations de l'utilisateur et le token JWT.</returns>
    /// <response code="200">Retourne les informations de l'utilisateur et son token JWT.</response>
    /// <response code="400">Une ou plusieurs informations sont manquantes ou incorrectes.</response>
    /// <response code="401">Les identifiants fournis sont invalides.</response>
    /// <response code="404">Aucun compte utilisateur ne correspond à l'adresse e-mail fournie.</response>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            throw new NotFoundException("Utilisateur non trouvé");
        }

        if (user.LockoutEnabled && user.LockoutEnd > DateTimeOffset.UtcNow)
        {
            _logger.LogInformation($"La tentative de connexion au compte de l'utilisateur {user.Id} a échoué. Le compte est verrouillé.");
            throw new AccountLockedOutException();
        }

        var signinResult = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!signinResult)
        {
            user.AccessFailedCount++;
            if (user.AccessFailedCount > 3)
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(5 ^ user.AccessFailedCount);
            }
            await _userManager.UpdateAsync(user);
            _logger.LogInformation($"La tentative de connexion au compte de l'utilisateur {user.Id} a échoué.");
            throw new InvalidCredentialsException();
        }

        user.AccessFailedCount = 0;
        await _userManager.UpdateAsync(user);

        var authResponse = new AuthResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            request.Email,
            ConvertToUrl(user.ProfilePicture),
            _jwtTokenGenerator.GenerateToken(user)
        );
        _logger.LogInformation($"La connexion au compte de l'utilisateur {user.Id} a été réussie.");
        return Ok(authResponse);
    }

    /// <summary>
    /// Inscrit l'utilisateur au moyen de son adresse email, de son mot de passe, de son prénom et de son nom.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Une réponse contenant les informations de l'utilisateur et le token JWT.</returns>
    /// <response code="200">L'utilisateur a été créé avec succès.</response>
    /// <response code="400">Une ou plusieurs informations sont manquantes ou incorrectes.</response>
    /// <response code="409">Un compte utilisateur existe déjà avec l'adresse e-mail fournie.</response>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, [FromServices] IEmailSender emailSender)
    {
        if (await _userManager.FindByEmailAsync(request.Email) is not null)
        {
            _logger.LogInformation($"La création du compte pour l'utilisateur {request.Email} a échoué. Le compte existe déjà.");
            throw new EmailAlreadyTakenException();
        }

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var createUserResult = await _userManager.CreateAsync(user, request.Password);
        if (!createUserResult.Succeeded)
        {
            throw new Exception("Something went wrong...");
        }

        var authResponse = new AuthResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            ConvertToUrl(user.ProfilePicture),
            _jwtTokenGenerator.GenerateToken(user)
        );
        MessageAddress email = new(user.FirstName, user.Email);
        Message message = new()
        {
            To = new List<MessageAddress> { email },
            Subject = "Création de compte",
            Content = "Cher(e) client(e),<br><br>Félicitations ! Votre compte Helmoliday a été créé avec succès.<br><br>L'équipe Helmoliday"
        };
        await emailSender.SendEmailAsync(message);
        await NotifyStats();

        return Ok(authResponse);
    }

    /// <summary>
    /// Authentifie l'utilisateur au moyen d'une plateforme tierce.
    /// </summary>
    /// <param name="platform">L'identifiant de la plateforme tierce (google, facebook ou linkedin).</param>
    /// <param name="token">Le jeton d'authentification de la plateforme tierce.</param>
    /// <returns>Une réponse contenant les informations de l'utilisateur et le token JWT.</returns>
    /// <response code="200">Retourne les informations de l'utilisateur et son token JWT.</response>
    /// <response code="400">Une ou plusieurs informations sont manquantes ou incorrectes.</response>
    /// <response code="401">Les identifiants fournis sont invalides.</response>
    [HttpPost("oauth/{platform}")]
    public async Task<IActionResult> OAuthCallback([FromRoute] string platform, [FromBody] OAuthTokenRequest token, [FromServices] OAuthStrategyFactory authStrategyFactory)
    {
        IOAuthStrategy strategy = authStrategyFactory.GetStrategy(platform);
        var userInfo = await strategy.AuthenticateAsync(token.Token);

        var user = await _userManager.FindByEmailAsync(userInfo.Email);
        if (user == null)
        {
            user = new User
            {
                Email = userInfo.Email,
                UserName = userInfo.Email,
                FirstName = userInfo.FirstName,
                LastName = userInfo.LastName
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                _logger.LogError($"Failed to create user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                return BadRequest("Failed to create user.");
            }
            await NotifyStats();
        }

        var authResponse = new AuthResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            ConvertToUrl(user.ProfilePicture),
            _jwtTokenGenerator.GenerateToken(user)
        );

        _logger.LogInformation($"User {user.Id} logged in via Google.");
        return Ok(authResponse);
    }

    private Task NotifyStats()
    {
        var options = new PusherOptions
        {
            Cluster = "eu",
            Encrypted = false
        };

        // TODO: Get from config
        var pusher = new Pusher(
          "1700454",
          "c79fa94e85416eeb4f1e",
          "bca1b2adb1b72d81f3f3",
          options);

        pusher.TriggerAsync("stats", "update:userCount", _context.Users.Count());
        return Task.CompletedTask;
    }

    private string ConvertToUrl(string filePath)
    {
        var protocol = HttpContext.Request.IsHttps ? "https" : "http";
        var domaineName = HttpContext.Request.Host.Value.Contains("localhost") ? HttpContext.Request.Host.Value : "porthos-intra.cg.helmo.be/Q210266";
        return $"{protocol}://{domaineName}{filePath}";
    }
}
