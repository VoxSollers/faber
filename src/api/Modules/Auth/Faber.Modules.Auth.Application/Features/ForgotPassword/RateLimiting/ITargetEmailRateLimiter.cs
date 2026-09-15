namespace Faber.Modules.Auth.Application.Features.ForgotPassword.RateLimiting;

/// <summary>
/// Per-target-email token bucket guarding this feature against inbox-bombing: throttles how many reset
/// emails a single address can trigger, independent of the per-IP <c>auth-password-reset</c> policy and
/// of whether the address belongs to a real account (the check never looks up existence). "Target" is
/// the address the request aims at, not the message we send.
/// </summary>
public interface ITargetEmailRateLimiter
{
    /// <summary>Attempts to consume one token for <paramref name="email"/>. False when the bucket is empty.</summary>
    bool TryAcquire(string email);

    /// <summary>Seconds a caller should wait before retrying after a rejection.</summary>
    int RetryAfterSeconds { get; }
}
