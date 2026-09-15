using System.Collections;
using Faber.Modules.Resumes.Application.Features.Persons.UpdatePerson;
using Faber.Modules.Resumes.Application.Tests.Features.Shared.Data;
using static Faber.Modules.Resumes.Application.Tests.Features.Shared.ResumesTestConstants;

namespace Faber.Modules.Resumes.Application.Tests.Features.Persons.UpdatePerson.Data;

public class InvalidUpdatePersonData : IEnumerable<TheoryDataRow<UpdatePersonRequest>>
{
    private const int FieldMaxLength = 100;

    public IEnumerator<TheoryDataRow<UpdatePersonRequest>> GetEnumerator()
    {
        var baseRequest = new CreatePersonRequestFaker(Guid.Empty, PersonSeed).Generate();

        yield return new TheoryDataRow<UpdatePersonRequest>(
            new UpdatePersonRequest(Guid.Empty, Guid.Empty, baseRequest.JobTitle, baseRequest.Firstname,
                baseRequest.Lastname, "invalid-email", baseRequest.Phone, baseRequest.Country,
                baseRequest.City, baseRequest.Street, baseRequest.PostCode, baseRequest.Nationality,
                baseRequest.DateOfBirth, baseRequest.DrivingLicense));

        yield return new TheoryDataRow<UpdatePersonRequest>(
            new UpdatePersonRequest(Guid.Empty, Guid.Empty, baseRequest.JobTitle,
                new string('a', FieldMaxLength + 1), baseRequest.Lastname, baseRequest.Email,
                baseRequest.Phone, baseRequest.Country, baseRequest.City, baseRequest.Street,
                baseRequest.PostCode, baseRequest.Nationality, baseRequest.DateOfBirth, baseRequest.DrivingLicense));

        yield return new TheoryDataRow<UpdatePersonRequest>(
            new UpdatePersonRequest(Guid.Empty, Guid.Empty, baseRequest.JobTitle, baseRequest.Firstname,
                new string('b', FieldMaxLength + 1), baseRequest.Email, baseRequest.Phone,
                baseRequest.Country, baseRequest.City, baseRequest.Street, baseRequest.PostCode,
                baseRequest.Nationality, baseRequest.DateOfBirth, baseRequest.DrivingLicense));

        yield return new TheoryDataRow<UpdatePersonRequest>(
            new UpdatePersonRequest(Guid.Empty, Guid.Empty, baseRequest.JobTitle, baseRequest.Firstname,
                baseRequest.Lastname, new string('x', FieldMaxLength + 1), baseRequest.Phone,
                baseRequest.Country, baseRequest.City, baseRequest.Street, baseRequest.PostCode,
                baseRequest.Nationality, baseRequest.DateOfBirth, baseRequest.DrivingLicense));

        yield return new TheoryDataRow<UpdatePersonRequest>(
            new UpdatePersonRequest(Guid.Empty, Guid.Empty, baseRequest.JobTitle, baseRequest.Firstname,
                baseRequest.Lastname, baseRequest.Email, baseRequest.Phone, baseRequest.Country,
                baseRequest.City, baseRequest.Street, baseRequest.PostCode, baseRequest.Nationality,
                new DateOnly(2099, 1, 1), baseRequest.DrivingLicense));
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
