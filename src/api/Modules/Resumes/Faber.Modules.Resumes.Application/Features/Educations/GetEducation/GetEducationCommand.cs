using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Educations.GetEducation;

public record GetEducationCommand(Guid ResumeId, Guid Id) : ICommand<ErrorOr<GetEducationResponse>>;