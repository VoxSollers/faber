using ErrorOr;
using Faber.Modules.Resumes.Application.Features.Resumes.Shared;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Resumes.GetResume;

public class GetResumeCommandHandler(
    ResumesDbContext dbContext,
    ILogger<GetResumeCommandHandler> logger)
    : ICommandHandler<GetResumeCommand, ErrorOr<ResumeResponse>>
{
    private const string HandlerName = nameof(GetResumeCommandHandler);

    public async Task<ErrorOr<ResumeResponse>> ExecuteAsync(GetResumeCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {ResumeId}", HandlerName, command.Id);

        var resume = await dbContext.Resumes
            .AsNoTracking()
            .Include(r => r.Person)
            .Include(r => r.Experiences.OrderBy(e => e.Order))
            .Include(r => r.Educations.OrderBy(e => e.Order))
            .Include(r => r.Skills.OrderBy(s => s.Order))
            .Include(r => r.Languages.OrderBy(l => l.Order))
            .Include(r => r.Courses.OrderBy(c => c.Order))
            .Include(r => r.Links.OrderBy(l => l.Order))
            .FirstOrDefaultAsync(r => r.Id == command.Id && r.UserId == command.UserId, ct);

        if (resume is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Resume {ResumeId} not found", HandlerName, command.Id);

            return Error.NotFound("Resume.NotFound", $"Resume with id '{command.Id}' was not found");
        }

        logger.LogInformation("[SUCCESS] {HandlerName} | Resume {ResumeId} found", HandlerName, command.Id);

        return resume.ToResponse();
    }
}
