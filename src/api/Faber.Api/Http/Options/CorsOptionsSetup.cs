using Faber.Modules.Common.PublicApi;
using Microsoft.Extensions.Options;

namespace Faber.Api.Http.Options;

/// <summary>Binds <see cref="CorsOptions"/> from the <c>Cors</c> configuration section.</summary>
public class CorsOptionsSetup(IConfiguration configuration) : IConfigureOptions<CorsOptions>
{
    public void Configure(CorsOptions options)
    {
        configuration.GetSection(ConfigurationConstants.Sections.Cors).Bind(options);
    }
}
