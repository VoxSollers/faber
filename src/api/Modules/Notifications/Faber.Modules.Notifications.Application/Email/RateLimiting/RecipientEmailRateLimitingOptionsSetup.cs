using Faber.Modules.Common.PublicApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Faber.Modules.Notifications.Application.Email.RateLimiting;

/// <summary>
/// Binds <see cref="RecipientEmailRateLimitingOptions"/> from <c>RateLimiting:OutboundEmail</c>.
/// <see cref="RecipientEmailRateLimitingOptions.Enabled"/> defaults from the shared root
/// <c>RateLimiting:Enabled</c> switch, so the limiter is off wherever the rate limiting middleware
/// already is (Testcontainers-based module suites), and can still be overridden independently.
/// </summary>
public class RecipientEmailRateLimitingOptionsSetup(IConfiguration configuration)
    : IConfigureOptions<RecipientEmailRateLimitingOptions>
{
    private const string FeatureSection = "OutboundEmail";

    public void Configure(RecipientEmailRateLimitingOptions options)
    {
        var rateLimitingSection = configuration.GetSection(ConfigurationConstants.Sections.RateLimiting);

        options.Enabled = rateLimitingSection.GetValue("Enabled", true);
        rateLimitingSection.GetSection(FeatureSection).Bind(options);
    }
}
