using Faber.Modules.Common.PublicApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Faber.Modules.Resumes.Application.Options;

public class ResumeLimitsOptionsSetup(IConfiguration configuration) : IConfigureOptions<ResumeLimitsOptions>
{
    public void Configure(ResumeLimitsOptions options)
    {
        configuration.GetSection(ConfigurationConstants.Sections.ResumeLimits).Bind(options);
    }
}