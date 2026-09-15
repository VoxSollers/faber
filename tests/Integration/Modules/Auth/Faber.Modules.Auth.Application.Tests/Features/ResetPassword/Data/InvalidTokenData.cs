using System.Collections;

namespace Faber.Modules.Auth.Application.Tests.Features.ResetPassword.Data;

public class InvalidTokenData : IEnumerable<TheoryDataRow<string, string>>
{
    private const int Seed = 99999;
    private const int MinCombinedKeyLength = 27;

    public IEnumerator<TheoryDataRow<string, string>> GetEnumerator()
    {
        var faker = new ResetPasswordFaker(Seed);

        yield return new TheoryDataRow<string, string>(
            faker.CombinedKey(MinCombinedKeyLength + 10),
            faker.Password(10));

        yield return new TheoryDataRow<string, string>(
            faker.CombinedKey(MinCombinedKeyLength + 5),
            faker.Password());
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}