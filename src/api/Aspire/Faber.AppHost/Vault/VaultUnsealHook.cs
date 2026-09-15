using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Faber.AppHost.Vault;

public class VaultUnsealHook(
    IVaultApi vaultApi,
    IOptions<VaultOptions> options,
    ILogger<VaultUnsealHook> logger)
{
    private readonly VaultOptions _options = options.Value;

    /// <summary>
    /// Kicks off unsealing in the background. Returns immediately so it never blocks the
    /// orchestrator: Vault is not running yet at this point, and the background task polls for it.
    /// </summary>
    /// <param name="ct">A token that cancels the background unseal loop.</param>
    public void StartUnseal(CancellationToken ct = default)
    {
        var unsealKeys = ReadUnsealKeys();

        if (unsealKeys.Count == 0)
        {
            logger.LogWarning("No unseal keys found at {Path}", _options.UnsealKeysFile);

            return;
        }

        _ = Task.Run(
            async () =>
            {
                try
                {
                    await WaitForVaultInitializationAsync(ct);
                    await UnsealVaultAsync(unsealKeys, ct);
                }
                catch (OperationCanceledException)
                {
                    logger.LogDebug("Vault unseal operation was cancelled");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to unseal Vault");
                }
            },
            ct);
    }

    private List<string> ReadUnsealKeys()
    {
        var keys = new List<string>();

        if (!File.Exists(_options.UnsealKeysFile)) return keys;

        using var stream = File.OpenRead(_options.UnsealKeysFile);
        using var reader = new StreamReader(stream);

        while (reader.ReadLine() is { } line)
        {
            var trimmed = line.Trim();

            if (string.IsNullOrEmpty(trimmed)) continue;

            if (trimmed.StartsWith('#')) continue;

            if (!trimmed.StartsWith("UNSEAL_KEY")) continue;

            var equalsIndex = trimmed.IndexOf('=');

            if (equalsIndex < 0) continue;

            var key = trimmed[(equalsIndex + 1)..].Trim().Trim('"');
            if (!string.IsNullOrEmpty(key)) keys.Add(key);
        }

        return keys;
    }

    private async Task WaitForVaultInitializationAsync(CancellationToken ct)
    {
        logger.LogInformation("Waiting for Vault to initialize...");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var response = await vaultApi.GetHealthAsync(true, ct);

                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation("Vault is ready");

                    return;
                }

                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    logger.LogInformation("Vault is ready (sealed)");

                    return;
                }
            }
            catch (HttpRequestException)
            {
                logger.LogDebug("Vault not yet available, retrying...");
            }
            catch (TaskCanceledException) when (!ct.IsCancellationRequested)
            {
                logger.LogDebug("Request timed out, retrying...");
            }

            await Task.Delay(1000, ct);
        }
    }

    private async Task UnsealVaultAsync(List<string> unsealKeys, CancellationToken ct)
    {
        var statusResponse = await vaultApi.GetSealStatusAsync(ct);

        if (!statusResponse.IsSuccessStatusCode)
        {
            logger.LogError("Failed to get Vault seal status: {StatusCode}", statusResponse.StatusCode);

            return;
        }

        if (statusResponse.Content is null)
        {
            logger.LogError("Vault seal status response had no content");

            return;
        }

        var status = statusResponse.Content;

        if (!status.Sealed)
        {
            logger.LogInformation("Vault is already unsealed");

            return;
        }

        logger.LogInformation(
            "Vault is sealed. Threshold: {Threshold}/{Total}",
            status.Threshold,
            status.TotalShares);

        if (unsealKeys.Count < status.Threshold)
        {
            logger.LogError(
                "Not enough unseal keys. Required: {Required}, Available: {Available}",
                status.Threshold,
                unsealKeys.Count);

            return;
        }

        foreach (var key in unsealKeys.Take(status.Threshold))
        {
            var unsealResponse = await vaultApi.UnsealAsync(new VaultUnsealRequest(key), ct);

            if (!unsealResponse.IsSuccessStatusCode)
            {
                logger.LogError("Failed to unseal Vault: {StatusCode}", unsealResponse.StatusCode);

                return;
            }

            if (unsealResponse.Content is null)
            {
                logger.LogError("Vault unseal response had no content");

                return;
            }

            logger.LogInformation(
                "Unseal progress: {Progress}/{Threshold}",
                unsealResponse.Content.Progress,
                status.Threshold);

            if (!unsealResponse.Content.Sealed)
            {
                logger.LogInformation("Vault successfully unsealed");

                return;
            }
        }
    }
}