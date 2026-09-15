using System.Collections;

namespace Faber.Modules.Auth.Application.Tests.Features.VerifyEmail.Data;

public class InvalidTokenData : IEnumerable<TheoryDataRow<string>>
{
    private const int Seed = 88888;
    private const int MinCombinedKeyLength = 27;

    public IEnumerator<TheoryDataRow<string>> GetEnumerator()
    {
        var faker = new VerifyEmailFaker(Seed);

        yield return new TheoryDataRow<string>(faker.CombinedKey(MinCombinedKeyLength + 10));
        yield return new TheoryDataRow<string>(faker.CombinedKey(MinCombinedKeyLength + 5));
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}