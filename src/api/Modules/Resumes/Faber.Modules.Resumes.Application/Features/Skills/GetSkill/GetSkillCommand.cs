using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Skills.GetSkill;

public record GetSkillCommand(Guid ResumeId, Guid Id) : ICommand<ErrorOr<GetSkillResponse>>;