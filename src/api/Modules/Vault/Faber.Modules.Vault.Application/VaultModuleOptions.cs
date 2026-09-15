namespace Faber.Modules.Vault.Application;

public class VaultModuleOptions
{
    public const string SectionName = "VaultModule";

    public int MaxRetryAttempts { get; set; } = 6;

    public double BaseDelaySeconds { get; set; } = 1.0;

    public double MaxDelaySeconds { get; set; } = 8.0;
}
