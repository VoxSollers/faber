using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Links.DeleteLink;

public class DeleteLinkCommandHandler(
    ResumesDbContext dbContext,
    ILogger<DeleteLinkCommandHandler> logger)
    : ICommandHandler<DeleteLinkCommand, ErrorOr<bool>>
{
    private const string HandlerName = nameof(DeleteLinkCommandHandler);

    public async Task<ErrorOr<bool>> ExecuteAsync(DeleteLinkCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {LinkId}", HandlerName, command.Id);

        var link = await dbContext.Links
            .FirstOrDefaultAsync(e => e.Id == command.Id && e.ResumeId == command.ResumeId, ct);

        if (link is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Link {LinkId} not found", HandlerName, command.Id);

            return Error.NotFound("Link.NotFound", $"Link with id '{command.Id}' was not found");
        }

        dbContext.Links.Remove(link);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("[SUCCESS] {HandlerName} | Link {LinkId} deleted", HandlerName, command.Id);

        return true;
    }
}