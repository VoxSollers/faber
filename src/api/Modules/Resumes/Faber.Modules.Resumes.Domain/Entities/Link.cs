namespace Faber.Modules.Resumes.Domain.Entities;

public class Link : ResumeSection, IOrderable
{
    public string? Label { get; set; }

    public string? Uri { get; set; }

    public int Order { get; set; }
}
