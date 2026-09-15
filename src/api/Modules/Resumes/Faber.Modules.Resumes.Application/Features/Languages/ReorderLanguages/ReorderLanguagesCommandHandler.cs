using ErrorOr;
using Faber.Modules.Resumes.Application.Features.Resumes.Shared;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Languages.ReorderLanguages;

public class ReorderLanguagesCommandHandler(
    ResumesDbContext dbContext,
    ILogger<ReorderLanguagesCommandHandler> logger)
    : ICommandHandler<ReorderLanguagesCommand, ErrorOr<bool>>
{
    private const string HandlerName = nameof(ReorderLanguagesCommandHandler);

    public async Task<ErrorOr<bool>> ExecuteAsync(ReorderLanguagesCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for resume {ResumeId}", HandlerName, command.ResumeId);

        var result = await SectionReorder.ApplyAsync(
            dbContext,
            dbContext.Languages,
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
