using Faber.Modules.Resumes.Application.Features.Resumes.Shared;

namespace Faber.Modules.Resumes.Application.Features.Resumes.GetAllResumes;

public record GetAllResumesResponse(List<ResumeResponse> Items);
