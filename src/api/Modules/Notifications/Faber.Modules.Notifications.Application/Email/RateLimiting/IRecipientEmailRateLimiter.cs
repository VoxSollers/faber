namespace Faber.Modules.Notifications.Application.Email.RateLimiting;

/// <summary>
/// Per-recipient token bucket guarding the outbound email layer against inbox bombing and against the
/// service being used as a spam amplifier. "Recipient" is the address a message is actually addressed
/// to — distinct from Auth's <c>ITargetEmailRateLimiter</c>, which throttles the address an inbound
/// HTTP request aims at. The two live at different layers on purpose: this one still holds when an
/// attacker rotates IPs, mixes flows (reset plus verification), or reaches the sender by any future
/// path that never passed through an Auth endpoint.
/// </summary>
public interface IRecipientEmailRateLimiter
{
    /// <summary>Attempts to consume one token for <paramref name="recipient"/>. False when the bucket is empty.</summary>
    bool TryAcquire(string recipient);

    /// <summary>Seconds a caller should wait before the bucket refills.</summary>
    int RetryAfterSeconds { get; }
}
