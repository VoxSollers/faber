using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Educations.CreateEducation;

public record CreateEducationCommand(
    Guid ResumeId,
    string? School,
    string? Degree,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? City,
    string? Description) : ICommand<ErrorOr<CreateEducationResponse>>;