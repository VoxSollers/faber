using ErrorOr;
using Faber.Modules.Users.Application.Features.Shared.Responses;
using FastEndpoints;

namespace Faber.Modules.Users.Application.Features.GetUserById;

public record GetUserByIdCommand(string UserId) : ICommand<ErrorOr<GetUserResponse>>;