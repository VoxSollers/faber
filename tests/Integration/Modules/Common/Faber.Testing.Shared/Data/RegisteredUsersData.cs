using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Faber.Modules.Users.PublicApi.Contracts;
using Xunit;

namespace Faber.Testing.Shared.Data;

public class RegisteredUsersData : IEnumerable<TheoryDataRow<CreateUserRequest, string>>
{
    private const int Seed = 12345;
    private const int Count = 5;

    public IEnumerator<TheoryDataRow<CreateUserRequest, string>> GetEnumerator()
    {
        return Generate().Select(t => new TheoryDataRow<CreateUserRequest, string>(t.User, t.Password)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static List<(CreateUserRequest User, string Password)> Generate()
    {
        var faker = new CreateUserRequestFaker(Seed);

        return Enumerable.Range(0, Count)
            .Select(_ => (faker.Generate(), faker.Password()))
            .ToList();
    }
}
