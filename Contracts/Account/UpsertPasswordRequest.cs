namespace HELMoliday.Contracts.Account;
public record UpsertPasswordRequest(
    string CurrentPassword,
    string NewPassword);
