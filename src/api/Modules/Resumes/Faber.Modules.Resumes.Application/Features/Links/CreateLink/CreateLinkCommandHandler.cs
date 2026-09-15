using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Links.CreateLink;

public class CreateLinkCommandHandler(
    ResumesDbContext dbContext,
    ILogger<CreateLinkCommandHandler> logger)
    : ICommandHandler<CreateLinkCommand, ErrorOr<CreateLinkResponse>>
{
    private const string HandlerName = nameof(CreateLinkCommandHandler);

    public async Task<ErrorOr<CreateLinkResponse>> ExecuteAsync(CreateLinkCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for resume {ResumeId}", HandlerName, command.ResumeId);

        var resume = await dbContext.Resumes
            .FirstOrDefaultAsync(r => r.Id == command.ResumeId, ct);

        if (resume is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Resume {ResumeId} not found", HandlerName, command.ResumeId);

            return Error.NotFound("Resume.NotFound", $"Resume with id '{command.ResumeId}' was not found");
        }

        var maxOrder = await dbContext.Links
            .Where(l => l.ResumeId == command.ResumeId)
            .MaxAsync(l => (int?)l.Order, ct);

        var link = new Domain.Entities.Link
        {
            Id = Guid.NewGuid(),
            ResumeId = command.ResumeId,
            Label = command.Label,
            Uri = command.Uri,
            Order = (maxOrder ?? -1) + 1
        };

        dbContext.Links.Add(link);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | Link {LinkId} created for resume {ResumeId}",
            HandlerName,
            link.Id,
            command.ResumeId);

        return link.MapToResponse();
    }
}