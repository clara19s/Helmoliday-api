using Microsoft.AspNetCore.Mvc;
using HELMoliday.Data;
using HELMoliday.Models;
using Microsoft.AspNetCore.Identity;
using HELMoliday.Contracts.Account;
using HELMoliday.Contracts.Authentication;
using HELMoliday.Services.JwtToken;
using HELMoliday.Contracts.User;
using PusherServer;
using HELMoliday.Contracts.Holiday;
using HELMoliday.Services.Weather;
using Microsoft.EntityFrameworkCore;
using HELMoliday.Services.Cal;

namespace HELMoliday.Controllers
{
    [Route("account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly HELMolidayContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AccountController(HELMolidayContext context, UserManager<User> userManager, IJwtTokenGenerator jwtTokenGenerator)
        {
            _context = context;
            _userManager = userManager;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetUserInfo()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
            {
                return NotFound(new { error = "User not found." });
            }

            return Ok(new UserInfoResponse(user.Id, user.FirstName, user.LastName, user.Email));
        }

        [Route("profile")]
        [HttpPut()]
        public async Task<IActionResult> ChangeProfileInformation(UpsertUserRequest dto)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
            {
                return NotFound(new { error = "User not found." });
            }

            if (dto.Email != user.Email && await _userManager.FindByEmailAsync(dto.Email) is not null)
            {
                return BadRequest(new { error = "E-mail already in use." });
            }

            user.Email = dto.Email;
            user.UserName = dto.Email;
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;

            await _userManager.UpdateAsync(user);

            return Ok(new AuthResponse(user.Id, user.FirstName, user.LastName, user.Email, _jwtTokenGenerator.GenerateToken(user)));
        }

        

        [Route("password")]
        [HttpPut()]
        public async Task<IActionResult> ChangePassword(UpsertPasswordRequest dto)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
            {
                return NotFound(new { error = "User not found." });
            }

            var passwordChangeResult = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!passwordChangeResult.Succeeded)
            {
                return Problem("Password couldn't be changed.");
            }

            return NoContent();
        }

        [HttpDelete()]
        public async Task<IActionResult> DeleteAccount()
        {
            if (_context.Users == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return NotFound();
            }

            var userDeletionResult = await _userManager.DeleteAsync(user);
            if (!userDeletionResult.Succeeded)
            {
                return Problem("User couldn't be deleted.");
            }

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

            return NoContent();
        }


    }
}
