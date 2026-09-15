namespace Faber.Modules.Users.Application.Features.GetUserByEmail;

public static class GetUserByEmailMapper
{
    public static GetUserByEmailCommand MapToCommand(this GetUserByEmailRequest request)
    {
        return new GetUserByEmailCommand(request.Email);
    }
}