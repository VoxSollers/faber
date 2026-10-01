using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Faber.Api.Health;

public sealed class VaultHealthCheck(IHttpClientFactory httpClientFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var address = Environment.GetEnvironmentVariable("VAULT_ADDR");
        var token = Environment.GetEnvironmentVariable("VAULT_TOKEN");

        if (!Uri.TryCreate(address, UriKind.Absolute, out var uri) || string.IsNullOrWhiteSpace(token))
        {
            return HealthCheckResult.Unhealthy();
        }

        try
        {
            var client = httpClientFactory.CreateClient("health-vault");
            using var healthResponse = await client.GetAsync(
                new Uri(uri, "/v1/sys/health?standbyok=true"),
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!healthResponse.IsSuccessStatusCode)
            {
                return HealthCheckResult.Unhealthy();
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(uri, "/v1/auth/token/lookup-self"));
            request.Headers.TryAddWithoutValidation("X-Vault-Token", token);
            using var accessResponse = await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            return accessResponse.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy();
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy();
        }
    }
}
