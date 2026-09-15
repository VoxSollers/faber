using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Skills.DeleteSkill;

public record DeleteSkillCommand(Guid ResumeId, Guid Id) : ICommand<ErrorOr<bool>>;