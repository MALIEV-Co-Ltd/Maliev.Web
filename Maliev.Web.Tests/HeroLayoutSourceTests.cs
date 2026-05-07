namespace Maliev.Web.Tests;

/// <summary>
/// Source-level layout tests for public hero composition.
/// </summary>
public sealed class HeroLayoutSourceTests
{
    /// <summary>
    /// Verifies the home hero uses the right-side GLB landing model and leaves quoting to the quote page.
    /// </summary>
    [Fact]
    public void HomeHeroUsesRightSideGlbLandingModel()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Pages", "Home.razor");

        Assert.Contains("landing-hero", source);
        Assert.Contains("landing-hero-visual", source);
        Assert.Contains("ModelUrl=\"/models/hero-3d.glb\"", source);
        Assert.Contains("EnableHoverMotion=\"true\"", source);
        Assert.Contains("UsePlasticMaterial=\"true\"", source);
        Assert.DoesNotContain("<InstantQuotePanel />", source);
        Assert.DoesNotContain("hero-workspace gizmo-workspace", source);
    }

    /// <summary>
    /// Verifies the home hero routes quote entry through a MudBlazor dropzone CTA.
    /// </summary>
    [Fact]
    public void HomeHeroMergesStartQuoteIntoDropzone()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Pages", "Home.razor");

        Assert.Contains("landing-quote-dropzone", source);
        Assert.Contains("Href=\"/quote\"", source);
        Assert.Contains("Icons.Material.Filled.CloudUpload", source);
        Assert.DoesNotContain("<a class=\"button primary\" href=\"/quote\">@L[\"StartQuote\"]</a>", source);
        Assert.DoesNotContain("quote-empty", source);
    }

    /// <summary>
    /// Verifies the quote page uses a mocked MudBlazor quote engine while the real engine is deferred.
    /// </summary>
    [Fact]
    public void QuoteHeroUsesMockQuoteEngineAndGizmo()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Pages", "Quote.razor");

        Assert.Contains("<ManufacturingGizmo />", source);
        Assert.Contains("quote-engine-mock", source);
        Assert.Contains("quote-engine-dropzone", source);
        Assert.Contains("MudButton", source);
        Assert.Contains("Icons.Material.Filled.CloudUpload", source);
        Assert.DoesNotContain("<InstantQuotePanel />", source);
        Assert.DoesNotContain("ModelUrl=", source);
        Assert.DoesNotContain("EnableHoverMotion=\"true\"", source);
        Assert.DoesNotContain("contract-panel", source);
    }

    /// <summary>
    /// Verifies the public layout uses MudBlazor icon navigation for cart and account.
    /// </summary>
    [Fact]
    public void HeaderUsesMudIconButtonsForCartAndAccount()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Layout", "MainLayout.razor");

        Assert.Contains("MudIconButton", source);
        Assert.Contains("MudBadge", source);
        Assert.Contains("Icons.Material.Filled.ShoppingCart", source);
        Assert.Contains("Icons.Material.Filled.AccountCircle", source);
        Assert.DoesNotContain("class=\"icon-link\"", source);
        Assert.DoesNotContain("<NavLink href=\"/cart\"", source);
        Assert.DoesNotContain("<NavLink href=\"/account/preferences\"", source);
    }

    /// <summary>
    /// Verifies MudBlazor receives MALIEV design tokens instead of default styling.
    /// </summary>
    [Fact]
    public void LayoutAppliesMalievMudTheme()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Layout", "MainLayout.razor");

        Assert.Contains("<MudThemeProvider Theme=\"@_malievTheme\" />", source);
        Assert.Contains("new MudTheme", source);
        Assert.Contains("PaletteLight", source);
        Assert.Contains("DefaultBorderRadius", source);
        Assert.Contains("Typography", source);
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

    /// <summary>
    /// Verifies the landing 3D runtime has a dedicated model scene path instead of stretching the compact quote gizmo.
    /// </summary>
    [Fact]
    public void ManufacturingGizmoSupportsHoverDrivenPlasticLandingModel()
    {
        var source = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "manufacturing-gizmo.js");

        Assert.Contains("babylonjs-loaders@9.6.0", source);
        Assert.Contains("createLandingHeroScene", source);
        Assert.Contains("configureLandingHeroCamera", source);
        Assert.Contains("addHoverMotion", source);
        Assert.Contains("allowNativeContextMenu", source);
        Assert.Contains("restoreNativeCanvasBehavior", source);
        Assert.Contains("applyInjectionMoldedPlasticMaterial", source);
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
