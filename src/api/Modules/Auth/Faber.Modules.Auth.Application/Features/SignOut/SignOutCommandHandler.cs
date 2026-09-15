using ErrorOr;
using Faber.Modules.Identity.PublicApi;
using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Auth.Application.Features.SignOut;

public class SignOutCommandHandler(
    IIdentityModuleApi identityModuleApi,
    ILogger<SignOutCommandHandler> logger)
    : ICommandHandler<SignOutCommand, ErrorOr<Success>>
{
    private const string HandlerName = nameof(SignOutCommandHandler);

    public async Task<ErrorOr<Success>> ExecuteAsync(SignOutCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName}", HandlerName);
        logger.LogInformation("[STEP] Signing out");

        await identityModuleApi.SignOutAsync(command.RefreshToken, ct);

        logger.LogInformation("[SUCCESS] {HandlerName} | Signed out successfully", HandlerName);

        return Result.Success;
    }
}