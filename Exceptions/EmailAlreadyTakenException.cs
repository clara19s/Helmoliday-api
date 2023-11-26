using HELMoliday.Filters;

namespace HELMoliday.Exceptions;
public class EmailAlreadyTakenException : HttpResponseException
{
    public EmailAlreadyTakenException() : base(StatusCodes.Status409Conflict, "Email already taken") { }
}
