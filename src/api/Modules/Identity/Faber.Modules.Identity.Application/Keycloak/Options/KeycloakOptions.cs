namespace Faber.Modules.Identity.Application.Keycloak.Options;

public class KeycloakOptions
{
    public string BaseUrl { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string Realm { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;
}