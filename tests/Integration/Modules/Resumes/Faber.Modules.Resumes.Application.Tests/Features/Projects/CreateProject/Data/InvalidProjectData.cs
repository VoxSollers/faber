using System.Collections;
using Faber.Modules.Resumes.Application.Features.Projects.CreateProject;
using Faber.Modules.Resumes.Application.Tests.Features.Shared.Data;

namespace Faber.Modules.Resumes.Application.Tests.Features.Projects.CreateProject.Data;

public class InvalidProjectData : IEnumerable<TheoryDataRow<CreateProjectRequest>>
{
    public IEnumerator<TheoryDataRow<CreateProjectRequest>> GetEnumerator()
    {
        var baseRequest = new CreateProjectRequestFaker(Guid.NewGuid()).Generate();

        yield return new TheoryDataRow<CreateProjectRequest>(
            baseRequest with { StartDate = new DateOnly(2024, 12, 31), EndDate = new DateOnly(2024, 1, 1) });

        yield return new TheoryDataRow<CreateProjectRequest>(
            baseRequest with { Description = new string('A', 1001) });

        yield return new TheoryDataRow<CreateProjectRequest>(
            baseRequest with { StartDate = new DateOnly(2099, 1, 1) });

        yield return new TheoryDataRow<CreateProjectRequest>(
            baseRequest with { Name = new string('a', 101) });

        yield return new TheoryDataRow<CreateProjectRequest>(
            baseRequest with { Role = new string('b', 101) });
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}