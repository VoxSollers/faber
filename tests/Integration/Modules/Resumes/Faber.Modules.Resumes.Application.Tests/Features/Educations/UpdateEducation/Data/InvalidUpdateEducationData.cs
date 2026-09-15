using System.Collections;
using Faber.Modules.Resumes.Application.Features.Educations.UpdateEducation;
using Faber.Modules.Resumes.Application.Tests.Features.Shared.Data;

namespace Faber.Modules.Resumes.Application.Tests.Features.Educations.UpdateEducation.Data;

public class InvalidUpdateEducationData : IEnumerable<TheoryDataRow<UpdateEducationRequest>>
{
    private const int InvalidSeed = 88889;
    private const int SchoolMaxLength = 100;
    private const int DegreeMaxLength = 100;
    private const int CityMaxLength = 100;
    private const int DescriptionMaxLength = 1000;

    public IEnumerator<TheoryDataRow<UpdateEducationRequest>> GetEnumerator()
    {
        var faker = new CreateEducationRequestFaker(Guid.Empty, InvalidSeed);

        var tooLongSchool = faker.Generate() with { School = new string('a', SchoolMaxLength + 1) };
        yield return new TheoryDataRow<UpdateEducationRequest>(
            new UpdateEducationRequest(Guid.NewGuid(), Guid.Empty, tooLongSchool.School, tooLongSchool.Degree,
                tooLongSchool.StartDate, tooLongSchool.EndDate, tooLongSchool.City, tooLongSchool.Description));

        var tooLongDegree = faker.Generate() with { Degree = new string('b', DegreeMaxLength + 1) };
        yield return new TheoryDataRow<UpdateEducationRequest>(
            new UpdateEducationRequest(Guid.NewGuid(), Guid.Empty, tooLongDegree.School, tooLongDegree.Degree,
                tooLongDegree.StartDate, tooLongDegree.EndDate, tooLongDegree.City, tooLongDegree.Description));

        var tooLongCity = faker.Generate() with { City = new string('c', CityMaxLength + 1) };
        yield return new TheoryDataRow<UpdateEducationRequest>(
            new UpdateEducationRequest(Guid.NewGuid(), Guid.Empty, tooLongCity.School, tooLongCity.Degree,
                tooLongCity.StartDate, tooLongCity.EndDate, tooLongCity.City, tooLongCity.Description));

        var tooLongDescription = faker.Generate() with { Description = new string('d', DescriptionMaxLength + 1) };
        yield return new TheoryDataRow<UpdateEducationRequest>(
            new UpdateEducationRequest(Guid.NewGuid(), Guid.Empty, tooLongDescription.School, tooLongDescription.Degree,
                tooLongDescription.StartDate, tooLongDescription.EndDate, tooLongDescription.City, tooLongDescription.Description));

        var invalidDateRange = faker.Generate() with
        {
            StartDate = new DateOnly(2023, 6, 1),
            EndDate = new DateOnly(2023, 1, 1)
        };
        yield return new TheoryDataRow<UpdateEducationRequest>(
            new UpdateEducationRequest(Guid.NewGuid(), Guid.Empty, invalidDateRange.School, invalidDateRange.Degree,
                invalidDateRange.StartDate, invalidDateRange.EndDate, invalidDateRange.City, invalidDateRange.Description));
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
