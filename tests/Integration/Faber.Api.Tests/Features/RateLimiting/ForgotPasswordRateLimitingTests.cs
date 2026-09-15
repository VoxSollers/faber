using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Faber.Modules.Auth.Application.Features.ForgotPassword;
using Faber.Modules.Auth.Application.Features.ForgotPassword.RateLimiting;
using Faber.Modules.Common.PublicApi.RateLimiting;
using Faber.Testing.Shared.Data;
using FastEndpoints.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using static Faber.Api.Tests.Features.RateLimiting.RateLimitingTestConstants;

namespace Faber.Api.Tests.Features.RateLimiting;

[Collection<CollectionRateLimiting>]
[Priority(3)]
public class ForgotPasswordRateLimitingTests(WebApp app) : TestBase
{
    // IP octets 60-77 reserved for this class (RateLimitingTests uses 10-32,
    // SignInRateLimitingTests uses 50-55, ResetPasswordRateLimitingTests uses 80+).

    private HttpClient CreateClientWithIp(string ip)
    {
        var client = app.CreateClient();
        client.DefaultRequestHeaders.Add(TestClientIpStartupFilter.HeaderName, ip);

        return client;
    }

    private static Task<HttpResponseMessage> PostForgotPasswordAsync(HttpClient client, string email)
    {
        return client.PostAsJsonAsync(
            ForgotPasswordUri,
            new ForgotPasswordRequest(email),
            TestContext.Current.CancellationToken);
    }

    private async Task<HttpResponseMessage> ExhaustEmailBucketAsync(string email, int startIpOctet)
    {
        for (var i = 0; i < TargetEmailTokenLimit; i++)
        {
            using var client = CreateClientWithIp($"203.0.113.{startIpOctet + i}");
            var allowed = await PostForgotPasswordAsync(client, email);
            allowed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        }

        using var finalClient = CreateClientWithIp($"203.0.113.{startIpOctet + TargetEmailTokenLimit}");

        return await PostForgotPasswordAsync(finalClient, email);
    }

    private static async Task AssertGenericRejectionAsync(HttpResponseMessage rejected, params string[] mustNotContain)
    {
        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        rejected.Headers.RetryAfter.ShouldNotBeNull();
        rejected.Headers.RetryAfter!.Delta.ShouldNotBeNull();
        rejected.Headers.RetryAfter.Delta!.Value.TotalSeconds.ShouldBeGreaterThan(0);

        // Both rejection paths (the middleware's RateLimitRejectionHandler and the per-email rejection
        // in ForgotPasswordEndpoint) go through the shared RateLimitRejection.Problem — this helper is
        // used by both, so it asserts that parity end to end.
        rejected.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await rejected.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        using var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("title").GetString().ShouldBe(ExpectedTitle);
        json.RootElement.GetProperty("detail").GetString().ShouldBe(ExpectedDetail);
        json.RootElement.GetProperty("status").GetInt32().ShouldBe(429);

        body.ShouldNotContain("PermitLimit");
        body.ShouldNotContain("TokenLimit");
        body.ShouldNotContain("ip:");

        foreach (var value in mustNotContain)
        {
            body.ShouldNotContain(value);
        }
    }

    [Fact]
    [Priority(1)]
    public void TargetEmailRateLimitingOptions_ShouldBindFromConfiguration()
    {
        var options = app.Services
            .GetRequiredService<IOptions<TargetEmailRateLimitingOptions>>().Value;

        options.Enabled.ShouldBeTrue();
        options.TokenLimit.ShouldBe(TargetEmailTokenLimit);
        options.TokensPerPeriod.ShouldBe(TargetEmailTokenLimit);
    }

    [Fact]
    [Priority(2)]
    public async Task ExceedingAuthPasswordResetLimit_ShouldReturn429WithRetryAfterAndGenericBody()
    {
        const string clientIp = "203.0.113.60";
        using var client = CreateClientWithIp(clientIp);

        // Distinct emails per request so the per-email bucket never trips first — isolates the
        // auth-password-reset per-IP policy as the layer that rejects.
        for (var i = 0; i < AuthPasswordResetPermitLimit; i++)
        {
            var allowed = await PostForgotPasswordAsync(client, $"ip-probe-{i}@example.com");
            allowed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        }

        var rejected = await PostForgotPasswordAsync(client, $"ip-probe-{AuthPasswordResetPermitLimit}@example.com");

        await AssertGenericRejectionAsync(rejected, clientIp);

        // Fresh IP is a separate partition, still allowed.
        using var freshIpClient = CreateClientWithIp("203.0.113.61");
        var freshResponse = await PostForgotPasswordAsync(freshIpClient, "ip-probe-fresh@example.com");
        freshResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    [Priority(3)]
    public async Task EmailBombing_ExceedingPerEmailLimit_ShouldReturn429WithRetryAfterAndGenericBody()
    {
        const string bombedEmail = "victim-inbox@example.com";

        // Each request comes from a distinct IP, so neither the global-anonymous nor the
        // auth-password-reset per-IP limiter ever sees more than one hit — only the per-email
        // bucket accumulates across all of them.
        var rejected = await ExhaustEmailBucketAsync(bombedEmail, startIpOctet: 62);

        await AssertGenericRejectionAsync(rejected, bombedEmail);

        // A different email from a fresh IP is a separate partition, still allowed.
        using var freshClient = CreateClientWithIp("203.0.113.65");
        var freshEmailResponse = await PostForgotPasswordAsync(freshClient, "not-the-victim@example.com");
        freshEmailResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    [Priority(4)]
    public async Task ThrottledEmailBombResponses_ShouldNotRevealAccountExistence()
    {
        var (registeredUser, _) = RegisteredUsersData.Generate()[0];
        const string unregisteredEmail = "ghost-bomb@example.com";

        var rejectedRegistered = await ExhaustEmailBucketAsync(registeredUser.Email, startIpOctet: 66);
        var rejectedUnknown = await ExhaustEmailBucketAsync(unregisteredEmail, startIpOctet: 69);

        rejectedRegistered.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        rejectedUnknown.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        var registeredBody = await rejectedRegistered.Content
            .ReadAsStringAsync(TestContext.Current.CancellationToken);
        var unknownBody = await rejectedUnknown.Content
            .ReadAsStringAsync(TestContext.Current.CancellationToken);

        using var registeredJson = JsonDocument.Parse(registeredBody);
        using var unknownJson = JsonDocument.Parse(unknownBody);

        registeredJson.RootElement.GetProperty("title").GetString()
            .ShouldBe(unknownJson.RootElement.GetProperty("title").GetString());
        registeredJson.RootElement.GetProperty("detail").GetString()
            .ShouldBe(unknownJson.RootElement.GetProperty("detail").GetString());
        registeredJson.RootElement.GetProperty("status").GetInt32()
            .ShouldBe(unknownJson.RootElement.GetProperty("status").GetInt32());

        registeredBody.ShouldNotContain(registeredUser.Email);
        unknownBody.ShouldNotContain(unregisteredEmail);
    }

    [Fact]
    [Priority(5)]
    public async Task EmailBombing_CaseAndWhitespaceVariants_ShouldShareSameBucket()
    {
        // TargetEmailRateLimiter partitions on NormalizeEmail (trim + lowercase), so an
        // attacker cannot dodge the per-email bucket by varying casing/whitespace across requests —
        // all three variants below must count against the SAME token bucket.
        string[] emailVariants =
        [
            "Case-Bomb@Example.com",
            "case-bomb@example.com",
            " CASE-BOMB@EXAMPLE.COM "
        ];

        const int startIpOctet = 72;

        for (var i = 0; i < TargetEmailTokenLimit; i++)
        {
            using var client = CreateClientWithIp($"203.0.113.{startIpOctet + i}");
            var allowed = await PostForgotPasswordAsync(client, emailVariants[i]);
            allowed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        }

        // Final request uses yet another casing/whitespace variant — still the same bucket, so it
        // must be throttled even though this exact string was never seen before.
        using var finalClient = CreateClientWithIp($"203.0.113.{startIpOctet + TargetEmailTokenLimit}");
        var rejected = await PostForgotPasswordAsync(finalClient, emailVariants[^1]);

        await AssertGenericRejectionAsync(rejected, emailVariants[0], emailVariants[1], emailVariants[2].Trim());
    }

    [Fact]
    [Priority(6)]
    public async Task EmailBombing_PerEmailThrottle_ShouldRecordRejectionMetric()
    {
        // Issue #397: the per-email bucket returned 429 and logged, but never recorded on the shared
        // rejection counter — the dashboard saw HTTP-policy and outbound-email rejections only.
        const string bombedEmail = "metric-bomb@example.com";

        var recordedPolicies = new ConcurrentBag<string>();

        using var listener = new MeterListener();

        listener.InstrumentPublished = (instrument, l) =>
        {
            if (instrument.Meter.Name == RateLimitingMetrics.MeterName
                && instrument.Name == RateLimitingMetrics.RejectionsCounterName)
            {
                l.EnableMeasurementEvents(instrument);
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

        // Distinct IPs per request (75, 76 allowed; 77 rejected), so only the per-email bucket trips —
        // any measurement tagged with the per-email policy is attributable to that limiter alone.
        var rejected = await ExhaustEmailBucketAsync(bombedEmail, startIpOctet: 75);

        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        // Exactly one rejection happened in this test, so exactly one measurement carries the tag.
        // No other test records this policy value (the listener started after the earlier tests in
        // this sequential class finished), so an exact count also guards against double-recording.
        recordedPolicies.Count(p => p == ForgotPasswordEndpoint.MetricPolicy).ShouldBe(1);
    }
}
