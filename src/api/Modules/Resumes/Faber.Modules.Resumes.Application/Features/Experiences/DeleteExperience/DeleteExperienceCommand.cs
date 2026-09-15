using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Experiences.DeleteExperience;

public record DeleteExperienceCommand(Guid ResumeId, Guid Id) : ICommand<ErrorOr<bool>>;