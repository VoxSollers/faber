using System.Collections;
using Faber.Testing.Shared.Data;
using Faber.Modules.Users.PublicApi.Contracts;

namespace Faber.Modules.Auth.Application.Tests.Features.ResetPassword.Data;

public class RegisteredUsersWithNewCredentialsData : IEnumerable<TheoryDataRow<CreateUserRequest, string>>
{
    private const int Seed = 77777;

    public IEnumerator<TheoryDataRow<CreateUserRequest, string>> GetEnumerator()
    {
        var users = RegisteredUsersData.Generate().Select(t => t.User).ToList();

        var faker = new ResetPasswordFaker(Seed);

        return users.Select(user =>
                new TheoryDataRow<CreateUserRequest, string>(user, faker.Password(10)))
            .GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}