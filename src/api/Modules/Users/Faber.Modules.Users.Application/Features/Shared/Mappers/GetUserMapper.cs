using Faber.Modules.Users.Application.Features.Shared.Responses;
using Faber.Modules.Users.PublicApi.Contracts;

namespace Faber.Modules.Users.Application.Features.Shared.Mappers;

public static class GetUserMapper
{
    public static GetUserResponse MapToResponse(this UserResponse getUserResponse)
    {
        return new GetUserResponse(
            getUserResponse.UserId,
            getUserResponse.Username,
            getUserResponse.Email,
            getUserResponse.FirstName,
            getUserResponse.LastName);
    }
}