using ErrorOr;
using Faber.Modules.Auth.PublicApi.Events;
using Faber.Modules.Identity.Domain.Enums;
using Faber.Modules.Identity.PublicApi;
using Faber.Modules.Identity.PublicApi.Shared;
using Faber.Modules.Users.PublicApi;
using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Auth.Application.Features.ForgotPassword;

public class ForgotPasswordCommandHandler(
    IUserModuleApi userModuleApi,
    IIdentityModuleApi identityModuleApi,
    ILogger<ForgotPasswordCommandHandler> logger)
    : ICommandHandler<ForgotPasswordCommand, ErrorOr<Success>>
{
    private const string HandlerName = nameof(ForgotPasswordCommandHandler);
    private const int TokenExpiryMinutes = 10;

    public async Task<ErrorOr<Success>> ExecuteAsync(ForgotPasswordCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName}", HandlerName);
        logger.LogInformation("[STEP] Verifying existence of the user for the supplied address");
        var userResponse = await userModuleApi.GetUserByEmailAsync(command.Email, ct);

        if (userResponse is null)
        {
            logger.LogWarning("[SKIP] {HandlerName} | No account for the supplied address", HandlerName);

            return Result.Success;
        }

        logger.LogInformation("[STEP] User found, generating reset token");
        var token = identityModuleApi.GenerateToken();

        var response = await identityModuleApi.TryStoreActionTokenAsync(
            command.Email,
            token,
            TokenExpiryMinutes,
            ActionTokenType.ForgotPassword,
            ct);

        if (string.IsNullOrEmpty(response.Selector))
        {
            logger.LogError("[FAIL] {HandlerName} | Error while storing action token", HandlerName);

            return Error.Failure("Auth.ForgotPassword", "Error while storing action token");
        }

        var combinedKey = new CombinedKey($"{response.Selector}{token}");
        var eventModel = new ForgotPasswordAcceptedEvent(command.Email, combinedKey);
        await eventModel.PublishAsync(cancellation: ct);

        logger.LogInformation("[SUCCESS] {HandlerName} | Reset email published", HandlerName);

        return Result.Success;
    }
}