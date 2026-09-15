using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Skills.GetAllSkills;

public record GetAllSkillsCommand(Guid ResumeId) : ICommand<ErrorOr<GetAllSkillsResponse>>;