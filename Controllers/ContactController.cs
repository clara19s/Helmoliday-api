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
    /// <summary>
    /// Envoie un e-mail à l'administrateur du site ainsi qu'une copie à l'utilisateur.
    /// </summary>
    /// <param name="request">Une demande de contact.</param>
    /// <returns>Une réponse HTTP 201.</returns>
    /// <response code="201">Une réponse HTTP 201.</response>
    [HttpPost]
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
