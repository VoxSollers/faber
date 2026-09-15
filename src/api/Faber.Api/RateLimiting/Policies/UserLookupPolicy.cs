using System.Threading.RateLimiting;
using Faber.Api.RateLimiting.Options;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Faber.Api.RateLimiting.Policies;

/// <summary>
/// <c>user-lookup</c>: tight per-user sliding window for the endpoints that resolve a user from a
/// caller-supplied identifier (email, username, id), so a signed-in account cannot harvest the
/// directory by probing which identifiers exist.
/// </summary>
public sealed class UserLookupPolicy(IOptions<RateLimitingOptions> options) : IRateLimiterPolicy<string>
{
    /// <summary>Rejections are handled centrally by <see cref="RateLimitRejectionHandler"/>.</summary>
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected => null;

    /// <summary>
    /// Partitions by authenticated user, falling back to client IP, using the configured
    /// <c>user-lookup</c> limits. The key carries no route component on purpose: every lookup
    /// endpoint draws on one budget, so rotating between them cannot multiply the harvest rate.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <returns>The sliding-window partition for the request's user or client IP.</returns>
    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        return RateLimitPartitions.SlidingWindow(RateLimitPartitions.ByUserOrIp(httpContext), options.Value.UserLookup);
    }
}
