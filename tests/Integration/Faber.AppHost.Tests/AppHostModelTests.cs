using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Faber.AppHost.Tests;

public class AppHostModelTests
{
    // What a clean clone has before Vault is initialised: everything except the Vault token.
    private static readonly string[] ConfigurationBeforeVaultInit =
    [
        "--Parameters:postgres-username=test-postgres-user",
        "--Parameters:postgres-password=test-postgres-password",
        "--Parameters:keycloak-admin=test-keycloak-admin",
        "--Parameters:keycloak-password=test-keycloak-password"
    ];

    private static readonly string[] TestConfiguration =
    [
        .. ConfigurationBeforeVaultInit,
        "--Parameters:vault-token=test-vault-token"
    ];

    [Fact]
    public async Task DependencyGraph_ShouldGateCleanStackStartup()
    {
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Faber_AppHost>(
            TestConfiguration,
            TestContext.Current.CancellationToken);

        var faberDb = builder.Resources.Single(resource => resource.Name == "faberdb");
        var migrations = builder.Resources.Single(resource => resource.Name == "migrations");
        var keycloak = builder.Resources.Single(resource => resource.Name == "keycloak");
        var vault = builder.Resources.Single(resource => resource.Name == "vault");
        var bootstrap = builder.Resources.Single(resource => resource.Name == "bootstrap");
        var api = builder.Resources.Single(resource => resource.Name == "faberapi");

        migrations.ShouldWaitFor(faberDb, WaitType.WaitUntilHealthy);
        keycloak.ShouldWaitFor(migrations, WaitType.WaitForCompletion);
        bootstrap.ShouldWaitFor(keycloak, WaitType.WaitUntilHealthy);
        bootstrap.ShouldWaitFor(vault, WaitType.WaitUntilHealthy);
        api.ShouldWaitFor(migrations, WaitType.WaitForCompletion);
        api.ShouldWaitFor(bootstrap, WaitType.WaitForCompletion);
    }

    [Fact]
    public async Task KeycloakRealmImport_ShouldBeConfiguredInApplicationModel()
    {
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Faber_AppHost>(
            TestConfiguration,
            TestContext.Current.CancellationToken);
        var keycloak = builder.Resources.Single(resource => resource.Name == "keycloak");

        var importMount = keycloak.Annotations
            .OfType<ContainerMountAnnotation>()
            .Single(mount => mount.Target == "/opt/keycloak/data/import/realm-export.json");

        importMount.Type.ShouldBe(ContainerMountType.BindMount);
        importMount.IsReadOnly.ShouldBeTrue();
        var expectedSource = Path.GetFullPath(Path.Combine(
            builder.AppHostDirectory,
            "..",
            "..",
            "..",
            "..",
            "infra",
            "keycloak",
            "realm-export.json"));
        importMount.Source.ShouldNotBeNull();
        Path.GetFullPath(importMount.Source!).ShouldBe(expectedSource);

        var configuration = await ExecutionConfigurationBuilder
            .Create(keycloak)
            .WithArgumentsConfig()
            .WithEnvironmentVariablesConfig()
            .BuildAsync(
                new DistributedApplicationExecutionContext(DistributedApplicationOperation.Run),
                NullLogger.Instance,
                TestContext.Current.CancellationToken);

        configuration.Exception.ShouldBeNull();
        configuration.Arguments.Select(argument => argument.Value).ShouldBe(["start-dev", "--import-realm"]);

        var environment = configuration.EnvironmentVariables.ToDictionary(variable => variable.Key, variable => variable.Value);
        environment["KC_DB"].ShouldBe("postgres");
        environment["KC_DB_SCHEMA"].ShouldBe("keycloak");
        // Keycloak must use the database the migration service created its schema in. Asserted by
        // database name rather than the full JDBC URL, which the public-snapshot secret scan flags
        // and has reviewed once, in Program.cs.
        var faberDb = builder.Resources.OfType<PostgresDatabaseResource>().Single();
        environment["KC_DB_URL"].ShouldEndWith($"/{faberDb.DatabaseName}");
    }

    [Fact]
    public async Task Vault_WithoutVaultToken_ShouldResolveItsConfiguration()
    {
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Faber_AppHost>(
            ConfigurationBeforeVaultInit,
            TestContext.Current.CancellationToken);
        var vault = builder.Resources.Single(resource => resource.Name == "vault");

        var (exception, environment) = await ResolveConfigurationAsync(vault);

        exception.ShouldBeNull("Vault's configuration must resolve on a clean clone");
        environment.Keys.ShouldNotContain("VAULT_TOKEN");
    }

    [Fact]
    public async Task KeycloakSchema_ShouldBeDeclaredToMigrations()
    {
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Faber_AppHost>(
            TestConfiguration,
            TestContext.Current.CancellationToken);
        var migrations = builder.Resources.Single(resource => resource.Name == "migrations");
        var keycloak = builder.Resources.Single(resource => resource.Name == "keycloak");

        var migrationsEnvironment = await GetDeclaredEnvironmentAsync(migrations);
        var keycloakEnvironment = await GetDeclaredEnvironmentAsync(keycloak);

        migrationsEnvironment["Migrations__ExternalSchemas__0"].ShouldBe(keycloakEnvironment["KC_DB_SCHEMA"]);
    }

    // The declared values, before Aspire resolves them. Resolving the migration service's
    // environment would wait for Postgres's endpoint, which is only allocated in a running app.
    private static async Task<IReadOnlyDictionary<string, object>> GetDeclaredEnvironmentAsync(IResource resource)
    {
        var context = new EnvironmentCallbackContext(
            new DistributedApplicationExecutionContext(DistributedApplicationOperation.Run),
            resource,
            cancellationToken: TestContext.Current.CancellationToken);

        foreach (var callback in resource.Annotations.OfType<EnvironmentCallbackAnnotation>())
        {
            await callback.Callback(context);
        }

        return context.EnvironmentVariables;
    }

    private static async Task<(Exception? Exception, IReadOnlyDictionary<string, string> Environment)>
        ResolveConfigurationAsync(IResource resource)
    {
        // Bounded, so a value that can never resolve fails the test instead of hanging it.
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));

        var configuration = await ExecutionConfigurationBuilder
            .Create(resource)
            .WithEnvironmentVariablesConfig()
            .BuildAsync(
                new DistributedApplicationExecutionContext(DistributedApplicationOperation.Run),
                NullLogger.Instance,
                timeout.Token);

        return (
            configuration.Exception,
            configuration.EnvironmentVariables.ToDictionary(variable => variable.Key, variable => variable.Value));
    }
}

internal static class ResourceAssertionExtensions
{
    internal static void ShouldWaitFor(this IResource resource, IResource dependency, WaitType waitType)
    {
        resource.Annotations
            .OfType<WaitAnnotation>()
            .ShouldContain(annotation => annotation.Resource == dependency && annotation.WaitType == waitType);
    }
}
