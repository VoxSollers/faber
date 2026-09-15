using static Faber.Modules.Resumes.Infrastructure.Database.DbConstants;

namespace Faber.Modules.Resumes.Application.Features.Persons;

internal static class ResumeTitleSnapshot
{
    public static string? FromName(string? firstname, string? lastname)
    {
        var fullName = string.Join(
            " ",
            new[] { firstname, lastname }.Where(value => !string.IsNullOrWhiteSpace(value))).Trim();

        return fullName.Length switch
        {
            0 => null,
            > OneLineStringMaxLength => fullName[..OneLineStringMaxLength],
            _ => fullName
        };
    }
}
