using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Languages.GetAllLanguages;

public class GetAllLanguagesCommandHandler(
    ResumesDbContext dbContext,
    ILogger<GetAllLanguagesCommandHandler> logger)
    : ICommandHandler<GetAllLanguagesCommand, ErrorOr<GetAllLanguagesResponse>>
{
    private const string HandlerName = nameof(GetAllLanguagesCommandHandler);

    public async Task<ErrorOr<GetAllLanguagesResponse>> ExecuteAsync(
        GetAllLanguagesCommand command,
        CancellationToken ct)
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

        var languages = await dbContext.Languages
            .AsNoTracking()
            .Where(e => e.ResumeId == command.ResumeId)
            .OrderBy(e => e.Order)
            .ToListAsync(ct);

        var items = languages.Select(e => e.MapToItem()).ToList();

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | Found {Count} languages for resume {ResumeId}",
            HandlerName,
            items.Count,
            command.ResumeId);

        return new GetAllLanguagesResponse(items);
    }
}