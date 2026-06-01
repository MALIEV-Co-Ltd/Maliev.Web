using Maliev.Web.Client.Content;

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
        Assert.Contains("ModelUrl=\"@_heroModel.Url\"", source);
        Assert.Contains("@key=\"_heroModel.Key\"", source);
        Assert.Contains("HeroModelCatalog.SelectRandomForService(_heroServiceSlug)", source);
        Assert.Contains("home.hero.model", source);
        Assert.Contains("EnableHoverMotion=\"true\"", source);
        Assert.Contains("UsePlasticMaterial=\"@_heroModel.UsePlasticMaterial\"", source);
        Assert.DoesNotContain("ModelScale", source);
        Assert.DoesNotContain("<InstantQuotePanel />", source);
        Assert.DoesNotContain("hero-workspace gizmo-workspace", source);
        Assert.DoesNotContain("/models/hero-3d.glb", source);
    }

    /// <summary>
    /// Verifies hero GLB models use a route-aware catalog and scalable naming convention.
    /// </summary>
    [Fact]
    public void HeroModelsUseRouteAwareCatalogAndScalableNames()
    {
        var root = FindRepoRoot();
        var home = ReadRepoFile("Maliev.Web.Client", "Pages", "Home.razor");
        var servicePage = ReadRepoFile("Maliev.Web.Client", "Pages", "ServicePage.razor");
        var catalog = ReadRepoFile("Maliev.Web.Client", "Content", "HeroModelCatalog.cs");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");
        var modelReadme = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "models", "README.md");

        Assert.True(File.Exists(Path.Combine(root, "Maliev.Web.Bff", "wwwroot", "models", "hero-3d-printing-part-01.glb")));
        Assert.True(File.Exists(Path.Combine(root, "Maliev.Web.Bff", "wwwroot", "models", "hero-3d-printing-part-02.glb")));
        Assert.True(File.Exists(Path.Combine(root, "Maliev.Web.Bff", "wwwroot", "models", "hero-3d-printing-part-03.glb")));
        Assert.True(File.Exists(Path.Combine(root, "Maliev.Web.Bff", "wwwroot", "models", "hero-3d-scanning-part-01.glb")));
        Assert.Contains("hero-{service-slug}-{short-subject}-{nn}.glb", modelReadme);
        Assert.Contains("hero-3d-printing-part-03.glb", modelReadme);
        Assert.Contains("hero-cnc-machining-fixture-01.glb", modelReadme);
        Assert.Contains("hero-3d-scanning-part-01.glb", modelReadme);
        Assert.Contains("internal const string DefaultServiceSlug = \"3d-printing\";", catalog);
        Assert.Contains("\"/models/hero-3d-printing-part-01.glb\"", catalog);
        Assert.Contains("\"/models/hero-3d-printing-part-02.glb\"", catalog);
        Assert.Contains("\"/models/hero-3d-printing-part-03.glb\"", catalog);
        Assert.Contains("\"/models/hero-3d-scanning-part-01.glb\"", catalog);
        Assert.Contains("\"3d-scanning\"", catalog);
        Assert.Contains("SelectRandomForService", catalog);
        Assert.Contains("ResolveForService", catalog);
        Assert.Contains("RandomNumberGenerator.GetInt32", catalog);
        Assert.Contains("HeroModelCatalog.SelectRandomForService(_heroServiceSlug)", home);
        Assert.Contains("HeroModelCatalog.SelectRandomForService(Service.Slug)", servicePage);
        Assert.Contains("service.hero.model.", servicePage);
        Assert.Contains("service-page-hero", servicePage);
        Assert.Contains("service-page-hero-visual", servicePage);
        Assert.Contains("Class=\"manufacturing-gizmo--service\"", servicePage);
        Assert.Contains("@key=\"_heroModel.Key\"", servicePage);
        Assert.Contains(".manufacturing-gizmo--service", styles);
        Assert.DoesNotContain("ModelScale", home);
        Assert.DoesNotContain("ModelScale", servicePage);
        Assert.DoesNotContain("DisplayScale", catalog);
        Assert.DoesNotContain("hero-3d-2.glb", catalog);
        Assert.DoesNotContain("hero-3d.glb", catalog);
    }

    /// <summary>
    /// Verifies the undersized legacy fixture model is kept out of the default 3D-printing hero rotation.
    /// </summary>
    [Fact]
    public void HeroModelCatalogExcludesLegacyFixtureFromDefaultPrintingRotation()
    {
        var candidates = HeroModelCatalog.ResolveForService(HeroModelCatalog.DefaultServiceSlug);

        Assert.Equal("3d-printing-part-02", HeroModelCatalog.Default.Key);
        Assert.Contains(candidates, asset => asset.Key == "3d-printing-part-02");
        Assert.Contains(candidates, asset => asset.Key == "3d-printing-part-03");
        Assert.DoesNotContain(candidates, asset => asset.Key == "3d-printing-part-01");
    }

    /// <summary>
    /// Verifies the service detail page explains quotation readiness instead of rendering shallow spec cards.
    /// </summary>
    [Fact]
    public void ServicePageExplainsSpecificQuotationReadiness()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Pages", "ServicePage.razor");
        var content = ReadRepoFile("Maliev.Web.Client", "Content", "SiteContent.cs");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("SiteContent.GetServiceDetail(Service.Slug)", source);
        Assert.Contains("service-detail-section", source);
        Assert.Contains("ServiceDetailSectionClass", source);
        Assert.Contains("Manufacturing answer path", source);
        Assert.Contains("service-answer-frame", source);
        Assert.Contains("service-answer-block--routes", source);
        Assert.Contains("service-answer-card-grid--checks", source);
        Assert.Contains("service-answer-steps", source);
        Assert.Contains("service-supported-files", source);
        Assert.DoesNotContain("service-answer-card has-media", source);
        Assert.DoesNotContain("service-answer-card-media", source);
        Assert.DoesNotContain("route.ImageUrl", source);
        Assert.DoesNotContain("DetailItemImageAlt(route)", source);
        Assert.DoesNotContain("DetailIcon(", source);
        Assert.DoesNotContain("SpecHelp(", source);
        Assert.DoesNotContain("<div class=\"service-detail-grid\">", source);

        Assert.Contains("ThreeDimensionalPrintingServiceDetail", content);
        Assert.Contains("CncMachiningServiceDetail", content);
        Assert.Contains("ThreeDimensionalScanningServiceDetail", content);
        Assert.Contains("ThreeDimensionalDesignServiceDetail", content);
        Assert.Contains("SiliconeCastingServiceDetail", content);
        Assert.Contains("RapidPrototypingServiceDetail", content);
        Assert.Contains("DeviationAnalysisServiceDetail", content);
        Assert.Contains("3D printing service details", content);
        Assert.Contains("CNC machining service details", content);
        Assert.Contains("3D scanning service details", content);
        Assert.Contains("3D design service details", content);
        Assert.Contains("Silicone casting service details", content);
        Assert.Contains("Rapid prototyping service details", content);
        Assert.Contains("Deviation analysis service details", content);
        Assert.Contains("We narrow the process from the part's job", source);
        Assert.DoesNotContain("MALIEV narrows the process from the part's job", source);
        Assert.Contains("STL", content);
        Assert.Contains("STEP / STP", content);
        Assert.Contains("OBJ", content);
        Assert.Contains("3MF", content);
        Assert.Contains("IGES / IGS", content);
        Assert.Contains("PLA, PETG, ABS / ASA, TPU, nylon", content);
        Assert.Contains("Standard and engineering SLA resin", content);
        Assert.Contains("PA12 nylon and powder-bed routes", content);
        Assert.Contains("FdmThermoplasticsImageUrl", content);
        Assert.Contains("SlaResinImageUrl", content);
        Assert.Contains("PowderBedNylonImageUrl", content);
        Assert.Contains("EngineeringPolymerReviewImageUrl", content);
        Assert.Contains("Wall thickness and unsupported spans", content);
        Assert.Contains("Tolerances and fit-critical faces", content);
        Assert.Contains("Orientation and support marks", content);
        Assert.Contains("Threads, inserts, and hole strategy", content);
        Assert.Contains("Upload CAD and requirements", content);
        Assert.Contains("Produce, check, and deliver", content);
        Assert.Contains("Tool access and internal radius", content);
        Assert.Contains("Reverse engineering", content);
        Assert.Contains("DFM-driven product changes", content);
        Assert.Contains("Master preparation", content);
        Assert.Contains("One decision per round", content);
        Assert.Contains("Scan-to-CAD comparison", content);
        Assert.Contains("\"cnc-machining\" => CncMachiningServiceDetail", content);
        Assert.Contains("\"deviation-analysis\" => DeviationAnalysisServiceDetail", content);

        Assert.Contains(".service-answer-hero", styles);
        Assert.Contains(".service-answer-frame", styles);
        Assert.Contains(".service-answer-card-grid--routes", styles);
        Assert.Matches(@"\.service-answer-frame\s*\{[^}]*background:\s*transparent;", styles);
        Assert.Matches(@"\.service-answer-card\s*\{[^}]*background:\s*transparent;", styles);
        Assert.Matches(@"\.service-answer-card\s*\{[^}]*border:\s*0;", styles);
        Assert.Matches(@"\.service-answer-card-grid--routes\s*\{[^}]*repeat\(4, minmax\(0, 1fr\)\);", styles);
        Assert.Matches(@"\.service-answer-meta\s*\{[^}]*display:\s*inline-flex;", styles);
        Assert.Matches(@"\.service-answer-meta\s*\{[^}]*width:\s*fit-content;", styles);
        Assert.Matches(@"\.service-answer-meta\s*\{[^}]*max-width:\s*100%;", styles);
        Assert.Matches(@"\.service-answer-meta\s*\{[^}]*min-height:\s*34px;", styles);
        Assert.Matches(@"\.service-answer-meta\s*\{[^}]*overflow-wrap:\s*anywhere;", styles);
        Assert.DoesNotMatch(@"\.service-answer-meta\s*\{[^}]*width:\s*42px;", styles);
        Assert.DoesNotMatch(@"\.service-answer-meta\s*\{[^}]*height:\s*42px;", styles);
        Assert.Contains(".service-answer-card-grid--checks", styles);
        Assert.Contains(".service-answer-steps", styles);
        Assert.Contains(".service-detail-section--cnc-machining", styles);
        Assert.Contains(".service-detail-section--3d-scanning", styles);
        Assert.Contains(".service-file-chip", styles);
        Assert.Matches(@"\.service-file-chip\s*\{[^}]*background:\s*transparent;", styles);
        Assert.Contains("grid-template-columns: repeat(4, minmax(0, 1fr));", styles);
        Assert.Contains(".final-dropzone", styles);
        Assert.Contains(".final-cta {\n  grid-template-columns: minmax(0, 660px) minmax(320px, 540px);\n  align-items: center;\n  justify-content: center;\n  max-width: none;", styles);
        Assert.Matches(@"\.final-cta\s*\{[^}]*background:\s*var\(--paper-2\);", styles);
        Assert.DoesNotMatch(@"\.final-cta\s*\{[^}]*border-top:\s*1px solid var\(--rule\);", styles);
        Assert.Matches(@"\.final-dropzone\s*\{[^}]*align-self:\s*center;", styles);
        Assert.DoesNotMatch(@"\.final-dropzone\s*\{[^}]*align-self:\s*stretch;", styles);
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
        Assert.Contains("Href=\"@SiteContent.QuoteNewProjectUrl\"", source);
        Assert.Contains("Class=\"final-dropzone\"", source);
        Assert.Contains("landing-quote-dropzone", dropzone);
        Assert.Contains("Icons.Material.Filled.Upload", dropzone);
        Assert.DoesNotContain("Icons.Material.Filled.CloudUpload", dropzone);
        Assert.DoesNotContain("Icons.Material.Filled.ArrowForward", dropzone);
        Assert.Contains("landing-quote-dropzone-primary", dropzone);
        Assert.Contains("landing-quote-dropzone-divider", dropzone);
        Assert.Contains("landing-quote-dropzone-browse", dropzone);
        Assert.Contains("role=\"button\"", dropzone);
        Assert.Contains("tabindex=\"0\"", dropzone);
        Assert.Contains("landing-quote-dropzone-format-button", dropzone);
        Assert.Contains("landing-quote-dropzone-format-popover", dropzone);
        Assert.Contains("data-dropzone-interactive", dropzone);
        Assert.Contains("aria-expanded=\"@FormatsExpanded\"", dropzone);
        Assert.Contains("@onclick:stopPropagation=\"true\"", dropzone);
        Assert.Contains("@onpointerdown:stopPropagation=\"true\"", dropzone);
        Assert.DoesNotContain("landing-quote-dropzone-or\">@OrText", dropzone);
        Assert.Contains("landing-quote-dropzone-icon", dropzone);
        Assert.Contains("landing-quote-dropzone-action", dropzone);
        Assert.Contains("Drop 3D files for instant quote", source);
        Assert.Contains("Browse files", source);
        Assert.Contains("Configure material, finish and quantity after upload.", source);
        Assert.Contains("Ready when you are.", source);
        Assert.DoesNotContain("Configure material, finish and quantity at quote.maliev.com.", source);
        Assert.DoesNotContain("Paste link", dropzone);
        Assert.DoesNotContain("StartIcon=", dropzone);
        Assert.DoesNotContain("EndIcon=", dropzone);
        Assert.DoesNotContain("class=\"quote-dropzone final-dropzone\"", source);
        Assert.DoesNotContain("landing-shop-button", source);
        Assert.DoesNotContain("<a class=\"button primary\" href=\"/quote\">@L[\"StartQuote\"]</a>", source);
        Assert.DoesNotContain("quote-empty", source);
    }

    /// <summary>
    /// Verifies public quote dropzones upload files through the Web BFF and hand them off to QuoteEngine via a signed token.
    /// </summary>
    [Fact]
    public void QuoteDropzoneRoutesSelectedFilesToQuoteEngine()
    {
        var dropzone = ReadRepoFile("Maliev.Web.Client", "Components", "Quote", "QuoteDropzone.razor");
        var script = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "maliev-quote-dropzone.js");
        var app = ReadRepoFile("Maliev.Web.Bff", "Components", "App.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("type=\"file\"", dropzone, StringComparison.Ordinal);
        Assert.Contains("data-opening-label=\"@OpeningText\"", dropzone, StringComparison.Ordinal);
        Assert.Contains("FormatsButtonText", dropzone, StringComparison.Ordinal);
        Assert.Contains("FormatsExpanded => _isFormatsOpen ? \"true\" : \"false\"", dropzone, StringComparison.Ordinal);
        Assert.Contains("SupportedFormats", dropzone, StringComparison.Ordinal);
        Assert.Contains("\".STL\", \".STEP\", \".STP\", \".3MF\", \".OBJ\", \".IGS\", \".IGES\", \".GLTF\", \".GLB\"", dropzone, StringComparison.Ordinal);
        Assert.Contains("accept=\"@SupportedFormatsAccept\"", dropzone, StringComparison.Ordinal);
        Assert.Contains("DotNetObjectReference<QuoteDropzone>", dropzone, StringComparison.Ordinal);
        Assert.Contains("[JSInvokable]", dropzone, StringComparison.Ordinal);
        Assert.Contains("CloseFormatsAsync", dropzone, StringComparison.Ordinal);
        Assert.Contains("malievQuoteDropzone.register", dropzone, StringComparison.Ordinal);
        Assert.Contains("malievQuoteDropzone.registerFormatDismissal", dropzone, StringComparison.Ordinal);
        Assert.Contains("event.dataTransfer.files", script, StringComparison.Ordinal);
        Assert.Contains("isInteractiveChild(event)", script, StringComparison.Ordinal);
        Assert.Contains("data-dropzone-interactive", script, StringComparison.Ordinal);
        Assert.Contains("registerFormatDismissal(shellId, dotNetReference)", script, StringComparison.Ordinal);
        Assert.Contains("document.addEventListener(\"pointerdown\", closeIfOutside, true)", script, StringComparison.Ordinal);
        Assert.Contains("unregisterFormatDismissal(shellId)", script, StringComparison.Ordinal);
        Assert.Contains("\"gltf\", \"glb\"", script, StringComparison.Ordinal);
        Assert.DoesNotContain("\"blend\"", script, StringComparison.Ordinal);
        Assert.DoesNotContain("\"fbx\"", script, StringComparison.Ordinal);
        Assert.Contains("dropzone.addEventListener(\"keydown\", handleKeydown)", script, StringComparison.Ordinal);
        Assert.Contains("routeToQuoteEngine(Array.from(input.files), dropzone, quoteEngineUrl, state)", script, StringComparison.Ordinal);
        Assert.Contains("redirectToQuoteEngine(quoteEngineUrl, handoff)", script, StringComparison.Ordinal);
        Assert.Contains("window.location.assign(url.toString())", script, StringComparison.Ordinal);
        Assert.Contains("Opening quote engine", script, StringComparison.Ordinal);
        // Handoff upload pipeline assertions
        Assert.Contains("/web/v1/quote/uploads/resumable", script, StringComparison.Ordinal);
        Assert.Contains("Content-Range", script, StringComparison.Ordinal);
        Assert.Contains("Uploading ", script, StringComparison.Ordinal);
        Assert.Contains("uploadAndBuildHandoff", script, StringComparison.Ordinal);
        Assert.Contains("toBase64Url", script, StringComparison.Ordinal);
        Assert.Contains("url.searchParams.set(\"handoff\", handoff)", script, StringComparison.Ordinal);
        Assert.DoesNotContain("Content-Length", script, StringComparison.Ordinal);
        Assert.Contains(".landing-quote-dropzone.is-opening", styles, StringComparison.Ordinal);
        Assert.Contains(".landing-quote-dropzone-divider", styles, StringComparison.Ordinal);
        Assert.Contains(".landing-quote-dropzone-browse", styles, StringComparison.Ordinal);
        Assert.Contains(".landing-quote-dropzone-format-button", styles, StringComparison.Ordinal);
        Assert.Contains("cursor: help;", styles, StringComparison.Ordinal);
        Assert.Contains(".landing-quote-dropzone-format-button::after", styles, StringComparison.Ordinal);
        Assert.Contains(".landing-quote-dropzone-format-popover", styles, StringComparison.Ordinal);
        Assert.Contains("grid-template-columns: repeat(3, max-content);", styles, StringComparison.Ordinal);
        Assert.Contains("z-index: 20;", styles, StringComparison.Ordinal);
        Assert.Contains(".landing-quote-dropzone-action {\n  display: inline-flex;\n  align-items: center;\n  justify-content: center;\n  min-height: 22px;\n  color: var(--muted);", styles, StringComparison.Ordinal);
        Assert.Contains("font-family: var(--font-mono);\n  font-size: .78rem;", styles, StringComparison.Ordinal);
        Assert.Contains(".landing-quote-dropzone-action:hover,\n.landing-quote-dropzone:focus-visible .landing-quote-dropzone-action", styles, StringComparison.Ordinal);
        Assert.Contains("background: transparent;", styles, StringComparison.Ordinal);
        Assert.Contains("js/maliev-quote-dropzone.js", app, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies the home hero metrics render crawlable final values and enhance them with viewport-triggered count-up motion.
    /// </summary>
    [Fact]
    public void HomeHeroMetricsUseCountUpEnhancement()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Pages", "Home.razor");
        var content = ReadRepoFile("Maliev.Web.Client", "Content", "SiteContent.cs");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");
        var app = ReadRepoFile("Maliev.Web.Bff", "Components", "App.razor");
        var script = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "maliev-countup.js");

        Assert.Contains("!string.IsNullOrWhiteSpace(metric.Suffix)", source);
        Assert.Contains("data-count-up", source);
        Assert.Contains("data-count-target=\"@metric.CountTarget\"", source);
        Assert.Contains("data-count-suffix=\"@metric.Suffix\"", source);
        Assert.Contains("data-count-final=\"@metric.Value.For(Preferences.Culture)\"", source);
        Assert.Contains("new(Text(\"12,000+\", \"12,000+\"), Text(\"parts produced\", \"ชิ้นงานที่ผลิตแล้ว\"), 12000, \"+\")", content);
        Assert.Contains("new(Text(\"850+\", \"850+\"), Text(\"businesses served\", \"ธุรกิจที่ให้บริการ\"), 850, \"+\")", content);
        Assert.Contains("internal sealed record MetricItem(LocalizedText Value, LocalizedText Label, int CountTarget, string Suffix = \"\")", content);
        Assert.Contains("js/maliev-countup.js", app);
        Assert.Contains("requestAnimationFrame", script);
        Assert.Contains("IntersectionObserver", script);
        Assert.Contains("prefers-reduced-motion: reduce", script);
        Assert.Contains("Intl.NumberFormat(\"en-US\"", script);
        Assert.Contains("[data-count-up]", script);
        Assert.Contains("const defaultDuration = 2400;", script);
        Assert.Contains("const counterIndex", script);
        Assert.Contains("const staggerDelay = counterIndex * 180;", script);
        Assert.Contains("setTimeout(() => animateCounter", script);
        Assert.Contains("counter.dataset.countState = \"running\";", script);
        Assert.Contains("font-variant-numeric: tabular-nums", styles);
    }

    /// <summary>
    /// Verifies the lower landing sections use finished page sections and no proof-band mock block.
    /// </summary>
    [Fact]
    public void HomeUsesFinishedManufacturingAndNewsSections()
    {
        var root = FindRepoRoot();
        var source = ReadRepoFile("Maliev.Web.Client", "Pages", "Home.razor");
        var content = ReadRepoFile("Maliev.Web.Client", "Content", "SiteContent.cs");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");
        var industryCardStylesStart = styles.IndexOf(".industry-sector-card {", StringComparison.Ordinal);
        var industryCardHoverStylesStart = styles.IndexOf(".industry-sector-card:is(:hover, :focus-visible)", StringComparison.Ordinal);
        var industryCardFocusStylesStart = styles.IndexOf(".industry-sector-card:focus-visible", StringComparison.Ordinal);
        Assert.True(industryCardStylesStart >= 0);
        Assert.True(industryCardHoverStylesStart > industryCardStylesStart);
        Assert.True(industryCardFocusStylesStart > industryCardHoverStylesStart);
        var industryCardStyles = styles[industryCardStylesStart..industryCardHoverStylesStart];
        var industryCardHoverStyles = styles[industryCardHoverStylesStart..industryCardFocusStylesStart];

        Assert.Contains("home-services-shell", source);
        Assert.Contains("home-services-tabs", source);
        Assert.Contains("home-services-panel", source);
        Assert.Contains("feature-band", source);
        Assert.Contains("industry-sector-band", source);
        Assert.Contains("industry-sector-band-inner", source);
        Assert.Contains("industry-sector-list", source);
        Assert.Contains("Major industries served", source);
        Assert.Contains("We help engineering teams move from prototype to usable production parts", source);
        Assert.Contains("industry-sector-card", source);
        Assert.Contains("industry-sector-card-copy", source);
        Assert.Contains("industry-sector-link", source);
        Assert.Contains("Learn more", source);
        Assert.Contains("Aerospace", source);
        Assert.Contains("Lightweight prototypes, inspection aids, and production support parts", source);
        Assert.Contains("Automotive", source);
        Assert.Contains("Rapid fixtures, jig components, and validation samples", source);
        Assert.Contains("Consumer electronics", source);
        Assert.Contains("Enclosures, inserts, fit-check models, and cosmetic prototypes", source);
        Assert.Contains("Medical", source);
        Assert.Contains("Device housings, ergonomic prototypes, and cleanable workshop-built components", source);
        Assert.Contains("IndustrySectors", source);
        Assert.Contains("industry-sector-image", source);
        Assert.Contains("/images/blog/articles/instant-part-pricing-02.jpg", source);
        Assert.Contains("/images/blog/surface-text-labels.jpg", source);
        Assert.Contains("/images/blog/articles/enclosure-snap-fits-01.jpg", source);
        Assert.Contains("/images/blog/instant-part-pricing.jpg", source);
        Assert.Contains("/services/cnc-machining", source);
        Assert.Contains("/services/deviation-analysis", source);
        Assert.Contains("/services/3d-design", source);
        Assert.Contains("/services/rapid-prototyping", source);
        Assert.Contains("Aircraft program hardware used as a visual reference for aerospace-adjacent engineering support", source);
        Assert.Contains("Vehicle body and trim surface used as a visual reference for automotive prototype validation", source);
        Assert.Contains("Clear consumer electronics enclosure prototype with visible internal fit and fastening details", source);
        Assert.Contains("Cleanroom product review scene representing medical device prototype handling and inspection", source);
        Assert.DoesNotContain("Used by engineers across industrial sectors", source);
        Assert.DoesNotContain("Trusted by engineers at", source);
        Assert.DoesNotContain("SIEMENS", source);
        Assert.DoesNotContain("BOSCH", source);
        Assert.True(
            source.IndexOf("<section class=\"landing-hero\"", StringComparison.Ordinal) <
            source.IndexOf("<section class=\"industry-sector-band\"", StringComparison.Ordinal));
        Assert.True(
            source.IndexOf("<section class=\"industry-sector-band\"", StringComparison.Ordinal) <
            source.IndexOf("<section class=\"section home-services-section\"", StringComparison.Ordinal));
        Assert.Contains("process-section", source);
        Assert.Contains("id=\"workflow-carousel\"", source);
        Assert.Contains("process-grid", source);
        Assert.Contains("bindWorkflowStepReveal", source);
        Assert.Contains("Quote to part workflow", source);
        Assert.DoesNotContain("process-step-kicker", source);
        Assert.DoesNotContain("How it works", source);
        Assert.Contains("Quote to part, no friction", source);
        Assert.Contains("Upload CAD once, configure online, let MALIEV make it", source);
        Assert.Contains("Drop your file", source);
        Assert.Contains("Configure online", source);
        Assert.Contains("We make it", source);
        Assert.Contains("Inspect and ship", source);
        Assert.DoesNotContain("workflow-step-visual", source);
        Assert.Contains("workflow-step-number", source);
        Assert.DoesNotContain("process-eyebrow", source);
        Assert.DoesNotContain("Customer workflow", source);
        Assert.DoesNotContain("ขั้นตอนลูกค้า", source);
        Assert.DoesNotContain("<MudIcon Icon=\"@step.Icon\" Size=\"Size.Large\" />", source);
        Assert.DoesNotContain("viewBox=\"0 0 64 64\"", source);
        Assert.DoesNotContain("Icons.Material.Filled.UploadFile", source);
        Assert.DoesNotContain("Icons.Material.Filled.FactCheck", source);
        Assert.DoesNotContain("Icons.Material.Filled.PriceChange", source);
        Assert.DoesNotContain("Icons.Material.Filled.Inventory2", source);
        Assert.DoesNotContain("quote-flow-band", source);
        Assert.Contains("blog-grid", source);
        Assert.Contains("SiteContent.BlogPosts", source);
        Assert.Contains("HomeBlogPostCount = 3", source);
        Assert.Contains("blogPosts.Take(HomeBlogPostCount)", source);
        Assert.Contains("PersistentComponentState", source);
        Assert.Contains("RandomizeSupportingServices", source);
        Assert.Contains("Shuffle(SiteContent.CaseStudies)", source);
        Assert.Contains("Shuffle(SiteContent.BlogPosts)", source);
        Assert.Contains("RandomNumberGenerator.GetInt32", source);
        Assert.Contains("case-card-media", source);
        Assert.Contains("home-work-section", source);
        Assert.Contains("home-work-heading", source);
        Assert.Contains("home-work-card", source);
        Assert.Contains("What we've made lately", source);
        Assert.Contains("ผลงานล่าสุดที่เราสร้าง", source);
        Assert.Contains("&rarr;", source);
        Assert.Contains(".home-work-section", styles);
        Assert.Contains("--home-work-surface: #f2f3f5;", styles);
        Assert.Contains("--home-work-surface: #11161d;", styles);
        Assert.Contains(".home-work-link", styles);
        Assert.Contains(".home-work-card", styles);
        Assert.Contains(".industry-sector-band", styles);
        Assert.Contains(".industry-sector-band {\n  width: 100%;\n  padding: clamp(76px, 7vw, 112px) 32px;\n  background: var(--paper);", styles);
        Assert.Contains(".home-services-section {\n  padding-top: clamp(82px, 8vw, 124px);\n  padding-bottom: clamp(88px, 8vw, 132px);\n  max-width: none;\n  background: var(--paper);", styles);
        Assert.Contains(".home-services-shell {\n  display: grid;\n  gap: clamp(26px, 3vw, 38px);", styles);
        Assert.Contains(".industry-sector-list {\n  display: grid;\n  grid-template-columns: repeat(4, minmax(0, 1fr));", styles);
        Assert.Contains(".industry-sector-list li", styles);
        Assert.Contains(".industry-sector-card", styles);
        Assert.Contains(".industry-sector-image", styles);
        Assert.Contains("aspect-ratio: 16 / 8.6;", styles);
        Assert.Contains("object-fit: cover;", styles);
        Assert.Contains(".industry-sector-card-copy", styles);
        Assert.Contains("grid-template-rows: auto minmax(0, 1fr) auto;", styles);
        Assert.Contains("align-content: stretch;", styles);
        Assert.Contains(".industry-sector-card-title", styles);
        Assert.Contains(".industry-sector-card-body", styles);
        Assert.Contains(".industry-sector-link", styles);
        Assert.Contains("border-radius: 4px;", styles);
        Assert.Contains("text-align: left;", styles);
        Assert.Contains("cursor: pointer;", styles);
        Assert.Contains(".industry-sector-card:is(:hover, :focus-visible)", styles);
        Assert.Contains("transition: box-shadow .22s ease-in-out;", industryCardStyles);
        Assert.DoesNotContain("transform", industryCardStyles);
        Assert.DoesNotContain("background-color", industryCardStyles);
        Assert.Contains("box-shadow:", industryCardHoverStyles);
        Assert.DoesNotContain("transform", industryCardHoverStyles);
        Assert.DoesNotContain("background:", industryCardHoverStyles);
        Assert.DoesNotContain(".landing-hero::after", styles);
        Assert.DoesNotContain(".industry-sector-list li {\n  display: grid;\n  place-items: center;", styles);
        Assert.Contains(".industry-sector-list {\n    grid-template-columns: repeat(2, minmax(0, 1fr));", styles);
        Assert.Contains(".industry-sector-list {\n    grid-template-columns: 1fr;", styles);
        Assert.Contains(".industry-sector-card-copy {\n    min-height: 220px;", styles);
        Assert.Contains(".industry-sector-image {\n    aspect-ratio: 16 / 8.8;", styles);
        Assert.DoesNotContain(".material-category-grid,\n  .industry-sector-list,\n  .service-answer-card-grid--routes", styles);
        Assert.DoesNotContain("Recent work from", source);
        Assert.DoesNotContain("logo-heading", source);
        Assert.DoesNotContain("<img src=\"/images/logo.svg\" alt=\"MALIEV\" />", source);
        Assert.Contains("https://images.unsplash.com/", content);
        Assert.Contains("ThreeDimensionalPrinterImageUrl", content);
        Assert.Contains("ThreeDimensionalScannerImageUrl", content);
        Assert.Contains("InjectionMoldingLineImageUrl", content);
        Assert.Contains("DeviationAnalysisMapImageUrl", content);
        Assert.Contains("https://gomeasure3d.com/wp-content/uploads/2019/09/geomagic-control-x-deviation-analysis-800w.jpg", content);
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
        Assert.Contains(".section-link {\n  display: inline-flex;\n  align-items: center;", styles);
        Assert.Contains(".section-heading .section-link {\n  justify-self: end;\n  width: fit-content;\n  max-width: 100%;", styles);
        Assert.Contains("border-radius: 999px;", styles);
        Assert.Contains(".section-link::after", styles);
        Assert.Contains("content: \"→\";", styles);
        Assert.DoesNotContain("proof-band", source);
        Assert.DoesNotContain("Build. Test. Produce.", source);
        Assert.DoesNotContain("Customer-facing quoting and commerce in one path", source);
        Assert.DoesNotContain("<span class=\"button primary\">@Text(\"Get part price\", \"ดูราคาชิ้นงาน\")</span>", source);
        Assert.Contains("class=\"final-cta-copy\"", source);
        Assert.Matches(@"@media \(max-width: 960px\)[\s\S]*?\.final-cta\s*\{[^}]*justify-items:\s*center;[^}]*gap:\s*26px;[^}]*text-align:\s*center;", styles);
        Assert.Matches(@"@media \(max-width: 960px\)[\s\S]*?\.final-cta-copy\s*\{[^}]*order:\s*2;[^}]*display:\s*grid;[^}]*justify-items:\s*center;", styles);
        Assert.Matches(@"@media \(max-width: 960px\)[\s\S]*?\.final-cta-copy \.lede\s*\{[^}]*max-width:\s*58ch;", styles);
        Assert.Matches(@"@media \(max-width: 960px\)[\s\S]*?\.final-cta > \.final-dropzone\s*\{[^}]*order:\s*1;[^}]*justify-self:\s*center;[^}]*width:\s*min\(100%, 560px\);", styles);
        Assert.DoesNotContain("final-dropzone-arrow", source);
        Assert.DoesNotContain(".final-dropzone-arrow", styles);
        Assert.DoesNotContain(".final-dropzone-icon", styles);
        Assert.DoesNotContain(".final-dropzone-copy", styles);
        Assert.Contains(".service-grid {\n  grid-template-columns: repeat(3, minmax(0, 1fr));", styles);
        Assert.Contains(".home-services-section {\n    padding-top: clamp(78px, 11vw, 108px);", styles);
        Assert.Contains(".home-services-section {\n    padding-top: 72px;\n    padding-bottom: 78px;", styles);
        Assert.DoesNotContain("grid-template-columns: 1.2fr 1fr 1fr;", styles);
        Assert.Contains("grid-template-rows: auto minmax(0, 1fr) auto", styles);
        Assert.Contains("align-content: start", styles);
        Assert.Contains("grid-auto-rows: minmax(320px, 1fr);", styles);
        Assert.Contains("align-items: stretch;", styles);
        Assert.Contains("height: 100%;", styles);
        Assert.Contains(".service-card.primary {\n  color: var(--inverse-text);", styles);
        Assert.Contains("transition: box-shadow .52s ease-in-out;", styles);
        Assert.Contains(".service-card:is(:hover, :focus-visible) {\n  box-shadow: rgba(17, 24, 39, .18) 0 24px 64px -30px, rgba(17, 24, 39, .10) 0 8px 22px -18px, var(--shadow-card);", styles);
        Assert.Contains(".service-card.primary:is(:hover, :focus-visible) {\n  box-shadow: rgba(0, 0, 0, .32) 0 26px 68px -30px, rgba(0, 0, 0, .20) 0 10px 24px -18px, var(--shadow-dark-card);", styles);
        Assert.Contains(".card-link {\n  align-self: end;\n  color: var(--blue);", styles);
        Assert.Contains(".service-card.primary .card-link {\n  color: var(--inverse-text);", styles);
        Assert.Contains(".service-card.primary .card-link::after", styles);
        Assert.Contains("content: \"→\";", styles);
        Assert.Contains(".service-card-media {\n  height: clamp(190px, 18vw, 236px);", styles);
        Assert.DoesNotContain(".service-card.primary .service-card-media", styles);
        Assert.DoesNotContain("grid-row: span 2;", styles);
        Assert.Contains("body {\n  margin: 0;\n  color: var(--ink);\n  background: var(--paper);\n  overflow-x: hidden;\n  font-size: 15px;", styles);
        Assert.Contains(".body {\n  color: var(--muted);\n  font-size: 1rem;\n  line-height: 1.6;", styles);
        Assert.Contains(".service-card h3,\n.case-card h3,\n.blog-card h3 {\n  margin: 0;\n  font-size: 1.0625rem;", styles);
        Assert.Contains(".service-card p,\n.case-card p,\n.blog-card p,\n.product-card p {\n  margin: 0;\n  color: var(--muted);\n  font-size: .9375rem;", styles);
        Assert.Contains(".card-link {\n  align-self: end;\n  color: var(--blue);\n  font-size: .875rem;", styles);
        Assert.Contains(".card-meta,\n.small,\n.load-message,\n.form-status,\n.success-message {\n  color: var(--muted);\n  font-size: .8125rem;", styles);
        Assert.Contains("filter: var(--logo-filter)", styles);
        Assert.Contains(".process-section", styles);
        Assert.Contains(".process-section {\n  max-width: none;\n  position: relative;\n  padding-top: clamp(96px, 10vw, 150px);\n  padding-bottom: clamp(92px, 9vw, 138px);\n  background: var(--paper);", styles);
        Assert.True(
            source.IndexOf("<section id=\"workflow-carousel\"", StringComparison.Ordinal) <
            source.IndexOf("<section id=\"machine-feature-preview\"", StringComparison.Ordinal));
        Assert.True(
            source.IndexOf("<section id=\"machine-feature-preview\"", StringComparison.Ordinal) <
            source.IndexOf("<section class=\"section home-work-section\"", StringComparison.Ordinal));
        Assert.DoesNotContain(".process-section::after", styles);
        Assert.DoesNotContain(".process-section::before", styles);
        var processStylesStart = styles.IndexOf(".process-grid", StringComparison.Ordinal);
        var processStylesEnd = styles.IndexOf(".service-detail-section", StringComparison.Ordinal);
        Assert.True(processStylesStart >= 0 && processStylesEnd > processStylesStart);
        var processStyles = styles[processStylesStart..processStylesEnd];
        Assert.DoesNotContain("rgba(222, 29, 141", processStyles);
        Assert.DoesNotContain("#7c3aed", processStyles);
        Assert.DoesNotContain("#be123c", processStyles);
        Assert.Contains("--workflow-accent: #315f72;", processStyles);
        Assert.Contains("--workflow-accent: #3f6f62;", processStyles);
        Assert.Contains("--workflow-accent: #8a6a2d;", processStyles);
        Assert.Contains("--workflow-accent: #743f3f;", processStyles);
        Assert.DoesNotContain(".process-step-kicker", processStyles);
        Assert.Contains("grid-template-columns: repeat(4, minmax(0, 1fr));", processStyles);
        Assert.Contains("align-items: start;", processStyles);
        Assert.Contains("grid-auto-flow: row;", processStyles);
        Assert.Contains("gap: clamp(36px, 5vw, 84px);", processStyles);
        Assert.Contains(".process-grid.workflow-steps-ready .workflow-step", processStyles);
        Assert.Contains(".process-grid.workflow-steps-visible .workflow-step", processStyles);
        Assert.Contains("@keyframes workflowStepReveal", processStyles);
        Assert.Contains("clip-path: inset(0 100% 0 0);", processStyles);
        Assert.Contains("clip-path: inset(0 0 0 0);", processStyles);
        Assert.Contains("width: 100%;", processStyles);
        Assert.Contains("min-width: 0;", processStyles);
        Assert.Contains("justify-items: start;", processStyles);
        Assert.Contains(".workflow-step:nth-child(odd) {\n    grid-column: 1;", styles);
        Assert.Contains(".workflow-step:nth-child(even) {\n    grid-column: 2;", styles);
        Assert.Contains(".workflow-step,\n  .workflow-step:nth-child(odd),\n  .workflow-step:nth-child(even) {\n    grid-column: auto;", styles);
        Assert.Contains("animation-delay: .26s;", processStyles);
        Assert.Contains("animation-delay: .52s;", processStyles);
        Assert.Contains("animation-delay: .78s;", processStyles);
        Assert.DoesNotContain("animation-delay: .16s;", processStyles);
        Assert.DoesNotContain("animation-delay: .32s;", processStyles);
        Assert.DoesNotContain("animation-delay: .48s;", processStyles);
        Assert.Contains("background: transparent;", processStyles);
        Assert.Contains("border: 0;", processStyles);
        Assert.Contains("border-radius: 0;", processStyles);
        Assert.Contains("width: 42px;", processStyles);
        Assert.Contains("height: 42px;", processStyles);
        Assert.Contains("font-size: .94rem;", processStyles);
        Assert.Contains("html[data-theme=\"dark\"] .workflow-step", styles);
        Assert.DoesNotContain("transform: translateY(-2px);", processStyles);
        Assert.DoesNotContain("transform: translateX(-24px);", processStyles);
        Assert.DoesNotContain("background: linear-gradient(180deg, #fff 0%, #fbfbfa 100%);", processStyles);
        Assert.DoesNotContain("background: rgba(255, 255, 255, .9);", processStyles);
        Assert.DoesNotContain("background: linear-gradient(180deg, #fff 0%, var(--workflow-accent-soft) 100%);", processStyles);
        Assert.DoesNotContain(".process-eyebrow", styles);
        Assert.Contains(".workflow-step-number", styles);
        Assert.DoesNotContain(".workflow-step-visual .mud-icon-root", styles);
        Assert.DoesNotContain(".workflow-step-visual span", styles);
        Assert.DoesNotContain(".workflow-step--dfm .workflow-step-visual", styles);
        Assert.Contains("home-services-section", source);
        Assert.Contains(".home-services-section", styles);
        Assert.DoesNotContain("padding-top: clamp(44px, 5vw, 72px);", styles);
        Assert.Contains("role=\"tablist\"", source);
        Assert.Contains("role=\"tabpanel\"", source);
        Assert.Contains("aria-selected=\"@HomeServiceAriaSelected(service)\"", source);
        Assert.Contains("SelectedHomeService", source);
        Assert.Contains("SelectHomeService(service)", source);
        Assert.Contains("HomeServiceTabClass", source);
        Assert.Contains("home-services-title", source);
        Assert.Contains("Our manufacturing services", source);
        Assert.Contains("home-services-proof-list", source);
        Assert.Contains("home-services-cta", source);
        Assert.Contains("home-services-media", source);
        Assert.DoesNotContain("Seven services. One workshop.", source);
        Assert.DoesNotContain("Vertically integrated so your part never waits for a vendor.", source);
        Assert.DoesNotContain("home-services-title-lead", source);
        Assert.Contains(".home-services-title {\n  margin: 0;\n  color: var(--ink);", styles);
        Assert.Contains("font-family: var(--maliev-font-sans);", styles);
        Assert.Contains("font-size: clamp(2rem, 3vw, 2.85rem);", styles);
        Assert.Contains("gap: clamp(26px, 3vw, 38px);", styles);
        Assert.Contains(".home-services-tabs {\n  display: flex;\n  justify-content: center;", styles);
        Assert.Contains("gap: clamp(16px, 2.4vw, 34px);", styles);
        Assert.Contains(".home-services-tab {\n  position: relative;", styles);
        Assert.Contains("color: var(--muted);", styles);
        Assert.Contains(".home-services-tab.is-active {\n  color: var(--ink);", styles);
        Assert.Contains("overflow-x: auto;\n  overflow-y: hidden;", styles);
        Assert.Contains(".home-services-tab::after", styles);
        Assert.Contains(".home-services-panel {\n  display: grid;\n  grid-template-columns: minmax(0, .94fr) minmax(240px, .78fr) minmax(260px, .88fr);", styles);
        Assert.Contains("grid-template-areas: \"copy proof media\";", styles);
        Assert.Contains("gap: clamp(22px, 3vw, 44px);", styles);
        Assert.DoesNotContain("gap: clamp(34px, 5vw, 76px);", styles);
        Assert.Contains(".home-services-proof-list li::before", styles);
        Assert.Contains(".home-services-media img", styles);
        Assert.Contains("height: clamp(220px, 18vw, 260px);", styles);
        Assert.Contains("object-fit: cover;", styles);
        Assert.Contains("border: 1px solid var(--rule);", styles);
        Assert.Contains("grid-template-columns: minmax(0, 1fr) minmax(220px, .82fr);", styles);
        Assert.Contains("\"copy media\"\n      \"proof media\";", styles);
        Assert.Contains(".home-services-media {\n    justify-items: start;\n    width: 100%;", styles);
        Assert.Contains("height: clamp(210px, 32vw, 320px);", styles);
        Assert.Contains(".home-services-tabs {\n    display: grid;\n    grid-template-columns: repeat(2, minmax(0, 1fr));", styles);
        Assert.Contains("overflow: visible;\n    border-bottom: 0;", styles);
        Assert.Contains("\"copy\"\n      \"media\"\n      \"proof\";", styles);
        Assert.Matches(@"@media \(max-width: 680px\)[\s\S]*?\.home-services-media figcaption\s*\{[^}]*width:\s*100%;[^}]*max-width:\s*none;[^}]*text-align:\s*left;", styles);
        Assert.Contains("machine-feature-scroller", source);
        Assert.Contains("machine-feature", source);
        Assert.Contains("id=\"machine-feature-preview\"", source);
        Assert.Contains("aria-labelledby=\"imm-intro-title\"", source);
        Assert.Contains("data-selected-variant=\"@SelectedMachineVariant.Key\"", source);
        Assert.Contains("data-compressor-included=\"@MachineCompressorAriaPressed\"", source);
        Assert.Contains("data-machine-panel=\"intro\"", source);
        Assert.Contains("machine-feature-panel", source);
        Assert.Contains("data-machine-content=\"intro\"", source);
        Assert.Contains("data-machine-content=\"details\"", source);
        Assert.DoesNotContain("machine-feature-panel machine-feature-panel--details", source);
        Assert.Contains("<span>@Text(\"Pneumatic\", \"เครื่องฉีดพลาสติก\")</span>", source);
        Assert.Contains("<span class=\"accent-blue\">@Text(\"Injection Machine\", \"ระบบลม\")</span>", source);
        Assert.Contains("machine-feature-title--intro machine-feature-title--reveal", source);
        Assert.Contains("machine-feature-title machine-feature-title--intro", source);
        Assert.Contains("machine-feature-backdrop", source);
        Assert.Contains("machine-feature-backdrop--light", source);
        Assert.Contains("machine-feature-backdrop--dark", source);
        Assert.Contains("@key=\"SelectedMachineImageKey\"", source);
        Assert.Contains("src=\"@SelectedMachineFeatureImageUrl\"", source);
        Assert.Contains("src=\"@SelectedMachineDarkFeatureImageUrl\"", source);
        Assert.Contains("alt=\"@SelectedMachineFeatureImageAlt.For(Preferences.Culture)\"", source);
        Assert.Contains("@SelectedMachineVariant.Body.For(Preferences.Culture)", source);
        Assert.Contains("/images/products/pneumatic-injection-molding-machines.png", source);
        Assert.Contains("/images/products/pneumatic-injection-molding-machines-dark.png", source);
        Assert.Contains("/images/products/pneumatic-injection-molding-machines-compressor.png", source);
        Assert.Contains("/images/products/pneumatic-injection-molding-machines-compressor-dark.png", source);
        Assert.True(File.Exists(Path.Combine(root, "Maliev.Web.Bff", "wwwroot", "images", "products", "pneumatic-injection-molding-machines.png")));
        Assert.True(File.Exists(Path.Combine(root, "Maliev.Web.Bff", "wwwroot", "images", "products", "pneumatic-injection-molding-machines-dark.png")));
        Assert.True(File.Exists(Path.Combine(root, "Maliev.Web.Bff", "wwwroot", "images", "products", "pneumatic-injection-molding-machines-compressor.png")));
        Assert.True(File.Exists(Path.Combine(root, "Maliev.Web.Bff", "wwwroot", "images", "products", "pneumatic-injection-molding-machines-compressor-dark.png")));
        Assert.Contains("private string SelectedMachineFeatureImageUrl => _includeAirCompressor", source);
        Assert.Contains("? SelectedMachineVariant.CompressorFeatureImageUrl", source);
        Assert.Contains("private string SelectedMachineDarkFeatureImageUrl => _includeAirCompressor", source);
        Assert.Contains("? SelectedMachineVariant.CompressorDarkFeatureImageUrl", source);
        Assert.Contains("private LocalizedText SelectedMachineFeatureImageAlt => _includeAirCompressor", source);
        Assert.Contains("? SelectedMachineVariant.CompressorFeatureImageAlt", source);
        Assert.Contains("machine-configurator", source);
        Assert.Contains("role=\"radiogroup\"", source);
        Assert.Contains("role=\"radio\"", source);
        Assert.Contains("aria-checked=\"@MachineVariantAriaChecked(variant)\"", source);
        Assert.Contains("25L air compressor", source);
        Assert.Contains("ToggleMachineCompressor", source);
        Assert.Contains("MachineVariantButtonClass(variant)", source);
        Assert.Contains("WorkflowStepClass(step)", source);
        Assert.Contains("@ref=\"_machineFeaturePreview\"", source);
        Assert.DoesNotContain("private ElementReference _machineFeatureDetails;", source);
        Assert.Contains("@ref=\"_workflowCarousel\"", source);
        Assert.Contains("malievScroll.bindMachineFeatureHandoff", source);
        Assert.Contains("malievScroll.bindWorkflowStepReveal", source);
        Assert.Contains("type=\"button\"", source);
        Assert.Contains("data-machine-variant=\"@variant.Key\"", source);
        Assert.Contains("data-workflow-accent=\"@step.Accent\"", source);
        Assert.Contains("aria-current=\"@WorkflowAriaCurrent(step)\"", source);
        Assert.Contains("SelectMachineVariant(variant)", source);
        Assert.DoesNotContain("SelectWorkflowStepAsync(step, _workflowCarousel)", source);
        Assert.Contains("SelectWorkflowStepAsync(step, _machineFeaturePreview)", source);
        Assert.Contains("malievScroll.scrollIntoView", source);
        var scrollScript = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "maliev-scroll.js");
        Assert.Contains("bindWorkflowStepReveal", scrollScript);
        Assert.Contains("workflow-steps-ready", scrollScript);
        Assert.Contains("workflow-steps-visible", scrollScript);
        Assert.Contains("window.addEventListener('scroll', revealWhenVisible", scrollScript);
        Assert.Contains("bindMachineFeatureHandoff", scrollScript);
        Assert.Contains("switchMachinePanel", scrollScript);
        Assert.Contains("section.dataset.machinePanel = nextPanel", scrollScript);
        Assert.Contains("const snapMachinePanel = nextPanel =>", scrollScript);
        Assert.Contains("window.addEventListener('wheel', onWheel, { passive: false });", scrollScript);
        Assert.Contains("targetProgress = nextPanel === 'details' ? Math.max(0, (state.scrollSpace - 48) / state.scrollSpace) : 0", scrollScript);
        Assert.Contains("readHeaderOffset", scrollScript);
        Assert.Contains("scroller.offsetHeight", scrollScript);
        Assert.Contains("window.addEventListener('scroll', onScroll", scrollScript);
        Assert.Contains("window.requestAnimationFrame(updatePanel)", scrollScript);
        Assert.Contains("stickyTolerance", scrollScript);
        Assert.DoesNotContain("sectionIsReadyForHandoff", scrollScript);
        Assert.DoesNotContain("rect.top < window.innerHeight * .72", scrollScript);
        Assert.Contains("Math.abs(event.deltaY) < 12", scrollScript);
        Assert.Contains("Math.abs(event.deltaY) < Math.abs(event.deltaX)", scrollScript);
        Assert.Contains("ArrowUp: -1", scrollScript);
        Assert.DoesNotContain("scrollToDetails", scrollScript);
        Assert.DoesNotContain("introHalfPassed", scrollScript);
        Assert.DoesNotContain("hasSnappedToDetails", scrollScript);
        Assert.Contains("machine-feature-title--reveal", scrollScript);
        Assert.Contains("IntersectionObserver", scrollScript);
        Assert.Contains("classList.add('is-visible')", scrollScript);
        Assert.DoesNotContain("window.addEventListener('scroll', scheduleHalfwayHandoff", scrollScript);
        Assert.Contains("private sealed record ProcessStep(\n        string Number,\n        string Accent,\n        LocalizedText Title,\n        LocalizedText Body);", source);
        Assert.Contains("private sealed record MachineVariant(\n        string Key,\n        LocalizedText Title,\n        LocalizedText Subtitle,\n        LocalizedText Body,\n        string FeatureImageUrl,\n        string DarkFeatureImageUrl,\n        string CompressorFeatureImageUrl,\n        string CompressorDarkFeatureImageUrl,\n        LocalizedText FeatureImageAlt,\n        LocalizedText CompressorFeatureImageAlt,\n        IReadOnlyList<MachineStat> Stats);", source);
        Assert.DoesNotContain("https://shop.maliev.com/cdn/shop/files/machine-portrait.21.png", source);
        Assert.DoesNotContain("machine-feature-kicker", source);
        Assert.DoesNotContain("PIMM-30 / PIMM-50", source);
        Assert.DoesNotContain(".machine-feature-kicker", styles);
        Assert.DoesNotContain("PIMM-30", source);
        Assert.DoesNotContain("PIMM-50", source);
        Assert.Contains("h-display machine-feature-title", source);
        Assert.Contains("machine-feature-title-line machine-feature-title-lead", source);
        Assert.DoesNotContain("<br />\n            <span class=\"accent-blue\">@Text(\"machines.\", \"สำหรับล็อตเล็ก\")</span>", source);
        Assert.Contains("@Text(\"Pneumatic\", \"เครื่องฉีดพลาสติก\")", source);
        Assert.Contains("@Text(\"Injection Machine\", \"ระบบลม\")", source);
        Assert.DoesNotContain("สำหรับล็อตเล็ก", source);
        Assert.Contains("private string _selectedMachineVariantKey = \"50g\";", source);
        Assert.Contains("Recommended shop choice", source);
        Assert.Contains("larger shot volume", source);
        Assert.Contains("350°C melt range", source);
        Assert.Contains("Entry desktop trials", source);
        Assert.DoesNotContain("private string _selectedMachineVariantKey = \"30g\";", source);
        Assert.DoesNotContain("desktop plastics lab", source);
        Assert.Contains("30g machine", source);
        Assert.Contains("50g machine", source);
        Assert.DoesNotContain("30g trials", source);
        Assert.DoesNotContain("50g small batches", source);
        Assert.DoesNotContain("Tooling setup", source);
        Assert.DoesNotContain("Run and tune", source);
        Assert.Contains("machine-stat-grid", source);
        Assert.Contains("@foreach (var stat in SelectedMachineVariant.Stats)", source);
        Assert.Contains("new(SiteContent.Text(\"30g\", \"30g\"), SiteContent.Text(\"shot capacity\", \"ปริมาตรฉีดต่อครั้ง\"))", source);
        Assert.Contains("new(SiteContent.Text(\"50g\", \"50g\"), SiteContent.Text(\"shot capacity\", \"ปริมาตรฉีดต่อครั้ง\"))", source);
        Assert.Contains("new(SiteContent.Text(\"350°C\", \"350°C\"), SiteContent.Text(\"max melt\", \"อุณหภูมิสูงสุด\"))", source);
        Assert.DoesNotContain("<div><strong>30g/50g</strong><small>@Text(\"shot capacity\", \"ปริมาตรฉีดต่อครั้ง\")</small></div>", source);
        Assert.DoesNotContain("<div><strong>50g</strong><small>@Text(\"shot capacity\", \"ปริมาตรฉีดต่อครั้ง\")</small></div>", source);
        Assert.DoesNotContain("<div><strong>300/350°C</strong><small>@Text(\"max melt\", \"อุณหภูมิสูงสุด\")</small></div>", source);
        Assert.DoesNotContain("<div><strong>180°C</strong><small>@Text(\"max melt\", \"อุณหภูมิสูงสุด\")</small></div>", source);
        Assert.DoesNotContain("<div><strong>7 bar</strong><small>@Text(\"air supply\", \"แรงดันลม\")</small></div>", source);
        Assert.DoesNotContain("<div><strong>6 bar</strong><small>@Text(\"air supply\", \"แรงดันลม\")</small></div>", source);
        Assert.Contains("new(SiteContent.Text(\"30 days\", \"30 วัน\"), SiteContent.Text(\"lead time\", \"ระยะเวลา\"))", source);
        Assert.DoesNotContain("<div><strong>30d</strong><small>@Text(\"lead time\", \"ระยะเวลา\")</small></div>", source);
        Assert.DoesNotContain("<div><strong>14d</strong><small>@Text(\"lead time\", \"ระยะเวลา\")</small></div>", source);
        Assert.Contains("@Text(\"Configure\", \"ตั้งค่า\")", source);
        Assert.Contains("machine-configure-button", source);
        Assert.Contains("machine-tooling-link", source);
        Assert.Contains("@Text(\"Plan tooling with us\", \"วางแผนทูลลิ่งกับเรา\")", source);
        Assert.DoesNotContain("Configure selected machine", source);
        Assert.DoesNotContain("ตั้งค่าเครื่องที่เลือก", source);
        Assert.DoesNotContain("class=\"button secondary\" href=\"/contact\"", source);
        Assert.Contains(".machine-feature-scroller", styles);
        Assert.Contains("height: 200svh;", styles);
        Assert.DoesNotContain("height: 120svh;", styles);
        Assert.Contains("position: sticky;", styles);
        Assert.Contains(".machine-feature", styles);
        Assert.Contains("display: block;", styles);
        Assert.Contains("scroll-margin-top: var(--site-header-height, 72px);", styles);
        Assert.Contains(".machine-feature-panel", styles);
        Assert.Contains("height: 100%;", styles);
        Assert.Contains("min-height: 100%;", styles);
        Assert.Contains("--site-header-height: 66px;", styles);
        Assert.Contains("padding: clamp(42px, 6svh, 72px) max(32px, calc((100vw - var(--container)) / 2 + 32px));", styles);
        Assert.Contains("grid-template-columns: minmax(0, 1fr) minmax(520px, min(44vw, 680px));", styles);
        Assert.Contains("align-items: center;", styles);
        Assert.DoesNotContain(".machine-feature-panel--details", styles);
        Assert.Contains(".machine-feature-details-copy", styles);
        Assert.Contains(".machine-feature[data-machine-panel=\"details\"] .machine-feature-details-copy", styles);
        Assert.DoesNotContain(".machine-feature[data-machine-panel=\"details\"] .machine-feature-backdrop", styles);
        Assert.Contains(".machine-feature-title--intro", styles);
        Assert.Contains(".machine-feature-title--reveal", styles);
        Assert.Contains("transform: translateX(44px);", styles);
        Assert.Contains(".machine-feature-title--reveal.is-visible", styles);
        Assert.Contains(".machine-feature-title--intro > span", styles);
        Assert.Contains("max-width: min(440px, 37vw);", styles);
        Assert.Contains("max-width: min(680px, 45vw);", styles);
        Assert.Contains("margin-bottom: clamp(88px, 12svh, 132px);", styles);
        Assert.Contains("text-align: right;", styles);
        Assert.Contains("margin-bottom: 0;", styles);
        Assert.Contains("@media (min-width: 961px) and (max-height: 920px)", styles);
        Assert.Contains("padding-block: clamp(32px, 5svh, 54px);", styles);
        Assert.Contains("font-size: clamp(3rem, 4.15vw, 3.75rem);", styles);
        Assert.Contains(".machine-variant-button {\n    min-height: 72px;", styles);
        Assert.Contains(".machine-addon-option {\n    min-height: 62px;", styles);
        Assert.Matches(
            @"@media \(max-width: 960px\)[\s\S]*?\.machine-feature\s*\{[^}]*position:\s*sticky;[^}]*height:\s*calc\(100svh - var\(--site-header-height\)\);[^}]*overflow:\s*clip;",
            styles);
        Assert.Matches(
            @"@media \(max-width: 960px\)[\s\S]*?\.machine-feature-panel\s*\{[^}]*align-content:\s*start;[^}]*justify-items:\s*center;",
            styles);
        Assert.Matches(
            @"@media \(max-width: 960px\)[\s\S]*?\.machine-feature-copy\s*\{[^}]*justify-self:\s*center;[^}]*text-align:\s*center;",
            styles);
        Assert.Matches(
            @"@media \(max-width: 960px\)[\s\S]*?\.machine-feature-intro-copy\s*\{[^}]*display:\s*grid;[^}]*text-align:\s*center;",
            styles);
        Assert.Matches(
            @"@media \(max-width: 960px\)[\s\S]*?\.machine-feature-details-copy\s*\{[^}]*align-self:\s*start;[^}]*align-content:\s*start;[^}]*justify-self:\s*center;[^}]*max-height:\s*calc\(100svh - var\(--site-header-height\) - clamp\(68px, 12svh, 118px\)\);[^}]*overflow-y:\s*auto;[^}]*text-align:\s*center;",
            styles);
        Assert.Matches(
            @"@media \(max-width: 960px\)[\s\S]*?\.machine-feature-title--intro\s*\{[^}]*text-align:\s*center;",
            styles);
        Assert.Matches(
            @"@media \(max-width: 960px\)[\s\S]*?\.machine-feature-title--reveal\s*\{[^}]*transform:\s*translateY\(-22px\);",
            styles);
        Assert.Matches(
            @"@media \(max-width: 960px\)[\s\S]*?\.machine-feature-backdrop\s*\{[^}]*position:\s*absolute;[^}]*inset:\s*0;[^}]*height:\s*100%;[^}]*object-fit:\s*cover;",
            styles);
        Assert.Contains(".machine-feature-backdrop", styles);
        Assert.Contains("height: calc(100% + clamp(132px, 16svh, 180px));", styles);
        Assert.Contains("bottom: clamp(-132px, -12svh, -96px);", styles);
        Assert.Contains("width: 100%;", styles);
        Assert.Contains("max-width: none;", styles);
        Assert.Contains("object-fit: cover;", styles);
        Assert.Contains("object-position: left bottom;", styles);
        Assert.Contains("object-position: center bottom;", styles);
        Assert.Contains("filter: contrast(1.04) saturate(1.02);", styles);
        Assert.Contains("transition: opacity .48s ease-in-out;", styles);
        Assert.Contains(".machine-feature-backdrop.machine-feature-backdrop--dark {\n  opacity: 0;", styles);
        Assert.Contains("html[data-theme=\"dark\"] .machine-feature-backdrop--light {\n  opacity: 0;", styles);
        Assert.Contains("html[data-theme=\"dark\"] .machine-feature-backdrop--dark {\n  opacity: 1;", styles);
        Assert.DoesNotContain("display: none;\n}\n\nhtml[data-theme=\"dark\"] .machine-feature-backdrop--light", styles);
        Assert.DoesNotContain("transform: translateX(-10vw) scale(1.02);", styles);
        Assert.DoesNotContain("transform: translateX(-5vw);", styles);
        Assert.DoesNotContain("object-position: -160px bottom;", styles);
        Assert.Contains("width: 100%;", styles);
        Assert.Contains("margin-inline: 0;", styles);
        Assert.Contains(".machine-configurator", styles);
        Assert.Contains(".machine-variant-button", styles);
        Assert.Contains(".machine-variant-button.is-selected", styles);
        Assert.Contains(".machine-addon-option", styles);
        Assert.Contains(".machine-addon-option.is-selected", styles);
        Assert.Contains(".machine-config-summary", styles);
        Assert.Contains(".machine-actions {\n  display: flex;", styles);
        Assert.Contains("flex-wrap: wrap;", styles);
        Assert.Contains(".machine-actions .button {\n  min-height: 46px;", styles);
        Assert.Contains(".machine-actions .machine-configure-button {\n  flex: 0 0 auto;", styles);
        Assert.Contains("min-width: 132px;", styles);
        Assert.Contains(".machine-actions .machine-configure-button::after", styles);
        Assert.Contains(".machine-tooling-link", styles);
        Assert.Contains("border: 0;", styles);
        Assert.DoesNotContain(".machine-actions .button.secondary", styles);
        Assert.Matches(
            @"@media \(max-width: 680px\)[\s\S]*?\.machine-actions\s*\{[^}]*align-items:\s*stretch;[^}]*gap:\s*10px;[^}]*padding:\s*10px;",
            styles);
        Assert.DoesNotContain(".machine-actions .button {\n    width: 100%;", styles);
        Assert.Matches(
            @"@media \(max-width: 680px\)[\s\S]*?\.machine-tooling-link\s*\{[^}]*justify-content:\s*center;",
            styles);
        Assert.DoesNotContain(".machine-feature-point-button", styles);
        Assert.Contains(".machine-feature .h-display", styles);
        Assert.Contains(".machine-feature-title", styles);
        Assert.Contains("html:lang(th) .machine-feature-title-lead", styles);
        Assert.Contains("html:lang(th) .machine-feature-title--intro > span", styles);
        Assert.Contains("white-space: nowrap;", styles);
        Assert.Contains("word-break: keep-all;", styles);
        Assert.Contains(".machine-stat-grid", styles);
        Assert.Contains(".workflow-step:is(:hover, :focus-visible)", styles);
        Assert.Contains(".workflow-step.is-active", styles);
        Assert.Contains("font-size: clamp(1.35rem, 1.85vw, 1.72rem);", styles);
        Assert.Contains("grid-template-columns: repeat(2, minmax(0, 1fr));", styles);
        Assert.Contains(".machine-stat-grid div {\n    min-height: 80px;", styles);
        Assert.Contains("font-size: clamp(1.25rem, 6vw, 1.55rem);", styles);
        Assert.Contains(".social-link", styles);
    }

    /// <summary>
    /// Verifies the practical notes library has enough researched content and printable article support.
    /// </summary>
    [Fact]
    public void BlogPostsIncludeExpandedPracticalNotesAndEbookDownload()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Pages", "StaticPage.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");
        var controller = ReadRepoFile("Maliev.Web.Bff", "Controllers", "BlogController.cs");
        var pdfService = ReadRepoFile("Maliev.Web.Bff", "Services", "BlogEbookPdfService.cs");
        var bffProject = ReadRepoFile("Maliev.Web.Bff", "Maliev.Web.Bff.csproj");

        Assert.True(SiteContent.BlogPosts.Count >= 53);
        Assert.Equal(SiteContent.BlogPosts.Count, SiteContent.BlogPosts.Select(post => post.Slug).Distinct(StringComparer.Ordinal).Count());
        Assert.All(SiteContent.BlogPosts, post =>
        {
            Assert.False(string.IsNullOrWhiteSpace(post.ImageUrl));
            Assert.NotEmpty(post.Sections);
            Assert.NotEmpty(post.Takeaways);
            Assert.All(post.Sections, section =>
            {
                Assert.False(string.IsNullOrWhiteSpace(section.Title.En));
                Assert.False(string.IsNullOrWhiteSpace(section.Title.Th));
                Assert.False(string.IsNullOrWhiteSpace(section.Body.En));
                Assert.False(string.IsNullOrWhiteSpace(section.Body.Th));
            });
        });
        Assert.Contains("fdm-print-orientation", SiteContent.BlogPosts.Select(post => post.Slug));
        Assert.Contains("order-ready-checklist", SiteContent.BlogPosts.Select(post => post.Slug));
        Assert.Contains("Practical Notes", source);
        Assert.Contains("ShouldRenderPageHero", source);
        Assert.Contains("!string.Equals(Path, \"blog\", StringComparison.Ordinal)", source);
        Assert.Contains("@if (ShouldRenderPageHeroActions)", source);
        Assert.Contains("private bool ShouldRenderPageHeroActions => CurrentBlogPost is null;", source);
        Assert.Contains("private string PageHeroClass => CurrentBlogPost is null", source);
        Assert.Contains("page-hero compact blog-article-hero", source);
        Assert.Contains(".page-hero.blog-article-hero {\n  padding-bottom: clamp(36px, 4vw, 48px);\n  border-bottom: 0;", styles);
        Assert.Contains(".page-hero.compact.blog-article-hero + .article-detail-layout.blog-detail {\n  padding-top: clamp(22px, 2.8vw, 34px);", styles);
        Assert.Contains("<h1 id=\"blog-library-title\">@Text(\"Practical Notes\", \"บทความเชิงปฏิบัติ\")</h1>", source);
        Assert.DoesNotContain("blog-hero-title", source);
        Assert.DoesNotContain("Browse practical notes", source);
        Assert.Contains("Download PDF", source);
        Assert.Contains("ดาวน์โหลด PDF", source);
        Assert.Contains("blog-pdf-button", source);
        Assert.Contains("BuildBlogPdfHref", source);
        Assert.Contains("/web/v1/blog/{Uri.EscapeDataString(post.Slug)}/ebook.pdf?culture=", source);
        Assert.Contains("BuildBlogPdfFileName", source);
        Assert.DoesNotContain("DownloadBlogPdfAsync", source);
        Assert.DoesNotContain("InvokeVoidAsync(\"print\")", source);
        Assert.Contains("[Route(\"web/v{version:apiVersion}/blog\")]", controller);
        Assert.Contains("[HttpGet(\"{slug}/ebook.pdf\")]", controller);
        Assert.Contains("BlogEbookPdfService", controller);
        Assert.DoesNotContain("QuestPDF", bffProject);
        Assert.DoesNotContain("ZXing", bffProject);
        Assert.Contains("IPdfServiceClient", pdfService);
        Assert.Contains("RenderBlogPracticalNoteAsync", pdfService);
        Assert.Contains("blog-search-form", source);
        Assert.Contains("blog-category-filter", source);
        Assert.Contains("blog-category-filter-head", source);
        Assert.Contains("blog-category-clear", source);
        Assert.Contains("blog-category-chip-dot", source);
        Assert.Contains("name=\"category\"", source);
        Assert.Contains("AllBlogCategoryFilter = \"all\"", source);
        Assert.Contains("BlogCategoryFilters", source);
        Assert.Contains("BlogCategoryKeys", source);
        Assert.Contains("SelectedBlogCategoryFilters", source);
        Assert.Contains("NormalizeBlogCategoryKeys", source);
        Assert.Contains("BuildBlogCategoryHref", source);
        Assert.Contains("BlogCategoryFilterClass", source);
        Assert.Contains("BlogCategoryFilterIsActive", source);
        Assert.Contains("aria-pressed=\"@BlogCategoryFilterIsActive(filter).ToString().ToLowerInvariant()\"", source);
        Assert.Contains("BlogPostMatchesCategory", source);
        Assert.Contains("category={Uri.EscapeDataString(string.Join(\",\", normalizedCategoryKeys))}", source);
        Assert.Contains("categories.Any(category => BlogPostMatchesCategory(post, category))", source);
        Assert.Contains("new(\"cnc\", SiteContent.Text(\"CNC\", \"CNC\")", source);
        Assert.Contains("new(\"design\", SiteContent.Text(\"Design / DFM\", \"ออกแบบ / DFM\")", source);
        Assert.Contains("new(\"materials\", SiteContent.Text(\"Materials\", \"วัสดุ\")", source);
        Assert.Contains("PagedBlogPosts", source);
        Assert.Contains("BlogPageSize = 12", source);
        Assert.Contains("BuildBlogPageHref", source);
        Assert.Contains("ResolveBlogImageUrl", source);
        Assert.Contains(".blog-library-tools", styles);
        Assert.Contains(".blog-library-section {\n  display: grid;\n  gap: 24px;\n  padding-top: clamp(64px, 7vw, 96px);", styles);
        Assert.Contains(".blog-library-tools h1", styles);
        Assert.DoesNotContain(".blog-library-tools h2", styles);
        Assert.Contains(".blog-category-list", styles);
        Assert.Contains(".blog-category-chip.is-active", styles);
        Assert.Contains(".blog-category-filter-head", styles);
        Assert.Contains(".blog-category-clear", styles);
        Assert.Contains(".blog-category-chip-dot", styles);
        Assert.Matches(@"\.blog-category-chip\s*\{[^}]*border-radius:\s*999px;", styles);
        Assert.Contains("background: var(--blue);", styles);
        Assert.Contains("background: var(--blue-hover);", styles);
        Assert.Contains("gap: 10px;\n  align-items: center;", styles);
        Assert.Contains(".blog-card-body {\n  flex: 1;\n  grid-template-rows: auto auto minmax(0, 1fr) auto;", styles);
        Assert.Contains(".blog-card-body .card-link {\n  align-self: end;\n  justify-self: start;", styles);
        Assert.Contains(".pagination-link", styles);
        Assert.Contains("@media print", styles);
        Assert.Contains(".article-detail-layout.blog-detail", styles);
        Assert.Contains(".site-header,\n  .site-footer,\n  .customer-chatbot,\n  .page-hero,\n  .detail-sidebar,\n  .blog-pdf-button", styles);
    }

    /// <summary>
    /// Verifies the pneumatic injection machine configurator limits selection to machine variants and the compressor add-on.
    /// </summary>
    [Fact]
    public void HomeMachineConfiguratorLimitsMachineSelectionAndCompressorAddon()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Pages", "Home.razor");
        var machineStart = source.IndexOf("<section id=\"machine-feature-preview\"", StringComparison.Ordinal);
        var workflowStart = source.IndexOf("<section id=\"workflow-carousel\"", StringComparison.Ordinal);
        var homeWorkStart = source.IndexOf("<section class=\"section home-work-section\"", StringComparison.Ordinal);

        Assert.True(machineStart >= 0);
        Assert.True(workflowStart >= 0);
        Assert.True(homeWorkStart > machineStart);
        Assert.True(machineStart > workflowStart);

        var machineSection = source[machineStart..homeWorkStart];

        Assert.Contains("data-selected-variant=\"@SelectedMachineVariant.Key\"", machineSection);
        Assert.Contains("private string _selectedMachineVariantKey = \"50g\";", source);
        Assert.Contains("data-compressor-included=\"@MachineCompressorAriaPressed\"", machineSection);
        Assert.Contains("@key=\"SelectedMachineImageKey\"", machineSection);
        Assert.Contains("src=\"@SelectedMachineFeatureImageUrl\"", machineSection);
        Assert.Contains("src=\"@SelectedMachineDarkFeatureImageUrl\"", machineSection);
        Assert.Contains("alt=\"@SelectedMachineFeatureImageAlt.For(Preferences.Culture)\"", machineSection);
        Assert.Contains("@SelectedMachineVariant.Body.For(Preferences.Culture)", machineSection);
        Assert.Contains("@foreach (var stat in SelectedMachineVariant.Stats)", machineSection);
        Assert.Contains("@foreach (var variant in MachineVariants)", machineSection);
        Assert.Contains("MachineVariantButtonClass(variant)", machineSection);
        Assert.Contains("MachineVariantAriaChecked(variant)", machineSection);
        Assert.Contains("@onclick=\"() => SelectMachineVariant(variant)\"", machineSection);
        Assert.Contains("ToggleMachineCompressor", machineSection);
        Assert.Contains("25L air compressor", machineSection);
        Assert.Contains("SelectedMachineConfigurationHref", machineSection);
        Assert.Contains("SelectedMachinePackageLabel", machineSection);
        Assert.Contains("machine-configure-button", machineSection);
        Assert.Contains("@Text(\"Configure\", \"ตั้งค่า\")", machineSection);
        Assert.Contains("machine-tooling-link", machineSection);
        Assert.DoesNotContain("Configure selected machine", machineSection);
        Assert.DoesNotContain("class=\"button secondary\" href=\"/contact\"", machineSection);
        Assert.DoesNotContain("ProcessSteps", machineSection);
        Assert.DoesNotContain("_workflowCarousel", machineSection);
        Assert.DoesNotContain("SelectWorkflowStepAsync", machineSection);
        Assert.DoesNotContain("data-machine-feature", machineSection);
        Assert.DoesNotContain("MachineFeatureButtonClass", machineSection);

        var machineFeaturesStart = source.IndexOf("private readonly IReadOnlyList<MachineVariant> MachineVariants", StringComparison.Ordinal);
        var processStepsStart = source.IndexOf("private readonly IReadOnlyList<ProcessStep> ProcessSteps", StringComparison.Ordinal);

        Assert.True(machineFeaturesStart >= 0);
        Assert.True(processStepsStart > machineFeaturesStart);

        var machineFeatures = source[machineFeaturesStart..processStepsStart];

        Assert.Contains("30g machine", machineFeatures);
        Assert.Contains("50g machine", machineFeatures);
        Assert.Contains("Entry desktop trials", machineFeatures);
        Assert.Contains("Recommended shop choice", machineFeatures);
        Assert.Contains("larger shot volume", machineFeatures);
        Assert.Contains("350°C melt range", machineFeatures);
        Assert.DoesNotContain("Desktop trials and inserts", machineFeatures);
        Assert.DoesNotContain("More desktop shot volume", machineFeatures);
        Assert.DoesNotContain("30g trials", machineFeatures);
        Assert.DoesNotContain("50g small batches", machineFeatures);
        Assert.DoesNotContain("Tooling setup", machineFeatures);
        Assert.DoesNotContain("Run and tune", machineFeatures);
        Assert.DoesNotContain("MALIEV 30g and 50g pneumatic injection molding machine variants", machineFeatures);
        Assert.DoesNotContain("Upload your file", machineFeatures);
        Assert.DoesNotContain("Review DFM", machineFeatures);
        Assert.DoesNotContain("Adjust price", machineFeatures);
        Assert.DoesNotContain("Order and track", machineFeatures);
    }

    /// <summary>
    /// Verifies the pneumatic injection machine section uses the prepared image without an extra fade overlay.
    /// </summary>
    [Fact]
    public void HomeMachineFeatureUsesPreparedImageWithoutFadeOverlay()
    {
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");
        var machineStylesStart = styles.IndexOf(".machine-feature {", StringComparison.Ordinal);
        var processStylesStart = styles.IndexOf(".process-grid", machineStylesStart, StringComparison.Ordinal);

        Assert.True(machineStylesStart >= 0);
        Assert.True(processStylesStart > machineStylesStart);

        var machineStyles = styles[machineStylesStart..processStylesStart];

        Assert.Contains("--machine-feature-surface-rgb: 247 248 251;", styles);
        Assert.Contains("--machine-feature-surface-rgb: 13 16 21;", styles);
        Assert.Contains("--machine-feature-grid-rgb: 23 23 23;", styles);
        Assert.Contains("--machine-feature-grid-rgb: 244 246 248;", styles);
        Assert.Contains("--machine-feature-panel-rgb: 255 255 255;", styles);
        Assert.Contains("--machine-feature-panel-rgb: 20 25 32;", styles);
        Assert.Contains("--machine-feature-active-bg: var(--active-pill-bg);", styles);
        Assert.Contains("--machine-feature-active-text: var(--active-pill-text);", styles);
        Assert.Contains("background: rgb(var(--machine-feature-surface-rgb));", machineStyles);
        Assert.Contains("rgb(var(--machine-feature-grid-rgb) / .045)", machineStyles);
        Assert.DoesNotContain(".machine-feature::after", machineStyles);
        Assert.DoesNotContain("rgb(var(--machine-feature-surface-rgb) / .68)", styles);
        Assert.DoesNotContain("rgb(var(--machine-feature-surface-rgb) / .76)", styles);
        Assert.DoesNotContain("rgb(var(--machine-feature-surface-rgb) / .98)", styles);
        Assert.Contains("background: rgb(var(--machine-feature-panel-rgb) / .72);", machineStyles);
        Assert.Contains("background: rgb(var(--machine-feature-panel-rgb) / .74);", machineStyles);
        Assert.Contains("height: calc(100% + clamp(132px, 16svh, 180px));", machineStyles);
        Assert.Contains("bottom: clamp(-132px, -12svh, -96px);", machineStyles);
        Assert.Contains("width: 100%;", machineStyles);
        Assert.Contains("max-width: none;", machineStyles);
        Assert.Contains("object-fit: cover;", machineStyles);
        Assert.Contains(".machine-variant-button.is-selected", machineStyles);
        Assert.Contains(".machine-addon-option.is-selected", machineStyles);
        Assert.Contains("border-color: var(--blue);", machineStyles);
        Assert.Contains(".machine-feature-backdrop--dark", machineStyles);
        Assert.Contains("html[data-theme=\"dark\"] .machine-feature-backdrop--light", machineStyles);
        Assert.Contains("html[data-theme=\"dark\"] .machine-feature-backdrop--dark", machineStyles);
        Assert.DoesNotContain("background: #f7f8fb;", machineStyles);
        Assert.DoesNotContain("rgba(247, 248, 251", styles);
        Assert.DoesNotContain("rgba(var(--machine-feature", styles);
    }

    /// <summary>
    /// Verifies the annotated home-section eyebrow labels are removed from the landing page.
    /// </summary>
    [Fact]
    public void HomeRemovesAnnotatedSectionEyebrowLabels()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Pages", "Home.razor");

        Assert.DoesNotContain("<p class=\"eyebrow\">@Text(\"Manufacturing capabilities\", \"ความสามารถงานผลิต\")</p>", source);
        Assert.DoesNotContain("<p class=\"eyebrow\">@Text(\"Workshop-made equipment\", \"เครื่องจักรที่ผลิตในเวิร์กช็อป\")</p>", source);
        Assert.DoesNotContain("<p class=\"eyebrow\">@Text(\"How it works\", \"ขั้นตอน\")</p>", source);
        Assert.DoesNotContain("<p class=\"eyebrow\">@Text(\"Recent work\", \"ผลงานล่าสุด\")</p>", source);
        Assert.DoesNotContain("<p class=\"eyebrow\">@Text(\"Journal and updates\", \"บทความและอัปเดต\")</p>", source);
        Assert.DoesNotContain("<p class=\"eyebrow\">@Text(\"One file is enough to start\", \"เริ่มได้ด้วยไฟล์เดียว\")</p>", source);
    }

    /// <summary>
    /// Verifies no customer page renders the removed eyebrow label pattern.
    /// </summary>
    [Fact]
    public void CustomerPagesDoNotRenderEyebrowLabels()
    {
        var root = FindRepoRoot();
        var pagesRoot = Path.Combine(root, "Maliev.Web.Client", "Pages");
        var pageFiles = Directory.EnumerateFiles(pagesRoot, "*.razor", SearchOption.AllDirectories);

        foreach (var file in pageFiles)
        {
            var source = File.ReadAllText(file);
            Assert.DoesNotContain("class=\"eyebrow\"", source);
        }

        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.DoesNotContain(".eyebrow", styles);
        Assert.DoesNotContain(":not(.eyebrow)", styles);
    }

    /// <summary>
    /// Verifies the home hero uses the ad-targeted localized copy catalog instead of one fixed headline.
    /// </summary>
    [Fact]
    public void HomeHeroUsesAdTargetedLocalizedCopyRotation()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Pages", "Home.razor");
        var content = ReadRepoFile("Maliev.Web.Client", "Content", "HeroCopyCatalog.cs");

        Assert.Contains("@inject NavigationManager Navigation", source);
        Assert.Contains("HeroCopyCatalog.ResolveExplicitTargetKey(Navigation.Uri)", source);
        Assert.Contains("explicitTargetKey ?? HeroCopyCatalog.DefaultTargetKey", source);
        Assert.Contains("HeroCopyCatalog.SelectRandom", source);
        Assert.Contains("HeroCopyCatalog.GetVariant", source);
        Assert.Contains("home.hero.copy", source);
        Assert.Contains("@_heroCopy.HeadlineLead.For(Preferences.Culture)", source);
        Assert.Contains("@_heroCopy.HeadlineAccent.For(Preferences.Culture)", source);
        Assert.DoesNotContain("data-hero-typewriter", source);
        Assert.DoesNotContain("HeroTypewriterAccents", source);
        Assert.Contains("@_heroCopy.Body.For(Preferences.Culture)", source);
        Assert.Contains("@_heroCopy.MetaDescription.For(Preferences.Culture)", source);
        Assert.Contains("\"service\"", content);
        Assert.Contains("\"utm_term\"", content);
        Assert.Contains("\"keyword\"", content);
        Assert.Contains("\"fdm-3d-printing\"", content);
        Assert.Contains("\"aluminum-cnc-milling\"", content);
        Assert.DoesNotContain("Parts in days,", source);
        Assert.DoesNotContain("not quarters.", source);
        Assert.DoesNotContain("ชิ้นงานในไม่กี่วัน", source);
        Assert.DoesNotContain("ไม่ใช่หลายเดือน", source);
    }

    /// <summary>
    /// Verifies the footer manufacturing line has a localized fast five-second typewriter rotation.
    /// </summary>
    [Fact]
    public void FooterManufacturingLineUsesLocalizedTypewriterRotation()
    {
        var app = ReadRepoFile("Maliev.Web.Bff", "Components", "App.razor");
        var layout = ReadRepoFile("Maliev.Web.Client", "Layout", "MainLayout.razor");
        var script = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "maliev-typewriter.js");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("js/maliev-typewriter.js", app);
        Assert.Contains("data-maliev-typewriter", layout);
        Assert.Contains("data-typewriter-interval=\"5000\"", layout);
        Assert.Contains("ManufacturingFooterLines[0].For(Preferences.Culture)", layout);
        Assert.Contains("line.For(Preferences.Culture)", layout);
        Assert.Contains("[data-maliev-typewriter]", script);
        Assert.Contains("typewriterSignature", script);
        Assert.Contains("root._malievTypewriterToken.cancelled = true", script);
        Assert.Contains("root.dataset.typewriterInterval ?? \"5000\"", script);
        Assert.Contains("root.dataset.typewriterSpeed ?? \"26\"", script);
        Assert.Contains("root.dataset.typewriterEraseSpeed ?? \"16\"", script);
        Assert.Contains("target.textContent = \"\";", script);
        Assert.Contains("eraseText", script);
        Assert.Contains("prefers-reduced-motion: reduce", script);
        Assert.Contains("runReducedMotionRotation", script);
        Assert.Contains("void runReducedMotionRotation(target, uniqueOptions, cycleInterval, token);", script);
        Assert.Contains("new MutationObserver", script);
        Assert.Contains("scheduleInitialize(1000)", script);
        Assert.Contains(".footer-typewriter-cursor", styles);
        Assert.Contains("@keyframes footer-typewriter-cursor", styles);
        Assert.Contains("animation: footer-typewriter-cursor 0.85s steps(1, end) infinite;", styles);
        Assert.DoesNotContain(".hero-typewriter", styles);
    }

    /// <summary>
    /// Verifies the home services grid hides shuffled catalog numbers, rotates generic highlights, and pins explicit service intent.
    /// </summary>
    [Fact]
    public void HomeServicesRotateGenericHighlightAndPinExplicitServiceTargets()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Pages", "Home.razor");
        var content = ReadRepoFile("Maliev.Web.Client", "Content", "HeroCopyCatalog.cs");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("HeroCopyCatalog.ResolveExplicitTargetKey(Navigation.Uri)", source);
        Assert.Contains("_hasExplicitHeroTarget", source);
        Assert.Contains("_heroServiceSlug", source);
        Assert.Contains("_highlightServiceSlug", source);
        Assert.DoesNotContain("Text(\"Primary service\", \"บริการหลัก\")", source);
        Assert.Contains("TryTakeMatchingServicesOrder", source);
        Assert.Contains("new ServicesOrderState(_heroTargetKey, _hasExplicitHeroTarget", source);
        Assert.Contains("serviceOrder.HasExplicitHeroTarget == _hasExplicitHeroTarget", source);
        Assert.Contains("string.Equals(serviceOrder.TargetKey, _heroTargetKey, StringComparison.OrdinalIgnoreCase)", source);
        Assert.Contains("PromoteHighlightedService", source);
        Assert.Contains("SelectRotatingHighlightedService", source);
        Assert.Contains("StringComparison.OrdinalIgnoreCase", source);
        Assert.Contains("internal static string? ResolveExplicitTargetKey(string? url)", content);
        Assert.Contains("internal static string ResolveServiceSlug(string? targetKey)", content);
        Assert.Contains("grid-template-rows: auto minmax(0, 1fr) auto;", styles);
        Assert.Contains("grid-auto-rows: minmax(320px, 1fr);", styles);
        Assert.Contains(".service-card-media {\n  height: clamp(190px, 18vw, 236px);", styles);
        Assert.Contains(".service-card-media .chip {\n  position: absolute;", styles);
        Assert.DoesNotContain(".service-card.primary .service-card-media", styles);
        Assert.DoesNotContain("grid-row: span 2;", styles);
        Assert.DoesNotContain("<span class=\"card-meta\">@service.Number</span>", source);
        Assert.DoesNotContain("return service.Primary ? \"service-card primary\" : \"service-card\";", source);
        Assert.DoesNotContain("@if (service.Primary)", source);
    }

    /// <summary>
    /// Verifies the footer uses real company identity, contact, address, and social links.
    /// </summary>
    [Fact]
    public void FooterUsesLogoAndManufacturingContactLinks()
    {
        var root = FindRepoRoot();
        var source = ReadRepoFile("Maliev.Web.Client", "Layout", "MainLayout.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("footer-logo", source);
        Assert.Contains("/images/logo.svg", source);
        Assert.Contains("Rapid manufacturing, custom parts, workshop-made machines", source);
        Assert.Contains("36/1 Moo 3", source);
        Assert.Contains("Khlong Khoi", source);
        Assert.Contains("https://maps.app.goo.gl/DPefucxBN2FTnZQa6", source);
        Assert.DoesNotContain("https://www.google.com/maps/search/", source);
        Assert.Contains("info@maliev.com", source);
        Assert.Contains("page.line.me/maliev", source);
        Assert.Contains("line-contact-link", source);
        Assert.Contains("line-contact-icon", source);
        Assert.Contains("line-app-icon", source);
        Assert.Contains("/images/line-app-icon.png", source);
        Assert.True(File.Exists(Path.Combine(root, "Maliev.Web.Bff", "wwwroot", "images", "line-app-icon.png")));
        Assert.Contains("Official Account @@maliev", source);
        Assert.Contains("facebook.com/maliev.manufacturing", source);
        Assert.Contains("youtube.com/channel/UCCosquPSUed6UPlMcRCq0Ig", source);
        Assert.Contains("instagram.com/maliev.manufacturing", source);
        Assert.Contains("class=\"social-link social-link--facebook\"", source);
        Assert.Contains("class=\"social-link social-link--youtube\"", source);
        Assert.Contains("class=\"social-link social-link--instagram\"", source);
        Assert.Contains("<svg viewBox=\"0 0 24 24\"", source);
        Assert.Contains("aria-label=\"Facebook\"", source);
        Assert.Contains("aria-label=\"YouTube\"", source);
        Assert.Contains("aria-label=\"Instagram\"", source);
        Assert.Contains("href=\"/refund-policy\"", source);
        Assert.Contains("href=\"/warranty-policy\"", source);
        Assert.Contains("href=\"/cookie-policy\"", source);
        Assert.True(
            source.IndexOf("class=\"footer-legal\"", StringComparison.Ordinal) <
            source.IndexOf("href=\"/privacy\"", StringComparison.Ordinal));
        Assert.Contains("footer-legal-copy", source);
        Assert.Contains("footer-legal-links", source);
        Assert.Contains("aria-label=\"@Text(\"Legal links\", \"ลิงก์กฎหมาย\")\"", source);
        Assert.Contains("href=\"/terms\"", source);
        Assert.Contains("OpenCookieSettingsAsync", source);
        Assert.Contains("ManufacturingFooterLines", source);
        Assert.Contains("data-maliev-typewriter", source);
        Assert.Contains("footer-typewriter", source);
        Assert.Contains("footer-typewriter-text", source);
        Assert.Contains("footer-typewriter-cursor", source);
        Assert.DoesNotContain("RotatingManufacturingLine", source);
        Assert.DoesNotContain("Task.Delay(TimeSpan.FromSeconds(6)", source);
        Assert.Contains("Measure twice. Print once. Ship with confidence.", source);
        Assert.Contains("Manufacturing is momentum with evidence.", source);
        Assert.Contains("วัดให้ชัด พิมพ์ให้แม่น ส่งมอบอย่างมั่นใจ", source);
        var footerLinesStart = source.IndexOf("private static readonly LocalizedText[] ManufacturingFooterLines", StringComparison.Ordinal);
        var footerLinesEnd = source.IndexOf("\n    ];", footerLinesStart, StringComparison.Ordinal);
        Assert.True(footerLinesStart >= 0 && footerLinesEnd > footerLinesStart);
        Assert.True(System.Text.RegularExpressions.Regex.Matches(source[footerLinesStart..footerLinesEnd], "new\\(").Count >= 50);
        Assert.DoesNotContain(">Facebook</a>", source);
        Assert.DoesNotContain(">YouTube</a>", source);
        Assert.DoesNotContain(">Instagram</a>", source);
        Assert.DoesNotContain(">LINE @@maliev</a>", source);
        Assert.DoesNotContain("M20.2 4.4C18.1 2.7", source);
        Assert.DoesNotContain("line-contact-logo", source);
        Assert.DoesNotContain("line-contact-logo-bg", styles);
        Assert.DoesNotContain("line-contact-logo-bubble", styles);
        Assert.DoesNotContain("line-contact-logo-text", styles);
        Assert.Contains(".line-app-icon", styles);
        Assert.Contains(".social-link--facebook", styles);
        Assert.Contains(".social-link--youtube", styles);
        Assert.Contains(".social-link--instagram", styles);
        Assert.Contains("#1877f2", styles);
        Assert.Contains("#ff0000", styles);
        Assert.Contains("#833ab4", styles);
        Assert.Contains("FoundingYear = 2018", source);
        Assert.Contains("DateTime.Today.Year", source);
        Assert.DoesNotContain("All rights reserved", source);
        Assert.Contains("footer-legal", source);
        Assert.Contains(".footer-legal {\n  grid-column: 1 / -1;\n  display: flex;", styles);
        Assert.Contains(".footer-legal-links", styles);
        Assert.Contains(".footer-legal-copy span", styles);
        Assert.Contains("@media (max-width: 820px) {\n  .footer-legal {\n    align-items: flex-start;\n    flex-direction: column;", styles);
        Assert.Contains(".footer-typewriter {\n    display: block;\n    margin-top: 4px;", styles);
        Assert.Contains(".footer-legal-links {\n    justify-content: flex-start;", styles);
        Assert.DoesNotContain("<strong>MALIEV Co., Ltd.</strong>", source);
        Assert.DoesNotContain("Nonthaburi, Thailand. Manufacturing services, machines, and production support.", source);
    }

    /// <summary>
    /// Verifies the contact page exposes the business channels customers need before submitting a form.
    /// </summary>
    [Fact]
    public void ContactPageShowsBusinessChannelsMapAndHours()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Pages", "StaticPage.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");
        var salesSection = source[
            source.IndexOf("contact-business-section contact-business-section--sales", StringComparison.Ordinal)..source.IndexOf("contact-business-section contact-business-section--support", StringComparison.Ordinal)];
        var supportSection = source[
            source.IndexOf("contact-business-section contact-business-section--support", StringComparison.Ordinal)..source.IndexOf("<article class=\"contact-map-section\">", StringComparison.Ordinal)];
        var mapHeading = source[
            source.IndexOf("<article class=\"contact-map-section\">", StringComparison.Ordinal)..source.IndexOf("<iframe class=\"contact-map-preview\"", StringComparison.Ordinal)];
        var mapDialogHeader = source[
            source.IndexOf("<header class=\"contact-map-dialog-header\">", StringComparison.Ordinal)..source.IndexOf("<iframe class=\"contact-map-dialog-frame\"", StringComparison.Ordinal)];
        var addFriendLineButtonStyle = styles[
            styles.IndexOf(".line-contact-link.contact-line-add-friend {", StringComparison.Ordinal)..styles.IndexOf(".line-contact-link.contact-line-add-friend .line-contact-icon", StringComparison.Ordinal)];

        Assert.Contains("contact-business-panel", source);
        Assert.Contains("contact-business-section contact-business-section--sales", source);
        Assert.Contains("contact-business-section contact-business-section--support", source);
        Assert.Contains("contact-map-section", source);
        Assert.Contains("contact-map-preview", source);
        Assert.Contains("contact-map-title", source);
        Assert.Contains("contact-map-title-logo", source);
        Assert.Contains("src=\"/images/logo.svg\"", source);
        Assert.Contains("alt=\"MALIEV\"", source);
        Assert.DoesNotContain("Visit MALIEV", source);
        Assert.DoesNotContain("เข้ามาที่ MALIEV", source);
        Assert.Contains("contact-section-head", source);
        Assert.Contains("contact-info-rows", source);
        Assert.Contains("contact-section-actions", source);
        Assert.Contains("line-contact-link contact-line-add-friend", source);
        Assert.Contains("line-contact-icon", source);
        Assert.Contains("line-contact-copy", source);
        Assert.Contains("contact-social-icon-row footer-social", source);
        Assert.Contains("class=\"social-link social-link--facebook\"", source);
        Assert.Contains("class=\"social-link social-link--youtube\"", source);
        Assert.Contains("class=\"social-link social-link--instagram\"", source);
        Assert.Contains("Icons.Material.Filled.LocationOn", source);
        Assert.Contains("Icons.Material.Filled.Schedule", source);
        Assert.Contains("Icons.Material.Filled.AlternateEmail", source);
        Assert.Contains("Icons.Material.Filled.MarkEmailUnread", source);
        Assert.Contains("Icons.Material.Filled.BuildCircle", source);
        Assert.Contains("Icons.Material.Filled.AttachFile", source);
        Assert.Contains("Icons.Material.Filled.InsertDriveFile", source);
        Assert.Contains("36/1 Moo 3", source);
        Assert.Contains("คลองข่อย", source);
        Assert.Contains("mailto:info@maliev.com", source);
        Assert.Contains("mailto:support@maliev.com", source);
        Assert.Contains("support@maliev.com", source);
        Assert.Contains("tel:+66898950690", source);
        Assert.Contains("+66 89 895 0690", source);
        Assert.Contains("+66 81 803 0404", source);
        Assert.Contains("contact-phone-reveal", source);
        Assert.Contains("page.line.me/maliev", source);
        Assert.Contains("line-app-icon", source);
        Assert.Contains("/images/line-app-icon.png", source);
        Assert.DoesNotContain("line-contact-logo-bg", source);
        Assert.DoesNotContain("line-contact-logo-bubble", source);
        Assert.DoesNotContain("line-contact-logo-text", source);
        Assert.Contains("facebook.com/maliev.manufacturing", source);
        Assert.Contains("youtube.com/channel/UCCosquPSUed6UPlMcRCq0Ig", source);
        Assert.Contains("instagram.com/maliev.manufacturing", source);
        Assert.Contains("10:00-18:00", source);
        Assert.Contains("Order reference, product name, photos or video, symptoms, machine serial number", source);
        Assert.Contains("MalievMapUrl", source);
        Assert.Contains("MalievMapEmbedUrl", source);
        Assert.Contains("https://www.google.com/maps?q=MALIEV%20Co.%2C%20Ltd.", source);
        Assert.Contains("href=\"@MalievMapUrl\"", source);
        Assert.Contains("src=\"@MalievMapEmbedUrl\"", source);
        Assert.Contains("&output=embed", source);
        Assert.Contains("Open larger map", source);
        Assert.Contains("@onclick=\"OpenContactMapDialog\"", source);
        Assert.Contains("role=\"dialog\"", source);
        Assert.Contains("contact-map-dialog-backdrop", source);
        Assert.Contains("contact-map-dialog-frame", source);
        Assert.Contains("contact-map-dialog-copy", mapDialogHeader);
        Assert.Contains("contact-map-dialog-title", mapDialogHeader);
        Assert.Contains("contact-map-dialog-logo", mapDialogHeader);
        Assert.Contains("aria-label='@Text(\"MALIEV location\", \"ตำแหน่ง MALIEV\")'", mapDialogHeader);
        Assert.Contains("src=\"/images/logo.svg\"", mapDialogHeader);
        Assert.Contains("Open in Google Maps", source);
        Assert.Contains("เปิดใน Google Maps", source);
        Assert.Contains("CloseContactMapDialog", source);
        Assert.DoesNotContain("href=\"https://maps.app.goo.gl/DPefucxBN2FTnZQa6\"", source);
        Assert.DoesNotContain("contact-section-icon", salesSection);
        Assert.DoesNotContain("Icons.Material.Filled.RequestQuote", salesSection);
        Assert.DoesNotContain("contact-line-add-friend", supportSection);
        Assert.DoesNotContain("contact-section-icon", supportSection);
        Assert.DoesNotContain("Icons.Material.Filled.SupportAgent", supportSection);
        Assert.DoesNotContain("Message support on LINE", supportSection);
        Assert.DoesNotContain("คุยกับทีมสนับสนุนทาง LINE", supportSection);
        Assert.DoesNotContain("contact-section-icon", mapHeading);
        Assert.DoesNotContain("Icons.Material.Filled.Map", mapHeading);
        Assert.DoesNotContain("contact-section-icon", mapDialogHeader);
        Assert.DoesNotContain("Icons.Material.Filled.Map", mapDialogHeader);
        Assert.DoesNotContain("contact-direct-grid", source);
        Assert.DoesNotContain("contact-direct-card", source);
        Assert.DoesNotContain("contact-map-card", source);
        Assert.Contains("contact-rfq-notice", source);
        Assert.Contains("This form is not the RFQ workspace.", source);
        Assert.Contains("แบบฟอร์มนี้ไม่ใช่พื้นที่ขอใบเสนอราคา", source);
        Assert.Contains("Quote Engine", source);
        Assert.Contains("contact-upload-panel", source);
        Assert.Contains("<InputFile OnChange=\"HandleContactFilesAsync\"", source);
        Assert.Contains("accept=\".pdf,.png,.jpg,.jpeg,.webp,.txt,.zip,.stl,.step,.stp,.obj,.3mf\"", source);
        Assert.Contains("MaxContactFiles = 5", source);
        Assert.Contains("MaxContactFileBytes = 10 * 1024 * 1024", source);
        Assert.Contains("catch (InvalidOperationException)", source);
        Assert.Contains("_contact.Files = _contactFileUploads", source);
        Assert.Contains("new ContactAttachmentDto", source);
        Assert.Contains("Base64Content = Convert.ToBase64String", source);

        Assert.Contains(".contact-details", styles);
        Assert.Contains(".contact-business-panel", styles);
        Assert.Contains(".contact-business-section", styles);
        Assert.Contains(".contact-section-icon", styles);
        Assert.Contains(".contact-info-row", styles);
        Assert.Contains(".contact-phone-reveal", styles);
        Assert.Contains(".contact-phone-reveal-secondary", styles);
        Assert.Contains("left: -1px;", styles);
        Assert.Contains("right: -1px;", styles);
        Assert.Contains("bottom: calc(100% - 1px);", styles);
        Assert.Contains("font-size: inherit;", styles);
        Assert.Contains("border-radius: 0 0 12px 12px;", styles);
        Assert.Contains(".contact-phone-reveal:hover .contact-phone-reveal-secondary", styles);
        Assert.Contains(".contact-phone-reveal:focus-within .contact-phone-reveal-secondary", styles);
        Assert.DoesNotContain(".contact-phone-reveal:hover,\n.contact-phone-reveal:focus-visible {\n  min-height: 52px;", styles);
        Assert.Contains(".line-contact-link.contact-line-add-friend", styles);
        Assert.Contains(".line-contact-link", styles);
        Assert.Contains("border-radius: var(--radius);", addFriendLineButtonStyle);
        Assert.DoesNotContain("border-radius: 999px;", addFriendLineButtonStyle);
        Assert.Contains("--line-link-bg", styles);
        Assert.Contains("--line-link-shadow", styles);
        Assert.Contains(".contact-social-icon-row .social-link", styles);
        Assert.Contains(".contact-map-preview", styles);
        Assert.Contains(".contact-map-actions", styles);
        Assert.Contains(".contact-map-dialog-backdrop", styles);
        Assert.Contains(".contact-map-dialog", styles);
        Assert.Contains(".contact-map-dialog-copy", styles);
        Assert.Contains(".contact-map-dialog-title", styles);
        Assert.Contains(".contact-map-dialog-logo", styles);
        Assert.Contains(".contact-map-dialog-frame", styles);
        Assert.Contains(".contact-map-dialog-actions", styles);
        Assert.Contains(".contact-rfq-notice", styles);
        Assert.Contains(".contact-upload-panel", styles);
        Assert.Contains(".contact-file-list", styles);
        Assert.Contains(".social-link--facebook", styles);
        Assert.Contains(".social-link--youtube", styles);
        Assert.Contains(".social-link--instagram", styles);
        Assert.Contains("--social-bg", styles);
        Assert.Contains(".contact-map-title", styles);
        Assert.Contains(".contact-map-title-logo", styles);
        Assert.Matches(@"\.contact-map-title\s*\{[^}]*align-items:\s*center;", styles);
        Assert.Matches(@"\.contact-map-title-logo\s*\{[^}]*width:\s*4\.4em;", styles);
        Assert.Matches(@"\.contact-map-title-logo\s*\{[^}]*transform:\s*translateY\(-\.06em\);", styles);
        Assert.Matches(@"\.contact-map-dialog-title\s*\{[^}]*align-items:\s*center;", styles);
        Assert.Matches(@"\.contact-map-dialog-logo\s*\{[^}]*height:\s*\.92em;", styles);
        Assert.Contains("filter: var(--logo-filter);", styles);
        Assert.DoesNotContain("transform: translateY(.08em);", styles);
        Assert.Contains("font-size: clamp(1.22rem, 1.55vw, 1.52rem);", styles);
        Assert.Contains("font-size: .9rem;", styles);
        Assert.Contains("grid-template-columns: minmax(104px, .28fr) minmax(0, 1fr);", styles);
        Assert.Contains("font-size: .72rem;", styles);
        Assert.Contains(".contact-info-row dt .mud-icon-root", styles);
        Assert.Contains(".line-contact-link.contact-line-add-friend .line-contact-copy strong", styles);
        Assert.Matches(@"\.contact-section-head,\s*\.contact-map-heading\s*\{[^}]*grid-template-columns:\s*minmax\(0,\s*1fr\);", styles);
        Assert.DoesNotContain(".contact-direct-grid", styles);
        Assert.DoesNotContain(".contact-direct-card", styles);
    }

    /// <summary>
    /// Verifies shared page action rows keep visible spacing between adjacent buttons.
    /// </summary>
    [Fact]
    public void QuoteActionRowsKeepButtonSpacing()
    {
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains(".quote-actions", styles);
        Assert.Contains("gap: 10px;", styles);
        Assert.Contains("flex-wrap: wrap;", styles);
    }

    /// <summary>
    /// Verifies compact hero pages do not inherit oversized landing-section spacing before their content.
    /// </summary>
    [Fact]
    public void CompactHeroPagesUseTighterContentSpacing()
    {
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains(".page-hero.compact + .section", styles);
        Assert.Contains("padding-top: clamp(40px, 5vw, 60px);", styles);
        Assert.Contains("padding-top: 38px;", styles);
    }

    /// <summary>
    /// Verifies the quote page sends custom manufacturing work to the dedicated quote engine.
    /// </summary>
    [Fact]
    public void QuotePageRoutesToDedicatedQuoteEngine()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Pages", "Quote.razor");

        Assert.Contains("<ManufacturingGizmo />", source);
        Assert.Contains("SiteContent.QuoteDemoUrl", source);
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
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("MudIconButton", source);
        Assert.Contains("MudBadge", source);
        Assert.Contains("@if (HasCartItems)", source);
        Assert.Contains("private bool HasCartItems => Cart.Count > 0;", source);
        Assert.Contains("Icons.Material.Outlined.ShoppingCart", source);
        Assert.Contains("Class=\"nav-icon-button cart-icon-button\"", source);
        Assert.Contains("Icons.Material.Filled.AccountCircle", source);
        Assert.Contains("Href=\"/account\"", source);
        Assert.Contains("\"/quote/start?returnUrl=%2Fquotes%2Fnew\"", source);
        Assert.Contains("<NavLink href=\"/services\">@Text(\"Services\", \"บริการ\")</NavLink>", source);
        Assert.Contains("<NavLink href=\"/shop\">@Text(\"Shop\", \"ร้านค้า\")</NavLink>", source);
        Assert.Contains("<NavLink href=\"/contact\">@Text(\"Contact\", \"ติดต่อ\")</NavLink>", source);
        Assert.Contains("class=\"mobile-nav-primary\"", source);
        Assert.Contains("class=\"mobile-nav-actions\"", source);
        Assert.Contains("class=\"mobile-nav-quote\"", source);
        Assert.DoesNotContain("<NavLink href=\"/materials\" @onclick=\"CloseMobileNav\">", source);
        Assert.DoesNotContain("<NavLink href=\"/case-studies\" @onclick=\"CloseMobileNav\">", source);
        Assert.DoesNotContain("<NavLink href=\"/blog\" @onclick=\"CloseMobileNav\">", source);
        Assert.Contains(".nav-links a {\n  white-space: nowrap;", styles);
        Assert.Contains(".mobile-nav-primary", styles);
        Assert.Contains(".mobile-nav-actions", styles);
        Assert.Contains(".mobile-nav-quote", styles);
        Assert.Contains(".nav-icon-button.mud-button-root {\n  width: 38px;", styles);
        Assert.Contains("background: transparent;\n  border: 0;\n  border-radius: var(--radius);\n  box-shadow: none;", styles);
        Assert.Contains(".nav-icon-button.mud-button-root:hover,\n.nav-icon-button.mud-button-root:focus-visible {\n  color: var(--ink);\n  background: transparent;\n  box-shadow: none;", styles);
        Assert.DoesNotContain(".cart-icon-button.mud-button-root {\n  background: transparent;", styles);
        Assert.Contains("--cta-bg: var(--blue);", styles);
        Assert.Contains("--cta-hover: var(--blue-hover);", styles);
        Assert.Contains("--cta-text: #ffffff;", styles);
        Assert.Contains(".nav-quote-link {\n  border-radius: 999px;", styles);
        Assert.Contains("@media (min-width: 961px) and (max-width: 1060px)", styles);
        Assert.DoesNotContain("class=\"icon-link\"", source);
    }

    /// <summary>
    /// Verifies the public header lets the landing hero backdrop bleed through only while the page is at the top.
    /// </summary>
    [Fact]
    public void HeaderBleedsLandingHeroBackdropAtPageTop()
    {
        var layout = ReadRepoFile("Maliev.Web.Client", "Layout", "MainLayout.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");
        var script = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "maliev-scroll.js");

        Assert.Contains("@inject IJSRuntime JS", layout);
        Assert.Contains("malievScroll.bindHeroHeaderBleed", layout);
        Assert.Contains(".site-header.site-header--hero-bleed", styles);
        Assert.Contains("background: var(--landing-gizmo-canvas-bg);", styles);
        Assert.Contains("box-shadow: none;", styles);
        Assert.Contains("bindHeroHeaderBleed: function ()", script);
        Assert.Contains("document.querySelector('.landing-hero')", script);
        Assert.Contains("site-header--hero-bleed", script);
        Assert.Contains("window.scrollY <= 24", script);
        Assert.Contains("MutationObserver", script);
        Assert.Contains("requestAnimationFrame", script);
        Assert.Contains("document.readyState === 'loading'", script);
        Assert.Contains("window.malievScroll.bindHeroHeaderBleed()", script);
    }

    /// <summary>
    /// Verifies the public layout includes the service-bounded customer chatbot widget.
    /// </summary>
    [Fact]
    public void PublicLayoutIncludesCustomerChatbotWidget()
    {
        var root = FindRepoRoot();
        var layout = ReadRepoFile("Maliev.Web.Client", "Layout", "MainLayout.razor");
        var component = ReadRepoFile("Maliev.Web.Client", "Components", "CustomerChatbot.razor");
        var authComplete = ReadRepoFile("Maliev.Web.Client", "Pages", "AuthChatbotComplete.razor");
        var app = ReadRepoFile("Maliev.Web.Bff", "Components", "App.razor");
        var bffProject = ReadRepoFile("Maliev.Web.Bff", "Maliev.Web.Bff.csproj");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");
        var script = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "maliev-chatbot.js");

        Assert.Contains("<CustomerChatbot />", layout);
        Assert.Contains("MALIEV manufacturing assistant", component);
        Assert.Contains("Materials, quotes, orders, and delivery", component);
        Assert.Contains("Customer manufacturing assistant", component);
        Assert.Contains("EnsureChatSessionStartedAsync", component);
        Assert.Contains("StartChatbotSessionForBrowserAsync", component);
        Assert.Contains("Connecting you to one of our agents", component);
        Assert.Contains("Mali is checking your message", component);
        Assert.Contains("Still waiting for Mali's reply", component);
        Assert.Contains("customer-chatbot-pending-copy", component);
        Assert.Contains("customer-chatbot-typing-dots", component);
        Assert.Contains("customer-chatbot-typing-dot", component);
        Assert.Contains("role=\"status\"", component);
        Assert.Contains("aria-live=\"polite\"", component);
        Assert.Contains("AssistantWaitingDelay", component);
        Assert.Contains("AssistantRequestTimeout", component);
        Assert.Contains("BeginPendingState", component);
        Assert.Contains("ShowLongWaitingStateAsync", component);
        Assert.Contains("CancelPendingDelay", component);
        Assert.Contains("malievChatbot.postJson", component);
        Assert.Contains("StartChatbotSessionAsync", ReadRepoFile("Maliev.Web.Client", "Services", "MalievApiClient.cs"));
        Assert.Contains("postJson: async function (path, payload, timeoutMs)", script);
        Assert.Contains("method: 'POST'", script);
        Assert.Contains("credentials: 'include'", script);
        Assert.Contains("AbortController", script);
        Assert.Contains("status: timedOut ? 408 : 0", script);
        Assert.Contains("CanInteract", component);
        Assert.Contains("The MALIEV assistant could not open a verified session yet.", component);
        Assert.DoesNotContain("How can I help with your MALIEV project today?", component);
        Assert.DoesNotContain("CreateGreeting", component);
        Assert.DoesNotContain("ResetDisplayedConversation(greetingName", component);
        Assert.DoesNotContain("if (!_isOpen && _unreadCount > 0)", component);
        Assert.DoesNotContain("CreateUnreadPopoutMessage", component);
        Assert.DoesNotContain("_sessionId = _memory.LastSessionId", component);
        Assert.DoesNotContain("ReadSharedSessionIdAsync", component);
        Assert.Contains("The assistant replied. Open the chat to continue.", component);
        Assert.DoesNotContain("CreatePopoutMessage", component);
        Assert.DoesNotContain("Ask about materials, pricing, lead time, or the right manufacturing process.", component);
        Assert.Contains("IsGeneratedGreeting", component);
        Assert.Contains("Where(message => !IsGeneratedGreeting(message))", component);
        Assert.DoesNotContain("OnPreferencesChanged", component);
        Assert.DoesNotContain("Ask me about MALIEV materials, 3D printing, CNC, scanning, molding, quotes, orders, or delivery.", component);
        Assert.Contains("Icons.Material.Filled.SupportAgent", component);
        Assert.Contains("customer-chatbot-profile", component);
        Assert.Contains("customer-chatbot-gemini-icon", component);
        Assert.Contains("/images/gemini-icon.svg", component);
        Assert.DoesNotContain("Icons.Material.Filled.AutoAwesome", component);
        Assert.True(File.Exists(Path.Combine(root, "Maliev.Web.Bff", "wwwroot", "images", "gemini-icon.svg")));
        Assert.Contains("customer-chatbot-title", component);
        Assert.Contains("customer-chatbot-header-actions", component);
        Assert.Contains("customer-chatbot-reset", component);
        Assert.Contains("ResetChatAsync", component);
        Assert.Contains("Icons.Material.Filled.RestartAlt", component);
        Assert.Contains("customer-chatbot-messages-wrap", component);
        Assert.Contains("@ref=\"_messagesContainer\"", component);
        Assert.Contains("customer-chatbot-jump-latest", component);
        Assert.Contains("Icons.Material.Filled.KeyboardArrowDown", component);
        Assert.Contains("aria-label=\"@Text(\"Open customer manufacturing assistant\"", component);
        Assert.DoesNotContain("<span>@Text(\"Mali\", \"น้องมะลิ\")</span>", component);
        Assert.DoesNotContain("<strong>@Text(\"Mali\", \"น้องมะลิ\")</strong>", component);
        Assert.DoesNotContain("Ask Mali about materials or quotes", component);
        Assert.DoesNotContain("Mali replied. Open the chat to continue.", component);
        Assert.DoesNotContain("unread Mali messages", component);
        Assert.Contains("maliev.chatbot.personalization.v1", component);
        Assert.Contains("localStorage.getItem", component);
        Assert.Contains("localStorage.setItem", component);
        Assert.Contains("localStorage.removeItem", component);
        Assert.Contains("maliev.customerAssistant.session.v1", component);
        Assert.Contains("IsAccountSpecificIntent", component);
        Assert.Contains("BeginChatSignInAsync", component);
        Assert.Contains("ChatActionSignInGoogle", component);
        Assert.Contains("ChatActionSignInEmail", component);
        Assert.Contains("CreateSignInActions", component);
        Assert.Contains("AddSignInChoiceMessageAsync", component);
        Assert.Contains("Continue with Google", component);
        Assert.Contains("Use email and password", component);
        Assert.Contains("customer-chatbot-auth-google", component);
        Assert.Contains("customer-chatbot-auth-email", component);
        Assert.Contains("/auth/google?returnUrl", component);
        Assert.Contains("RefreshIdentityAsync", component);
        Assert.Contains("Sign in to continue", component);
        Assert.Contains("/auth/chatbot-complete", component);
        Assert.Contains("CustomerContext = BuildCustomerContext()", component);
        Assert.Contains("AddContextPart(parts, \"Authentication\"", component);
        Assert.Contains("Assistant session", component);
        Assert.Contains("GetSignedInProfileAsync", component);
        Assert.Contains("GetSignedInAddressesAsync", component);
        Assert.Contains("GetSignedInOrdersAsync", component);
        Assert.Contains("CreateAccountActions", component);
        Assert.Contains("ChatActionViewServices", component);
        Assert.Contains("ChatActionContact", component);
        Assert.Contains("ChatActionRequestQuote", component);
        Assert.Contains("ResolveResponseActionHref", component);
        Assert.Contains("ResolveServicesHref", component);
        Assert.Contains("ChatActionViewServices => ResolveServicesHref(action.Data)", component);
        Assert.Contains("ChatActionContact => \"/contact\"", component);
        Assert.Contains("return \"/services\";", component);
        Assert.Contains("IsInternalHref(href) ? null : \"_blank\"", component);
        Assert.Contains("rel=\"@action.Rel\"", component);
        Assert.Contains("customer-chatbot-actions", component);
        Assert.Contains("customer-chatbot-action", component);
        Assert.DoesNotContain("customer-chatbot-prompts", component);
        Assert.DoesNotContain("SuggestedPrompts", component);
        Assert.DoesNotContain("SendSuggestionAsync", component);
        Assert.DoesNotContain("CustomerChatPrompt", component);
        Assert.Contains("customer-chatbot-popout", component);
        Assert.Contains("customer-chatbot-unread-badge", component);
        Assert.Contains("rows=\"1\"", component);
        Assert.Contains("@ref=\"_sendButton\"", component);
        Assert.Contains("OnDraftInputAsync", component);
        Assert.Contains("malievChatbot.initComposerKeys", component);
        Assert.Contains("malievChatbot.fitComposer", component);
        Assert.Contains("malievChatbot.isNearBottom", component);
        Assert.Contains("malievChatbot.scrollToBottom", component);
        Assert.Contains("malievChatbot.initFooterAwareFloat", component);
        Assert.Contains("QueueMessageScroll(true)", component);
        Assert.Contains("JumpToLatestAsync", component);
        Assert.Contains("HtmlSanitizer", component);
        Assert.Contains("Markdown.ToHtml", component);
        Assert.Contains("<PackageReference Include=\"HtmlSanitizer\"", bffProject);
        Assert.Contains("<PackageReference Include=\"Markdig\"", bffProject);
        Assert.Contains("MessageHtmlSanitizer.Sanitize(html)", component);
        Assert.Contains("RenderMessageContent(message.Content)", component);
        Assert.Contains("new MarkupString", component);
        Assert.Contains("DangerousHtmlBlockPattern.Replace", component);
        Assert.Contains("DangerousHtmlTagPattern.Replace", component);
        Assert.Contains("AllowedTags.Clear()", component);
        Assert.Contains("AllowedAttributes.Add(\"href\")", component);
        Assert.Contains("AllowedSchemes.Add(\"https\")", component);
        Assert.DoesNotContain("<p>@message.Content</p>", component);
        Assert.Contains("js/maliev-chatbot.js", app);
        Assert.Contains("lineHeight * 5", script);
        Assert.Contains("textarea.style.overflowY", script);
        Assert.DoesNotContain("Ask MALIEV", component);
        Assert.Contains(".customer-chatbot", styles);
        Assert.Contains("bottom: max(24px, env(safe-area-inset-bottom));", styles);
        Assert.Contains("transform: translateY(calc(-1 * var(--customer-chatbot-footer-lift, 0px)));", styles);
        Assert.Contains("transition: transform .22s ease-in-out;", styles);
        Assert.Contains(".customer-chatbot-panel", styles);
        Assert.Contains(".customer-chatbot-popout", styles);
        Assert.Contains(".customer-chatbot-profile", styles);
        Assert.Contains(".customer-chatbot-header-actions", styles);
        Assert.Contains(".customer-chatbot-reset", styles);
        Assert.Contains(".customer-chatbot-gemini-icon", styles);
        Assert.Contains(".customer-chatbot-gemini-icon img", styles);
        Assert.DoesNotContain(".customer-chatbot-gemini-icon .mud-icon-root", styles);
        Assert.DoesNotContain("border: 1px solid color-mix(in srgb, #1a73e8", styles);
        Assert.Contains("background: transparent;", styles);
        Assert.Contains(".customer-chatbot-title", styles);
        Assert.Contains(".customer-chatbot-messages-wrap", styles);
        Assert.Contains(".customer-chatbot-jump-latest", styles);
        Assert.Contains(".customer-chatbot-jump-badge", styles);
        Assert.Contains(".customer-chatbot-unread-badge", styles);
        Assert.Contains(".customer-chatbot-message-rich", styles);
        Assert.Contains(".customer-chatbot-actions", styles);
        Assert.Contains(".customer-chatbot-actions:has(.customer-chatbot-auth-google)", styles);
        Assert.Contains(".customer-chatbot-auth-email", styles);
        Assert.Contains(".customer-chatbot-action.primary", styles);
        Assert.Contains(".customer-chatbot-action {\n  min-height: 34px;\n  display: inline-flex;\n  align-items: center;\n  gap: 6px;\n  padding: 8px 10px;\n  color: var(--blue-ink);\n  background: var(--blue-soft);\n  border: 0;", styles);
        Assert.DoesNotContain(".customer-chatbot-prompts", styles);
        Assert.Contains(".customer-chatbot-message-rich a", styles);
        Assert.Contains(".customer-chatbot-message-rich :where(ul, ol)", styles);
        Assert.Contains(".customer-chatbot-message-rich pre", styles);
        Assert.Contains(".customer-chatbot-message-rich table", styles);
        Assert.Contains(".customer-chatbot-message.pending-with-copy", styles);
        Assert.Contains(".customer-chatbot-pending-copy", styles);
        Assert.Contains(".customer-chatbot-typing-dots", styles);
        Assert.Contains(".customer-chatbot-typing-dot", styles);
        Assert.DoesNotContain(".customer-chatbot-message.pending span {", styles);
        Assert.Contains(".customer-chatbot-composer {\n  display: grid;\n  grid-template-columns: minmax(0, 1fr) 44px;\n  align-items: end;", styles);
        Assert.Contains(".customer-chatbot-composer textarea {\n  height: 42px;", styles);
        Assert.Contains(".customer-chatbot-composer button {\n  width: 44px;\n  height: 42px;", styles);
        Assert.DoesNotContain("bottom: clamp(84px, 8vh, 108px);", styles);
        Assert.Contains(".customer-chatbot-toggle {\n  width: 50px;\n  min-height: 50px;\n  padding: 0;\n  border-radius: 9999px;", styles);
        Assert.Contains("opacity: .78;", styles);
        Assert.Contains("transition: opacity .18s ease-in-out, box-shadow .18s ease-in-out, background-color .18s ease-in-out;", styles);
        Assert.Contains(".customer-chatbot-toggle .mud-icon-root", styles);
        Assert.Contains(".customer-chatbot-toggle {\n    width: 46px;\n    min-height: 46px;", styles);
        Assert.Contains("bottom: max(16px, env(safe-area-inset-bottom));", styles);
        Assert.Contains("initFooterAwareFloat: function ()", script);
        Assert.Contains("document.querySelectorAll('.footer-legal-links a, .footer-legal-links button')", script);
        Assert.Contains("const horizontalOverlap = naturalRect.left - clearance < targetRect.right", script);
        Assert.Contains("const verticalOverlap = naturalRect.top - clearance < targetRect.bottom", script);
        Assert.Contains("const currentLift = getCurrentLift();", script);
        Assert.Contains("top: toggleRect.top + currentLift", script);
        Assert.DoesNotContain("footerLegal.getBoundingClientRect()", script);
        Assert.Contains("--customer-chatbot-footer-lift", script);
        Assert.Contains("isNearBottom: function (container)", script);
        Assert.Contains("scrollToBottom: function (container, smooth)", script);
        Assert.Contains("container.scrollTo({", script);
        Assert.Contains("getJson: async function (path)", script);
        Assert.Contains("openSignInPopup: function (url)", script);
        Assert.Contains("notifyAuthenticationComplete: function ()", script);
        Assert.Contains("readSharedSessionId: function (storageKey)", script);
        Assert.Contains("writeSharedSession: function (storageKey, sessionId, userKey, language, isAuthenticated)", script);
        Assert.Contains("maliev_customer_assistant_session", script);
        Assert.Contains("clearSharedSession: function (storageKey)", script);
        Assert.Contains("initComposerKeys: function (textarea, sendButton)", script);
        Assert.Contains("event.key !== 'Enter'", script);
        Assert.Contains("event.shiftKey", script);
        Assert.Contains("sendButton.click()", script);
        Assert.Contains("@page \"/auth/chatbot-complete\"", authComplete);
        Assert.Contains("malievChatbot.notifyAuthenticationComplete", authComplete);
    }

    /// <summary>
    /// Verifies the expanded customer chatbot waits before softening, then returns to full opacity for mouse and keyboard use.
    /// </summary>
    [Fact]
    public void CustomerChatbotPanelFadesUntilHoveredOrFocused()
    {
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("--customer-chatbot-inactive-opacity: .62;", styles);
        Assert.Contains("--customer-chatbot-inactive-delay: 12s;", styles);
        Assert.Contains(".customer-chatbot-panel,\n.customer-chatbot-popout {\n  opacity: 1;", styles);
        Assert.Contains("animation: customer-chatbot-idle-fade .48s ease-in-out var(--customer-chatbot-inactive-delay) both;", styles);
        Assert.Contains("transition: opacity .32s ease-in-out, box-shadow .22s ease-in-out, border-color .22s ease-in-out;", styles);
        Assert.Contains(".customer-chatbot-panel:is(:hover, :focus-within),\n.customer-chatbot-popout:is(:hover, :focus-visible, :focus-within) {\n  opacity: 1;", styles);
        Assert.Contains("animation: none;", styles);
        Assert.Contains("@keyframes customer-chatbot-idle-fade", styles);
        Assert.Contains("to {\n    opacity: var(--customer-chatbot-inactive-opacity);", styles);
    }

    /// <summary>
    /// Verifies the customer chatbot can surface contextual page hints without invoking the assistant service.
    /// </summary>
    [Fact]
    public void CustomerChatbotUsesAnalyticsReadyContextualHints()
    {
        var component = ReadRepoFile("Maliev.Web.Client", "Components", "CustomerChatbot.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("@inject NavigationManager Navigation", component);
        Assert.Contains("Navigation.LocationChanged += OnLocationChanged;", component);
        Assert.Contains("ContextHintShowProbability = 0.30;", component);
        Assert.Contains("ContextHintDwellDelay = TimeSpan.FromSeconds(8);", component);
        Assert.Contains("ResolveContextHint(Navigation.Uri)", component);
        Assert.Contains("ShouldSurfaceContextHint(hint, Navigation.Uri)", component);
        Assert.Contains("HashCode.Combine(uri.PathAndQuery, hint.AnalyticsKey)", component);
        Assert.Contains("AddContextPart(parts, \"Page context\", _activeContextHint?.Context);", component);
        Assert.Contains("AddDistinct(_memory.ServiceInterests, hint.ServiceInterest", component);
        Assert.Contains("Are you looking at any specific FDM material?", component);
        Assert.Contains("Any questions about the 3D scanning service you're looking at?", component);
        Assert.Contains("data-chatbot-popout-kind=\"@_popoutKind\"", component);
        Assert.Contains("hint.OpeningMessage", component);
        Assert.Contains(".customer-chatbot-popout[data-chatbot-popout-kind=\"context\"]", styles);
    }

    /// <summary>
    /// Verifies the customer chatbot uses first-party behavior signals to choose assistive popouts and assistant context.
    /// </summary>
    [Fact]
    public void CustomerChatbotTracksLocalBehaviorForAssistivePopouts()
    {
        var component = ReadRepoFile("Maliev.Web.Client", "Components", "CustomerChatbot.razor");
        var script = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "maliev-chatbot.js");

        Assert.Contains("malievChatbot.initBehaviorTracker", component);
        Assert.Contains("malievChatbot.refreshBehaviorTracker", component);
        Assert.Contains("ReadBehaviorSnapshotAsync", component);
        Assert.Contains("ResolveBehaviorHint", component);
        Assert.Contains("BuildBehaviorContext", component);
        Assert.Contains("Recent page behavior", component);
        Assert.Contains("quote upload intent", component);
        Assert.Contains("material comparison intent", component);
        Assert.Contains("CustomerChatbotBehaviorSnapshot", component);
        Assert.Contains("CustomerChatbotBehaviorEvent", component);
        Assert.Contains("assistant_popout_opened", component);
        Assert.Contains("recordBehaviorEvent", component);

        Assert.Contains("initBehaviorTracker: function ()", script);
        Assert.Contains("refreshBehaviorTracker: function ()", script);
        Assert.Contains("readBehaviorSnapshot: function ()", script);
        Assert.Contains("recordBehaviorEvent: function (kind, label, detail)", script);
        Assert.Contains("maliev.chatbot.behavior.v1", script);
        Assert.Contains("sessionStorage", script);
        Assert.Contains("IntersectionObserver", script);
        Assert.Contains("document.addEventListener('click'", script);
        Assert.Contains("landing-quote-dropzone", script);
        Assert.Contains("material-filter-row", script);
        Assert.Contains("shop-search", script);
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
        var design = ReadRepoFile("DESIGN.md");
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
        Assert.Contains("family=Albert+Sans:wght@400;500;600;700", app);
        Assert.Contains("family=JetBrains+Mono:wght@400;500;600;700", app);
        Assert.Contains("family=Noto+Sans+Thai:wght@400;500;600;700", app);
        Assert.Contains("family=Geist:wght@400;500;600;700", app);
        Assert.Contains("--font-sans-en", styles);
        Assert.Contains("\"Albert Sans\"", styles);
        Assert.Contains("\"Noto Sans Thai\"", styles);
        Assert.Contains("--font-sans-th: \"Noto Sans Thai\"", styles);
        Assert.Contains("--font-mono: \"JetBrains Mono\", \"Noto Sans Thai\"", styles);
        Assert.Contains("--font-mono: var(--font-sans-th)", styles);
        Assert.Contains("The default English typography is Inter.", design);
        Assert.Contains("JetBrains Mono completes the system", design);
        Assert.Contains("Noto Sans Thai preserved as the Thai fallback", design);
        Assert.DoesNotContain("Geist", design);
        Assert.Contains("html:lang(th)", styles);
        Assert.Contains("hyphens: none;", styles);
        Assert.Contains("line-break: strict;", styles);
        Assert.Contains("overflow-wrap: normal;", styles);
        Assert.Contains("word-break: keep-all;", styles);
        Assert.Contains("html[data-theme=\"dark\"]", styles);
        Assert.Contains("--logo-filter: brightness(0) invert(1)", styles);
        Assert.Contains("--blue-on: #ffffff;", styles);
        Assert.Contains("--cta-text: #ffffff;", styles);
        Assert.Contains("--reconnect-button-text: var(--blue-on);", styles);
        Assert.Contains("--reconnect-button-focus: rgba(10, 114, 239, .24);", styles);
        Assert.Contains("--reconnect-button-focus: rgba(10, 114, 239, .32);", styles);
        Assert.Contains("document.documentElement.lang", cultureScript);
        Assert.Contains("document.documentElement.dataset.theme", cultureScript);
        Assert.DoesNotContain("isThaiRegionSignal", cultureScript);
        Assert.DoesNotContain("Asia/Bangkok", cultureScript);
        Assert.Contains("return window.malievCulture.applyDocumentCulture(fallback || 'en-US');", cultureScript);
    }

    /// <summary>
    /// Verifies the public web app uses server-side Blazor interactivity.
    /// </summary>
    [Fact]
    public void BffUsesInteractiveServerRenderMode()
    {
        var program = ReadRepoFile("Maliev.Web.Bff", "Program.cs");
        var app = ReadRepoFile("Maliev.Web.Bff", "Components", "App.razor");
        var reconnectModal = ReadRepoFile("Maliev.Web.Bff", "Components", "Layout", "ReconnectModal.razor");
        var clientProject = ReadRepoFile("Maliev.Web.Client", "Maliev.Web.Client.csproj");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");
        var reconnectScript = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "maliev-reconnect.js");

        Assert.Contains("AddInteractiveServerComponents", program);
        Assert.Contains("AddInteractiveServerRenderMode", program);
        Assert.Contains("UseStaticWebAssets", program);
        Assert.Contains("app.UseStaticFiles();", program);
        Assert.Contains("@rendermode=\"InteractiveServer\"", app);
        Assert.Contains("<ReconnectModal />", app);
        Assert.Contains("js/maliev-reconnect.js", app);
        Assert.DoesNotContain("AddInteractiveWebAssemblyComponents", program);
        Assert.DoesNotContain("AddInteractiveWebAssemblyRenderMode", program);
        Assert.DoesNotContain("Microsoft.NET.Sdk.BlazorWebAssembly", clientProject);
        Assert.DoesNotContain("Microsoft.AspNetCore.Components.WebAssembly", clientProject);
        Assert.Contains("id=\"components-reconnect-modal\"", reconnectModal);
        Assert.Contains("components-reconnect-hide maliev-reconnect-modal", reconnectModal);
        Assert.Contains("class=\"maliev-reconnect-logo\" src=\"/images/logo.svg\"", reconnectModal);
        Assert.Contains("data-reconnect-elapsed", reconnectModal);
        Assert.Contains("Reconnecting to your workspace", reconnectModal);
        Assert.Contains("Still trying to reconnect", reconnectModal);
        Assert.Contains("Refresh to continue", reconnectModal);
        Assert.Contains("maliev-reconnect-actions", reconnectModal);
        Assert.Contains("Blazor.reconnect()", reconnectModal);
        Assert.Contains("#components-reconnect-modal.components-reconnect-show", styles);
        Assert.Contains(".maliev-reconnect-panel--show", styles);
        Assert.Contains(".maliev-reconnect-panel--failed", styles);
        Assert.Contains("display: grid;\n  align-content: start;\n  grid-auto-rows: max-content;\n  gap: 8px;", styles);
        Assert.Contains(".maliev-reconnect-actions", styles);
        Assert.Contains(".maliev-reconnect-logo", styles);
        Assert.Contains(".maliev-reconnect-timer", styles);
        Assert.Contains(".maliev-reconnect-panel--rejected .maliev-reconnect-actions", styles);
        Assert.Contains("grid-template-columns: minmax(0, 1fr) auto;", styles);
        Assert.Contains("gap: 8px;\n  align-items: center;\n  justify-content: space-between;\n  margin-top: 2px;\n  padding-top: 4px;", styles);
        Assert.Contains("#components-reconnect-modal .maliev-reconnect-panel .maliev-reconnect-actions button", styles);
        Assert.Contains("min-height: 30px;\n  min-width: auto;\n  margin: 0 !important;\n  padding: 6px 14px !important;", styles);
        Assert.Contains("color: var(--reconnect-button-text) !important;\n  background: var(--reconnect-button-bg);", styles);
        Assert.Contains("text-transform: none !important;", styles);
        Assert.Contains("#components-reconnect-modal .maliev-reconnect-panel .maliev-reconnect-actions button:is(:hover, :focus-visible)", styles);
        Assert.Contains("background: var(--reconnect-button-hover);", styles);
        Assert.Contains("#components-reconnect-modal .maliev-reconnect-panel .maliev-reconnect-actions button:focus-visible", styles);
        Assert.Contains("0 0 0 3px var(--reconnect-button-focus)", styles);
        Assert.Contains(".maliev-reconnect-progress::after", styles);
        Assert.Contains("@keyframes reconnect-progress", styles);
        Assert.Contains("components-reconnect-retrying", reconnectScript);
        Assert.Contains("data-reconnect-elapsed", reconnectScript);
        Assert.Contains("formatElapsed", reconnectScript);

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
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("@page \"/services\"", services);
        Assert.Contains("@page \"/services/{Slug}\"", servicePage);
        Assert.Contains("SiteContent.QuoteNewUrl", services);
        Assert.Contains("SiteContent.QuoteNewUrl", servicePage);
        Assert.Contains("services-hero-title", services);
        Assert.Contains("services-hero-logo", services);
        Assert.Contains("src=\"/images/logo.svg\"", services);
        Assert.DoesNotContain("Seven manufacturing services under one MALIEV workflow.", services);
        Assert.DoesNotContain("เวิร์กโฟลว์ MALIEV เดียว", services);
        Assert.Contains(".services-hero-logo", styles);
        Assert.Contains("\"silicone-casting\"", content);
        Assert.Contains("\"rapid-prototyping\"", content);
        Assert.Contains("\"deviation-analysis\"", content);
        Assert.DoesNotContain("window.location.hash", services);
        Assert.DoesNotContain("window.location.hash", servicePage);
    }

    /// <summary>
    /// Verifies material direct comparison removes the property column and uses status icons for insight cards.
    /// </summary>
    [Fact]
    public void MaterialsDirectComparisonUsesInlinePropertyLabelsAndInsightIcons()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Pages", "StaticPage.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.DoesNotContain("material-compare-property-label", source);
        Assert.DoesNotContain("@Text(\"Property\", \"คุณสมบัติ\")", source);
        Assert.DoesNotContain("grid-template-columns: 140px repeat(var(--compare-columns), minmax(0, 1fr));", styles);
        Assert.Contains("grid-template-columns: repeat(var(--compare-columns), minmax(0, 1fr));", styles);
        Assert.Contains("material-compare-cell", source);
        Assert.Contains("material-compare-cell-label", source);
        Assert.Contains("material-insight-heading", source);
        Assert.Contains("material-insight-icon", source);
        Assert.Contains("<MudIcon Icon=\"@Icons.Material.Filled.CheckCircle\" Size=\"Size.Small\" />", source);
        Assert.Contains("<MudIcon Icon=\"@Icons.Material.Filled.Warning\" Size=\"Size.Small\" />", source);
        Assert.Contains(".material-insight-heading", styles);
        Assert.Contains(".material-insight-icon", styles);
        Assert.Contains("overflow-x: auto;", styles);
        Assert.Contains("grid-template-columns: repeat(var(--compare-columns), minmax(240px, 1fr));", styles);
        Assert.DoesNotContain(".material-compare-row {\n    grid-template-columns: 1fr;", styles);
        Assert.DoesNotContain(".material-pro-con-line strong::before", styles);
        Assert.DoesNotContain(".material-compare-insight strong::before", styles);
    }

    /// <summary>
    /// Verifies static customer pages are real routes and keep contact wired through the BFF.
    /// </summary>
    [Fact]
    public void StaticPagesCoverCustomerRoutesAndContactBoundary()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Pages", "StaticPage.razor");
        var content = ReadRepoFile("Maliev.Web.Client", "Content", "SiteContent.cs");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");
        var app = ReadRepoFile("Maliev.Web.Bff", "Components", "App.razor");
        var scrollScript = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "maliev-scroll.js");

        Assert.Contains("@page \"/materials\"", source);
        Assert.Contains("@page \"/case-studies/{Slug}\"", source);
        Assert.Contains("@page \"/blog/{Slug}\"", source);
        Assert.Contains("SiteContent.BlogPosts", source);
        Assert.Contains("ShouldRenderPageHero", source);
        Assert.DoesNotContain("blog-hero-logo", source);
        Assert.Contains("\"blog\" => Text(\"Practical Notes\", \"บทความเชิงปฏิบัติ\")", source);
        Assert.DoesNotContain("\"blog\" => Text(\"Journal\", \"บทความ\")", source);
        Assert.DoesNotContain("<img class=\"blog-hero-logo\"", source);
        Assert.DoesNotContain("\"blog\" => Text(\"MALIEV Journal\", \"บทความ MALIEV\")", source);
        Assert.Contains("@page \"/shipping-returns\"", source);
        Assert.Contains("@page \"/privacy\"", source);
        Assert.Contains("@page \"/terms\"", source);
        Assert.Contains("@page \"/cookie-policy\"", source);
        Assert.Contains("@page \"/refund-policy\"", source);
        Assert.Contains("@page \"/warranty-policy\"", source);
        Assert.Contains("Data subject rights", source);
        Assert.Contains("สิทธิของเจ้าของข้อมูล", source);
        Assert.Contains("Cookie policy", source);
        Assert.Contains("นโยบายคุกกี้", source);
        Assert.Contains("Refund policy", source);
        Assert.Contains("นโยบายการคืนเงิน", source);
        Assert.Contains("Warranty and support policy", source);
        Assert.Contains("นโยบายการรับประกันและการสนับสนุน", source);
        Assert.Contains("IReadOnlyList<LocalizedText> Items", source);
        Assert.Contains("<li>@point.For(Preferences.Culture)</li>", source);
        Assert.Contains("SubmitContactMessageAsync", source);
        Assert.Contains("SiteContent.QuoteNewUrl", source);
        Assert.Contains("class=\"@ContactStatusClass\" role=\"@ContactStatusRole\"", source);
        Assert.Contains("@ContactStatusTitle", source);
        Assert.Contains("contact-file-progress", source);
        Assert.Contains("SetContactFileStage(ContactFileUploadStage.Sending)", source);
        Assert.Contains("We sent a confirmation copy", source);
        Assert.Contains("form-status--error", source);
        Assert.Contains("We could not send the message right now.", source);
        Assert.Contains("aria-busy=\"@_submittingContact\"", source);
        Assert.Contains("@ContactSubmitLabel", source);
        Assert.Contains("await InvokeAsync(StateHasChanged);", source);
        Assert.Contains(".button:disabled,", styles);
        Assert.Contains(".button-busy-indicator", styles);
        Assert.DoesNotContain("_contactStatus = ex.Message;", source);
        Assert.DoesNotContain("@page \"/account/orders\"", source);
        Assert.DoesNotContain("@page \"/account/preferences\"", source);
        Assert.Contains("material-category-media", source);
        Assert.Contains("material-category-body", source);
        Assert.Contains("material-comparison-table", source);
        Assert.Contains("material-mobile-list", source);
        Assert.Contains("material-compare-workbench", source);
        Assert.Contains("class=\"material-compare-workbench\" @ref=\"_materialCompareWorkbench\"", source);
        Assert.Contains("@inject IJSRuntime JS", source);
        Assert.Contains("private ElementReference _materialCompareWorkbench;", source);
        Assert.Contains("private bool _pendingMaterialComparisonScroll;", source);
        Assert.Contains("protected override async Task OnAfterRenderAsync(bool firstRender)", source);
        Assert.Contains("await JS.InvokeVoidAsync(\"malievScroll.scrollIntoView\", _materialCompareWorkbench);", source);
        Assert.Contains("_selectedMaterialNames.Count is >= 2 and <= 3", source);
        Assert.Contains("_pendingMaterialComparisonScroll = true;", source);
        Assert.Contains("_pendingMaterialComparisonScroll = false;", source);
        Assert.Contains("material-row-summary", source);
        Assert.Contains("material-row-media", source);
        Assert.DoesNotContain("material-compare-property-label", source);
        Assert.Contains("material-compare-cell-label", source);
        Assert.Contains("material-compare-material-head", source);
        Assert.Contains("material-compare-head-media", source);
        Assert.Contains("material-compare-head-copy", source);
        Assert.Contains("material-compare-head-name", source);
        Assert.Contains("material-compare-insight", source);
        Assert.Contains("material-insight-heading", source);
        Assert.Contains("material-insight-icon", source);
        Assert.Contains("material-mobile-card-visual", source);
        Assert.Contains("material-mobile-card-basic", source);
        Assert.Contains("material-mobile-card-detail", source);
        Assert.Contains("material-mobile-card-heading", source);
        Assert.Contains("material-mobile-card-summary", source);
        Assert.Contains("material-mobile-card-best-fit", source);
        Assert.Contains("material-mobile-card-media", source);
        Assert.Contains("<img src=\"@material.ImageUrl\"", source);
        Assert.Contains("string ImageUrl,\n        LocalizedText ImageAlt", source);
        Assert.DoesNotContain("/images/materials/pla.svg", source);
        Assert.DoesNotContain("/images/materials/petg.svg", source);
        Assert.Contains("SiteContent.FdmThermoplasticsImageUrl", source);
        Assert.Contains("SiteContent.PowderBedNylonImageUrl", source);
        Assert.Contains("SiteContent.SlaResinImageUrl", source);
        Assert.Contains("https://images.unsplash.com/photo-1742971239045-afabc9f7d744", content);
        Assert.Contains("https://upload.wikimedia.org/wikipedia/commons/thumb/d/d4/SLS_3D_Systems_Printed_Duraform_HST_Pulley_Shaft_%2849014691207%29.jpg", content);
        Assert.Contains("https://images.pexels.com/photos/12268465/pexels-photo-12268465.jpeg", content);
        Assert.Contains("https://images.unsplash.com/photo-1740209475472-aa7d280f7452", source);

        var materialRowSummaryIndex = source.IndexOf("<div class=\"material-row-summary\">", StringComparison.Ordinal);
        var materialRowCopyIndex = source.IndexOf("<span class=\"material-row-copy\">", materialRowSummaryIndex, StringComparison.Ordinal);
        var materialRowMediaIndex = source.IndexOf("<span class=\"material-row-media\">", materialRowSummaryIndex, StringComparison.Ordinal);
        Assert.True(materialRowSummaryIndex >= 0, "material row summary markup should exist");
        Assert.True(materialRowCopyIndex >= 0, "material row copy should exist");
        Assert.True(materialRowMediaIndex >= 0, "material row media should exist");
        Assert.True(materialRowCopyIndex < materialRowMediaIndex, "desktop material table should render text before image");

        var materialCompareHeadIndex = source.IndexOf("<strong class=\"material-compare-material-head\">", StringComparison.Ordinal);
        var materialCompareHeadMediaIndex = source.IndexOf("<span class=\"material-compare-head-media\">", materialCompareHeadIndex, StringComparison.Ordinal);
        var materialCompareHeadCopyIndex = source.IndexOf("<span class=\"material-compare-head-copy\">", materialCompareHeadIndex, StringComparison.Ordinal);
        var materialCompareHeadImageIndex = source.IndexOf("<img src=\"@material.ImageUrl\"", materialCompareHeadMediaIndex, StringComparison.Ordinal);
        var materialCompareHeadNameIndex = source.IndexOf("<span class=\"material-compare-head-name\">@material.Name</span>", materialCompareHeadCopyIndex, StringComparison.Ordinal);
        Assert.True(materialCompareHeadIndex >= 0, "direct comparison material header should exist");
        Assert.True(materialCompareHeadMediaIndex >= 0, "direct comparison header should include material imagery");
        Assert.True(materialCompareHeadImageIndex >= 0, "direct comparison header should render material image");
        Assert.True(materialCompareHeadCopyIndex >= 0, "direct comparison header should include material copy");
        Assert.True(materialCompareHeadNameIndex >= 0, "direct comparison header should include material name");
        Assert.True(materialCompareHeadMediaIndex < materialCompareHeadCopyIndex, "direct comparison header should render image before material copy");
        Assert.True(materialCompareHeadImageIndex < materialCompareHeadNameIndex, "direct comparison header image should come before the material name");

        var materialMobileVisualIndex = source.IndexOf("<div class=\"material-mobile-card-visual\">", StringComparison.Ordinal);
        var materialMobileBasicIndex = source.IndexOf("<div class=\"material-mobile-card-basic\">", StringComparison.Ordinal);
        var materialMobileDetailIndex = source.IndexOf("<div class=\"material-mobile-card-detail\">", StringComparison.Ordinal);
        var materialMobileHeadingIndex = source.IndexOf("<div class=\"material-mobile-card-heading\">", materialMobileBasicIndex, StringComparison.Ordinal);
        var materialMobileSummaryIndex = source.IndexOf("<div class=\"material-mobile-card-summary\">", materialMobileBasicIndex, StringComparison.Ordinal);
        var materialMobileTitleIndex = source.IndexOf("<div class=\"material-mobile-card-title\">", materialMobileHeadingIndex, StringComparison.Ordinal);
        var materialMobileMediaIndex = source.IndexOf("<span class=\"material-mobile-card-media\">", materialMobileVisualIndex, StringComparison.Ordinal);
        var materialMobileCopyIndex = source.IndexOf("<span class=\"material-mobile-card-copy\">", materialMobileTitleIndex, StringComparison.Ordinal);
        var materialMobileBestFitIndex = source.IndexOf("<p class=\"material-mobile-card-best-fit\">", materialMobileSummaryIndex, StringComparison.Ordinal);
        var materialMobileSpecsIndex = source.IndexOf("<dl class=\"material-mobile-specs\">", materialMobileDetailIndex, StringComparison.Ordinal);
        var materialMobileButtonIndex = source.IndexOf("<button class=\"@MaterialSelectButtonClass(material)\"", materialMobileHeadingIndex, StringComparison.Ordinal);
        Assert.True(materialMobileVisualIndex >= 0, "material mobile card visual markup should exist");
        Assert.True(materialMobileBasicIndex >= 0, "material mobile card basic markup should exist");
        Assert.True(materialMobileDetailIndex >= 0, "material mobile card detail markup should exist");
        Assert.True(materialMobileHeadingIndex >= 0, "material mobile card heading markup should exist");
        Assert.True(materialMobileSummaryIndex >= 0, "material mobile card summary markup should exist");
        Assert.True(materialMobileTitleIndex >= 0, "material mobile card title markup should exist");
        Assert.True(materialMobileCopyIndex >= 0, "material mobile card copy should exist");
        Assert.True(materialMobileMediaIndex >= 0, "material mobile card media should exist");
        Assert.True(materialMobileBestFitIndex >= 0, "material mobile card best-fit copy should exist in the summary row");
        Assert.True(materialMobileSpecsIndex >= 0, "material mobile card specs should exist in the detail row");
        Assert.True(materialMobileButtonIndex >= 0, "material select button should exist in the heading row");
        Assert.True(materialMobileVisualIndex < materialMobileBasicIndex, "mobile material cards should render the image before the basic information");
        Assert.True(materialMobileBasicIndex < materialMobileDetailIndex, "mobile material cards should render detail data after the image/basic row");
        Assert.True(materialMobileMediaIndex < materialMobileCopyIndex, "tablet material cards should render material imagery before the data copy");
        Assert.True(materialMobileButtonIndex < materialMobileSummaryIndex, "tablet material cards should keep the compare button in the top heading row");
        Assert.True(materialMobileBestFitIndex < materialMobileSpecsIndex, "material cards should render the summary before detailed material data");

        Assert.Contains("material-pro-con-line material-pro-con-pro", source);
        Assert.Contains("material-pro-con-line material-pro-con-con", source);
        Assert.Contains("material-compare-row material-compare-tone-row material-pro-con-pro", source);
        Assert.Contains("material-compare-row material-compare-tone-row material-pro-con-con", source);
        Assert.Contains("@Text(\"Pros / watch\", \"ข้อดี / ระวัง\")", source);
        Assert.Contains("@Text(\"Advantage\", \"จุดเด่น\")", source);
        Assert.Contains("@Text(\"Check before use\", \"ควรตรวจ\")", source);
        Assert.Contains("Icons.Material.Filled.CheckCircle", source);
        Assert.Contains("Icons.Material.Filled.Warning", source);
        Assert.Contains("<span class=\"material-compare-insight\">", source);
        Assert.Contains("Compare material properties before uploading CAD.", source);
        Assert.Contains("เปรียบเทียบคุณสมบัติวัสดุก่อนอัปโหลด CAD", source);
        Assert.DoesNotContain("Compare strength, heat, chemistry, finish, and trade-offs before uploading CAD.", source);
        Assert.DoesNotContain("เปรียบเทียบความแข็งแรง ความร้อน สารเคมี ผิวงาน และข้อแลกเปลี่ยนก่อนอัปโหลด CAD", source);
        Assert.Contains("FilteredMaterialComparisons", source);
        Assert.Contains("SelectedMaterialComparisons", source);
        Assert.Contains("ToggleMaterialComparison", source);
        Assert.Contains("_selectedMaterialNames.Count < 3", source);
        Assert.Contains("MaterialProcessFilters", source);
        Assert.Contains("MaterialUseFilters", source);
        Assert.Contains("ImageUrl", source);
        Assert.Contains("ImageAlt", source);
        Assert.Contains("js/maliev-scroll.js", app);
        Assert.Contains("window.malievScroll", scrollScript);
        Assert.Contains("scrollIntoView: function (element, offset)", scrollScript);
        Assert.Contains("getBoundingClientRect", scrollScript);
        Assert.Contains("window.scrollTo", scrollScript);
        Assert.Contains("prefers-reduced-motion: reduce", scrollScript);
        Assert.Contains("behavior: prefersReducedMotion ? 'auto' : 'smooth'", scrollScript);
        Assert.Contains(".material-category-media", styles);
        Assert.Contains(".material-category-body", styles);
        Assert.DoesNotContain(".blog-hero-logo", styles);
        Assert.Contains(".content-stack ul", styles);
        Assert.Contains(".material-comparison-table", styles);
        Assert.Contains("border-collapse: separate;", styles);
        Assert.Contains("border-spacing: 0;", styles);
        Assert.Contains(".material-comparison-table thead th:first-child,\n.material-comparison-table tbody td:first-child {\n  position: sticky;\n  left: 0;", styles);
        Assert.Contains(".material-comparison-table tbody td:first-child {\n  z-index: 3;\n  background: var(--paper);", styles);
        Assert.Contains(".material-row-media", styles);
        Assert.Contains(".material-mobile-card-media", styles);
        Assert.Contains(".material-comparison-section .section-heading", styles);
        Assert.Contains(".material-comparison-section .section-heading p {\n  margin: 0;\n  max-width: 62ch;", styles);
        Assert.Contains(".material-comparison-section {\n  max-width: none;\n  padding-left: clamp(24px, 4vw, 72px);\n  padding-right: clamp(24px, 4vw, 72px);", styles);
        Assert.Contains(".material-comparison-panel {\n  display: grid;\n  width: 100%;", styles);
        Assert.Contains(".material-table-shell {\n  width: 100%;", styles);
        Assert.Contains("grid-template-columns: minmax(0, 1.35fr) minmax(300px, .65fr);", styles);
        Assert.Contains(".material-filter-block {\n  display: grid;\n  grid-template-columns: minmax(0, 1fr) minmax(0, 1fr) auto;\n  align-items: start;", styles);
        Assert.Contains(".material-comparison-count {\n  align-self: end;\n  justify-self: end;", styles);
        Assert.DoesNotContain(".material-filter-block {\n  display: grid;\n  grid-template-columns: minmax(0, 1fr) minmax(0, 1fr) auto;\n  align-items: end;", styles);
        Assert.Contains("@media (max-width: 1180px)", styles);
        Assert.Contains("@media (min-width: 681px) and (max-width: 1180px)", styles);
        Assert.Contains(".material-mobile-card {\n    grid-template-columns: minmax(260px, .38fr) minmax(0, 1fr);", styles);
        Assert.Contains("grid-template-areas:\n      \"visual basic\"\n      \"visual detail\";", styles);
        Assert.Contains(".material-comparison-section {\n    padding-left: 24px;\n    padding-right: 24px;", styles);
        Assert.Contains(".material-comparison-section .section-heading {\n    grid-template-columns: minmax(0, 1fr);\n    gap: 14px;", styles);
        Assert.Contains("@media (max-width: 680px)", styles);
        Assert.Contains(".material-comparison-section {\n    padding-left: 18px;\n    padding-right: 18px;", styles);
        Assert.Contains("display: none;", styles);
        Assert.Contains("display: grid;", styles);
        Assert.Contains(".material-select-button", styles);
        Assert.Contains(".material-compare-matrix", styles);
        Assert.DoesNotContain(".material-compare-property-label", styles);
        Assert.Contains(".material-compare-cell-label", styles);
        Assert.Contains(".material-compare-material-head", styles);
        Assert.Contains(".material-compare-head-media", styles);
        Assert.Contains(".material-compare-insight", styles);
        Assert.Contains(".material-insight-heading", styles);
        Assert.Contains(".material-insight-icon", styles);
        Assert.Contains(".material-pro-con-pro", styles);
        Assert.Contains(".material-pro-con-con", styles);
        Assert.Contains("color-mix(in srgb, #f59e0b 10%, var(--paper))", styles);
        Assert.Contains("border-left: 4px solid var(--material-tone);", styles);
        Assert.DoesNotContain(".material-pro-con-line strong::before", styles);
        Assert.DoesNotContain(".material-compare-insight strong::before", styles);
        Assert.DoesNotContain("color-mix(in srgb, var(--red) 72%, var(--ink))", styles);
        Assert.Contains(".material-filter", styles);
        Assert.Contains("grid-template-columns: repeat(4, minmax(0, 1fr));", styles);
        Assert.Contains(".material-row-summary {\n  display: grid;\n  gap: 12px;", styles);
        Assert.Contains("max-width: 168px;", styles);
        Assert.DoesNotContain("grid-template-columns: 68px minmax(0, 1fr);", styles);
        Assert.Contains(".material-mobile-card-title {\n  display: grid;\n  gap: 12px;", styles);
        Assert.Contains(".material-mobile-card-basic {\n  grid-area: basic;\n  display: grid;\n  align-content: start;", styles);
        Assert.Contains("justify-items: stretch;", styles);
        Assert.Contains(".material-mobile-card-detail {\n  grid-area: detail;\n  display: grid;\n  align-content: start;\n  gap: 14px;", styles);
        Assert.Contains(".material-mobile-card-heading {\n  display: flex;\n  align-items: flex-start;\n  justify-content: space-between;", styles);
        Assert.Contains(".material-mobile-card-summary {\n  display: grid;\n  grid-template-columns: auto minmax(0, 1fr);", styles);
        Assert.Contains("border-top: 1px solid var(--rule);", styles);
        Assert.Contains(".material-mobile-card {\n    grid-template-columns: minmax(0, 1fr);\n    grid-template-areas:\n      \"visual\"\n      \"basic\"\n      \"detail\";", styles);
        Assert.Contains(".material-mobile-card-media {\n    align-self: stretch;\n    height: 100%;\n    min-height: 260px;", styles);
        Assert.Contains(".material-mobile-card-media {\n    align-self: stretch;\n    aspect-ratio: 16 / 10;", styles);
        Assert.Contains(".material-mobile-card-heading .material-select-button {\n    flex: 0 0 auto;", styles);
        Assert.Contains(".material-mobile-card-heading {\n    display: grid;\n    grid-template-columns: minmax(0, 1fr);", styles);
        Assert.Contains(".material-mobile-card-heading .material-select-button {\n    justify-self: stretch;\n    inline-size: 100%;\n    min-width: 100%;\n    width: 100%;", styles);
        Assert.Contains(".material-mobile-card-summary .material-process-pill {\n    width: 100%;\n    justify-content: flex-start;", styles);
        Assert.Contains(".material-mobile-card-copy", styles);
        Assert.DoesNotContain(".material-mobile-card-title {\n  display: grid;\n  grid-template-columns: 74px minmax(0, 1fr);", styles);
        Assert.DoesNotContain("href=\"/quote\"", source);
    }

    /// <summary>
    /// Verifies public journal and case-study cards navigate to real detail pages with slug-specific content.
    /// </summary>
    [Fact]
    public void JournalAndCaseStudyCardsHaveRealDetailPages()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Pages", "StaticPage.razor");
        var content = ReadRepoFile("Maliev.Web.Client", "Content", "SiteContent.cs");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("CurrentBlogPost", source);
        Assert.Contains("CurrentCaseStudy", source);
        Assert.Contains("article-detail-layout", source);
        Assert.Contains("article-detail-layout blog-detail", source);
        Assert.Contains("case-study-detail", source);
        Assert.Contains(".article-detail-layout", styles);
        Assert.Contains(".article-detail-layout .content-stack", styles);
        Assert.Contains(".article-detail-layout .content-stack article", styles);
        Assert.Contains(".article-detail-layout .detail-section + .detail-section", styles);
        Assert.Contains("box-shadow: none", styles);
        Assert.Contains("background: transparent", styles);
        Assert.Contains("border-radius: 0", styles);
        Assert.DoesNotContain(".blog-detail .content-stack article", styles);
        Assert.DoesNotContain(".case-study-detail .content-stack article", styles);
        Assert.Contains(".detail-sidebar", styles);
        Assert.Contains("detail-sidebar-actions", source);
        Assert.Contains(".detail-sidebar-actions", styles);
        Assert.Contains(".detail-sidebar-actions .button {\n  width: 100%;", styles);
        Assert.Contains(".blog-detail .detail-section ul", styles);
        Assert.Contains(".blog-detail .detail-section li", styles);
        Assert.Contains(".blog-detail .detail-section li::before", styles);
        Assert.Contains(".blog-detail .detail-sidebar li", styles);
        Assert.Contains("list-style: none;", styles);
        Assert.Contains("grid-template-columns: 10px minmax(0, 1fr);", styles);
        Assert.Contains("BlogDetailSectionClass", source);
        Assert.Contains("IsBlogChecklistSection", source);
        Assert.Contains("article-checklist", source);
        Assert.Contains(".detail-section--checklist", styles);
        Assert.Contains(".article-checklist", styles);
        Assert.Contains("content: \"✓\";", styles);
        Assert.DoesNotContain("var path when path == \"blog\" || path.StartsWith(\"blog/\"", source);
        Assert.DoesNotContain("var path when path == \"case-studies\" || path.StartsWith(\"case-studies/\"", source);

        Assert.Contains("IReadOnlyList<ArticleSectionContent> Sections", content);
        Assert.Contains("design-for-manufacturing", content);
        Assert.Contains("Wall thickness is the first DFM signal", content);
        Assert.Contains("choosing-3d-printing-materials", content);
        Assert.Contains("Start from the part's job, not the material name", content);
        Assert.Contains("instant-part-pricing", content);
        Assert.Contains("Instant pricing is a quoting workspace, not a blind checkout", content);
        Assert.Contains("fixture-turnaround", content);
        Assert.Contains("The worn sample was not enough by itself", content);
        Assert.Contains("prototype-iteration", content);
        Assert.Contains("Each revision had one decision to answer", content);
        Assert.Contains("scan-to-cad-repair", content);
        Assert.Contains("Scanning captured the old part before CAD cleanup", content);
    }

    /// <summary>
    /// Verifies the shared static-page contact CTA uses the MALIEV logo as the visible brand mark.
    /// </summary>
    [Fact]
    public void StaticPageSecondaryContactCtaUsesLogoMark()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Pages", "StaticPage.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");
        var buttonLogoStyle = styles[
            styles.IndexOf(".button-logo-mark {", StringComparison.Ordinal)..styles.IndexOf(".button.danger {", StringComparison.Ordinal)];

        Assert.Contains("aria-label=\"@SecondaryAction\"", source);
        Assert.Contains("@if (SecondaryActionUsesLogo)", source);
        Assert.Contains("<span>@SecondaryActionPrefix</span>", source);
        Assert.Contains("<img class=\"button-logo-mark\" src=\"/images/logo.svg\" alt=\"\" aria-hidden=\"true\" />", source);
        Assert.Contains("private bool SecondaryActionUsesLogo => string.Equals(SecondaryHref, \"/contact\", StringComparison.Ordinal);", source);
        Assert.Contains("private string SecondaryActionPrefix => Text(\"Contact\", \"ติดต่อ\");", source);
        Assert.DoesNotContain("<a class=\"button secondary\" href=\"@SecondaryHref\">@SecondaryAction</a>", source);
        Assert.Contains(".button-logo-mark {", styles);
        Assert.Contains("display: block;", buttonLogoStyle);
        Assert.Contains("align-self: center;", buttonLogoStyle);
        Assert.Contains("flex: 0 0 auto;", buttonLogoStyle);
        Assert.Contains("height: .9em;", buttonLogoStyle);
        Assert.Contains("filter: var(--logo-filter);", buttonLogoStyle);
        Assert.Contains("transform: none;", buttonLogoStyle);
        Assert.DoesNotContain("transform: translateY(.03em);", buttonLogoStyle);
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

        Assert.Contains("AddMalievIdentityCookie", program);
        Assert.Contains("AddGoogle", program);
        Assert.Contains("sharedsecrets.json", program);
        Assert.Contains("Maliev.Aspire", program);
        Assert.Contains("Maliev.Aspire.AppHost", program);
        Assert.Contains("options.Scope.Add(\"profile\")", program);
        Assert.Contains("options.Scope.Add(\"email\")", program);
        Assert.Contains("prompt=select_account", program);
        Assert.Contains("AddCascadingAuthenticationState", program);
        Assert.Contains("UseAuthentication", program);
        Assert.Contains("UseAuthorization", program);
        Assert.Contains("IAuthServiceClient, AuthServiceClient", program);
        Assert.Contains("ICustomerServiceClient, CustomerServiceClient", program);
        Assert.Contains("AddAuthenticatedServiceClient<ICustomerServiceClient, CustomerServiceClient>(\"CustomerService\")\n    .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(10));", program);
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
    /// Verifies the account orders empty state is aligned beside the account navigation without card framing.
    /// </summary>
    [Fact]
    public void AccountOrdersEmptyStateIsUnframedBesideNavigation()
    {
        var orders = ReadRepoFile("Maliev.Web.Client", "Pages", "AccountOrders.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("class=\"empty-state compact account-orders-empty\"", orders);
        Assert.Contains(".account-orders-empty", styles);
        Assert.Contains(".account-orders-empty {\n  display: grid;\n  gap: 12px;\n  max-width: 620px;\n  padding: 0;\n  padding-inline-start: clamp(20px, 3vw, 36px);\n  background: transparent;\n  border-radius: 0;\n  box-shadow: none;\n  overflow: visible;", styles);
        Assert.Contains(".account-orders-empty h2,\n.account-orders-empty p {\n  margin: 0;\n}", styles);
        Assert.DoesNotContain("account-orders-empty {\n  padding: 28px;", styles);
    }

    /// <summary>
    /// Verifies the account landing page uses polished loading and sanitized error states.
    /// </summary>
    [Fact]
    public void AccountLandingPageUsesDesignedLoadingAndSanitizedErrorStates()
    {
        var account = ReadRepoFile("Maliev.Web.Client", "Pages", "Account.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("account-loading-panel", account);
        Assert.Contains("role=\"status\"", account);
        Assert.Contains("account-loading-spinner", account);
        Assert.Contains("account-loading-skeleton", account);
        Assert.Contains("account-unavailable-panel", account);
        Assert.Contains("Account details are temporarily unavailable.", account);
        Assert.Contains("_accountUnavailable = true;", account);
        Assert.DoesNotContain("class=\"load-message\"", account);
        Assert.DoesNotContain("_error = ex.Message", account);
        Assert.DoesNotContain("<p>@_error</p>", account);

        Assert.Contains(".account-loading-panel", styles);
        Assert.Contains(".account-loading-spinner", styles);
        Assert.Contains("@keyframes account-spinner", styles);
        Assert.Contains(".account-loading-skeleton", styles);
        Assert.Contains(".account-unavailable-panel", styles);
        Assert.Contains(".account-unavailable-icon", styles);
    }

    /// <summary>
    /// Verifies direct Web runs do not let shared Aspire secrets override the Web Google OAuth client.
    /// </summary>
    [Fact]
    public void DirectWebGoogleUserSecretsOverrideSharedAspireSecrets()
    {
        var program = ReadRepoFile("Maliev.Web.Bff", "Program.cs");
        var readme = ReadRepoFile("README.md");

        Assert.Contains("AddDevelopmentSharedSecretsFallback(builder);", program);
        Assert.Contains("ApplyMissingSharedSecretValues(builder.Configuration, sharedSecrets);", program);
        Assert.Contains("string.IsNullOrWhiteSpace(target[path])", program);
        Assert.Contains("Direct Web user-secrets take priority", readme);
        Assert.DoesNotContain("builder.Configuration.AddJsonFile(sharedSecretsPath, optional: true, reloadOnChange: true);", program);
    }

    /// <summary>
    /// Verifies customer auth pages keep form titles text-only while using a Google-branded OAuth button.
    /// </summary>
    [Fact]
    public void AuthPagesUseTextOnlyTitleAndGoogleBrandedButton()
    {
        var signIn = ReadRepoFile("Maliev.Web.Client", "Pages", "AuthSignIn.razor");
        var signUp = ReadRepoFile("Maliev.Web.Client", "Pages", "AuthSignUp.razor");
        var googleButton = ReadRepoFile("Maliev.Web.Client", "Components", "AuthGoogleButton.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");
        var app = ReadRepoFile("Maliev.Web.Bff", "Components", "App.razor");

        Assert.DoesNotContain("auth-title-logo", signIn);
        Assert.DoesNotContain("<img class=\"auth-title-logo\"", signIn);
        Assert.Contains("<AuthGoogleButton Href=\"@GoogleHref\"", signIn);
        Assert.Contains("auth-google-icon", googleButton);
        Assert.Contains("viewBox=\"0 0 18 18\"", googleButton);
        Assert.Contains("#4285F4", googleButton);
        Assert.Contains("#34A853", googleButton);
        Assert.Contains("#FBBC05", googleButton);
        Assert.Contains("#EA4335", googleButton);
        Assert.Contains("border: 1px solid #747775;", styles);
        Assert.Contains("font-family: Roboto, var(--maliev-font-sans);", styles);
        Assert.Contains("family=Roboto:wght@500", app);
        Assert.DoesNotContain("@Text(\"Sign in to MALIEV\", \"เข้าสู่ระบบ MALIEV\")", signIn);
        Assert.DoesNotContain("@Text(\"Create your MALIEV account\", \"สร้างบัญชี MALIEV\")", signUp);
        Assert.DoesNotContain("auth-google-mark", signIn);
        Assert.DoesNotContain(".auth-google-mark", styles);
    }

    /// <summary>
    /// Verifies the auth brand panel uses real manufacturing imagery instead of generated line-art assets.
    /// </summary>
    [Fact]
    public void AuthBrandPanelUsesPhotoBasedManufacturingTreatment()
    {
        var signIn = ReadRepoFile("Maliev.Web.Client", "Pages", "AuthSignIn.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("auth-brand-panel", signIn);
        Assert.Contains("auth-brand-features", signIn);
        Assert.DoesNotContain("auth-blueprint", signIn);
        Assert.DoesNotContain("<svg class=\"auth-blueprint", signIn);

        Assert.Contains("url(\"/images/blog/design-for-manufacturing.jpg\")", styles);
        Assert.Contains("center / cover no-repeat", styles);
        Assert.Contains("backdrop-filter: blur(10px);", styles);
        Assert.DoesNotContain(".auth-blueprint-card", styles);
    }

    /// <summary>
    /// Verifies the Google OAuth button keeps the official light button treatment on dark pages.
    /// </summary>
    [Fact]
    public void GoogleAuthButtonUsesGoogleLightTreatmentInDarkTheme()
    {
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("html[data-theme=\"dark\"] .auth-google", styles);
        Assert.Contains("html[data-theme=\"dark\"] .auth-google:hover", styles);
        Assert.Contains("html[data-theme=\"dark\"] .auth-google:focus-visible", styles);
        Assert.Contains("background: #ffffff;", styles);
        Assert.Contains("color: #1f1f1f;", styles);
        Assert.Contains("border-color: #747775;", styles);
        Assert.Contains("box-shadow: 0 1px 2px rgba(60, 64, 67, .30), 0 1px 3px 1px rgba(60, 64, 67, .15);", styles);
        Assert.Contains("background: #f8fafd;", styles);
        Assert.Contains("outline-color: #8ab4f8;", styles);
    }

    /// <summary>
    /// Verifies browser auth forms post to dedicated action routes instead of colliding with Blazor page routes.
    /// </summary>
    [Fact]
    public void AuthFormsPostToDedicatedActionRoutes()
    {
        var signIn = ReadRepoFile("Maliev.Web.Client", "Pages", "AuthSignIn.razor");
        var forgotPassword = ReadRepoFile("Maliev.Web.Client", "Pages", "AuthForgotPassword.razor");
        var resetPassword = ReadRepoFile("Maliev.Web.Client", "Pages", "AuthResetPassword.razor");
        var authController = ReadRepoFile("Maliev.Web.Bff", "Controllers", "AuthController.cs");
        var signInLines = signIn.Split('\n', StringSplitOptions.TrimEntries);
        var forgotPasswordLines = forgotPassword.Split('\n', StringSplitOptions.TrimEntries);
        var resetPasswordLines = resetPassword.Split('\n', StringSplitOptions.TrimEntries);
        var authControllerLines = authController.Split('\n', StringSplitOptions.TrimEntries);

        Assert.Contains("action=\"/auth/sign-in/email\"", signIn);
        Assert.Contains("action=\"/auth/sign-up/email\"", signIn);
        Assert.Contains("action=\"/auth/forgot-password/request\"", forgotPassword);
        Assert.Contains("action=\"/auth/reset-password/confirm\"", resetPassword);
        Assert.Contains("[HttpPost(\"sign-in/email\")]", authController);
        Assert.Contains("[HttpPost(\"sign-up/email\")]", authController);
        Assert.Contains("[HttpPost(\"forgot-password/request\")]", authController);
        Assert.Contains("[HttpPost(\"reset-password/confirm\")]", authController);
        Assert.DoesNotContain("<form class=\"auth-form\" method=\"post\" action=\"/auth/sign-in\">", signInLines);
        Assert.DoesNotContain("<form class=\"auth-form\" method=\"post\" action=\"/auth/sign-up\">", signInLines);
        Assert.DoesNotContain("<form class=\"auth-form\" method=\"post\" action=\"/auth/forgot-password\">", forgotPasswordLines);
        Assert.DoesNotContain("<form class=\"auth-form\" method=\"post\" action=\"/auth/reset-password\">", resetPasswordLines);
        Assert.DoesNotContain("[HttpPost(\"sign-in\")]", authControllerLines);
        Assert.DoesNotContain("[HttpPost(\"sign-up\")]", authControllerLines);
        Assert.DoesNotContain("[HttpPost(\"forgot-password\")]", authControllerLines);
        Assert.DoesNotContain("[HttpPost(\"reset-password\")]", authControllerLines);
    }

    /// <summary>
    /// Verifies email auth starts with only an email field while Google stays a primary action.
    /// </summary>
    [Fact]
    public void AuthPagesUseEmailFirstGooglePrimaryFlow()
    {
        var signIn = ReadRepoFile("Maliev.Web.Client", "Pages", "AuthSignIn.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("@Text(\"Sign in or sign up\", \"เข้าสู่ระบบหรือสมัครสมาชิก\")", signIn);
        Assert.Contains("<AuthGoogleButton Href=\"@GoogleHref\"", signIn);
        Assert.Contains("class=\"auth-email-entry-form", signIn);
        Assert.Contains("id=\"auth-email-entry\"", signIn);
        Assert.Contains("aria-describedby=\"auth-email-entry-requirements\"", signIn);
        Assert.Contains("@onsubmit=\"ContinueWithEmail\"", signIn);
        Assert.Contains("@Text(\"Continue\", \"ดำเนินการต่อ\")", signIn);
        Assert.Contains("private bool ShowCredentialStep => _authStep == AuthStep.Credentials;", signIn);
        Assert.DoesNotContain("<details class=\"auth-email-panel\"", signIn);
        Assert.True(signIn.IndexOf("class=\"auth-email-entry-form", StringComparison.Ordinal) < signIn.IndexOf("<AuthGoogleButton", StringComparison.Ordinal));

        Assert.Contains(".auth-email-entry-form", styles);
        Assert.Contains(".auth-credential-form", styles);
        Assert.Contains(".auth-step-actions", styles);
        Assert.Contains(".auth-requirement-list", styles);
    }

    /// <summary>
    /// Verifies email auth forms explain requirements and use browser-native live validation constraints.
    /// </summary>
    [Fact]
    public void AuthEmailFormsExplainRequirementsAndUseLiveValidation()
    {
        var signIn = ReadRepoFile("Maliev.Web.Client", "Pages", "AuthSignIn.razor");
        var signUp = ReadRepoFile("Maliev.Web.Client", "Pages", "AuthSignUp.razor");
        var resetPassword = ReadRepoFile("Maliev.Web.Client", "Pages", "AuthResetPassword.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("id=\"auth-email-entry-requirements\"", signIn);
        Assert.Contains("EmailRequirementClass", signIn);
        Assert.Contains("id=\"sign-in-email-requirements\"", signIn);
        Assert.Contains("id=\"sign-in-password-requirements\"", signIn);
        Assert.Contains("aria-describedby=\"sign-in-email-requirements\"", signIn);
        Assert.Contains("aria-describedby=\"sign-in-password-requirements\"", signIn);
        Assert.Contains("minlength=\"6\"", signIn);
        Assert.Contains("@Text(\"Use a full email address, for example name@company.com.\", \"ใช้อีเมลแบบเต็ม เช่น name@company.com\")", signIn);
        Assert.Contains("@Text(\"Password has at least 6 characters.\", \"รหัสผ่านมีอย่างน้อย 6 ตัวอักษร\")", signIn);
        Assert.Contains("PasswordRequirementClass", signIn);

        Assert.Contains("id=\"sign-up-email-requirements\"", signIn);
        Assert.Contains("id=\"sign-up-password-requirements\"", signIn);
        Assert.Contains("aria-describedby=\"sign-up-email-requirements\"", signIn);
        Assert.Contains("aria-describedby=\"sign-up-password-requirements\"", signIn);
        Assert.Contains("minlength=\"6\"", signIn);
        Assert.Contains("@Text(\"Verify email address\", \"ยืนยันอีเมล\")", signIn);
        Assert.Contains("@onclick=\"BackToEmailStep\"", signIn);
        Assert.DoesNotContain("name=\"FirstName\"", signIn);
        Assert.DoesNotContain("name=\"LastName\"", signIn);
        Assert.DoesNotContain("minlength=\"12\"", signUp);

        Assert.Contains("id=\"reset-password-requirements\"", resetPassword);
        Assert.Contains("aria-describedby=\"reset-password-requirements\"", resetPassword);
        Assert.Contains("minlength=\"6\"", resetPassword);
        Assert.DoesNotContain("minlength=\"12\"", resetPassword);

        Assert.Contains(".auth-field-help", styles);
        Assert.Contains(".auth-requirement-item.is-met", styles);
        Assert.Contains(".auth-requirement-item.is-pending", styles);
        Assert.Contains(".auth-form input:user-invalid", styles);
        Assert.Contains(".auth-form input:user-valid", styles);
    }

    /// <summary>
    /// Verifies the mobile navigation opens as a viewport overlay instead of growing the sticky header.
    /// </summary>
    [Fact]
    public void MobileNavigationDoesNotGrowStickyHeader()
    {
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("--site-header-height: 66px;", styles);
        Assert.Contains("--site-header-height: 60px;", styles);
        Assert.Contains(".mobile-nav.open", styles);
        Assert.Contains("position: fixed;", styles);
        Assert.Contains("top: var(--site-header-height);", styles);
        Assert.Contains("left: 0;", styles);
        Assert.Contains("right: 0;", styles);
        Assert.Contains("max-height: calc(100svh - var(--site-header-height));", styles);
        Assert.Contains("overflow-y: auto;", styles);
        Assert.Contains("background: var(--paper);", styles);
        Assert.Contains("border-bottom: 1px solid var(--rule);", styles);
        Assert.Contains("-webkit-overflow-scrolling: touch;", styles);
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

        Assert.Contains("DefaultQuoteEngineUrl = \"https://quote.maliev.com\"", content);
        Assert.Contains("QuoteDemoUrl => $\"{QuoteEngineUrl}/demo\"", content);
        Assert.Contains("QuoteNewProjectUrl => $\"{QuoteEngineUrl}/quotes/new\"", content);
        Assert.DoesNotContain("https://quote.maliev.com/quotes/new", content);
        Assert.Contains("SiteContent.QuoteNewUrl", shop);
        Assert.Contains("SiteContent.QuoteNewUrl", product);
        Assert.Contains("SiteContent.QuoteNewUrl", error);
        Assert.DoesNotContain("href=\"/quote\"", shop);
        Assert.DoesNotContain("href=\"/quote\"", product);
        Assert.DoesNotContain("href=\"/quote\"", error);
    }

    /// <summary>
    /// Verifies the shop page uses the editorial marketplace layout.
    /// </summary>
    [Fact]
    public void ShopUsesEditorialMarketplaceLayout()
    {
        var shop = ReadRepoFile("Maliev.Web.Client", "Pages", "Shop.razor");
        var grid = ReadRepoFile("Maliev.Web.Client", "Components", "Commerce", "ProductGrid.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("class=\"shop-masthead\"", shop);
        Assert.Contains("class=\"shop-search\"", shop);
        Assert.Contains("class=\"shop-feature-band\"", shop);
        Assert.Contains("class=\"shop-category-rail\"", shop);
        Assert.Contains("class=\"shop-catalog-stage\"", shop);
        Assert.Contains("class=\"empty-state shop-empty-state\"", shop);
        Assert.Contains("ProductGrid Products=\"@visibleProducts\" FeaturedHandle=\"@FeaturedProduct?.Handle\"", shop);
        Assert.Contains("@ProductCardClass(product)", grid);
        Assert.Contains("ProductCategory(product)", grid);
        Assert.Contains("public string? FeaturedHandle { get; set; }", grid);
        Assert.Contains(".shop-masthead", styles);
        Assert.Contains(".shop-feature-band", styles);
        Assert.Contains(".shop-category-rail", styles);
        Assert.Contains(".shop-catalog-stage", styles);
        Assert.Contains(".product-card-featured", styles);
        Assert.Contains(".shop-empty-state", styles);
        Assert.Contains(".shop-search::before", styles);
        Assert.DoesNotContain("page-hero compact shop-hero", shop);
        Assert.DoesNotContain("shop-toolbar", shop);
    }

    /// <summary>
    /// Verifies public shop catalog calls fail fast when CommerceService is unavailable.
    /// </summary>
    [Fact]
    public void CommerceCatalogClientUsesPublicPageTimeout()
    {
        var program = ReadRepoFile("Maliev.Web.Bff", "Program.cs");

        Assert.Contains("ICommerceServiceClient, CommerceServiceClient", program);
        Assert.Contains(".ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(10));", program);
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
        Assert.Contains("isCanvasNearViewport", source);
        Assert.Contains("requestAnimationFrame", source);
        Assert.Contains("alpha: true", source);
        Assert.Contains("antialias: true", source);
        Assert.Contains("highQualityRendering: Boolean(modelUrl)", source);
        Assert.Contains("createEngine(canvas, BABYLON, { highQuality: true });", source);
        Assert.Contains("configureSceneAntialiasing(scene, camera, BABYLON);", source);
        Assert.Contains("new BABYLON.FxaaPostProcess(\"canvas-fxaa\", 1.0, camera);", source);
        Assert.Contains("fxaa.samples = Math.min(4, Math.max(1, maxSamples));", source);
        Assert.Contains("premultipliedAlpha: false", source);
        Assert.Contains("renderRatio", source);
        Assert.Contains("setHardwareScalingLevel", source);
        Assert.Contains("engine.setHardwareScalingLevel(1 / renderRatio)", source);
        Assert.Contains("resizeRenderRatio: 0", source);
        Assert.Contains("const renderRatio = getRenderPixelRatio(state.highQualityRendering);", source);
        Assert.Contains("const minimumRatio = highQuality ? (mobile ? 1.75 : 2) : (mobile ? 1.5 : 1.2);", source);
        Assert.Contains("const maxRatio = highQuality ? 2.5 : 2;", source);
        Assert.Contains("state.engine.resize(true);", source);
        Assert.Contains("resizeScene(state, true);", source);
        Assert.Contains("canvas.width", source);
        Assert.Contains("state.resizeRenderRatio === renderRatio", source);
        Assert.Contains("ResizeObserver", source);
        Assert.Contains("powerPreference: highQuality ? \"high-performance\" : \"low-power\"", source);

        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");
        Assert.Contains(".manufacturing-gizmo--landing", styles);
        Assert.Contains("overflow: visible", styles);
        Assert.DoesNotContain(".manufacturing-gizmo--landing::before", styles);
        Assert.Contains("--landing-gizmo-canvas-bg: linear-gradient(45deg, #ffffff 0%", styles);
        Assert.Contains("rgba(217, 233, 255, .72) 68%, rgba(10, 114, 239, .16) 100%", styles);
        Assert.Contains("--landing-gizmo-canvas-bg:\n    radial-gradient(circle at 100% 0%", styles);
        Assert.Contains("linear-gradient(225deg, rgba(244, 246, 248, .06)", styles);
        Assert.Contains("linear-gradient(180deg, #11263d 0%, #101d2d 48%, #0f1824 100%);", styles);
        Assert.Contains("background: var(--landing-gizmo-canvas-bg);", styles);
    }

    /// <summary>
    /// Verifies the prerendered 3D component starts in a visible loading state before Blazor interactivity mounts the scene.
    /// </summary>
    [Fact]
    public void ManufacturingGizmoPrerendersLoadingFallback()
    {
        var component = ReadRepoFile("Maliev.Web.Client", "Components", "Quote", "ManufacturingGizmo.razor");
        var app = ReadRepoFile("Maliev.Web.Bff", "Components", "App.razor");
        var source = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "manufacturing-gizmo.js");

        Assert.Contains("\"manufacturing-gizmo is-loading\"", component);
        Assert.Contains("$\"manufacturing-gizmo is-loading {Class}\"", component);
        Assert.Contains("data-manufacturing-gizmo", component);
        Assert.Contains("data-model-url=\"@ModelUrl\"", component);
        Assert.Contains("type=\"module\"", app);
        Assert.Contains("js/manufacturing-gizmo.js", app);
        Assert.Contains("mountDocumentGizmos", source);
        Assert.Contains("querySelectorAll", source);
        Assert.Contains("canvas[data-manufacturing-gizmo]", source);
    }

    /// <summary>
    /// Verifies the landing 3D runtime has a dedicated model scene path instead of stretching the compact quote gizmo.
    /// </summary>
    [Fact]
    public void ManufacturingGizmoSupportsHoverDrivenPlasticLandingModel()
    {
        var catalog = ReadRepoFile("Maliev.Web.Client", "Content", "HeroModelCatalog.cs");
        var component = ReadRepoFile("Maliev.Web.Client", "Components", "Quote", "ManufacturingGizmo.razor");
        var source = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "manufacturing-gizmo.js");

        Assert.Contains("\"3d-printing-part-02\"", catalog);
        Assert.DoesNotContain("DisplayScale", catalog);
        Assert.DoesNotContain("1.72", catalog);
        Assert.DoesNotContain("1.24", catalog);
        Assert.DoesNotContain("ModelScale", component);
        Assert.DoesNotContain("data-model-scale", component);
        Assert.Contains("babylonjs-loaders@9.6.0", source);
        Assert.Contains("createLandingHeroScene", source);
        Assert.Contains("configureLandingHeroCamera", source);
        Assert.Contains("Math.PI / 1.95", source);
        Assert.DoesNotContain("Math.PI / 2.65", source);
        Assert.Contains("state.landingFrame = frameImportedModel", source);
        Assert.Contains("const normalizedHeroModelSize = 2.28;", source);
        Assert.Contains("normalizeImportedModelDimensions(displayBounds, root, frameMeshes);", source);
        Assert.Contains("const scale = normalizedHeroModelSize / maxDimension;", source);
        Assert.Contains("root.scaling.setAll(scale);", source);
        Assert.Contains("for (const mesh of frameMeshes)", source);
        Assert.Contains("state.landingFrame", source);
        Assert.Contains("worldBounds: captureWorldAabb", source);
        Assert.Contains("applyLandingHeroFraming(camera, frame.worldBounds, metrics, BABYLON);", source);
        Assert.Contains("configureLandingHeroCamera(camera, state.host, BABYLON, state.landingFrame);", source);
        Assert.Contains("camera.getViewMatrix(true)", source);
        Assert.Contains("refreshCameraMatrices", source);
        Assert.Contains("camera.getProjectionMatrix?.(true)", source);
        Assert.Contains("camera.upperRadiusLimit = null", source);
        Assert.Contains("updateWorldMatrixChain", source);
        Assert.Contains("state.engine.resize(true);\n  state.cameraConfigurator?.();", source);
        Assert.Contains("addHoverMotion", source);
        Assert.DoesNotContain("modelScale", source);
        Assert.DoesNotContain("const targetSize = 2.28 * modelScale", source);
        Assert.DoesNotContain("wide ? 7.35 : 7.05", source);
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
        Assert.Contains("addIdleLevitation", source);
        Assert.Contains("root.position.y", source);
        Assert.Contains("Math.sin", source);
        Assert.Contains("const baseRotation = new BABYLON.Vector3(0.04, -0.18, 0.005)", source);
        Assert.Contains("Math.sin(elapsed * 0.00055) * 0.025", source);
        Assert.DoesNotContain("Math.sin(elapsed * 0.0012) * 0.055", source);
        Assert.DoesNotContain("* 0.24", source, StringComparison.Ordinal);
        Assert.DoesNotContain("* 0.11", source, StringComparison.Ordinal);
        Assert.DoesNotContain("frameLandingHeroCamera", source);
        Assert.DoesNotContain("measureProjectedMeshFrame", source);
        Assert.DoesNotContain("projectedFrameFits", source);
        Assert.DoesNotContain("BABYLON.Vector3.Project", source);
        Assert.DoesNotContain("getModelAwareHeroMetrics", source);
    }

    /// <summary>
    /// Verifies the landing hero model uses camera framing instead of CSS canvas scaling, and resize reframing is coalesced.
    /// </summary>
    [Fact]
    public void LandingHeroModelUsesBabylonFramingAndResizeDebounced()
    {
        var source = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "manufacturing-gizmo.js");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("resizeFrame: 0", source);
        Assert.Contains("scheduleResizeScene(state)", source);
        Assert.Contains("ResizeObserver(() => scheduleResizeScene(state))", source);
        Assert.Contains("state.resizeHandler = () => scheduleResizeScene(state);", source);
        Assert.Contains("function applyLandingHeroFraming(camera, worldBounds, metrics, BABYLON)", source);
        Assert.Contains("const center = worldBounds.min.add(worldBounds.max).scale(0.5);", source);
        Assert.Contains("camera.target = new BABYLON.Vector3(targetX, center.y + metrics.targetY, center.z);", source);
        Assert.Contains("const radius = Math.max(verticalRadius, horizontalRadius) + halfD;", source);
        Assert.Contains("camera.radius = clamp(radius, metrics.minRadius, metrics.maxRadius);", source);
        Assert.Contains("function captureWorldAabb(meshes, BABYLON)", source);
        Assert.Contains("info.update(worldMatrix);", source);
        Assert.Contains("fill: narrowTall ? 0.62 : compact ? 0.68", source);
        Assert.DoesNotContain("camera.radius = Math.max(camera.radius * metrics.framingRadiusScale * modelRadiusScale, metrics.minRadius);", source);
        Assert.DoesNotContain("camera.radius = Math.max(camera.radius * metrics.framingRadiusScale * modelRadiusScale, fitRadius, metrics.minRadius);", source);
        Assert.DoesNotContain("camera.useFramingBehavior = true;", source);
        Assert.DoesNotContain("framing.zoomOnMeshesHierarchy", source);
        Assert.DoesNotContain("function computeLandingHeroFitRadius", source);
        Assert.DoesNotContain("framingRadiusScale", source);
        Assert.DoesNotContain("tallFrameRadiusScale", source);
        Assert.Contains("selectDominantModelFrame(entries, aggregate, BABYLON) ?? aggregate", source);
        Assert.Contains("const sparseAssemblyRatio = aggregateSpan / Math.max(largestMeshSpan, 0.0001);", source);
        Assert.Contains("sparseAssemblyRatio < 18", source);
        Assert.Contains("meshes: [dominant.mesh]", source);
        Assert.Contains("node.getBoundingInfo().update(worldMatrix);", source);
        Assert.Contains(".manufacturing-gizmo--landing .manufacturing-gizmo-canvas", styles);
        Assert.Contains("pointer-events: none;", styles);
        Assert.DoesNotContain("transform: scale(4.25);", styles);
        Assert.DoesNotContain("transform-origin: 50% 52%;", styles);
        Assert.DoesNotContain("will-change: transform;", styles);
        Assert.DoesNotContain("targetFill", source);
        Assert.DoesNotContain("safeInset", source);
    }

    /// <summary>
    /// Verifies the home hero canvas paints a full-viewport backdrop in both color themes.
    /// </summary>
    [Fact]
    public void LandingHeroCanvasBackdropSpansViewportInBothThemes()
    {
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("--landing-gizmo-canvas-bg: linear-gradient(45deg, #ffffff 0%", styles);
        Assert.Contains("rgba(217, 233, 255, .72) 68%, rgba(10, 114, 239, .16) 100%", styles);
        Assert.Contains("--landing-gizmo-canvas-bg:\n    radial-gradient(circle at 100% 0%", styles);
        Assert.Contains("linear-gradient(225deg, rgba(244, 246, 248, .06)", styles);
        Assert.Contains("linear-gradient(180deg, #11263d 0%, #101d2d 48%, #0f1824 100%);", styles);
        Assert.Contains(".landing-hero {\n  position: relative;", styles);
        Assert.Contains("isolation: isolate;\n  display: grid;", styles);
        Assert.Contains("overflow: visible;", styles);
        Assert.Contains(".manufacturing-gizmo--landing {\n  position: absolute;\n  top: 0;\n  bottom: 0;\n  left: 50%;\n  right: auto;\n  width: 100vw;\n  height: 100%;\n  transform: translateX(-50%);", styles);
        Assert.Contains("z-index: 0;\n  pointer-events: none;", styles);
        Assert.Contains("@media (min-width: 681px) and (max-width: 960px)", styles);
        Assert.Contains(".manufacturing-gizmo--landing {\n    position: relative;\n    inset: auto;\n    width: 100vw;\n    height: min(46vh, 470px);\n    min-height: 440px;\n    overflow: hidden;\n    transform: none;", styles);
        Assert.Contains("@media (max-width: 680px)", styles);
        Assert.Contains(".manufacturing-gizmo--landing {\n    position: relative;\n    inset: auto;\n    width: 100vw;\n    height: clamp(240px, 58vw, 360px);\n    min-height: 0;\n    overflow: hidden;\n    transform: none;", styles);
        Assert.DoesNotContain("--landing-gizmo-canvas-bg: transparent;", styles);
    }

    /// <summary>
    /// Verifies the home hero model is lifted and reclined so the front no longer reads as facing downward.
    /// </summary>
    [Fact]
    public void LandingHeroModelPoseLiftsAndReclinesTheModel()
    {
        var source = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "manufacturing-gizmo.js");

        Assert.Contains("targetY: compact ? -0.08 : wide ? -0.22 : -0.2", source);
        Assert.DoesNotContain("wide ? -0.38", source);
        Assert.Contains("const baseRotation = new BABYLON.Vector3(0.04, -0.18, 0.005)", source);
        Assert.Contains("const presentationColumnBias = wide ? 0.018 : 0;", source);
        Assert.DoesNotContain("const presentationColumnBias = wide ? 0.045 : 0;", source);
        Assert.DoesNotContain("const baseRotation = new BABYLON.Vector3(0.06, -0.36, 0.02)", source);
        Assert.DoesNotContain("const baseRotation = new BABYLON.Vector3(-0.12, -0.36, 0.02)", source);
    }

    /// <summary>
    /// Verifies the landing hero model uses a CAD-style studio lighting rig.
    /// </summary>
    [Fact]
    public void LandingHeroModelUsesCadStudioLighting()
    {
        var source = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "manufacturing-gizmo.js");

        Assert.Contains("configureLandingCadToneMapping(scene, BABYLON);", source);
        Assert.Contains("configureLandingCadStudioLighting(scene, camera, BABYLON)", source);
        Assert.Contains("const softbox = new BABYLON.DirectionalLight(\"landing-softbox\"", source);
        Assert.Contains("const cameraHeadlight = new BABYLON.DirectionalLight(\"landing-camera-headlight\"", source);
        Assert.Contains("scene.onBeforeRenderObservable.add(() => updateLandingHeadlight(camera, cameraHeadlight, BABYLON));", source);
        Assert.Contains("updateLandingHeadlight(camera, cameraHeadlight, BABYLON);", source);
        Assert.Contains("configureCadAmbientOcclusion(scene, camera, BABYLON);", source);
        Assert.Contains("new BABYLON.SSAO2RenderingPipeline(\"landing-cad-ambient-occlusion\"", source);
        Assert.Contains("ssao.totalStrength = 0.58;", source);
        Assert.Contains("configureLandingCadShadows(scene, lights.key, state.landingFrame, renderMeshes, BABYLON);", source);
        Assert.Contains("scene.shadowsEnabled = true;", source);
        Assert.Contains("const shadowGenerator = new BABYLON.ShadowGenerator(2048, keyLight);", source);
        Assert.Contains("shadowGenerator.useBlurExponentialShadowMap = true;", source);
        Assert.Contains("shadowGenerator.blurKernel = 22;", source);
        Assert.Contains("shadowGenerator.addShadowCaster(mesh, false);", source);
        Assert.Contains("BABYLON.MeshBuilder.CreateGround(\"landing-shadow-catcher\"", source);
        Assert.Contains("shadowCatcher.receiveShadows = true;", source);
        Assert.Contains("shadowMaterial.opacityTexture = createLandingShadowOpacityTexture(scene, BABYLON);", source);
        Assert.Contains("function createLandingShadowOpacityTexture(scene, BABYLON)", source);
        Assert.Contains("gradient.addColorStop(1, \"rgba(255, 255, 255, 0)\");", source);
        Assert.Contains("shadowMaterial.alpha = dark ? 0.18 : 0.1;", source);
        Assert.Contains("scene.imageProcessingConfiguration.toneMappingEnabled = true;", source);
        Assert.Contains("scene.imageProcessingConfiguration.toneMappingType = BABYLON.ImageProcessingConfiguration.TONEMAPPING_ACES;", source);
        Assert.Contains("scene.imageProcessingConfiguration.exposure = 1.06;", source);
        Assert.Contains("scene.imageProcessingConfiguration.contrast = 1.18;", source);
        Assert.Contains("plastic.roughness = 0.74;", source);
        Assert.Contains("plastic.specularIntensity = 0.2;", source);
        Assert.Contains("plastic.environmentIntensity = 0.36;", source);
        Assert.Contains("plastic.clearCoat.isEnabled = false;", source);
        Assert.Contains("mesh.useVertexColors = false;", source);
        Assert.Contains("mesh.hasVertexAlpha = false;", source);
        Assert.Contains("disableCadMeshEdges(meshes);", source);
        Assert.Contains("disableCadMeshEdges(plasticMaterial.metadata?.cadMeshes ?? []);", source);
        Assert.Contains("mesh.disableEdgesRendering();", source);
        Assert.Contains("mesh.edgesWidth = 0;", source);
        Assert.DoesNotContain("configureCadMeshEdges", source);
        Assert.DoesNotContain("mesh.enableEdgesRendering(0.42);", source);
        Assert.DoesNotContain("mesh.edgesColor = edgeColor;", source);
        Assert.Contains("lights.cameraHeadlight.intensity = dark ? 0.62 : 0.34;", source);
        Assert.Contains("lights.softbox.intensity = dark ? 0.72 : 0.42;", source);
        Assert.Contains("plasticMaterial.albedoColor = BABYLON.Color3.FromHexString(dark ? \"#9ea8b4\" : \"#8f99a6\");", source);
        Assert.Contains("plasticMaterial.specularIntensity = dark ? 0.28 : 0.24;", source);
        Assert.Contains("plasticMaterial.environmentIntensity = dark ? 0.34 : 0.24;", source);
        Assert.DoesNotContain("scene.environmentIntensity = dark ? 0.55 : 0.42;", source);
    }

    /// <summary>
    /// Verifies horizontal hover motion rotates the hero model toward the pointer direction.
    /// </summary>
    [Fact]
    public void LandingHeroHoverMotionRotatesTowardPointerDirection()
    {
        var source = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "manufacturing-gizmo.js");

        Assert.Contains("root.rotation.y = baseRotation.y - hoverX * 0.12;", source);
        Assert.Contains("root.rotation.z = baseRotation.z + hoverX * 0.014;", source);
        Assert.Contains("camera.alpha = baseAlpha - hoverX * 0.026;", source);
        Assert.DoesNotContain("root.rotation.y = baseRotation.y + hoverX * 0.12;", source);
        Assert.DoesNotContain("root.rotation.z = baseRotation.z - hoverX * 0.014;", source);
        Assert.DoesNotContain("camera.alpha = baseAlpha + hoverX * 0.026;", source);
    }

    /// <summary>
    /// Verifies vertical hover motion maps screen-up to model-up instead of using inverted screen Y.
    /// </summary>
    [Fact]
    public void LandingHeroHoverMotionUsesNonInvertedVerticalPointerDirection()
    {
        var source = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "manufacturing-gizmo.js");

        Assert.Contains("pointer.targetY = (0.5 - (event.clientY - rect.top) / rect.height) * 2;", source);
        Assert.Contains("root.rotation.x = baseRotation.x + hoverY * 0.055;", source);
        Assert.Contains("camera.beta = clamp(baseBeta + hoverY * 0.018, 0.72, 1.36);", source);
        Assert.DoesNotContain("pointer.targetY = ((event.clientY - rect.top) / rect.height - 0.5) * 2;", source);
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
        Assert.Contains("grid-template-columns: minmax(0, 1.08fr) minmax(320px, .92fr);", styles);
        Assert.Contains("text-wrap: balance;", styles);
        Assert.Contains("@media (min-width: 681px) and (max-width: 960px)", styles);
        Assert.Contains("text-align: center;", styles);
        Assert.Contains("gap: 18px;", styles);
        Assert.Contains("background: var(--landing-gizmo-canvas-bg);", styles);
        Assert.Contains("margin-top: 22px;", styles);
        Assert.Contains("width: min(100%, 640px);\n    margin-inline: auto;", styles);
        Assert.Contains("grid-template-columns: repeat(3, minmax(0, 1fr));", styles);
        Assert.Contains("width: 100vw;\n    margin-left: calc(50% - 50vw);\n    margin-right: calc(50% - 50vw);", styles);
        Assert.Contains("min-height: 440px;", styles);
        Assert.Contains("height: min(46vh, 470px);", styles);
        Assert.Contains("@media (max-width: 680px)", styles);
        Assert.DoesNotContain(".landing-hero {\n    grid-template-columns: minmax(0, 1fr);\n    grid-template-areas:\n      \"visual\"\n      \"copy\";", styles);
        Assert.Contains(".landing-hero {\n    grid-template-columns: minmax(0, 1fr);\n    grid-template-areas:\n      \"copy\"\n      \"visual\";", styles);
        Assert.Contains("align-content: start;", styles);
        Assert.Contains("min-height: auto;", styles);
        Assert.Contains(".landing-hero-copy {\n    grid-area: copy;\n    max-width: none;\n    text-align: center;", styles);
        Assert.DoesNotContain(".landing-hero-visual {\n    position: relative;\n    grid-area: visual;\n    justify-self: center;\n    width: min(100%, 560px);\n    min-height: 320px;", styles);
        Assert.Contains(".landing-hero-visual {\n    position: relative;\n    grid-area: visual;\n    justify-self: center;\n    width: 100vw;\n    margin-left: calc(50% - 50vw);\n    margin-right: calc(50% - 50vw);", styles);
        Assert.Contains(".manufacturing-gizmo--landing {\n    position: relative;\n    inset: auto;\n    width: 100vw;\n    height: clamp(240px, 58vw, 360px);\n    min-height: 0;\n    overflow: hidden;", styles);
        Assert.Contains(".manufacturing-gizmo--landing,\n  .manufacturing-gizmo--landing .manufacturing-gizmo-canvas {\n    background: transparent;", styles);
        Assert.Contains(".metric-strip {\n    display: grid;\n    grid-template-columns: repeat(3, minmax(0, 1fr));", styles);
        Assert.Contains(".metric-strip div {\n    min-width: 0;\n    justify-items: center;", styles);
        Assert.Contains(".metric-strip {\n  flex-wrap: wrap;\n  justify-content: flex-start;\n  gap: clamp(34px, 4vw, 58px);", styles);
        Assert.Contains("width: fit-content;\n  max-width: 100%;", styles);
        Assert.Contains(".metric-strip {\n    display: grid;\n    grid-template-columns: repeat(3, minmax(0, 1fr));\n    justify-content: initial;", styles);
        Assert.Contains("@media (min-width: 1600px)", styles);
        Assert.Contains("@media (min-width: 2400px)", styles);
        Assert.Contains("justify-content: center;", styles);
        Assert.Contains("const narrowTall = width < 700 && height >= 500 && aspect < 1.12;", gizmo);
        Assert.Contains("fill: narrowTall ? 0.62 : compact ? 0.68", gizmo);
        Assert.Contains("fallbackRadius: narrowTall ? 6.4", gizmo);
        Assert.Contains("const balancedTablet = width >= 640 && width <= 920 && height >= 460;", gizmo);
        Assert.Contains("balancedTablet ? 0.48 : wide ? 0.45 : 0.46", gizmo);
        Assert.Contains("applyLandingHeroFraming", gizmo);
        Assert.Contains("captureWorldAabb", gizmo);
        Assert.DoesNotContain("framing.zoomOnMeshesHierarchy", gizmo);
        Assert.DoesNotContain("frameLandingHeroCamera", gizmo);
        Assert.DoesNotContain("measureProjectedMeshFrame", gizmo);
        Assert.DoesNotContain("projectedFrameFits", gizmo);
    }

    /// <summary>
    /// Verifies the narrow mobile landing hero stays centered and compact instead of stacking a tall canvas above the copy.
    /// </summary>
    [Fact]
    public void HomeHeroUsesCompactCenteredNarrowMobileComposition()
    {
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("@media (max-width: 680px)", styles);
        Assert.Contains(".landing-hero {\n    grid-template-columns: minmax(0, 1fr);\n    grid-template-areas:\n      \"copy\"\n      \"visual\";", styles);
        Assert.Contains("min-height: auto;\n    padding-top: 24px;\n    padding-bottom: 34px;\n    text-align: center;", styles);
        Assert.Contains("background: var(--landing-gizmo-canvas-bg);", styles);
        Assert.Contains(".landing-hero-copy {\n    grid-area: copy;\n    max-width: none;\n    text-align: center;", styles);
        Assert.Contains(".landing-hero-copy > p,\n  .landing-hero-actions {\n    max-width: none;\n    margin-inline: auto;", styles);
        Assert.Contains(".landing-quote-dropzone-primary {\n    grid-template-columns: 38px minmax(0, 1fr);\n    gap: 10px;", styles);
        Assert.Contains(".landing-quote-dropzone-browse {\n    grid-template-columns: minmax(0, 1fr) auto;", styles);
        Assert.Contains(".landing-quote-dropzone-action {\n    justify-self: start;", styles);
        Assert.Contains(".landing-hero-visual {\n    position: relative;\n    grid-area: visual;\n    justify-self: center;\n    width: 100vw;\n    margin-left: calc(50% - 50vw);\n    margin-right: calc(50% - 50vw);", styles);
        Assert.Contains(".manufacturing-gizmo--landing {\n    position: relative;\n    inset: auto;\n    width: 100vw;\n    height: clamp(240px, 58vw, 360px);\n    min-height: 0;\n    overflow: hidden;", styles);
        Assert.Contains(".manufacturing-gizmo--landing,\n  .manufacturing-gizmo--landing .manufacturing-gizmo-canvas {\n    background: transparent;", styles);
        Assert.Contains(".metric-strip {\n    display: grid;\n    grid-template-columns: repeat(3, minmax(0, 1fr));", styles);
        Assert.Contains(".metric-strip div {\n    min-width: 0;\n    justify-items: center;", styles);
        Assert.DoesNotContain("grid-template-areas:\n      \"visual\"\n      \"copy\";", styles);
    }

    /// <summary>
    /// Verifies the 3D canvas does not show a browser focus outline when clicked.
    /// </summary>
    [Fact]
    public void ManufacturingGizmoCanvasDoesNotExposeFocusRing()
    {
        var component = ReadRepoFile("Maliev.Web.Client", "Components", "Quote", "ManufacturingGizmo.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("ModulePath = \"/js/manufacturing-gizmo.js?v=20260525-hero-left\"", component);
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
