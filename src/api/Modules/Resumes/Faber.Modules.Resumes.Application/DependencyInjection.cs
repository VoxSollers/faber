using Faber.Modules.Resumes.Application.Authorization;
using Faber.Modules.Resumes.Application.Caching;
using Faber.Modules.Resumes.Application.Options;
using Faber.Modules.Resumes.Infrastructure.Database;
using Faber.Modules.Resumes.PublicApi;
using Faber.Modules.Vault.PublicApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Faber.Modules.Resumes.Application;

public static class DependencyInjection
{
    public static async Task AddResumesModuleAsync(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IResumesModuleApi, ResumesModuleApi>();

        services.AddScoped<ResumesCacheInvalidationInterceptor>();

        services.ConfigureOptions<ResumeLimitsOptionsSetup>();
        services.AddScoped<IAuthorizationHandler, MaxResumeCreationHandler>();
        services.AddScoped<IAuthorizationHandler, ResumeOwnershipHandler>();

        services
            .AddAuthorizationBuilder()
            .AddPolicy(
                "MaxResumeCreationPolicy",
                p =>
                    p.Requirements.Add(new MaxResumeCreationRequirement()))
            .AddPolicy(
                "ResumeOwnerPolicy",
                p =>
                    p.Requirements.Add(new ResumeOwnershipRequirement()));

        var connectionString = configuration.GetConnectionString("faberdb");

        if (string.IsNullOrEmpty(connectionString))
        {
            var vaultModuleApi = services.BuildServiceProvider().GetRequiredService<IVaultModuleApi>();

            var host = await vaultModuleApi.GetSecretValueAsync(
                VaultConstants.Paths.Database,
                VaultConstants.DefaultMountPoint,
                VaultConstants.Keys.Database.Host);

            var port = await vaultModuleApi.GetSecretValueAsync(
                VaultConstants.Paths.Database,
                VaultConstants.DefaultMountPoint,
                VaultConstants.Keys.Database.Port);

            var name = await vaultModuleApi.GetSecretValueAsync(
                VaultConstants.Paths.Database,
                VaultConstants.DefaultMountPoint,
                VaultConstants.Keys.Database.Name);

            var username = await vaultModuleApi.GetSecretValueAsync(
                VaultConstants.Paths.Database,
                VaultConstants.DefaultMountPoint,
                VaultConstants.Keys.Database.Username);

            var password = await vaultModuleApi.GetSecretValueAsync(
                VaultConstants.Paths.Database,
                VaultConstants.DefaultMountPoint,
                VaultConstants.Keys.Database.Password);

            connectionString = $"Host={host};Port={port};Database={name};Username={username};Password={password}";
        }

        services.AddDbContext<ResumesDbContext>((sp, o) => o
            .UseNpgsql(
                connectionString,
                npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable(
                        DbConstants.MigrationsHistoryTableName,
                        DbConstants.SchemaName))
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(sp.GetRequiredService<ResumesCacheInvalidationInterceptor>())
        );
    }
}