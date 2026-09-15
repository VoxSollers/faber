using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Links.GetLink;

public class GetLinkCommandHandler(
    ResumesDbContext dbContext,
    ILogger<GetLinkCommandHandler> logger)
    : ICommandHandler<GetLinkCommand, ErrorOr<GetLinkResponse>>
{
    private const string HandlerName = nameof(GetLinkCommandHandler);

    public async Task<ErrorOr<GetLinkResponse>> ExecuteAsync(GetLinkCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {LinkId}", HandlerName, command.Id);

        var link = await dbContext.Links
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == command.Id && e.ResumeId == command.ResumeId, ct);

        if (link is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Link {LinkId} not found", HandlerName, command.Id);

            return Error.NotFound("Link.NotFound", $"Link with id '{command.Id}' was not found");
        }

        logger.LogInformation("[SUCCESS] {HandlerName} | Link {LinkId} found", HandlerName, command.Id);

        return link.MapToResponse();
    }
}