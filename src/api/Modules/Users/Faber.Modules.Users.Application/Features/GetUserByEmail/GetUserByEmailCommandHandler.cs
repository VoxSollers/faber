using ErrorOr;
using Faber.Modules.Users.Application.Features.Shared.Mappers;
using Faber.Modules.Users.Application.Features.Shared.Responses;
using Faber.Modules.Users.PublicApi;
using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Users.Application.Features.GetUserByEmail;

public class GetUserByEmailCommandHandler(
    IUserModuleApi userModuleApi,
    ILogger<GetUserByEmailCommandHandler> logger)
    : ICommandHandler<GetUserByEmailCommand, ErrorOr<GetUserResponse>>
{
    private const string HandlerName = nameof(GetUserByEmailCommandHandler);

    public async Task<ErrorOr<GetUserResponse>> ExecuteAsync(
        GetUserByEmailCommand command,
        CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName}", HandlerName);
        logger.LogInformation("[STEP] Fetching the user for the supplied address");

        var userResponse = await userModuleApi.GetUserByEmailAsync(command.Email, ct);

        if (userResponse is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | User not found", HandlerName);

            return Error.NotFound("User.NotFound", "User not found");
        }

        logger.LogInformation("[SUCCESS] {HandlerName} | User found", HandlerName);

        return userResponse.MapToResponse();
    }
}