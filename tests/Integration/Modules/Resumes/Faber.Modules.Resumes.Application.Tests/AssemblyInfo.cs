using FastEndpoints.Testing;
using Xunit.Sdk;
using Xunit.v3;

[assembly: EnableAdvancedTesting]

// WebApp.PreSetupAsync sets VAULT_ADDR/VAULT_TOKEN via Environment.SetEnvironmentVariable, which is
// process-wide. Vault.Application.DependencyInjection reads those env vars (not IConfiguration) when
// each fixture's host is built. With more than one collection (WebApp, RedisUnavailableWebApp) xUnit v3
// runs collections in parallel by default, so a second fixture's PreSetupAsync could overwrite the env
// vars while the first fixture's host is still being built, pointing it at a Vault that isn't seeded
// yet. The assembly only ever runs one collection's fixture setup meaningfully in parallel today, so
// disabling test parallelization here costs nothing and removes the race for any future collection too.
[assembly: Parallelization(Mode = ParallelMode.None)]