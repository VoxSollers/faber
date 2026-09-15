using Faber.Modules.Identity.PublicApi;
using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Identity.Application.Features.VerifyActionToken;

public class VerifyActionTokenCommandHandler(
    IIdentityModuleApi identityModuleApi,
    ILogger<VerifyActionTokenCommandHandler> logger)
    : CommandHandler<VerifyActionTokenCommand, VerifyActionTokenResponse>
{
    private const string HandlerName = nameof(VerifyActionTokenCommandHandler);

    public override async Task<VerifyActionTokenResponse> ExecuteAsync(
        VerifyActionTokenCommand command,
        CancellationToken ct = default)
    {
        logger.LogInformation("[START] {HandlerName} for {Selector}", HandlerName, command.VerificationKey.Selector);
        logger.LogInformation("[STEP] Verifying action token for {Type}", command.Type);

        var actionTokenResponse = await identityModuleApi.VerifyActionTokenAsync(
            command.VerificationKey.Selector,
            command.VerificationKey.Token,
            command.Type,
            ct);

        if (!actionTokenResponse.IsValid)
        {
            logger.LogWarning(
                "[FAIL] {HandlerName} | Token verification failed for {Selector}: {Message}",
                HandlerName,
                command.VerificationKey.Selector,
                actionTokenResponse.Message);

            return new VerifyActionTokenResponse(actionTokenResponse.IsValid);
        }

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | Token verified successfully for {Selector}",
            HandlerName,
            command.VerificationKey.Selector);

        return new VerifyActionTokenResponse(actionTokenResponse.IsValid);
    }
}