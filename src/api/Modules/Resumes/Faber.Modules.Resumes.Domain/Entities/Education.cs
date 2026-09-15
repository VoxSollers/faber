namespace Faber.Modules.Resumes.Domain.Entities;

public class Education : ResumeSection, IOrderable
{
    public string? School { get; set; }

    public string? Degree { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? City { get; set; }

    public string? Description { get; set; }

    public int Order { get; set; }
}
