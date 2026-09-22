using ErrorOr;
using Faber.Modules.Resumes.Domain.Entities;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Documents.Shared;

public record DocumentCommand(Guid ResumeId, Guid UserId) : ICommand<ErrorOr<Resume>>;