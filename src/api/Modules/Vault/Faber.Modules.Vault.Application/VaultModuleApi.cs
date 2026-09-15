using System.Net;
using Faber.Modules.Vault.PublicApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using VaultSharp;
using VaultSharp.Core;

namespace Faber.Modules.Vault.Application;

public class VaultModuleApi : IVaultModuleApi
{
    private readonly IVaultClient _vaultClient;
    private readonly ResiliencePipeline _pipeline;

    public VaultModuleApi(
        IVaultClient vaultClient,
        IOptions<VaultModuleOptions> options,
        ILogger<VaultModuleApi> logger)
    {
        _vaultClient = vaultClient;

        var opts = options.Value;

        _pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = opts.MaxRetryAttempts,
                Delay = TimeSpan.FromSeconds(opts.BaseDelaySeconds),
                MaxDelay = TimeSpan.FromSeconds(opts.MaxDelaySeconds),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                // Outside Aspire nothing gates the API on Vault's health, so a Vault that is briefly
                // unreachable or still sealed is retried rather than failing startup outright.
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>()
                    .Handle<VaultApiException>(IsTransientVaultException),
                OnRetry = args =>
                {
                    logger.LogWarning(
                        "Vault read failed (attempt {Attempt}). Retrying in {Delay}s. Reason: {Reason}",
                        args.AttemptNumber + 1,
                        args.RetryDelay.TotalSeconds,
                        GetRetryReason(args.Outcome.Exception));

                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    public async Task<string> GetSecretValueAsync(string path, string mountPoint, string key)
    {
        try
        {
            var secret = await _pipeline.ExecuteAsync(
                async _ => await _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(path, mountPoint: mountPoint));

            if (secret.Data.Data.TryGetValue(key, out var value) && value is not null)
            {
                var secretValue = value.ToString();
                if (!string.IsNullOrEmpty(secretValue)) return secretValue;
            }

            throw new InvalidOperationException(
                $"Vault secret key '{key}' was not found in KV mount '{mountPoint}' at path '{path}'.");
        }
        catch (VaultApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
        {
            if (IsMissingMountException(ex))
            {
                throw new InvalidOperationException(
                    $"Vault KV mount '{mountPoint}' was not found while reading path '{path}' and key '{key}'.",
                    ex);
            }

            throw new InvalidOperationException(
                $"Vault secret path '{path}' was not found in KV mount '{mountPoint}' while reading key '{key}'.",
                ex);
        }
        catch (VaultApiException ex) when (IsTransientVaultException(ex))
        {
            var state = IsUninitializedVaultException(ex) ? "not initialized" : "sealed";
            throw new InvalidOperationException(
                $"Vault secret at KV mount '{mountPoint}', path '{path}', key '{key}' could not be read because Vault is {state}.",
                ex);
        }
        catch (VaultApiException ex)
        {
            // Keep the Vault exception as the cause: a 403 from an expired or wrong token must stay
            // distinguishable from a missing secret.
            throw new InvalidOperationException(
                $"Vault secret lookup failed for KV mount '{mountPoint}', path '{path}', key '{key}' " +
                $"(HTTP {(int)ex.HttpStatusCode} {ex.HttpStatusCode}).",
                ex);
        }
    }

    private static bool IsMissingMountException(VaultApiException ex) =>
        ex.ApiErrors?.Any(error => error.Contains("no handler for route", StringComparison.OrdinalIgnoreCase)) == true;

    private static string GetRetryReason(Exception? exception) => exception switch
    {
        VaultApiException vaultException when IsTransientVaultException(vaultException) =>
            IsUninitializedVaultException(vaultException)
                ? "Vault is not initialized"
                : "Vault is sealed",
        HttpRequestException => "Vault is unreachable",
        TaskCanceledException => "Vault request timed out",
        _ => "Transient Vault failure"
    };

    private static bool IsUninitializedVaultException(VaultApiException ex) =>
        ex.Message?.Contains("Vault is not initialized", StringComparison.OrdinalIgnoreCase) == true;

    private static bool IsTransientVaultException(VaultApiException ex)
    {
        var message = ex.Message ?? string.Empty;

        return message.Contains("Vault is sealed", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Vault is not initialized", StringComparison.OrdinalIgnoreCase);
    }
}
