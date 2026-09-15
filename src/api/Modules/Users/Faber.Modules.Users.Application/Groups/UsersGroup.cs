using FastEndpoints;

namespace Faber.Modules.Users.Application.Groups;

public sealed class UsersGroup : Group
{
    public UsersGroup()
    {
        Configure(
            "users",
            ep =>
            {
                ep.Tags("Users");
            });
    }
}