using ErrorOr;
using Faber.Modules.Common.PublicApi.RateLimiting;
using Faber.Modules.Users.Application.Features.Shared.Responses;
using Faber.Modules.Users.Application.Groups;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Users.Application.Features.GetUserByName;

public class GetUserByNameEndpoint(ILogger<GetUserByNameEndpoint> logger)
    : Endpoint<GetUserByNameRequest, Results<Ok<GetUserResponse>, NotFound<Error>>>
{
    public override void Configure()
    {
        Get("{Username}/username");
        Group<UsersGroup>();
        Version(1);
        Options(x => x.RequireRateLimiting(RateLimitPolicies.UserLookup));
    }

    public override async Task<Results<Ok<GetUserResponse>, NotFound<Error>>> ExecuteAsync(
        GetUserByNameRequest request,
        CancellationToken ct)
    {
        var path = HttpContext.Request.Path.Value;
        logger.LogInformation("[HTTP GET] {Path} started for {Username}", path, request.Username);
        var result = await request.MapToCommand().ExecuteAsync(ct);

        if (result.IsError)
        {
            logger.LogWarning(
                "[HTTP GET] {Path} failed for {Username}: {Error}",
                path,
                request.Username,
                result.FirstError.Description);

            return TypedResults.NotFound(result.FirstError);
        }

        logger.LogInformation("[HTTP GET] {Path} completed successfully for {Username}", path, request.Username);

        return TypedResults.Ok(result.Value);
    }
}