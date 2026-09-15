using Bogus;

namespace Faber.Modules.Auth.Application.Tests.Features.VerifyEmail.Data;

public class VerifyEmailFaker
{
    private readonly Faker _faker;

    public VerifyEmailFaker(int seed)
    {
        _faker = new Faker("en_US") { Random = new Randomizer(seed) };
    }

    public string CombinedKey(int length)
    {
        return _faker.Random.String2(length);
    }
}