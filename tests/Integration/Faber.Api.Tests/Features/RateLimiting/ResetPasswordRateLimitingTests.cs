using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Faber.Modules.Auth.Application.Features.ResetPassword;
using Faber.Modules.Identity.PublicApi.Shared;
using FastEndpoints.Testing;
using Shouldly;
using static Faber.Api.Tests.Features.RateLimiting.RateLimitingTestConstants;

namespace Faber.Api.Tests.Features.RateLimiting;

[Collection<CollectionRateLimiting>]
[Priority(4)]
public class ResetPasswordRateLimitingTests(WebApp app) : TestBase
{
    // A syntactically valid (>= 27 char) but meaningless combined key: passes validation, reaches
    // the handler, and deterministically fails token verification with a 400 — same probe shape as
    // the existing InvalidTokenData-driven tests in the Auth module suite.
    private static readonly string InvalidCombinedKey = new('x', 40);
    private const string ValidPassword = "Valid-Password-123!";

    private HttpClient CreateClientWithIp(string ip)
    {
        var client = app.CreateClient();
        client.DefaultRequestHeaders.Add(TestClientIpStartupFilter.HeaderName, ip);

        return client;
    }

    private static Task<HttpResponseMessage> PutResetPasswordAsync(HttpClient client)
    {
        return client.PutAsJsonAsync(
            ResetPasswordUri,
            new ResetPasswordRequest(new CombinedKey(InvalidCombinedKey), ValidPassword, ValidPassword),
            TestContext.Current.CancellationToken);
    }

    [Fact]
    [Priority(1)]
    public async Task ExceedingAuthPasswordResetLimit_ShouldReturn429WithRetryAfterAndGenericBody()
    {
        const string clientIp = "203.0.113.80";
        using var client = CreateClientWithIp(clientIp);

        for (var i = 0; i < AuthPasswordResetPermitLimit; i++)
        {
            var allowed = await PutResetPasswordAsync(client);
            allowed.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        var rejected = await PutResetPasswordAsync(client);

        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        rejected.Headers.RetryAfter.ShouldNotBeNull();
        rejected.Headers.RetryAfter!.Delta.ShouldNotBeNull();
        rejected.Headers.RetryAfter.Delta!.Value.TotalSeconds.ShouldBeGreaterThan(0);

        var body = await rejected.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        using var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("title").GetString().ShouldBe(ExpectedTitle);
        json.RootElement.GetProperty("detail").GetString().ShouldBe(ExpectedDetail);
        json.RootElement.GetProperty("status").GetInt32().ShouldBe(429);

        body.ShouldNotContain(clientIp);
        body.ShouldNotContain("PermitLimit");
        body.ShouldNotContain("ip:");
        body.ShouldNotContain(InvalidCombinedKey);

        // Fresh IP is a separate partition, still allowed.
        using var freshIpClient = CreateClientWithIp("203.0.113.81");
        var freshResponse = await PutResetPasswordAsync(freshIpClient);
        freshResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
