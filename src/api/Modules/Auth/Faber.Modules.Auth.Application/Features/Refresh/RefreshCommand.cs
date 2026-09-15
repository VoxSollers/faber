using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Auth.Application.Features.Refresh;

public record RefreshCommand(string RefreshToken) : ICommand<ErrorOr<RefreshResponse>>;