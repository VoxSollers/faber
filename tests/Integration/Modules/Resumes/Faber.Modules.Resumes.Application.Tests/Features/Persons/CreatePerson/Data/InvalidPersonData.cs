using System.Collections;
using Faber.Modules.Resumes.Application.Features.Persons.CreatePerson;
using Faber.Modules.Resumes.Application.Tests.Features.Shared.Data;

namespace Faber.Modules.Resumes.Application.Tests.Features.Persons.CreatePerson.Data;

public class InvalidPersonData : IEnumerable<TheoryDataRow<CreatePersonRequest>>
{
    private const int FieldMaxLength = 100;

    public IEnumerator<TheoryDataRow<CreatePersonRequest>> GetEnumerator()
    {
        var baseRequest = new CreatePersonRequestFaker(Guid.NewGuid()).Generate();

        yield return new TheoryDataRow<CreatePersonRequest>(
            baseRequest with { Email = "invalid-email" });

        yield return new TheoryDataRow<CreatePersonRequest>(
            baseRequest with { Email = "invalid email example.com" });

        yield return new TheoryDataRow<CreatePersonRequest>(
            baseRequest with { Firstname = new string('a', FieldMaxLength + 1) });

        yield return new TheoryDataRow<CreatePersonRequest>(
            baseRequest with { Lastname = new string('b', FieldMaxLength + 1) });

        yield return new TheoryDataRow<CreatePersonRequest>(
            baseRequest with { Email = new string('x', FieldMaxLength + 1) });

        yield return new TheoryDataRow<CreatePersonRequest>(
            baseRequest with { DateOfBirth = new DateOnly(2099, 1, 1) });
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
