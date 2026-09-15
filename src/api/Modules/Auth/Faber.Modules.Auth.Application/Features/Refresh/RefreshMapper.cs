using Faber.Modules.Auth.Application.Features.Shared.Requests;
using Faber.Modules.Identity.PublicApi.Contracts;

namespace Faber.Modules.Auth.Application.Features.Refresh;

public static class RefreshMapper
{
    public static RefreshResponse MapToResponse(this TokenResponse tokenResponse)
    {
        return new RefreshResponse(tokenResponse.AccessToken, tokenResponse.RefreshToken);
    }

    public static RefreshCommand MapToCommand(this RefreshTokenRequest request, string refreshToken)
    {
        if (!string.IsNullOrEmpty(request.RefreshToken)) return new RefreshCommand(request.RefreshToken);

        return new RefreshCommand(refreshToken);
    }
}