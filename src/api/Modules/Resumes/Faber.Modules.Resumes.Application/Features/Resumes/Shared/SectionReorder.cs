using ErrorOr;
using Faber.Modules.Resumes.Domain.Entities;
using Faber.Modules.Resumes.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Faber.Modules.Resumes.Application.Features.Resumes.Shared;

/// <summary>
/// Shared, transactional reordering for resume section collections. The server owns
/// <see cref="IOrderable.Order"/>: the supplied id list must match the resume's items
/// exactly, after which each item's <c>Order</c> is normalised to its index (0..n-1).
/// </summary>
public static class SectionReorder
{
    public static async Task<ErrorOr<bool>> ApplyAsync<T>(
        ResumesDbContext dbContext,
        DbSet<T> set,
        Guid resumeId,
        IReadOnlyList<Guid> orderedIds,
        CancellationToken ct)
        where T : ResumeSection, IOrderable
    {
        var items = await set
            .Where(x => x.ResumeId == resumeId)
            .ToListAsync(ct);

        var byId = items.ToDictionary(x => x.Id);

        if (orderedIds.Count != items.Count || orderedIds.Any(id => !byId.ContainsKey(id)))
        {
            return Error.NotFound(
                "Reorder.Mismatch",
                "The provided ids must match the resume's items exactly.");
        }

        for (var i = 0; i < orderedIds.Count; i++)
        {
            byId[orderedIds[i]].Order = i;
        }

        await dbContext.SaveChangesAsync(ct);

        return true;
    }
}
