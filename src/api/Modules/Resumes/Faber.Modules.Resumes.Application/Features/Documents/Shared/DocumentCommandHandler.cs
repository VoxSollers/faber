using ErrorOr;
using Faber.Modules.Resumes.Domain.Entities;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Documents.Shared;

public class DocumentCommandHandler(
    ResumesDbContext dbContext,
    ILogger<DocumentCommandHandler> logger)
    : ICommandHandler<DocumentCommand, ErrorOr<Resume>>
{
    private const string HandlerName = nameof(DocumentCommandHandler);

    public async Task<ErrorOr<Resume>> ExecuteAsync(
        DocumentCommand command,
        CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for resume {ResumeId}", HandlerName, command.ResumeId);

        var resume = await dbContext.Resumes
            .AsNoTracking()
            .Include(r => r.Person)
            .Include(r => r.Experiences.OrderBy(e => e.Order))
            .Include(r => r.Educations.OrderBy(e => e.Order))
            .Include(r => r.Skills.OrderBy(s => s.Order))
            .Include(r => r.Languages.OrderBy(l => l.Order))
            .Include(r => r.Courses.OrderBy(c => c.Order))
            .Include(r => r.Links.OrderBy(l => l.Order))
            .FirstOrDefaultAsync(r => r.Id == command.ResumeId, ct);

        if (resume is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Resume {ResumeId} not found", HandlerName, command.ResumeId);

            return Error.NotFound("Resume.NotFound", $"Resume with id '{command.ResumeId}' was not found");
        }

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | Resume {ResumeId} loaded with all entities",
            HandlerName,
            command.ResumeId);

        return resume;
    }
}