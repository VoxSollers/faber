using System.Security.Cryptography;
using System.Text;

namespace Faber.AppHost;

public static class PostgresVolumeName
{
    public static string FromCheckoutPath(string checkoutPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(checkoutPath);

        var canonicalPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(checkoutPath));
        var pathHash = SHA256.HashData(Encoding.UTF8.GetBytes(canonicalPath));
        var suffix = Convert.ToHexString(pathHash.AsSpan(0, 8)).ToLowerInvariant();

        return $"faber-postgres-data-{suffix}";
    }
}
