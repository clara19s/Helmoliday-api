using HELMoliday.Contracts.User;
using HELMoliday.Data;
using Microsoft.AspNetCore.Mvc;

namespace HELMoliday.Controllers;
[Route("users")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly HELMolidayContext _context;

    public UsersController(HELMolidayContext context)
    {
        _context = context;
    }

    [HttpGet()]
    public async Task<IActionResult> GetUsers([FromQuery] string? query)
    {
        var users = _context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(query))
        {
            users = users.Where(u => u.FirstName.Contains(query) || u.LastName.Contains(query) || u.Email.Contains(query));
        }

        return Ok(users.ToList()
            .Select(u => new UserInfoResponse(
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email)));
    }
}
