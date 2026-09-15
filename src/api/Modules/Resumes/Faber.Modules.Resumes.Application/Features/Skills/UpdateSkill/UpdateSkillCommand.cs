using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Skills.UpdateSkill;

public record UpdateSkillCommand(
    Guid Id,
    Guid ResumeId,
    string? Name,
    string? Level) : ICommand<ErrorOr<bool>>;