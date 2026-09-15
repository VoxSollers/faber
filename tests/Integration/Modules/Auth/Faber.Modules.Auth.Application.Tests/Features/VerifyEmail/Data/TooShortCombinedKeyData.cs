using System.Collections;

namespace Faber.Modules.Auth.Application.Tests.Features.VerifyEmail.Data;

public class TooShortCombinedKeyData : IEnumerable<TheoryDataRow<string>>
{
    private const int Seed = 55555;

    public IEnumerator<TheoryDataRow<string>> GetEnumerator()
    {
        var faker = new VerifyEmailFaker(Seed);

        yield return new TheoryDataRow<string>(faker.CombinedKey(5));
        yield return new TheoryDataRow<string>(faker.CombinedKey(26));
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}