namespace Faber.Modules.Identity.PublicApi.Contracts;

public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    int RefreshExpiresIn);