using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Faber.Modules.Auth.Application.Features.SignIn;
using Faber.Testing.Shared.Data;
using FastEndpoints.Testing;
using Shouldly;
using static Faber.Api.Tests.Features.RateLimiting.RateLimitingTestConstants;

namespace Faber.Api.Tests.Features.RateLimiting;

[Collection<CollectionRateLimiting>]
[Priority(2)]
public class SignInRateLimitingTests(WebApp app) : TestBase
{
    // Each test uses its own unique fake client IPs (203.0.113.50+) so partitions never
    // interfere across tests. AuthStrictPermitLimit (2) < AnonymousPermitLimit (3): a 429 on
    // request 3 can only come from the auth-strict policy attached to the sign-in endpoint.

    private const string UnknownUsername = "ghost.user@example.com";
    private const string WrongPassword = "Wrong-Password-123!";

    private HttpClient CreateClientWithIp(string ip)
    {
        var client = app.CreateClient();
        client.DefaultRequestHeaders.Add(TestClientIpStartupFilter.HeaderName, ip);

        return client;
    }

    private static Task<HttpResponseMessage> PostSignInAsync(
        HttpClient client,
        string username,
        string password)
    {
        return client.PostAsJsonAsync(
            SignInUri,
            new SignInRequest(username, password),
            TestContext.Current.CancellationToken);
    }

    private static async Task<HttpResponseMessage> ExhaustAuthStrictLimitAsync(
        HttpClient client,
        string username,
        string password)
    {
        for (var i = 0; i < AuthStrictPermitLimit; i++)
        {
            var allowed = await PostSignInAsync(client, username, password);
            allowed.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        return await PostSignInAsync(client, username, password);
    }

    [Fact]
    [Priority(1)]
    public async Task ExceedingAuthStrictLimit_ShouldReturn429WithRetryAfterAndGenericBody()
    {
        const string clientIp = "203.0.113.50";
        using var client = CreateClientWithIp(clientIp);

        var rejected = await ExhaustAuthStrictLimitAsync(client, UnknownUsername, WrongPassword);

        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        rejected.Headers.RetryAfter.ShouldNotBeNull();
        rejected.Headers.RetryAfter.Delta.ShouldNotBeNull();
        rejected.Headers.RetryAfter.Delta.Value.TotalSeconds.ShouldBeGreaterThan(0);

        var body = await rejected.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        using var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("title").GetString().ShouldBe(ExpectedTitle);
        json.RootElement.GetProperty("detail").GetString().ShouldBe(ExpectedDetail);
        json.RootElement.GetProperty("status").GetInt32().ShouldBe(429);

        // No information leakage: partition key, IP, limits and username must not appear.
        body.ShouldNotContain(clientIp);
        body.ShouldNotContain("PermitLimit");
        body.ShouldNotContain("ip:");
        body.ShouldNotContain(UnknownUsername);
    }

    [Fact]
    [Priority(2)]
    public async Task DifferentClientIps_ShouldHaveIsolatedSignInPartitions()
    {
        using var exhaustedClient = CreateClientWithIp("203.0.113.51");
        using var freshClient = CreateClientWithIp("203.0.113.52");

        var rejected = await ExhaustAuthStrictLimitAsync(exhaustedClient, UnknownUsername, WrongPassword);
        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        var freshResponse = await PostSignInAsync(freshClient, UnknownUsername, WrongPassword);
        freshResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Priority(3)]
    public async Task ThrottledResponses_ShouldNotRevealAccountExistence()
    {
        var (registeredUser, _) = RegisteredUsersData.Generate()[2];

        using var registeredClient = CreateClientWithIp("203.0.113.53");
        using var unknownClient = CreateClientWithIp("203.0.113.54");

        var rejectedRegistered = await ExhaustAuthStrictLimitAsync(
            registeredClient, registeredUser.Username, WrongPassword);
        var rejectedUnknown = await ExhaustAuthStrictLimitAsync(
            unknownClient, UnknownUsername, WrongPassword);

        rejectedRegistered.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        rejectedUnknown.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        var registeredBody = await rejectedRegistered.Content
            .ReadAsStringAsync(TestContext.Current.CancellationToken);
        var unknownBody = await rejectedUnknown.Content
            .ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Identical rejection contract whether or not the account exists (traceId differs).
        using var registeredJson = JsonDocument.Parse(registeredBody);
        using var unknownJson = JsonDocument.Parse(unknownBody);

        registeredJson.RootElement.GetProperty("title").GetString()
            .ShouldBe(unknownJson.RootElement.GetProperty("title").GetString());
        registeredJson.RootElement.GetProperty("detail").GetString()
            .ShouldBe(unknownJson.RootElement.GetProperty("detail").GetString());
        registeredJson.RootElement.GetProperty("status").GetInt32()
            .ShouldBe(unknownJson.RootElement.GetProperty("status").GetInt32());

        registeredBody.ShouldNotContain(registeredUser.Username);
        unknownBody.ShouldNotContain(UnknownUsername);
    }

    [Fact]
    [Priority(4)]
    public async Task ValidCredentialsAfterLimitExceeded_ShouldStillReturn429()
    {
        var (user, password) = RegisteredUsersData.Generate()[3];
        using var client = CreateClientWithIp("203.0.113.55");

        var rejected = await ExhaustAuthStrictLimitAsync(client, user.Username, WrongPassword);
        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        // The limiter rejects before credentials are checked — a throttled attacker cannot
        // use a non-429 response as an oracle for a correct guess.
        var validAttempt = await PostSignInAsync(client, user.Username, password);
        validAttempt.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }
}
