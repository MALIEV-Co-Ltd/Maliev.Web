namespace Maliev.Web.Tests;

/// <summary>
/// Source-level layout tests for public hero composition.
/// </summary>
public sealed class HeroLayoutSourceTests
{
    /// <summary>
    /// Verifies the home hero keeps the quote panel below the heading and reserves the side rail for the gizmo.
    /// </summary>
    [Fact]
    public void HomeHeroPlacesQuotePanelBelowHeadingAndUsesGizmo()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Pages", "Home.razor");

        Assert.Contains("<ManufacturingGizmo />", source);
        AssertQuotePanelFollowsHeading(source);
        Assert.DoesNotContain("<div class=\"hero-workspace\">\r\n        <InstantQuotePanel />", source);
    }

    /// <summary>
    /// Verifies the quote page uses the same direct upload-first hero composition.
    /// </summary>
    [Fact]
    public void QuoteHeroPlacesQuotePanelBelowHeadingAndUsesGizmo()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Pages", "Quote.razor");

        Assert.Contains("<ManufacturingGizmo />", source);
        AssertQuotePanelFollowsHeading(source);
        Assert.DoesNotContain("contract-panel", source);
    }

    /// <summary>
    /// Verifies the 3D gizmo runtime is lazy and constrained for mobile devices.
    /// </summary>
    [Fact]
    public void ManufacturingGizmoUsesLazyMobileFriendlyBabylonRuntime()
    {
        var source = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "manufacturing-gizmo.js");

        Assert.Contains("babylonjs@9.6.0", source);
        Assert.Contains("IntersectionObserver", source);
        Assert.Contains("setHardwareScalingLevel", source);
        Assert.Contains("ResizeObserver", source);
        Assert.Contains("powerPreference: \"low-power\"", source);
    }

    private static void AssertQuotePanelFollowsHeading(string source)
    {
        var headingIndex = source.IndexOf("<h1", StringComparison.Ordinal);
        var quoteIndex = source.IndexOf("<InstantQuotePanel />", StringComparison.Ordinal);

        Assert.True(headingIndex >= 0, "Hero heading was not found.");
        Assert.True(quoteIndex > headingIndex, "InstantQuotePanel must render after the hero heading.");
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
