using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Courses.DeleteCourse;

public record DeleteCourseCommand(Guid ResumeId, Guid Id) : ICommand<ErrorOr<bool>>;