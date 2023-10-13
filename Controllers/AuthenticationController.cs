using HELMoliday.Contracts;
using HELMoliday.Models;
using HELMoliday.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HELMoliday.Controllers;

[Route("Auth")]
[ApiController]
[AllowAnonymous]
public class AuthenticationController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    public AuthenticationController(UserManager<User> userManager, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    [Route("Login")]
    [HttpPost]
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
            user.FirstName,
            user.LastName,
            request.Email,
            _jwtTokenGenerator.GenerateToken(user)
        );

        return Ok(authResponse);
    }

    [Route("Register")]
    [HttpPost]
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
            request.FirstName,
            request.LastName,
            request.Email,
            _jwtTokenGenerator.GenerateToken(user)
        );

        return Ok(authResponse);
    }
}
