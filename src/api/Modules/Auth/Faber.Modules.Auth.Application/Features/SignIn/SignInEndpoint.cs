using Faber.Modules.Auth.Application.Groups;
using Faber.Modules.Common.PublicApi;
using Faber.Modules.Common.PublicApi.RateLimiting;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Auth.Application.Features.SignIn;

public class SignInEndpoint(ILogger<SignInEndpoint> logger)
    : Endpoint<SignInRequest, Results<Ok<SignInResponse>, UnauthorizedHttpResult>>
{
    public override void Configure()
    {
        Post("sign-in");
        Group<AuthGroup>();
        Version(1);
        AllowAnonymous();
        Options(x => x.RequireRateLimiting(RateLimitPolicies.AuthStrict));
    }

    public override async Task<Results<Ok<SignInResponse>, UnauthorizedHttpResult>> ExecuteAsync(
        SignInRequest request,
        CancellationToken ct)
    {
        var path = HttpContext.Request.Path.Value;
        logger.LogInformation("[HTTP POST] {Path} started for {Username}", path, request.Username);
        var response = await request.MapToCommand().ExecuteAsync(ct);

        if (response.IsError)
        {
            logger.LogWarning(
                "[HTTP POST] {Path} failed for {Username}: {Error}",
                path,
                request.Username,
                response.FirstError.Description);

            return TypedResults.Unauthorized();
        }

        logger.LogInformation(
            "[HTTP POST] {Path} completed successfully for {Username}",
            path,
            request.Username);

        HttpContext.SetCookiesRefreshToken(response.Value.RefreshToken);

        return TypedResults.Ok(response.Value);
    }
}