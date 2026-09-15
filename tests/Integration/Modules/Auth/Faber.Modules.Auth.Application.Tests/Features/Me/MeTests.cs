using System.Net;
using Faber.Modules.Auth.Application.Features.Me;
using Faber.Modules.Auth.Application.Features.SignIn;
using Faber.Testing.Shared.Data;
using Faber.Testing.Shared.Http;
using Faber.Modules.Users.PublicApi.Contracts;
using FastEndpoints;
using FastEndpoints.Testing;
using Shouldly;
using static Faber.Modules.Auth.Application.Tests.Features.Me.MeConstants;

namespace Faber.Modules.Auth.Application.Tests.Features.Me;

[Collection<CollectionAuth>]
[Priority(5)]
public class MeTests(WebApp app) : TestBase
{
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
    public async Task NoAuthorizationHeader_ShouldReturnUnauthorized()
    {
        var httpResponse = await app.Client.GetWithHeadersAsync(RequestUri, ct: TestContext.Current.CancellationToken);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Priority(2)]
    public async Task InvalidBearerToken_ShouldReturnUnauthorized()
    {
        var httpResponse = await app.Client.GetWithHeadersAsync(
            RequestUri,
            new Dictionary<string, string> { { AuthorizationHeader, InvalidToken } },
            TestContext.Current.CancellationToken);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [Priority(3)]
    [ClassData(typeof(RegisteredUsersData))]
    public async Task ValidToken_ShouldReturnOk(CreateUserRequest userRequest, string password)
    {
        var signIn = await SignInAsync(userRequest.Username, password);

        var httpResponse = await app.Client.GetWithHeadersAsync(
            RequestUri,
            new Dictionary<string, string> { { AuthorizationHeader, $"Bearer {signIn.AccessToken}" } },
            TestContext.Current.CancellationToken);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Theory]
    [Priority(4)]
    [ClassData(typeof(RegisteredUsersData))]
    public async Task ValidToken_ShouldReturnCorrectUserData(CreateUserRequest userRequest, string password)
    {
        var signIn = await SignInAsync(userRequest.Username, password);

        var (httpResponse, response) = await app.Client.GetWithHeadersAsync<MeResponse>(
            RequestUri,
            new Dictionary<string, string> { { AuthorizationHeader, $"Bearer {signIn.AccessToken}" } },
            TestContext.Current.CancellationToken);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.ShouldNotBeNull();
        response.Id.ShouldNotBeNullOrEmpty();
        response.Username.ShouldBe(userRequest.Username, StringCompareShould.IgnoreCase);
        response.Email.ShouldBe(userRequest.Email, StringCompareShould.IgnoreCase);
        response.FirstName.ShouldBe(userRequest.FirstName);
        response.LastName.ShouldBe(userRequest.LastName);
    }
}