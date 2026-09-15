using System.Collections;
using Faber.Modules.Resumes.Application.Features.Experiences.CreateExperience;
using Faber.Modules.Resumes.Application.Tests.Features.Shared.Data;
using static Faber.Modules.Resumes.Application.Tests.Features.Shared.ResumesTestConstants;

namespace Faber.Modules.Resumes.Application.Tests.Features.Experiences.CreateExperience.Data;

public class InvalidExperienceData : IEnumerable<TheoryDataRow<CreateExperienceRequest>>
{
    private const int InvalidSeed = 88888;
    private const int JobTitleMaxLength = 100;
    private const int EmployerMaxLength = 100;

    public IEnumerator<TheoryDataRow<CreateExperienceRequest>> GetEnumerator()
    {
        var faker = new CreateExperienceRequestFaker(Guid.Empty, InvalidSeed);

        var tooLongJobTitle = faker.Generate() with { JobTitle = new string('a', JobTitleMaxLength + 1) };
        yield return new TheoryDataRow<CreateExperienceRequest>(tooLongJobTitle);

        var tooLongEmployer = faker.Generate() with { Employer = new string('b', EmployerMaxLength + 1) };
        yield return new TheoryDataRow<CreateExperienceRequest>(tooLongEmployer);

        var tooLongCity = faker.Generate() with { City = new string('c', 101) };
        yield return new TheoryDataRow<CreateExperienceRequest>(tooLongCity);

        var tooLongDescription = faker.Generate() with { Description = new string('d', 1001) };
        yield return new TheoryDataRow<CreateExperienceRequest>(tooLongDescription);

        var invalidDateRange = faker.Generate() with
        {
            StartDate = new DateOnly(2023, 6, 1),
            EndDate = new DateOnly(2023, 1, 1)
        };
        yield return new TheoryDataRow<CreateExperienceRequest>(invalidDateRange);

        var futureStartDate = faker.Generate() with { StartDate = new DateOnly(2099, 1, 1) };
        yield return new TheoryDataRow<CreateExperienceRequest>(futureStartDate);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
