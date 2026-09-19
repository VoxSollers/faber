using System.Collections;
using Faber.Modules.Resumes.Application.Features.Projects.UpdateProject;
using Faber.Modules.Resumes.Application.Tests.Features.Shared.Data;

namespace Faber.Modules.Resumes.Application.Tests.Features.Projects.UpdateProject.Data;

public class InvalidUpdateProjectData : IEnumerable<TheoryDataRow<UpdateProjectRequest>>
{
    private const int DescriptionMaxLength = 1000;
    private const int NameMaxLength = 100;
    private const int RoleMaxLength = 100;

    public IEnumerator<TheoryDataRow<UpdateProjectRequest>> GetEnumerator()
    {
        var baseRequest = new CreateProjectRequestFaker(Guid.Empty).Generate();

        yield return new TheoryDataRow<UpdateProjectRequest>(
            new UpdateProjectRequest(
                Guid.NewGuid(),
                Guid.Empty,
                baseRequest.Tagline,
                baseRequest.Name,
                baseRequest.Url,
                new DateOnly(2023, 6, 1),
                new DateOnly(2023, 1, 1),
                baseRequest.Description));

        yield return new TheoryDataRow<UpdateProjectRequest>(
            new UpdateProjectRequest(
                Guid.NewGuid(),
                Guid.Empty,
                baseRequest.Tagline,
                baseRequest.Name,
                baseRequest.Url,
                baseRequest.StartDate,
                baseRequest.EndDate,
                new string('x', DescriptionMaxLength + 1)));

        yield return new TheoryDataRow<UpdateProjectRequest>(
            new UpdateProjectRequest(
                Guid.NewGuid(),
                Guid.Empty,
                baseRequest.Tagline,
                new string('a', NameMaxLength + 1),
                baseRequest.Url,
                baseRequest.StartDate,
                baseRequest.EndDate,
                baseRequest.Description));

        yield return new TheoryDataRow<UpdateProjectRequest>(
            new UpdateProjectRequest(
                Guid.NewGuid(),
                Guid.Empty,
                new string('b', RoleMaxLength + 1),
                baseRequest.Name,
                baseRequest.Url,
                baseRequest.StartDate,
                baseRequest.EndDate,
                baseRequest.Description));
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
