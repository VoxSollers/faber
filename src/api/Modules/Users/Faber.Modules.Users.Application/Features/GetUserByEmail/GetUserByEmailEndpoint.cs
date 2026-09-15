using ErrorOr;
using Faber.Modules.Common.PublicApi.RateLimiting;
using Faber.Modules.Users.Application.Features.Shared.Responses;
using Faber.Modules.Users.Application.Groups;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Users.Application.Features.GetUserByEmail;

public class GetUserByEmailEndpoint(ILogger<GetUserByEmailEndpoint> logger)
    : Endpoint<GetUserByEmailRequest, Results<Ok<GetUserResponse>, NotFound<Error>>>
{
    public override void Configure()
    {
        Get("{Email}/email");
        Group<UsersGroup>();
        Version(1);
        Options(x => x.RequireRateLimiting(RateLimitPolicies.UserLookup));
    }

    public override async Task<Results<Ok<GetUserResponse>, NotFound<Error>>> ExecuteAsync(
        GetUserByEmailRequest request,
        CancellationToken ct)
    {
        var path = HttpContext.Request.Path.Value;
        logger.LogInformation("[HTTP GET] {Path} started for {Email}", path, request.Email);
        var result = await request.MapToCommand().ExecuteAsync(ct);

        if (result.IsError)
        {
            logger.LogWarning(
                "[HTTP GET] {Path} failed for {Email}: {Error}",
                path,
                request.Email,
                result.FirstError.Description);

            return TypedResults.NotFound(result.FirstError);
        }

        logger.LogInformation("[HTTP GET] {Path} completed successfully for {Email}", path, request.Email);

        return TypedResults.Ok(result.Value);
    }
}