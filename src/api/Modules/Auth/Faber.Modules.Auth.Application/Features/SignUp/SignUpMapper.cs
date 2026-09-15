using Faber.Modules.Users.PublicApi.Contracts;

namespace Faber.Modules.Auth.Application.Features.SignUp;

public static class SignUpMapper
{
    public static SignUpCommand MapToCommand(this SignUpRequest request)
    {
        return new SignUpCommand(
            request.Username,
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);
    }

    public static SignUpResponse MapToResponse(this UserResponse userResponse)
    {
        return new SignUpResponse(
            userResponse.UserId,
            userResponse.Username,
            userResponse.Email,
            userResponse.FirstName,
            userResponse.LastName);
    }
}