using Faber.Modules.Auth.Application.Features.Shared.Requests;

namespace Faber.Modules.Auth.Application.Features.SignOut;

public static class SignOutMapper
{
    public static SignOutCommand MapToCommand(this RefreshTokenRequest request, string refreshToken)
    {
        if (!string.IsNullOrEmpty(request.RefreshToken)) return new SignOutCommand(request.RefreshToken);

        return new SignOutCommand(refreshToken);
    }
}