using System.Collections;
using Faber.Modules.Resumes.Application.Features.Courses.UpdateCourse;
using Faber.Modules.Resumes.Application.Tests.Features.Shared.Data;

namespace Faber.Modules.Resumes.Application.Tests.Features.Courses.UpdateCourse.Data;

public class InvalidUpdateCourseData : IEnumerable<TheoryDataRow<UpdateCourseRequest>>
{
    private const int DescriptionMaxLength = 1000;
    private const int NameMaxLength = 100;
    private const int SchoolMaxLength = 100;

    public IEnumerator<TheoryDataRow<UpdateCourseRequest>> GetEnumerator()
    {
        var baseRequest = new CreateCourseRequestFaker(Guid.Empty).Generate();

        yield return new TheoryDataRow<UpdateCourseRequest>(
            new UpdateCourseRequest(
                Guid.NewGuid(),
                Guid.Empty,
                baseRequest.School,
                baseRequest.Name,
                new DateOnly(2023, 6, 1),
                new DateOnly(2023, 1, 1),
                baseRequest.Description));

        yield return new TheoryDataRow<UpdateCourseRequest>(
            new UpdateCourseRequest(
                Guid.NewGuid(),
                Guid.Empty,
                baseRequest.School,
                baseRequest.Name,
                baseRequest.StartDate,
                baseRequest.EndDate,
                new string('x', DescriptionMaxLength + 1)));

        yield return new TheoryDataRow<UpdateCourseRequest>(
            new UpdateCourseRequest(
                Guid.NewGuid(),
                Guid.Empty,
                baseRequest.School,
                new string('a', NameMaxLength + 1),
                baseRequest.StartDate,
                baseRequest.EndDate,
                baseRequest.Description));

        yield return new TheoryDataRow<UpdateCourseRequest>(
            new UpdateCourseRequest(
                Guid.NewGuid(),
                Guid.Empty,
                new string('b', SchoolMaxLength + 1),
                baseRequest.Name,
                baseRequest.StartDate,
                baseRequest.EndDate,
                baseRequest.Description));
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
