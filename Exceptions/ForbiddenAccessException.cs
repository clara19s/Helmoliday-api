using HELMoliday.Filters;

namespace HELMoliday.Exceptions;
public class ForbiddenAccessException : HttpResponseException
{
    public ForbiddenAccessException() : base(StatusCodes.Status403Forbidden)
    {
    }

    public ForbiddenAccessException(string message) : base(StatusCodes.Status403Forbidden, message)
    {
    }
}
