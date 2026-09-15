using System.Threading.RateLimiting;
using Faber.Api.RateLimiting.Options;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Faber.Api.RateLimiting.Policies;

/// <summary>
/// <c>auth-strict</c>: tight per-IP sliding window for anonymous credential endpoints, so an attacker
/// cannot spray password guesses from one host.
/// </summary>
public sealed class AuthStrictPolicy(IOptions<RateLimitingOptions> options) : IRateLimiterPolicy<string>
{
    /// <summary>Rejections are handled centrally by <see cref="RateLimitRejectionHandler"/>.</summary>
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected => null;

    /// <summary>Partitions by client IP using the configured <c>auth-strict</c> limits.</summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <returns>The sliding-window partition for the request's client IP.</returns>
    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        return RateLimitPartitions.SlidingWindow(RateLimitPartitions.ByIp(httpContext), options.Value.AuthStrict);
    }
}
