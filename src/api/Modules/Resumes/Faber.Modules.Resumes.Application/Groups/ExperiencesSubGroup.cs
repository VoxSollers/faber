using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Groups;

public sealed class ExperiencesSubGroup : SubGroup<ResumesGroup>
{
    public ExperiencesSubGroup()
    {
        Configure(
            "{ResumeId}/experiences",
            ep =>
            {
                ep.Tags("Experiences");
            });
    }
}
