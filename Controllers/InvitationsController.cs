using Microsoft.AspNetCore.Mvc;
using HELMoliday.Data;
using HELMoliday.Models;
using HELMoliday.Contracts.Invitation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Internal;

namespace HELMoliday.Controllers;
[Route("invitations")]
[ApiController]
public class InvitationsController : ControllerBase
{
    private readonly HELMolidayContext _context;
    private readonly UserManager<User> _userManager;


    public InvitationsController(HELMolidayContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    
       // POST: api/Invitations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
    public async Task<ActionResult<Invitation>> PostInvitation([FromBody] InvitationRequest invitation)
    {
        if (_context.Invitations == null)
        {
            return Problem("Entity set 'HELMolidayContext.Invitations'  is null.");
        }

        var user = _context.Users.Where(u => u.Email == invitation.Email).FirstOrDefault();

        if (user == null)
        {
            return NotFound("Aucun utilisateur ne correspond à cette adresse e-mail.");
        }

        var invitationModel = new Invitation
        {
            UserId = user.Id,
            HolidayId = Guid.Parse(invitation.HolidayId),

        };
        _context.Invitations.Add(invitationModel);
        await _context.SaveChangesAsync();
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
        var user =  await _userManager.GetUserAsync(HttpContext.User);

        var invitations = _context.Invitations.Where(i => i.HolidayId == id);
        var invitation = invitations.Where (i => i.UserId == user.Id).FirstOrDefault();
        
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
