using Faber.Modules.Vault.PublicApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;

namespace Faber.Modules.Vault.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddVaultModule(
        this IServiceCollection services,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        services.Configure<VaultModuleOptions>(
            configuration.GetSection(VaultModuleOptions.SectionName));

        services.AddSingleton<IVaultClient>(_ =>
        {
            var token = Environment.GetEnvironmentVariable("VAULT_TOKEN");
            var addr = Environment.GetEnvironmentVariable("VAULT_ADDR");
            var authMethod = new TokenAuthMethodInfo(token);

            var vaultClientSettings = new VaultClientSettings(addr, authMethod)
            {
                MyHttpClientProviderFunc = handler =>
                {
                    if (environment.IsDevelopment() && handler is HttpClientHandler httpClientHandler)
                        httpClientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;

                    return new HttpClient(handler);
                }
            };

            return new VaultClient(vaultClientSettings);
        });

        services.AddSingleton<IVaultModuleApi, VaultModuleApi>();

        return services;
    }
}
