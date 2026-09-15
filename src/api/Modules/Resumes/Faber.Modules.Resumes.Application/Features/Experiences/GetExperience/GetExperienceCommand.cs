using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Experiences.GetExperience;

public record GetExperienceCommand(Guid ResumeId, Guid Id) : ICommand<ErrorOr<GetExperienceResponse>>;