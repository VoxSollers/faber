namespace Faber.Modules.Resumes.Application.Features.Resumes.CreateResume;

public record CreateResumeResponse(Guid Id, DateTimeOffset CreatedAt, string? Localization);