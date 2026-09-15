using ErrorOr;
using Faber.Modules.Resumes.Application.Features.Resumes.Shared;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Resumes.UpdateHobbies;

public class UpdateHobbiesCommandHandler(
    ResumesDbContext dbContext,
    ILogger<UpdateHobbiesCommandHandler> logger)
    : ICommandHandler<UpdateHobbiesCommand, ErrorOr<bool>>
{
    private const string HandlerName = nameof(UpdateHobbiesCommandHandler);

    public async Task<ErrorOr<bool>> ExecuteAsync(UpdateHobbiesCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {ResumeId}", HandlerName, command.Id);

        var resume = await dbContext.Resumes
            .FirstOrDefaultAsync(r => r.Id == command.Id && r.UserId == command.UserId, ct);

        if (resume is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Resume {ResumeId} not found", HandlerName, command.Id);

            return Error.NotFound("Resume.NotFound", $"Resume with id '{command.Id}' was not found");
        }

        resume.Hobbies = HtmlContentSanitizer.SanitizeBasicFormatting(command.Hobbies);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("[SUCCESS] {HandlerName} | Resume {ResumeId} hobbies updated", HandlerName, command.Id);

        return true;
    }
}
