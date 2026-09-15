using ErrorOr;
using Faber.Modules.Identity.Domain.Enums;
using Faber.Modules.Identity.PublicApi;
using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Auth.Application.Features.VerifyEmail;

public class VerifyEmailCommandHandler(
    IIdentityModuleApi identityModuleApi,
    ILogger<VerifyEmailCommandHandler> logger)
    : ICommandHandler<VerifyEmailCommand, ErrorOr<VerifyEmailResponse>>
{
    private const string HandlerName = nameof(VerifyEmailCommandHandler);

    public async Task<ErrorOr<VerifyEmailResponse>> ExecuteAsync(
        VerifyEmailCommand command,
        CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName}", HandlerName);
        logger.LogInformation("[STEP] Verifying action token");

        var response = await identityModuleApi.VerifyActionTokenAsync(
            command.VerificationKey.Selector,
            command.VerificationKey.Token,
            ActionTokenType.VerifyEmail,
            ct);

        if (!response.IsValid)
        {
            logger.LogWarning(
                "[FAIL] {HandlerName} | Token verification failed: {Message}",
                HandlerName,
                response.Message);

            return Error.Failure("Users.VerifyEmail", response.Message);
        }

        if (string.IsNullOrEmpty(response.Email))
        {
            logger.LogWarning("[FAIL] {HandlerName} | Email not found for token", HandlerName);

            return Error.Failure("Users.VerifyEmail", "Email not found!");
        }

        logger.LogInformation("[STEP] Verifying user email in Keycloak: {Email}", response.Email);
        var verifyResult = await identityModuleApi.VerifyEmailAsync(response.Email, ct);

        if (!verifyResult.IsSuccess)
        {
            logger.LogWarning(
                "[FAIL] {HandlerName} | Email verification failed: {Message}",
                HandlerName,
                verifyResult.Message);

            return Error.Failure("Users.VerifyEmail", verifyResult.Message ?? "Email verification failed.");
        }

        logger.LogInformation("[STEP] Consuming action token for: {Email}", response.Email);
        await identityModuleApi.ConsumeActionTokenAsync(command.VerificationKey.Selector, ct);

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | Email verified successfully for: {Email}",
            HandlerName,
            response.Email);

        return new VerifyEmailResponse(response.IsValid);
    }
}