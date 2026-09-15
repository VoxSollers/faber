using Refit;

namespace Faber.Modules.Identity.Application.Keycloak.Contracts;

public class UserEmailVerifyRequest
{
    [AliasAs("emailVerified")]
    public bool EmailVerified { get; set; }

    [AliasAs("requiredActions")]
    public List<string> RequiredActions { get; set; } = [];
}