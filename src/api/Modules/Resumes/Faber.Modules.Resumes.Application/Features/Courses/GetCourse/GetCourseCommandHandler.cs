using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Courses.GetCourse;

public class GetCourseCommandHandler(
    ResumesDbContext dbContext,
    ILogger<GetCourseCommandHandler> logger)
    : ICommandHandler<GetCourseCommand, ErrorOr<GetCourseResponse>>
{
    private const string HandlerName = nameof(GetCourseCommandHandler);

    public async Task<ErrorOr<GetCourseResponse>> ExecuteAsync(GetCourseCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {CourseId}", HandlerName, command.Id);

        var course = await dbContext.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == command.Id && c.ResumeId == command.ResumeId, ct);

        if (course is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Course {CourseId} not found", HandlerName, command.Id);

            return Error.NotFound("Course.NotFound", $"Course with id '{command.Id}' was not found");
        }

        logger.LogInformation("[SUCCESS] {HandlerName} | Course {CourseId} found", HandlerName, command.Id);

        return course.MapToResponse();
    }
}