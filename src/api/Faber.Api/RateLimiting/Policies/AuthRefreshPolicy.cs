using System.Threading.RateLimiting;
using Faber.Api.RateLimiting.Options;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Faber.Api.RateLimiting.Policies;

/// <summary>
/// <c>auth-refresh</c>: generous but bounded per-IP sliding window for the refresh-token endpoint, so
/// refresh-spam and refresh-token grinding are capped without costing a normal session its ability to
/// renew. Partitioned by IP rather than user because <c>Refresh</c> is anonymous and never reads the
/// bearer token, so keying on it would let an attacker buy a second budget just by attaching one.
/// </summary>
public sealed class AuthRefreshPolicy(IOptions<RateLimitingOptions> options) : IRateLimiterPolicy<string>
{
    /// <summary>Rejections are handled centrally by <see cref="RateLimitRejectionHandler"/>.</summary>
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected => null;

    /// <summary>Partitions by client IP using the configured <c>auth-refresh</c> limits.</summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <returns>The sliding-window partition for the request's client IP.</returns>
    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        return RateLimitPartitions.SlidingWindow(RateLimitPartitions.ByIp(httpContext), options.Value.AuthRefresh);
    }
}
