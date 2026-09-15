namespace Faber.Modules.Users.Application.Features.UpdateUserFullName;

public record UpdateUserFullNameRequest(string UserId, string FirstName, string LastName);