using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Experiences.CreateExperience;

public record CreateExperienceCommand(
    Guid ResumeId,
    string? JobTitle,
    string? Employer,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? City,
    string? Description) : ICommand<ErrorOr<CreateExperienceResponse>>;