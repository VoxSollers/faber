using System.Net;
using Faber.Modules.Auth.Application.Features.VerifyEmail;
using Faber.Modules.Auth.Application.Tests.Features.VerifyEmail.Data;
using Faber.Modules.Identity.Domain.Enums;
using Faber.Modules.Identity.PublicApi;
using Faber.Modules.Identity.PublicApi.Shared;
using Faber.Modules.Users.PublicApi.Contracts;
using FastEndpoints;
using FastEndpoints.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Faber.Modules.Auth.Application.Tests.Features.VerifyEmail;

[Collection<CollectionAuth>]
[Priority(8)]
public class VerifyEmailTests(WebApp app) : TestBase
{
    private async Task<CombinedKey> CreateVerifyEmailTokenAsync(string email, CancellationToken ct)
    {
        using var scope = app.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityModuleApi>();
        var token = identityApi.GenerateToken();

        var response = await identityApi.TryStoreActionTokenAsync(
            email,
            token,
            10,
            ActionTokenType.VerifyEmail,
            ct);

        return new CombinedKey($"{response.Selector}{token}");
    }

    [Theory]
    [Priority(1)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task EmptyCombinedKey_ShouldReturnBadRequest(string combinedKeyValue)
    {
        var request = new VerifyEmailRequest(new CombinedKey(combinedKeyValue));

        var (httpResponse, response) =
            await app.Client.POSTAsync<VerifyEmailEndpoint, VerifyEmailRequest, ErrorResponse>(request);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Errors.ShouldContainKey("combinedKey.Value");
    }

    [Theory]
    [Priority(2)]
    [ClassData(typeof(TooShortCombinedKeyData))]
    public async Task CombinedKeyTooShort_ShouldReturnBadRequest(string combinedKeyValue)
    {
        var request = new VerifyEmailRequest(new CombinedKey(combinedKeyValue));

        var (httpResponse, response) =
            await app.Client.POSTAsync<VerifyEmailEndpoint, VerifyEmailRequest, ErrorResponse>(request);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Errors.ShouldContainKey("combinedKey.Value");
    }

    [Theory]
    [Priority(3)]
    [ClassData(typeof(InvalidTokenData))]
    public async Task InvalidToken_ShouldReturnBadRequest(string invalidCombinedKey)
    {
        var request = new VerifyEmailRequest(new CombinedKey(invalidCombinedKey));

        var httpResponse =
            await app.Client.POSTAsync<VerifyEmailEndpoint, VerifyEmailRequest>(request);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [Priority(4)]
    [ClassData(typeof(RegisteredUsersEmailData))]
    public async Task ValidToken_ShouldReturnOkWithIsVerified(CreateUserRequest user)
    {
        var combinedKey = await CreateVerifyEmailTokenAsync(
            user.Email,
            TestContext.Current.CancellationToken);

        var request = new VerifyEmailRequest(combinedKey);

        var (httpResponse, response) =
            await app.Client.POSTAsync<VerifyEmailEndpoint, VerifyEmailRequest, VerifyEmailResponse>(request);

        httpResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.IsVerified.ShouldBeTrue();
    }

    [Theory]
    [Priority(5)]
    [ClassData(typeof(RegisteredUsersEmailData))]
    public async Task ConsumedToken_ShouldReturnBadRequest(CreateUserRequest user)
    {
        var combinedKey = await CreateVerifyEmailTokenAsync(
            user.Email,
            TestContext.Current.CancellationToken);

        var request = new VerifyEmailRequest(combinedKey);

        var firstResponse =
            await app.Client.POSTAsync<VerifyEmailEndpoint, VerifyEmailRequest>(request);

        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var secondResponse =
            await app.Client.POSTAsync<VerifyEmailEndpoint, VerifyEmailRequest>(request);

        secondResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}