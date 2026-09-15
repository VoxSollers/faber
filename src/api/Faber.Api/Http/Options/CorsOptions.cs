namespace Faber.Api.Http.Options;

/// <summary>
/// Binds the <c>Cors</c> configuration section. Kept out of compiled code so the allowed origin
/// list — including any production origin — never has to be committed to source control; a
/// deployment supplies it via <c>appsettings.json</c> or an environment variable override (e.g.
/// <c>Cors__AllowedOrigins__0</c>).
/// </summary>
public class CorsOptions
{
    public string[] AllowedOrigins { get; set; } = [];
}
