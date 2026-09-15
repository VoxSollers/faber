using Ganss.Xss;

namespace Faber.Modules.Resumes.Application.Features.Resumes.Shared;

public static partial class HtmlContentSanitizer
{
    private static readonly HtmlSanitizer Sanitizer = CreateSanitizer();

    public static string? SanitizeBasicFormatting(string? value)
    {
        if (value is null)
        {
            return null;
        }

        return Sanitizer.Sanitize(value);
    }

    public static int GetPlainTextLength(string? html)
    {
        if (html is null)
        {
            return 0;
        }

        return Sanitizer.SanitizeDom(html).Body?.TextContent.Length ?? 0;
    }

    private static HtmlSanitizer CreateSanitizer()
    {
        var sanitizer = new HtmlSanitizer
        {
            // Degrade unexpected tags to their text instead of dropping the content.
            KeepChildNodes = true,
        };

        sanitizer.AllowedTags.Clear();
        sanitizer.AllowedAttributes.Clear();
        sanitizer.AllowedCssProperties.Clear();
        sanitizer.AllowedAtRules.Clear();
        sanitizer.AllowedSchemes.Clear();

        // Tags emitted by the Tiptap editor (StarterKit v3 + TextAlign + Link).
        sanitizer.AllowedTags.Add("p");
        sanitizer.AllowedTags.Add("br");
        sanitizer.AllowedTags.Add("strong");
        sanitizer.AllowedTags.Add("b");
        sanitizer.AllowedTags.Add("em");
        sanitizer.AllowedTags.Add("i");
        sanitizer.AllowedTags.Add("u");
        sanitizer.AllowedTags.Add("s");
        sanitizer.AllowedTags.Add("h1");
        sanitizer.AllowedTags.Add("h2");
        sanitizer.AllowedTags.Add("h3");
        sanitizer.AllowedTags.Add("ul");
        sanitizer.AllowedTags.Add("ol");
        sanitizer.AllowedTags.Add("li");
        sanitizer.AllowedTags.Add("a");

        // Alignment is serialized as an inline `style="text-align: ..."` attribute.
        sanitizer.AllowedAttributes.Add("style");
        sanitizer.AllowedCssProperties.Add("text-align");

        sanitizer.AllowedAttributes.Add("href");
        sanitizer.AllowedSchemes.Add("http");
        sanitizer.AllowedSchemes.Add("https");
        sanitizer.AllowedSchemes.Add("mailto");

        return sanitizer;
    }
}
