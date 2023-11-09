using HELMoliday.Data;
using HELMoliday.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HELMoliday.Controllers;
[Route("[controller]")]
[ApiController]
public class StatisticsController : ControllerBase
{
    private readonly HELMolidayContext _context;

    public StatisticsController(HELMolidayContext context)
    {
        _context = context;
    }

    [HttpGet("holidays")]
    [AllowAnonymous]
    public async Task<IActionResult> GetHolidaysByDate([FromQuery] string dateString)
    {
        try
        {
            var date = DateConverter.ConvertStringToDate(dateString);

            var usersOnHolidayCount = _context.Holidays
                .Where(h => h.StartDate <= date && date <= h.EndDate)
                .Join(_context.Invitations,
                      holiday => holiday.Id,
                      invitation => invitation.HolidayId,
                     (holiday, invitation) => invitation.User)
                .Distinct()
                .Count();

            return Ok(usersOnHolidayCount);
        }
        catch (Exception e)
        {
            return BadRequest(new { error = e.Message });
        }
    }
}
