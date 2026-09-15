using System.Text.Json.Serialization;

namespace Faber.Modules.Common.PublicApi.Shared.Responses;

public record AuthErrorResponse(
    [property: JsonPropertyName("error")] string Error,
    [property: JsonPropertyName("error_description")]
    string ErrorDescription,
    [property: JsonPropertyName("type")] string Type = "keycloak");