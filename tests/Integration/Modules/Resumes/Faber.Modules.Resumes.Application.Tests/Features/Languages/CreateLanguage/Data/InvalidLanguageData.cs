using System.Collections;
using Faber.Modules.Resumes.Application.Features.Languages.CreateLanguage;
using Faber.Modules.Resumes.Application.Tests.Features.Shared.Data;
using static Faber.Modules.Resumes.Application.Tests.Features.Shared.ResumesTestConstants;

namespace Faber.Modules.Resumes.Application.Tests.Features.Languages.CreateLanguage.Data;

public class InvalidLanguageData : IEnumerable<TheoryDataRow<CreateLanguageRequest>>
{
    public IEnumerator<TheoryDataRow<CreateLanguageRequest>> GetEnumerator()
    {
        var resumeId = Guid.Empty;

        yield return new TheoryDataRow<CreateLanguageRequest>(
            new CreateLanguageRequest(resumeId, "Klingon", "InvalidLevel"));

        yield return new TheoryDataRow<CreateLanguageRequest>(
            new CreateLanguageRequest(resumeId, "Korean", "X9"));

        yield return new TheoryDataRow<CreateLanguageRequest>(
            new CreateLanguageRequest(resumeId, new string('a', 101), "B2"));
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
