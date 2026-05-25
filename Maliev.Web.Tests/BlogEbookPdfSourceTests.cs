namespace Maliev.Web.Tests;

/// <summary>
/// Source-level contract tests for generated practical note PDFs.
/// </summary>
public sealed class BlogEbookPdfSourceTests
{
    /// <summary>
    /// Verifies the PDF cover is print-friendly and links readers back to the public blog post.
    /// </summary>
    [Fact]
    public void BlogEbookPdfCoverUsesPrintableLogoTitleAndBlogQr()
    {
        var source = ReadRepoFile("Maliev.Web.Bff", "Services", "BlogEbookPdfService.cs");

        Assert.Contains("LogoPath", source);
        Assert.Contains(".Svg(LogoPath)", source);
        Assert.Contains("BlogPostUrl", source);
        Assert.Contains("QRCodeWriter", source);
        Assert.Contains("ComposeQrCode", source);
        Assert.Contains("page.Size(PageSizes.A4);", source);
        Assert.Contains("post.Title.For(cultureName)", source);
        Assert.DoesNotContain(".Background(DarkPanel)\r\n                    .Padding(32)", source);
        Assert.DoesNotContain(".Background(DarkPanel)\n                    .Padding(32)", source);
        Assert.DoesNotContain("ExtendVertical", source);
        Assert.DoesNotContain("column.Item().Text(\"MALIEV\").FontSize(20).Bold()", source);
    }

    /// <summary>
    /// Verifies article images and navigable table-of-contents entries are part of the PDF structure.
    /// </summary>
    [Fact]
    public void BlogEbookPdfIncludesImagesAndNavigableContents()
    {
        var source = ReadRepoFile("Maliev.Web.Bff", "Services", "BlogEbookPdfService.cs");

        Assert.Contains("ComposeArticleImage", source);
        Assert.Contains("section.Image is { } image", source);
        Assert.Contains(".Image(imagePath)", source);
        Assert.Contains("SemanticTableOfContents", source);
        Assert.Contains("SemanticTableOfContentsItem", source);
        Assert.Contains("SemanticHeader1", source);
        Assert.Contains("SemanticHeader2", source);
        Assert.Contains("PDFUA_Conformance = PDFUA_Conformance.PDFUA_1", source);
        Assert.Contains("SectionLink(sectionId)", source);
        Assert.Contains("BeginPageNumberOfSection(sectionId)", source);
        Assert.DoesNotContain("TrimToLength", source);
    }

    /// <summary>
    /// Verifies footer branding and CTA URLs stay consistent across generated pages.
    /// </summary>
    [Fact]
    public void BlogEbookPdfUsesMalievFooterAndHttpsCtaLinks()
    {
        var source = ReadRepoFile("Maliev.Web.Bff", "Services", "BlogEbookPdfService.cs");

        Assert.Contains("private const string QuoteUrl = \"https://quote.maliev.com/projects/new\";", source);
        Assert.Contains("private const string MaterialsUrl = \"https://www.maliev.com/materials\";", source);
        Assert.Contains("ComposeFooter(page);", source);
        Assert.Contains("Hyperlink(url)", source);
        Assert.DoesNotContain("ComposeFooter(page, post.Category.For(cultureName))", source);
        Assert.DoesNotContain("ComposeFooter(page, \"Practical note\")", source);
        Assert.DoesNotContain("ContactCard(item, \"Compare materials\", \"www.maliev.com/materials\")", source);
    }

    private static string ReadRepoFile(params string[] pathSegments)
    {
        var root = FindRepoRoot();
        return File.ReadAllText(Path.Combine([root, .. pathSegments]));
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Maliev.Web.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate Maliev.Web repository root.");
    }
}
