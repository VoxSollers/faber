using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Auth.Application.Features.ForgotPassword;

public record ForgotPasswordCommand(string Email) : ICommand<ErrorOr<Success>>;