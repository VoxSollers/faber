using System.Collections;
using Bogus;

namespace Faber.Modules.Auth.Application.Tests.Features.ResetPassword.Data;

public class TooShortCombinedKeyData : IEnumerable<TheoryDataRow<string, string>>
{
    private const int Seed = 44444;

    public IEnumerator<TheoryDataRow<string, string>> GetEnumerator()
    {
        Randomizer.Seed = new Random(Seed);
        var faker = new Faker();

        yield return new TheoryDataRow<string, string>(
            faker.Random.String2(5),
            faker.Internet.Password());

        yield return new TheoryDataRow<string, string>(
            faker.Random.String2(26),
            faker.Internet.Password(8));
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}