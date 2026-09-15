using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace Faber.Api.ExceptionHandlers;

public class FallbackExceptionHandler(ILogger<FallbackExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception at {Path}", httpContext.Request.Path.Value);

        var traceId = httpContext.TraceIdentifier;

        var problemDetails = new ProblemDetails
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Title = "Internal Server Error",
            Detail = $"An unexpected error occurred. Please contact support with TraceId: {traceId}",
            Extensions = { ["traceId"] = traceId }
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}