using Bogus;
using Faber.Modules.Resumes.Application.Features.Links.CreateLink;

namespace Faber.Modules.Resumes.Application.Tests.Features.Shared.Data;

public class CreateLinkRequestFaker : Faker<CreateLinkRequest>
{
    public CreateLinkRequestFaker(Guid resumeId, int seed = ResumesTestConstants.LinkSeed)
    {
        UseSeed(seed);
        CustomInstantiator(f => new CreateLinkRequest(
            resumeId,
            f.PickRandom("GitHub", "LinkedIn", "Portfolio", "Twitter", "Blog", "GitLab"),
            f.Internet.Url()));
    }
}
