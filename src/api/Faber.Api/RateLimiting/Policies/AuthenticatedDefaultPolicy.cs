using System.Threading.RateLimiting;
using Faber.Api.RateLimiting.Options;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Faber.Api.RateLimiting.Policies;

/// <summary>
/// <c>authenticated-default</c>: generous per-user baseline for authenticated CRUD endpoints.
/// </summary>
public sealed class AuthenticatedDefaultPolicy(IOptions<RateLimitingOptions> options) : IRateLimiterPolicy<string>
{
    /// <summary>Rejections are handled centrally by <see cref="RateLimitRejectionHandler"/>.</summary>
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected => null;

    /// <summary>Partitions by authenticated user, falling back to client IP, using the configured <c>authenticated-default</c> limits.</summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <returns>The sliding-window partition for the request's user or client IP.</returns>
    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        return RateLimitPartitions.SlidingWindow(RateLimitPartitions.ByUserOrIp(httpContext), options.Value.AuthenticatedDefault);
    }
}
