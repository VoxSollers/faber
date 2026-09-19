using ErrorOr;
using Faber.Modules.Resumes.Application.Features.Resumes.Shared;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Projects.UpdateProject;

public class UpdateProjectCommandHandler(
    ResumesDbContext dbContext,
    ILogger<UpdateProjectCommandHandler> logger)
    : ICommandHandler<UpdateProjectCommand, ErrorOr<bool>>
{
    private const string HandlerName = nameof(UpdateProjectCommandHandler);

    public async Task<ErrorOr<bool>> ExecuteAsync(UpdateProjectCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {ProjectId}", HandlerName, command.Id);

        var project = await dbContext.Projects
            .FirstOrDefaultAsync(c => c.Id == command.Id && c.ResumeId == command.ResumeId, ct);

        if (project is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Project {ProjectId} not found", HandlerName, command.Id);

            return Error.NotFound("Project.NotFound", $"Project with id '{command.Id}' was not found");
        }

        project.Tagline = command.Tagline;
        project.Name = command.Name;
        project.Url = command.Url;
        project.StartDate = command.StartDate;
        project.EndDate = command.EndDate;
        project.Description = HtmlContentSanitizer.SanitizeBasicFormatting(command.Description);

        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("[SUCCESS] {HandlerName} | Project {ProjectId} updated", HandlerName, command.Id);

        return true;
    }
}
