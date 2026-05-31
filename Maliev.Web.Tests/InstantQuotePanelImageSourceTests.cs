using System.Text.RegularExpressions;

namespace Maliev.Web.Tests;

/// <summary>
/// Source-level tests for customer-facing quote configurator choices.
/// </summary>
public sealed class InstantQuotePanelImageSourceTests
{
    private static string Razor => ReadRepoFile("Maliev.Web.Client", "Components", "Quote", "InstantQuotePanel.razor");
    private static string Css => ReadRepoFile("Maliev.Web.Client", "Components", "Quote", "InstantQuotePanel.razor.css");

    /// <summary>
    /// Verifies material, finish, color, and deburr choices render without internal configurator image assets.
    /// </summary>
    [Fact]
    public void Razor_UsesIconCardsForCustomerConfiguratorOptions()
    {
        Assert.DoesNotContain("<select @bind=\"part.MaterialCode\"", Razor, StringComparison.Ordinal);
        Assert.Contains("quote-option-card", Razor, StringComparison.Ordinal);
        Assert.Contains("quote-option-icon", Razor, StringComparison.Ordinal);
        Assert.DoesNotContain("quote-option-image", Razor, StringComparison.Ordinal);
        Assert.DoesNotContain("GetMaterialImageUrl", Razor, StringComparison.Ordinal);
        Assert.DoesNotContain("GetSurfaceFinishImageUrl", Razor, StringComparison.Ordinal);
        Assert.DoesNotContain("GetConfigOptionImageUrl", Razor, StringComparison.Ordinal);
        Assert.DoesNotContain("ImageErrorFallback", Razor, StringComparison.Ordinal);
        Assert.DoesNotContain("/images/materials/", Razor, StringComparison.Ordinal);
        Assert.Contains("deburr_edges", Razor, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies the visual configurator cards have stable icon sizing.
    /// </summary>
    [Fact]
    public void Css_DefinesStableIconCardLayout()
    {
        Assert.Contains(".quote-option-grid", Css, StringComparison.Ordinal);
        Assert.Contains(".quote-option-icon", Css, StringComparison.Ordinal);
        Assert.Contains("aspect-ratio: 1 / 1", Css, StringComparison.Ordinal);
        Assert.DoesNotContain(".quote-option-image", Css, StringComparison.Ordinal);
        Assert.DoesNotContain(".quote-option-fallback", Css, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies internal configurator material assets are not stored in the customer-facing Web project.
    /// </summary>
    [Fact]
    public void MaterialPreviewAssets_AreNotStoredInWebProject()
    {
        var root = FindRepositoryRoot();
        var materialsDirectory = Path.Combine(root, "Maliev.Web.Bff", "wwwroot", "images", "materials");

        Assert.Empty(Regex.Matches(Razor, "\"(?<file>[^\"]+-material-image\\.png)\""));
        Assert.False(Directory.Exists(materialsDirectory), "Configurator material assets belong in Maliev.Intranet, not Maliev.Web.");
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
