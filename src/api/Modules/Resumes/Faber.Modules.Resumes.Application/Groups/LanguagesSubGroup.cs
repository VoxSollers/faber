using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Groups;

public sealed class LanguagesSubGroup : SubGroup<ResumesGroup>
{
    public LanguagesSubGroup()
    {
        Configure(
            "{ResumeId}/languages",
            ep =>
            {
                ep.Tags("Languages");
            });
    }
}