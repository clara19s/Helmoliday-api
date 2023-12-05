using Microsoft.AspNetCore.Mvc;
using HELMoliday.Data;
using HELMoliday.Models;
using Microsoft.AspNetCore.Identity;
using HELMoliday.Contracts.Account;
using HELMoliday.Contracts.Authentication;
using HELMoliday.Services.JwtToken;
using HELMoliday.Contracts.User;
using PusherServer;
using HELMoliday.Exceptions;
using HELMoliday.Services.ImageUpload;

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

            return Ok(new UserInfoResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                ConvertToUrl(user.ProfilePicture)));
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
                throw new EmailAlreadyTakenException();
            }

            user.Email = dto.Email;
            user.UserName = dto.Email;
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;

            await _userManager.UpdateAsync(user);

            return Ok(new AuthResponse(user.Id, user.FirstName, user.LastName, user.Email, ConvertToUrl(user.ProfilePicture), _jwtTokenGenerator.GenerateToken(user)));
        }

        [HttpPut("picture")]
        public async Task<IActionResult> ChangeProfilePicture(IFormFile file, [FromServices] IFileUploadService fileUploadService)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var localFilePath = await fileUploadService.UploadFileAsync(file, user.Id.ToString());
            user.ProfilePicture = localFilePath;

            await _userManager.UpdateAsync(user);

            return Ok(new AuthResponse(user.Id, user.FirstName, user.LastName, user.Email, ConvertToUrl(user.ProfilePicture), _jwtTokenGenerator.GenerateToken(user)));
        }

        [HttpGet("picture")]
        public async Task<IActionResult> GetProfilePicture([FromServices] IFileUploadService fileUploadService)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
            {
                return NotFound(new { error = "User not found." });
            }
            var image = await fileUploadService.GetFileAsync(user.ProfilePicture);

            return File(image, "image/jpeg");
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

            _ = pusher.TriggerAsync("stats", "update:userCount", _context.Users.Count());

            return NoContent();
        }

        private string ConvertToUrl(string filePath)
        {
            var protocol = HttpContext.Request.IsHttps ? "https" : "http";
            var domaineName = HttpContext.Request.Host.Value.Contains("localhost") ? HttpContext.Request.Host.Value : "porthos-intra.cg.helmo.be/Q210266";
            return $"{protocol}://{domaineName}{filePath}";
        }
    }
}
