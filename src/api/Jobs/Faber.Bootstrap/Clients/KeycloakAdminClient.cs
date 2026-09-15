using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Faber.Bootstrap;
using Microsoft.Extensions.Options;

namespace Faber.Bootstrap.Clients;

public sealed class KeycloakAdminClient(HttpClient httpClient, IOptions<BootstrapOptions> options)
{
    private readonly BootstrapOptions _options = options.Value;

    public async Task<string> GetClientSecretAsync(CancellationToken cancellationToken)
    {
        var accessToken = await GetAccessTokenAsync(cancellationToken);
        var internalClientId = await GetInternalClientIdAsync(accessToken, cancellationToken);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"admin/realms/{Uri.EscapeDataString(_options.KeycloakRealm)}/clients/{Uri.EscapeDataString(internalClientId)}/client-secret");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        ResponseValidation.EnsureSuccess(response, "regenerate the Keycloak client secret");
        var credential = await response.Content.ReadFromJsonAsync<ClientSecret>(cancellationToken: cancellationToken);

        return string.IsNullOrWhiteSpace(credential?.Value)
            ? throw new InvalidOperationException($"Keycloak returned no secret for client '{_options.KeycloakClientId}'.")
            : credential.Value;
    }

    private async Task<string> GetInternalClientIdAsync(string accessToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"admin/realms/{Uri.EscapeDataString(_options.KeycloakRealm)}/clients?clientId={Uri.EscapeDataString(_options.KeycloakClientId)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        ResponseValidation.EnsureSuccess(response, "find the Keycloak client");
        var clients = await response.Content.ReadFromJsonAsync<List<KeycloakClient>>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Keycloak returned an empty client lookup response.");
        var client = clients.SingleOrDefault(candidate => candidate.ClientId == _options.KeycloakClientId)
            ?? throw new InvalidOperationException(
                $"Keycloak realm '{_options.KeycloakRealm}' does not contain client '{_options.KeycloakClientId}'. Ensure the realm import completed.");

        return client.Id;
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "realms/master/protocol/openid-connect/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = "admin-cli",
                ["username"] = _options.KeycloakAdminUsername,
                ["password"] = _options.KeycloakAdminPassword
            })
        };
        using var response = await httpClient.SendAsync(request, cancellationToken);
        ResponseValidation.EnsureSuccess(response, "authenticate with Keycloak");
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);

        return string.IsNullOrWhiteSpace(token?.AccessToken)
            ? throw new InvalidOperationException("Keycloak authentication returned no access token.")
            : token.AccessToken;
    }

    private sealed record TokenResponse([property: JsonPropertyName("access_token")] string AccessToken);
    private sealed record KeycloakClient(string Id, string ClientId);
    private sealed record ClientSecret(string Value);
}
