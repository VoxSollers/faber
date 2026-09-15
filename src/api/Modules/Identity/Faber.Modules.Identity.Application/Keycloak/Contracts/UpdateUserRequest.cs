using Refit;

namespace Faber.Modules.Identity.Application.Keycloak.Contracts;

public class UpdateUserRequest
{
    [AliasAs("firstName")]
    public string? FirstName { get; set; }

    [AliasAs("lastName")]
    public string? LastName { get; set; }
}