using ErrorOr;
using Faber.Modules.Users.PublicApi;
using Faber.Modules.Users.PublicApi.Contracts;
using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Users.Application.Features.UpdateUserFullName;

public class UpdateUserFullNameCommandHandler(
    IUserModuleApi userModuleApi,
    ILogger<UpdateUserFullNameCommandHandler> logger)
    : ICommandHandler<UpdateUserFullNameCommand, ErrorOr<UpdateUserFullNameResponse>>
{
    private const string HandlerName = nameof(UpdateUserFullNameCommandHandler);

    public async Task<ErrorOr<UpdateUserFullNameResponse>> ExecuteAsync(
        UpdateUserFullNameCommand fullNameCommand,
        CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {UserId}", HandlerName, fullNameCommand.UserId);
        logger.LogInformation("[STEP] Updating user full name for {UserId}", fullNameCommand.UserId);

        var updateUserRequest = new UpdateUserRequest(
            fullNameCommand.UserId,
            fullNameCommand.FirstName,
            fullNameCommand.LastName);

        var userResponse = await userModuleApi.UpdateUserAsync(updateUserRequest, ct);

        if (userResponse is null || string.IsNullOrEmpty(userResponse.UserId))
        {
            logger.LogWarning(
                "[FAIL] {HandlerName} | Error while updating user: {UserId}",
                HandlerName,
                fullNameCommand.UserId);

            return Error.Failure("User.UpdateFailed", "Error while updating user");
        }

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | User {UserId} updated successfully",
            HandlerName,
            fullNameCommand.UserId);

        return new UpdateUserFullNameResponse(userResponse.FirstName, userResponse.LastName);
    }
}