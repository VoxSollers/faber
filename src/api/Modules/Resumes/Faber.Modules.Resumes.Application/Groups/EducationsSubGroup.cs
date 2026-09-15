using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Groups;

public sealed class EducationsSubGroup : SubGroup<ResumesGroup>
{
    public EducationsSubGroup()
    {
        Configure(
            "{ResumeId}/educations",
            ep =>
            {
                ep.Tags("Educations");
            });
    }
}