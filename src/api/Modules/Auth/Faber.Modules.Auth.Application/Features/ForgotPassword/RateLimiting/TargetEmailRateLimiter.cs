using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;

namespace Faber.Modules.Auth.Application.Features.ForgotPassword.RateLimiting;

/// <inheritdoc cref="ITargetEmailRateLimiter"/>
public sealed class TargetEmailRateLimiter : ITargetEmailRateLimiter, IDisposable
{
    private readonly bool _enabled;
    private readonly PartitionedRateLimiter<string> _limiter;

    public TargetEmailRateLimiter(IOptions<TargetEmailRateLimitingOptions> options)
    {
        var policy = options.Value;
        _enabled = policy.Enabled;
        RetryAfterSeconds = policy.ReplenishmentPeriodSeconds;

        _limiter = PartitionedRateLimiter.Create<string, string>(email =>
            RateLimitPartition.GetTokenBucketLimiter(
                NormalizeEmail(email),
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

    public bool TryAcquire(string email)
    {
        if (!_enabled)
        {
            return true;
        }

        using var lease = _limiter.AttemptAcquire(email);

        return lease.IsAcquired;
    }

    public void Dispose() => _limiter.Dispose();

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
