using System.Net;
using Faber.Modules.Auth.Application.Features.SignIn;
using Faber.Testing.Shared.Data;
using Faber.Modules.Auth.Application.Tests.Features.SignIn.Data;
using Faber.Modules.Users.PublicApi.Contracts;
using FastEndpoints;
using FastEndpoints.Testing;
using Shouldly;
using static Faber.Modules.Auth.Application.Tests.Features.SignIn.SignInConstants;

namespace Faber.Modules.Auth.Application.Tests.Features.SignIn;

[Collection<CollectionAuth>]
[Priority(1)]
public class SignInTests(WebApp app) : TestBase
{
    [Theory]
    [Priority(1)]
    [InlineData("", "")]
    [InlineData("  ", "  ")]
    public async Task EmptyUsernameAndPassword_ShouldReturnBadRequest(string username, string password)
    {
        var request = new SignInRequest(username, password);

        var (httpResponse, response) =
            await app.Client.POSTAsync<SignInEndpoint, SignInRequest, ErrorResponse>(request);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.StatusCode.ShouldBe((int)HttpStatusCode.BadRequest);
        response.Errors.Count.ShouldBeGreaterThanOrEqualTo(MinValidationErrorsForBothFields);
    }

    [Theory]
    [Priority(2)]
    [InlineData("", "ValidPass1")]
    [InlineData("  ", "ValidPass1")]
    public async Task EmptyUsername_ShouldReturnBadRequest(string username, string password)
    {
        var request = new SignInRequest(username, password);

        var (httpResponse, response) =
            await app.Client.POSTAsync<SignInEndpoint, SignInRequest, ErrorResponse>(request);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.StatusCode.ShouldBe((int)HttpStatusCode.BadRequest);
        response.Errors.ShouldContainKey("username");
    }

    [Theory]
    [Priority(3)]
    [InlineData("validuser", "")]
    [InlineData("validuser", "  ")]
    public async Task EmptyPassword_ShouldReturnBadRequest(string username, string password)
    {
        var request = new SignInRequest(username, password);

        var (httpResponse, response) =
            await app.Client.POSTAsync<SignInEndpoint, SignInRequest, ErrorResponse>(request);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.StatusCode.ShouldBe((int)HttpStatusCode.BadRequest);
        response.Errors.ShouldContainKey("password");
    }

    [Theory]
    [Priority(4)]
    [InlineData("validuser", "short")]
    [InlineData("validuser", "1234567")]
    public async Task PasswordTooShort_ShouldReturnBadRequest(string username, string password)
    {
        var request = new SignInRequest(username, password);

        var (httpResponse, response) =
            await app.Client.POSTAsync<SignInEndpoint, SignInRequest, ErrorResponse>(request);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.StatusCode.ShouldBe((int)HttpStatusCode.BadRequest);
        response.Errors.ShouldContainKey("password");
    }

    [Theory]
    [Priority(5)]
    [ClassData(typeof(UnregisteredUsersData))]
    public async Task UnregisteredUser_ShouldReturnUnauthorized(SignInRequest request)
    {
        var httpResponse =
            await app.Client.POSTAsync<SignInEndpoint, SignInRequest>(request);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [Priority(6)]
    [ClassData(typeof(RegisteredUsersData))]
    public async Task RegisteredUserWrongPassword_ShouldReturnUnauthorized(
        CreateUserRequest userRequest,
        string password)
    {
        var wrongPassword = password + WrongPasswordSuffix;
        var request = new SignInRequest(userRequest.Username, wrongPassword);

        var httpResponse =
            await app.Client.POSTAsync<SignInEndpoint, SignInRequest>(request);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [Priority(7)]
    [ClassData(typeof(RegisteredUsersData))]
    public async Task RegisteredUserValidCredentials_ShouldReturnOk(
        CreateUserRequest userRequest,
        string password)
    {
        var request = new SignInRequest(userRequest.Username, password);

        var (httpResponse, response) =
            await app.Client.POSTAsync<SignInEndpoint, SignInRequest, SignInResponse>(request);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.AccessToken.ShouldNotBeNullOrEmpty();
        response.RefreshToken.ShouldNotBeNullOrEmpty();
    }

    [Theory]
    [Priority(8)]
    [ClassData(typeof(RegisteredUsersData))]
    public async Task RegisteredUserValidCredentials_ShouldSetRefreshTokenCookie(
        CreateUserRequest userRequest,
        string password)
    {
        var request = new SignInRequest(userRequest.Username, password);

        var (httpResponse, _) =
            await app.Client.POSTAsync<SignInEndpoint, SignInRequest, SignInResponse>(request);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var setCookieHeaders = httpResponse.Headers
            .Where(h => h.Key.Equals(SetCookieHeader, StringComparison.OrdinalIgnoreCase))
            .SelectMany(h => h.Value)
            .ToList();

        setCookieHeaders.ShouldContain(c => c.StartsWith(RefreshTokenCookie));

        var refreshTokenCookie = setCookieHeaders
            .First(c => c.StartsWith(RefreshTokenCookie));

        refreshTokenCookie.IndexOf(HttpOnlyAttribute, StringComparison.OrdinalIgnoreCase)
            .ShouldBeGreaterThanOrEqualTo(0);

        refreshTokenCookie.IndexOf(PathAttribute, StringComparison.OrdinalIgnoreCase).ShouldBeGreaterThanOrEqualTo(0);

        refreshTokenCookie.IndexOf(SecureAttribute, StringComparison.OrdinalIgnoreCase)
            .ShouldBeGreaterThanOrEqualTo(0, "Outside LAN mode the refresh cookie must stay Secure");

        refreshTokenCookie.IndexOf(SameSiteNoneAttribute, StringComparison.OrdinalIgnoreCase)
            .ShouldBeGreaterThanOrEqualTo(0, "Outside LAN mode the refresh cookie must stay SameSite=None");
    }

    [Theory]
    [Priority(9)]
    [ClassData(typeof(RegisteredUsersData))]
    public async Task RegisteredUserValidCredentials_AccessTokenShouldBeJwt(
        CreateUserRequest userRequest,
        string password)
    {
        var request = new SignInRequest(userRequest.Username, password);

        var (httpResponse, response) =
            await app.Client.POSTAsync<SignInEndpoint, SignInRequest, SignInResponse>(request);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var parts = response.AccessToken.Split('.');
        parts.Length.ShouldBe(JwtPartsCount, "Access token should be a valid JWT with 3 parts");
    }

    [Fact]
    [Priority(10)]
    public async Task SignInTwiceWithSameCredentials_ShouldReturnDifferentTokens()
    {
        var (user, password) = RegisteredUsersData.Generate().First();
        var request = new SignInRequest(user.Username, password);

        var (firstHttp, firstResponse) =
            await app.Client.POSTAsync<SignInEndpoint, SignInRequest, SignInResponse>(request);

        var (secondHttp, secondResponse) =
            await app.Client.POSTAsync<SignInEndpoint, SignInRequest, SignInResponse>(request);

        firstHttp.StatusCode.ShouldBe(HttpStatusCode.OK);
        secondHttp.StatusCode.ShouldBe(HttpStatusCode.OK);

        firstResponse.AccessToken.ShouldNotBe(secondResponse.AccessToken);
    }
}