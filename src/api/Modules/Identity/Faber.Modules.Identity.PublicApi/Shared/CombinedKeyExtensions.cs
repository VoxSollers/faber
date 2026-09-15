namespace Faber.Modules.Identity.PublicApi.Shared;

public static class CombinedKeyExtensions
{
    private const int UlidLength = 26;

    public static VerificationKey ToVerificationKey(this CombinedKey combinedKey)
    {
        if (string.IsNullOrWhiteSpace(combinedKey.Value) || combinedKey.Value.Length <= UlidLength)
            throw new ArgumentException("Invalid combined key format. Expected Selector(ULID) + Token.");

        var selector = combinedKey.Value[..UlidLength];
        var token = combinedKey.Value[UlidLength..];

        return new VerificationKey(selector, token);
    }
}