using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Persons.DeletePerson;

public record DeletePersonCommand(Guid ResumeId, Guid Id) : ICommand<ErrorOr<bool>>;