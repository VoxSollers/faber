using ErrorOr;
using Faber.Modules.Common.PublicApi.RateLimiting;
using Faber.Modules.Identity.Application.Groups;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Identity.Application.Features.VerifyActionToken;

public class VerifyActionTokenEndpoint(ILogger<VerifyActionTokenEndpoint> logger)
    : Endpoint<VerifyActionTokenRequest, Results<Ok<VerifyActionTokenResponse>, BadRequest<Error>>>
{
    public override void Configure()
    {
        Post("verify-action-token");
        Group<IdentityGroup>();
        Version(1);
        AllowAnonymous();
        Options(x => x.RequireRateLimiting(RateLimitPolicies.AuthStrict));
    }

    public override async Task<Results<Ok<VerifyActionTokenResponse>, BadRequest<Error>>> ExecuteAsync(
        VerifyActionTokenRequest request,
        CancellationToken ct)
    {
        var path = HttpContext.Request.Path.Value;
        logger.LogInformation("[HTTP POST] {Path} started", path);
        var response = await request.MapToCommand().ExecuteAsync(ct);

        if (!response.IsValid)
        {
            logger.LogWarning("[HTTP POST] {Path} failed: Token is invalid", path);

            return TypedResults.BadRequest(Error.Validation());
        }

        logger.LogInformation("[HTTP POST] {Path} completed successfully", path);

        return TypedResults.Ok(response);
    }
}