using Faber.Modules.Auth.Application.Features.SignIn;
using Faber.Modules.Resumes.Application.Features.Resumes.CreateResume;
using Faber.Testing.Shared.Data;
using Faber.Testing.Shared.Http;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Tests.Features.Shared;

public static class ResumesTestHelper
{
    public static async Task<string> SignInAsFirstUserAsync(HttpClient client, CancellationToken ct = default)
    {
        return await SignInAsUserAsync(client, 0, ct);
    }

    public static async Task<string> SignInAsUserAsync(HttpClient client, int userIndex, CancellationToken ct = default)
    {
        var (user, password) = RegisteredUsersData.Generate()[userIndex];
        var signInRequest = new SignInRequest(user.Username, password);

        var (_, signInResponse) = await client
            .POSTAsync<SignInEndpoint, SignInRequest, SignInResponse>(signInRequest);

        return signInResponse!.AccessToken;
    }

    public static async Task<CreateResumeResponse> CreateResumeAsync(
        HttpClient client,
        string accessToken,
        string localization = "en-us",
        CancellationToken ct = default)
    {
        var (_, response) = await client
            .WithAuthToken(accessToken)
            .POSTAsync<CreateResumeEndpoint, CreateResumeRequest, CreateResumeResponse>(
                new CreateResumeRequest(localization));

        return response!;
    }
}
