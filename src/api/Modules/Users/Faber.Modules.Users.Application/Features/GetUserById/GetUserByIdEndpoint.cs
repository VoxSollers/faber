using ErrorOr;
using Faber.Modules.Common.PublicApi.RateLimiting;
using Faber.Modules.Users.Application.Features.Shared.Responses;
using Faber.Modules.Users.Application.Groups;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Users.Application.Features.GetUserById;

public class GetUserByIdEndpoint(ILogger<GetUserByIdEndpoint> logger)
    : Endpoint<GetUserByIdRequest, Results<Ok<GetUserResponse>, NotFound<Error>>>
{
    public override void Configure()
    {
        Get("{UserId}");
        Group<UsersGroup>();
        Version(1);
        Options(x => x.RequireRateLimiting(RateLimitPolicies.UserLookup));
    }

    public override async Task<Results<Ok<GetUserResponse>, NotFound<Error>>> ExecuteAsync(
        GetUserByIdRequest request,
        CancellationToken ct)
    {
        var path = HttpContext.Request.Path.Value;
        logger.LogInformation("[HTTP GET] {Path} started for {UserId}", path, request.UserId);
        var result = await request.MapToCommand().ExecuteAsync(ct);

        if (result.IsError)
        {
            logger.LogWarning(
                "[HTTP GET] {Path} failed for {UserId}: {Error}",
                path,
                request.UserId,
                result.FirstError.Description);

            return TypedResults.NotFound(result.FirstError);
        }

        logger.LogInformation("[HTTP GET] {Path} completed successfully for {UserId}", path, request.UserId);

        return TypedResults.Ok(result.Value);
    }
}