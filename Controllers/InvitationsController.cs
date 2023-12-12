using Microsoft.AspNetCore.Mvc;
using HELMoliday.Data;
using HELMoliday.Models;
using HELMoliday.Contracts.Invitation;
using Microsoft.AspNetCore.Identity;
using HELMoliday.Options;
using HELMoliday.Services.Email;

namespace HELMoliday.Controllers;
[Route("invitations")]
[ApiController]
public class InvitationsController : ControllerBase
{
    private readonly HELMolidayContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IEmailSender _emailSender;

    public InvitationsController(HELMolidayContext context, UserManager<User> userManager, IEmailSender emailSender)
    {
        _context = context;
        _userManager = userManager;
        _emailSender = emailSender;
    }

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
            return NotFound("Aucun groupe ne correspond à cet identifiant.");
        }

        var user = _context.Users.Where(u => u.Email == invitation.Email).FirstOrDefault();

        if (user == null)
        {
            return NotFound("Aucun utilisateur ne correspond à cette adresse e-mail.");
        }

        var invitationModel = new Invitation
        {
            UserId = user.Id,
            HolidayId = holiday.Id,
        };
        _context.Invitations.Add(invitationModel);
        await _context.SaveChangesAsync();

        MessageAddress email = new(user.FirstName, user.Email);
        Message message = new()
        {
            To = new List<MessageAddress> { email },
            Subject = $"Vous avez été invité dans le groupe \"{holiday.Name}\"",
            Content = $"Cher(e) {user.FullName},<br><br>Vous avez une nouvelle invitation pour le groupe {holiday.Name}"
        };
        await _emailSender.SendEmailAsync(message);

        return NoContent();
    }

    // DELETE: api/Invitations/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInvitation(Guid id)
    {
        if (_context.Invitations == null)
        {
            return NotFound();
        }
        var user = await _userManager.GetUserAsync(HttpContext.User);

        var invitations = _context.Invitations.Where(i => i.HolidayId == id);
        var invitation = invitations.Where(i => i.UserId == user.Id).FirstOrDefault();

        if (invitation == null)
        {
            return NotFound();
        }

        if (invitations.Count() == 1)
        {
            _context.Invitations.Remove(invitation);
            var holiday = _context.Holidays.Where(h => h.Id == id).FirstOrDefault();
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
