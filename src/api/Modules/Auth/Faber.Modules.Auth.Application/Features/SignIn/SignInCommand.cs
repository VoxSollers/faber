using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Auth.Application.Features.SignIn;

public record SignInCommand(string Username, string Password) : ICommand<ErrorOr<SignInResponse>>;