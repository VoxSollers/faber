namespace Faber.Modules.Users.PublicApi.Contracts;

public record UserResponse(
    string UserId,
    string Username,
    string Email,
    string FirstName,
    string LastName);