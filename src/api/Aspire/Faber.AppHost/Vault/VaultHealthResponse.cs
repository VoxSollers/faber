using System.Text.Json.Serialization;

namespace Faber.AppHost.Vault;

public record VaultHealthResponse(
    [property: JsonPropertyName("initialized")] bool Initialized,
    [property: JsonPropertyName("sealed")] bool Sealed,
    [property: JsonPropertyName("standby")] bool Standby);