using Faber.Modules.Vault.PublicApi;
using Microsoft.Extensions.Options;
using Resend;

namespace Faber.Modules.Notifications.PublicApi.OptionsSetup;

public class ResendClientOptionsSetup(IVaultModuleApi vaultModuleApi) : IPostConfigureOptions<ResendClientOptions>
{
    public void PostConfigure(string? name, ResendClientOptions options)
    {
        options.ApiToken = vaultModuleApi.GetSecretValueAsync(
                                   VaultConstants.Paths.Mail,
                                   VaultConstants.DefaultMountPoint,
                                   VaultConstants.Keys.Mail.ResendApiKey)
                               .GetAwaiter().GetResult();
    }
}
