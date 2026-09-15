using Bogus;

namespace Faber.Modules.Auth.Application.Tests.Features.ResetPassword.Data;

public class ResetPasswordFaker
{
    private readonly Faker _faker;

    public ResetPasswordFaker(int seed)
    {
        _faker = new Faker("en_US") { Random = new Randomizer(seed) };
    }

    public string Password(int minLength = 8)
    {
        return _faker.Internet.Password(minLength);
    }

    public string CombinedKey(int length)
    {
        return _faker.Random.String2(length);
    }
}