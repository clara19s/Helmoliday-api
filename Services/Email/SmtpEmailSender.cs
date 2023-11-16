using HELMoliday.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace HELMoliday.Services.Email;
public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _emailConfig;
    public SmtpEmailSender(EmailSettings emailConfig)
    {
        _emailConfig = emailConfig;
    }

    public Task SendEmailAsync(Message message)
    {
        var emailMessage = CreateEmailMessage(message);
        Send(emailMessage);
        return Task.CompletedTask;
    }

    private MimeMessage CreateEmailMessage(Message message)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(_emailConfig.FromName, _emailConfig.FromEmailAddress));
        emailMessage.To.AddRange(message.To);
        emailMessage.ReplyTo.AddRange(message.To);
        emailMessage.Subject = message.Subject;
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message.Content };
        return emailMessage;
    }
    private void Send(MimeMessage mailMessage)
    {
        using var client = new SmtpClient();
        try
        {
            client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, SecureSocketOptions.Auto);
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            client.Authenticate(_emailConfig.UserName, _emailConfig.Password);
            client.Send(mailMessage);
        }
        catch
        {
            // TODO: Log le message d'erreur
            throw;
        }
        finally
        {
            client.Disconnect(true);
            client.Dispose();
        }
    }
}
