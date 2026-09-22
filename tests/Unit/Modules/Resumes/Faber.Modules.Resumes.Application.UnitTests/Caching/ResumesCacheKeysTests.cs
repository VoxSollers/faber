using Faber.Modules.Documents.Application.Templates;
using Faber.Modules.Resumes.Application.Caching;
using Shouldly;

namespace Faber.Modules.Resumes.Application.UnitTests.Caching;

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

    [Fact]
    public void Pdf_WithAcronymInTemplateName_ShouldKeepAcronymTogether()
    {
        var userId = Guid.NewGuid();
        var resumeId = Guid.NewGuid();

        var key = ResumesCacheKeys.Pdf(userId, resumeId, typeof(PdfATSTemplate));

        key.ShouldContain($":pdf:{resumeId}:pdf-ats-template:");
    }

    // Dummy type used only to exercise ToKebabCase's acronym handling ("ATS") — no real
    // template with this shape exists yet.
    private sealed class PdfATSTemplate;
}
