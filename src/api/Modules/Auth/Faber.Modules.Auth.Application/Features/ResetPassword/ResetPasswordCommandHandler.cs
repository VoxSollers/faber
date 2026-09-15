using ErrorOr;
using Faber.Modules.Identity.Domain.Enums;
using Faber.Modules.Identity.PublicApi;
using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Auth.Application.Features.ResetPassword;

public class ResetPasswordCommandHandler(
    IIdentityModuleApi identityModuleApi,
    ILogger<ResetPasswordCommandHandler> logger)
    : ICommandHandler<ResetPasswordCommand, ErrorOr<Success>>
{
    private const string HandlerName = nameof(ResetPasswordCommandHandler);

    public async Task<ErrorOr<Success>> ExecuteAsync(ResetPasswordCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName}", HandlerName);
        logger.LogInformation("[STEP] Verifying action token");

        var response = await identityModuleApi.VerifyActionTokenAsync(
            command.VerificationKey.Selector,
            command.VerificationKey.Token,
            ActionTokenType.ForgotPassword,
            ct);

        if (!response.IsValid)
        {
            logger.LogWarning(
                "[FAIL] {HandlerName} | Token verification failed: {Message}",
                HandlerName,
                response.Message);

            return Error.Validation("Users.Validation", response.Message ?? "Token verification failed");
        }

        if (string.IsNullOrEmpty(response.Email))
        {
            logger.LogWarning("[FAIL] {HandlerName} | Email not found for token", HandlerName);

            return Error.Failure("Users.ResetPassword", "Email not found!");
        }

        logger.LogInformation("[STEP] Resetting password for the address carried by the verified token");
        var resetResult = await identityModuleApi.ResetPasswordByEmailAsync(response.Email, command.NewPassword, ct);

        if (!resetResult.IsSuccess)
        {
            logger.LogWarning(
                "[FAIL] {HandlerName} | Password reset failed: {Message}",
                HandlerName,
                resetResult.Message);

            return Error.Failure("Auth.ResetPassword", resetResult.Message ?? "Error while resetting password");
        }

        logger.LogInformation("[STEP] Consuming action token");
        await identityModuleApi.ConsumeActionTokenAsync(command.VerificationKey.Selector, ct);

        logger.LogInformation("[SUCCESS] {HandlerName} | Password reset successfully", HandlerName);

        return Result.Success;
    }
}