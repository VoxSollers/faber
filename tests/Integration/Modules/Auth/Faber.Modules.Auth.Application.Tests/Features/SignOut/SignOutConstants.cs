namespace Faber.Modules.Auth.Application.Tests.Features.SignOut;

public static class SignOutConstants
{
    public const string RefreshTokenCookie = "X-REFRESH-TOKEN";
    public const string ClientTypeHeader = "X-Client-Type";
    public const string RequestUri = "api/v1/auth/sign-out";
    public const string SetCookieHeader = "Set-Cookie";
    public const string InvalidClientType = "InvalidType";
    public const string SomeToken = "some-token";
    public const string InvalidRefreshToken = "invalid-refresh-token";
}