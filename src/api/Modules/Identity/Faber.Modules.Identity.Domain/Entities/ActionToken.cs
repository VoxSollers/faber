using System.Security.Cryptography;
using System.Text;
using Faber.Modules.Identity.Domain.Enums;
using Konscious.Security.Cryptography;

namespace Faber.Modules.Identity.Domain.Entities;

public class ActionToken
{
    private ActionToken(string email, string hash, string salt, ActionTokenType type)
    {
        Email = email;
        Hash = hash;
        Salt = salt;
        Type = type;
    }

    public Ulid Id { get; private set; }

    public Ulid Selector { get; private set; }

    public string Email { get; private set; }

    public string Hash { get; }

    public string Salt { get; }

    public ActionTokenType Type { get; set; }

    public DateTimeOffset ExpiresAt { get; private init; }

    public DateTimeOffset CreatedAt { get; private init; }

    public DateTimeOffset? ConsumedAt { get; private set; }

    public static ActionToken Create(
        string email,
        string rawToken,
        TimeSpan time,
        ActionTokenType type,
        ReadOnlySpan<char> pepper = default)
    {
        var (hash, salt) = GenerateHashWithSalt(rawToken, pepper);

        return new ActionToken(email, hash, salt, type)
        {
            Id = Ulid.NewUlid(),
            Selector = Ulid.NewUlid(),
            ExpiresAt = DateTimeOffset.UtcNow.Add(time),
            CreatedAt = DateTimeOffset.UtcNow,
            ConsumedAt = null
        };
    }

    public void Consume()
    {
        ConsumedAt = DateTimeOffset.UtcNow;
    }

    public bool Verify(string rawToken, ReadOnlySpan<char> pepper = default)
    {
        if (ExpiresAt < DateTimeOffset.UtcNow || ConsumedAt is not null) return false;

        var computed = DeriveArgon2Id(rawToken, Convert.FromHexString(Salt), pepper);
        var stored = Convert.FromHexString(Hash);

        return CryptographicOperations.FixedTimeEquals(computed, stored);
    }

    private static (string Hash, string Salt) GenerateHashWithSalt(string rawToken, ReadOnlySpan<char> pepper = default)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var derived = DeriveArgon2Id(rawToken, saltBytes, pepper);

        return (Convert.ToHexString(derived), Convert.ToHexString(saltBytes));
    }

    private static byte[] DeriveArgon2Id(string rawToken, byte[] saltBytes, ReadOnlySpan<char> pepper)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(rawToken);

        byte[] effectiveSalt;

        if (!pepper.IsEmpty)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(pepper.ToString()));
            effectiveSalt = hmac.ComputeHash(saltBytes);
        }
        else
        {
            effectiveSalt = saltBytes;
        }

        using var argon2 = new Argon2id(passwordBytes);

        argon2.Salt = effectiveSalt;
        argon2.DegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2);
        argon2.Iterations = 3;
        argon2.MemorySize = 64 * 1024;

        return argon2.GetBytes(32);
    }
}