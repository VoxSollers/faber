using ErrorOr;
using Faber.Modules.Common.PublicApi.RateLimiting;
using Faber.Modules.Users.Application.Groups;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Users.Application.Features.UpdateUserFullName;

public class UpdateUserFullNameEndpoint(ILogger<UpdateUserFullNameEndpoint> logger)
    : Endpoint<UpdateUserFullNameRequest, Results<Ok<UpdateUserFullNameResponse>, NotFound<Error>>>
{
    public override void Configure()
    {
        Put("{UserId}");
        Group<UsersGroup>();
        Version(1);
        Options(x => x.RequireRateLimiting(RateLimitPolicies.AuthenticatedDefault));
    }

    public override async Task<Results<Ok<UpdateUserFullNameResponse>, NotFound<Error>>> ExecuteAsync(
        UpdateUserFullNameRequest fullNameRequest,
        CancellationToken ct)
    {
        var path = HttpContext.Request.Path.Value;
        logger.LogInformation("[HTTP PUT] {Path} started for {UserId}", path, fullNameRequest.UserId);
        var result = await fullNameRequest.MapToCommand().ExecuteAsync(ct);

        if (result.IsError)
        {
            logger.LogWarning(
                "[HTTP PUT] {Path} failed for {UserId}: {Error}",
                path,
                fullNameRequest.UserId,
                result.FirstError.Description);

            return TypedResults.NotFound(result.FirstError);
        }

        logger.LogInformation("[HTTP PUT] {Path} completed successfully for {UserId}", path, fullNameRequest.UserId);

        return TypedResults.Ok(result.Value);
    }
}