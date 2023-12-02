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
        var message = new Message()
        {
            To = new List<MessageAddress> { new MessageAddress(request.FullName, request.Email) },
            Subject = request.Subject,
            CarbonCopy = new List<MessageAddress> { new MessageAddress("HELMoliday", "admin@schiltz.dev") },
            Content = request.Message
        };
        await emailSender.SendEmailAsync(message);
        return CreatedAtAction(nameof(Post), request);
    }
}
