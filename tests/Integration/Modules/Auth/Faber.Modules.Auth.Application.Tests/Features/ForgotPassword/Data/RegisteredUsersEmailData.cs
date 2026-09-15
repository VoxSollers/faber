using System.Collections;
using Faber.Testing.Shared.Data;
using Faber.Modules.Users.PublicApi.Contracts;

namespace Faber.Modules.Auth.Application.Tests.Features.ForgotPassword.Data;

public class RegisteredUsersEmailData : IEnumerable<TheoryDataRow<CreateUserRequest>>
{
    public IEnumerator<TheoryDataRow<CreateUserRequest>> GetEnumerator()
    {
        return RegisteredUsersData.Generate()
            .Select(t => new TheoryDataRow<CreateUserRequest>(t.User))
            .GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}