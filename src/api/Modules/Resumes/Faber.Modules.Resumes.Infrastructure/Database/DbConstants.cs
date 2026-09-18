namespace Faber.Modules.Resumes.Infrastructure.Database;

public static class DbConstants
{
    public const string SchemaName = "resumes";
    public const string MigrationsHistoryTableName = "migrations_history";

    public const int DescriptionMaxLength = 1000;
    public const int SummaryMaxLength = 2000;
    public const int HobbiesMaxLength = 1000;
    public const int OneLineStringMaxLength = 100;
    public const int UriMaxLength = 2083;
    public const string DefaultLocalization = "en-us";
}
