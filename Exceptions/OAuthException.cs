using HELMoliday.Filters;

namespace HELMoliday.Exceptions;
public class OAuthException : HttpResponseException
{
    public OAuthException(string message) : base(StatusCodes.Status401Unauthorized, message) { }
}

public sealed class OAuthInvalidTokenException : OAuthException
{
    public OAuthInvalidTokenException() : base("The given token was not valid.") { }
}