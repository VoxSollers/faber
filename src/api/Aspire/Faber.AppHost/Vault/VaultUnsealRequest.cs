using System.Text.Json.Serialization;

namespace Faber.AppHost.Vault;

public record VaultUnsealRequest(
    [property: JsonPropertyName("key")] string Key);