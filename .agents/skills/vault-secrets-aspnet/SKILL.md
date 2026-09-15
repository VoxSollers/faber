---
name: vault-secrets-aspnet
description: HashiCorp Vault integration for ASP.NET Core in Faber — VaultSharp KV v2 client, VaultConstants, Aspire lifecycle hook for unsealing, Polly resilience, test seeding via HTTP. Use when working with Vault secrets, the Vault module, or Aspire AppHost vault integration.
---

# HashiCorp Vault — ASP.NET Core Integration (Faber)

Faber uses HashiCorp Vault (KV v2) as the secrets store. The Vault module provides `IVaultModuleApi` for secret retrieval. The Aspire AppHost contains a lifecycle hook for automatic unsealing.

Sources: `src/api/Modules/Vault/`, `src/api/Aspire/Faber.AppHost/Vault/`

Canonical sources to mirror before generating code:
- `src/api/Modules/Vault/Faber.Modules.Vault.PublicApi/VaultConstants.cs`
- `src/api/Modules/Vault/Faber.Modules.Vault.PublicApi/IVaultModuleApi.cs`
- `src/api/Modules/Vault/Faber.Modules.Vault.Application/VaultModuleApi.cs`
- `src/api/Modules/Vault/Faber.Modules.Vault.Application/DependencyInjection.cs`
- `src/api/Aspire/Faber.AppHost/Vault/VaultExtensions.cs`
- `src/api/Aspire/Faber.AppHost/Vault/VaultUnsealHook.cs`
- `src/api/Aspire/Faber.AppHost/Vault/IVaultApi.cs`
- `src/api/Aspire/Faber.AppHost/Program.cs`
- `tests/Integration/Modules/Auth/Faber.Modules.Auth.Application.Tests/WebApp.cs`

---

## Architecture

```
AppHost startup
  └── VaultUnsealHook (IDistributedApplicationLifecycleHook)
        ├── Wait for Vault health check
        └── Unseal with keys from file

Runtime
  └── IVaultModuleApi.GetSecretValueAsync(path, mountPoint, key)
        └── VaultSharp → KV v2 ReadSecretAsync
              └── Returns the value, or throws InvalidOperationException naming mount/path/key
```

---

## 1. VaultConstants — Single Source of Truth

```csharp
// Faber.Modules.Vault.PublicApi/VaultConstants.cs
public static class VaultConstants
{
    public const string DefaultMountPoint = "secrets";

    public static class Paths
    {
        public const string Database = "database";
        public const string Keycloak = "keycloak";
        public const string Mail = "mail";
    }

    public static class Keys
    {
        public static class Database
        {
            public const string Host = "host";
            public const string Port = "port";
            public const string Name = "name";
            public const string Username = "username";
            public const string Password = "password";
        }

        public static class Keycloak
        {
            public const string ClientId = "client-id";
            public const string ClientSecret = "client-secret";
        }

        public static class Mail
        {
            public const string ResendApiKey = "resend-api-key";
        }
    }
}
```

---

## 2. IVaultModuleApi Contract

```csharp
// Faber.Modules.Vault.PublicApi/IVaultModuleApi.cs
public interface IVaultModuleApi
{
    Task<string> GetSecretValueAsync(string path, string mountPoint, string key);
}
```

Usage from module setup / options setup:
```csharp
var clientSecret = await vaultApi.GetSecretValueAsync(
    VaultConstants.Paths.Keycloak,
    VaultConstants.DefaultMountPoint,
    VaultConstants.Keys.Keycloak.ClientSecret);
```

---

## 3. VaultModuleApi — VaultSharp Implementation

Reads are wrapped in a Polly v8 `ResiliencePipeline` so cold starts tolerate a briefly-sealed Vault. Pipeline params come from `VaultModuleOptions` (section `VaultModule`) — `MaxRetryAttempts`, `BaseDelaySeconds`, `MaxDelaySeconds` (defaults: 6 / 1.0 / 8.0). Exponential backoff with jitter.

```csharp
// VaultModuleApi.cs
public class VaultModuleApi : IVaultModuleApi
{
    private readonly IVaultClient _vaultClient;
    private readonly ILogger<VaultModuleApi> _logger;
    private readonly ResiliencePipeline _pipeline;

    public VaultModuleApi(
        IVaultClient vaultClient,
        IOptions<VaultModuleOptions> options,
        ILogger<VaultModuleApi> logger)
    {
        _vaultClient = vaultClient;
        _logger = logger;

        var opts = options.Value;

        _pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = opts.MaxRetryAttempts,
                Delay = TimeSpan.FromSeconds(opts.BaseDelaySeconds),
                MaxDelay = TimeSpan.FromSeconds(opts.MaxDelaySeconds),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>()
                    .Handle<VaultApiException>(IsTransientVaultException),
                OnRetry = args => { /* logs warning */ return ValueTask.CompletedTask; }
            })
            .Build();
    }

    public async Task<string> GetSecretValueAsync(string path, string mountPoint, string key)
    {
        try
        {
            var secret = await _pipeline.ExecuteAsync(
                async _ => await _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(path, mountPoint: mountPoint));

            if (secret.Data.Data.TryGetValue(key, out var value) && value?.ToString() is { Length: > 0 } secretValue)
                return secretValue;

            throw new InvalidOperationException(
                $"Vault secret key '{key}' was not found in KV mount '{mountPoint}' at path '{path}'.");
        }
        catch (VaultApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
        {
            // "no handler for route" → missing mount; otherwise → missing path. Both keep `ex` as the cause.
        }
        catch (VaultApiException ex) when (IsTransientVaultException(ex))
        {
            // retries exhausted while sealed / not initialized
        }
        catch (VaultApiException ex)
        {
            // anything else (e.g. 403 from a stale token): message carries the HTTP status, `ex` is the cause
        }
    }

    private static bool IsTransientVaultException(VaultApiException ex) =>
        (ex.Message ?? string.Empty).Contains("Vault is sealed", StringComparison.OrdinalIgnoreCase)
        || (ex.Message ?? string.Empty).Contains("Vault is not initialized", StringComparison.OrdinalIgnoreCase);
}
```

**Retry contract:**
- Retries on `VaultApiException` whose message contains `Vault is sealed` / `Vault is not initialized`, plus `HttpRequestException` and `TaskCanceledException` (HTTP/network transient errors).
- Does **not** retry on auth/permission `VaultApiException` or any other exception — those bubble.
- A required secret is never returned empty. A missing mount, path or key, a Vault that stays sealed, or
  any other Vault error surfaces as `InvalidOperationException` whose message names the KV mount, path and
  key; the original `VaultApiException` is kept as `InnerException` so the status (e.g. 403) stays visible.
- Missing mount/path/key are not transient, so they fail on the first attempt without retrying.

**Configure via `VaultModule` section in `appsettings.json`:**
```jsonc
"VaultModule": {
  "MaxRetryAttempts": 6,
  "BaseDelaySeconds": 1.0,
  "MaxDelaySeconds": 8.0
}
```

---

## 4. Vault DI Registration

```csharp
// DependencyInjection.cs (Vault.Application)
public static class DependencyInjection
{
    public static IServiceCollection AddVaultModule(
        this IServiceCollection services,
        IWebHostEnvironment env,
        IConfiguration configuration)
    {
        services.Configure<VaultModuleOptions>(
            configuration.GetSection(VaultModuleOptions.SectionName));

        services.AddSingleton<IVaultClient>(_ =>
        {
            var token = Environment.GetEnvironmentVariable("VAULT_TOKEN");
            var addr = Environment.GetEnvironmentVariable("VAULT_ADDR");
            var authMethod = new TokenAuthMethodInfo(token);

            var vaultClientSettings = new VaultClientSettings(addr, authMethod)
            {
                MyHttpClientProviderFunc = handler =>
                {
                    if (env.IsDevelopment() && handler is HttpClientHandler httpClientHandler)
                        httpClientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;

                    return new HttpClient(handler);
                };
            };

            return new VaultClient(vaultClientSettings);
        });

        services.AddSingleton<IVaultModuleApi, VaultModuleApi>();

        return services;
    }
}
```

**Key rules:**
- Vault client is **Singleton** — one instance per app lifetime
- `IVaultModuleApi` is also registered as **Singleton**
- Token and address come from **environment variables** `VAULT_TOKEN` and `VAULT_ADDR`
- SSL bypass only in Development

---

## 5. Aspire AppHost — VaultUnsealHook

The AppHost automatically unseals Vault on startup using an `IDistributedApplicationLifecycleHook`:

```csharp
// Faber.AppHost/Vault/VaultUnsealHook.cs
public class VaultUnsealHook(IVaultApi vaultApi, IOptions<VaultOptions> options, ILogger<VaultUnsealHook> logger)
    : IDistributedApplicationLifecycleHook
{
    public async Task AfterResourcesCreatedAsync(DistributedApplicationModel appModel, CancellationToken ct)
    {
        var vaultResource = appModel.Resources
            .OfType<ContainerResource>()
            .FirstOrDefault(r => r.Name == "vault");

        if (vaultResource is null)
        {
            logger.LogDebug("Vault resource not found, skipping unseal");
            return;
        }

        var unsealKeys = ReadUnsealKeys();

        if (unsealKeys.Count == 0)
        {
            logger.LogWarning("No unseal keys found at {Path}", options.Value.UnsealKeysFile);
            return;
        }

        _ = Task.Run(async () =>
        {
            await WaitForVaultInitializationAsync(ct);
            await UnsealVaultAsync(unsealKeys, ct);
        }, ct);
    }

    private async Task WaitForVaultInitializationAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var response = await vaultApi.GetHealthAsync(true, ct);

            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                return;
            }

            await Task.Delay(1000, ct);
        }
    }

    private List<string> ReadUnsealKeys()
    {
        // supports comments/blank lines and values like UNSEAL_KEY_1="keyvalue"
        // ...
    }
}

// VaultOptions.cs
public class VaultOptions
{
    public string UnsealKeysFile { get; set; } = string.Empty;
    public string Address { get; set; } = "http://127.0.0.1:8200";
    public int RetryCount { get; set; } = 5;
    public int RetryDelaySeconds { get; set; } = 2;
    public int TimeoutSeconds { get; set; } = 30;
}
```

**AppHost registration:**
```csharp
// Program.cs (AppHost)
builder.Services.AddVaultIntegration(builder.Configuration, options =>
{
    options.UnsealKeysFile = Path.Combine(vaultDir, ".vault-unseal-keys");
});
```

---

## 6. Aspire IVaultApi — Refit Client (AppHost only)

```csharp
// IVaultApi.cs (AppHost-only, not the module IVaultModuleApi)
public interface IVaultApi
{
    [Get("/v1/sys/health")]
    Task<IApiResponse<VaultHealthResponse>> GetHealthAsync([Query] bool standbyok = true, CancellationToken ct = default);

    [Get("/v1/sys/seal-status")]
    Task<IApiResponse<VaultSealStatusResponse>> GetSealStatusAsync(CancellationToken ct = default);

    [Post("/v1/sys/unseal")]
    Task<IApiResponse<VaultSealStatusResponse>> UnsealAsync([Body] VaultUnsealRequest request, CancellationToken ct = default);
}

// VaultExtensions.cs — Polly resilience
public static IServiceCollection AddVaultIntegration(
    this IServiceCollection services,
    IConfiguration configuration,
    Action<VaultOptions> configureOptions)
{
    services.Configure<VaultOptions>(options =>
    {
        configuration.GetSection(VaultOptions.SectionName).Bind(options);
        configureOptions?.Invoke(options);
    });

    services.AddRefitClient<IVaultApi>()
        .ConfigureHttpClient((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<VaultOptions>>().Value;
            client.BaseAddress = new Uri(opts.Address);
            client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
        })
        .AddPolicyHandler((sp, _) => GetRetryPolicy(sp))
        .AddPolicyHandler((sp, _) => GetCircuitBreakerPolicy(sp));

    services.AddLifecycleHook<VaultUnsealHook>();

    return services;
}
```

Current behavior note:
- retry policy multiplies `RetryDelaySeconds * attempt` rather than using powers of two directly
- circuit breaker uses Polly HTTP extensions and logs open/reset events
- health/unseal code works with `IApiResponse<T>` and inspects `StatusCode`, `IsSuccessStatusCode`, and `Content`

---

## 7. Integration Testing — Vault Container

In tests, Vault runs in **dev mode** (auto-unsealed, no key required) and secrets are seeded via raw HTTP:

```csharp
// WebApp.cs — test fixture
private IContainer _vault = null!;
private readonly HttpClient _vaultHttpClient = new();

protected override async Task PreSetupAsync()
{
    _vault = new ContainerBuilder()
        .WithImage("hashicorp/vault:1.17.3")
        .WithEnvironment("VAULT_DEV_ROOT_TOKEN_ID", "root-token")
        .WithEnvironment("VAULT_DEV_LISTEN_ADDRESS", "0.0.0.0:8200")
        .WithPortBinding(8200, true)
        .WithWaitStrategy(
            Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(strategy =>
                strategy.ForPort(8200).ForPath("/v1/sys/health")))
        .Build();

    await _vault.StartAsync();

    var vaultUrl = $"http://{Vault.Hostname}:{Vault.GetMappedPublicPort(8200)}";
    httpClient.DefaultRequestHeaders.Add("X-Vault-Token", VaultToken);

    await SeedVaultSecretsAsync(vaultUrl);
}

private async Task SeedVaultSecretsAsync(string vaultUrl)
{
    // Enable KV v2 mount
    await _vaultHttpClient.PostAsJsonAsync($"{vaultUrl}/v1/sys/mounts/secrets", new
    {
        type = "kv",
        options = new { version = "2" }
    });

    // Seed keycloak secrets
    await _vaultHttpClient.PostAsJsonAsync($"{vaultUrl}/v1/secrets/data/keycloak", new
        {
            data = new Dictionary<string, string>
            {
                ["client-id"] = KeycloakClientId,
                ["client-secret"] = KeycloakClientSecret
            }
        })).EnsureSuccessStatusCode();

    // Seed database secrets
    await _vaultHttpClient.PostAsJsonAsync($"{vaultUrl}/v1/secrets/data/database", new
    {
        data = new
        {
                ["host"] = "localhost",
                ["port"] = "5432",
                ["name"] = "test",
                ["username"] = "test",
                ["password"] = "test"
        }
        })).EnsureSuccessStatusCode();

    // Seed mail secrets
    await _vaultHttpClient.PostAsJsonAsync($"{vaultUrl}/v1/secrets/data/mail", new
    {
        data = new Dictionary<string, string>
        {
            ["resend-api-key"] = "test-api-key"
        }
    })).EnsureSuccessStatusCode();
}
```

Current test reality:
- auth integration tests explicitly start Vault + seed KV v2 mount and secrets in `WebApp.cs`
- test code sets `VAULT_TOKEN` and `VAULT_ADDR` environment variables before app startup
- seeding uses the same hyphenated secret keys as `VaultConstants` (`client-id`, `client-secret`, `resend-api-key`)

---

## 8. Security Rules

| DO | DON'T |
|---|---|
| Read secrets via `IVaultModuleApi` | Hardcode credentials in code or config |
| Store `VAULT_TOKEN` in env var | Rely on magic defaults for `VAULT_TOKEN` / `VAULT_ADDR` |
| Use KV v2 paths via `VaultConstants` | Use raw string paths inline |
| Bypass SSL only in Development | Bypass SSL in Production |
| Singleton `IVaultClient` | Recreate client per request |
| Let a missing secret fail startup with its mount/path/key | Fall back to `string.Empty` (`?? string.Empty`) for a required secret |
| Keep unseal keys outside normal code paths | Expose vault token in logs |

## 9. Current Caveats

- `VaultModuleApi` retries transient Vault errors (`Vault is sealed` / `Vault is not initialized` / HTTP transient) via a Polly v8 `ResiliencePipeline`; other `VaultApiException`s (auth, permission, etc.) still bubble.
- Missing mounts, paths and keys throw `InvalidOperationException` naming the location — they are not retried.
- Locally, `Faber.Bootstrap` creates the `secrets` KV v2 mount and writes `secrets/keycloak` before the API starts (`secrets/mail` is Production-only and not provisioned); see `docs/decisions/local-stack-provisioning.md`. The AppHost's Vault container itself starts without `VAULT_TOKEN`; only bootstrap and the API take the `vault-token` parameter.
- AppHost resilience policies (`IVaultApi` Refit client) cover unseal/health; they're independent from the in-module retry pipeline.
- `.vault-unseal-keys` is referenced by AppHost startup from `infra/vault/`.
- dev/test patterns differ: AppHost unseals a normal Vault server; Auth integration tests run Vault in dev mode and seed secrets over HTTP; `tests/Integration/Modules/Vault/Faber.Modules.Vault.Application.Tests` runs Vault in production server mode with `inmem` storage to exercise the sealed-startup retry path.
