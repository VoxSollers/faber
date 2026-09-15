using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using ErrorOr;
using Faber.Modules.Communication.PublicApi;
using Faber.Modules.Common.PublicApi.RateLimiting;
using Faber.Modules.Notifications.Application.Email;
using Faber.Modules.Notifications.Application.Email.RateLimiting;
using Faber.Modules.Notifications.PublicApi;
using Faber.Modules.Notifications.PublicApi.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;

namespace Faber.Modules.Notifications.Application.UnitTests.Email;

public class ThrottledEmailSenderTests
{
    private const string Victim = "victim-inbox@example.com";
    private const string Bystander = "bystander@example.com";

    /// <summary>Minimal Razor component stand-in; the decorator never renders it, it only forwards the type.</summary>
    private sealed class TestTemplate : IComponent
    {
        public void Attach(RenderHandle renderHandle)
        {
        }

        public Task SetParametersAsync(ParameterView parameters) => Task.CompletedTask;
    }

    private static IEmailSender CreateInner()
    {
        var inner = Substitute.For<IEmailSender>();

        inner.SendAsync<TestTemplate>(Arg.Any<EmailSenderRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<Success>>(Result.Success));

        return inner;
    }

    private static RecipientEmailRateLimiter CreateLimiter(int tokenLimit, bool enabled = true)
    {
        return new RecipientEmailRateLimiter(Options.Create(new RecipientEmailRateLimitingOptions
        {
            Enabled = enabled,
            TokenLimit = tokenLimit,
            TokensPerPeriod = tokenLimit,
            ReplenishmentPeriodSeconds = 3600
        }));
    }

    private static Task<ErrorOr<Success>> Send(ThrottledEmailSender sender, string recipient)
    {
        var request = new EmailSenderRequest(recipient, "Password reset", new Dictionary<string, object?>());

        return sender.SendAsync<TestTemplate>(request, CancellationToken.None);
    }

    [Fact]
    public async Task WithinRecipientBudget_ShouldDelegateToInnerSender()
    {
        var inner = CreateInner();
        using var limiter = CreateLimiter(tokenLimit: 3);
        var sender = new ThrottledEmailSender(inner, limiter, NullLogger<ThrottledEmailSender>.Instance);

        var result = await Send(sender, Victim);

        result.IsError.ShouldBeFalse();

        await inner.Received(1)
            .SendAsync<TestTemplate>(Arg.Any<EmailSenderRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EmailBombAgainstOneRecipient_ShouldStopReachingTheTransport()
    {
        // The scenario from issue #339: a burst aimed at a single inbox. Once the budget is spent the
        // decorator must stop calling the transport at all — dropping the message is the whole point,
        // returning an error while still sending would leave us a spam amplifier.
        var inner = CreateInner();
        using var limiter = CreateLimiter(tokenLimit: 3);
        var sender = new ThrottledEmailSender(inner, limiter, NullLogger<ThrottledEmailSender>.Instance);

        for (var i = 0; i < 3; i++)
        {
            (await Send(sender, Victim)).IsError.ShouldBeFalse();
        }

        for (var i = 0; i < 20; i++)
        {
            var throttled = await Send(sender, Victim);

            throttled.IsError.ShouldBeTrue();
            throttled.FirstError.Code.ShouldBe(EmailSendErrors.RecipientThrottled.Code);
        }

        await inner.Received(3)
            .SendAsync<TestTemplate>(Arg.Any<EmailSenderRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExhaustedRecipient_ShouldNotBlockAnotherRecipient()
    {
        var inner = CreateInner();
        using var limiter = CreateLimiter(tokenLimit: 1);
        var sender = new ThrottledEmailSender(inner, limiter, NullLogger<ThrottledEmailSender>.Instance);

        (await Send(sender, Victim)).IsError.ShouldBeFalse();
        (await Send(sender, Victim)).IsError.ShouldBeTrue();

        (await Send(sender, Bystander)).IsError.ShouldBeFalse();
    }

    [Fact]
    public async Task CaseAndWhitespaceVariantsOfOneRecipient_ShouldShareTheBudget()
    {
        // Guards the normalization contract end-to-end: padding or re-casing the address must not
        // mint a fresh budget once the decorator is the thing calling the limiter.
        var inner = CreateInner();
        using var limiter = CreateLimiter(tokenLimit: 1);
        var sender = new ThrottledEmailSender(inner, limiter, NullLogger<ThrottledEmailSender>.Instance);

        (await Send(sender, Victim)).IsError.ShouldBeFalse();

        (await Send(sender, " VICTIM-INBOX@EXAMPLE.COM ")).IsError.ShouldBeTrue();
    }

    [Fact]
    public async Task DisabledLimiter_ShouldNeverThrottle()
    {
        // Mirrors the #330 test-environment relaxation: with RateLimiting:Enabled=false the decorator
        // must be transparent, so existing integration suites keep sending freely.
        var inner = CreateInner();
        using var limiter = CreateLimiter(tokenLimit: 1, enabled: false);
        var sender = new ThrottledEmailSender(inner, limiter, NullLogger<ThrottledEmailSender>.Instance);

        for (var i = 0; i < 50; i++)
        {
            (await Send(sender, Victim)).IsError.ShouldBeFalse();
        }

        await inner.Received(50)
            .SendAsync<TestTemplate>(Arg.Any<EmailSenderRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InnerSenderFailure_ShouldPropagateUnchanged()
    {
        var inner = Substitute.For<IEmailSender>();

        inner.SendAsync<TestTemplate>(Arg.Any<EmailSenderRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<Success>>(EmailSendErrors.DeliveryFailed("smtp down")));

        using var limiter = CreateLimiter(tokenLimit: 3);
        var sender = new ThrottledEmailSender(inner, limiter, NullLogger<ThrottledEmailSender>.Instance);

        var result = await Send(sender, Victim);

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Notifications.DeliveryFailed");
    }

    [Fact]
    public async Task ThrottledSend_ShouldRecordRejectionMetric()
    {
        // Issue #416: the outbound bucket is the second in-process limiter (the first is Auth's
        // per-target-email bucket, covered by ForgotPasswordRateLimitingTests). Its rejection branch
        // calls RateLimitingMetrics.RecordRejection, but nothing behavioural proved that — an edit
        // that moved the call onto the success branch, or changed the tag, would have shipped green.
        var inner = CreateInner();
        using var limiter = CreateLimiter(tokenLimit: 1);
        var sender = new ThrottledEmailSender(inner, limiter, NullLogger<ThrottledEmailSender>.Instance);

        var recordedPolicies = new ConcurrentBag<string>();

        using var listener = new MeterListener();

        listener.InstrumentPublished = (instrument, meterListener) =>
        {
            if (instrument.Meter.Name == RateLimitingMetrics.MeterName
                && instrument.Name == RateLimitingMetrics.RejectionsCounterName)
            {
                meterListener.EnableMeasurementEvents(instrument);
            }
        };

        listener.SetMeasurementEventCallback<long>((_, _, tags, _) =>
        {
            foreach (var tag in tags)
            {
                if (tag is { Key: "policy", Value: string policy })
                {
                    recordedPolicies.Add(policy);
                }
            }
        });

        listener.Start();

        (await Send(sender, Victim)).IsError.ShouldBeFalse();
        (await Send(sender, Victim)).IsError.ShouldBeTrue();

        // Exactly one send was throttled while the listener was running. ThrottledEmailSender is the
        // only type in this assembly that records on the shared counter, and xUnit runs the methods of
        // a single class sequentially, so an exact count also catches double-recording.
        recordedPolicies.Count(policy => policy == ThrottledEmailSender.MetricPolicy).ShouldBe(1);
    }
}
