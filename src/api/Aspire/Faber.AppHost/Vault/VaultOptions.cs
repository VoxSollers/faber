namespace Faber.AppHost.Vault;

public class VaultOptions
{
    public const string SectionName = "Vault";

    public string UnsealKeysFile { get; set; } = string.Empty;

    public string Address { get; set; } = "http://localhost:8200";

    public int RetryCount { get; set; } = 5;

    public int RetryDelaySeconds { get; set; } = 2;

    public int TimeoutSeconds { get; set; } = 30;
}