namespace Faber.Modules.Users.Application.Features.GetUserById;

public static class GetUserByIdMapper
{
    public static GetUserByIdCommand MapToCommand(this GetUserByIdRequest request)
    {
        return new GetUserByIdCommand(request.UserId);
    }
}