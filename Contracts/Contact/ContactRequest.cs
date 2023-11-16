namespace HELMoliday.Contracts.Contact;
public record ContactRequest(
    string FullName,
    string Subject,
    string Email,
    string Message);