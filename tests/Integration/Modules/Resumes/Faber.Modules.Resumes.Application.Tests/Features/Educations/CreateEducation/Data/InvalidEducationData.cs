using System.Collections;
using Faber.Modules.Resumes.Application.Features.Educations.CreateEducation;
using Faber.Modules.Resumes.Application.Tests.Features.Shared.Data;
using static Faber.Modules.Resumes.Application.Tests.Features.Shared.ResumesTestConstants;

namespace Faber.Modules.Resumes.Application.Tests.Features.Educations.CreateEducation.Data;

public class InvalidEducationData : IEnumerable<TheoryDataRow<CreateEducationRequest>>
{
    private const int InvalidSeed = 88888;
    private const int SchoolMaxLength = 100;
    private const int DegreeMaxLength = 100;
    private const int CityMaxLength = 100;
    private const int DescriptionMaxLength = 1000;

    public IEnumerator<TheoryDataRow<CreateEducationRequest>> GetEnumerator()
    {
        var faker = new CreateEducationRequestFaker(Guid.Empty, InvalidSeed);

        var tooLongSchool = faker.Generate() with { School = new string('a', SchoolMaxLength + 1) };
        yield return new TheoryDataRow<CreateEducationRequest>(tooLongSchool);

        var tooLongDegree = faker.Generate() with { Degree = new string('b', DegreeMaxLength + 1) };
        yield return new TheoryDataRow<CreateEducationRequest>(tooLongDegree);

        var tooLongCity = faker.Generate() with { City = new string('c', CityMaxLength + 1) };
        yield return new TheoryDataRow<CreateEducationRequest>(tooLongCity);

        var tooLongDescription = faker.Generate() with { Description = new string('d', DescriptionMaxLength + 1) };
        yield return new TheoryDataRow<CreateEducationRequest>(tooLongDescription);

        var invalidDateRange = faker.Generate() with
        {
            StartDate = new DateOnly(2023, 6, 1),
            EndDate = new DateOnly(2023, 1, 1)
        };
        yield return new TheoryDataRow<CreateEducationRequest>(invalidDateRange);

        var futureStartDate = faker.Generate() with { StartDate = new DateOnly(2099, 1, 1) };
        yield return new TheoryDataRow<CreateEducationRequest>(futureStartDate);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
