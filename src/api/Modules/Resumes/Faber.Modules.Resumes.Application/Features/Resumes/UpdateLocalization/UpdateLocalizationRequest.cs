namespace Faber.Modules.Resumes.Application.Features.Resumes.UpdateLocalization;

public record UpdateLocalizationRequest(Guid ResumeId, string? Localization);