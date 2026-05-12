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
        var dropzone = ReadRepoFile("Maliev.Web.Client", "Components", "Quote", "QuoteDropzone.razor");

        Assert.Contains("<QuoteDropzone", source);
        Assert.Contains("Href=\"@SiteContent.QuoteNewUrl\"", source);
        Assert.Contains("Class=\"final-dropzone\"", source);
        Assert.Contains("landing-quote-dropzone", dropzone);
        Assert.Contains("Icons.Material.Filled.CloudUpload", dropzone);
        Assert.Contains("Icons.Material.Filled.ArrowForward", dropzone);
        Assert.Contains("landing-quote-dropzone-icon", dropzone);
        Assert.Contains("landing-quote-dropzone-action", dropzone);
        Assert.DoesNotContain("StartIcon=", dropzone);
        Assert.DoesNotContain("EndIcon=", dropzone);
        Assert.DoesNotContain("class=\"quote-dropzone final-dropzone\"", source);
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
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("service-grid", source);
        Assert.Contains("feature-band", source);
        Assert.Contains("process-grid", source);
        Assert.DoesNotContain("quote-flow-band", source);
        Assert.Contains("blog-grid", source);
        Assert.Contains("SiteContent.BlogPosts", source);
        Assert.Contains("PersistentComponentState", source);
        Assert.Contains("RandomizeSupportingServices", source);
        Assert.Contains("Shuffle(SiteContent.CaseStudies)", source);
        Assert.Contains("Shuffle(SiteContent.BlogPosts)", source);
        Assert.Contains("RandomNumberGenerator.GetInt32", source);
        Assert.Contains("case-card-media", source);
        Assert.Contains("logo-heading", source);
        Assert.Contains("/images/logo.svg", source);
        Assert.Contains("https://images.unsplash.com/", content);
        Assert.Contains("ThreeDimensionalPrinterImageUrl", content);
        Assert.Contains("ThreeDimensionalScannerImageUrl", content);
        Assert.Contains("InjectionMoldingLineImageUrl", content);
        Assert.Contains("founded in Thailand", content);
        Assert.Contains("12,000+", content);
        Assert.Contains("parts produced", content);
        Assert.Contains("businesses served", content);
        Assert.DoesNotContain("12k+", content);
        Assert.DoesNotContain("feedback before order", content);
        Assert.DoesNotContain("continue when ready", content);
        Assert.DoesNotContain("/images/home/", content);
        Assert.DoesNotContain("shop-section", source);
        Assert.DoesNotContain("trust-band", source);
        Assert.DoesNotContain("trust-grid", source);
        Assert.Contains("section-link", source);
        Assert.DoesNotContain("proof-band", source);
        Assert.DoesNotContain("Build. Test. Produce.", source);
        Assert.DoesNotContain("Customer-facing quoting and commerce in one path", source);
        Assert.DoesNotContain("<span class=\"button primary\">@Text(\"Get part price\", \"ดูราคาชิ้นงาน\")</span>", source);
        Assert.DoesNotContain("final-dropzone-arrow", source);
        Assert.DoesNotContain(".final-dropzone-arrow", styles);
        Assert.DoesNotContain(".final-dropzone-icon", styles);
        Assert.DoesNotContain(".final-dropzone-copy", styles);
        Assert.Contains("grid-template-rows: auto auto minmax(0, 1fr) auto", styles);
        Assert.Contains("align-content: start", styles);
        Assert.Contains("filter: var(--logo-filter)", styles);
        Assert.Contains("home-services-section", source);
        Assert.Contains(".home-services-section", styles);
        Assert.Contains("padding-top: clamp(44px, 5vw, 72px);", styles);
        Assert.Contains(".home-services-section .section-heading", styles);
        Assert.Contains("margin-bottom: 30px;", styles);
        Assert.Contains("grid-template-columns: repeat(2, minmax(0, 1fr));", styles);
        Assert.Contains(".social-link", styles);
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
        Assert.Contains("line-contact-link", source);
        Assert.Contains("line-contact-icon", source);
        Assert.Contains("Official Account @@maliev", source);
        Assert.Contains("facebook.com/maliev.manufacturing", source);
        Assert.Contains("youtube.com/channel/UCCosquPSUed6UPlMcRCq0Ig", source);
        Assert.Contains("instagram.com/maliev.manufacturing", source);
        Assert.Contains("class=\"social-link\"", source);
        Assert.Contains("<svg viewBox=\"0 0 24 24\"", source);
        Assert.Contains("aria-label=\"Facebook\"", source);
        Assert.Contains("aria-label=\"YouTube\"", source);
        Assert.Contains("aria-label=\"Instagram\"", source);
        Assert.DoesNotContain(">Facebook</a>", source);
        Assert.DoesNotContain(">YouTube</a>", source);
        Assert.DoesNotContain(">Instagram</a>", source);
        Assert.DoesNotContain(">LINE @@maliev</a>", source);
        Assert.Contains("FoundingYear = 2018", source);
        Assert.Contains("DateTime.Today.Year", source);
        Assert.Contains("All rights reserved", source);
        Assert.Contains("footer-legal", source);
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
        Assert.Contains("Href=\"/account\"", source);
        Assert.Contains("SiteContent.QuoteNewUrl", source);
        Assert.DoesNotContain("class=\"icon-link\"", source);
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

        Assert.Contains("<MudThemeProvider Theme=\"@_malievTheme\" IsDarkMode=\"@IsDarkMode\" />", source);
        Assert.Contains("new MudTheme", source);
        Assert.Contains("PaletteLight", source);
        Assert.Contains("PaletteDark", source);
        Assert.Contains("DefaultBorderRadius", source);
        Assert.Contains("Typography", source);
        Assert.Contains("FontFamily = [\"var(--maliev-font-sans)\"]", source);
        Assert.Contains("<html lang=\"@documentLanguage\" data-culture=\"@currentCulture\" data-theme=\"light\">", app);
        Assert.Contains("<meta name=\"color-scheme\" content=\"light dark\" />", app);
        Assert.Contains("maliev.theme", app);
        Assert.Contains("Noto+Sans+Thai", app);
        Assert.Contains("--font-sans-en: Geist, \"Noto Sans Thai\"", styles);
        Assert.Contains("--font-sans-th: \"Noto Sans Thai\"", styles);
        Assert.Contains("--font-mono: var(--font-sans-th)", styles);
        Assert.Contains("html:lang(th)", styles);
        Assert.Contains("html[data-theme=\"dark\"]", styles);
        Assert.Contains("--logo-filter: brightness(0) invert(1)", styles);
        Assert.Contains("document.documentElement.lang", cultureScript);
        Assert.Contains("document.documentElement.dataset.theme", cultureScript);
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
        Assert.Contains("UseStaticWebAssets", program);
        Assert.Contains("app.UseStaticFiles();", program);
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
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("@page \"/materials\"", source);
        Assert.Contains("@page \"/case-studies/{Slug}\"", source);
        Assert.Contains("@page \"/blog/{Slug}\"", source);
        Assert.Contains("SiteContent.BlogPosts", source);
        Assert.Contains("@page \"/shipping-returns\"", source);
        Assert.Contains("SubmitContactMessageAsync", source);
        Assert.Contains("SiteContent.QuoteNewUrl", source);
        Assert.DoesNotContain("@page \"/account/orders\"", source);
        Assert.DoesNotContain("@page \"/account/preferences\"", source);
        Assert.Contains("material-category-media", source);
        Assert.Contains("material-category-body", source);
        Assert.Contains("material-comparison-table", source);
        Assert.Contains("material-mobile-list", source);
        Assert.Contains("material-compare-workbench", source);
        Assert.Contains("FilteredMaterialComparisons", source);
        Assert.Contains("SelectedMaterialComparisons", source);
        Assert.Contains("ToggleMaterialComparison", source);
        Assert.Contains("_selectedMaterialNames.Count < 3", source);
        Assert.Contains("MaterialProcessFilters", source);
        Assert.Contains("MaterialUseFilters", source);
        Assert.Contains("ImageUrl", source);
        Assert.Contains("ImageAlt", source);
        Assert.Contains(".material-category-media", styles);
        Assert.Contains(".material-category-body", styles);
        Assert.Contains(".material-comparison-table", styles);
        Assert.Contains("@media (max-width: 1180px)", styles);
        Assert.Contains("display: none;", styles);
        Assert.Contains("display: grid;", styles);
        Assert.Contains(".material-select-button", styles);
        Assert.Contains(".material-compare-matrix", styles);
        Assert.Contains(".material-filter", styles);
        Assert.Contains("grid-template-columns: repeat(4, minmax(0, 1fr));", styles);
        Assert.Contains("grid-template-columns: 122px minmax(0, 1fr);", styles);
        Assert.DoesNotContain("href=\"/quote\"", source);
    }

    /// <summary>
    /// Verifies the public account area uses local authenticated routes backed by the Web BFF.
    /// </summary>
    [Fact]
    public void AccountAreaUsesLocalCustomerSessionRoutes()
    {
        var program = ReadRepoFile("Maliev.Web.Bff", "Program.cs");
        var authController = ReadRepoFile("Maliev.Web.Bff", "Controllers", "AuthController.cs");
        var accountController = ReadRepoFile("Maliev.Web.Bff", "Controllers", "AccountController.cs");
        var layout = ReadRepoFile("Maliev.Web.Client", "Layout", "MainLayout.razor");
        var routes = ReadRepoFile("Maliev.Web.Client", "Routes.razor");
        var account = ReadRepoFile("Maliev.Web.Client", "Pages", "Account.razor");
        var profile = ReadRepoFile("Maliev.Web.Client", "Pages", "AccountProfile.razor");
        var addresses = ReadRepoFile("Maliev.Web.Client", "Pages", "AccountAddresses.razor");
        var preferences = ReadRepoFile("Maliev.Web.Client", "Pages", "AccountPreferences.razor");
        var orders = ReadRepoFile("Maliev.Web.Client", "Pages", "AccountOrders.razor");

        Assert.Contains("AddAuthentication", program);
        Assert.Contains("AddCookie", program);
        Assert.Contains("AddGoogle", program);
        Assert.Contains("AddCascadingAuthenticationState", program);
        Assert.Contains("UseAuthentication", program);
        Assert.Contains("UseAuthorization", program);
        Assert.Contains("IAuthServiceClient, AuthServiceClient", program);
        Assert.Contains("ICustomerServiceClient, CustomerServiceClient", program);
        Assert.Contains("ICountryServiceClient, CountryServiceClient", program);
        Assert.Contains("customer_id", authController);
        Assert.Contains("principal_id", authController);
        Assert.Contains("[Route(\"web/v{version:apiVersion}/account\")]", accountController);
        Assert.Contains("GetProfile", accountController);
        Assert.Contains("GetAddresses", accountController);
        Assert.Contains("GetOrders", accountController);
        Assert.Contains("QuoteOrdersUrl", accountController);
        Assert.Contains("Href=\"/account\"", layout);
        Assert.Contains("<CascadingAuthenticationState>", routes);
        Assert.Contains("<AuthorizeRouteView", routes);
        Assert.Contains("@page \"/account\"", account);
        Assert.Contains("@page \"/account/profile\"", profile);
        Assert.Contains("@page \"/account/addresses\"", addresses);
        Assert.Contains("@page \"/account/preferences\"", preferences);
        Assert.Contains("@page \"/account/orders\"", orders);
        Assert.Contains("@page \"/account/orders/{OrderId}\"", orders);
        Assert.Contains("@attribute [Authorize]", account);
        Assert.Contains("@attribute [Authorize]", profile);
        Assert.Contains("@attribute [Authorize]", addresses);
        Assert.Contains("@attribute [Authorize]", preferences);
        Assert.Contains("@attribute [Authorize]", orders);
        Assert.DoesNotContain("SiteContent.QuoteProfileUrl", layout);
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
        Assert.Contains("alpha: true", source);
        Assert.Contains("antialias: true", source);
        Assert.Contains("premultipliedAlpha: false", source);
        Assert.Contains("renderRatio", source);
        Assert.Contains("setHardwareScalingLevel", source);
        Assert.Contains("engine.setHardwareScalingLevel(1 / renderRatio)", source);
        Assert.Contains("ResizeObserver", source);
        Assert.Contains("powerPreference: \"low-power\"", source);

        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");
        Assert.Contains(".manufacturing-gizmo--landing", styles);
        Assert.Contains("overflow: visible", styles);
        Assert.Contains(".manufacturing-gizmo--landing::before", styles);
        Assert.Contains("background: transparent", styles);
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
        Assert.Contains("wide ? 7.35 : 7.05", source);
        Assert.DoesNotContain("createLandingSurface", source);
        Assert.DoesNotContain("landing-contact-shadow", source);
        Assert.Contains("allowNativeContextMenu", source);
        Assert.Contains("restoreNativeCanvasBehavior", source);
        Assert.Contains("applyInjectionMoldedPlasticMaterial", source);
        Assert.Contains("applyLandingHeroTheme", source);
        Assert.Contains("observeDocumentTheme", source);
        Assert.Contains("MutationObserver", source);
        Assert.Contains("pointerEnterHandler", source);
        Assert.Contains("targetStrength", source);
        Assert.Contains("pointerFollow", source);
        Assert.Contains("hoverFade", source);
        Assert.DoesNotContain("* 0.24", source);
        Assert.DoesNotContain("* 0.11", source);
    }

    /// <summary>
    /// Verifies the landing hero has dedicated tablet composition rules instead of using the narrow mobile stack.
    /// </summary>
    [Fact]
    public void HomeHeroUsesBalancedTabletComposition()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Pages", "Home.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");
        var gizmo = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "manufacturing-gizmo.js");

        Assert.DoesNotContain("landing-hero-badge", source);
        Assert.DoesNotContain("Rapid manufacturing / live part pricing", source);
        Assert.Contains(".landing-hero-badge", styles);
        Assert.Contains("display: none;", styles);
        Assert.Contains("@media (min-width: 961px) and (max-width: 1180px)", styles);
        Assert.Contains("grid-template-columns: minmax(0, .96fr) minmax(340px, .92fr);", styles);
        Assert.Contains("@media (min-width: 681px) and (max-width: 960px)", styles);
        Assert.Contains("text-align: center;", styles);
        Assert.Contains("gap: 18px;", styles);
        Assert.Contains("margin-top: 22px;", styles);
        Assert.Contains("width: min(100%, 820px);", styles);
        Assert.Contains("min-height: 440px;", styles);
        Assert.Contains("height: min(46vh, 470px);", styles);
        Assert.Contains("justify-content: center;", styles);
        Assert.Contains("const balancedTablet = width >= 640 && width <= 920 && height >= 460;", gizmo);
        Assert.Contains("balancedTablet ? 0.43 : 0.44", gizmo);
        Assert.Contains("balancedTablet ? 6.6 : wide ? 7.35 : 7.05", gizmo);
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
