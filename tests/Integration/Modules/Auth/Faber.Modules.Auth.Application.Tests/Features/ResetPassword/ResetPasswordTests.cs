using System.Net;
using Faber.Modules.Auth.Application.Features.ResetPassword;
using Faber.Modules.Auth.Application.Features.SignIn;
using Faber.Modules.Auth.Application.Tests.Features.ResetPassword.Data;
using Faber.Modules.Identity.Domain.Enums;
using Faber.Modules.Identity.PublicApi;
using Faber.Modules.Identity.PublicApi.Shared;
using Faber.Modules.Users.PublicApi.Contracts;
using FastEndpoints;
using FastEndpoints.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using static Faber.Modules.Auth.Application.Tests.Features.ResetPassword.ResetPasswordConstants;

namespace Faber.Modules.Auth.Application.Tests.Features.ResetPassword;

[Collection<CollectionAuth>]
[Priority(7)]
public class ResetPasswordTests(WebApp app) : TestBase
{
    private async Task<CombinedKey> CreateResetTokenAsync(string email, CancellationToken ct)
    {
        using var scope = app.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityModuleApi>();
        var token = identityApi.GenerateToken();

        var response = await identityApi.TryStoreActionTokenAsync(
            email,
            token,
            10,
            ActionTokenType.ForgotPassword,
            ct);

        return new CombinedKey($"{response.Selector}{token}");
    }

    [Theory]
    [Priority(1)]
    [InlineData("", "", "")]
    [InlineData(" ", " ", " ")]
    public async Task EmptyAllFields_ShouldReturnBadRequest(
        string combinedKeyValue,
        string newPassword,
        string confirmNewPassword)
    {
        var request = new ResetPasswordRequest(
            new CombinedKey(combinedKeyValue),
            newPassword,
            confirmNewPassword);

        var (httpResponse, response) =
            await app.Client.PUTAsync<ResetPasswordEndpoint, ResetPasswordRequest, ErrorResponse>(request);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.StatusCode.ShouldBe((int)HttpStatusCode.BadRequest);
        response.Errors.Count.ShouldBeGreaterThanOrEqualTo(MinValidationErrorsForAllFields);
    }

    [Theory]
    [Priority(2)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task EmptyCombinedKey_ShouldReturnBadRequest(string combinedKeyValue)
    {
        var request = new ResetPasswordRequest(
            new CombinedKey(combinedKeyValue),
            ValidPassword,
            ValidPassword);

        var (httpResponse, response) =
            await app.Client.PUTAsync<ResetPasswordEndpoint, ResetPasswordRequest, ErrorResponse>(request);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Errors.ShouldContainKey("combinedKey.Value");
    }

    [Theory]
    [Priority(3)]
    [ClassData(typeof(TooShortCombinedKeyData))]
    public async Task CombinedKeyTooShort_ShouldReturnBadRequest(
        string combinedKeyValue,
        string validPassword)
    {
        var request = new ResetPasswordRequest(
            new CombinedKey(combinedKeyValue),
            validPassword,
            validPassword);

        var (httpResponse, response) =
            await app.Client.PUTAsync<ResetPasswordEndpoint, ResetPasswordRequest, ErrorResponse>(request);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Errors.ShouldContainKey("combinedKey.Value");
    }

    [Theory]
    [Priority(4)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task EmptyNewPassword_ShouldReturnBadRequest(string newPassword)
    {
        var request = new ResetPasswordRequest(
            new CombinedKey(new string('a', MinCombinedKeyLength)),
            newPassword,
            ValidPassword);

        var (httpResponse, response) =
            await app.Client.PUTAsync<ResetPasswordEndpoint, ResetPasswordRequest, ErrorResponse>(request);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Errors.ShouldContainKey("newPassword");
    }

    [Theory]
    [Priority(5)]
    [ClassData(typeof(TooShortPasswordData))]
    public async Task NewPasswordTooShort_ShouldReturnBadRequest(
        string newPassword,
        string validCombinedKey)
    {
        var request = new ResetPasswordRequest(
            new CombinedKey(validCombinedKey),
            newPassword,
            newPassword);

        var (httpResponse, response) =
            await app.Client.PUTAsync<ResetPasswordEndpoint, ResetPasswordRequest, ErrorResponse>(request);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Errors.ShouldContainKey("newPassword");
    }

    [Theory]
    [Priority(6)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task EmptyConfirmNewPassword_ShouldReturnBadRequest(string confirmNewPassword)
    {
        var request = new ResetPasswordRequest(
            new CombinedKey(new string('a', MinCombinedKeyLength)),
            ValidPassword,
            confirmNewPassword);

        var (httpResponse, response) =
            await app.Client.PUTAsync<ResetPasswordEndpoint, ResetPasswordRequest, ErrorResponse>(request);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Errors.ShouldContainKey("confirmNewPassword");
    }

    [Theory]
    [Priority(7)]
    [ClassData(typeof(MismatchedPasswordsData))]
    public async Task PasswordsDoNotMatch_ShouldReturnBadRequest(
        string newPassword,
        string confirmNewPassword,
        string validCombinedKey)
    {
        var request = new ResetPasswordRequest(
            new CombinedKey(validCombinedKey),
            newPassword,
            confirmNewPassword);

        var (httpResponse, response) =
            await app.Client.PUTAsync<ResetPasswordEndpoint, ResetPasswordRequest, ErrorResponse>(request);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Errors.ShouldContainKey("confirmNewPassword");
    }

    [Theory]
    [Priority(8)]
    [ClassData(typeof(InvalidTokenData))]
    public async Task InvalidToken_ShouldReturnBadRequest(
        string invalidCombinedKey,
        string validPassword)
    {
        var request = new ResetPasswordRequest(
            new CombinedKey(invalidCombinedKey),
            validPassword,
            validPassword);

        var httpResponse =
            await app.Client.PUTAsync<ResetPasswordEndpoint, ResetPasswordRequest>(request);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [Priority(9)]
    [ClassData(typeof(RegisteredUsersWithNewCredentialsData))]
    public async Task ValidToken_ShouldReturnNoContent(CreateUserRequest user, string newPassword)
    {
        var combinedKey = await CreateResetTokenAsync(user.Email, TestContext.Current.CancellationToken);
        var request = new ResetPasswordRequest(combinedKey, newPassword, newPassword);

        var httpResponse =
            await app.Client.PUTAsync<ResetPasswordEndpoint, ResetPasswordRequest>(request);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Theory]
    [Priority(10)]
    [ClassData(typeof(RegisteredUsersWithNewCredentialsData))]
    public async Task ValidToken_ShouldAllowSignInWithNewPassword(CreateUserRequest user, string newPassword)
    {
        var combinedKey = await CreateResetTokenAsync(user.Email, TestContext.Current.CancellationToken);
        var resetRequest = new ResetPasswordRequest(combinedKey, newPassword, newPassword);

        var resetResponse =
            await app.Client.PUTAsync<ResetPasswordEndpoint, ResetPasswordRequest>(resetRequest);

        resetResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var signInRequest = new SignInRequest(user.Username, newPassword);

        var (signInHttp, signInResponse) =
            await app.Client.POSTAsync<SignInEndpoint, SignInRequest, SignInResponse>(signInRequest);

        signInHttp.StatusCode.ShouldBe(HttpStatusCode.OK);
        signInResponse.AccessToken.ShouldNotBeNullOrEmpty();
    }

    [Theory]
    [Priority(11)]
    [ClassData(typeof(RegisteredUsersWithNewCredentialsData))]
    public async Task ConsumedToken_ShouldReturnBadRequest(CreateUserRequest user, string newPassword)
    {
        var combinedKey = await CreateResetTokenAsync(user.Email, TestContext.Current.CancellationToken);

        var firstRequest = new ResetPasswordRequest(combinedKey, newPassword, newPassword);

        var firstResponse =
            await app.Client.PUTAsync<ResetPasswordEndpoint, ResetPasswordRequest>(firstRequest);

        firstResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var secondRequest = new ResetPasswordRequest(combinedKey, newPassword, newPassword);

        var secondResponse =
            await app.Client.PUTAsync<ResetPasswordEndpoint, ResetPasswordRequest>(secondRequest);

        secondResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}