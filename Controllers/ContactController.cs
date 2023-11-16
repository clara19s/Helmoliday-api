using HELMoliday.Contracts.Contact;
using HELMoliday.Options;
using HELMoliday.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HELMoliday.Controllers;
[Route("contact")]
[AllowAnonymous]
[ApiController]
public class ContactController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromBody] ContactRequest request, [FromServices] IEmailSender emailSender)
    {
        var message = new Message(new List<MessageAddress> { new MessageAddress(request.FullName, request.Email) }, request.Subject, request.Message);
        await emailSender.SendEmailAsync(message);
        return CreatedAtAction(nameof(Post), request);
    }
}
