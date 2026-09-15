namespace Faber.Modules.Vault.PublicApi;

public static class VaultConstants
{
    public const string DefaultMountPoint = "secrets";

    public static class Paths
    {
        public const string Database = "database";
        public const string Keycloak = "keycloak";
        public const string Mail = "mail";
    }

    public static class Keys
    {
        public static class Database
        {
            public const string Host = "host";
            public const string Port = "port";
            public const string Name = "name";
            public const string Username = "username";
            public const string Password = "password";
        }

        public static class Keycloak
        {
            public const string ClientId = "client-id";
            public const string ClientSecret = "client-secret";
        }

        public static class Mail
        {
            public const string ResendApiKey = "resend-api-key";
        }
    }
}