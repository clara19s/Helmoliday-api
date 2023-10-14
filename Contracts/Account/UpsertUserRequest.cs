namespace HELMoliday.Contracts.Account;

public record UpsertUserRequest(
    string Email,
    string FirstName,
    string LastName);

