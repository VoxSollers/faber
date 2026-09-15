using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Faber.Api.Tests.Features.RateLimiting.Data;
using Faber.Modules.Auth.Application.Features.SignIn;
using Faber.Modules.Auth.Application.Features.VerifyEmail;
using Faber.Modules.Identity.Application.Features.VerifyActionToken;
using Faber.Modules.Identity.Domain.Enums;
using Faber.Modules.Identity.PublicApi.Shared;
using Faber.Testing.Shared.Http;
using FastEndpoints.Testing;
using Shouldly;
using static Faber.Api.Tests.Features.RateLimiting.RateLimitingTestConstants;

namespace Faber.Api.Tests.Features.RateLimiting;

[Collection<CollectionRateLimiting>]
[Priority(11)]
public class VerifyActionTokenRateLimitingTests(WebApp app) : TestBase
{
    // IP octets 150-159 reserved for this class (RateLimitingTests uses 10-32, SignInRateLimitingTests
    // 50-55, ForgotPasswordRateLimitingTests 60-74, ResetPasswordRateLimitingTests 80-81,
    // SignUpRateLimitingTests 90-99, VerifyEmailRateLimitingTests 100-109, RefreshRateLimitingTests
    // 110-119, SignOutRateLimitingTests 120-129, UserLookupRateLimitingTests 130-139,
    // UpdateUserFullNameRateLimitingTests 140-149). No IP sends more than 3 anonymous requests, so a
    // 429 on the 3rd can only come from auth-strict (limit 2), never from the global anonymous
    // baseline (limit 3).

    // A syntactically valid (>= 27 char) but meaningless combined key: passes validation, splits into
    // selector + token, and deterministically fails verification with a 400 — the same probe shape
    // VerifyEmailRateLimitingTests uses. This is exactly what action-token grinding looks like.
    private static readonly string InvalidCombinedKey = new('x', 40);

    private HttpClient CreateClientWithIp(string ip)
    {
        var client = app.CreateClient();
        client.DefaultRequestHeaders.Add(TestClientIpStartupFilter.HeaderName, ip);

        return client;
    }

    private static Task<HttpResponseMessage> PostVerifyActionTokenAsync(HttpClient client)
    {
        return client.PostAsJsonAsync(
            VerifyActionTokenUri,
            new VerifyActionTokenRequest(new CombinedKey(InvalidCombinedKey), ActionTokenType.VerifyEmail),
            TestContext.Current.CancellationToken);
    }

    [Fact]
    [Priority(1)]
    public async Task ExceedingAuthStrictLimit_ShouldReturn429WithRetryAfterAndGenericBody()
    {
        const string clientIp = "203.0.113.150";
        using var client = CreateClientWithIp(clientIp);

        for (var i = 0; i < AuthStrictPermitLimit; i++)
        {
            var allowed = await PostVerifyActionTokenAsync(client);
            allowed.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        var rejected = await PostVerifyActionTokenAsync(client);

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

        // No token-validity leakage beyond the necessary result: partition key, IP, limits and the
        // guessed token must not appear in the rejection body.
        body.ShouldNotContain(clientIp);
        body.ShouldNotContain("PermitLimit");
        body.ShouldNotContain("ip:");
        body.ShouldNotContain(InvalidCombinedKey);

        // Fresh IP is a separate partition, still allowed.
        using var freshClient = CreateClientWithIp("203.0.113.151");
        var freshResponse = await PostVerifyActionTokenAsync(freshClient);
        freshResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Priority(2)]
    public async Task VerifyEmailAndVerifyActionToken_ShouldShareOneAuthStrictBudgetPerIp()
    {
        using var client = CreateClientWithIp("203.0.113.152");

        // auth-strict is a single limiter instance shared by every endpoint that opts into it, so
        // Auth's verify-email and Identity's verify-action-token spend the whole per-IP allowance (2)
        // between them — grinding the same token through the other module's route must not buy an
        // attacker a second budget.
        var verifyEmailResponse = await client.PostAsJsonAsync(
            VerifyEmailUri,
            new VerifyEmailRequest(new CombinedKey(InvalidCombinedKey)),
            TestContext.Current.CancellationToken);
        verifyEmailResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var verifyActionTokenResponse = await PostVerifyActionTokenAsync(client);
        verifyActionTokenResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var rejected = await PostVerifyActionTokenAsync(client);
        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    [Priority(3)]
    public async Task BearerToken_ShouldNotBuyFreshAuthStrictBudget()
    {
        // auth-strict partitions by IP only. On an AllowAnonymous endpoint a user-or-IP partition
        // would let an attacker double the budget by toggling a bearer header (anonymous probes spend
        // the ip: partition, authenticated ones a user: partition) — this pins the ByIp choice.
        var (user, password) = RateLimitingUsersData.Generate()[3];

        using var signInClient = CreateClientWithIp("203.0.113.153");

        var signInResponse = await signInClient.PostAsJsonAsync(
            SignInUri,
            new SignInRequest(user.Username, password),
            TestContext.Current.CancellationToken);

        signInResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var signIn = await signInResponse.Content.ReadFromJsonAsync<SignInResponse>(
            TestContext.Current.CancellationToken);

        signIn.ShouldNotBeNull();

        const string grindingIp = "203.0.113.154";
        using var anonymousClient = CreateClientWithIp(grindingIp);

        for (var i = 0; i < AuthStrictPermitLimit; i++)
        {
            var allowed = await PostVerifyActionTokenAsync(anonymousClient);
            allowed.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        using var authedClient = CreateClientWithIp(grindingIp).WithAuthToken(signIn.AccessToken);

        var rejected = await PostVerifyActionTokenAsync(authedClient);
        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }
}
