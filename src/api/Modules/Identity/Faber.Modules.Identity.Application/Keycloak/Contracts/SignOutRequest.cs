using Refit;

namespace Faber.Modules.Identity.Application.Keycloak.Contracts;

public class SignOutRequest
{
    [AliasAs("client_id")]
    public required string ClientId { get; set; }

    [AliasAs("client_secret")]
    public required string ClientSecret { get; set; }

    [AliasAs("refresh_token")]
    public required string RefreshToken { get; set; }
}