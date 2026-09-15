using System.Net;
using Faber.Modules.Auth.Application.Features.Shared.Requests;
using Faber.Modules.Auth.Application.Features.SignIn;
using Faber.Testing.Shared.Data;
using Faber.Testing.Shared.Http;
using Faber.Modules.Auth.Domain.Enums;
using FastEndpoints;
using FastEndpoints.Testing;
using Shouldly;
using static Faber.Modules.Auth.Application.Tests.Features.SignOut.SignOutConstants;

namespace Faber.Modules.Auth.Application.Tests.Features.SignOut;

[Collection<CollectionAuth>]
[Priority(2)]
public class SignOutTests(WebApp app) : TestBase
{
    private static Dictionary<string, string> WebHeaders => new() { { ClientTypeHeader, nameof(ClientType.Web) } };

    private static Dictionary<string, string> MobileHeaders =>
        new() { { ClientTypeHeader, nameof(ClientType.Mobile) } };

    private async Task<SignInResponse> SignInAsync()
    {
        var (user, password) = RegisteredUsersData.Generate().First();

        var (httpResponse, response) =
            await app.Client.POSTAsync<SignInEndpoint, SignInRequest, SignInResponse>(
                new SignInRequest(user.Username, password));

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        return response;
    }

    [Fact]
    [Priority(1)]
    public async Task WebClient_EmptyRefreshTokenCookie_ShouldReturnUnauthorized()
    {
        var request = new RefreshTokenRequest(null, nameof(ClientType.Web));

        var httpResponse = await app.Client.PostWithCookiesAsync(
            RequestUri,
            request,
            new Cookie(RefreshTokenCookie, string.Empty),
            WebHeaders,
            TestContext.Current.CancellationToken);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Priority(2)]
    public async Task WebClient_NoCookie_ShouldReturnUnauthorized()
    {
        var request = new RefreshTokenRequest(null, nameof(ClientType.Web));

        var httpResponse = await app.Client.PostWithHeadersAsync(
            RequestUri,
            request,
            WebHeaders,
            TestContext.Current.CancellationToken);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [Priority(3)]
    [InlineData(null)]
    [InlineData("")]
    public async Task MobileClient_EmptyOrNullRefreshToken_ShouldReturnUnauthorized(string? refreshToken)
    {
        var request = new RefreshTokenRequest(refreshToken, nameof(ClientType.Mobile));

        var httpResponse = await app.Client.PostWithHeadersAsync(
            RequestUri,
            request,
            MobileHeaders,
            TestContext.Current.CancellationToken);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Priority(4)]
    public async Task InvalidClientType_ShouldReturnUnauthorized()
    {
        var request = new RefreshTokenRequest(SomeToken, InvalidClientType);

        var httpResponse = await app.Client.PostWithHeadersAsync(
            RequestUri,
            request,
            new Dictionary<string, string> { { ClientTypeHeader, InvalidClientType } },
            TestContext.Current.CancellationToken);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Priority(5)]
    public async Task MissingClientTypeHeader_ShouldReturnBadRequest()
    {
        var request = new RefreshTokenRequest(SomeToken, "");

        var httpResponse = await app.Client.PostWithHeadersAsync(
            RequestUri,
            request,
            ct: TestContext.Current.CancellationToken);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Priority(6)]
    public async Task EmptyClientTypeHeader_ShouldReturnBadRequest()
    {
        var request = new RefreshTokenRequest(SomeToken, "");

        var httpResponse = await app.Client.PostWithHeadersAsync(
            RequestUri,
            request,
            new Dictionary<string, string> { { ClientTypeHeader, "" } },
            TestContext.Current.CancellationToken);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Priority(7)]
    public async Task WebClient_InvalidRefreshToken_ShouldReturnBadRequest()
    {
        var request = new RefreshTokenRequest(null, nameof(ClientType.Web));

        var httpResponse = await app.Client.PostWithCookiesAsync(
            RequestUri,
            request,
            new Cookie(RefreshTokenCookie, InvalidRefreshToken),
            WebHeaders,
            TestContext.Current.CancellationToken);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Priority(8)]
    public async Task MobileClient_InvalidRefreshToken_ShouldReturnBadRequest()
    {
        var request = new RefreshTokenRequest(InvalidRefreshToken, nameof(ClientType.Mobile));

        var httpResponse = await app.Client.PostWithHeadersAsync(
            RequestUri,
            request,
            MobileHeaders,
            TestContext.Current.CancellationToken);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Priority(9)]
    public async Task WebClient_ValidRefreshToken_ShouldReturnNoContent()
    {
        var signIn = await SignInAsync();
        var request = new RefreshTokenRequest(null, nameof(ClientType.Web));

        var httpResponse = await app.Client.PostWithCookiesAsync(
            RequestUri,
            request,
            new Cookie(RefreshTokenCookie, signIn.RefreshToken),
            WebHeaders,
            TestContext.Current.CancellationToken);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var setCookieHeaders = httpResponse.Headers
            .Where(h => h.Key.Equals(SetCookieHeader, StringComparison.OrdinalIgnoreCase))
            .SelectMany(h => h.Value)
            .ToList();

        setCookieHeaders.ShouldContain(c => c.StartsWith(RefreshTokenCookie));
    }

    [Fact]
    [Priority(10)]
    public async Task MobileClient_ValidRefreshToken_ShouldReturnNoContent()
    {
        var signIn = await SignInAsync();
        var request = new RefreshTokenRequest(signIn.RefreshToken, nameof(ClientType.Mobile));

        var httpResponse = await app.Client.PostWithHeadersAsync(
            RequestUri,
            request,
            MobileHeaders,
            TestContext.Current.CancellationToken);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    [Priority(11)]
    public async Task WebClient_SignOutTwiceWithSameToken_ShouldBeIdempotent()
    {
        var signIn = await SignInAsync();
        var request = new RefreshTokenRequest(null, nameof(ClientType.Web));
        var cookie = new Cookie(RefreshTokenCookie, signIn.RefreshToken);

        var first = await app.Client.PostWithCookiesAsync(
            RequestUri,
            request,
            cookie,
            WebHeaders,
            TestContext.Current.CancellationToken);

        first.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var second = await app.Client.PostWithCookiesAsync(
            RequestUri,
            request,
            cookie,
            WebHeaders,
            TestContext.Current.CancellationToken);

        second.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
}