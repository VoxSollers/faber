using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Experiences.UpdateExperience;

public record UpdateExperienceCommand(
    Guid Id,
    Guid ResumeId,
    string? JobTitle,
    string? Employer,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? City,
    string? Description) : ICommand<ErrorOr<bool>>;