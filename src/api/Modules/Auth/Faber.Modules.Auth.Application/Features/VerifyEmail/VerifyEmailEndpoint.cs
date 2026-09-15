using ErrorOr;
using Faber.Modules.Auth.Application.Groups;
using Faber.Modules.Common.PublicApi.RateLimiting;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Auth.Application.Features.VerifyEmail;

public class VerifyEmailEndpoint(ILogger<VerifyEmailEndpoint> logger) :
    Endpoint<VerifyEmailRequest, Results<Ok<VerifyEmailResponse>, BadRequest<Error>>>
{
    public override void Configure()
    {
        Post("verify-email");
        Group<AuthGroup>();
        Version(1);
        AllowAnonymous();
        Options(x => x.RequireRateLimiting(RateLimitPolicies.AuthStrict));
    }

    public override async Task<Results<Ok<VerifyEmailResponse>, BadRequest<Error>>> ExecuteAsync(
        VerifyEmailRequest request,
        CancellationToken ct)
    {
        var path = HttpContext.Request.Path.Value;
        logger.LogInformation("[HTTP POST] {Path} started", path);
        var response = await request.MapToCommand().ExecuteAsync(ct);

        if (response.IsError)
        {
            logger.LogWarning(
                "[HTTP POST] {Path} failed: {Error}",
                path,
                response.FirstError.Description);

            return TypedResults.BadRequest(response.FirstError);
        }

        logger.LogInformation("[HTTP POST] {Path} completed successfully", path);

        return TypedResults.Ok(response.Value);
    }
}