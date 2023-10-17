using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HELMoliday.Data;
using HELMoliday.Models;
using HELMoliday.Contracts.Invitation;

namespace HELMoliday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvitationsController : ControllerBase
    {
        private readonly HELMolidayContext _context;

        public InvitationsController(HELMolidayContext context)
        {
            _context = context;
        }


        // POST: api/Invitations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Invitation>> PostInvitation( [FromBody] InvitationRequest invitation)
        {
          if (_context.Invitations == null)
          {
              return Problem("Entity set 'HELMolidayContext.Invitations'  is null.");
          }
            var invitationModel = new Invitation
            {
                UserId = Guid.Parse(invitation.UserId),
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
            var invitation = await _context.Invitations.FindAsync(id);
            if (invitation == null)
            {
                return NotFound();
            }

            _context.Invitations.Remove(invitation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InvitationExists(Guid id)
        {
            return (_context.Invitations?.Any(e => e.UserId == id)).GetValueOrDefault();
        }
    }
}
