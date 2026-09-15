using Faber.Modules.Common.PublicApi;
using Faber.Modules.Notifications.PublicApi.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Faber.Modules.Notifications.PublicApi.OptionsSetup;

public class FluentEmailOptionsSetup : IConfigureOptions<FluentEmailOptions>
{
    private readonly IConfiguration _configuration;

    public FluentEmailOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(FluentEmailOptions options)
    {
        _configuration.GetSection(ConfigurationConstants.Sections.FluentEmail).Bind(options);
    }
}