using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Faber.Modules.Vault.Application;
using Faber.Modules.Vault.PublicApi;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;
using VaultSharp;
using VaultSharp.Core;
using VaultSharp.V1.AuthMethods.Token;

namespace Faber.Modules.Vault.Application.Tests;

public class VaultSealedRetryTests : IAsyncLifetime
{
    private const string SeededClientId = "test-client-id";

    private static readonly string VaultConfigJson = """
        {
          "listener": [{"tcp": {"address": "0.0.0.0:8200", "tls_disable": true}}],
          "storage": {"inmem": {}},
          "disable_mlock": true
        }
        """;

    private readonly HttpClient _http = new();
    private IContainer _vault = null!;
    private string _vaultUrl = string.Empty;
    private string _rootToken = string.Empty;
    private string _unsealKey = string.Empty;

    public async ValueTask InitializeAsync()
    {
        _vault = new ContainerBuilder()
            .WithImage("hashicorp/vault:1.17.3")
            .WithEnvironment("VAULT_LOCAL_CONFIG", VaultConfigJson)
            .WithEnvironment("SKIP_SETCAP", "true")
            .WithCommand("server")
            .WithPortBinding(8200, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(s =>
                s.ForPort(8200)
                    .ForPath("/v1/sys/health")
                    .ForStatusCodeMatching(code =>
                        code == HttpStatusCode.OK
                        || code == HttpStatusCode.ServiceUnavailable
                        || (int)code == 501)))
            .Build();

        await _vault.StartAsync();

        _vaultUrl = $"http://{_vault.Hostname}:{_vault.GetMappedPublicPort(8200)}";

        await InitializeVaultLeaveSealedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        _http.Dispose();
        await _vault.DisposeAsync();
    }

    [Fact]
    public async Task ColdStart_VaultSealedThenUnsealed_ShouldRetrySucceed()
    {
        // A cold start is a Vault that already holds its secrets but restarted sealed. Seed first
        // and seal again, so unsealing exposes the finished data at once; seeding after the unseal
        // leaves a window where a retry sees the mount missing, which is not a transient failure.
        await UnsealAsync();
        await SeedKeycloakClientIdAsync();
        await SealAsync();
        var api = CreateApi(maxRetries: 6, baseDelaySeconds: 1.0, maxDelaySeconds: 4.0);

        var readTask = api.GetSecretValueAsync(
            VaultConstants.Paths.Keycloak,
            VaultConstants.DefaultMountPoint,
            VaultConstants.Keys.Keycloak.ClientId);

        await Task.Delay(TimeSpan.FromSeconds(2.5), TestContext.Current.CancellationToken);

        await UnsealAsync();

        var value = await readTask;

        value.ShouldBe(SeededClientId);
    }

    [Fact]
    public async Task VaultStaysSealed_ShouldExhaustRetriesAndThrow()
    {
        var api = CreateApi(maxRetries: 2, baseDelaySeconds: 0.3, maxDelaySeconds: 0.3);

        var ex = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await api.GetSecretValueAsync(
                VaultConstants.Paths.Keycloak,
                VaultConstants.DefaultMountPoint,
                VaultConstants.Keys.Keycloak.ClientId));

        ex.Message.ShouldBe(
            "Vault secret at KV mount 'secrets', path 'keycloak', key 'client-id' could not be read because Vault is sealed.");
    }

    [Fact]
    public async Task TransientNetworkFailure_ShouldRetrySucceed()
    {
        await UnsealAsync();
        await SeedKeycloakClientIdAsync();
        var api = CreateApi(
            maxRetries: 3,
            baseDelaySeconds: 0,
            maxDelaySeconds: 0,
            wrapHandler: inner => new FailFirstRequestsHandler(failures: 2) { InnerHandler = inner });

        var value = await api.GetSecretValueAsync(
            VaultConstants.Paths.Keycloak,
            VaultConstants.DefaultMountPoint,
            VaultConstants.Keys.Keycloak.ClientId);

        value.ShouldBe(SeededClientId);
    }

    [Fact]
    public async Task RejectedToken_ShouldReportLocationAndKeepVaultCause()
    {
        await UnsealAsync();
        await SeedKeycloakClientIdAsync();
        var api = CreateApi(maxRetries: 1, baseDelaySeconds: 0, maxDelaySeconds: 0, token: "stale-token");

        var ex = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await api.GetSecretValueAsync(
                VaultConstants.Paths.Keycloak,
                VaultConstants.DefaultMountPoint,
                VaultConstants.Keys.Keycloak.ClientId));

        ex.Message.ShouldBe(
            "Vault secret lookup failed for KV mount 'secrets', path 'keycloak', key 'client-id' (HTTP 403 Forbidden).");
        ex.InnerException.ShouldBeOfType<VaultApiException>()
            .HttpStatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task MissingMount_ShouldReportRequestedSecretLocation()
    {
        await UnsealAsync();
        var api = CreateApi(maxRetries: 1, baseDelaySeconds: 0, maxDelaySeconds: 0);

        var ex = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await api.GetSecretValueAsync(
                VaultConstants.Paths.Keycloak,
                "missing-secrets",
                VaultConstants.Keys.Keycloak.ClientId));

        ex.Message.ShouldBe(
            "Vault KV mount 'missing-secrets' was not found while reading path 'keycloak' and key 'client-id'.");
    }

    [Fact]
    public async Task MissingPath_ShouldReportRequestedSecretLocation()
    {
        await UnsealAsync();
        await EnableKvMountAsync();
        var api = CreateApi(maxRetries: 1, baseDelaySeconds: 0, maxDelaySeconds: 0);

        var ex = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await api.GetSecretValueAsync(
                VaultConstants.Paths.Keycloak,
                VaultConstants.DefaultMountPoint,
                VaultConstants.Keys.Keycloak.ClientId));

        ex.Message.ShouldBe(
            "Vault secret path 'keycloak' was not found in KV mount 'secrets' while reading key 'client-id'.");
    }

    [Fact]
    public async Task MissingKey_ShouldReportRequestedSecretLocation()
    {
        await UnsealAsync();
        await SeedKeycloakClientIdAsync();
        var api = CreateApi(maxRetries: 1, baseDelaySeconds: 0, maxDelaySeconds: 0);

        var ex = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await api.GetSecretValueAsync(
                VaultConstants.Paths.Keycloak,
                VaultConstants.DefaultMountPoint,
                VaultConstants.Keys.Keycloak.ClientSecret));

        ex.Message.ShouldBe(
            "Vault secret key 'client-secret' was not found in KV mount 'secrets' at path 'keycloak'.");
    }

    private IVaultModuleApi CreateApi(
        int maxRetries,
        double baseDelaySeconds,
        double maxDelaySeconds,
        string? token = null,
        Func<HttpMessageHandler, HttpMessageHandler>? wrapHandler = null)
    {
        var settings = new VaultClientSettings(_vaultUrl, new TokenAuthMethodInfo(token ?? _rootToken))
        {
            MyHttpClientProviderFunc = handler => new HttpClient(wrapHandler?.Invoke(handler) ?? handler)
        };
        var client = new VaultClient(settings);

        var options = Options.Create(new VaultModuleOptions
        {
            MaxRetryAttempts = maxRetries,
            BaseDelaySeconds = baseDelaySeconds,
            MaxDelaySeconds = maxDelaySeconds
        });

        return new VaultModuleApi(client, options, NullLogger<VaultModuleApi>.Instance);
    }

    private async Task InitializeVaultLeaveSealedAsync()
    {
        var response = await _http.PostAsJsonAsync(
            $"{_vaultUrl}/v1/sys/init",
            new { secret_shares = 1, secret_threshold = 1 });

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        _unsealKey = json.GetProperty("keys").EnumerateArray().First().GetString()!;
        _rootToken = json.GetProperty("root_token").GetString()!;
    }

    private async Task UnsealAsync()
    {
        var response = await _http.PostAsJsonAsync(
            $"{_vaultUrl}/v1/sys/unseal",
            new { key = _unsealKey });

        response.EnsureSuccessStatusCode();
    }

    private async Task SealAsync()
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"{_vaultUrl}/v1/sys/seal");
        request.Headers.Add("X-Vault-Token", _rootToken);

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task SeedKeycloakClientIdAsync()
    {
        await EnableKvMountAsync();

        await SendWithTokenAsync(
            HttpMethod.Post,
            $"/v1/secrets/data/{VaultConstants.Paths.Keycloak}",
            new
            {
                data = new Dictionary<string, string>
                {
                    [VaultConstants.Keys.Keycloak.ClientId] = SeededClientId
                }
            });
    }

    private async Task EnableKvMountAsync()
    {
        await SendWithTokenAsync(
            HttpMethod.Post,
            "/v1/sys/mounts/secrets",
            new { type = "kv", options = new { version = "2" } });
    }

    private async Task SendWithTokenAsync(HttpMethod method, string path, object body)
    {
        using var request = new HttpRequestMessage(method, $"{_vaultUrl}{path}")
        {
            Content = JsonContent.Create(body)
        };
        request.Headers.Add("X-Vault-Token", _rootToken);

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Simulates Vault being briefly unreachable: the first <c>failures</c> requests fail the way
    /// a refused connection does, and every later request goes through to the real container.
    /// </summary>
    private sealed class FailFirstRequestsHandler(int failures) : DelegatingHandler
    {
        private int _remaining = failures;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Interlocked.Decrement(ref _remaining) >= 0
                ? throw new HttpRequestException("Simulated connection failure")
                : base.SendAsync(request, cancellationToken);
    }
}
