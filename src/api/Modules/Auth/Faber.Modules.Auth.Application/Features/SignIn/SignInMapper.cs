using Faber.Modules.Identity.PublicApi.Contracts;

namespace Faber.Modules.Auth.Application.Features.SignIn;

public static class SignInMapper
{
    public static SignInResponse MapToResponse(this TokenResponse tokenResponse)
    {
        return new SignInResponse(tokenResponse.AccessToken, tokenResponse.RefreshToken);
    }

    public static SignInCommand MapToCommand(this SignInRequest request)
    {
        return new SignInCommand(request.Username, request.Password);
    }
}