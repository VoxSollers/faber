using ErrorOr;
using Faber.Modules.Resumes.Application.Features.Resumes.Shared;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Courses.ReorderCourses;

public class ReorderCoursesCommandHandler(
    ResumesDbContext dbContext,
    ILogger<ReorderCoursesCommandHandler> logger)
    : ICommandHandler<ReorderCoursesCommand, ErrorOr<bool>>
{
    private const string HandlerName = nameof(ReorderCoursesCommandHandler);

    public async Task<ErrorOr<bool>> ExecuteAsync(ReorderCoursesCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for resume {ResumeId}", HandlerName, command.ResumeId);

        var result = await SectionReorder.ApplyAsync(
            dbContext,
            dbContext.Courses,
            command.ResumeId,
            command.OrderedIds,
            ct);

        if (result.IsError)
        {
            logger.LogWarning("[FAIL] {HandlerName} | {Error}", HandlerName, result.FirstError.Description);

            return result;
        }

        logger.LogInformation("[SUCCESS] {HandlerName} | resume {ResumeId} reordered", HandlerName, command.ResumeId);

        return result;
    }
}
