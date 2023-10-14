namespace HELMoliday.Contracts.Account;
public record UpsertPasswordRequest(
    string oldPassword,
    string newPassword);
