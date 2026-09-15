using System.Collections;
using Bogus;
using Faber.Modules.Auth.Application.Features.SignIn;

namespace Faber.Modules.Auth.Application.Tests.Features.SignIn.Data;

public class UnregisteredUsersData : IEnumerable<TheoryDataRow<SignInRequest>>
{
    private const int Seed = 54321;
    private const int Count = 5;

    public IEnumerator<TheoryDataRow<SignInRequest>> GetEnumerator()
    {
        return Generate().Select(request => new TheoryDataRow<SignInRequest>(request)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static List<SignInRequest> Generate()
    {
        Randomizer.Seed = new Random(Seed);

        var faker = new Faker<SignInRequest>()
            .CustomInstantiator(f =>
                new SignInRequest(
                    f.Person.UserName,
                    f.Internet.Password(8)));

        return faker.Generate(Count);
    }
}