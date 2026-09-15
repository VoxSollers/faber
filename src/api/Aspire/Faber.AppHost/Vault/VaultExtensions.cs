using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Retry;
using Refit;

namespace Faber.AppHost.Vault;

public static class VaultExtensions
{
    public static IDistributedApplicationBuilder AddVaultIntegration(
        this IDistributedApplicationBuilder builder,
        Action<VaultOptions>? configure = null)
    {
        builder.Services.Configure<VaultOptions>(options =>
        {
            builder.Configuration.GetSection(VaultOptions.SectionName).Bind(options);
            configure?.Invoke(options);
        });

        builder.Services.AddRefitGeneratedClient<IVaultApi>()
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<VaultOptions>>().Value;
                client.BaseAddress = new Uri(options.Address);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            })
            .AddPolicyHandler((sp, _) => GetRetryPolicy(sp))
            .AddPolicyHandler((sp, _) => GetCircuitBreakerPolicy(sp));

        builder.Services.AddSingleton<VaultUnsealHook>();

        return builder;
    }

    /// <summary>
    /// Starts the Vault unseal hook as soon as the Vault container is about to start.
    /// </summary>
    /// <param name="builder">The Vault container resource builder.</param>
    /// <returns>The same builder, for chaining.</returns>
    /// <remarks>
    /// This deliberately subscribes to <see cref="BeforeResourceStartedEvent"/> for the Vault
    /// resource itself, NOT to the application-wide <see cref="AfterResourcesCreatedEvent"/>.
    /// That global event is published only after every resource has been created, and any resource
    /// gated on Vault's health check (<c>faberapi</c> via <c>WaitFor(vault)</c>) cannot be created
    /// until Vault reports healthy — which in turn requires this hook to have unsealed it. Hanging
    /// the hook off the global event therefore deadlocks the whole app host on a sealed Vault.
    /// The hook polls Vault itself until it answers, so starting this early is safe.
    /// </remarks>
    public static IResourceBuilder<ContainerResource> WithUnsealHook(
        this IResourceBuilder<ContainerResource> builder)
    {
        builder.ApplicationBuilder.Eventing.Subscribe<BeforeResourceStartedEvent>(
            builder.Resource,
            (@event, ct) =>
            {
                @event.Services.GetRequiredService<VaultUnsealHook>().StartUnseal(ct);

                return Task.CompletedTask;
            });

        return builder;
    }

    private static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy(IServiceProvider sp)
    {
        var options = sp.GetRequiredService<IOptions<VaultOptions>>().Value;
        var logger = sp.GetRequiredService<ILogger<IVaultApi>>();

        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r =>
            {
                var code = (int)r.StatusCode;

                return code >= 500 && r.StatusCode != HttpStatusCode.ServiceUnavailable;
            })
            .WaitAndRetryAsync(
                options.RetryCount,
                attempt =>
                    TimeSpan.FromSeconds(options.RetryDelaySeconds * attempt),
                (outcome, delay, attempt, _) =>
                {
                    logger.LogWarning(
                        "Vault request failed. Retry {Attempt} after {Delay}s. Reason: {Reason}",
                        attempt,
                        delay.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                });
    }

    private static AsyncCircuitBreakerPolicy<
        HttpResponseMessage> GetCircuitBreakerPolicy(IServiceProvider sp)
    {
        var logger = sp.GetRequiredService<ILogger<IVaultApi>>();

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                3,
                TimeSpan.FromSeconds(30),
                (_, duration) =>
                {
                    logger.LogError("Vault circuit breaker opened for {Duration}s", duration.TotalSeconds);
                },
                () =>
                {
                    logger.LogInformation("Vault circuit breaker reset");
                });
    }
}
