using Microsoft.AspNetCore.Mvc;
using HELMoliday.Data;
using HELMoliday.Models;
using HELMoliday.Contracts.Invitation;
using Microsoft.AspNetCore.Identity;
using HELMoliday.Options;
using HELMoliday.Services.Email;
using HELMoliday.Exceptions;
using HELMoliday.Filters;

namespace HELMoliday.Controllers;
[Route("invitations")]
[ApiController]
public class InvitationsController : ControllerBase
{
    private readonly HELMolidayContext _context;
    private readonly ILogger<InvitationsController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IEmailSender _emailSender;

    public InvitationsController(HELMolidayContext context, UserManager<User> userManager, IEmailSender emailSender, ILogger<InvitationsController> logger)
    {
        _context = context;
        _userManager = userManager;
        _emailSender = emailSender;
        _logger = logger;
    }

    /// <summary>
    /// Crée une invitation pour un utilisateur dans une période de vacances.
    /// </summary>
    /// <param name="invitation">Un objet reprenant l'identifiant d'une période de vacances ainsi que l'e-mail d'un utilisateur.</param>
    /// <returns></returns>
    /// <response code="204">L'invitation a été créée.</response>
    /// <response code="400">L'invitation est invalide.</response>
    /// <response code="404">L'utilisateur ou la période de vacances n'a pas été trouvé.</response>
    [HttpPost]
    public async Task<ActionResult<Invitation>> PostInvitation([FromBody] InvitationRequest invitation)
    {
        if (_context.Invitations == null)
        {
            return Problem("Entity set 'HELMolidayContext.Invitations'  is null.");
        }

        var holiday = _context.Holidays.Where(h => h.Id == Guid.Parse(invitation.HolidayId)).FirstOrDefault();

        if (holiday == null)
        {
            throw new NotFoundException("La période de vacances n'a pas été trouvée.");
        }

        var user = _context.Users.Where(u => u.Email == invitation.Email).FirstOrDefault();

        if (user == null)
        {
            throw new NotFoundException("L'utilisateur n'a pas été trouvé.");
        }

        if (_context.Invitations.Where(i => i.HolidayId == holiday.Id && i.UserId == user.Id).FirstOrDefault() != null)
        {
            throw new HttpResponseException(400, "L'utilisateur a déjà été invité dans cette période de vacances.");
        }

        var invitationModel = new Invitation
        {
            UserId = user.Id,
            HolidayId = holiday.Id,
        };
        _context.Invitations.Add(invitationModel);
        await _context.SaveChangesAsync();

        try
        {
            MessageAddress email = new(user.FirstName, user.Email);
            Message message = new()
            {
                To = new List<MessageAddress> { email },
                Subject = $"Vous avez été invité dans le groupe \"{holiday.Name}\"",
                Content = $"Cher(e) {user.FullName},<br><br>Vous avez une nouvelle invitation pour le groupe {holiday.Name}"
            };
            await _emailSender.SendEmailAsync(message);
        } catch (Exception)
        {
            _logger.LogError($"Failed to send email to {user.Email}");
        }

        return NoContent();
    }

    /// <summary>
    /// Supprime l'utilisateur courant d'une période de vacances.
    /// </summary>
    /// <param name="holidayId">Identifiant unique d'une période de vacances.</param>
    /// <returns></returns>
    /// <response code="204">L'utilisateur a été supprimé de la période de vacances.</response>
    /// <response code="404">L'utilisateur ou la période de vacances n'a pas été trouvé.</response>
    [HttpDelete("{holidayId}")]
    public async Task<IActionResult> DeleteInvitation([FromRoute] Guid holidayId)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);

        var invitations = _context.Invitations.Where(i => i.HolidayId == holidayId);
        var invitation = invitations.Where(i => i.UserId == user.Id).FirstOrDefault();

        if (invitation == null)
        {
            return NotFound();
        }

        if (invitations.Count() == 1)
        {
            _context.Invitations.Remove(invitation);
            var holiday = _context.Holidays.Where(h => h.Id == holidayId).FirstOrDefault();
            _context.Holidays.Remove(holiday);
        }
        else
        {
            _context.Invitations.Remove(invitation);
        }

        await _context.SaveChangesAsync();


        return NoContent();
    }
}
