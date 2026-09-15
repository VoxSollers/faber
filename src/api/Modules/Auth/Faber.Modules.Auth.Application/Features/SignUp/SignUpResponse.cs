namespace Faber.Modules.Auth.Application.Features.SignUp;

public record SignUpResponse(
    string UserId,
    string Username,
    string Email,
    string FirstName,
    string LastName);