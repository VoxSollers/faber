using Faber.Modules.Auth.Application.Groups;
using Faber.Modules.Identity.PublicApi;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Auth.Application.Features.Me;

public class MeEndpoint(ILogger<MeEndpoint> logger)
    : EndpointWithoutRequest<Results<Ok<MeResponse>, UnauthorizedHttpResult>>
{
    public override void Configure()
    {
        Get("me");
        Group<AuthGroup>();
        Version(1);
    }

    public override async Task<Results<Ok<MeResponse>, UnauthorizedHttpResult>> ExecuteAsync(CancellationToken ct)
    {
        var path = HttpContext.Request.Path.Value;
        logger.LogInformation("[HTTP GET] {Path} started", path);

        var userId = HttpContext.User.FindFirst(JwtClaimTypes.Aliases.UserId)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("[HTTP GET] {Path} failed: UserId claim not found", path);

            return TypedResults.Unauthorized();
        }

        var result = await new MeCommand(userId).ExecuteAsync(ct);

        if (result.IsError)
        {
            logger.LogWarning(
                "[HTTP GET] {Path} failed: {Error}",
                path,
                result.FirstError.Description);

            return TypedResults.Unauthorized();
        }

        logger.LogInformation("[HTTP GET] {Path} completed successfully", path);

        return TypedResults.Ok(result.Value);
    }
}