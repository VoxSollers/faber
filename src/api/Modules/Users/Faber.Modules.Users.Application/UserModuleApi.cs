using Faber.Modules.Identity.PublicApi;
using Faber.Modules.Users.PublicApi;
using Faber.Modules.Users.PublicApi.Contracts;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Users.Application;

public class UserModuleApi : IUserModuleApi
{
    private readonly IIdentityModuleApi _identityModuleApi;
    private readonly ILogger<UserModuleApi> _logger;

    public UserModuleApi(
        IIdentityModuleApi identityModuleApi,
        ILogger<UserModuleApi> logger)
    {
        _identityModuleApi = identityModuleApi;
        _logger = logger;
    }

    public async Task<UserResponse?> CreateUserAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _identityModuleApi.CreateUserAsync(
            request.Email,
            request.Username,
            request.FirstName,
            request.LastName,
            cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("User '{Username}' not created!", request.Username);

            return null;
        }

        _logger.LogInformation("User '{Username}' created successfully!", request.Username);

        return new UserResponse(user.Id, user.Username, user.Email, user.FirstName, user.LastName);
    }

    public async Task<UserResponse?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = await _identityModuleApi.GetUserByEmailAsync(email, cancellationToken);

        if (user is null)
        {
            _logger.LogInformation("No user matched the supplied address.");

            return null;
        }

        _logger.LogInformation("User '{Username}' retrieved successfully!", user.Username);

        return new UserResponse(user.Id, user.Username, user.Email, user.FirstName, user.LastName);
    }

    public async Task<UserResponse?> GetUserByIdAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await _identityModuleApi.GetUserByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            _logger.LogInformation("User by {Id} not found.", userId);

            return null;
        }

        return new UserResponse(user.Id, user.Username, user.Email, user.FirstName, user.LastName);
    }

    public async Task<UserResponse?> GetUserByNameAsync(string username, CancellationToken cancellationToken)
    {
        var user = await _identityModuleApi.GetUserByNameAsync(username, cancellationToken);

        if (user is null)
        {
            _logger.LogInformation("User by {Username} not found.", username);

            return null;
        }

        _logger.LogInformation("User '{Username}' retrieved successfully!", user.Username);

        return new UserResponse(user.Id, user.Username, user.Email, user.FirstName, user.LastName);
    }

    public async Task<UserResponse?> UpdateUserAsync(UpdateUserRequest request, CancellationToken cancellationToken)
    {
        await _identityModuleApi.UpdateUserProfileAsync(
            request.UserId,
            request.FirstName,
            request.LastName,
            cancellationToken);

        _logger.LogInformation("User '{UserId}' updated successfully!", request.UserId);

        return new UserResponse(request.UserId, "", "", request.FirstName, request.LastName);
    }
}