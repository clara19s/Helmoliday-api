using HELMoliday.Data;
using HELMoliday.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HELMoliday.Controllers;
[Route("statistics")]
[ApiController]
public class StatisticsController : ControllerBase
{
    private readonly HELMolidayContext _context;

    public StatisticsController(HELMolidayContext context)
    {
        _context = context;
    }

    [HttpGet("users")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTotalUsers()
    {
        var userCount = await _context.Users.CountAsync();
        return Ok(userCount);
    }

    [HttpGet("holidays")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUsersOnHolidayByCountry([FromQuery] string dateString)
    {
        try
        {
            var date = DateConverter.ConvertStringToDate(dateString);

            var usersOnHolidayByCountry = await _context.Holidays
                .Include(h => h.Address)
                .Include(h => h.Invitations)
                .Where(h => h.StartDate <= date && date <= h.EndDate)
                .SelectMany(h => h.Invitations, (holiday, invitation) => new { holiday.Address.Country, invitation.User })
                .GroupBy(x => x.Country)
                .Select(group => new { Country = group.Key, Count = group.Count() })
                .ToListAsync();

            return Ok(usersOnHolidayByCountry);
        }
        catch (Exception e)
        {
            return BadRequest(new { error = e.Message });
        }
    }
}
