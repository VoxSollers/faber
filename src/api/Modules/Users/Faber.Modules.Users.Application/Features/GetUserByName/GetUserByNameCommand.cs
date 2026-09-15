using ErrorOr;
using Faber.Modules.Users.Application.Features.Shared.Responses;
using FastEndpoints;

namespace Faber.Modules.Users.Application.Features.GetUserByName;

public record GetUserByNameCommand(string Username) : ICommand<ErrorOr<GetUserResponse>>;