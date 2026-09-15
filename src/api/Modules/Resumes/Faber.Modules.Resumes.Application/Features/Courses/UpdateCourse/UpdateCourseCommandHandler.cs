using ErrorOr;
using Faber.Modules.Resumes.Application.Features.Resumes.Shared;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Courses.UpdateCourse;

public class UpdateCourseCommandHandler(
    ResumesDbContext dbContext,
    ILogger<UpdateCourseCommandHandler> logger)
    : ICommandHandler<UpdateCourseCommand, ErrorOr<bool>>
{
    private const string HandlerName = nameof(UpdateCourseCommandHandler);

    public async Task<ErrorOr<bool>> ExecuteAsync(UpdateCourseCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {CourseId}", HandlerName, command.Id);

        var course = await dbContext.Courses
            .FirstOrDefaultAsync(c => c.Id == command.Id && c.ResumeId == command.ResumeId, ct);

        if (course is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Course {CourseId} not found", HandlerName, command.Id);

            return Error.NotFound("Course.NotFound", $"Course with id '{command.Id}' was not found");
        }

        course.School = command.School;
        course.Name = command.Name;
        course.StartDate = command.StartDate;
        course.EndDate = command.EndDate;
        course.Description = HtmlContentSanitizer.SanitizeBasicFormatting(command.Description);

        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("[SUCCESS] {HandlerName} | Course {CourseId} updated", HandlerName, command.Id);

        return true;
    }
}
