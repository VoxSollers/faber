using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Courses.GetAllCourses;

public record GetAllCoursesCommand(Guid ResumeId) : ICommand<ErrorOr<GetAllCoursesResponse>>;