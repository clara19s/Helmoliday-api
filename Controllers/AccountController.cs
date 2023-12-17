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

        /// <summary>
        /// Récupère les informations d'un utilisateur.
        /// </summary>
        /// <returns>Les informations de l'utilisateur.</returns>
        /// <response code="200">Les informations de l'utilisateur.</response>
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

        /// <summary>
        /// Met à jour les informations d'un utilisateur.
        /// </summary>
        /// <param name="dto">Les informations mises à jour de l'utilisateur.</param>
        /// <returns>Les informations de l'utilisateur mises à jour ainsi qu'un nouveau token JWT.</returns>
        /// <response code="200">Les informations de l'utilisateur mises à jour ainsi qu'un nouveau token JWT.</response>
        /// <response code="400">Les informations de l'utilisateur sont invalides.</response>
        /// <response code="404">L'utilisateur n'a pas été trouvé.</response>
        /// <response code="409">L'adresse email est déjà utilisée.</response>
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

        /// <summary>
        /// Change la photo de profil de l'utilisateur.
        /// </summary>
        /// <param name="file">La nouvelle image de profil de l'utilisateur.</param>
        /// <returns>Les informations de l'utilisateur mises à jour ainsi qu'un nouveau token JWT.</returns>
        /// <response code="200">Les informations de l'utilisateur mises à jour ainsi qu'un nouveau token JWT.</response>
        /// <response code="400">Les informations de l'utilisateur sont invalides.</response>
        [HttpPut("picture")]
        public async Task<IActionResult> ChangeProfilePicture(IFormFile file, [FromServices] IFileUploadService fileUploadService)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var localFilePath = await fileUploadService.UploadFileAsync(file, user.Id.ToString());
            user.ProfilePicture = localFilePath;

            await _userManager.UpdateAsync(user);

            return Ok(new AuthResponse(user.Id, user.FirstName, user.LastName, user.Email, ConvertToUrl(user.ProfilePicture), _jwtTokenGenerator.GenerateToken(user)));
        }

        /// <summary>
        /// Récupère la photo de profil de l'utilisateur.
        /// </summary>
        /// <returns>Un fichier représentant la photo de profil de l'utilisateur actuellement connecté.</returns>
        /// <response code="200">Un fichier représentant la photo de profil de l'utilisateur actuellement connecté.</response>
        /// <response code="404">L'utilisateur n'a pas été trouvé.</response>
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

        /// <summary>
        /// Supprime le compte de l'utilisateur.
        /// </summary>
        /// <returns></returns>
        /// <response code="204">Le compte de l'utilisateur a été supprimé.</response>
        /// <response code="404">L'utilisateur n'a pas été trouvé.</response>
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
