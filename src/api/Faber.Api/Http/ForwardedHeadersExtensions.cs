using System.Net;
using Faber.Modules.Common.PublicApi;
using Microsoft.AspNetCore.HttpOverrides;

namespace Faber.Api.Http;

/// <summary>
/// Trust configuration for <c>X-Forwarded-For</c>. A networking concern rather than a rate limiting one,
/// but the rate limiter's per-IP partitions are only spoof-resistant because of it: the header is honoured
/// solely when the direct peer is listed in <c>ForwardedHeaders:KnownProxies</c> (default: empty ⇒ ignored).
/// </summary>
public static class ForwardedHeadersExtensions
{
    /// <summary>Configures <see cref="ForwardedHeadersOptions"/> from the <c>ForwardedHeaders</c> section.</summary>
    /// <param name="services">The service collection to add the configuration to.</param>
    /// <param name="configuration">The application configuration to read the <c>ForwardedHeaders</c> section from.</param>
    /// <returns>The same service collection so calls can be chained.</returns>
    public static IServiceCollection AddFaberForwardedHeaders(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();

            var proxies = configuration
                .GetSection(ConfigurationConstants.Sections.ForwardedHeaders)
                .GetSection("KnownProxies")
                .Get<string[]>() ?? [];

            foreach (var proxy in proxies)
            {
                options.KnownProxies.Add(IPAddress.Parse(proxy));
            }
        });

        return services;
    }
}
