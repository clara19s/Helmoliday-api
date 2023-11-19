using HELMoliday.Contracts.Authentication;
using HELMoliday.Data;
using HELMoliday.Models;
using HELMoliday.Options;
using HELMoliday.Services.Email;
using HELMoliday.Services.JwtToken;
using HELMoliday.Services.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
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

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return BadRequest("User not found.");
        }

        if (user.LockoutEnabled && user.LockoutEnd > DateTimeOffset.UtcNow)
        {
            _logger.LogInformation($"La tentative de connexion au compte de l'utilisateur {user.Id} a échoué. Le compte est verrouillé.");
            return BadRequest("Account locked.");
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
            return BadRequest("Invalid credentials.");
        }

        user.AccessFailedCount = 0;
        _ = _userManager.UpdateAsync(user);

        var authResponse = new AuthResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            request.Email,
            _jwtTokenGenerator.GenerateToken(user)
        );
        _logger.LogInformation($"La connexion au compte de l'utilisateur {user.Id} a été réussie.");
        return Ok(authResponse);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, [FromServices] IEmailSender emailSender)
    {
        if (await _userManager.FindByEmailAsync(request.Email) is not null)
        {
            _logger.LogInformation($"La création du compte pour l'utilisateur {request.Email} a échoué. Le compte existe déjà.");
            return BadRequest(new { message = "User already exists" });
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
            _jwtTokenGenerator.GenerateToken(user)
        );
        MessageAddress email = new MessageAddress(user.FirstName, user.Email);
        Message message = new Message()
        {
            To = new List<MessageAddress> { email },
            Subject = "Création de compte",
            Content = " Cher(e) client(e), <br><br> Félicitations ! Votre compte Helmoliday a été créé avec succès. <br><br> L'équipe Helmoliday "
        };
        emailSender.SendEmailAsync(message);

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

        return Ok(authResponse);
    }

    [Route("oauth/google")]
    [HttpPost]
    public async Task<IActionResult> LoginWithGoogle([FromBody] OAuthRequest request, [FromServices] GoogleOAuthService authService)
    {
        // On convertir le jeton d'autorisation en jeton d'accès.
        var accessToken = await authService.GetTokenAsync(request.AuthorizationCode);

        if (accessToken == null)
            return BadRequest();

        // On récupère les informations de l'utilisateur liées au jeton d'accès.
        var userInfo = await authService.GetUserInfoAsync(accessToken);

        if (userInfo == null)
            return BadRequest();

        var user = await _userManager.FindByEmailAsync(userInfo.Email);

        if (user == null) // Si l'utilisateur n'existe pas, alors on le crée
        {
            user = new User
            {
                UserName = userInfo.Email,
                Email = userInfo.Email,
                FirstName = userInfo.GivenName,
                LastName = userInfo.FamilyName
            };
            var signUpResult = await _userManager.CreateAsync(user);
            if (!signUpResult.Succeeded)
            {
                return BadRequest("User could not be created.");
            }
        }

        // Sinon, l'utilisateur existe et on peut lui retourner une AuthResponse
        var authResponse = new AuthResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            _jwtTokenGenerator.GenerateToken(user)
        );
        return Ok(authResponse);
    }
}
