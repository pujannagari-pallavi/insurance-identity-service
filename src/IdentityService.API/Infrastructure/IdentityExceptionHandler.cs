using IdentityService.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Infrastructure;

public sealed class IdentityExceptionHandler(ILogger<IdentityExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException and not AuthenticationException)
        {
            return false;
        }

        logger.LogWarning(exception, "Request failed with a handled application exception.");

        var problemDetails = new ProblemDetails
        {
            Status = exception is ValidationException
                ? StatusCodes.Status400BadRequest
                : StatusCodes.Status401Unauthorized,
            Title = exception is ValidationException ? "Validation failed." : "Authentication failed.",
            Detail = exception.Message
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}