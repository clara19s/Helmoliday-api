using HELMoliday.Filters;

namespace HELMoliday.Exceptions;
public class NotFoundException : HttpResponseException
{
    public NotFoundException() : base(StatusCodes.Status404NotFound)
    {
    }

    public NotFoundException(string message) : base(StatusCodes.Status404NotFound, message)
    {
    }
}
