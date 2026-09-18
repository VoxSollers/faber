using ErrorOr;
using Faber.Modules.Resumes.Application.Features.Resumes.Shared;
using Faber.Modules.Resumes.Domain.Entities;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Projects.CreateProject;

public class CreateProjectCommandHandler(
    ResumesDbContext dbContext,
    ILogger<CreateProjectCommandHandler> logger)
    : ICommandHandler<CreateProjectCommand, ErrorOr<CreateProjectResponse>>
{
    private const string HandlerName = nameof(CreateProjectCommandHandler);

    public async Task<ErrorOr<CreateProjectResponse>> ExecuteAsync(CreateProjectCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for resume {ResumeId}", HandlerName, command.ResumeId);

        var resume = await dbContext.Resumes
            .FirstOrDefaultAsync(r => r.Id == command.ResumeId, ct);

        if (resume is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Resume {ResumeId} not found", HandlerName, command.ResumeId);

            return Error.NotFound("Resume.NotFound", $"Resume with id '{command.ResumeId}' was not found");
        }

        var maxOrder = await dbContext.Projects
            .Where(c => c.ResumeId == command.ResumeId)
            .MaxAsync(c => (int?)c.Order, ct);

        var project = new Project
        {
            Id = Guid.NewGuid(),
            ResumeId = command.ResumeId,
            Role = command.Role,
            Name = command.Name,
            Url = command.Url,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            Description = HtmlContentSanitizer.SanitizeBasicFormatting(command.Description),
            Order = (maxOrder ?? -1) + 1
        };

        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | Project {ProjectId} created for resume {ResumeId}",
            HandlerName,
            project.Id,
            command.ResumeId);

        return project.MapToResponse();
    }
}