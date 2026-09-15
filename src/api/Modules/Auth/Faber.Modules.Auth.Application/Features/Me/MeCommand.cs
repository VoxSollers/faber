using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Auth.Application.Features.Me;

public record MeCommand(string UserId) : ICommand<ErrorOr<MeResponse>>;