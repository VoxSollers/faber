using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Faber.Modules.Auth.Application.Features.SignUp;
using Faber.Modules.Auth.Application.Features.VerifyEmail;
using Faber.Modules.Identity.PublicApi.Shared;
using FastEndpoints.Testing;
using Shouldly;
using static Faber.Api.Tests.Features.RateLimiting.RateLimitingTestConstants;

namespace Faber.Api.Tests.Features.RateLimiting;

[Collection<CollectionRateLimiting>]
[Priority(6)]
public class VerifyEmailRateLimitingTests(WebApp app) : TestBase
{
    // IP octets 100-109 reserved for this class (RateLimitingTests uses 10-32, SignInRateLimitingTests
    // 50-55, ForgotPasswordRateLimitingTests 60-74, ResetPasswordRateLimitingTests 80-81,
    // SignUpRateLimitingTests 90-99). No IP is used for more than 3 requests, so a 429 on the 3rd
    // can only come from auth-strict (limit 2), never from the global anonymous baseline (limit 3).

    // A syntactically valid (>= 27 char) but meaningless combined key: passes validation, splits into
    // selector + token, and deterministically fails token verification with a 400 — the same probe
    // shape ResetPasswordRateLimitingTests uses. This is exactly what link-hammering looks like.
    private static readonly string InvalidCombinedKey = new('x', 40);

    private HttpClient CreateClientWithIp(string ip)
    {
        var client = app.CreateClient();
        client.DefaultRequestHeaders.Add(TestClientIpStartupFilter.HeaderName, ip);

        return client;
    }

    private static Task<HttpResponseMessage> PostVerifyEmailAsync(HttpClient client)
    {
        return client.PostAsJsonAsync(
            VerifyEmailUri,
            new VerifyEmailRequest(new CombinedKey(InvalidCombinedKey)),
            TestContext.Current.CancellationToken);
    }

    [Fact]
    [Priority(1)]
    public async Task ExceedingAuthStrictLimit_ShouldReturn429WithRetryAfterAndGenericBody()
    {
        const string clientIp = "203.0.113.100";
        using var client = CreateClientWithIp(clientIp);

        for (var i = 0; i < AuthStrictPermitLimit; i++)
        {
            var allowed = await PostVerifyEmailAsync(client);
            allowed.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        var rejected = await PostVerifyEmailAsync(client);

        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        rejected.Headers.RetryAfter.ShouldNotBeNull();
        rejected.Headers.RetryAfter!.Delta.ShouldNotBeNull();
        rejected.Headers.RetryAfter.Delta!.Value.TotalSeconds.ShouldBeGreaterThan(0);
        rejected.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await rejected.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        using var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("title").GetString().ShouldBe(ExpectedTitle);
        json.RootElement.GetProperty("detail").GetString().ShouldBe(ExpectedDetail);
        json.RootElement.GetProperty("status").GetInt32().ShouldBe(429);

        // No information leakage: partition key, IP, limits and the guessed token must not appear.
        body.ShouldNotContain(clientIp);
        body.ShouldNotContain("PermitLimit");
        body.ShouldNotContain("ip:");
        body.ShouldNotContain(InvalidCombinedKey);

        // Fresh IP is a separate partition, still allowed.
        using var freshClient = CreateClientWithIp("203.0.113.101");
        var freshResponse = await PostVerifyEmailAsync(freshClient);
        freshResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Priority(2)]
    public async Task SignUpAndVerifyEmail_ShouldShareOneAuthStrictBudgetPerIp()
    {
        using var client = CreateClientWithIp("203.0.113.102");

        // auth-strict is a single limiter instance shared by every endpoint that opts into it, so
        // these two requests spend the whole per-IP allowance (2) between them — alternating
        // endpoints must not buy an attacker a second budget.
        var signUpProbe = new SignUpRequest(
            $"probe-{Guid.NewGuid():N}",
            "Valid-Password-123!",
            "Different-Password-456!",
            "shared-budget@example.com",
            "Probe",
            "User");

        var signUpResponse = await client.PostAsJsonAsync(
            SignUpUri, signUpProbe, TestContext.Current.CancellationToken);
        signUpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var verifyResponse = await PostVerifyEmailAsync(client);
        verifyResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var rejected = await PostVerifyEmailAsync(client);
        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }
}
