using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VaultSharp;
using VaultSharp.Core;

namespace Faber.Modules.Vault.Application;

/// <summary>Renews the configured API token before its current Vault lease expires.</summary>
/// <remarks>
/// Renewal preserves the singleton client's token. Vault maximum TTL rules still apply;
/// production deployments require periodic tokens without an explicit maximum TTL for indefinite renewal.
/// </remarks>
/// <param name="client">The shared authenticated Vault client.</param>
/// <param name="logger">The logger receiving sanitized lifecycle events.</param>
public sealed class VaultTokenRenewalService(IVaultClient client, ILogger<VaultTokenRenewalService> logger)
    : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // VaultSharp token APIs have no cancellation parameter. WaitAsync bounds the hosted
        // worker's waits without changing the HTTP client shared by secret reads.
        try
        {
            var metadata = await TryRequestAsync(
                () => client.V1.Auth.Token.LookupSelfAsync(), null, stoppingToken);
            if (metadata is null)
            {
                return;
            }

            var ttl = metadata.Value.Data.TimeToLive;
            if (ttl == 0 && string.IsNullOrEmpty(metadata.Value.Data.ExpireTime))
            {
                logger.LogInformation("Vault token has no expiry; renewal is not required.");
                return;
            }

            if (!metadata.Value.Data.Renewable)
            {
                logger.LogError("Vault API token expires but is not renewable; provision a renewable periodic token.");
                return;
            }

            if (metadata.Value.Data.ExplicitMaximumTimeToLive > 0)
            {
                logger.LogWarning("Vault API token has an explicit maximum TTL; renewal cannot prevent eventual expiry.");
            }

            // lookup-self's typed VaultSharp model does not expose 'period'. Never infer that
            // a renewable token is periodic, or that renewal removes the system/mount max TTL.
            logger.LogInformation("Vault token renewal enabled; ordinary tokens remain limited by Vault maximum TTL. Use a periodic token without explicit maximum TTL for indefinite renewal.");
            var started = metadata.RequestedAt;
            var lifetime = TimeSpan.FromSeconds(ttl);
            while (!stoppingToken.IsCancellationRequested)
            {
                var elapsed = Stopwatch.GetElapsedTime(started);
                var remaining = lifetime - elapsed;
                if (remaining <= TimeSpan.Zero)
                {
                    logger.LogError("Vault API token lifetime exhausted before renewal; provision a new token.");
                    return;
                }

                var renewalDelay = remaining / 2;
                await Task.Delay(renewalDelay < TimeSpan.FromDays(1) ? renewalDelay : TimeSpan.FromDays(1), stoppingToken);
                var renewed = await TryRequestAsync(
                    () => client.V1.Auth.Token.RenewSelfAsync(),
                    () => lifetime - Stopwatch.GetElapsedTime(started), stoppingToken);
                if (renewed is null)
                {
                    return;
                }

                if (renewed.Value is not { Renewable: true, LeaseDurationSeconds: > 0 } auth)
                {
                    logger.LogError("Vault API token renewal returned no renewable lifetime; provision a new token.");
                    return;
                }

                lifetime = TimeSpan.FromSeconds(auth.LeaseDurationSeconds);
                started = renewed.RequestedAt;
                logger.LogInformation("Vault API token renewed successfully; granted TTL is {TtlSeconds} seconds.", auth.LeaseDurationSeconds);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogDebug("Vault token renewal stopped.");
        }
        catch (Exception exception)
        {
            LogFailure(exception, terminal: true);
        }
    }

    private async Task<RequestResult<T>?> TryRequestAsync<T>(Func<Task<T>> request, Func<TimeSpan>? remainingLifetime,
        CancellationToken stoppingToken) where T : class
    {
        for (var attempt = 0; ; attempt = Math.Min(attempt + 1, 5))
        {
            stoppingToken.ThrowIfCancellationRequested();
            var remaining = remainingLifetime?.Invoke();
            if (remaining <= TimeSpan.Zero)
            {
                logger.LogError("Vault API token lifetime exhausted while retrying renewal; provision a new token.");
                return null;
            }

            try
            {
                // Bound initial lookup as well: a hung server must not hold shutdown indefinitely.
                var requestedAt = Stopwatch.GetTimestamp();
                var timeout = remaining is { } lifetime && lifetime < TimeSpan.FromSeconds(30)
                    ? lifetime
                    : TimeSpan.FromSeconds(30);
                var value = await request().WaitAsync(timeout, stoppingToken);
                return new RequestResult<T>(value, requestedAt);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                var retry = IsTransient(exception);
                LogFailure(exception, terminal: !retry);
                if (!retry)
                {
                    return null;
                }
            }

            var delay = TimeSpan.FromSeconds(Math.Min(0.25 * Math.Pow(2, attempt), 4));
            if (remainingLifetime is not null)
            {
                var available = remainingLifetime();
                if (available <= TimeSpan.Zero)
                {
                    logger.LogError("Vault API token lifetime exhausted while retrying renewal; provision a new token.");
                    return null;
                }
                var retryWindow = available / 2;
                var minimumDelay = TimeSpan.FromMilliseconds(100);
                retryWindow = retryWindow > minimumDelay ? retryWindow : minimumDelay;
                delay = delay < retryWindow ? delay : retryWindow;
                delay = delay < available ? delay : available;
            }
            await Task.Delay(delay, stoppingToken);
        }
    }

    private sealed record RequestResult<T>(T Value, long RequestedAt);

    private static bool IsTransient(Exception exception) => exception switch
    {
        HttpRequestException or TaskCanceledException or TimeoutException => true,
        VaultApiException vaultException => vaultException.HttpStatusCode is
            HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests || (int)vaultException.HttpStatusCode >= 500,
        _ => false
    };

    private void LogFailure(Exception exception, bool terminal)
    {
        var status = exception is VaultApiException vaultException ? (int)vaultException.HttpStatusCode : (int?)null;
        // Never pass the exception to the logger: Vault payloads may contain credentials.
        logger.Log(terminal ? LogLevel.Error : LogLevel.Warning,
            "Vault token lifecycle request failed ({FailureType}, HTTP {StatusCode}); {Action}.",
            exception.GetType().Name, status,
            terminal ? "renewal stopped; check token validity and lookup-self/renew-self permissions" : "retrying before token expiry");
    }
}
