using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Languages.GetLanguage;

public class GetLanguageCommandHandler(
    ResumesDbContext dbContext,
    ILogger<GetLanguageCommandHandler> logger)
    : ICommandHandler<GetLanguageCommand, ErrorOr<GetLanguageResponse>>
{
    private const string HandlerName = nameof(GetLanguageCommandHandler);

    public async Task<ErrorOr<GetLanguageResponse>> ExecuteAsync(GetLanguageCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {LanguageId}", HandlerName, command.Id);

        var language = await dbContext.Languages
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == command.Id && e.ResumeId == command.ResumeId, ct);

        if (language is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Language {LanguageId} not found", HandlerName, command.Id);

            return Error.NotFound("Language.NotFound", $"Language with id '{command.Id}' was not found");
        }

        logger.LogInformation("[SUCCESS] {HandlerName} | Language {LanguageId} found", HandlerName, command.Id);

        return language.MapToResponse();
    }
}