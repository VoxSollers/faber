using ErrorOr;
using Faber.Modules.Common.PublicApi.RateLimiting;
using Faber.Modules.Communication.PublicApi;
using Faber.Modules.Notifications.Application.Email.RateLimiting;
using Faber.Modules.Notifications.PublicApi;
using Faber.Modules.Notifications.PublicApi.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Notifications.Application.Email;

/// <summary>
/// Enforces the per-recipient outbound budget in front of whichever transport the environment selected
/// (FluentEmail in development, Resend in production). It is a decorator rather than a check inside
/// each sender for two reasons: the guard exists once instead of per transport, and a sender added
/// later cannot silently ship without it.
///
/// This is the last line of defence, below every Auth endpoint limit. The per-IP policies and the
/// per-target-email bucket on <c>ForgotPassword</c> all guard an *inbound* request, so a rotated IP
/// pool or a second flow (sign-up verification) can still converge on one inbox; this guards the
/// *outbound* send, where those paths finally meet.
/// </summary>
/// <param name="inner">The transport-backed sender being wrapped.</param>
/// <param name="rateLimiter">The per-recipient token bucket.</param>
/// <param name="logger">Sink for the throttle warning.</param>
public sealed class ThrottledEmailSender(
    IEmailSender inner,
    IRecipientEmailRateLimiter rateLimiter,
    ILogger<ThrottledEmailSender> logger) : IEmailSender
{
    /// <summary>
    /// Value of the <c>policy</c> tag on the shared rejection counter. Deliberately distinct from
    /// the HTTP policy names in <c>RateLimitPolicies</c>, so a dashboard can tell an inbound
    /// request refusal apart from an outbound message that was never handed to the transport.
    /// </summary>
    public const string MetricPolicy = "outbound-email";

    /// <inheritdoc />
    public async Task<ErrorOr<Success>> SendAsync<TComponent>(
        EmailSenderRequest request,
        CancellationToken cancellationToken = default)
        where TComponent : IComponent
    {
        if (!rateLimiter.TryAcquire(request.To))
        {
            RateLimitingMetrics.RecordRejection(MetricPolicy);

            // The recipient address is omitted on purpose: this line fires exactly when someone is
            // aiming volume at an inbox, and it must not turn the log into a list of targets.
            logger.LogWarning(
                "Outbound email throttled: per-recipient limit exceeded, retry after {RetryAfterSeconds}s",
                rateLimiter.RetryAfterSeconds);

            return EmailSendErrors.RecipientThrottled;
        }

        return await inner.SendAsync<TComponent>(request, cancellationToken);
    }
}
