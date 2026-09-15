using Bogus;
using Faber.Modules.Resumes.Application.Features.Courses.CreateCourse;

namespace Faber.Modules.Resumes.Application.Tests.Features.Shared.Data;

public sealed class CreateCourseRequestFaker : Faker<CreateCourseRequest>
{
    public CreateCourseRequestFaker(Guid resumeId, int seed = ResumesTestConstants.CourseSeed)
    {
        UseSeed(seed);
        CustomInstantiator(f => new CreateCourseRequest(
            resumeId,
            f.PickRandom("Udemy", "Coursera", "edX", "Pluralsight", "LinkedIn Learning"),
            f.Hacker.Phrase(),
            f.Date.PastDateOnly(3, new DateOnly(2023, 1, 1)),
            f.Date.RecentDateOnly(365),
            f.Lorem.Sentence()));
    }
}
