namespace HELMoliday.Options;
public class EmailSettings
{
    public string FromEmailAddress { get; set; }
    public string FromName { get; set; }
    public string SmtpServer { get; set; }
    public int Port { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}

public record MessageAddress(string Name, string EmailAddress);