using Refit;

namespace Faber.Modules.Identity.Application.Keycloak.Contracts;

public class TokenRequest
{
    [AliasAs("grant_type")]
    public string GrantType { get; set; } = "password";

    [AliasAs("client_id")]
    public required string ClientId { get; set; }

    [AliasAs("scope")]
    public string Scope { get; set; } = "offline_access";

    [AliasAs("client_secret")]
    public required string ClientSecret { get; set; }

    [AliasAs("username")]
    public required string Username { get; set; }

    [AliasAs("password")]
    public required string Password { get; set; }
}