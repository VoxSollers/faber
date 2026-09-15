namespace Faber.Api.Tests.Features.RateLimiting;

public static class RateLimitingTestConstants
{
    public const string SignInUri = "api/v1/auth/sign-in";
    public const string SignUpUri = "api/v1/auth/sign-up";
    public const string VerifyEmailUri = "api/v1/auth/verify-email";
    public const string MeUri = "api/v1/auth/me";
    public const string SignOutUri = "api/v1/auth/sign-out";

    public const string ForwardedForHeader = "X-Forwarded-For";

    public const string TrustedProxyIp = "10.10.10.10";
    public const string UntrustedProxyIp = "10.99.99.99";

    // Must match the WebApp fixture's RateLimiting:GlobalAnonymous PermitLimit setting.
    public const int AnonymousPermitLimit = 3;

    // Must match the WebApp fixture's RateLimiting:GlobalAuthenticated PermitLimit setting.
    // Held well above every per-user named policy so a 429 from one of those is never masked by
    // the global baseline.
    public const int AuthenticatedPermitLimit = 30;

    // Must match the WebApp fixture's RateLimiting:AuthStrict:PermitLimit setting.
    // Deliberately below AnonymousPermitLimit so a 429 on request AuthStrictPermitLimit + 1
    // proves the auth-strict endpoint policy (not the global baseline) rejected it.
    public const int AuthStrictPermitLimit = 2;

    public const string ForgotPasswordUri = "api/v1/auth/forgot-password";
    public const string ResetPasswordUri = "api/v1/auth/reset-password";

    // Must match the WebApp fixture's RateLimiting:AuthPasswordReset:PermitLimit setting.
    public const int AuthPasswordResetPermitLimit = 2;

    public const string RefreshUri = "api/v1/auth/refresh";
    public const string ClientTypeHeader = "X-Client-Type";

    // Must match the WebApp fixture's RateLimiting:AuthRefresh:PermitLimit setting.
    // Deliberately below AnonymousPermitLimit so a 429 on request AuthRefreshPermitLimit + 1
    // proves the auth-refresh endpoint policy (not the global baseline) rejected it.
    public const int AuthRefreshPermitLimit = 2;

    // Must match the WebApp fixture's RateLimiting:AuthSession:PermitLimit setting.
    // Deliberately below AnonymousPermitLimit so a 429 on request AuthSessionPermitLimit + 1
    // proves the auth-session endpoint policy (not the global baseline) rejected it.
    public const int AuthSessionPermitLimit = 2;

    // Must match the WebApp fixture's RateLimiting:ForgotPassword:TokenLimit/TokensPerPeriod setting.
    public const int TargetEmailTokenLimit = 2;

    // Must match the WebApp fixture's RateLimiting:GlobalAnonymous/GlobalAuthenticated WindowSeconds/SegmentsPerWindow settings.
    public const int TestWindowSeconds = 60;
    public const int TestSegmentsPerWindow = 1;

    public const string UsersByEmailUriFormat = "api/v1/users/{0}/email";
    public const string UsersByNameUriFormat = "api/v1/users/{0}/username";
    public const string UsersByIdUriFormat = "api/v1/users/{0}";

    // Must match the WebApp fixture's RateLimiting:UserLookup:PermitLimit setting.
    // Deliberately far below AuthenticatedPermitLimit so a 429 on request UserLookupPermitLimit + 1
    // proves the user-lookup endpoint policy (not the global baseline) rejected it.
    public const int UserLookupPermitLimit = 2;

    // Must match the WebApp fixture's RateLimiting:AuthenticatedDefault:PermitLimit setting.
    // Deliberately far below AuthenticatedPermitLimit so a 429 on request
    // AuthenticatedDefaultPermitLimit + 1 proves the authenticated-default endpoint policy
    // (not the global baseline) rejected it.
    public const int AuthenticatedDefaultPermitLimit = 2;

    public const string VerifyActionTokenUri = "api/v1/identity/verify-action-token";

    public const string ResumesUri = "api/v1/resumes";
    public const string ResumeEducationsUriFormat = "api/v1/resumes/{0}/educations";
    public const string ResumeGenerateUriFormat = "api/v1/resumes/{0}/generate";
    public const string ResumeDownloadUriFormat = "api/v1/resumes/{0}/download";

    public const string ExpectedTitle = "Too Many Requests";
    public const string ExpectedDetail = "Rate limit exceeded. Please retry later.";
}
