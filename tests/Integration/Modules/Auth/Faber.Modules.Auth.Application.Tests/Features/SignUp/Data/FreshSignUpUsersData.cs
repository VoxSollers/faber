using System.Collections;
using Bogus;
using Faber.Modules.Auth.Application.Features.SignUp;
using static Faber.Modules.Auth.Application.Tests.Features.SignUp.SignUpConstants;

namespace Faber.Modules.Auth.Application.Tests.Features.SignUp.Data;

public class FreshSignUpUsersData : IEnumerable<TheoryDataRow<SignUpRequest>>
{
    private const int Seed = 67890;
    private const int Count = 2;

    public IEnumerator<TheoryDataRow<SignUpRequest>> GetEnumerator()
    {
        return Generate().Select(request => new TheoryDataRow<SignUpRequest>(request)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static List<SignUpRequest> Generate()
    {
        Randomizer.Seed = new Random(Seed);

        var signUpFaker = new Faker<SignUpRequest>()
            .CustomInstantiator(f => new SignUpRequest(
                f.Internet.UserName(),
                ValidPassword,
                ValidPassword,
                f.Internet.Email(),
                f.Name.FirstName(),
                f.Name.LastName()));

        return signUpFaker.Generate(Count);
    }
}