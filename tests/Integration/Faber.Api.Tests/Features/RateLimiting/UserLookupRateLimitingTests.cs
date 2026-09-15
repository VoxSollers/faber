using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Faber.Api.Tests.Features.RateLimiting.Data;
using Faber.Modules.Auth.Application.Features.SignIn;
using Faber.Testing.Shared.Http;
using FastEndpoints.Testing;
using Shouldly;
using static Faber.Api.Tests.Features.RateLimiting.RateLimitingTestConstants;

namespace Faber.Api.Tests.Features.RateLimiting;

[Collection<CollectionRateLimiting>]
[Priority(9)]
public class UserLookupRateLimitingTests(WebApp app) : TestBase
{
    // IP octets 130-139 reserved for this class (RateLimitingTests uses 10-32, SignInRateLimitingTests
    // 50-55, ForgotPasswordRateLimitingTests 60-74, ResetPasswordRateLimitingTests 80-81,
    // SignUpRateLimitingTests 90-99, VerifyEmailRateLimitingTests 100-109, RefreshRateLimitingTests
    // 110-119, SignOutRateLimitingTests 120-129, UpdateUserFullNameRateLimitingTests 140-149).
    //
    // Accounts come from RateLimitingUsersData, not the shared RegisteredUsersData set: every user
    // there is already spent by another suite, and these tests need budgets nobody has touched.
    // Each test claims its own index so no two tests contend for one user-lookup budget.
    //
    // UserLookupPermitLimit (2) sits far below AuthenticatedPermitLimit (30): a 429 on the 3rd
    // lookup can only come from user-lookup, never from the global baseline.

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

    // Probing an identifier that belongs to nobody is exactly what an enumerator does, and by-email
    // and by-username resolve through Keycloak *search* endpoints, which answer with an empty array
    // for an unknown value — a deterministic 404 that isolates what these tests measure to the
    // limiter alone. The by-id route is NOT interchangeable here: it resolves through Keycloak's
    // GET /users/{id}, which 404s at the transport level and surfaces as a 500. Only use the by-id
    // route where the limiter rejects the request before the handler runs.
    private static Task<HttpResponseMessage> LookupAsync(HttpClient client, string uriFormat, string identifier)
    {
        return client.GetAsync(
            string.Format(uriFormat, identifier),
            TestContext.Current.CancellationToken);
    }

    private static async Task<HttpResponseMessage> ExhaustLookupBudgetAsync(HttpClient client)
    {
        for (var i = 0; i < UserLookupPermitLimit; i++)
        {
            var allowed = await LookupAsync(client, UsersByEmailUriFormat, $"ghost{i}@example.com");
            allowed.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        return await LookupAsync(client, UsersByEmailUriFormat, "ghost-final@example.com");
    }

    [Fact]
    [Priority(1)]
    public async Task ExceedingUserLookupLimit_ShouldReturn429WithRetryAfterAndGenericBody()
    {
        var token = await SignInAsync(0, "203.0.113.130");

        const string clientIp = "203.0.113.131";
        using var client = CreateClientWithIp(clientIp).WithAuthToken(token);

        var rejected = await ExhaustLookupBudgetAsync(client);

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
    public async Task RotatingAcrossLookupEndpoints_ShouldDrawOnOneSharedBudget()
    {
        var token = await SignInAsync(1, "203.0.113.132");

        using var client = CreateClientWithIp("203.0.113.133").WithAuthToken(token);

        // Three different lookup routes, one budget: the whole point of leaving the route out of the
        // partition key. An enumerator who switches identifier form must not earn a fresh allowance.
        var byEmail = await LookupAsync(client, UsersByEmailUriFormat, "ghost@example.com");
        byEmail.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var byUsername = await LookupAsync(client, UsersByNameUriFormat, "ghost-user");
        byUsername.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // Deliberately the third and last call: the budget is already spent, so the limiter rejects
        // this before the by-id handler could reach Keycloak. Do not reorder.
        var byId = await LookupAsync(client, UsersByIdUriFormat, Guid.NewGuid().ToString());
        byId.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    [Priority(3)]
    public async Task DifferentUsersOnSameIp_ShouldHaveIsolatedLookupPartitions()
    {
        var firstToken = await SignInAsync(2, "203.0.113.134");
        var secondToken = await SignInAsync(3, "203.0.113.135");

        const string sharedIp = "203.0.113.136";
        using var firstClient = CreateClientWithIp(sharedIp).WithAuthToken(firstToken);
        using var secondClient = CreateClientWithIp(sharedIp).WithAuthToken(secondToken);

        var rejected = await ExhaustLookupBudgetAsync(firstClient);
        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        // Same address, different `sub` claim => its own partition. One abusive account behind a
        // corporate NAT must not lock out everyone else sharing that egress IP.
        var secondUserResponse = await LookupAsync(secondClient, UsersByEmailUriFormat, "ghost@example.com");
        secondUserResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    [Priority(4)]
    public async Task AnonymousLookupFlood_ShouldBeThrottledPerIp()
    {
        // No bearer token: with no `sub` claim the partition falls back to the client IP. The
        // limiter runs before UseAuthorization, so the first probes get the endpoint's usual 401 and
        // an exhausted budget then yields 429 — an unauthenticated enumerator is stopped at the
        // limiter, not merely at the auth check.
        using var client = CreateClientWithIp("203.0.113.137");

        for (var i = 0; i < UserLookupPermitLimit; i++)
        {
            var allowed = await LookupAsync(client, UsersByEmailUriFormat, $"ghost{i}@example.com");
            allowed.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        var rejected = await LookupAsync(client, UsersByEmailUriFormat, "ghost-final@example.com");
        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }
}
