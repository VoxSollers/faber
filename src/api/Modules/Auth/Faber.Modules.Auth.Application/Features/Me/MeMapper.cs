using Faber.Modules.Users.PublicApi.Contracts;

namespace Faber.Modules.Auth.Application.Features.Me;

public static class MeMapper
{
    public static MeResponse MapToResponse(this UserResponse userResponse)
    {
        return new MeResponse(
            userResponse.UserId,
            userResponse.Username,
            userResponse.Email,
            userResponse.FirstName,
            userResponse.LastName);
    }
}