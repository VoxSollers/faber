using Faber.Modules.Auth.PublicApi.Events;
using Faber.Modules.Communication.PublicApi;
using Faber.Modules.Notifications.PublicApi.Contracts;
using FastEndpoints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Notifications.Application.Features.ForgotPasswordAccepted;

public class ForgotPasswordAcceptedEventHandler(
    IEmailSender emailSender,
    IConfiguration configuration,
    ILogger<ForgotPasswordAcceptedEventHandler> logger)
    : IEventHandler<ForgotPasswordAcceptedEvent>
{
    public async Task HandleAsync(ForgotPasswordAcceptedEvent eventModel, CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, object?>
        {
            { "Host", configuration.GetConnectionString("FaberHost") },
            { "Key", eventModel.CombinedKey.Value }
        };

        var headers = new EmailSenderRequest(eventModel.Email, "Password reset", parameters);
        var result = await emailSender.SendAsync<ForgotPasswordAcceptedTemplate>(headers, cancellationToken);

        if (result.IsError)
        {
            // The event is published fire-and-forget after the HTTP response was already written, so
            // there is no caller left to hand a 429 to — the log is the back-pressure signal here.
            logger.LogWarning(
                "Password reset email not sent: {ErrorCode}",
                result.FirstError.Code);
        }
    }
}
