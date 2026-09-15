using ErrorOr;

namespace Faber.Modules.Notifications.PublicApi;

/// <summary>
/// Failure modes of an outbound email send. Defined once, in PublicApi, because the throttle error is
/// produced in the Application layer, consumed by event handlers, and asserted in tests — three places
/// that must agree on the exact code string.
/// </summary>
public static class EmailSendErrors
{
    /// <summary>
    /// The recipient's per-address budget is exhausted, so the message was deliberately not sent. The
    /// description is intentionally generic: it may end up in a log line correlated with a request and
    /// must not restate limit values.
    /// </summary>
    public static Error RecipientThrottled { get; } = Error.Failure(
        "Notifications.RecipientThrottled",
        "Outbound email to this recipient is rate limited.");

    /// <summary>The transport accepted the request but reported a delivery failure.</summary>
    /// <param name="detail">Transport-reported detail, for logging only.</param>
    /// <returns>An <see cref="Error"/> coded <c>Notifications.DeliveryFailed</c>.</returns>
    public static Error DeliveryFailed(string detail) => Error.Failure(
        "Notifications.DeliveryFailed",
        detail);
}
