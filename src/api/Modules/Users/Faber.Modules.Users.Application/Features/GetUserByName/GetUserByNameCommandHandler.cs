using ErrorOr;
using Faber.Modules.Users.Application.Features.Shared.Mappers;
using Faber.Modules.Users.Application.Features.Shared.Responses;
using Faber.Modules.Users.PublicApi;
using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Users.Application.Features.GetUserByName;

public class GetUserByNameCommandHandler(
    IUserModuleApi userModuleApi,
    ILogger<GetUserByNameCommandHandler> logger)
    : ICommandHandler<GetUserByNameCommand, ErrorOr<GetUserResponse>>
{
    private const string HandlerName = nameof(GetUserByNameCommandHandler);

    public async Task<ErrorOr<GetUserResponse>> ExecuteAsync(
        GetUserByNameCommand command,
        CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {Username}", HandlerName, command.Username);
        logger.LogInformation("[STEP] Fetching user by name: {Username}", command.Username);

        var userResponse = await userModuleApi.GetUserByNameAsync(command.Username, ct);

        if (userResponse is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | User not found: {Username}", HandlerName, command.Username);

            return Error.NotFound("User.NotFound", $"User by {command.Username} not found");
        }

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | User {Username} retrieved successfully",
            HandlerName,
            userResponse.Username);

        return userResponse.MapToResponse();
    }
}