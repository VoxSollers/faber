using System.Security.Claims;
using System.Threading.RateLimiting;
using Faber.Api.RateLimiting.Options;

namespace Faber.Api.RateLimiting;

/// <summary>
/// Partition keys and limiter factories shared by the global limiter and every named policy.
/// Keys are prefixed (<c>ip:</c> / <c>user:</c>) so an IP can never collide with a user id.
/// </summary>
public static class RateLimitPartitions
{
    private const string SubClaim = "sub";

    /// <summary>Partition key for the request's client IP (post-ForwardedHeaders).</summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>The <c>ip:</c>-prefixed partition key.</returns>
    public static string ByIp(HttpContext context)
    {
        return $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
    }

    /// <summary>Partition key for the authenticated user, or <see langword="null"/> when the request is anonymous.</summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>The <c>user:</c>-prefixed partition key, or <see langword="null"/> if the request carries no <c>sub</c> claim.</returns>
    public static string? ByUser(HttpContext context)
    {
        return context.User.FindFirstValue(SubClaim) is { Length: > 0 } sub ? $"user:{sub}" : null;
    }

    /// <summary>Partition key for the authenticated user, falling back to the client IP.</summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>The <c>user:</c>-prefixed partition key, or the <c>ip:</c>-prefixed key if the request is anonymous.</returns>
    public static string ByUserOrIp(HttpContext context)
    {
        return ByUser(context) ?? ByIp(context);
    }

    /// <summary>Sliding-window partition for <paramref name="key"/> using the given tuning.</summary>
    /// <param name="key">The partition key, typically from <see cref="ByIp"/> or <see cref="ByUserOrIp"/>.</param>
    /// <param name="policy">The sliding-window tuning to apply.</param>
    /// <returns>The configured sliding-window <see cref="RateLimitPartition{TKey}"/>.</returns>
    public static RateLimitPartition<string> SlidingWindow(string key, SlidingWindowPolicyOptions policy)
    {
        return RateLimitPartition.GetSlidingWindowLimiter(
            key,
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = policy.PermitLimit,
                Window = TimeSpan.FromSeconds(policy.WindowSeconds),
                SegmentsPerWindow = policy.SegmentsPerWindow,
                QueueLimit = 0,
                AutoReplenishment = true
            });
    }

    /// <summary>Concurrency partition for <paramref name="key"/> using the given tuning.</summary>
    /// <param name="key">The partition key, typically from <see cref="ByUserOrIp"/>.</param>
    /// <param name="policy">The concurrency tuning to apply.</param>
    /// <returns>The configured concurrency <see cref="RateLimitPartition{TKey}"/>.</returns>
    public static RateLimitPartition<string> Concurrency(string key, ConcurrencyPolicyOptions policy)
    {
        return RateLimitPartition.GetConcurrencyLimiter(
            key,
            _ => new ConcurrencyLimiterOptions
            {
                PermitLimit = policy.PermitLimit,
                QueueLimit = policy.QueueLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    }
}
