using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Faber.Modules.Auth.Application.Features.Refresh;
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
[Priority(7)]
public class RefreshRateLimitingTests(WebApp app) : TestBase
{
    // IP octets 110-119 reserved for this class (RateLimitingTests uses 10-32, SignInRateLimitingTests
    // 50-55, ForgotPasswordRateLimitingTests 60-74, ResetPasswordRateLimitingTests 80-81,
    // SignUpRateLimitingTests 90-99, VerifyEmailRateLimitingTests 100-109).
    // AuthRefreshPermitLimit (2) < AnonymousPermitLimit (3): a 429 on the 3rd request from an
    // anonymous IP can only come from auth-refresh, never from the global baseline — which is also
    // why no anonymous IP here is used for more than 3 requests.

    // Token grinding looks exactly like this: a syntactically plausible but meaningless token.
    // Keycloak answers invalid_grant, which ApiExceptionHandler surfaces as a deterministic 400.
    private const string InvalidRefreshToken = "invalid-refresh-token";

    private const string UnknownUsername = "ghost.refresh@example.com";
    private const string WrongPassword = "Wrong-Password-123!";

    private HttpClient CreateMobileClientWithIp(string ip)
    {
        var client = app.CreateClient();
        client.DefaultRequestHeaders.Add(TestClientIpStartupFilter.HeaderName, ip);
        client.DefaultRequestHeaders.Add(ClientTypeHeader, nameof(ClientType.Mobile));

        return client;
    }

    private HttpClient CreateClientWithIp(string ip)
    {
        var client = app.CreateClient();
        client.DefaultRequestHeaders.Add(TestClientIpStartupFilter.HeaderName, ip);

        return client;
    }

    private static Task<HttpResponseMessage> PostRefreshAsync(HttpClient client, string refreshToken)
    {
        return client.PostAsJsonAsync(
            RefreshUri,
            new RefreshTokenRequest(refreshToken, nameof(ClientType.Mobile)),
            TestContext.Current.CancellationToken);
    }

    private static async Task<HttpResponseMessage> ExhaustAuthRefreshLimitAsync(HttpClient client)
    {
        for (var i = 0; i < AuthRefreshPermitLimit; i++)
        {
            var allowed = await PostRefreshAsync(client, InvalidRefreshToken);
            allowed.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        return await PostRefreshAsync(client, InvalidRefreshToken);
    }

    private async Task<SignInResponse> SignInAsync(string username, string password, string clientIp)
    {
        using var client = CreateClientWithIp(clientIp);

        var response = await client.PostAsJsonAsync(
            SignInUri,
            new SignInRequest(username, password),
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var signInResponse = await response.Content.ReadFromJsonAsync<SignInResponse>(
            TestContext.Current.CancellationToken);

        signInResponse.ShouldNotBeNull();

        return signInResponse;
    }

    [Fact]
    [Priority(1)]
    public async Task ExceedingAuthRefreshLimit_ShouldReturn429WithRetryAfterAndGenericBody()
    {
        const string clientIp = "203.0.113.110";
        using var client = CreateMobileClientWithIp(clientIp);

        var rejected = await ExhaustAuthRefreshLimitAsync(client);

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

        // No information leakage: partition keys, IP, limits — and the submitted refresh token,
        // which is itself a credential — must never be echoed back.
        body.ShouldNotContain(clientIp);
        body.ShouldNotContain("PermitLimit");
        body.ShouldNotContain("ip:");
        body.ShouldNotContain("user:");
        body.ShouldNotContain(InvalidRefreshToken);
    }

    [Fact]
    [Priority(2)]
    public async Task DifferentClientIps_ShouldHaveIsolatedRefreshPartitions()
    {
        using var exhaustedClient = CreateMobileClientWithIp("203.0.113.111");
        using var freshClient = CreateMobileClientWithIp("203.0.113.112");

        var rejected = await ExhaustAuthRefreshLimitAsync(exhaustedClient);
        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        var freshResponse = await PostRefreshAsync(freshClient, InvalidRefreshToken);
        freshResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Priority(3)]
    public async Task SpentAuthStrictBudget_ShouldNotThrottleRefresh()
    {
        const string clientIp = "203.0.113.113";
        using var signInClient = CreateClientWithIp(clientIp);

        // Spend this IP's entire auth-strict budget on failed sign-ins.
        for (var i = 0; i < AuthStrictPermitLimit; i++)
        {
            var attempt = await signInClient.PostAsJsonAsync(
                SignInUri,
                new SignInRequest(UnknownUsername, WrongPassword),
                TestContext.Current.CancellationToken);

            attempt.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        // Refresh draws on its own auth-refresh budget: a sign-in storm from an address must not
        // strand a legitimate session on that same address with no way to renew its tokens.
        // Third and final request from this IP, so the global anonymous baseline (3) still permits it.
        using var refreshClient = CreateMobileClientWithIp(clientIp);

        var refreshResponse = await PostRefreshAsync(refreshClient, InvalidRefreshToken);
        refreshResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Priority(4)]
    public async Task BearerTokenOnRefresh_ShouldNotBuyASecondBudget()
    {
        var (user, password) = RegisteredUsersData.Generate()[2];

        var signIn = await SignInAsync(user.Username, password, "203.0.113.114");

        // Spend the whole per-IP budget anonymously, the way the real client refreshes.
        const string clientIp = "203.0.113.116";
        using var anonymousClient = CreateMobileClientWithIp(clientIp);

        var rejected = await ExhaustAuthRefreshLimitAsync(anonymousClient);
        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        // Same address, now with a valid bearer token attached. The refresh handler never reads the
        // Authorization header, so this token buys nothing but a different partition key — and it
        // must not: otherwise one actor doubles its budget just by toggling a header. The global
        // limiter cannot be what rejects this request, because a bearer token moves it onto the
        // user's own global-authenticated partition, which is untouched (1 of 3).
        using var bearerClient = CreateMobileClientWithIp(clientIp).WithAuthToken(signIn.AccessToken);

        var stillRejected = await PostRefreshAsync(bearerClient, InvalidRefreshToken);
        stillRejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    [Priority(5)]
    public async Task NormalSessionRefresh_ShouldNotBeThrottled()
    {
        var (user, password) = RegisteredUsersData.Generate()[4];

        var signIn = await SignInAsync(user.Username, password, "203.0.113.117");

        using var client = CreateMobileClientWithIp("203.0.113.118");

        // A real session's refresh cycle, token rotation included, fits inside the budget: the
        // policy is generous by design and must never cost a legitimate user their session.
        var first = await PostRefreshAsync(client, signIn.RefreshToken);
        first.StatusCode.ShouldBe(HttpStatusCode.OK);

        var firstTokens = await first.Content.ReadFromJsonAsync<RefreshResponse>(
            TestContext.Current.CancellationToken);

        firstTokens.ShouldNotBeNull();
        firstTokens.RefreshToken.ShouldNotBe(signIn.RefreshToken);

        var second = await PostRefreshAsync(client, firstTokens.RefreshToken);
        second.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
