using ErrorOr;
using Faber.Modules.Auth.Application.Features.Shared;
using Faber.Modules.Auth.Application.Features.Shared.Requests;
using Faber.Modules.Auth.Application.Groups;
using Faber.Modules.Common.PublicApi;
using Faber.Modules.Common.PublicApi.RateLimiting;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Auth.Application.Features.Refresh;

public class RefreshEndpoint(ILogger<RefreshEndpoint> logger)
    : EndpointWithRefreshToken<Results<Ok<RefreshResponse>, BadRequest<Error>, UnauthorizedHttpResult>>
{
    public override void Configure()
    {
        Post("refresh");
        Group<AuthGroup>();
        Version(1);
        AllowAnonymous();
        Options(x => x.RequireRateLimiting(RateLimitPolicies.AuthRefresh));
    }

    public override async Task<Results<Ok<RefreshResponse>, BadRequest<Error>, UnauthorizedHttpResult>> ExecuteAsync(
        RefreshTokenRequest request,
        CancellationToken ct)
    {
        var path = HttpContext.Request.Path.Value;
        logger.LogInformation("[HTTP POST] {Path} started", path);
        var refreshToken = GetRefreshToken(request);

        if (string.IsNullOrEmpty(refreshToken))
        {
            logger.LogWarning("[HTTP POST] {Path} failed: No refresh token found", path);

            return TypedResults.Unauthorized();
        }

        var response = await request.MapToCommand(refreshToken).ExecuteAsync(ct);

        if (response.IsError)
        {
            logger.LogWarning(
                "[HTTP POST] {Path} failed: {Error}",
                path,
                response.FirstError.Description);

            return TypedResults.BadRequest(response.FirstError);
        }

        HttpContext.SetCookiesRefreshToken(response.Value.RefreshToken);

        logger.LogInformation("[HTTP POST] {Path} completed successfully", path);

        return TypedResults.Ok(response.Value);
    }
}