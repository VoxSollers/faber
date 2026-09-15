namespace Faber.Modules.Users.PublicApi.Contracts;

public record UpdateUserRequest(string UserId, string FirstName, string LastName);