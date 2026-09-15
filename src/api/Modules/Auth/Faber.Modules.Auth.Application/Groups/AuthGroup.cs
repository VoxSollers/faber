using FastEndpoints;

namespace Faber.Modules.Auth.Application.Groups;

public sealed class AuthGroup : Group
{
    public AuthGroup()
    {
        Configure(
            "auth",
            ep =>
            {
                ep.Tags("Auth");
            });
    }
}