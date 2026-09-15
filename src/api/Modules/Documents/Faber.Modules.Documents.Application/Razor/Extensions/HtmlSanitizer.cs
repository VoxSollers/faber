using System.Net;
using System.Text.RegularExpressions;

namespace Faber.Modules.Documents.Application.Razor.Extensions;

public static partial class HtmlSanitizer
{
    public static string Sanitize(this string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var decoded = WebUtility.HtmlDecode(input);

        decoded = decoded
            .Replace("\u00A0", " ")
            .Replace("\u202F", " ")
            .Replace("\u2007", " ")
            .Replace("&nbsp;", " ")
            .Replace("\u200B", "")
            .Replace("\u200C", "")
            .Replace("\u200D", "")
            .Replace("\uFEFF", "")
            .Replace("\u2060", "")
            .Replace("\u00AD", "")
            .Replace("&shy;", "");

        decoded = BlindOrControlSymbolsRegex().Replace(decoded, "");
        decoded = TwoAndMoreSpaces().Replace(decoded, " ");

        return decoded.Trim();
    }

    [GeneratedRegex(@"[\p{Cc}&&[^\r\n\t]]")]
    private static partial Regex BlindOrControlSymbolsRegex();

    [GeneratedRegex(@"[ ]{2,}")]
    private static partial Regex TwoAndMoreSpaces();
}