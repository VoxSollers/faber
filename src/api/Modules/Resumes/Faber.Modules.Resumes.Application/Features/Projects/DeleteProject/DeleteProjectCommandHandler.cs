using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Projects.DeleteProject;

public class DeleteProjectCommandHandler(
    ResumesDbContext dbContext,
    ILogger<DeleteProjectCommandHandler> logger)
    : ICommandHandler<DeleteProjectCommand, ErrorOr<bool>>
{
    private const string HandlerName = nameof(DeleteProjectCommandHandler);

    public async Task<ErrorOr<bool>> ExecuteAsync(DeleteProjectCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {ProjectId}", HandlerName, command.Id);

        var project = await dbContext.Projects
            .FirstOrDefaultAsync(c => c.Id == command.Id && c.ResumeId == command.ResumeId, ct);

        if (project is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Project {ProjectId} not found", HandlerName, command.Id);

            return Error.NotFound("Project.NotFound", $"Project with id '{command.Id}' was not found");
        }

        dbContext.Projects.Remove(project);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("[SUCCESS] {HandlerName} | Project {ProjectId} deleted", HandlerName, command.Id);

        return true;
    }
}