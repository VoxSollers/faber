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
        logger.LogInformation("[START] {HandlerName} for {Email}", HandlerName, command.Email);
        logger.LogInformation("[STEP] Fetching user by email: {Email}", command.Email);

        var userResponse = await userModuleApi.GetUserByEmailAsync(command.Email, ct);

        if (userResponse is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | User not found: {Email}", HandlerName, command.Email);

            return Error.NotFound("User.NotFound", $"User by {command.Email} not found");
        }

        logger.LogInformation("[SUCCESS] {HandlerName} | User found: {Email}", HandlerName, command.Email);

        return userResponse.MapToResponse();
    }
}