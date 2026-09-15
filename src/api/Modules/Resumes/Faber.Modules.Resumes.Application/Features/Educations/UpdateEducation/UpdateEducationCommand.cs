using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Educations.UpdateEducation;

public record UpdateEducationCommand(
    Guid Id,
    Guid ResumeId,
    string? School,
    string? Degree,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? City,
    string? Description) : ICommand<ErrorOr<bool>>;