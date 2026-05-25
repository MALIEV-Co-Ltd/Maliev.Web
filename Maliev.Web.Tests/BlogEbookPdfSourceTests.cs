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
    public void BlogEbookPdfIsRenderedByPdfServiceNotWebBff()
    {
        var source = ReadRepoFile("Maliev.Web.Bff", "Services", "BlogEbookPdfService.cs");
        var client = ReadRepoFile("Maliev.Web.Bff", "Clients", "PdfServiceClient.cs");
        var program = ReadRepoFile("Maliev.Web.Bff", "Program.cs");
        var project = ReadRepoFile("Maliev.Web.Bff", "Maliev.Web.Bff.csproj");

        Assert.Contains("IPdfServiceClient", source);
        Assert.Contains("RenderBlogPracticalNoteAsync", source);
        Assert.Contains("builder.AddAuthenticatedServiceClient<IPdfServiceClient, PdfServiceClient>(\"PdfService\")", program);
        Assert.Contains("/pdf/v1/blog-practical-notes/render", client);
        Assert.DoesNotContain("QuestPDF", project);
        Assert.DoesNotContain("ZXing", project);
        Assert.DoesNotContain("using QuestPDF", source);
        Assert.DoesNotContain("using ZXing", source);
    }

    /// <summary>
    /// Verifies the Web BFF includes localized content and image bytes in the PdfService request.
    /// </summary>
    [Fact]
    public void BlogEbookPdfMapsLocalizedContentAndImagesForPdfService()
    {
        var source = ReadRepoFile("Maliev.Web.Bff", "Services", "BlogEbookPdfService.cs");

        Assert.Contains("Title = post.Title.For(normalizedCulture)", source);
        Assert.Contains("Summary = post.Summary.For(normalizedCulture)", source);
        Assert.Contains("PublicUrl = $\"{PublicSiteBaseUrl}/blog/{Uri.EscapeDataString(post.Slug)}\"", source);
        Assert.Contains("CoverImage = await MapImage(post.ImageUrl", source);
        Assert.Contains("Image = section.Image is null ? null : await MapImage(", source);
        Assert.Contains("await File.ReadAllBytesAsync(imagePath, cancellationToken)", source);
        Assert.DoesNotContain("TrimToLength", source);
    }

    private static string ReadRepoFile(params string[] pathSegments)
    {
        var root = FindRepoRoot();
        return File.ReadAllText(Path.Combine([root, .. pathSegments]));
    }

    private static string FindRepoRoot()
    {
        foreach (var startDirectory in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            var directory = new DirectoryInfo(startDirectory);

            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "Maliev.Web.slnx")))
                {
                    return directory.FullName;
                }

                var siblingCandidate = Path.Combine(directory.FullName, "Maliev.Web");
                if (File.Exists(Path.Combine(siblingCandidate, "Maliev.Web.slnx")))
                {
                    return siblingCandidate;
                }

                directory = directory.Parent;
            }
        }

        throw new DirectoryNotFoundException("Could not locate Maliev.Web repository root.");
    }
}
