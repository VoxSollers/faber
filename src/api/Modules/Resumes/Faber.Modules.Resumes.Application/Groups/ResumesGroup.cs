using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Groups;

public sealed class ResumesGroup : Group
{
    public ResumesGroup()
    {
        Configure(
            "resumes",
            ep =>
            {
                ep.Tags("Resumes");
            });
    }
}