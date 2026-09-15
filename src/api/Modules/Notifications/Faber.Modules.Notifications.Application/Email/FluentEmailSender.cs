using ErrorOr;
using Faber.Modules.Communication.PublicApi;
using Faber.Modules.Documents.PublicApi;
using Faber.Modules.Notifications.PublicApi;
using Faber.Modules.Notifications.PublicApi.Contracts;
using FluentEmail.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Notifications.Application.Email;

/// <summary>
/// Sends email via SMTP through FluentEmail, used for the Development environment. Internal because it
/// must only ever be reached through the <see cref="IEmailSender"/> decorator chain built by
/// <see cref="DependencyInjection.AddNotificationsModule"/> — anything injecting this type directly
/// instead of <see cref="IEmailSender"/> would bypass the per-recipient throttle in
/// <see cref="ThrottledEmailSender"/>.
/// </summary>
internal sealed class FluentEmailSender : IEmailSender
{
    private readonly IFluentEmail _fluentEmail;
    private readonly ILogger<FluentEmailSender> _logger;
    private readonly IDocumentsModuleApi _documentsModuleApi;

    public FluentEmailSender(
        IFluentEmail fluentEmail,
        ILogger<FluentEmailSender> logger,
        IDocumentsModuleApi documentsModuleApi)
    {
        _fluentEmail = fluentEmail;
        _logger = logger;
        _documentsModuleApi = documentsModuleApi;
    }

    public async Task<ErrorOr<Success>> SendAsync<TComponent>(
        EmailSenderRequest request,
        CancellationToken cancellationToken = default)
        where TComponent : IComponent
    {
        var messageBody = await _documentsModuleApi.RenderToHtmlAsync<TComponent>(request.Parameters, cancellationToken);

        var email = _fluentEmail
            .To(request.To)
            .Subject(request.Subject)
            .Body(messageBody, request.IsHtml);

        var result = await email.SendAsync();

        if (!result.Successful)
        {
            var errors = string.Join(", ", result.ErrorMessages);
            _logger.LogWarning("Failed to send email: {Errors}", errors);

            return EmailSendErrors.DeliveryFailed(errors);
        }

        return Result.Success;
    }
}