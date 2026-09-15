namespace Faber.Modules.Identity.Infrastructure.Database;

public static class DbConstants
{
    public const string IdentitySchemaName = "identity";
    public const string MigrationsHistoryTableName = "migrations_history";
    public const int TokenTypeMaxLength = 15;
    public const int UlidMaxLength = 26;
}