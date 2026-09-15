using System.Collections;

namespace Faber.Modules.Auth.Application.Tests.Features.ResetPassword.Data;

public class TooShortPasswordData : IEnumerable<TheoryDataRow<string, string>>
{
    private const int Seed = 66666;
    private const int MinCombinedKeyLength = 27;

    public IEnumerator<TheoryDataRow<string, string>> GetEnumerator()
    {
        var faker = new ResetPasswordFaker(Seed);

        var validCombinedKey = faker.CombinedKey(MinCombinedKeyLength + 10);

        yield return new TheoryDataRow<string, string>(faker.Password(3), validCombinedKey);
        yield return new TheoryDataRow<string, string>(faker.Password(7), validCombinedKey);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}