using Microsoft.AspNetCore.Mvc;
using HELMoliday.Data;
using HELMoliday.Models;
using Microsoft.AspNetCore.Identity;
using HELMoliday.Contracts.Account;
using HELMoliday.Contracts.Authentication;
using HELMoliday.Services.JwtToken;

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
                return BadRequest(new {error = "E-mail already in use."});
            }

            user.Email = dto.Email;
            user.UserName = dto.Email;
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;

            await _userManager.UpdateAsync(user);

            return Ok(new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, _jwtTokenGenerator.GenerateToken(user)));
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

            return NoContent();
        }
    }
}
