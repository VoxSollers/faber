using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Faber.Modules.Auth.Application.Features.SignUp;
using Faber.Testing.Shared.Data;
using FastEndpoints.Testing;
using Shouldly;
using static Faber.Api.Tests.Features.RateLimiting.RateLimitingTestConstants;

namespace Faber.Api.Tests.Features.RateLimiting;

[Collection<CollectionRateLimiting>]
[Priority(5)]
public class SignUpRateLimitingTests(WebApp app) : TestBase
{
    // IP octets 90-99 reserved for this class (RateLimitingTests uses 10-32, SignInRateLimitingTests
    // 50-55, ForgotPasswordRateLimitingTests 60-74, ResetPasswordRateLimitingTests 80-81).
    // AuthStrictPermitLimit (2) < AnonymousPermitLimit (3): a 429 on the 3rd request from an IP can
    // only come from the auth-strict policy on the sign-up endpoint, never the global baseline —
    // which is also why no IP here is used for more than 3 requests.

    private const string ValidPassword = "Valid-Password-123!";
    private const string MismatchedPassword = "Different-Password-456!";

    private HttpClient CreateClientWithIp(string ip)
    {
        var client = app.CreateClient();
        client.DefaultRequestHeaders.Add(TestClientIpStartupFilter.HeaderName, ip);

        return client;
    }

    // Deliberately mismatched ConfirmPassword: the request still binds and is counted by the limiter,
    // then fails validation with a deterministic 400 — so a throttling probe never creates an account.
    private static SignUpRequest Probe(string email)
    {
        return new SignUpRequest(
            $"probe-{Guid.NewGuid():N}",
            ValidPassword,
            MismatchedPassword,
            email,
            "Probe",
            "User");
    }

    private static Task<HttpResponseMessage> PostSignUpAsync(HttpClient client, string email)
    {
        return client.PostAsJsonAsync(SignUpUri, Probe(email), TestContext.Current.CancellationToken);
    }

    private static async Task<HttpResponseMessage> ExhaustAuthStrictLimitAsync(HttpClient client, string email)
    {
        for (var i = 0; i < AuthStrictPermitLimit; i++)
        {
            var allowed = await PostSignUpAsync(client, email);
            allowed.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        return await PostSignUpAsync(client, email);
    }

    [Fact]
    [Priority(1)]
    public async Task ExceedingAuthStrictLimit_ShouldReturn429WithRetryAfterAndGenericBody()
    {
        const string clientIp = "203.0.113.90";
        const string email = "mass-signup@example.com";
        using var client = CreateClientWithIp(clientIp);

        var rejected = await ExhaustAuthStrictLimitAsync(client, email);

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

        // No information leakage: partition key, IP, limits and the submitted email must not appear.
        body.ShouldNotContain(clientIp);
        body.ShouldNotContain("PermitLimit");
        body.ShouldNotContain("ip:");
        body.ShouldNotContain(email);
    }

    [Fact]
    [Priority(2)]
    public async Task DifferentClientIps_ShouldHaveIsolatedSignUpPartitions()
    {
        using var exhaustedClient = CreateClientWithIp("203.0.113.91");
        using var freshClient = CreateClientWithIp("203.0.113.92");

        var rejected = await ExhaustAuthStrictLimitAsync(exhaustedClient, "isolated-a@example.com");
        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        var freshResponse = await PostSignUpAsync(freshClient, "isolated-b@example.com");
        freshResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Priority(3)]
    public async Task ValidSignUpAfterLimitExceeded_ShouldBeRejectedAndCreateNoAccount()
    {
        using var throttledClient = CreateClientWithIp("203.0.113.93");

        for (var i = 0; i < AuthStrictPermitLimit; i++)
        {
            var allowed = await PostSignUpAsync(throttledClient, "mass-registration@example.com");
            allowed.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        // The 3rd request is a fully valid sign-up that would otherwise create an account. The limiter
        // runs before validation and before the handler, so a throttled bot cannot slip one more real
        // registration through — this is the mass-registration property, not just a 429 on garbage.
        var validRequest = new SignUpRequest(
            "throttled-signup-user",
            ValidPassword,
            ValidPassword,
            "throttled-signup@example.com",
            "Throttled",
            "User");

        var throttled = await throttledClient.PostAsJsonAsync(
            SignUpUri, validRequest, TestContext.Current.CancellationToken);

        throttled.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        // Proof the rejected request had no side effect: the very same registration succeeds from a
        // fresh partition, which it could not do if the throttled attempt had already created it.
        using var freshClient = CreateClientWithIp("203.0.113.94");

        var accepted = await freshClient.PostAsJsonAsync(
            SignUpUri, validRequest, TestContext.Current.CancellationToken);

        accepted.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    [Priority(4)]
    public async Task ThrottledResponses_ShouldNotRevealAccountExistence()
    {
        var (registeredUser, _) = RegisteredUsersData.Generate()[4];
        const string unregisteredEmail = "ghost-signup@example.com";

        using var registeredClient = CreateClientWithIp("203.0.113.95");
        using var unknownClient = CreateClientWithIp("203.0.113.96");

        var rejectedRegistered = await ExhaustAuthStrictLimitAsync(registeredClient, registeredUser.Email);
        var rejectedUnknown = await ExhaustAuthStrictLimitAsync(unknownClient, unregisteredEmail);

        rejectedRegistered.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        rejectedUnknown.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        var registeredBody = await rejectedRegistered.Content
            .ReadAsStringAsync(TestContext.Current.CancellationToken);
        var unknownBody = await rejectedUnknown.Content
            .ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Sign-up's validator does distinguish taken from free emails at 400 level; the throttled
        // response must not — identical rejection contract either way (traceId aside).
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
}
