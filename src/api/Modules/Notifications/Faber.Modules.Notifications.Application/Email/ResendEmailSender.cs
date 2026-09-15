using ErrorOr;
using Faber.Modules.Communication.PublicApi;
using Faber.Modules.Documents.PublicApi;
using Faber.Modules.Notifications.PublicApi;
using Faber.Modules.Notifications.PublicApi.Contracts;
using Faber.Modules.Notifications.PublicApi.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;

namespace Faber.Modules.Notifications.Application.Email;

/// <summary>
/// Sends email via the Resend API, used for the Production environment. Internal because it must only
/// ever be reached through the <see cref="IEmailSender"/> decorator chain built by
/// <see cref="DependencyInjection.AddNotificationsModule"/> — anything injecting this type directly
/// instead of <see cref="IEmailSender"/> would bypass the per-recipient throttle in
/// <see cref="ThrottledEmailSender"/>.
/// </summary>
internal sealed class ResendEmailSender : IEmailSender
{
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly ILogger<ResendEmailSender> _logger;
    private readonly IDocumentsModuleApi _documentsModuleApi;
    private readonly IResend _resend;

    public ResendEmailSender(
        IResend resend,
        IDocumentsModuleApi documentsModuleApi,
        ILogger<ResendEmailSender> logger,
        IOptions<FluentEmailOptions> options)
    {
        _resend = resend;
        _documentsModuleApi = documentsModuleApi;
        _logger = logger;
        _fromEmail = options.Value.FromEmail;
        _fromName = options.Value.FromName;
    }

    /// <inheritdoc />
    public async Task<ErrorOr<Success>> SendAsync<TComponent>(
        EmailSenderRequest request,
        CancellationToken cancellationToken = default)
        where TComponent : IComponent
    {
        var messageBody = await _documentsModuleApi.RenderToHtmlAsync<TComponent>(request.Parameters, cancellationToken);

        var emailMessage = new EmailMessage
        {
            To = request.To,

            From = new EmailAddress
            {
                Email = _fromEmail,
                DisplayName = _fromName
            },

            Subject = request.Subject,
            HtmlBody = messageBody
        };

        try
        {
            var response = await _resend.EmailSendAsync(emailMessage, cancellationToken);

            // ResendClientOptions.ThrowExceptions defaults to true and ResendClientOptionsSetup only
            // sets the API token, so in production a failure arrives through the catch below. This
            // branch is what keeps the ErrorOr contract intact if that option is ever turned off,
            // because the client then reports the very same failure on the response instead.
            if (!response.Success)
            {
                return ToDeliveryFailure(response.Exception);
            }

            return Result.Success;
        }
        catch (ResendException exception)
        {
            // ResendClient rethrows TaskCanceledException untouched and wraps everything else —
            // including a transport-level HttpRequestException — as ResendException. Catching only
            // that type is therefore total over delivery failures while leaving cancellation, which
            // is not a delivery failure, free to propagate.
            return ToDeliveryFailure(exception);
        }
    }

    /// <summary>Logs a transport failure and converts it into the shared delivery-failure error.</summary>
    /// <param name="exception">
    /// The failure Resend reported. Null only in the theoretical case of an unsuccessful response
    /// carrying no exception.
    /// </param>
    /// <returns>An <see cref="Error"/> coded <c>Notifications.DeliveryFailed</c>.</returns>
    private ErrorOr<Success> ToDeliveryFailure(ResendException? exception)
    {
        // The recipient address is not added to the message: this line fires on every bounced send,
        // and the log must not become a list of the inboxes someone was aiming at. What the transport
        // itself chose to say travels with the attached exception.
        _logger.LogWarning(
            exception,
            "Failed to send email via Resend: {ErrorType} {StatusCode}",
            exception?.ErrorType,
            exception?.StatusCode);

        var detail = exception is null
            ? "Resend reported an unsuccessful send."
            : $"{exception.ErrorType}: {exception.Message}";

        return EmailSendErrors.DeliveryFailed(detail);
    }
}
