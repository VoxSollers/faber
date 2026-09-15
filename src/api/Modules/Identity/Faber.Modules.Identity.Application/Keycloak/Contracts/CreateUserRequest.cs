using Refit;

namespace Faber.Modules.Identity.Application.Keycloak.Contracts;

public class CreateUserRequest
{
    [AliasAs("username")]
    public string? Username { get; set; }

    [AliasAs("firstName")]
    public string? FirstName { get; set; }

    [AliasAs("lastName")]
    public string? LastName { get; set; }

    [AliasAs("email")]
    public string? Email { get; set; }

    [AliasAs("enabled")]
    public bool Enabled { get; set; }

    [AliasAs("emailVerified")]
    public bool? EmailVerified { get; set; }

    [AliasAs("realmRoles")]
    public List<string>? RealmRoles { get; set; }
}