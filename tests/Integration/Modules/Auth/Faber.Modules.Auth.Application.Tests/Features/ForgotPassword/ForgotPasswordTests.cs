using System.Net;
using Faber.Modules.Auth.Application.Features.ForgotPassword;
using Faber.Modules.Auth.Application.Tests.Features.ForgotPassword.Data;
using Faber.Testing.Shared.Data;
using Faber.Testing.Shared.Http;
using Faber.Modules.Users.PublicApi.Contracts;
using FastEndpoints;
using FastEndpoints.Testing;
using Shouldly;
using static Faber.Modules.Auth.Application.Tests.Features.ForgotPassword.ForgotPasswordConstants;

namespace Faber.Modules.Auth.Application.Tests.Features.ForgotPassword;

[Collection<CollectionAuth>]
[Priority(4)]
public class ForgotPasswordTests(WebApp app) : TestBase
{
    [Theory]
    [Priority(1)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task EmptyEmail_ShouldReturnBadRequest(string email)
    {
        var request = new ForgotPasswordRequest(email);

        var (httpResponse, response) =
            await app.Client.POSTAsync<ForgotPasswordEndpoint, ForgotPasswordRequest, ErrorResponse>(request);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.StatusCode.ShouldBe((int)HttpStatusCode.BadRequest);
        response.Errors.Count.ShouldBeGreaterThanOrEqualTo(MinValidationErrorsForEmail);
    }

    [Theory]
    [Priority(2)]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@missing.com")]
    public async Task InvalidEmailFormat_ShouldReturnBadRequest(string email)
    {
        var request = new ForgotPasswordRequest(email);

        var (httpResponse, response) =
            await app.Client.POSTAsync<ForgotPasswordEndpoint, ForgotPasswordRequest, ErrorResponse>(request);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.StatusCode.ShouldBe((int)HttpStatusCode.BadRequest);
        response.Errors.ShouldContainKey("email");
    }

    [Theory]
    [Priority(3)]
    [InlineData("nonexistent@example.com")]
    [InlineData("unknown_user@test.org")]
    public async Task UnregisteredEmail_ShouldReturnNoContent(string email)
    {
        var request = new ForgotPasswordRequest(email);

        var httpResponse = await app.Client.PostWithHeadersAsync(
            RequestUri,
            request,
            ct: TestContext.Current.CancellationToken);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Theory]
    [Priority(4)]
    [ClassData(typeof(RegisteredUsersEmailData))]
    public async Task RegisteredUserEmail_ShouldReturnNoContent(CreateUserRequest userRequest)
    {
        var request = new ForgotPasswordRequest(userRequest.Email);

        var httpResponse = await app.Client.PostWithHeadersAsync(
            RequestUri,
            request,
            ct: TestContext.Current.CancellationToken);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    [Priority(5)]
    public async Task ForgotPasswordTwice_ShouldReturnNoContent()
    {
        var (user, _) = RegisteredUsersData.Generate().First();
        var request = new ForgotPasswordRequest(user.Email);

        var firstResponse = await app.Client.PostWithHeadersAsync(
            RequestUri,
            request,
            ct: TestContext.Current.CancellationToken);

        var secondResponse = await app.Client.PostWithHeadersAsync(
            RequestUri,
            request,
            ct: TestContext.Current.CancellationToken);

        firstResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
}