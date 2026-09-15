namespace Faber.Modules.Auth.Application.Tests.Features.SignIn;

public static class SignInConstants
{
    public const string RefreshTokenCookie = "X-REFRESH-TOKEN";
    public const string SetCookieHeader = "Set-Cookie";
    public const string InvalidGrantCode = "invalid_grant";
    public const string InvalidCredentialsDescription = "Invalid user credentials";
    public const string WrongPasswordSuffix = "_wrong";
    public const string HttpOnlyAttribute = "httponly";
    public const string PathAttribute = "path=/";
    public const string SecureAttribute = "secure";
    public const string SameSiteNoneAttribute = "samesite=none";
    public const int MinPasswordLength = 8;
    public const int JwtPartsCount = 3;
    public const int MinValidationErrorsForBothFields = 2;
}