namespace Faber.Modules.Users.Application.Features.GetUserByName;

public static class GetUserByNameMapper
{
    public static GetUserByNameCommand MapToCommand(this GetUserByNameRequest request)
    {
        return new GetUserByNameCommand(request.Username);
    }
}