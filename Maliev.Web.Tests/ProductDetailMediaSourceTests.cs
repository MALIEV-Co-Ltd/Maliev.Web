namespace Maliev.Web.Tests;

/// <summary>
/// Source-level regression tests for the product detail left thumbnail carousel
/// and cross-fade image transition.
/// </summary>
public sealed class ProductDetailMediaSourceTests
{
    private static string Razor => ReadRepoFile("Maliev.Web.Client", "Pages", "ProductDetail.razor");
    private static string Css   => ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

    // ── CSS: layout ─────────────────────────────────────────────────────────

    /// <summary>Verifies the media grid reserves a 72 px left column for thumbnails.</summary>
    [Fact]
    public void Css_MediaGrid_HasLeftThumbColumn()
        => Assert.Contains("grid-template-columns: 72px", Css, StringComparison.Ordinal);

    /// <summary>Verifies the thumbnail strip stacks vertically on desktop.</summary>
    [Fact]
    public void Css_MediaStrip_IsVerticalFlex()
        => Assert.Contains("flex-direction: column", Css, StringComparison.Ordinal);

    // ── CSS: thumbnail opacity ───────────────────────────────────────────────

    /// <summary>Verifies inactive thumbnail images are rendered at half opacity.</summary>
    [Fact]
    public void Css_MediaThumbImg_HasHalfOpacityAtRest()
        => Assert.Contains("opacity: 0.5", Css, StringComparison.Ordinal);

    /// <summary>Verifies hovering a thumbnail button raises its image to full opacity.</summary>
    [Fact]
    public void Css_MediaThumbHover_BringsToFullOpacity()
        => Assert.Contains(".media-thumb:hover img", Css, StringComparison.Ordinal);

    /// <summary>Verifies the active thumbnail receives a visible outline with offset.</summary>
    [Fact]
    public void Css_MediaThumbActiveImg_HasOutlineOffset()
        => Assert.Contains("outline-offset: 2px", Css, StringComparison.Ordinal);

    // ── CSS: cross-fade ──────────────────────────────────────────────────────

    /// <summary>Verifies the CSS file declares the cross-fade-in keyframe animation.</summary>
    [Fact]
    public void Css_HasCrossfadeInKeyframe()
        => Assert.Contains("img-crossfade-in", Css, StringComparison.Ordinal);

    /// <summary>Verifies the CSS file declares the cross-fade-out keyframe animation.</summary>
    [Fact]
    public void Css_HasCrossfadeOutKeyframe()
        => Assert.Contains("img-crossfade-out", Css, StringComparison.Ordinal);

    /// <summary>Verifies the incoming-image CSS class exists for the fade-in animation.</summary>
    [Fact]
    public void Css_HasProductDetailImgInClass()
        => Assert.Contains(".product-detail-img--in", Css, StringComparison.Ordinal);

    /// <summary>Verifies the outgoing-image CSS class exists for the fade-out animation.</summary>
    [Fact]
    public void Css_HasProductDetailImgOutClass()
        => Assert.Contains(".product-detail-img--out", Css, StringComparison.Ordinal);

    // ── CSS: mobile override ─────────────────────────────────────────────────

    /// <summary>Verifies the mobile breakpoint collapses the thumbnail strip to a horizontal row.</summary>
    [Fact]
    public void Css_MobileOverride_MediaStripIsHorizontal()
        => Assert.Contains("flex-direction: row", Css, StringComparison.Ordinal);

    // ── Razor: state ─────────────────────────────────────────────────────────

    /// <summary>Verifies the old _selectedImage field has been removed from the component.</summary>
    [Fact]
    public void Razor_DoesNotContainSelectedImageField()
        => Assert.DoesNotContain("_selectedImage", Razor, StringComparison.Ordinal);

    /// <summary>Verifies the component tracks the currently displayed image separately from fading state.</summary>
    [Fact]
    public void Razor_HasDisplayedImageField()
        => Assert.Contains("_displayedImage", Razor, StringComparison.Ordinal);

    /// <summary>Verifies the component holds an outgoing image reference during the cross-fade transition.</summary>
    [Fact]
    public void Razor_HasFadingOutImageField()
        => Assert.Contains("_fadingOutImage", Razor, StringComparison.Ordinal);

    /// <summary>Verifies the component guards against re-entrant rapid clicks with a transitioning flag.</summary>
    [Fact]
    public void Razor_HasIsTransitioningField()
        => Assert.Contains("_isTransitioning", Razor, StringComparison.Ordinal);

    /// <summary>Verifies the async method that drives the cross-fade transition is present.</summary>
    [Fact]
    public void Razor_HasSelectImageMethod()
        => Assert.Contains("SelectImage", Razor, StringComparison.Ordinal);

    // ── Razor: template ──────────────────────────────────────────────────────

    /// <summary>Verifies the main image area container is present in the template.</summary>
    [Fact]
    public void Razor_HasMediaMainContainer()
        => Assert.Contains("product-detail-media-main", Razor, StringComparison.Ordinal);

    /// <summary>Verifies the template applies the fade-in class to the incoming image element.</summary>
    [Fact]
    public void Razor_HasImgInClass()
        => Assert.Contains("product-detail-img--in", Razor, StringComparison.Ordinal);

    /// <summary>Verifies the template applies the fade-out class to the outgoing image element.</summary>
    [Fact]
    public void Razor_HasImgOutClass()
        => Assert.Contains("product-detail-img--out", Razor, StringComparison.Ordinal);

    /// <summary>Verifies Blazor's @key directive is bound to the displayed image URL to force DOM re-creation on switch.</summary>
    [Fact]
    public void Razor_HasKeyOnDisplayedImageUrl()
        => Assert.Contains("@key=\"@_displayedImage.Url\"", Razor, StringComparison.Ordinal);

    // ── helpers ──────────────────────────────────────────────────────────────

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
                    return directory.FullName;
                var siblingCandidate = Path.Combine(directory.FullName, "Maliev.Web");
                if (File.Exists(Path.Combine(siblingCandidate, "Maliev.Web.slnx")))
                    return siblingCandidate;
                directory = directory.Parent;
            }
        }
        throw new DirectoryNotFoundException("Could not find Maliev.Web repository root.");
    }
}
