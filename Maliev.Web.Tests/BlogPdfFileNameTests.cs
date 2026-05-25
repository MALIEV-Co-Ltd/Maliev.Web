using Maliev.Web.Client.Content;

namespace Maliev.Web.Tests;

/// <summary>
/// Contract tests for practical note download file names.
/// </summary>
public sealed class BlogPdfFileNameTests
{
    /// <summary>
    /// Verifies practical note PDF names are ready for customer file organization.
    /// </summary>
    [Fact]
    public void BuildBlogPdfFileName_FdmPrintOrientation_UsesOrganizedName()
    {
        var post = SiteContent.BlogPosts.Single(item => item.Slug == "fdm-print-orientation");

        var fileName = SiteContent.BuildBlogPdfFileName(post);

        Assert.Equal("Practical note - FDM - Print Orientation - MALIEV.pdf", fileName);
    }
}
