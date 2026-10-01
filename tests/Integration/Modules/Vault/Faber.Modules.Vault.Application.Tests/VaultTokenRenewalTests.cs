using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;

namespace Faber.Modules.Vault.Application.Tests;

/// <summary>Exercises token renewal against a real Vault server with restricted policies.</summary>
public class VaultTokenRenewalTests : IAsyncLifetime
{
    private readonly HttpClient _admin = new();
    private IContainer _vault = null!;
    private string _address = string.Empty;

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        _vault = new ContainerBuilder("hashicorp/vault:1.17.3")
            .WithEnvironment("VAULT_DEV_ROOT_TOKEN_ID", "renewal-test-root")
            .WithEnvironment("VAULT_DEV_LISTEN_ADDRESS", "0.0.0.0:8200")
            .WithPortBinding(8200, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(s =>
                s.ForPort(8200).ForPath("/v1/sys/health")))
            .Build();
        await _vault.StartAsync(TestContext.Current.CancellationToken);
        _address = $"http://{_vault.Hostname}:{_vault.GetMappedPublicPort(8200)}";
        _admin.BaseAddress = new Uri(_address);
        _admin.DefaultRequestHeaders.Add("X-Vault-Token", "renewal-test-root");
        await WriteAsync("/v1/sys/mounts/secrets", new { type = "kv", options = new { version = "2" } });
        foreach (var path in new[] { "keycloak", "mail", "database", "other" })
        {
            await WriteAsync($"/v1/secrets/data/{path}", new { data = new { value = "seeded" } });
        }
        await WritePolicyAsync("reader", true);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _admin.Dispose();
        await _vault.DisposeAsync();
    }

    /// <summary>Verifies that periodic tokens retain permitted secret access across repeated renewals.</summary>
    [Fact]
    public async Task PeriodicToken_ShouldReadSecretsBeyondOriginalLifetimeAndRenewRepeatedly()
    {
        await WriteAsync("/v1/sys/mounts/auth/token/tune", new { default_lease_ttl = "4s", max_lease_ttl = "6s" });
        var token = await CreateTokenAsync(new { policies = new[] { "reader" }, no_default_policy = true, period = "4s" });
        var logger = new RecordingLogger();
        var client = CreateClient(token);
        using var service = new VaultTokenRenewalService(client, logger);
        await service.StartAsync(TestContext.Current.CancellationToken);
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(9), TestContext.Current.CancellationToken);
            var api = new VaultModuleApi(client, Options.Create(new VaultModuleOptions()), NullLogger<VaultModuleApi>.Instance);
            (await api.GetSecretValueAsync("keycloak", "secrets", "value")).ShouldBe("seeded");
            logger.Messages.Count(m => m.Contains("renewed successfully")).ShouldBeGreaterThanOrEqualTo(3);
            using var http = TokenHttp(token);
            using var forbidden = await http.PostAsJsonAsync("/v1/auth/token/create", new { }, TestContext.Current.CancellationToken);
            forbidden.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
            foreach (var path in new[] { "keycloak", "mail", "database" })
            {
                using var read = await http.GetAsync($"/v1/secrets/data/{path}", TestContext.Current.CancellationToken);
                read.StatusCode.ShouldBe(HttpStatusCode.OK);
            }
            using var extra = await http.GetAsync("/v1/secrets/data/other", TestContext.Current.CancellationToken);
            extra.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
            using var write = await http.PostAsJsonAsync("/v1/secrets/data/keycloak", new { data = new { value = "overwritten" } }, TestContext.Current.CancellationToken);
            write.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        }
        finally
        {
            await service.StopAsync(TestContext.Current.CancellationToken);
        }
    }

    /// <summary>Verifies that expiring nonrenewable tokens produce safe, actionable errors.</summary>
    [Fact]
    public async Task NonrenewableToken_ShouldReportActionableErrorWithoutLeakingToken()
    {
        var token = await CreateTokenAsync(new { policies = new[] { "reader" }, no_default_policy = true, ttl = "4s", renewable = false });
        var logger = new RecordingLogger();
        using var service = new VaultTokenRenewalService(CreateClient(token), logger);
        await service.StartAsync(TestContext.Current.CancellationToken);
        await service.ExecuteTask!.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        logger.Messages.ShouldContain(m => m.Contains("not renewable"));
        logger.Messages.ShouldAllBe(m => !m.Contains(token));
    }

    /// <summary>Verifies that nonexpiring root tokens need no renewal.</summary>
    [Fact]
    public async Task NonexpiringRootToken_ShouldSkipRenewal()
    {
        var logger = new RecordingLogger();
        using var service = new VaultTokenRenewalService(CreateClient("renewal-test-root"), logger);
        await service.StartAsync(TestContext.Current.CancellationToken);
        await service.ExecuteTask!.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        logger.Messages.ShouldContain(m => m.Contains("no expiry"));
        logger.Messages.ShouldNotContain(m => m.Contains("renewed successfully"));
    }

    /// <summary>Verifies that denied renewal permissions stop renewal with sanitized diagnostics.</summary>
    [Fact]
    public async Task RenewalPermissionDenied_ShouldStopAndLogSanitizedFailure()
    {
        await WritePolicyAsync("no-renew", false);
        var token = await CreateTokenAsync(new { policies = new[] { "no-renew" }, no_default_policy = true, period = "4s" });
        var logger = new RecordingLogger();
        using var service = new VaultTokenRenewalService(CreateClient(token), logger);
        await service.StartAsync(TestContext.Current.CancellationToken);
        await service.ExecuteTask!.WaitAsync(TimeSpan.FromSeconds(8), TestContext.Current.CancellationToken);
        logger.Messages.ShouldContain(m => m.Contains("HTTP 403") && m.Contains("renewal stopped"));
        logger.Messages.ShouldAllBe(m => !m.Contains(token));
    }

    /// <summary>Verifies that revoked tokens stop renewal without exposing credentials.</summary>
    [Fact]
    public async Task RevokedToken_ShouldStopAndLogSanitizedFailure()
    {
        var token = await CreateTokenAsync(new { policies = new[] { "reader" }, no_default_policy = true, period = "8s" });
        var logger = new RecordingLogger();
        using var service = new VaultTokenRenewalService(CreateClient(token), logger);
        await service.StartAsync(TestContext.Current.CancellationToken);
        await WaitUntilAsync(() => logger.Messages.Any(m => m.Contains("renewal enabled")));
        await WriteAsync("/v1/auth/token/revoke", new { token });
        await service.ExecuteTask!.WaitAsync(TimeSpan.FromSeconds(8), TestContext.Current.CancellationToken);
        logger.Messages.ShouldContain(m => m.Contains("renewal stopped"));
        logger.Messages.ShouldAllBe(m => !m.Contains(token));
    }

    /// <summary>Verifies that transient renewal failures recover and subsequent renewals continue.</summary>
    [Fact]
    public async Task TransientRenewalFailures_ShouldRecoverAfterSevenAttemptsAndKeepRenewing()
    {
        var token = await CreateTokenAsync(new { policies = new[] { "reader" }, no_default_policy = true, period = "40s" });
        var logger = new RecordingLogger();
        var handler = new RenewalFailureHandler(7);
        var client = CreateClient(token, handler);
        using var service = new VaultTokenRenewalService(client, logger);
        await service.StartAsync(TestContext.Current.CancellationToken);
        try
        {
            await WaitUntilAsync(() => logger.Messages.Count(m => m.Contains("renewed successfully")) >= 2, TimeSpan.FromSeconds(65));
            handler.FailureCount.ShouldBe(7);
            var api = new VaultModuleApi(client, Options.Create(new VaultModuleOptions()), NullLogger<VaultModuleApi>.Instance);
            (await api.GetSecretValueAsync("keycloak", "secrets", "value")).ShouldBe("seeded");
            logger.Messages.ShouldAllBe(m => !m.Contains(token) && !m.Contains("sensitive-payload"));
        }
        finally
        {
            await service.StopAsync(TestContext.Current.CancellationToken);
        }
    }

    /// <summary>Verifies that explicit maximum TTLs are reported and shortened leases are respected.</summary>
    [Fact]
    public async Task ExplicitMaximumTtl_ShouldWarnAndRespectShortenedRenewalLease()
    {
        var token = await CreateTokenAsync(new { policies = new[] { "reader" }, no_default_policy = true, period = "8s", explicit_max_ttl = "10s" });
        var logger = new RecordingLogger();
        using var service = new VaultTokenRenewalService(CreateClient(token), logger);
        await service.StartAsync(TestContext.Current.CancellationToken);
        await service.ExecuteTask!.WaitAsync(TimeSpan.FromSeconds(15), TestContext.Current.CancellationToken);
        logger.Messages.ShouldContain(m => m.Contains("explicit maximum TTL"));
        logger.Messages.ShouldContain(m => m.Contains("renewed successfully") && !m.Contains("TTL is 8 seconds"));
        logger.Messages.ShouldContain(m => m.Contains("renewal stopped") || m.Contains("lifetime exhausted") || m.Contains("no renewable lifetime"));
        logger.Messages.Count.ShouldBeLessThan(40);
    }

    /// <summary>Verifies that shutdown interrupts scheduled renewal promptly.</summary>
    [Fact]
    public async Task StoppingDuringDelay_ShouldCancelPromptlyWithoutFurtherRenewals()
    {
        var token = await CreateTokenAsync(new { policies = new[] { "reader" }, no_default_policy = true, period = "60s" });
        var logger = new RecordingLogger();
        using var service = new VaultTokenRenewalService(CreateClient(token), logger);
        await service.StartAsync(TestContext.Current.CancellationToken);
        await WaitUntilAsync(() => logger.Messages.Any(m => m.Contains("renewal enabled")));
        await service.StopAsync(TestContext.Current.CancellationToken).WaitAsync(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
        service.ExecuteTask!.IsCompleted.ShouldBeTrue();
        logger.Messages.ShouldNotContain(m => m.Contains("renewed successfully"));
    }

    /// <summary>Verifies that persistent outages exhaust the token lifetime without leaking payloads.</summary>
    [Fact]
    public async Task PersistentTransientFailures_ShouldStopAtTokenExpiryWithoutLeakingPayload()
    {
        var token = await CreateTokenAsync(new { policies = new[] { "reader" }, no_default_policy = true, period = "4s" });
        var logger = new RecordingLogger();
        var handler = new RenewalFailureHandler(100);
        using var service = new VaultTokenRenewalService(CreateClient(token, handler), logger);
        await service.StartAsync(TestContext.Current.CancellationToken);
        await service.ExecuteTask!.WaitAsync(TimeSpan.FromSeconds(8), TestContext.Current.CancellationToken);
        logger.Messages.ShouldContain(m => m.Contains("lifetime exhausted"));
        logger.Messages.ShouldAllBe(m => !m.Contains(token) && !m.Contains("sensitive-payload"));
        handler.FailureCount.ShouldBeLessThan(15);
    }

    /// <summary>Verifies that shutdown interrupts an outstanding metadata request promptly.</summary>
    [Fact]
    public async Task StoppingDuringLookupRequest_ShouldCancelPromptly()
    {
        var handler = new HangingLookupHandler();
        var logger = new RecordingLogger();
        using var service = new VaultTokenRenewalService(CreateClient("renewal-test-root", handler), logger);
        await service.StartAsync(TestContext.Current.CancellationToken);
        await handler.Entered.Task.WaitAsync(TimeSpan.FromSeconds(3), TestContext.Current.CancellationToken);
        try
        {
            await service.StopAsync(TestContext.Current.CancellationToken).WaitAsync(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
            service.ExecuteTask!.IsCompleted.ShouldBeTrue();
            logger.Messages.ShouldNotContain(m => m.Contains("lifecycle request failed"));
        }
        finally
        {
            handler.Release.TrySetResult();
        }
    }

    private sealed class HangingLookupHandler : DelegatingHandler
    {
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Entered.TrySetResult();
            await Release.Task.WaitAsync(cancellationToken);
            return await base.SendAsync(request, cancellationToken);
        }
    }

    /// <summary>Verifies that long token lifetimes remain supported by the renewal scheduler.</summary>
    [Fact]
    public async Task VeryLongTokenLifetime_ShouldKeepRenewalWorkerRunning()
    {
        await WriteAsync("/v1/sys/mounts/auth/token/tune", new { default_lease_ttl = "2400h", max_lease_ttl = "4800h" });
        var token = await CreateTokenAsync(new { policies = new[] { "reader" }, no_default_policy = true, period = "2400h" });
        var logger = new RecordingLogger();
        using var service = new VaultTokenRenewalService(CreateClient(token), logger);
        await service.StartAsync(TestContext.Current.CancellationToken);
        try
        {
            await WaitUntilAsync(() => logger.Messages.Any(m => m.Contains("renewal enabled")));
            await Task.Delay(100, TestContext.Current.CancellationToken);
            service.ExecuteTask!.IsCompleted.ShouldBeFalse();
            logger.Messages.ShouldNotContain(m => m.Contains("lifecycle request failed"));
        }
        finally
        {
            await service.StopAsync(TestContext.Current.CancellationToken);
        }
    }

    /// <summary>Verifies that metadata inspection recovers after prolonged transient failures.</summary>
    [Fact]
    public async Task TransientLookupFailures_ShouldRecoverAfterSevenAttempts()
    {
        var token = await CreateTokenAsync(new { policies = new[] { "reader" }, no_default_policy = true, period = "60s" });
        var logger = new RecordingLogger();
        var handler = new RenewalFailureHandler(7, "/lookup-self");
        using var service = new VaultTokenRenewalService(CreateClient(token, handler), logger);
        await service.StartAsync(TestContext.Current.CancellationToken);
        try
        {
            await WaitUntilAsync(() => logger.Messages.Any(m => m.Contains("renewal enabled")), TimeSpan.FromSeconds(22));
            handler.FailureCount.ShouldBe(7);
            logger.Messages.ShouldAllBe(m => !m.Contains(token) && !m.Contains("sensitive-payload"));
        }
        finally
        {
            await service.StopAsync(TestContext.Current.CancellationToken);
        }
    }

    private VaultClient CreateClient(string token, DelegatingHandler? handler = null) => new(new VaultClientSettings(_address, new TokenAuthMethodInfo(token))
    {
        MyHttpClientProviderFunc = inner =>
        {
            if (handler is null)
            {
                return new HttpClient(inner);
            }
            handler.InnerHandler = inner;
            return new HttpClient(handler);
        }
    });

    private static async Task WaitUntilAsync(Func<bool> condition, TimeSpan? timeout = null)
    {
        var clock = Stopwatch.StartNew();
        while (!condition())
        {
            if (clock.Elapsed > (timeout ?? TimeSpan.FromSeconds(5)))
            {
                throw new TimeoutException("Expected Vault lifecycle event was not observed.");
            }
            await Task.Delay(50, TestContext.Current.CancellationToken);
        }
    }

    private sealed class RenewalFailureHandler(int failures, string endpoint = "/renew-self") : DelegatingHandler
    {
        public int FailureCount { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri!.AbsolutePath.EndsWith(endpoint) && FailureCount < failures)
            {
                FailureCount++;
                throw new HttpRequestException("sensitive-payload");
            }
            return base.SendAsync(request, cancellationToken);
        }
    }

    private HttpClient TokenHttp(string token)
    {
        var http = new HttpClient { BaseAddress = new Uri(_address) };
        http.DefaultRequestHeaders.Add("X-Vault-Token", token);
        return http;
    }

    private async Task<string> CreateTokenAsync(object body)
    {
        using var response = await _admin.PostAsJsonAsync("/v1/auth/token/create", body, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        return json.GetProperty("auth").GetProperty("client_token").GetString()!;
    }

    private Task WritePolicyAsync(string name, bool allowRenewal) => WriteAsync($"/v1/sys/policies/acl/{name}", new
    {
        policy = "path \"secrets/data/keycloak\" { capabilities = [\"read\"] }\n" +
                 "path \"secrets/data/mail\" { capabilities = [\"read\"] }\n" +
                 "path \"secrets/data/database\" { capabilities = [\"read\"] }\n" +
                 "path \"auth/token/lookup-self\" { capabilities = [\"read\"] }\n" +
                 (allowRenewal ? "path \"auth/token/renew-self\" { capabilities = [\"update\"] }" : string.Empty)
    });

    private async Task WriteAsync(string path, object body)
    {
        using var response = await _admin.PostAsJsonAsync(path, body, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private sealed class RecordingLogger : ILogger<VaultTokenRenewalService>
    {
        public ConcurrentQueue<string> Messages { get; } = new();
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            exception.ShouldBeNull();
            Messages.Enqueue(formatter(state, exception));
        }
    }
}
