using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Resumes.DeleteResume;

public class DeleteResumeCommandHandler(
    ResumesDbContext dbContext,
    ILogger<DeleteResumeCommandHandler> logger)
    : ICommandHandler<DeleteResumeCommand, ErrorOr<bool>>
{
    private const string HandlerName = nameof(DeleteResumeCommandHandler);

    public async Task<ErrorOr<bool>> ExecuteAsync(DeleteResumeCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {ResumeId}", HandlerName, command.Id);

        var resume = await dbContext.Resumes
            .FirstOrDefaultAsync(r => r.Id == command.Id && r.UserId == command.UserId, ct);

        if (resume is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Resume {ResumeId} not found", HandlerName, command.Id);

            return Error.NotFound("Resume.NotFound", $"Resume with id '{command.Id}' was not found");
        }

        dbContext.Resumes.Remove(resume);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("[SUCCESS] {HandlerName} | Resume {ResumeId} deleted", HandlerName, command.Id);

        return true;
    }
}