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
        Assert.Contains("hero-{service-slug}-{short-subject}-{nn}.glb", modelReadme);
        Assert.Contains("hero-cnc-machining-fixture-01.glb", modelReadme);
        Assert.Contains("hero-3d-scanning-reference-01.glb", modelReadme);
        Assert.Contains("internal const string DefaultServiceSlug = \"3d-printing\";", catalog);
        Assert.Contains("\"/models/hero-3d-printing-part-01.glb\"", catalog);
        Assert.Contains("\"/models/hero-3d-printing-part-02.glb\"", catalog);
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
        Assert.DoesNotContain("hero-3d-2.glb", catalog);
        Assert.DoesNotContain("hero-3d.glb", catalog);
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

        Assert.Contains("service-grid", source);
        Assert.Contains("feature-band", source);
        Assert.Contains("process-section", source);
        Assert.Contains("process-grid", source);
        Assert.Contains("workflow-step-visual", source);
        Assert.Contains("workflow-step-number", source);
        Assert.DoesNotContain("process-kicker", source);
        Assert.DoesNotContain("Customer workflow", source);
        Assert.DoesNotContain("ขั้นตอนลูกค้า", source);
        Assert.Contains("<MudIcon Icon=\"@step.Icon\" Size=\"Size.Large\" />", source);
        Assert.DoesNotContain("viewBox=\"0 0 64 64\"", source);
        Assert.Contains("new(\"01\", \"upload\", Icons.Material.Filled.UploadFile", source);
        Assert.Contains("new(\"02\", \"dfm\", Icons.Material.Filled.FactCheck", source);
        Assert.Contains("new(\"03\", \"price\", Icons.Material.Filled.PriceChange", source);
        Assert.Contains("new(\"04\", \"order\", Icons.Material.Filled.Inventory2", source);
        Assert.True(
            source.IndexOf("class=\"workflow-step-number\"", StringComparison.Ordinal) <
            source.IndexOf("class=\"workflow-step-visual\"", StringComparison.Ordinal));
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
        Assert.Contains("grid-template-rows: auto minmax(0, 1fr) auto", styles);
        Assert.Contains("align-content: start", styles);
        Assert.Contains("filter: var(--logo-filter)", styles);
        Assert.Contains(".process-section", styles);
        Assert.Contains(".process-section::after", styles);
        Assert.DoesNotContain(".process-section::before", styles);
        Assert.DoesNotContain(".process-kicker", styles);
        Assert.Contains(".workflow-step-number", styles);
        Assert.Contains(".workflow-step-visual .mud-icon-root", styles);
        Assert.DoesNotContain(".workflow-step-visual span", styles);
        Assert.Contains(".workflow-step--dfm .workflow-step-visual", styles);
        Assert.Contains("home-services-section", source);
        Assert.Contains(".home-services-section", styles);
        Assert.Contains("padding-top: clamp(44px, 5vw, 72px);", styles);
        Assert.Contains(".home-services-section .section-heading", styles);
        Assert.Contains("machine-feature", source);
        Assert.Contains("machine-feature-backdrop", source);
        Assert.Contains("/images/products/pneumatic-injection-molding-machines.png", source);
        Assert.True(File.Exists(Path.Combine(root, "Maliev.Web.Bff", "wwwroot", "images", "products", "pneumatic-injection-molding-machines.png")));
        Assert.DoesNotContain("https://shop.maliev.com/cdn/shop/files/machine-portrait.21.png", source);
        Assert.DoesNotContain("machine-feature-kicker", source);
        Assert.DoesNotContain("PIMM-30 / PIMM-50", source);
        Assert.DoesNotContain(".machine-feature-kicker", styles);
        Assert.DoesNotContain("PIMM-30", source);
        Assert.DoesNotContain("PIMM-50", source);
        Assert.Contains("the 30g variant reaches 300°C, the 50g variant reaches 350°C", source);
        Assert.Contains("machine-stat-grid", source);
        Assert.Contains("<div><strong>30g/50g</strong><small>@Text(\"shot capacity\", \"ปริมาตรฉีดต่อครั้ง\")</small></div>", source);
        Assert.DoesNotContain("<div><strong>50g</strong><small>@Text(\"shot capacity\", \"ปริมาตรฉีดต่อครั้ง\")</small></div>", source);
        Assert.Contains("<div><strong>300/350°C</strong><small>@Text(\"max melt\", \"อุณหภูมิสูงสุด\")</small></div>", source);
        Assert.DoesNotContain("<div><strong>180°C</strong><small>@Text(\"max melt\", \"อุณหภูมิสูงสุด\")</small></div>", source);
        Assert.Contains("<div><strong>7 bar</strong><small>@Text(\"air supply\", \"แรงดันลม\")</small></div>", source);
        Assert.DoesNotContain("<div><strong>6 bar</strong><small>@Text(\"air supply\", \"แรงดันลม\")</small></div>", source);
        Assert.Contains("<div><strong>30d</strong><small>@Text(\"lead time\", \"ระยะเวลา\")</small></div>", source);
        Assert.DoesNotContain("<div><strong>14d</strong><small>@Text(\"lead time\", \"ระยะเวลา\")</small></div>", source);
        Assert.Contains(".machine-feature", styles);
        Assert.Contains("grid-template-columns: minmax(0, .9fr) minmax(540px, .82fr);", styles);
        Assert.Contains(".machine-feature-backdrop", styles);
        Assert.Contains("object-fit: cover;", styles);
        Assert.Contains(".machine-feature .h-display", styles);
        Assert.Contains(".machine-stat-grid", styles);
        Assert.Contains("font-size: clamp(1.5rem, 2.1vw, 2rem);", styles);
        Assert.Contains("margin-bottom: 30px;", styles);
        Assert.Contains("grid-template-columns: repeat(2, minmax(0, 1fr));", styles);
        Assert.Contains(".social-link", styles);
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
        Assert.Contains("@if (IsPrimaryService(service))", source);
        Assert.Contains("PromoteHighlightedService", source);
        Assert.Contains("SelectRotatingHighlightedService", source);
        Assert.Contains("StringComparison.OrdinalIgnoreCase", source);
        Assert.Contains("internal static string? ResolveExplicitTargetKey(string? url)", content);
        Assert.Contains("internal static string ResolveServiceSlug(string? targetKey)", content);
        Assert.Contains("grid-template-rows: auto minmax(0, 1fr) auto;", styles);
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
        Assert.Contains("line-contact-logo", source);
        Assert.Contains("line-contact-logo-bg", source);
        Assert.Contains("line-contact-logo-bubble", source);
        Assert.Contains("line-contact-logo-text", source);
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
        Assert.DoesNotContain(">Facebook</a>", source);
        Assert.DoesNotContain(">YouTube</a>", source);
        Assert.DoesNotContain(">Instagram</a>", source);
        Assert.DoesNotContain(">LINE @@maliev</a>", source);
        Assert.DoesNotContain("M20.2 4.4C18.1 2.7", source);
        Assert.Contains(".line-contact-logo-bg", styles);
        Assert.Contains(".line-contact-logo-bubble", styles);
        Assert.Contains(".line-contact-logo-text", styles);
        Assert.Contains(".social-link--facebook", styles);
        Assert.Contains(".social-link--youtube", styles);
        Assert.Contains(".social-link--instagram", styles);
        Assert.Contains("#1877f2", styles);
        Assert.Contains("#ff0000", styles);
        Assert.Contains("#833ab4", styles);
        Assert.Contains("FoundingYear = 2018", source);
        Assert.Contains("DateTime.Today.Year", source);
        Assert.Contains("All rights reserved", source);
        Assert.Contains("footer-legal", source);
        Assert.DoesNotContain("<strong>MALIEV Co., Ltd.</strong>", source);
        Assert.DoesNotContain("Nonthaburi, Thailand. Manufacturing services, machines, and production support.", source);
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

        Assert.Contains("MudIconButton", source);
        Assert.Contains("MudBadge", source);
        Assert.Contains("Icons.Material.Filled.ShoppingCart", source);
        Assert.Contains("Icons.Material.Filled.AccountCircle", source);
        Assert.Contains("Href=\"/account\"", source);
        Assert.Contains("SiteContent.QuoteNewUrl", source);
        Assert.DoesNotContain("class=\"icon-link\"", source);
    }

    /// <summary>
    /// Verifies the public layout includes the service-bounded customer chatbot widget.
    /// </summary>
    [Fact]
    public void PublicLayoutIncludesCustomerChatbotWidget()
    {
        var layout = ReadRepoFile("Maliev.Web.Client", "Layout", "MainLayout.razor");
        var component = ReadRepoFile("Maliev.Web.Client", "Components", "CustomerChatbot.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("<CustomerChatbot />", layout);
        Assert.Contains("customer manufacturing assistant", component);
        Assert.Contains("Hi, I'm Mali.", component);
        Assert.Contains("น้องมะลิ", component);
        Assert.Contains("Icons.Material.Filled.SupportAgent", component);
        Assert.Contains("aria-label=\"@Text(\"Open Mali\"", component);
        Assert.Contains("maliev.chatbot.personalization.v1", component);
        Assert.Contains("localStorage.getItem", component);
        Assert.Contains("localStorage.setItem", component);
        Assert.Contains("CustomerContext = BuildCustomerContext()", component);
        Assert.Contains("customer-chatbot-popout", component);
        Assert.Contains("customer-chatbot-unread-badge", component);
        Assert.DoesNotContain("Ask MALIEV", component);
        Assert.Contains(".customer-chatbot", styles);
        Assert.Contains(".customer-chatbot-panel", styles);
        Assert.Contains(".customer-chatbot-popout", styles);
        Assert.Contains(".customer-chatbot-unread-badge", styles);
        Assert.Contains(".customer-chatbot-toggle {\n  width: 56px;\n  min-height: 56px;\n  padding: 0;\n  border-radius: 9999px;", styles);
        Assert.Contains(".customer-chatbot-toggle .mud-icon-root", styles);
        Assert.Contains(".customer-chatbot-toggle {\n    width: 50px;\n    min-height: 50px;", styles);
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
        Assert.Contains("blog-hero-title", source);
        Assert.DoesNotContain("blog-hero-logo", source);
        Assert.Contains("\"blog\" => Text(\"Journal\", \"บทความ\")", source);
        Assert.Contains("Text(\"Journal\", \"บทความ\")", source);
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
        Assert.DoesNotContain("@page \"/account/orders\"", source);
        Assert.DoesNotContain("@page \"/account/preferences\"", source);
        Assert.Contains("material-category-media", source);
        Assert.Contains("material-category-body", source);
        Assert.Contains("material-comparison-table", source);
        Assert.Contains("material-mobile-list", source);
        Assert.Contains("material-compare-workbench", source);
        Assert.Contains("material-row-summary", source);
        Assert.Contains("material-row-media", source);
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
        Assert.Contains("https://images.unsplash.com/photo-1742971239045-afabc9f7d744", source);
        Assert.Contains("https://upload.wikimedia.org/wikipedia/commons/thumb/d/d4/SLS_3D_Systems_Printed_Duraform_HST_Pulley_Shaft_%2849014691207%29.jpg", source);
        Assert.Contains("https://images.pexels.com/photos/12268465/pexels-photo-12268465.jpeg", source);
        Assert.Contains("https://images.unsplash.com/photo-1740209475472-aa7d280f7452", source);

        var materialRowSummaryIndex = source.IndexOf("<div class=\"material-row-summary\">", StringComparison.Ordinal);
        var materialRowCopyIndex = source.IndexOf("<span class=\"material-row-copy\">", materialRowSummaryIndex, StringComparison.Ordinal);
        var materialRowMediaIndex = source.IndexOf("<span class=\"material-row-media\">", materialRowSummaryIndex, StringComparison.Ordinal);
        Assert.True(materialRowSummaryIndex >= 0, "material row summary markup should exist");
        Assert.True(materialRowCopyIndex >= 0, "material row copy should exist");
        Assert.True(materialRowMediaIndex >= 0, "material row media should exist");
        Assert.True(materialRowCopyIndex < materialRowMediaIndex, "desktop material table should render text before image");

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
        Assert.Contains("@media (max-width: 680px)", styles);
        Assert.Contains(".material-comparison-section {\n    padding-left: 18px;\n    padding-right: 18px;", styles);
        Assert.Contains("display: none;", styles);
        Assert.Contains("display: grid;", styles);
        Assert.Contains(".material-select-button", styles);
        Assert.Contains(".material-compare-matrix", styles);
        Assert.Contains(".material-pro-con-pro", styles);
        Assert.Contains(".material-pro-con-con", styles);
        Assert.Contains("color-mix(in srgb, var(--red) 72%, var(--ink))", styles);
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
        Assert.Contains("case-study-detail", source);
        Assert.Contains(".article-detail-layout", styles);
        Assert.Contains(".detail-sidebar", styles);
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
    /// Verifies customer auth pages use the MALIEV logo in the sign-in title and a Google-branded OAuth button.
    /// </summary>
    [Fact]
    public void AuthPagesUseLogoTitleAndGoogleBrandedButton()
    {
        var signIn = ReadRepoFile("Maliev.Web.Client", "Pages", "AuthSignIn.razor");
        var signUp = ReadRepoFile("Maliev.Web.Client", "Pages", "AuthSignUp.razor");
        var googleButton = ReadRepoFile("Maliev.Web.Client", "Components", "AuthGoogleButton.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");
        var app = ReadRepoFile("Maliev.Web.Bff", "Components", "App.razor");

        Assert.Contains("auth-title-logo", signIn);
        Assert.Contains("src=\"/images/logo.svg\"", signIn);
        Assert.Contains("auth-title-logo", signUp);
        Assert.Contains("src=\"/images/logo.svg\"", signUp);
        Assert.Contains("<AuthGoogleButton Href=\"@GoogleHref\"", signIn);
        Assert.Contains("<AuthGoogleButton Href=\"@GoogleHref\"", signUp);
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
        Assert.DoesNotContain("auth-google-mark", signUp);
        Assert.DoesNotContain(".auth-google-mark", styles);
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
        var signUp = ReadRepoFile("Maliev.Web.Client", "Pages", "AuthSignUp.razor");
        var forgotPassword = ReadRepoFile("Maliev.Web.Client", "Pages", "AuthForgotPassword.razor");
        var resetPassword = ReadRepoFile("Maliev.Web.Client", "Pages", "AuthResetPassword.razor");
        var authController = ReadRepoFile("Maliev.Web.Bff", "Controllers", "AuthController.cs");
        var signInLines = signIn.Split('\n', StringSplitOptions.TrimEntries);
        var signUpLines = signUp.Split('\n', StringSplitOptions.TrimEntries);
        var forgotPasswordLines = forgotPassword.Split('\n', StringSplitOptions.TrimEntries);
        var resetPasswordLines = resetPassword.Split('\n', StringSplitOptions.TrimEntries);
        var authControllerLines = authController.Split('\n', StringSplitOptions.TrimEntries);

        Assert.Contains("action=\"/auth/sign-in/email\"", signIn);
        Assert.Contains("action=\"/auth/sign-up/email\"", signUp);
        Assert.Contains("action=\"/auth/forgot-password/request\"", forgotPassword);
        Assert.Contains("action=\"/auth/reset-password/confirm\"", resetPassword);
        Assert.Contains("[HttpPost(\"sign-in/email\")]", authController);
        Assert.Contains("[HttpPost(\"sign-up/email\")]", authController);
        Assert.Contains("[HttpPost(\"forgot-password/request\")]", authController);
        Assert.Contains("[HttpPost(\"reset-password/confirm\")]", authController);
        Assert.DoesNotContain("<form class=\"auth-form\" method=\"post\" action=\"/auth/sign-in\">", signInLines);
        Assert.DoesNotContain("<form class=\"auth-form\" method=\"post\" action=\"/auth/sign-up\">", signUpLines);
        Assert.DoesNotContain("<form class=\"auth-form\" method=\"post\" action=\"/auth/forgot-password\">", forgotPasswordLines);
        Assert.DoesNotContain("<form class=\"auth-form\" method=\"post\" action=\"/auth/reset-password\">", resetPasswordLines);
        Assert.DoesNotContain("[HttpPost(\"sign-in\")]", authControllerLines);
        Assert.DoesNotContain("[HttpPost(\"sign-up\")]", authControllerLines);
        Assert.DoesNotContain("[HttpPost(\"forgot-password\")]", authControllerLines);
        Assert.DoesNotContain("[HttpPost(\"reset-password\")]", authControllerLines);
    }

    /// <summary>
    /// Verifies email auth remains available but is collapsed behind the preferred Google action by default.
    /// </summary>
    [Fact]
    public void AuthPagesCollapseEmailFallbackByDefault()
    {
        var signIn = ReadRepoFile("Maliev.Web.Client", "Pages", "AuthSignIn.razor");
        var signUp = ReadRepoFile("Maliev.Web.Client", "Pages", "AuthSignUp.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("<AuthGoogleButton Href=\"@GoogleHref\"", signIn);
        Assert.Contains("<details class=\"auth-email-panel\" open=\"@EmailPanelOpen\">", signIn);
        Assert.Contains("<summary>@Text(\"Use email instead\", \"ใช้อีเมลแทน\")</summary>", signIn);
        Assert.Contains("private bool EmailPanelOpen => !string.IsNullOrWhiteSpace(Error);", signIn);
        Assert.True(signIn.IndexOf("<AuthGoogleButton", StringComparison.Ordinal) < signIn.IndexOf("<details class=\"auth-email-panel\"", StringComparison.Ordinal));
        Assert.DoesNotContain("<div class=\"auth-divider\"", signIn);

        Assert.Contains("<AuthGoogleButton Href=\"@GoogleHref\"", signUp);
        Assert.Contains("<details class=\"auth-email-panel\" open=\"@EmailPanelOpen\">", signUp);
        Assert.Contains("<summary>@Text(\"Create with email\", \"สร้างด้วยอีเมล\")</summary>", signUp);
        Assert.Contains("private bool EmailPanelOpen => !string.IsNullOrWhiteSpace(Error);", signUp);
        Assert.True(signUp.IndexOf("<AuthGoogleButton", StringComparison.Ordinal) < signUp.IndexOf("<details class=\"auth-email-panel\"", StringComparison.Ordinal));
        Assert.DoesNotContain("<div class=\"auth-divider\"", signUp);

        Assert.Contains(".auth-email-panel", styles);
        Assert.Contains(".auth-email-panel summary", styles);
        Assert.Contains(".auth-email-panel summary::after", styles);
        Assert.Contains(".auth-email-panel[open] summary", styles);
        Assert.Contains(".auth-email-panel .auth-form", styles);
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

        Assert.Contains("id=\"sign-in-email-requirements\"", signIn);
        Assert.Contains("id=\"sign-in-password-requirements\"", signIn);
        Assert.Contains("aria-describedby=\"sign-in-email-requirements\"", signIn);
        Assert.Contains("aria-describedby=\"sign-in-password-requirements\"", signIn);
        Assert.Contains("minlength=\"6\"", signIn);
        Assert.Contains("@Text(\"Use a full email address, for example name@company.com.\", \"ใช้อีเมลแบบเต็ม เช่น name@company.com\")", signIn);
        Assert.Contains("@Text(\"Password must be at least 6 characters.\", \"รหัสผ่านต้องมีอย่างน้อย 6 ตัวอักษร\")", signIn);

        Assert.Contains("id=\"sign-up-email-requirements\"", signUp);
        Assert.Contains("id=\"sign-up-password-requirements\"", signUp);
        Assert.Contains("aria-describedby=\"sign-up-email-requirements\"", signUp);
        Assert.Contains("aria-describedby=\"sign-up-password-requirements\"", signUp);
        Assert.Contains("minlength=\"6\"", signUp);
        Assert.DoesNotContain("minlength=\"12\"", signUp);

        Assert.Contains("id=\"reset-password-requirements\"", resetPassword);
        Assert.Contains("aria-describedby=\"reset-password-requirements\"", resetPassword);
        Assert.Contains("minlength=\"6\"", resetPassword);
        Assert.DoesNotContain("minlength=\"12\"", resetPassword);

        Assert.Contains(".auth-field-help", styles);
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
        Assert.Contains("QuoteNewUrl => $\"{QuoteEngineUrl}/projects/new\"", content);
        Assert.DoesNotContain("https://quote.maliev.com/quotes/new", content);
        Assert.Contains("SiteContent.QuoteNewUrl", shop);
        Assert.Contains("SiteContent.QuoteNewUrl", product);
        Assert.Contains("SiteContent.QuoteNewUrl", error);
        Assert.DoesNotContain("href=\"/quote\"", shop);
        Assert.DoesNotContain("href=\"/quote\"", product);
        Assert.DoesNotContain("href=\"/quote\"", error);
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
        Assert.Contains("premultipliedAlpha: false", source);
        Assert.Contains("renderRatio", source);
        Assert.Contains("setHardwareScalingLevel", source);
        Assert.Contains("engine.setHardwareScalingLevel(1 / renderRatio)", source);
        Assert.Contains("ResizeObserver", source);
        Assert.Contains("powerPreference: \"low-power\"", source);

        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");
        Assert.Contains(".manufacturing-gizmo--landing", styles);
        Assert.Contains("overflow: visible", styles);
        Assert.DoesNotContain(".manufacturing-gizmo--landing::before", styles);
        Assert.Contains("background: transparent", styles);
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

        Assert.Contains("DisplayScale", catalog);
        Assert.Contains("\"3d-printing-part-02\"", catalog);
        Assert.Contains("1.72", catalog);
        Assert.Contains("ModelScale", component);
        Assert.Contains("data-model-scale=\"@ModelScale.ToString(CultureInfo.InvariantCulture)\"", component);
        Assert.Contains("babylonjs-loaders@9.6.0", source);
        Assert.Contains("createLandingHeroScene", source);
        Assert.Contains("configureLandingHeroCamera", source);
        Assert.Contains("state.landingFrame = frameImportedModel", source);
        Assert.Contains("frameLandingHeroCamera", source);
        Assert.Contains("measureProjectedMeshFrame", source);
        Assert.Contains("targetFill", source);
        Assert.Contains("safeInset", source);
        Assert.Contains("camera.getViewMatrix(true)", source);
        Assert.Contains("refreshCameraMatrices", source);
        Assert.Contains("camera.getProjectionMatrix?.(true)", source);
        Assert.Contains("projectedFrameFits", source);
        Assert.Contains("high *= 1.24", source);
        Assert.Contains("getModelAwareHeroMetrics", source);
        Assert.Contains("radiusFloor", source);
        Assert.Contains("camera.upperRadiusLimit = null", source);
        Assert.Contains("updateWorldMatrixChain", source);
        Assert.Contains("state.engine.resize();\n  state.cameraConfigurator?.();", source);
        Assert.Contains("addHoverMotion", source);
        Assert.Contains("modelScale", source);
        Assert.Contains("const targetSize = 2.28 * modelScale", source);
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
        Assert.Contains("const baseRotation = new BABYLON.Vector3(0.06, -0.36, 0.02)", source);
        Assert.Contains("Math.sin(elapsed * 0.00055) * 0.025", source);
        Assert.DoesNotContain("Math.sin(elapsed * 0.0012) * 0.055", source);
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
        Assert.Contains("grid-template-columns: minmax(0, 1.08fr) minmax(320px, .92fr);", styles);
        Assert.Contains("text-wrap: balance;", styles);
        Assert.Contains("@media (min-width: 681px) and (max-width: 960px)", styles);
        Assert.Contains("text-align: center;", styles);
        Assert.Contains("gap: 18px;", styles);
        Assert.Contains("margin-top: 22px;", styles);
        Assert.Contains("width: min(100%, 820px);", styles);
        Assert.Contains("min-height: 440px;", styles);
        Assert.Contains("height: min(46vh, 470px);", styles);
        Assert.Contains("@media (max-width: 680px)", styles);
        Assert.Contains(".landing-hero {\n    grid-template-columns: minmax(0, 1fr);\n    grid-template-areas:\n      \"visual\"\n      \"copy\";", styles);
        Assert.Contains("align-content: space-between;", styles);
        Assert.Contains("min-height: calc(100svh - var(--site-header-height));", styles);
        Assert.Contains(".landing-hero-copy {\n    grid-area: copy;\n    max-width: none;", styles);
        Assert.Contains(".landing-hero-visual {\n    grid-area: visual;\n    justify-self: center;\n    width: min(100%, 560px);\n    min-height: 320px;", styles);
        Assert.Contains(".metric-strip {\n    justify-content: center;\n    text-align: center;", styles);
        Assert.Contains(".metric-strip div {\n    justify-items: center;", styles);
        Assert.Contains("@media (min-width: 1600px)", styles);
        Assert.Contains("height: min(60vh, 760px);", styles);
        Assert.Contains("@media (min-width: 2400px)", styles);
        Assert.Contains("height: min(58vh, 860px);", styles);
        Assert.Contains("justify-content: center;", styles);
        Assert.Contains("const narrowTall = width < 700 && height >= 500 && aspect < 1.12;", gizmo);
        Assert.Contains("narrowTall ? 0.72 : compact ? 0.76", gizmo);
        Assert.Contains("safeInset: narrowTall ? 0.12 : compact ? 0.07 : 0.055", gizmo);
        Assert.Contains("fallbackRadius: narrowTall ? 7.15", gizmo);
        Assert.Contains("const balancedTablet = width >= 640 && width <= 920 && height >= 460;", gizmo);
        Assert.Contains("balancedTablet ? 0.46 : wide ? 0.43 : 0.45", gizmo);
        Assert.Contains("frameLandingHeroCamera", gizmo);
        Assert.Contains("measureProjectedMeshFrame", gizmo);
        Assert.Contains("projectedFrameFits", gizmo);
        Assert.Contains("high *= 1.24", gizmo);
    }

    /// <summary>
    /// Verifies the 3D canvas does not show a browser focus outline when clicked.
    /// </summary>
    [Fact]
    public void ManufacturingGizmoCanvasDoesNotExposeFocusRing()
    {
        var component = ReadRepoFile("Maliev.Web.Client", "Components", "Quote", "ManufacturingGizmo.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("ModulePath = \"/js/manufacturing-gizmo.js?v=20260517-hero-fit\"", component);
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
