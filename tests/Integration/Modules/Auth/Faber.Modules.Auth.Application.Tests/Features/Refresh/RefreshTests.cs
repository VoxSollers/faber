using System.Net;
using Faber.Modules.Auth.Application.Features.Refresh;
using Faber.Modules.Auth.Application.Features.Shared.Requests;
using Faber.Modules.Auth.Application.Features.SignIn;
using Faber.Testing.Shared.Data;
using Faber.Testing.Shared.Http;
using Faber.Modules.Auth.Domain.Enums;
using Faber.Modules.Users.PublicApi.Contracts;
using FastEndpoints;
using FastEndpoints.Testing;
using Shouldly;
using static Faber.Modules.Auth.Application.Tests.Features.Refresh.RefreshConstants;

namespace Faber.Modules.Auth.Application.Tests.Features.Refresh;

[Collection<CollectionAuth>]
[Priority(6)]
public class RefreshTests(WebApp app) : TestBase
{
    private static Dictionary<string, string> WebHeaders => new() { { ClientTypeHeader, nameof(ClientType.Web) } };

    private static Dictionary<string, string> MobileHeaders =>
        new() { { ClientTypeHeader, nameof(ClientType.Mobile) } };

    private async Task<SignInResponse> SignInAsync(string username, string password)
    {
        var request = new SignInRequest(username, password);

        var (httpResponse, response) =
            await app.Client.POSTAsync<SignInEndpoint, SignInRequest, SignInResponse>(request);

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

    [Theory]
    [Priority(9)]
    [ClassData(typeof(RegisteredUsersData))]
    public async Task WebClient_ValidRefreshToken_ShouldReturnOkWithNewTokens(
        CreateUserRequest userRequest,
        string password)
    {
        var signIn = await SignInAsync(userRequest.Username, password);
        var request = new RefreshTokenRequest(null, nameof(ClientType.Web));

        var (httpResponse, response) = await app.Client.PostWithCookiesAsync<RefreshTokenRequest, RefreshResponse>(
            RequestUri,
            request,
            new Cookie(RefreshTokenCookie, signIn.RefreshToken),
            WebHeaders,
            TestContext.Current.CancellationToken);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.ShouldNotBeNull();
        response.AccessToken.ShouldNotBeNullOrEmpty();
        response.RefreshToken.ShouldNotBeNullOrEmpty();

        var setCookieHeaders = httpResponse.Headers
            .Where(h => h.Key.Equals(SetCookieHeader, StringComparison.OrdinalIgnoreCase))
            .SelectMany(h => h.Value)
            .ToList();

        setCookieHeaders.ShouldContain(c => c.StartsWith(RefreshTokenCookie));
    }

    [Theory]
    [Priority(10)]
    [ClassData(typeof(RegisteredUsersData))]
    public async Task MobileClient_ValidRefreshToken_ShouldReturnOkWithNewTokens(
        CreateUserRequest userRequest,
        string password)
    {
        var signIn = await SignInAsync(userRequest.Username, password);
        var request = new RefreshTokenRequest(signIn.RefreshToken, nameof(ClientType.Mobile));

        var (httpResponse, response) = await app.Client.PostWithHeadersAsync<RefreshTokenRequest, RefreshResponse>(
            RequestUri,
            request,
            MobileHeaders,
            TestContext.Current.CancellationToken);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.ShouldNotBeNull();
        response.AccessToken.ShouldNotBeNullOrEmpty();
        response.RefreshToken.ShouldNotBeNullOrEmpty();
    }

    [Theory]
    [Priority(11)]
    [ClassData(typeof(RegisteredUsersData))]
    public async Task ValidRefreshToken_AccessTokenShouldBeJwt(
        CreateUserRequest userRequest,
        string password)
    {
        var signIn = await SignInAsync(userRequest.Username, password);
        var request = new RefreshTokenRequest(signIn.RefreshToken, nameof(ClientType.Mobile));

        var (httpResponse, response) = await app.Client.PostWithHeadersAsync<RefreshTokenRequest, RefreshResponse>(
            RequestUri,
            request,
            MobileHeaders,
            TestContext.Current.CancellationToken);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.ShouldNotBeNull();

        var parts = response.AccessToken.Split('.');
        parts.Length.ShouldBe(JwtPartsCount, "Access token should be a valid JWT with 3 parts");
    }

    [Theory]
    [Priority(12)]
    [ClassData(typeof(RegisteredUsersData))]
    public async Task RefreshTwice_ShouldReturnDifferentTokens(
        CreateUserRequest userRequest,
        string password)
    {
        var signIn = await SignInAsync(userRequest.Username, password);

        var firstRequest = new RefreshTokenRequest(signIn.RefreshToken, nameof(ClientType.Mobile));

        var (firstHttp, firstResponse) = await app.Client.PostWithHeadersAsync<RefreshTokenRequest, RefreshResponse>(
            RequestUri,
            firstRequest,
            MobileHeaders,
            TestContext.Current.CancellationToken);

        firstHttp.StatusCode.ShouldBe(HttpStatusCode.OK);
        firstResponse.ShouldNotBeNull();

        var secondRequest = new RefreshTokenRequest(firstResponse.RefreshToken, nameof(ClientType.Mobile));

        var (secondHttp, secondResponse) = await app.Client.PostWithHeadersAsync<RefreshTokenRequest, RefreshResponse>(
            RequestUri,
            secondRequest,
            MobileHeaders,
            TestContext.Current.CancellationToken);

        secondHttp.StatusCode.ShouldBe(HttpStatusCode.OK);
        secondResponse.ShouldNotBeNull();

        firstResponse.AccessToken.ShouldNotBe(secondResponse.AccessToken);
        firstResponse.RefreshToken.ShouldNotBe(secondResponse.RefreshToken);
    }
}