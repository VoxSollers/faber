using Refit;

namespace Faber.Modules.Identity.Application.Keycloak.Contracts;

public class AdminTokenRequest
{
    [AliasAs("grant_type")]
    public string GrantType { get; set; } = "client_credentials";

    [AliasAs("client_id")]
    public required string ClientId { get; set; }

    [AliasAs("client_secret")]
    public required string ClientSecret { get; set; }
}