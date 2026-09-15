using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Languages.UpdateLanguage;

public class UpdateLanguageCommandHandler(
    ResumesDbContext dbContext,
    ILogger<UpdateLanguageCommandHandler> logger)
    : ICommandHandler<UpdateLanguageCommand, ErrorOr<bool>>
{
    private const string HandlerName = nameof(UpdateLanguageCommandHandler);

    public async Task<ErrorOr<bool>> ExecuteAsync(UpdateLanguageCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {LanguageId}", HandlerName, command.Id);

        var language = await dbContext.Languages
            .FirstOrDefaultAsync(e => e.Id == command.Id && e.ResumeId == command.ResumeId, ct);

        if (language is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Language {LanguageId} not found", HandlerName, command.Id);

            return Error.NotFound("Language.NotFound", $"Language with id '{command.Id}' was not found");
        }

        language.Name = command.Name;
        language.Level = command.Level;

        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("[SUCCESS] {HandlerName} | Language {LanguageId} updated", HandlerName, command.Id);

        return true;
    }
}