using System.Collections;

namespace Faber.Modules.Auth.Application.Tests.Features.ResetPassword.Data;

public class MismatchedPasswordsData : IEnumerable<TheoryDataRow<string, string, string>>
{
    private const int Seed = 88888;
    private const int MinCombinedKeyLength = 27;

    public IEnumerator<TheoryDataRow<string, string, string>> GetEnumerator()
    {
        var faker = new ResetPasswordFaker(Seed);

        var validCombinedKey = faker.CombinedKey(MinCombinedKeyLength + 10);

        yield return new TheoryDataRow<string, string, string>(
            faker.Password(),
            faker.Password(12),
            validCombinedKey);

        yield return new TheoryDataRow<string, string, string>(
            faker.Password(),
            faker.Password(11),
            validCombinedKey);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}