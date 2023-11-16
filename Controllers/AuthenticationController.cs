using HELMoliday.Contracts.Authentication;
using HELMoliday.Models;
using HELMoliday.Services.JwtToken;
using HELMoliday.Services.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HELMoliday.Controllers;

[Route("auth")]
[ApiController]
[AllowAnonymous]
public class AuthenticationController : ControllerBase
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthenticationController(SignInManager<User> signInManager, UserManager<User> userManager, IJwtTokenGenerator jwtTokenGenerator)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return BadRequest("User not found.");
        }

        var signinResult = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!signinResult)
        {
            return BadRequest("Invalid credentials.");
        }

        var authResponse = new AuthResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            request.Email,
            _jwtTokenGenerator.GenerateToken(user)
        );

        return Ok(authResponse);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _userManager.FindByEmailAsync(request.Email) is not null)
        {
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
