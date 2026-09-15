using Refit;

namespace Faber.Modules.Identity.Application.Keycloak.Contracts;

public class CredentialRequest
{
    [AliasAs("type")]
    public string Type { get; set; } = "password";

    [AliasAs("temporary")]
    public bool Temporary { get; set; } = false;

    [AliasAs("value")]
    public required string Value { get; set; }
}