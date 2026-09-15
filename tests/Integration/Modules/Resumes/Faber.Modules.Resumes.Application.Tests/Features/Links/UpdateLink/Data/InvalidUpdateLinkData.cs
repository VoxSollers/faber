using System.Collections;
using Faber.Modules.Resumes.Application.Features.Links.UpdateLink;
using Faber.Modules.Resumes.Application.Tests.Features.Shared.Data;
using static Faber.Modules.Resumes.Application.Tests.Features.Shared.ResumesTestConstants;

namespace Faber.Modules.Resumes.Application.Tests.Features.Links.UpdateLink.Data;

public class InvalidUpdateLinkData : IEnumerable<TheoryDataRow<UpdateLinkRequest>>
{
    public IEnumerator<TheoryDataRow<UpdateLinkRequest>> GetEnumerator()
    {
        var baseRequest = new CreateLinkRequestFaker(Guid.Empty, LinkSeed).Generate();

        yield return new TheoryDataRow<UpdateLinkRequest>(
            new UpdateLinkRequest(Guid.NewGuid(), Guid.Empty, new string('a', 256), baseRequest.Uri));

        yield return new TheoryDataRow<UpdateLinkRequest>(
            new UpdateLinkRequest(Guid.NewGuid(), Guid.Empty, baseRequest.Label, "not-a-valid-url"));

        yield return new TheoryDataRow<UpdateLinkRequest>(
            new UpdateLinkRequest(Guid.NewGuid(), Guid.Empty, baseRequest.Label,
                "https://example.com/" + new string('a', 2065)));
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
