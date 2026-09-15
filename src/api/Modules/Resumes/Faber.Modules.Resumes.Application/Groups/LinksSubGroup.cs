using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Groups;

public sealed class LinksSubGroup : SubGroup<ResumesGroup>
{
    public LinksSubGroup()
    {
        Configure(
            "{ResumeId}/links",
            ep =>
            {
                ep.Tags("Links");
            });
    }
}