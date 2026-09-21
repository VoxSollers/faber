using Faber.Modules.Documents.Application.Templates;
using Faber.Modules.Resumes.Application.Caching;
using Shouldly;

namespace Faber.Modules.Resumes.Application.Tests.Features.Resumes.Caching;

public class ResumesCacheKeysTests
{
    [Fact]
    public void Pdf_WithTemplateType_ShouldKebabCaseTemplateSegment()
    {
        var userId = Guid.NewGuid();
        var resumeId = Guid.NewGuid();

        var key = ResumesCacheKeys.Pdf(userId, resumeId, typeof(FirstTemplate));

        key.ShouldContain($":pdf:{resumeId}:first-template:");
    }
}
