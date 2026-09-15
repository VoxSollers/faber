using System.Text.Json.Serialization;

namespace Faber.Modules.Identity.Application.Keycloak.Contracts;

public class UserResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }

    [JsonPropertyName("emailVerified")]
    public bool? EmailVerified { get; set; }

    [JsonPropertyName("createdTimestamp")]
    public long? CreatedTimestamp { get; set; }

    [JsonPropertyName("credentials")]
    public List<CredentialRequest>? Credentials { get; set; }

    [JsonPropertyName("totp")]
    public bool? Totp { get; set; }

    [JsonPropertyName("disableableCredentialTypes")]
    public List<string>? DisableableCredentialTypes { get; set; }

    [JsonPropertyName("requiredActions")]
    public List<string>? RequiredActions { get; set; }

    [JsonPropertyName("notBefore")]
    public int? NotBefore { get; set; }

    [JsonPropertyName("realmRoles")]
    public List<string>? RealmRoles { get; set; }

    [JsonPropertyName("clientRoles")]
    public Dictionary<string, List<string>>? ClientRoles { get; set; }

    [JsonPropertyName("access")]
    public Dictionary<string, bool>? Access { get; set; }

    [JsonPropertyName("attributes")]
    public Dictionary<string, List<string>>? Attributes { get; set; }

    [JsonPropertyName("federatedIdentities")]
    public List<FederatedIdentityRepresentation>? FederatedIdentities { get; set; }

    [JsonPropertyName("federationLink")]
    public string? FederationLink { get; set; }

    [JsonPropertyName("groups")]
    public List<string>? Groups { get; set; }

    [JsonPropertyName("clientConsents")]
    public List<UserConsentRepresentation>? ClientConsents { get; set; }

    [JsonPropertyName("serviceAccountClientId")]
    public string? ServiceAccountClientId { get; set; }
}

public class FederatedIdentityRepresentation
{
    [JsonPropertyName("identityProvider")]
    public string? IdentityProvider { get; set; }

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("userName")]
    public string? UserName { get; set; }
}

public class UserConsentRepresentation
{
    [JsonPropertyName("clientId")]
    public string? ClientId { get; set; }

    [JsonPropertyName("grantedClientScopes")]
    public List<string>? GrantedClientScopes { get; set; }

    [JsonPropertyName("createdDate")]
    public long? CreatedDate { get; set; }

    [JsonPropertyName("lastUpdatedDate")]
    public long? LastUpdatedDate { get; set; }
}