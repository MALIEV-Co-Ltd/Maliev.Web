using System.Text.RegularExpressions;

namespace Maliev.Web.Tests;

/// <summary>
/// Source-level tests for image-backed quote configurator choices.
/// </summary>
public sealed class InstantQuotePanelImageSourceTests
{
    private static string Razor => ReadRepoFile("Maliev.Web.Client", "Components", "Quote", "InstantQuotePanel.razor");
    private static string Css => ReadRepoFile("Maliev.Web.Client", "Components", "Quote", "InstantQuotePanel.razor.css");

    /// <summary>
    /// Verifies material, finish, color, and deburr choices render as image-backed options with icon fallbacks.
    /// </summary>
    [Fact]
    public void Razor_UsesPreviewCardsForVisualConfiguratorOptions()
    {
        Assert.DoesNotContain("<select @bind=\"part.MaterialCode\"", Razor, StringComparison.Ordinal);
        Assert.Contains("quote-option-card", Razor, StringComparison.Ordinal);
        Assert.Contains("quote-option-image", Razor, StringComparison.Ordinal);
        Assert.Contains("quote-option-fallback", Razor, StringComparison.Ordinal);
        Assert.Contains("GetMaterialImageUrl", Razor, StringComparison.Ordinal);
        Assert.Contains("GetSurfaceFinishImageUrl", Razor, StringComparison.Ordinal);
        Assert.Contains("GetConfigOptionImageUrl", Razor, StringComparison.Ordinal);
        Assert.Contains("ImageErrorFallback", Razor, StringComparison.Ordinal);
        Assert.Contains("deburr_edges", Razor, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies the visual configurator cards have stable image sizing and fallback styles.
    /// </summary>
    [Fact]
    public void Css_DefinesStablePreviewCardLayout()
    {
        Assert.Contains(".quote-option-grid", Css, StringComparison.Ordinal);
        Assert.Contains(".quote-option-image", Css, StringComparison.Ordinal);
        Assert.Contains("aspect-ratio: 1 / 1", Css, StringComparison.Ordinal);
        Assert.Contains(".quote-option-fallback", Css, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies every filename used by the configurator helpers exists in the Web BFF static asset folder.
    /// </summary>
    [Fact]
    public void MaterialPreviewAssets_ExistForAllMappedConfiguratorImages()
    {
        var root = FindRepositoryRoot();
        var materialsDirectory = Path.Combine(root, "Maliev.Web.Bff", "wwwroot", "images", "materials");

        var mappedFiles = Regex.Matches(Razor, "\"(?<file>[^\"]+-material-image\\.png)\"")
            .Select(match => match.Groups["file"].Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(mappedFiles);
        foreach (var fileName in mappedFiles)
        {
            Assert.True(
                File.Exists(Path.Combine(materialsDirectory, fileName)),
                $"Missing material preview asset: {fileName}");
        }
    }

    private static string ReadRepoFile(params string[] pathSegments)
    {
        var root = FindRepositoryRoot();
        return File.ReadAllText(Path.Combine([root, .. pathSegments]));
    }

    private static string FindRepositoryRoot()
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

        throw new DirectoryNotFoundException("Could not find Maliev.Web repository root.");
    }
}
