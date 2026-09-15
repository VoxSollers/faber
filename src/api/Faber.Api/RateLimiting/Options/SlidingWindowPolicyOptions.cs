namespace Faber.Api.RateLimiting.Options;

/// <summary>Tunable settings for a sliding-window rate limiting policy.</summary>
public class SlidingWindowPolicyOptions
{
    /// <summary>Maximum number of requests permitted per window.</summary>
    public int PermitLimit { get; set; }

    /// <summary>Window length in seconds.</summary>
    public int WindowSeconds { get; set; }

    /// <summary>Number of segments the window is divided into.</summary>
    public int SegmentsPerWindow { get; set; } = 6;
}
