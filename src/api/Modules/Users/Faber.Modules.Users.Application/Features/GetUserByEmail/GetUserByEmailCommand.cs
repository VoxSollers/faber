using ErrorOr;
using Faber.Modules.Users.Application.Features.Shared.Responses;
using FastEndpoints;

namespace Faber.Modules.Users.Application.Features.GetUserByEmail;

public record GetUserByEmailCommand(string Email) : ICommand<ErrorOr<GetUserResponse>>;