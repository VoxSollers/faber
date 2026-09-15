using ErrorOr;
using Faber.Modules.Users.PublicApi;
using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Auth.Application.Features.Me;

public class MeCommandHandler(
    IUserModuleApi userModuleApi,
    ILogger<MeCommandHandler> logger)
    : ICommandHandler<MeCommand, ErrorOr<MeResponse>>
{
    private const string HandlerName = nameof(MeCommandHandler);

    public async Task<ErrorOr<MeResponse>> ExecuteAsync(MeCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {UserId}", HandlerName, command.UserId);
        logger.LogInformation("[STEP] Fetching user by Id: {UserId}", command.UserId);

        var user = await userModuleApi.GetUserByIdAsync(command.UserId, ct);

        if (user is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | User not found: {UserId}", HandlerName, command.UserId);

            return Error.NotFound("User.NotFound", "User not found");
        }

        logger.LogInformation("[SUCCESS] {HandlerName} | User found: {UserId}", HandlerName, command.UserId);

        return user.MapToResponse();
    }
}