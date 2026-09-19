using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Projects.GetProject;

public class GetProjectCommandHandler(
    ResumesDbContext dbContext,
    ILogger<GetProjectCommandHandler> logger)
    : ICommandHandler<GetProjectCommand, ErrorOr<GetProjectResponse>>
{
    private const string HandlerName = nameof(GetProjectCommandHandler);

    public async Task<ErrorOr<GetProjectResponse>> ExecuteAsync(GetProjectCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {ProjectId}", HandlerName, command.Id);

        var project = await dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == command.Id && c.ResumeId == command.ResumeId, ct);

        if (project is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Project {ProjectId} not found", HandlerName, command.Id);

            return Error.NotFound("Project.NotFound", $"Project with id '{command.Id}' was not found");
        }

        logger.LogInformation("[SUCCESS] {HandlerName} | Project {ProjectId} found", HandlerName, command.Id);

        return project.MapToResponse();
    }
}