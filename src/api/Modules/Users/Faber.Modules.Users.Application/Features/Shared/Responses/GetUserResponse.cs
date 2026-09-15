namespace Faber.Modules.Users.Application.Features.Shared.Responses;

public record GetUserResponse(
    string Id,
    string Username,
    string Email,
    string FirstName,
    string LastName);