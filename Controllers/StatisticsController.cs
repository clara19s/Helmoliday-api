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

    /// <summary>
    /// Récupère le nombre total d'utilisateurs.
    /// </summary>
    /// <returns>Le nombre total d'utilisateurs.</returns>
    /// <response code="200">Le nombre total d'utilisateurs.</response>
    [HttpGet("users")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTotalUsers()
    {
        var userCount = await _context.Users.CountAsync();
        return Ok(userCount);
    }

    /// <summary>
    /// Récupère le nombre total d'utilisateurs par pays.
    /// </summary>
    /// <param name="dateString">Une date au format "YYYY-mm-dd HH:mm".</param>
    /// <returns>Un tableau contenant le nombre total d'utilisateurs par pays.</returns>
    /// <response code="200">Un tableau contenant le nombre total d'utilisateurs par pays.</response>
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
                .Distinct()
                .ToListAsync();

            return Ok(usersOnHolidayByCountry);
        }
        catch (Exception e)
        {
            return BadRequest(new { error = e.Message });
        }
    }
}
