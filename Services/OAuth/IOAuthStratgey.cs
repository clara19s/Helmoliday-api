using HELMoliday.Contracts.Authentication;

namespace HELMoliday.Services.OAuth;

public interface IOAuthStrategy
{
    Task<UserInfo> AuthenticateAsync(string code);
}

public record UserInfo(
       string Email,
       string FirstName,
       string LastName);