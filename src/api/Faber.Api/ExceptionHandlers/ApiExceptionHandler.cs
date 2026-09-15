using Faber.Modules.Common.PublicApi.Shared.Responses;
using Microsoft.AspNetCore.Diagnostics;
using Refit;
using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace Faber.Api.ExceptionHandlers;

public class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ApiException apiException) return false;

        logger.LogError(apiException, "API exception at {Path}", httpContext.Request.Path.Value);

        AuthErrorResponse errorResponse;

        try
        {
            errorResponse = await apiException.GetContentAsAsync<AuthErrorResponse>() ??
                            new AuthErrorResponse("Unknown", "An unknown error occurred.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to deserialize API exception response at {Path}",
                httpContext.Request.Path.Value);

            errorResponse = new AuthErrorResponse("Unknown", "An unknown error occurred.");
        }

        var problemDetails = new ProblemDetails
        {
            Status = (int)apiException.StatusCode,
            Title = errorResponse.Error,
            Detail = errorResponse.ErrorDescription
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}