using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Persons.GetPerson;

public record GetPersonCommand(Guid ResumeId, Guid Id) : ICommand<ErrorOr<GetPersonResponse>>;