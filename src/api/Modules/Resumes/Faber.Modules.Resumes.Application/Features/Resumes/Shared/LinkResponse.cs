namespace Faber.Modules.Resumes.Application.Features.Resumes.Shared;

public record LinkResponse(Guid Id, string? Label, string? Uri, int Order);
