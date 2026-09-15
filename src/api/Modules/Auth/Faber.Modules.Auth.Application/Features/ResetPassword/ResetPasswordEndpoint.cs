using ErrorOr;
using Faber.Modules.Auth.Application.Groups;
using Faber.Modules.Common.PublicApi.RateLimiting;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Auth.Application.Features.ResetPassword;

public class ResetPasswordEndpoint(ILogger<ResetPasswordEndpoint> logger)
    : Endpoint<ResetPasswordRequest, Results<NoContent, BadRequest<Error>>>
{
    public override void Configure()
    {
        Put("reset-password");
        Group<AuthGroup>();
        Version(1);
        AllowAnonymous();
        Options(x => x.RequireRateLimiting(RateLimitPolicies.AuthPasswordReset));
    }

    public override async Task<Results<NoContent, BadRequest<Error>>> ExecuteAsync(
        ResetPasswordRequest request,
        CancellationToken ct)
    {
        var path = HttpContext.Request.Path.Value;
        logger.LogInformation("[HTTP PUT] {Path} started", path);
        var response = await request.MapToCommand().ExecuteAsync(ct);

        if (response.IsError)
        {
            logger.LogWarning(
                "[HTTP PUT] {Path} failed: {Error}",
                path,
                response.FirstError.Description);

            return TypedResults.BadRequest(response.FirstError);
        }

        logger.LogInformation("[HTTP PUT] {Path} completed successfully", path);

        return TypedResults.NoContent();
    }
}