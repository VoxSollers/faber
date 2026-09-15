namespace Faber.Modules.Vault.PublicApi;

public interface IVaultModuleApi
{
    public Task<string> GetSecretValueAsync(string path, string mountPoint, string key);
}
