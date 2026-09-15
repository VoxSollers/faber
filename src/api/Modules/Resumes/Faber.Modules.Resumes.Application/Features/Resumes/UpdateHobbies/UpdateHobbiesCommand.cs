using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Resumes.UpdateHobbies;

public record UpdateHobbiesCommand(Guid Id, Guid UserId, string? Hobbies) : ICommand<ErrorOr<bool>>;