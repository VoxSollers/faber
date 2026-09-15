namespace Faber.Modules.Resumes.Domain.Entities;

public class Course : ResumeSection, IOrderable
{
    public string? School { get; set; }

    public string? Name { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Description { get; set; }

    public int Order { get; set; }
}
