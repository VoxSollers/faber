using FastEndpoints;

namespace Faber.Modules.Identity.Application.Groups;

public sealed class IdentityGroup : Group
{
    public IdentityGroup()
    {
        Configure(
            "identity",
            ep =>
            {
                ep.Tags("Identity");
            });
    }
}