using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Faber.Api.Tests.Features.RateLimiting.Data;
using Faber.Modules.Auth.Application.Features.SignIn;
using Faber.Testing.Shared.Http;
using FastEndpoints.Testing;
using Shouldly;
using static Faber.Api.Tests.Features.RateLimiting.RateLimitingTestConstants;

namespace Faber.Api.Tests.Features.RateLimiting;

[Collection<CollectionRateLimiting>]
[Priority(10)]
public class UpdateUserFullNameRateLimitingTests(WebApp app) : TestBase
{
    // IP octets 140-149 reserved for this class (RateLimitingTests uses 10-32, SignInRateLimitingTests
    // 50-55, ForgotPasswordRateLimitingTests 60-74, ResetPasswordRateLimitingTests 80-81,
    // SignUpRateLimitingTests 90-99, VerifyEmailRateLimitingTests 100-109, RefreshRateLimitingTests
    // 110-119, SignOutRateLimitingTests 120-129, UserLookupRateLimitingTests 130-139).
    //
    // Accounts come from RateLimitingUsersData. Each policy owns its own limiter dictionary, so the
    // authenticated-default budgets of indices 0-2 are untouched even though UserLookupRateLimitingTests
    // spent their user-lookup budgets. SpentUserLookupBudget_... needs both budgets fresh, so it
    // uses index 4 — the one index that suite never signs in as.
    //
    // AuthenticatedDefaultPermitLimit (2) sits far below AuthenticatedPermitLimit (30): a 429 on the
    // 3rd write can only come from authenticated-default, never from the global baseline.

    private HttpClient CreateClientWithIp(string ip)
    {
        var client = app.CreateClient();
        client.DefaultRequestHeaders.Add(TestClientIpStartupFilter.HeaderName, ip);

        return client;
    }

    private async Task<string> SignInAsync(int userIndex, string clientIp)
    {
        var (user, password) = RateLimitingUsersData.Generate()[userIndex];

        using var client = CreateClientWithIp(clientIp);

        var response = await client.PostAsJsonAsync(
            SignInUri,
            new SignInRequest(user.Username, password),
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var signIn = await response.Content.ReadFromJsonAsync<SignInResponse>(
            TestContext.Current.CancellationToken);

        signIn.ShouldNotBeNull();

        return signIn.AccessToken;
    }

    // An empty firstName fails request validation before the handler runs, so no Keycloak round-trip
    // and no profile is actually rewritten: a deterministic 400 that isolates what these tests
    // measure to the limiter alone.
    private static Task<HttpResponseMessage> PutFullNameAsync(HttpClient client)
    {
        return client.PutAsync(
            string.Format(UsersByIdUriFormat, Guid.NewGuid()),
            new StringContent("""{"firstName":"","lastName":""}""", Encoding.UTF8, "application/json"),
            TestContext.Current.CancellationToken);
    }

    private static async Task<HttpResponseMessage> ExhaustAuthenticatedDefaultBudgetAsync(HttpClient client)
    {
        for (var i = 0; i < AuthenticatedDefaultPermitLimit; i++)
        {
            var allowed = await PutFullNameAsync(client);
            allowed.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        return await PutFullNameAsync(client);
    }

    [Fact]
    [Priority(1)]
    public async Task ExceedingAuthenticatedDefaultLimit_ShouldReturn429WithRetryAfterAndGenericBody()
    {
        var token = await SignInAsync(0, "203.0.113.140");

        const string clientIp = "203.0.113.141";
        using var client = CreateClientWithIp(clientIp).WithAuthToken(token);

        var rejected = await ExhaustAuthenticatedDefaultBudgetAsync(client);

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
    public async Task DifferentUsersOnSameIp_ShouldHaveIsolatedWritePartitions()
    {
        var firstToken = await SignInAsync(1, "203.0.113.142");
        var secondToken = await SignInAsync(2, "203.0.113.143");

        const string sharedIp = "203.0.113.144";
        using var firstClient = CreateClientWithIp(sharedIp).WithAuthToken(firstToken);
        using var secondClient = CreateClientWithIp(sharedIp).WithAuthToken(secondToken);

        var rejected = await ExhaustAuthenticatedDefaultBudgetAsync(firstClient);
        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        // Same address, different `sub` claim => its own partition. One account hammering profile
        // writes must not lock out everyone else sharing that egress IP.
        var secondUserResponse = await PutFullNameAsync(secondClient);
        secondUserResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Priority(3)]
    public async Task SpentUserLookupBudget_ShouldNotThrottleFullNameUpdate()
    {
        var token = await SignInAsync(4, "203.0.113.145");

        using var lookupClient = CreateClientWithIp("203.0.113.146").WithAuthToken(token);

        // Spend this user's entire user-lookup budget on directory probing.
        for (var i = 0; i < UserLookupPermitLimit; i++)
        {
            var probe = await lookupClient.GetAsync(
                string.Format(UsersByEmailUriFormat, $"ghost{i}@example.com"),
                TestContext.Current.CancellationToken);

            probe.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        // The write draws on its own authenticated-default budget: an enumeration burst must never
        // leave the same account unable to edit its own profile.
        using var writeClient = CreateClientWithIp("203.0.113.147").WithAuthToken(token);

        var writeResponse = await PutFullNameAsync(writeClient);
        writeResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
