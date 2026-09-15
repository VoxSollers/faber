namespace Faber.Modules.Users.Application.Features.UpdateUserFullName;

public static class UpdateUserFullNameMapper
{
    public static UpdateUserFullNameCommand MapToCommand(this UpdateUserFullNameRequest fullNameRequest)
    {
        return new UpdateUserFullNameCommand(
            fullNameRequest.UserId,
            fullNameRequest.FirstName,
            fullNameRequest.LastName);
    }
}