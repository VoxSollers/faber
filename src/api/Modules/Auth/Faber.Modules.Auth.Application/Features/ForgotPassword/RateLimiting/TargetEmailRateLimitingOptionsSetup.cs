using Faber.Modules.Common.PublicApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Faber.Modules.Auth.Application.Features.ForgotPassword.RateLimiting;

/// <summary>
/// Binds <see cref="TargetEmailRateLimitingOptions"/> from <c>RateLimiting:ForgotPassword</c>.
/// <see cref="TargetEmailRateLimitingOptions.Enabled"/> defaults from the shared root
/// <c>RateLimiting:Enabled</c> switch (so this limiter is disabled wherever the middleware already is,
/// e.g. Testcontainers-based module suites) and can still be overridden independently.
/// </summary>
public class TargetEmailRateLimitingOptionsSetup(IConfiguration configuration)
    : IConfigureOptions<TargetEmailRateLimitingOptions>
{
    private const string FeatureSection = "ForgotPassword";

    public void Configure(TargetEmailRateLimitingOptions options)
    {
        var rateLimitingSection = configuration.GetSection(ConfigurationConstants.Sections.RateLimiting);

        options.Enabled = rateLimitingSection.GetValue("Enabled", true);
        rateLimitingSection.GetSection(FeatureSection).Bind(options);
    }
}
