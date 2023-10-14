namespace HELMoliday.Contracts.Account;
public record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName);
