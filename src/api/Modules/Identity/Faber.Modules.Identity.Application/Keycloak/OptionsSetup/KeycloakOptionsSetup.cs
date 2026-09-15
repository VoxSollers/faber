using Faber.Modules.Common.PublicApi;
using Faber.Modules.Identity.Application.Keycloak.Options;
using Faber.Modules.Vault.PublicApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Faber.Modules.Identity.Application.Keycloak.OptionsSetup;

public class KeycloakOptionsSetup : IConfigureOptions<KeycloakOptions>
{
    private readonly IConfiguration _configuration;
    private readonly IVaultModuleApi _vaultModuleApi;

    public KeycloakOptionsSetup(IConfiguration configuration, IVaultModuleApi vaultModuleApi)
    {
        _configuration = configuration;
        _vaultModuleApi = vaultModuleApi;
    }

    public void Configure(KeycloakOptions options)
    {
        _configuration.GetSection(ConfigurationConstants.Sections.Keycloak).Bind(options);

        options.ClientId = _vaultModuleApi.GetSecretValueAsync(
                                   VaultConstants.Paths.Keycloak,
                                   VaultConstants.DefaultMountPoint,
                                   VaultConstants.Keys.Keycloak.ClientId)
                               .GetAwaiter().GetResult();

        options.ClientSecret = _vaultModuleApi.GetSecretValueAsync(
                                       VaultConstants.Paths.Keycloak,
                                       VaultConstants.DefaultMountPoint,
                                       VaultConstants.Keys.Keycloak.ClientSecret)
                                   .GetAwaiter().GetResult();
    }
}
