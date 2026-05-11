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
        Assert.Contains("Href=\"@SiteContent.QuoteNewUrl\"", source);
        Assert.Contains("Icons.Material.Filled.CloudUpload", source);
        Assert.Contains("landing-quote-dropzone-action", source);
        Assert.DoesNotContain("landing-shop-button", source);
        Assert.DoesNotContain("<a class=\"button primary\" href=\"/quote\">@L[\"StartQuote\"]</a>", source);
        Assert.DoesNotContain("quote-empty", source);
    }

    /// <summary>
    /// Verifies the lower landing sections use finished page sections and no proof-band mock block.
    /// </summary>
    [Fact]
    public void HomeUsesFinishedManufacturingAndNewsSections()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Pages", "Home.razor");
        var content = ReadRepoFile("Maliev.Web.Client", "Content", "SiteContent.cs");

        Assert.Contains("service-grid", source);
        Assert.Contains("feature-band", source);
        Assert.Contains("process-grid", source);
        Assert.Contains("quote-flow-band", source);
        Assert.Contains("blog-grid", source);
        Assert.Contains("SiteContent.BlogPosts", source);
        Assert.Contains("case-card-media", source);
        Assert.Contains("logo-heading", source);
        Assert.Contains("/images/logo.svg", source);
        Assert.Contains("/images/home/service-3d-printing.png", content);
        Assert.Contains("/images/home/case-fixture.png", content);
        Assert.Contains("/images/home/blog-dfm.png", content);
        Assert.DoesNotContain("shop-section", source);
        Assert.DoesNotContain("trust-band", source);
        Assert.DoesNotContain("trust-grid", source);
        Assert.Contains("section-link", source);
        Assert.DoesNotContain("proof-band", source);
        Assert.DoesNotContain("Build. Test. Produce.", source);
        Assert.DoesNotContain("Customer-facing quoting and commerce in one path", source);
    }

    /// <summary>
    /// Verifies the footer uses real company identity, contact, address, and social links.
    /// </summary>
    [Fact]
    public void FooterUsesLogoAndManufacturingContactLinks()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Layout", "MainLayout.razor");

        Assert.Contains("footer-logo", source);
        Assert.Contains("/images/logo.svg", source);
        Assert.Contains("Rapid manufacturing, custom parts, workshop-made machines", source);
        Assert.Contains("36/1 Moo 3", source);
        Assert.Contains("Khlong Khoi", source);
        Assert.Contains("info@maliev.com", source);
        Assert.Contains("page.line.me/maliev", source);
        Assert.Contains("facebook.com/maliev.manufacturing", source);
        Assert.Contains("youtube.com/%40maliev.manufacturing", source);
        Assert.Contains("instagram.com/maliev.manufacturing", source);
        Assert.DoesNotContain("<strong>MALIEV Co., Ltd.</strong>", source);
        Assert.DoesNotContain("Nonthaburi, Thailand. Manufacturing services, machines, and production support.", source);
    }

    /// <summary>
    /// Verifies the quote page sends custom manufacturing work to the dedicated quote engine.
    /// </summary>
    [Fact]
    public void QuotePageRoutesToDedicatedQuoteEngine()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Pages", "Quote.razor");

        Assert.Contains("<ManufacturingGizmo />", source);
        Assert.Contains("SiteContent.QuoteNewUrl", source);
        Assert.Contains("http-equiv=\"refresh\"", source);
        Assert.Contains("content=\"5; url=@SiteContent.QuoteNewUrl\"", source);
        Assert.DoesNotContain("<InstantQuotePanel />", source);
        Assert.DoesNotContain("quote-engine-mock", source);
        Assert.DoesNotContain("quote-engine-dropzone", source);
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
        Assert.Contains("SiteContent.QuoteProfileUrl", source);
        Assert.Contains("SiteContent.QuoteNewUrl", source);
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
        var app = ReadRepoFile("Maliev.Web.Bff", "Components", "App.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");
        var cultureScript = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "maliev-culture.js");

        Assert.Contains("<MudThemeProvider Theme=\"@_malievTheme\" />", source);
        Assert.Contains("new MudTheme", source);
        Assert.Contains("PaletteLight", source);
        Assert.Contains("DefaultBorderRadius", source);
        Assert.Contains("Typography", source);
        Assert.Contains("FontFamily = [\"var(--maliev-font-sans)\"]", source);
        Assert.Contains("<html lang=\"@documentLanguage\" data-culture=\"@currentCulture\">", app);
        Assert.Contains("Noto+Sans+Thai", app);
        Assert.Contains("--font-sans-th: \"Noto Sans Thai\"", styles);
        Assert.Contains("--font-mono: var(--font-sans-th)", styles);
        Assert.Contains("html:lang(th)", styles);
        Assert.Contains("document.documentElement.lang", cultureScript);
    }

    /// <summary>
    /// Verifies the public web app uses server-side Blazor interactivity.
    /// </summary>
    [Fact]
    public void BffUsesInteractiveServerRenderMode()
    {
        var program = ReadRepoFile("Maliev.Web.Bff", "Program.cs");
        var app = ReadRepoFile("Maliev.Web.Bff", "Components", "App.razor");
        var clientProject = ReadRepoFile("Maliev.Web.Client", "Maliev.Web.Client.csproj");

        Assert.Contains("AddInteractiveServerComponents", program);
        Assert.Contains("AddInteractiveServerRenderMode", program);
        Assert.Contains("@rendermode=\"InteractiveServer\"", app);
        Assert.DoesNotContain("AddInteractiveWebAssemblyComponents", program);
        Assert.DoesNotContain("AddInteractiveWebAssemblyRenderMode", program);
        Assert.DoesNotContain("Microsoft.NET.Sdk.BlazorWebAssembly", clientProject);
        Assert.DoesNotContain("Microsoft.AspNetCore.Components.WebAssembly", clientProject);

        var centralPackages = ReadRepoFile("Directory.Build.props");
        Assert.DoesNotContain("Microsoft.AspNetCore.Components.WebAssembly", centralPackages);
    }

    /// <summary>
    /// Verifies all designed service slugs are implemented as real Blazor routes.
    /// </summary>
    [Fact]
    public void ServicesUseRealRoutesAndQuoteEngineLinks()
    {
        var services = ReadRepoFile("Maliev.Web.Client", "Pages", "Services.razor");
        var servicePage = ReadRepoFile("Maliev.Web.Client", "Pages", "ServicePage.razor");
        var content = ReadRepoFile("Maliev.Web.Client", "Content", "SiteContent.cs");

        Assert.Contains("@page \"/services\"", services);
        Assert.Contains("@page \"/services/{Slug}\"", servicePage);
        Assert.Contains("SiteContent.QuoteNewUrl", services);
        Assert.Contains("SiteContent.QuoteNewUrl", servicePage);
        Assert.Contains("\"silicone-casting\"", content);
        Assert.Contains("\"rapid-prototyping\"", content);
        Assert.Contains("\"deviation-analysis\"", content);
        Assert.DoesNotContain("window.location.hash", services);
        Assert.DoesNotContain("window.location.hash", servicePage);
    }

    /// <summary>
    /// Verifies static customer pages are real routes and keep contact wired through the BFF.
    /// </summary>
    [Fact]
    public void StaticPagesCoverCustomerRoutesAndContactBoundary()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Pages", "StaticPage.razor");

        Assert.Contains("@page \"/materials\"", source);
        Assert.Contains("@page \"/case-studies/{Slug}\"", source);
        Assert.Contains("@page \"/blog/{Slug}\"", source);
        Assert.Contains("SiteContent.BlogPosts", source);
        Assert.Contains("@page \"/shipping-returns\"", source);
        Assert.Contains("SubmitContactMessageAsync", source);
        Assert.Contains("SiteContent.QuoteNewUrl", source);
        Assert.Contains("SiteContent.QuoteProfileUrl", source);
        Assert.Contains("SiteContent.QuoteOrdersUrl", source);
        Assert.DoesNotContain("href=\"/quote\"", source);
    }

    /// <summary>
    /// Verifies product and fallback quote CTAs leave the local bridge only for the SEO quote page.
    /// </summary>
    [Fact]
    public void CommerceQuoteCtasUseDedicatedQuoteEngine()
    {
        var content = ReadRepoFile("Maliev.Web.Client", "Content", "SiteContent.cs");
        var shop = ReadRepoFile("Maliev.Web.Client", "Pages", "Shop.razor");
        var product = ReadRepoFile("Maliev.Web.Client", "Pages", "ProductDetail.razor");
        var error = ReadRepoFile("Maliev.Web.Client", "Pages", "Error.razor");

        Assert.Contains("https://quote.maliev.com/projects/new", content);
        Assert.DoesNotContain("https://quote.maliev.com/quotes/new", content);
        Assert.Contains("SiteContent.QuoteNewUrl", shop);
        Assert.Contains("SiteContent.QuoteNewUrl", product);
        Assert.Contains("SiteContent.QuoteNewUrl", error);
        Assert.DoesNotContain("href=\"/quote\"", shop);
        Assert.DoesNotContain("href=\"/quote\"", product);
        Assert.DoesNotContain("href=\"/quote\"", error);
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
        Assert.Contains("antialias: true", source);
        Assert.Contains("renderRatio", source);
        Assert.Contains("setHardwareScalingLevel", source);
        Assert.Contains("engine.setHardwareScalingLevel(1 / renderRatio)", source);
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
        Assert.Contains("const targetSize = 2.28", source);
        Assert.Contains("wide ? 7.05 : 6.65", source);
        Assert.Contains("allowNativeContextMenu", source);
        Assert.Contains("restoreNativeCanvasBehavior", source);
        Assert.Contains("applyInjectionMoldedPlasticMaterial", source);
        Assert.Contains("pointerEnterHandler", source);
        Assert.Contains("targetStrength", source);
        Assert.Contains("pointerFollow", source);
        Assert.Contains("hoverFade", source);
        Assert.DoesNotContain("* 0.24", source);
        Assert.DoesNotContain("* 0.11", source);
    }

    /// <summary>
    /// Verifies the 3D canvas does not show a browser focus outline when clicked.
    /// </summary>
    [Fact]
    public void ManufacturingGizmoCanvasDoesNotExposeFocusRing()
    {
        var component = ReadRepoFile("Maliev.Web.Client", "Components", "Quote", "ManufacturingGizmo.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.DoesNotContain("tabindex", component);
        Assert.Contains(".manufacturing-gizmo-canvas:focus", styles);
        Assert.Contains(".manufacturing-gizmo-canvas:focus-visible", styles);
        Assert.Contains("box-shadow: none", styles);
        Assert.Contains("[tabindex=\"-1\"]:focus", styles);
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
