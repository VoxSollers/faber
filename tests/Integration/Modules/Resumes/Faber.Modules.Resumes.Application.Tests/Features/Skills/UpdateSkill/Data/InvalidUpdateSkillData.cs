using System.Collections;
using Faber.Modules.Resumes.Application.Features.Skills.UpdateSkill;
using Faber.Modules.Resumes.Application.Tests.Features.Shared.Data;

namespace Faber.Modules.Resumes.Application.Tests.Features.Skills.UpdateSkill.Data;

public class InvalidUpdateSkillData : IEnumerable<TheoryDataRow<UpdateSkillRequest>>
{
    public IEnumerator<TheoryDataRow<UpdateSkillRequest>> GetEnumerator()
    {
        var baseFaker = new CreateSkillRequestFaker(Guid.Empty);

        yield return new TheoryDataRow<UpdateSkillRequest>(
            new UpdateSkillRequest(Guid.Empty, Guid.Empty, new string('a', 256),
                baseFaker.Generate().Level));

        yield return new TheoryDataRow<UpdateSkillRequest>(
            new UpdateSkillRequest(Guid.Empty, Guid.Empty, baseFaker.Generate().Name,
                "InvalidLevel"));
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
