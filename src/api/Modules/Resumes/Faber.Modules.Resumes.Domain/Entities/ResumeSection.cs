namespace Faber.Modules.Resumes.Domain.Entities;

public abstract class ResumeSection
{
    public Guid Id { get; set; }

    public Guid ResumeId { get; set; }

    public Resume? Resume { get; set; }
}