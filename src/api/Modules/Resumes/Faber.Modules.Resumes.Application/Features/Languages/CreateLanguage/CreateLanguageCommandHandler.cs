using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Languages.CreateLanguage;

public class CreateLanguageCommandHandler(
    ResumesDbContext dbContext,
    ILogger<CreateLanguageCommandHandler> logger)
    : ICommandHandler<CreateLanguageCommand, ErrorOr<CreateLanguageResponse>>
{
    private const string HandlerName = nameof(CreateLanguageCommandHandler);

    public async Task<ErrorOr<CreateLanguageResponse>> ExecuteAsync(CreateLanguageCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for resume {ResumeId}", HandlerName, command.ResumeId);

        var resume = await dbContext.Resumes
            .FirstOrDefaultAsync(r => r.Id == command.ResumeId, ct);

        if (resume is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Resume {ResumeId} not found", HandlerName, command.ResumeId);

            return Error.NotFound("Resume.NotFound", $"Resume with id '{command.ResumeId}' was not found");
        }

        var maxOrder = await dbContext.Languages
            .Where(l => l.ResumeId == command.ResumeId)
            .MaxAsync(l => (int?)l.Order, ct);

        var language = new Domain.Entities.Language
        {
            Id = Guid.NewGuid(),
            ResumeId = command.ResumeId,
            Name = command.Name,
            Level = command.Level,
            Order = (maxOrder ?? -1) + 1
        };

        dbContext.Languages.Add(language);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | Language {LanguageId} created for resume {ResumeId}",
            HandlerName,
            language.Id,
            command.ResumeId);

        return language.MapToResponse();
    }
}