using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Courses.ReorderCourses;

public record ReorderCoursesCommand(
    Guid ResumeId,
    IReadOnlyList<Guid> OrderedIds) : ICommand<ErrorOr<bool>>;
