using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Auth.Application.Features.SignUp;

public record SignUpCommand(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName)
    : ICommand<ErrorOr<SignUpResponse>>;