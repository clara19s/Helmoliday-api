using Google.Apis.Auth;
using HELMoliday.Exceptions;

namespace HELMoliday.Services.OAuth.Strategies;
public class GoogleOAuthStrategy : IOAuthStrategy
{
    public async Task<UserInfo> AuthenticateAsync(string code)
    {
        try
        {
            GoogleJsonWebSignature.Payload validatedToken = await GoogleJsonWebSignature.ValidateAsync(code);
            return new UserInfo(validatedToken.Email, validatedToken.GivenName, validatedToken.FamilyName);
        }
        catch (InvalidJwtException)
        {
            throw new OAuthInvalidTokenException();
        }
    }
}