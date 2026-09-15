using System.Security.Cryptography;
using Faber.Modules.Identity.Domain.Enums;
using Faber.Modules.Identity.PublicApi.Contracts;

namespace Faber.Modules.Identity.PublicApi;

public interface IIdentityModuleApi
{
    public Task<VerifyActionTokenResponse> VerifyActionTokenAsync(
        string selector,
        string token,
        ActionTokenType type,
        CancellationToken cancellationToken = default);

    public Task ConsumeActionTokenAsync(string selector, CancellationToken cancellationToken = default);

    public Task<StoreActionTokenResponse> TryStoreActionTokenAsync(
        string email,
        string token,
        int expiresInMinutes,
        ActionTokenType type,
        CancellationToken cancellationToken = default);

    public Task ResetPasswordAsync(string userId, string password, CancellationToken cancellationToken = default);

    public Task<ResetPasswordResponse> ResetPasswordByEmailAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    public Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken);

    public Task<bool> IsUsernameUniqueAsync(string username, CancellationToken cancellationToken);

    public Task<TokenResponse> SignInAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);

    public Task SignOutAsync(string refreshToken, CancellationToken cancellationToken = default);

    public Task<TokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    public Task<VerifyEmailResponse> VerifyEmailAsync(string email, CancellationToken cancellationToken = default);

    public Task<UserResponse?> CreateUserAsync(
        string email,
        string username,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default);

    public Task<UserResponse?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    public Task<UserResponse?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);

    public Task<UserResponse?> GetUserByNameAsync(string username, CancellationToken cancellationToken = default);

    public Task UpdateUserProfileAsync(
        string userId,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default);

    public string GenerateToken(int sizeBytes = 32)
    {
        var bytes = RandomNumberGenerator.GetBytes(sizeBytes);

        return Base64UrlEncode(bytes);
    }

    private static string Base64UrlEncode(ReadOnlySpan<byte> data)
    {
        var base64 = Convert.ToBase64String(data);

        return base64
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}