using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Persons.GetPersonByResume;

public record GetPersonByResumeCommand(Guid ResumeId) : ICommand<ErrorOr<GetPersonByResumeResponse>>;