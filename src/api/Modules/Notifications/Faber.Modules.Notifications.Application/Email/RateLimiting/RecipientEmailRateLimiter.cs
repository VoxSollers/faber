using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;

namespace Faber.Modules.Notifications.Application.Email.RateLimiting;

/// <inheritdoc cref="IRecipientEmailRateLimiter"/>
public sealed class RecipientEmailRateLimiter : IRecipientEmailRateLimiter, IDisposable
{
    private readonly bool _enabled;
    private readonly PartitionedRateLimiter<string> _limiter;

    public RecipientEmailRateLimiter(IOptions<RecipientEmailRateLimitingOptions> options)
    {
        var policy = options.Value;
        _enabled = policy.Enabled;
        RetryAfterSeconds = policy.ReplenishmentPeriodSeconds;

        _limiter = PartitionedRateLimiter.Create<string, string>(recipient =>
            RateLimitPartition.GetTokenBucketLimiter(
                NormalizeRecipient(recipient),
                _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = policy.TokenLimit,
                    TokensPerPeriod = policy.TokensPerPeriod,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(policy.ReplenishmentPeriodSeconds),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
    }

    public int RetryAfterSeconds { get; }

    public bool TryAcquire(string recipient)
    {
        if (!_enabled)
        {
            return true;
        }

        using var lease = _limiter.AttemptAcquire(recipient);

        return lease.IsAcquired;
    }

    public void Dispose() => _limiter.Dispose();

    /// <summary>
    /// Collapses casing and padding onto one partition key so <c>A@x.com</c>, <c>a@x.com</c> and
    /// <c> A@X.COM </c> share a single budget instead of minting three.
    /// </summary>
    private static string NormalizeRecipient(string recipient) => recipient.Trim().ToLowerInvariant();
}
