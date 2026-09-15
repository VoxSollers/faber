using System.Collections;
using Faber.Modules.Resumes.Application.Features.Courses.CreateCourse;
using Faber.Modules.Resumes.Application.Tests.Features.Shared.Data;

namespace Faber.Modules.Resumes.Application.Tests.Features.Courses.CreateCourse.Data;

public class InvalidCourseData : IEnumerable<TheoryDataRow<CreateCourseRequest>>
{
    public IEnumerator<TheoryDataRow<CreateCourseRequest>> GetEnumerator()
    {
        var baseRequest = new CreateCourseRequestFaker(Guid.NewGuid()).Generate();

        yield return new TheoryDataRow<CreateCourseRequest>(
            baseRequest with { StartDate = new DateOnly(2024, 12, 31), EndDate = new DateOnly(2024, 1, 1) });

        yield return new TheoryDataRow<CreateCourseRequest>(
            baseRequest with { Description = new string('A', 1001) });

        yield return new TheoryDataRow<CreateCourseRequest>(
            baseRequest with { StartDate = new DateOnly(2099, 1, 1) });

        yield return new TheoryDataRow<CreateCourseRequest>(
            baseRequest with { Name = new string('a', 101) });

        yield return new TheoryDataRow<CreateCourseRequest>(
            baseRequest with { School = new string('b', 101) });
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}