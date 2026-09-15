using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Faber.Modules.Auth.Application.Features.Shared.Requests;
using Faber.Modules.Auth.Application.Features.SignIn;
using Faber.Modules.Auth.Domain.Enums;
using Faber.Testing.Shared.Data;
using Faber.Testing.Shared.Http;
using FastEndpoints.Testing;
using Shouldly;
using static Faber.Api.Tests.Features.RateLimiting.RateLimitingTestConstants;

namespace Faber.Api.Tests.Features.RateLimiting;

[Collection<CollectionRateLimiting>]
[Priority(8)]
public class SignOutRateLimitingTests(WebApp app) : TestBase
{
    // IP octets 120-129 reserved for this class (RateLimitingTests uses 10-32, SignInRateLimitingTests
    // 50-55, ForgotPasswordRateLimitingTests 60-74, ResetPasswordRateLimitingTests 80-81,
    // SignUpRateLimitingTests 90-99, VerifyEmailRateLimitingTests 100-109, RefreshRateLimitingTests
    // 110-119).
    // AuthSessionPermitLimit (2) < AnonymousPermitLimit (3): a 429 on the 3rd request from an
    // anonymous IP can only come from auth-session, never from the global baseline — which is also
    // why no anonymous IP here is used for more than 3 requests.

    private HttpClient CreateClientWithIp(string ip)
    {
        var client = app.CreateClient();
        client.DefaultRequestHeaders.Add(TestClientIpStartupFilter.HeaderName, ip);

        return client;
    }

    // A bare {} body fails request validation (no client type) before the handler runs, so no
    // Keycloak round-trip and no session is actually terminated: a deterministic 400 that isolates
    // what these tests measure to the limiter alone.
    private static Task<HttpResponseMessage> PostSignOutAsync(HttpClient client)
    {
        return client.PostAsync(
            SignOutUri,
            new StringContent("{}", Encoding.UTF8, "application/json"),
            TestContext.Current.CancellationToken);
    }

    private static async Task<HttpResponseMessage> ExhaustAuthSessionLimitAsync(HttpClient client)
    {
        for (var i = 0; i < AuthSessionPermitLimit; i++)
        {
            var allowed = await PostSignOutAsync(client);
            allowed.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        return await PostSignOutAsync(client);
    }

    [Fact]
    [Priority(1)]
    public async Task ExceedingAuthSessionLimit_ShouldReturn429WithRetryAfterAndGenericBody()
    {
        const string clientIp = "203.0.113.120";
        using var client = CreateClientWithIp(clientIp);

        var rejected = await ExhaustAuthSessionLimitAsync(client);

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

        // No information leakage: partition keys, IP and limit values must never be echoed back.
        body.ShouldNotContain(clientIp);
        body.ShouldNotContain("PermitLimit");
        body.ShouldNotContain("ip:");
        body.ShouldNotContain("user:");
    }

    [Fact]
    [Priority(2)]
    public async Task DifferentClientIps_ShouldHaveIsolatedSignOutPartitions()
    {
        using var exhaustedClient = CreateClientWithIp("203.0.113.121");
        using var freshClient = CreateClientWithIp("203.0.113.122");

        var rejected = await ExhaustAuthSessionLimitAsync(exhaustedClient);
        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        var freshResponse = await PostSignOutAsync(freshClient);
        freshResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Priority(3)]
    public async Task SpentAuthRefreshBudget_ShouldNotThrottleSignOut()
    {
        const string clientIp = "203.0.113.123";
        using var refreshClient = CreateClientWithIp(clientIp);
        refreshClient.DefaultRequestHeaders.Add(ClientTypeHeader, nameof(ClientType.Mobile));

        // Spend this IP's entire auth-refresh budget on token grinding.
        for (var i = 0; i < AuthRefreshPermitLimit; i++)
        {
            var attempt = await refreshClient.PostAsJsonAsync(
                RefreshUri,
                new RefreshTokenRequest("invalid-refresh-token", nameof(ClientType.Mobile)),
                TestContext.Current.CancellationToken);

            attempt.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        // Sign-out draws on its own auth-session budget: a refresh storm from an address must never
        // leave a legitimate user on that same address unable to terminate their session.
        // Third and final request from this IP, so the global anonymous baseline (3) still permits it.
        using var signOutClient = CreateClientWithIp(clientIp);

        var signOutResponse = await PostSignOutAsync(signOutClient);
        signOutResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Priority(4)]
    public async Task BearerTokenOnSignOut_ShouldNotBuyASecondBudget()
    {
        var (user, password) = RegisteredUsersData.Generate()[3];

        using var signInClient = CreateClientWithIp("203.0.113.124");

        var signInResponse = await signInClient.PostAsJsonAsync(
            SignInUri,
            new SignInRequest(user.Username, password),
            TestContext.Current.CancellationToken);

        signInResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var signIn = await signInResponse.Content.ReadFromJsonAsync<SignInResponse>(
            TestContext.Current.CancellationToken);

        signIn.ShouldNotBeNull();

        // Spend the whole per-IP budget anonymously, the way the real client signs out.
        const string clientIp = "203.0.113.126";
        using var anonymousClient = CreateClientWithIp(clientIp);

        var rejected = await ExhaustAuthSessionLimitAsync(anonymousClient);
        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        // Same address, now with a valid bearer token attached. Sign-out is AllowAnonymous and never
        // reads the Authorization header, so this token must buy nothing — otherwise one actor
        // doubles its budget just by toggling a header. The global limiter cannot be what rejects
        // this request, because a bearer token moves it onto the user's own global-authenticated
        // partition, which is untouched (1 of 3).
        using var bearerClient = CreateClientWithIp(clientIp).WithAuthToken(signIn.AccessToken);

        var stillRejected = await PostSignOutAsync(bearerClient);
        stillRejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }
}
