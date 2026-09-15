namespace Faber.Modules.Auth.Application.Features.SignUp;

public record SignUpRequest(
    string Username,
    string Password,
    string ConfirmPassword,
    string Email,
    string FirstName,
    string LastName);