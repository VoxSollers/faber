namespace Faber.Modules.Auth.Application.Features.Me;

public record MeResponse(
    string Id,
    string Username,
    string Email,
    string FirstName,
    string LastName);