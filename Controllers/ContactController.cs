using HELMoliday.Contracts.Contact;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HELMoliday.Controllers;
[Route("contact")]
[ApiController]
public class ContactController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromBody] ContactRequest request)
    {
        return Ok();
    }
}
