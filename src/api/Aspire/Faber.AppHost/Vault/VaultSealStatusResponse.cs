using System.Text.Json.Serialization;

namespace Faber.AppHost.Vault;

public record VaultSealStatusResponse(
    [property: JsonPropertyName("sealed")] bool Sealed,
    [property: JsonPropertyName("t")] int Threshold,
    [property: JsonPropertyName("n")] int TotalShares,
    [property: JsonPropertyName("progress")] int Progress);