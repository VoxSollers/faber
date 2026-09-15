namespace Faber.Modules.Resumes.Application.Features.Persons.DeletePerson;

public static class DeletePersonMapper
{
    public static DeletePersonCommand MapToCommand(this DeletePersonRequest request)
    {
        return new DeletePersonCommand(request.ResumeId, request.Id);
    }
}