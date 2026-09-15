namespace Faber.Migrations;

/// <summary>
/// Configuration for <see cref="MigrationWorker"/>, bound from the <c>Migrations</c> section.
/// </summary>
public sealed class MigrationOptions
{
    public const string SectionName = "Migrations";

    /// <summary>
    /// Schemas owned by something other than EF Core — for example Keycloak's own Liquibase
    /// migrations — that must exist before their owner starts. The worker only ensures each one
    /// exists; it never migrates its contents. Empty unless the host declares them, so an
    /// environment that does not configure this section gets no extra schemas.
    /// </summary>
    public string[] ExternalSchemas { get; init; } = [];
}
