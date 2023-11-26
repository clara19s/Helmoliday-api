using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace HELMoliday.Filters;

public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
{
    public int Order => int.MaxValue - 10;

    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var problemDetailsFactory =
            context.HttpContext
            .RequestServices
            .GetRequiredService<ProblemDetailsFactory>();

        if (context.Exception is HttpResponseException httpResponseException)
        {
            var problemDetails = problemDetailsFactory.CreateProblemDetails(
                context.HttpContext,
                (int)httpResponseException.StatusCode,
                detail: httpResponseException.Value?.ToString(),
                instance: context.HttpContext.Request.Path
            );

            context.Result = new ObjectResult(problemDetails);
            context.ExceptionHandled = true;
        }
    }
}

public class HttpResponseException : Exception
{
    public HttpResponseException(int statusCode, object? value = null) =>
        (StatusCode, Value) = (statusCode, value);

    public int StatusCode { get; }

    public object? Value { get; }
}