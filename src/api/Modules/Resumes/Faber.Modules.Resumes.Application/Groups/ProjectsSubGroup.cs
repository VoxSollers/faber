using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Groups;

public sealed class ProjectsSubGroup : SubGroup<ResumesGroup>
{
    public ProjectsSubGroup()
    {
        Configure("{ResumeId}/projects", ep => ep.Tags("Projects"));
    }
}
