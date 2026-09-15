using Faber.Modules.Identity.Application.Keycloak.Contracts;
using Faber.Modules.Identity.Application.Keycloak.Options;

namespace Faber.Modules.Identity.Application.Keycloak;

public class BearerAdminToken
{
    private BearerAdminToken()
    {
    }

    public required string Value { get; init; }

    public static async Task<BearerAdminToken> GenerateAsync(
        IKeycloakApi keycloakApi,
        KeycloakOptions options,
        CancellationToken cancellationToken)
    {
        var request = new AdminTokenRequest
        {
            ClientId = options.ClientId,
            ClientSecret = options.ClientSecret,
            GrantType = "client_credentials"
        };

        var response = await keycloakApi.GetAdminTokenAsync(options.Realm, request, cancellationToken);
        var bearerAdminToken = $"{response.TokenType} {response.AccessToken}";

        return new BearerAdminToken
        {
            Value = bearerAdminToken
        };
    }
}