using ErrorOr;
using Faber.Modules.Resumes.Domain.Entities;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Resumes.CreateResume;

public class CreateResumeCommandHandler(
    ResumesDbContext dbContext,
    ILogger<CreateResumeCommandHandler> logger)
    : ICommandHandler<CreateResumeCommand, ErrorOr<CreateResumeResponse>>
{
    private const string HandlerName = nameof(CreateResumeCommandHandler);

    public async Task<ErrorOr<CreateResumeResponse>> ExecuteAsync(CreateResumeCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {UserId}", HandlerName, command.UserId);

        var resume = new Resume
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            CreatedAt = DateTimeOffset.UtcNow,
            Localization = command.Localization
        };

        dbContext.Resumes.Add(resume);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | Resume {ResumeId} created for {UserId}",
            HandlerName,
            resume.Id,
            command.UserId);

        return resume.MapToResponse();
    }
}