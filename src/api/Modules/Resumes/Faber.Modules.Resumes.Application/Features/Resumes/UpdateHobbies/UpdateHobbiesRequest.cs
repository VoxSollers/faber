namespace Faber.Modules.Resumes.Application.Features.Resumes.UpdateHobbies;

public record UpdateHobbiesRequest(Guid ResumeId, string? Hobbies);