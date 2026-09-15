using Faber.Modules.Resumes.Application.Features.Resumes.Shared;
using Shouldly;

namespace Faber.Modules.Resumes.Application.UnitTests.Features.Resumes.Shared;

public class HtmlContentSanitizerTests
{
    [Fact]
    public void Null_ShouldReturnNull()
    {
        HtmlContentSanitizer.SanitizeBasicFormatting(null).ShouldBeNull();
    }

    [Theory]
    [InlineData("<p><strong>bold</strong></p>", "bold")]
    [InlineData("<p><em>italic</em></p>", "italic")]
    [InlineData("<p><u>underline</u></p>", "underline")]
    [InlineData("<p><s>strike</s></p>", "strike")]
    [InlineData("<h2>Heading two</h2>", "Heading two")]
    [InlineData("<h3>Heading three</h3>", "Heading three")]
    [InlineData("<ul><li><p>bullet</p></li></ul>", "bullet")]
    [InlineData("<ol><li><p>ordered</p></li></ol>", "ordered")]
    public void TiptapFormatting_ShouldKeepTagAndContent(string input, string expectedText)
    {
        var result = HtmlContentSanitizer.SanitizeBasicFormatting(input);

        result.ShouldNotBeNull();
        result.ShouldContain(expectedText);
    }

    [Theory]
    [InlineData("<p><strong>bold</strong></p>", "<strong>")]
    [InlineData("<p><em>italic</em></p>", "<em>")]
    [InlineData("<p><s>strike</s></p>", "<s>")]
    [InlineData("<h2>Heading</h2>", "<h2")]
    [InlineData("<ol><li><p>ordered</p></li></ol>", "<ol")]
    public void TiptapFormatting_ShouldPreserveAllowedTag(string input, string expectedTag)
    {
        HtmlContentSanitizer.SanitizeBasicFormatting(input)
            .ShouldNotBeNull()
            .ShouldContain(expectedTag);
    }

    [Fact]
    public void Alignment_ShouldKeepTextAlignStyle()
    {
        const string input = "<p style=\"text-align: center\">centered</p>";

        var result = HtmlContentSanitizer.SanitizeBasicFormatting(input);

        result.ShouldNotBeNull();
        result.ShouldContain("text-align");
        result.ShouldContain("center");
    }

    [Fact]
    public void Link_ShouldKeepHttpsHref()
    {
        const string input = "<p><a href=\"https://example.com\">site</a></p>";

        var result = HtmlContentSanitizer.SanitizeBasicFormatting(input);

        result.ShouldNotBeNull();
        result.ShouldContain("href");
        result.ShouldContain("https://example.com");
    }

    [Fact]
    public void ScriptTag_ShouldBeStripped()
    {
        // KeepChildNodes preserves user text, but the executable <script> element
        // itself must be removed — leftover text is inert when rendered as markup.
        const string input = "<p>safe</p><script>alert('xss')</script>";

        var result = HtmlContentSanitizer.SanitizeBasicFormatting(input);

        result.ShouldNotBeNull();
        result.ShouldContain("safe");
        result.ShouldNotContain("<script");
    }

    [Fact]
    public void JavascriptScheme_ShouldBeStripped()
    {
        const string input = "<p><a href=\"javascript:alert(1)\">x</a></p>";

        var result = HtmlContentSanitizer.SanitizeBasicFormatting(input);

        result.ShouldNotBeNull();
        result.ShouldNotContain("javascript:");
    }

    [Fact]
    public void DisallowedTag_ShouldKeepTextContent()
    {
        const string input = "<div><span>kept text</span></div>";

        var result = HtmlContentSanitizer.SanitizeBasicFormatting(input);

        result.ShouldNotBeNull();
        result.ShouldContain("kept text");
        result.ShouldNotContain("<span");
        result.ShouldNotContain("<div");
    }
}
