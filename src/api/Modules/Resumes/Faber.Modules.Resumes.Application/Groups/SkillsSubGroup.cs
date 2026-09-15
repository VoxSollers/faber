using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Groups;

public sealed class SkillsSubGroup : SubGroup<ResumesGroup>
{
    public SkillsSubGroup()
    {
        Configure(
            "{ResumeId}/skills",
            ep =>
            {
                ep.Tags("Skills");
            });
    }
}