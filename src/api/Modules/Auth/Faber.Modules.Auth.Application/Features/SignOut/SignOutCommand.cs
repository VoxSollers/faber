using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Auth.Application.Features.SignOut;

public record SignOutCommand(string RefreshToken) : ICommand<ErrorOr<Success>>;