using Bogus;
using Faber.Modules.Users.PublicApi.Contracts;

namespace Faber.Testing.Shared.Data;

public class CreateUserRequestFaker
{
    private readonly Faker _faker;
    private readonly Faker<CreateUserRequest> _userFaker;

    public CreateUserRequestFaker(int seed)
    {
        _userFaker = new Faker<CreateUserRequest>("en_US").UseSeed(seed);
        _faker = new Faker("en_US") { Random = new Randomizer(seed) };
    }

    public CreateUserRequest Generate()
    {
        return _userFaker
            .CustomInstantiator(f =>
                new CreateUserRequest(
                    f.Person.UserName,
                    f.Person.Email,
                    f.Person.FirstName,
                    f.Person.LastName))
            .Generate();
    }

    public string Password(int minLength = 8)
    {
        return _faker.Internet.Password(minLength);
    }
}
