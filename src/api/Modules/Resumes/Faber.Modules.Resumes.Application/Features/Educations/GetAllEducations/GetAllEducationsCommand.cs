using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Educations.GetAllEducations;

public record GetAllEducationsCommand(Guid ResumeId) : ICommand<ErrorOr<GetAllEducationsResponse>>;