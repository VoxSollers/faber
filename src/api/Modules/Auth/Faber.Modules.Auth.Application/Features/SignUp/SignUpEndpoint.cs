using ErrorOr;
using Faber.Modules.Auth.Application.Groups;
using Faber.Modules.Common.PublicApi.RateLimiting;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Auth.Application.Features.SignUp;

public class SignUpEndpoint(ILogger<SignUpEndpoint> logger)
    : Endpoint<SignUpRequest, Results<NoContent, BadRequest<Error>>>
{
    public override void Configure()
    {
        Post("sign-up");
        Group<AuthGroup>();
        Version(1);
        AllowAnonymous();
        Options(x => x.RequireRateLimiting(RateLimitPolicies.AuthStrict));
    }

    public override async Task<Results<NoContent, BadRequest<Error>>> ExecuteAsync(
        SignUpRequest request,
        CancellationToken ct)
    {
        var path = HttpContext.Request.Path.Value;
        logger.LogInformation("[HTTP POST] {Path} started", path);
        var result = await request.MapToCommand().ExecuteAsync(ct);

        if (result.IsError)
        {
            logger.LogWarning(
                "[HTTP POST] {Path} failed: {Error}",
                path,
                result.FirstError.Description);

            return TypedResults.BadRequest(result.FirstError);
        }

        logger.LogInformation("[HTTP POST] {Path} completed successfully", path);

        return TypedResults.NoContent();
    }
}