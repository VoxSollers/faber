using System.Collections;
using Faber.Modules.Resumes.Application.Features.Languages.UpdateLanguage;
using Faber.Modules.Resumes.Application.Tests.Features.Shared.Data;

namespace Faber.Modules.Resumes.Application.Tests.Features.Languages.UpdateLanguage.Data;

public class InvalidUpdateLanguageData : IEnumerable<TheoryDataRow<UpdateLanguageRequest>>
{
    public IEnumerator<TheoryDataRow<UpdateLanguageRequest>> GetEnumerator()
    {
        var baseFaker = new CreateLanguageRequestFaker(Guid.Empty);

        yield return new TheoryDataRow<UpdateLanguageRequest>(
            new UpdateLanguageRequest(Guid.Empty, Guid.Empty, baseFaker.Generate().Name,
                "InvalidLevel"));

        yield return new TheoryDataRow<UpdateLanguageRequest>(
            new UpdateLanguageRequest(Guid.Empty, Guid.Empty, baseFaker.Generate().Name,
                "X9"));

        yield return new TheoryDataRow<UpdateLanguageRequest>(
            new UpdateLanguageRequest(Guid.Empty, Guid.Empty, new string('a', 101),
                baseFaker.Generate().Level));
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
