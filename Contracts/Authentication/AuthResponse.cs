namespace HELMoliday.Contracts.Authentication;

public record AuthResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string ProfilePicture,
    string Token);
