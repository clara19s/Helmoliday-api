using HELMoliday.Contracts.User;
using HELMoliday.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<IActionResult> GetUsersByEmail([FromQuery] string query)
    {

        if (string.IsNullOrEmpty(query))
        {
            return BadRequest("Veuillez spécifier une adresse e-mail valide.");
        }

        var user =await _context.Users.Where(u => u.Email == query).FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound($"L'adresse e-mail {query} ne correspond à aucun compte utilisateur.");
        }

        return Ok(new UserInfoResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email));
    }
}
