namespace Faber.Modules.Users.PublicApi.Contracts;

public record CreateUserRequest(
    string Username,
    string Email,
    string FirstName,
    string LastName);