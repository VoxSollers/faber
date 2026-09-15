using System.Collections;
using Faber.Modules.Resumes.Application.Features.Experiences.UpdateExperience;
using Faber.Modules.Resumes.Application.Tests.Features.Shared.Data;

namespace Faber.Modules.Resumes.Application.Tests.Features.Experiences.UpdateExperience.Data;

public class InvalidUpdateExperienceData : IEnumerable<TheoryDataRow<UpdateExperienceRequest>>
{
    private const int InvalidSeed = 88888;
    private const int JobTitleMaxLength = 100;
    private const int EmployerMaxLength = 100;

    public IEnumerator<TheoryDataRow<UpdateExperienceRequest>> GetEnumerator()
    {
        var faker = new CreateExperienceRequestFaker(Guid.Empty, InvalidSeed);

        var tooLongJobTitle = faker.Generate() with { JobTitle = new string('a', JobTitleMaxLength + 1) };
        yield return new TheoryDataRow<UpdateExperienceRequest>(
            new UpdateExperienceRequest(Guid.NewGuid(), Guid.Empty, tooLongJobTitle.JobTitle,
                tooLongJobTitle.Employer, tooLongJobTitle.StartDate, tooLongJobTitle.EndDate,
                tooLongJobTitle.City, tooLongJobTitle.Description));

        var tooLongEmployer = faker.Generate() with { Employer = new string('b', EmployerMaxLength + 1) };
        yield return new TheoryDataRow<UpdateExperienceRequest>(
            new UpdateExperienceRequest(Guid.NewGuid(), Guid.Empty, tooLongEmployer.JobTitle,
                tooLongEmployer.Employer, tooLongEmployer.StartDate, tooLongEmployer.EndDate,
                tooLongEmployer.City, tooLongEmployer.Description));

        var tooLongCity = faker.Generate() with { City = new string('c', 101) };
        yield return new TheoryDataRow<UpdateExperienceRequest>(
            new UpdateExperienceRequest(Guid.NewGuid(), Guid.Empty, tooLongCity.JobTitle,
                tooLongCity.Employer, tooLongCity.StartDate, tooLongCity.EndDate,
                tooLongCity.City, tooLongCity.Description));

        var tooLongDescription = faker.Generate() with { Description = new string('d', 1001) };
        yield return new TheoryDataRow<UpdateExperienceRequest>(
            new UpdateExperienceRequest(Guid.NewGuid(), Guid.Empty, tooLongDescription.JobTitle,
                tooLongDescription.Employer, tooLongDescription.StartDate, tooLongDescription.EndDate,
                tooLongDescription.City, tooLongDescription.Description));

        var invalidDateRange = faker.Generate() with
        {
            StartDate = new DateOnly(2023, 6, 1),
            EndDate = new DateOnly(2023, 1, 1)
        };
        yield return new TheoryDataRow<UpdateExperienceRequest>(
            new UpdateExperienceRequest(Guid.NewGuid(), Guid.Empty, invalidDateRange.JobTitle,
                invalidDateRange.Employer, invalidDateRange.StartDate, invalidDateRange.EndDate,
                invalidDateRange.City, invalidDateRange.Description));
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
