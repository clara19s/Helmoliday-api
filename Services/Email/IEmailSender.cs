using HELMoliday.Options;
using MimeKit;

namespace HELMoliday.Services.Email;
public interface IEmailSender
{
    Task SendEmailAsync(Message message);
}

public class Message
{
    public List<MailboxAddress> To { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }
    public Message(IEnumerable<MessageAddress> to, string subject, string content)
    {
        To = new List<MailboxAddress>();
        To.AddRange(to.Select(x => new MailboxAddress(x.Name, x.EmailAddress)));
        Subject = subject;
        Content = content;
    }
}
