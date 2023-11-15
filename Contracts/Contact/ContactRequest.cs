namespace HELMoliday.Contracts.Contact;
public record ContactRequest(
    string FullName,
    string Email,
    string Message);