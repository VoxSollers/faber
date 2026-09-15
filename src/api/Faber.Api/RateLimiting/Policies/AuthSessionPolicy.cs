using System.Threading.RateLimiting;
using Faber.Api.RateLimiting.Options;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Faber.Api.RateLimiting.Policies;

/// <summary>
/// <c>auth-session</c>: bounded per-IP sliding window for the anonymous sign-out endpoint. Sign-out
/// consumes a refresh token and answers 400 for an invalid one against 204 for a valid one, so left
/// on the global anonymous baseline it is a refresh-token validity oracle running at three times the
/// budget of <see cref="AuthRefreshPolicy"/>, which consumes the very same credential. It gets its
/// own budget rather than sharing <c>auth-refresh</c> so that ordinary sign-outs from a shared
/// address can never exhaust the refresh budget a legitimate session needs to renew itself.
/// Partitioned by IP rather than user because sign-out is anonymous and never reads the bearer
/// token, so keying on it would let an attacker buy a second budget just by attaching one.
/// </summary>
public sealed class AuthSessionPolicy(IOptions<RateLimitingOptions> options) : IRateLimiterPolicy<string>
{
    /// <summary>Rejections are handled centrally by <see cref="RateLimitRejectionHandler"/>.</summary>
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected => null;

    /// <summary>Partitions by client IP using the configured <c>auth-session</c> limits.</summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <returns>The sliding-window partition for the request's client IP.</returns>
    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        return RateLimitPartitions.SlidingWindow(RateLimitPartitions.ByIp(httpContext), options.Value.AuthSession);
    }
}
