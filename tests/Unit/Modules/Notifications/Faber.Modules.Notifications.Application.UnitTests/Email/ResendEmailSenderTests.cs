using System.Net;
using ErrorOr;
using Faber.Modules.Documents.PublicApi;
using Faber.Modules.Notifications.Application.Email;
using Faber.Modules.Notifications.PublicApi.Contracts;
using Faber.Modules.Notifications.PublicApi.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Resend;
using Shouldly;

namespace Faber.Modules.Notifications.Application.UnitTests.Email;

public class ResendEmailSenderTests
{
    private const string Recipient = "recipient@example.com";
    private const string DeliveryFailedCode = "Notifications.DeliveryFailed";

    /// <summary>Minimal Razor component stand-in; rendering is stubbed, only the type is forwarded.</summary>
    private sealed class TestTemplate : IComponent
    {
        public void Attach(RenderHandle renderHandle)
        {
        }

        public Task SetParametersAsync(ParameterView parameters) => Task.CompletedTask;
    }

    private static ResendEmailSender CreateSender(IResend resend)
    {
        var documentsModuleApi = Substitute.For<IDocumentsModuleApi>();

        documentsModuleApi
            .RenderToHtmlAsync<TestTemplate>(Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns("<p>body</p>");

        var options = Options.Create(new FluentEmailOptions
        {
            FromEmail = "no-reply@faber.test",
            FromName = "Faber"
        });

        return new ResendEmailSender(resend, documentsModuleApi, NullLogger<ResendEmailSender>.Instance, options);
    }

    private static Task<ErrorOr<Success>> Send(ResendEmailSender sender)
    {
        var request = new EmailSenderRequest(Recipient, "Password reset", new Dictionary<string, object?>());

        return sender.SendAsync<TestTemplate>(request, CancellationToken.None);
    }

    [Fact]
    public async Task ThrownResendFailure_ShouldMapToDeliveryFailed()
    {
        // The #399 defect. ResendClientOptions.ThrowExceptions defaults to true and nothing overrides
        // it, so in production an API rejection arrives as an exception. Uncaught, it escapes past the
        // `result.IsError` check in the fire-and-forget event handlers, and the lost email produces no
        // log line at all — the failure mode ThrottledEmailSender's ErrorOr contract promises against.
        var resend = Substitute.For<IResend>();

        resend
            .EmailSendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ResendException(
                HttpStatusCode.TooManyRequests,
                Resend.ErrorType.RateLimitExceeded,
                "Too many requests."));

        var result = await Send(CreateSender(resend));

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(DeliveryFailedCode);
    }

    [Fact]
    public async Task TransportLevelResendFailure_ShouldMapToDeliveryFailed()
    {
        // ResendClient wraps a dead socket or DNS failure as ResendException(ErrorType.HttpSendFailed)
        // rather than letting HttpRequestException through, so catching the wrapper is what makes the
        // network path — not just API rejections — observable.
        var resend = Substitute.For<IResend>();

        resend
            .EmailSendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ResendException(
                null,
                Resend.ErrorType.HttpSendFailed,
                "Connection refused.",
                new HttpRequestException("Connection refused.")));

        var result = await Send(CreateSender(resend));

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(DeliveryFailedCode);
    }

    [Fact]
    public async Task UnsuccessfulResendResponse_ShouldMapToDeliveryFailed()
    {
        // The same failure as above with ResendClientOptions.ThrowExceptions turned off: the client
        // reports it on the response instead of throwing. The sender must not read Success as implied,
        // otherwise flipping one option silently reopens the bug this issue closes.
        var resend = Substitute.For<IResend>();

        var failure = new ResendException(
            HttpStatusCode.BadRequest,
            Resend.ErrorType.ValidationError,
            "Invalid `to` field.");

        resend
            .EmailSendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(new ResendResponse<Guid>(failure, null));

        var result = await Send(CreateSender(resend));

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(DeliveryFailedCode);
    }

    [Fact]
    public async Task AcceptedSend_ShouldReturnSuccess()
    {
        var resend = Substitute.For<IResend>();

        resend
            .EmailSendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(new ResendResponse<Guid>(Guid.NewGuid(), null));

        var result = await Send(CreateSender(resend));

        result.IsError.ShouldBeFalse();

        await resend.Received(1).EmailSendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelledSend_ShouldPropagateCancellation()
    {
        // ResendClient rethrows TaskCanceledException untouched instead of wrapping it, and a caller
        // walking away is not a delivery failure. Widening the catch to Exception would report a
        // rejected message here and hide shutdown-time cancellation behind a warning.
        var resend = Substitute.For<IResend>();

        resend
            .EmailSendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException());

        await Should.ThrowAsync<TaskCanceledException>(() => Send(CreateSender(resend)));
    }
}
