using Microsoft.AspNetCore.Hosting;

namespace Faber.Modules.Resumes.Application.Tests;

/// <summary>
/// Points <c>ConnectionStrings:redis</c> at a closed port instead of the Testcontainers Redis
/// instance, so tests in this fixture prove the API keeps working (uncached) when Redis is
/// unreachable, per the graceful-degradation requirement. The Redis container from
/// <see cref="WebApp"/> is never built or started for this fixture — see <see cref="UsesRedis"/>.
/// </summary>
public class RedisUnavailableWebApp : WebApp
{
    /// <summary>A closed local port — connection attempts fail fast instead of resolving to a live server.</summary>
    private const string UnreachableRedisConnectionString = "127.0.0.1:1";

    protected override bool UsesRedis => false;

    protected override void ConfigureApp(IWebHostBuilder builder)
    {
        base.ConfigureApp(builder);

        builder.UseSetting("ConnectionStrings:redis", UnreachableRedisConnectionString);
    }
}
