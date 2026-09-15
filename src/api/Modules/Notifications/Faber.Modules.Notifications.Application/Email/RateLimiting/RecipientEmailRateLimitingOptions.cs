namespace Faber.Modules.Notifications.Application.Email.RateLimiting;

/// <summary>
/// Options for the per-recipient token bucket that caps how much outbound mail a single address can
/// receive. This is the last line of defence, below every Auth endpoint limit: the per-IP policies
/// (<c>auth-password-reset</c>, <c>auth-strict</c>) and the per-target-email bucket on
/// <c>ForgotPassword</c> all guard the *inbound* request, so a rotated IP pool or a second flow
/// (sign-up verification) can still converge on one inbox. This bucket guards the *outbound* send.
/// Bound from <c>RateLimiting:OutboundEmail</c>.
/// </summary>
public class RecipientEmailRateLimitingOptions
{
    /// <summary>
    /// Master switch. Defaults to the shared <c>RateLimiting:Enabled</c> flag (see
    /// <see cref="RecipientEmailRateLimitingOptionsSetup"/>) so the test-environment relaxation from
    /// issue #330 covers this limiter too; can be overridden independently via
    /// <c>RateLimiting:OutboundEmail:Enabled</c>.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Maximum number of messages a single recipient address can accumulate a budget for.</summary>
    public int TokenLimit { get; set; } = 10;

    /// <summary>Number of tokens replenished every <see cref="ReplenishmentPeriodSeconds"/>.</summary>
    public int TokensPerPeriod { get; set; } = 10;

    /// <summary>Replenishment period in seconds. Also reported as the retry-after hint on rejection.</summary>
    public int ReplenishmentPeriodSeconds { get; set; } = 3600;
}
