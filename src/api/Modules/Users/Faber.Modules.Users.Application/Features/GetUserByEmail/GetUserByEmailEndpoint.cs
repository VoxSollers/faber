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
    /// <summary>
    /// Logged instead of the request path: this route carries the address in the path itself, so
    /// logging the path would write a user e-mail address into the logs.
    /// </summary>
    private const string RouteName = "users/{Email}/email";

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
        logger.LogInformation("[HTTP GET] {Route} started", RouteName);
        var result = await request.MapToCommand().ExecuteAsync(ct);

        if (result.IsError)
        {
            logger.LogWarning(
                "[HTTP GET] {Route} failed: {Error}",
                RouteName,
                result.FirstError.Description);

            return TypedResults.NotFound(result.FirstError);
        }

        logger.LogInformation("[HTTP GET] {Route} completed successfully", RouteName);

        return TypedResults.Ok(result.Value);
    }
}