using ErrorOr;
using Faber.Modules.Identity.PublicApi;
using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Auth.Application.Features.Refresh;

public class RefreshCommandHandler(
    IIdentityModuleApi identityModuleApi,
    ILogger<RefreshCommandHandler> logger)
    : ICommandHandler<RefreshCommand, ErrorOr<RefreshResponse>>
{
    private const string HandlerName = nameof(RefreshCommandHandler);

    public async Task<ErrorOr<RefreshResponse>> ExecuteAsync(RefreshCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName}", HandlerName);

        if (string.IsNullOrEmpty(command.RefreshToken))
        {
            logger.LogWarning("[FAIL] {HandlerName} | Missing refresh token", HandlerName);

            return Error.Validation("Auth.Refresh", "Refresh token is required");
        }

        logger.LogInformation("[STEP] Refreshing token");

        var token = await identityModuleApi.RefreshTokenAsync(command.RefreshToken, ct);

        if (string.IsNullOrEmpty(token.AccessToken) || string.IsNullOrEmpty(token.RefreshToken))
        {
            logger.LogWarning("[FAIL] {HandlerName} | Token refresh failed", HandlerName);

            return Error.Unauthorized("Auth.Refresh", "Token refresh failed");
        }

        logger.LogInformation("[SUCCESS] {HandlerName} | Token refreshed successfully", HandlerName);

        return token.MapToResponse();
    }
}