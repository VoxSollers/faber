namespace Faber.Modules.Resumes.Application.Caching;

/// <summary>
/// Builds the cache keys and tags used to cache resume reads (and, eventually, rendered PDFs) in
/// <see cref="Microsoft.Extensions.Caching.Hybrid.HybridCache"/>.
/// </summary>
/// <remarks>
/// Every key embeds the <see cref="Build"/> identifier of this assembly so a deployment that
/// changes a cached DTO's shape automatically busts every previously cached entry instead of
/// risking a failed deserialization of stale data.
/// </remarks>
public static class ResumesCacheKeys
{
    /// <summary>The module version id of this assembly, embedded in every cache key so a new build busts stale entries.</summary>
    private static readonly Guid Build = typeof(ResumesCacheKeys).Assembly.ManifestModule.ModuleVersionId;

    /// <summary>Builds the cache key for a user's resume list.</summary>
    /// <param name="userId">The id of the user whose resume list is cached.</param>
    /// <returns>The cache key for the user's resume list.</returns>
    public static string List(Guid userId)
    {
        return $"resumes:{Build}:user:{userId}:list";
    }

    /// <summary>Builds the cache key for a single resume.</summary>
    /// <param name="userId">The id of the user who owns the resume.</param>
    /// <param name="resumeId">The id of the cached resume.</param>
    /// <returns>The cache key for the resume.</returns>
    public static string Resume(Guid userId, Guid resumeId)
    {
        return $"resumes:{Build}:user:{userId}:resume:{resumeId}";
    }

    /// <summary>Builds the cache key for a rendered resume PDF.</summary>
    /// <param name="userId">The id of the user who owns the resume.</param>
    /// <param name="resumeId">The id of the resume the PDF was rendered from.</param>
    /// <param name="template">The template type used to render the PDF.</param>
    /// <returns>The cache key for the rendered PDF.</returns>
    public static string Pdf(Guid userId, Guid resumeId, Type template)
    {
        var templateBuild = template.Assembly.ManifestModule.ModuleVersionId;

        return $"resumes:{Build}:user:{userId}:pdf:{resumeId}:{template.Name}:{templateBuild}";
    }

    /// <summary>Builds the cache tag shared by every cache entry derived from a single resume.</summary>
    /// <param name="resumeId">The id of the resume the tag scopes cache invalidation to.</param>
    /// <returns>The cache tag for the resume.</returns>
    public static string ResumeTag(Guid resumeId)
    {
        return $"resume:{resumeId}";
    }

    /// <summary>Builds the cache tag shared by every cache entry derived from a user's resume list.</summary>
    /// <param name="userId">The id of the user the tag scopes cache invalidation to.</param>
    /// <returns>The cache tag for the user's resume list.</returns>
    public static string UserResumesTag(Guid userId)
    {
        return $"user:{userId}:resumes";
    }
}
