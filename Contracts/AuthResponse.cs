namespace HELMoliday.Contracts;

public record AuthResponse(
    string FirstName,
    string LastName,
    string Email,
    string Token);
