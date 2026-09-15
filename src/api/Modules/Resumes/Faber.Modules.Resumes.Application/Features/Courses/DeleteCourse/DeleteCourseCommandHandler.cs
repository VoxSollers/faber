using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Courses.DeleteCourse;

public class DeleteCourseCommandHandler(
    ResumesDbContext dbContext,
    ILogger<DeleteCourseCommandHandler> logger)
    : ICommandHandler<DeleteCourseCommand, ErrorOr<bool>>
{
    private const string HandlerName = nameof(DeleteCourseCommandHandler);

    public async Task<ErrorOr<bool>> ExecuteAsync(DeleteCourseCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {CourseId}", HandlerName, command.Id);

        var course = await dbContext.Courses
            .FirstOrDefaultAsync(c => c.Id == command.Id && c.ResumeId == command.ResumeId, ct);

        if (course is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Course {CourseId} not found", HandlerName, command.Id);

            return Error.NotFound("Course.NotFound", $"Course with id '{command.Id}' was not found");
        }

        dbContext.Courses.Remove(course);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("[SUCCESS] {HandlerName} | Course {CourseId} deleted", HandlerName, command.Id);

        return true;
    }
}