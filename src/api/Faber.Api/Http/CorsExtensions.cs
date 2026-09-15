using System.Net;
using System.Net.Sockets;
using Faber.Api.Http.Options;
using Microsoft.Extensions.Options;

namespace Faber.Api.Http;

/// <summary>
/// CORS policy for the Angular client(s). When <see cref="LanModeEnvVar"/> is set (the "lan"
/// launch profile, selected via the AppHost's own "lan" profile), any origin on a private-LAN
/// IP (RFC 1918) on the Angular LAN dev-server port is also allowed, so the app can be opened
/// from a phone/tablet on the same WiFi network without hand-editing an IP into this file —
/// the dev machine's LAN IP is DHCP-assigned and changes over time (this policy previously
/// hardcoded 192.168.1.28, which went stale when the machine was reassigned 192.168.1.228).
/// Outside LAN mode (including Production), the origin list is fixed and explicit.
/// </summary>
public static class CorsExtensions
{
    private const string PolicyName = "Faber.NgApp";

    /// <summary>
    /// Name of the environment variable that switches on LAN mode. Referenced from C# here and
    /// from <c>Faber.AppHost/Program.cs</c> (same solution, so it can share this constant instead
    /// of repeating the literal) — but it must also be typed out verbatim as a JSON key in both
    /// launchSettings.json "lan" profiles (Faber.Api's and Faber.AppHost's), since a launch
    /// profile is static JSON with no way to reference a compiled symbol. Keep those two in sync
    /// by hand if this ever changes.
    /// </summary>
    public const string LanModeEnvVar = "LAN_MODE";

    // Ports the Angular dev server may be reached on from a LAN device.
    // Keep in sync with the "lan" serve configuration in faber-app/angular.json.
    private static readonly int[] LanDevPorts = [4200];

    /// <summary>Registers the <see cref="PolicyName"/> CORS policy used by the Angular app.</summary>
    /// <param name="services">The service collection to add the configuration to.</param>
    /// <param name="configuration">Read for the <c>FABER_LAN_MODE</c> flag carried by the "lan" launch profile,
    /// and (via <see cref="CorsOptionsSetup"/>) for the <c>Cors</c> section. A real deployment overrides
    /// <c>Cors:AllowedOrigins</c> via an environment variable (e.g. <c>Cors__AllowedOrigins__0</c>) instead of
    /// committing its production origin to source control.</param>
    /// <returns>The same service collection so calls can be chained.</returns>
    public static IServiceCollection AddFaberCors(this IServiceCollection services, IConfiguration configuration)
    {
        var lanModeEnabled = configuration.GetValue<bool>(LanModeEnvVar);

        services.ConfigureOptions<CorsOptionsSetup>();
        var allowedOrigins = services.BuildServiceProvider().GetRequiredService<IOptions<CorsOptions>>().Value.AllowedOrigins;

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials();

                if (lanModeEnabled)
                {
                    policy.SetIsOriginAllowed(origin => allowedOrigins.Contains(origin) || IsLanDevOrigin(origin));
                }
                else
                {
                    policy.WithOrigins(allowedOrigins);
                }
            });
        });

        return services;
    }

    private static bool IsLanDevOrigin(string origin)
    {
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)) return false;
        if (uri.Scheme != Uri.UriSchemeHttp) return false;
        if (Array.IndexOf(LanDevPorts, uri.Port) < 0) return false;
        if (!IPAddress.TryParse(uri.Host, out var ip) || ip.AddressFamily != AddressFamily.InterNetwork) return false;

        var b = ip.GetAddressBytes();
        return b[0] == 10                               // 10.0.0.0/8
            || (b[0] == 172 && b[1] is >= 16 and <= 31)  // 172.16.0.0/12
            || (b[0] == 192 && b[1] == 168);              // 192.168.0.0/16
    }
}
