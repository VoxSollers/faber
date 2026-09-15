using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Users.Application.Features.UpdateUserFullName;

public record UpdateUserFullNameCommand(string UserId, string FirstName, string LastName)
    : ICommand<ErrorOr<UpdateUserFullNameResponse>>;