using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Links.UpdateLink;

public class UpdateLinkCommandHandler(
    ResumesDbContext dbContext,
    ILogger<UpdateLinkCommandHandler> logger)
    : ICommandHandler<UpdateLinkCommand, ErrorOr<bool>>
{
    private const string HandlerName = nameof(UpdateLinkCommandHandler);

    public async Task<ErrorOr<bool>> ExecuteAsync(UpdateLinkCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {LinkId}", HandlerName, command.Id);

        var link = await dbContext.Links
            .FirstOrDefaultAsync(e => e.Id == command.Id && e.ResumeId == command.ResumeId, ct);

        if (link is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Link {LinkId} not found", HandlerName, command.Id);

            return Error.NotFound("Link.NotFound", $"Link with id '{command.Id}' was not found");
        }

        link.Label = command.Label;
        link.Uri = command.Uri;

        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("[SUCCESS] {HandlerName} | Link {LinkId} updated", HandlerName, command.Id);

        return true;
    }
}
