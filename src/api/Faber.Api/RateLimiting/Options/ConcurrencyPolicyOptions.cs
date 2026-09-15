namespace Faber.Api.RateLimiting.Options;

/// <summary>Tunable settings for a concurrency rate limiting policy.</summary>
public class ConcurrencyPolicyOptions
{
    /// <summary>Maximum number of concurrent requests permitted.</summary>
    public int PermitLimit { get; set; }

    /// <summary>Maximum number of requests queued when the permit limit is reached.</summary>
    public int QueueLimit { get; set; }
}
