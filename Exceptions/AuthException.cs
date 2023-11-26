using HELMoliday.Filters;

namespace HELMoliday.Exceptions;

public class InvalidCredentialsException : HttpResponseException
{
    public InvalidCredentialsException() : base(StatusCodes.Status401Unauthorized, "Identifiants invalides") { }
}

public class AccountLockedOutException : HttpResponseException
{
    public AccountLockedOutException() : base(StatusCodes.Status401Unauthorized, "Compte verrouillé") { }
}