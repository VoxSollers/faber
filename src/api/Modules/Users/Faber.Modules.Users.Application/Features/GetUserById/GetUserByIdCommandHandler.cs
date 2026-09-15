using ErrorOr;
using Faber.Modules.Users.Application.Features.Shared.Mappers;
using Faber.Modules.Users.Application.Features.Shared.Responses;
using Faber.Modules.Users.PublicApi;
using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Users.Application.Features.GetUserById;

public class GetUserByIdCommandHandler(
    IUserModuleApi userModuleApi,
    ILogger<GetUserByIdCommandHandler> logger)
    : ICommandHandler<GetUserByIdCommand, ErrorOr<GetUserResponse>>
{
    private const string HandlerName = nameof(GetUserByIdCommandHandler);

    public async Task<ErrorOr<GetUserResponse>> ExecuteAsync(GetUserByIdCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {UserId}", HandlerName, command.UserId);
        logger.LogInformation("[STEP] Fetching user by id: {UserId}", command.UserId);

        var userResponse = await userModuleApi.GetUserByIdAsync(command.UserId, ct);

        if (userResponse is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | User not found: {UserId}", HandlerName, command.UserId);

            return Error.NotFound("User.NotFound", $"User by id {command.UserId} not found");
        }

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | User {Username} retrieved successfully for {UserId}",
            HandlerName,
            userResponse.Username,
            command.UserId);

        return userResponse.MapToResponse();
    }
}