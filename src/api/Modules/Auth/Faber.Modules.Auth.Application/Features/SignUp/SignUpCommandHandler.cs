using ErrorOr;
using Faber.Modules.Auth.PublicApi.Events;
using Faber.Modules.Identity.Domain.Enums;
using Faber.Modules.Identity.PublicApi;
using Faber.Modules.Identity.PublicApi.Shared;
using Faber.Modules.Users.PublicApi;
using Faber.Modules.Users.PublicApi.Contracts;
using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Auth.Application.Features.SignUp;

public class SignUpCommandHandler(
    IUserModuleApi userModuleApi,
    IIdentityModuleApi identityModuleApi,
    ILogger<SignUpCommandHandler> logger)
    : ICommandHandler<SignUpCommand, ErrorOr<SignUpResponse>>
{
    private const string HandlerName = nameof(SignUpCommandHandler);

    public async Task<ErrorOr<SignUpResponse>> ExecuteAsync(SignUpCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName}", HandlerName);
        logger.LogInformation("[STEP] Creating user in User module");

        var userRequest = new CreateUserRequest(
            command.Username,
            command.Email,
            command.FirstName,
            command.LastName);

        var userResult = await userModuleApi.CreateUserAsync(userRequest, ct);

        if (userResult is null)
        {
            logger.LogError("[FAIL] {HandlerName} | Error while creating user", HandlerName);

            return Error.Failure("Auth.SignUp", "Error while creating user");
        }

        logger.LogInformation(
            "[STEP] User created with ID: {UserId}, setting password in Identity module",
            userResult.UserId);

        await identityModuleApi.ResetPasswordAsync(userResult.UserId, command.Password, ct);

        logger.LogInformation("[STEP] Generating email verification token");
        var token = identityModuleApi.GenerateToken();

        var response = await identityModuleApi.TryStoreActionTokenAsync(
            command.Email,
            token,
            30,
            ActionTokenType.VerifyEmail,
            ct);

        if (string.IsNullOrEmpty(response.Selector))
        {
            logger.LogError("[FAIL] {HandlerName} | Error while storing verification token", HandlerName);

            return Error.Failure("Auth.SignUp", "Error while storing verification token");
        }

        logger.LogInformation("[STEP] Publishing NewUserSignedUpEvent");
        var combinedKey = new CombinedKey($"{response.Selector}{token}");
        var eventModel = new NewUserSignedUpEvent(command.Email, combinedKey);
        await eventModel.PublishAsync(cancellation: ct);

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | User {UserId} signed up successfully",
            HandlerName,
            userResult.UserId);

        return userResult.MapToResponse();
    }
}