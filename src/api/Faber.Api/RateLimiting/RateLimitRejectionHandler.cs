using System.Threading.RateLimiting;
using Faber.Api.RateLimiting.Options;
using Faber.Modules.Common.PublicApi.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Faber.Api.RateLimiting;

/// <summary>
/// Handles every rejected request: derives <c>Retry-After</c> from the failing lease (falling back to
/// <see cref="RateLimitingOptions.RetryAfterFallbackSeconds"/>), records the rejection metric, logs a
/// structured warning, and writes the shared <see cref="RateLimitRejection"/> response.
/// </summary>
public static class RateLimitRejectionHandler
{
    /// <summary>Metric/log tag used when only the global baseline limiter applies.</summary>
    private const string GlobalPolicyName = "global";

    private const string LogCategory = "Faber.Api.RateLimiting";

    /// <summary>Wired as <c>RateLimiterOptions.OnRejected</c>; <paramref name="ct"/> is required by that delegate.</summary>
    /// <param name="context">The rejection context supplied by the ASP.NET Core rate limiter.</param>
    /// <param name="ct">The cancellation token supplied by the framework delegate signature; unused.</param>
    /// <returns>A task that completes once the rejection response has been written.</returns>
    public static async ValueTask HandleAsync(OnRejectedContext context, CancellationToken ct)
    {
        var httpContext = context.HttpContext;
        var options = httpContext.RequestServices.GetRequiredService<IOptions<RateLimitingOptions>>().Value;

        var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var metadata)
            ? metadata
            : TimeSpan.FromSeconds(options.RetryAfterFallbackSeconds);

        var retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));

        var policy = httpContext.GetEndpoint()?.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName
                     ?? GlobalPolicyName;

        RateLimitingMetrics.RecordRejection(policy);

        var logger = httpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(LogCategory);

        logger.LogWarning(
            "Rate limit exceeded: policy {Policy}, path {Path}, retry after {RetryAfterSeconds}s",
            policy,
            httpContext.Request.Path,
            retryAfterSeconds);

        await RateLimitRejection.Problem(httpContext, retryAfterSeconds).ExecuteAsync(httpContext);
    }
}
