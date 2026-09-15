using ErrorOr;
using Faber.Modules.Identity.PublicApi;
using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Auth.Application.Features.SignIn;

public class SignInCommandHandler(
    IIdentityModuleApi identityModuleApi,
    ILogger<SignInCommandHandler> logger)
    : ICommandHandler<SignInCommand, ErrorOr<SignInResponse>>
{
    private const string HandlerName = nameof(SignInCommandHandler);

    public async Task<ErrorOr<SignInResponse>> ExecuteAsync(SignInCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {Username}", HandlerName, command.Username);
        logger.LogInformation("[STEP] Signing in {Username}", command.Username);

        var token = await identityModuleApi.SignInAsync(command.Username, command.Password, ct);

        if (string.IsNullOrEmpty(token.AccessToken) || string.IsNullOrEmpty(token.RefreshToken))
        {
            logger.LogWarning("[FAIL] {HandlerName} | Sign in failed for {Username}", HandlerName, command.Username);

            return Error.Unauthorized("Auth.SignIn", "Sign in failed");
        }

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | User {Username} signed in successfully",
            HandlerName,
            command.Username);

        return token.MapToResponse();
    }
}