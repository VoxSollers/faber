using System.Collections;
using Faber.Modules.Resumes.Application.Features.Links.CreateLink;
using Faber.Modules.Resumes.Application.Tests.Features.Shared.Data;

namespace Faber.Modules.Resumes.Application.Tests.Features.Links.CreateLink.Data;

public class InvalidLinkData : IEnumerable<TheoryDataRow<CreateLinkRequest>>
{
    public IEnumerator<TheoryDataRow<CreateLinkRequest>> GetEnumerator()
    {
        var baseFaker = new CreateLinkRequestFaker(Guid.NewGuid());

        yield return new TheoryDataRow<CreateLinkRequest>(
            baseFaker.RuleFor(r => r.Uri, "not-a-valid-url").Generate());

        yield return new TheoryDataRow<CreateLinkRequest>(
            baseFaker.RuleFor(r => r.Label, new string('a', 256)).Generate());

        yield return new TheoryDataRow<CreateLinkRequest>(
            baseFaker.RuleFor(r => r.Uri, "https://example.com/" + new string('a', 2065)).Generate());
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
