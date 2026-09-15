using System.Threading.RateLimiting;
using Faber.Api.RateLimiting.Options;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Faber.Api.RateLimiting.Policies;

/// <summary>
/// <c>expensive-resource</c>: per-user concurrency limiter for expensive operations (e.g. PDF generation).
/// </summary>
public sealed class ExpensiveResourcePolicy(IOptions<RateLimitingOptions> options) : IRateLimiterPolicy<string>
{
    /// <summary>Rejections are handled centrally by <see cref="RateLimitRejectionHandler"/>.</summary>
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected => null;

    /// <summary>Partitions by authenticated user, falling back to client IP, using the configured <c>expensive-resource</c> limits.</summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <returns>The concurrency partition for the request's user or client IP.</returns>
    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        return RateLimitPartitions.Concurrency(RateLimitPartitions.ByUserOrIp(httpContext), options.Value.ExpensiveResource);
    }
}
