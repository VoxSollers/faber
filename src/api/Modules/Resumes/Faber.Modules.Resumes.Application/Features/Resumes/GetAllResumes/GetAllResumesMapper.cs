using Faber.Modules.Resumes.Application.Features.Resumes.Shared;
using Faber.Modules.Resumes.Domain.Entities;

namespace Faber.Modules.Resumes.Application.Features.Resumes.GetAllResumes;

public static class GetAllResumesMapper
{
    public static GetAllResumesResponse MapToResponse(this List<Resume> resumes)
    {
        return new GetAllResumesResponse(resumes.ConvertAll(r => r.ToResponse()));
    }
}
