using Faber.Modules.Users.PublicApi.Contracts;

namespace Faber.Modules.Users.PublicApi;

public interface IUserModuleApi
{
    public Task<UserResponse?> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken);

    public Task<UserResponse?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);

    public Task<UserResponse?> GetUserByIdAsync(string userId, CancellationToken cancellationToken);

    public Task<UserResponse?> GetUserByNameAsync(string username, CancellationToken cancellationToken);

    public Task<UserResponse?> UpdateUserAsync(UpdateUserRequest request, CancellationToken cancellationToken);
}