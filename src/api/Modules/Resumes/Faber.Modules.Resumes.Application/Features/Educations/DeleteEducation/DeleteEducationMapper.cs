namespace Faber.Modules.Resumes.Application.Features.Educations.DeleteEducation;

public static class DeleteEducationMapper
{
    public static DeleteEducationCommand MapToCommand(this DeleteEducationRequest request)
    {
        return new DeleteEducationCommand(request.ResumeId, request.Id);
    }
}