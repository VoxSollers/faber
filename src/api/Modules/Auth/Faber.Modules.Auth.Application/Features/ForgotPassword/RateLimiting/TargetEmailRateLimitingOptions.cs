namespace Faber.Modules.Auth.Application.Features.ForgotPassword.RateLimiting;

/// <summary>
/// Options for the per-target-email token bucket that throttles <c>ForgotPassword</c> independent of the
/// per-IP <c>auth-password-reset</c> policy, to stop email-bombing a victim's inbox regardless of how
/// many different client IPs an attacker rotates through. Bound from <c>RateLimiting:ForgotPassword</c>.
/// </summary>
public class TargetEmailRateLimitingOptions
{
    /// <summary>
    /// Master switch. Defaults to the shared <c>RateLimiting:Enabled</c> flag (see
    /// <see cref="TargetEmailRateLimitingOptionsSetup"/>) so the existing test-environment relaxation
    /// from issue #330 covers this limiter too; can be overridden independently via
    /// <c>RateLimiting:ForgotPassword:Enabled</c>.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Maximum number of tokens (reset requests) a single target email can accumulate.</summary>
    public int TokenLimit { get; set; } = 3;

    /// <summary>Number of tokens replenished every <see cref="ReplenishmentPeriodSeconds"/>.</summary>
    public int TokensPerPeriod { get; set; } = 3;

    /// <summary>Replenishment period in seconds. Also used as the <c>Retry-After</c> hint on rejection.</summary>
    public int ReplenishmentPeriodSeconds { get; set; } = 900;
}
