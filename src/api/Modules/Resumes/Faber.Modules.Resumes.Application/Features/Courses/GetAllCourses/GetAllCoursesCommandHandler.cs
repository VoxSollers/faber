using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Courses.GetAllCourses;

public class GetAllCoursesCommandHandler(
    ResumesDbContext dbContext,
    ILogger<GetAllCoursesCommandHandler> logger)
    : ICommandHandler<GetAllCoursesCommand, ErrorOr<GetAllCoursesResponse>>
{
    private const string HandlerName = nameof(GetAllCoursesCommandHandler);

    public async Task<ErrorOr<GetAllCoursesResponse>> ExecuteAsync(GetAllCoursesCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for resume {ResumeId}", HandlerName, command.ResumeId);

        var resume = await dbContext.Resumes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == command.ResumeId, ct);

        if (resume is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Resume {ResumeId} not found", HandlerName, command.ResumeId);

            return Error.NotFound("Resume.NotFound", $"Resume with id '{command.ResumeId}' was not found");
        }

        var courses = await dbContext.Courses
            .AsNoTracking()
            .Where(c => c.ResumeId == command.ResumeId)
            .OrderBy(c => c.Order)
            .ToListAsync(ct);

        var items = courses.Select(e => e.MapToItem()).ToList();

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | Found {Count} courses for resume {ResumeId}",
            HandlerName,
            items.Count,
            command.ResumeId);

        return new GetAllCoursesResponse(items);
    }
}