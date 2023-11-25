using Google.Apis.Auth;
using HELMoliday.Contracts.Authentication;
using HELMoliday.Data;
using HELMoliday.Models;
using HELMoliday.Options;
using HELMoliday.Services.Email;
using HELMoliday.Services.JwtToken;
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
        _ = emailSender.SendEmailAsync(message);
        _ = NotifyStats();

        return Ok(authResponse);
    }

    [HttpPost("oauth/google")]
    public async Task<IActionResult> GoogleLogin([FromBody] OAuthTokenRequest token)
    {
        GoogleJsonWebSignature.Payload validatedToken;
        try
        {
            validatedToken = await GoogleJsonWebSignature.ValidateAsync(token.Token);
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogError($"Invalid Google JWT: {ex.Message}");
            return Unauthorized("Invalid token.");
        }

        var user = await _userManager.FindByEmailAsync(validatedToken.Email);
        if (user == null)
        {
            user = new User
            {
                Email = validatedToken.Email,
                UserName = validatedToken.Email,
                FirstName = validatedToken.GivenName,
                LastName = validatedToken.FamilyName
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                _logger.LogError($"Failed to create user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                return BadRequest("Failed to create user.");
            }
            _ = NotifyStats();
        }

        var authResponse = new AuthResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
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
}
