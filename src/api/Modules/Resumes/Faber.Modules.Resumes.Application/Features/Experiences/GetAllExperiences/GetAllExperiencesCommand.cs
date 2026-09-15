using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Experiences.GetAllExperiences;

public record GetAllExperiencesCommand(Guid ResumeId)
    : ICommand<ErrorOr<GetAllExperiencesResponse>>;