namespace Maliev.Web.Tests;

/// <summary>
/// Source-level contract tests for the material datasheet PDF pipeline (catalog → detail page → BFF → PdfService).
/// </summary>
public sealed class MaterialDatasheetPdfSourceTests
{
    /// <summary>
    /// Verifies the datasheet PDF is rendered by the downstream PdfService, not in the Web BFF.
    /// </summary>
    [Fact]
    public void MaterialDatasheetPdfIsRenderedByPdfServiceNotWebBff()
    {
        var service = ReadRepoFile("Maliev.Web.Bff", "Services", "MaterialDatasheetPdfService.cs");
        var client = ReadRepoFile("Maliev.Web.Bff", "Clients", "PdfServiceClient.cs");
        var program = ReadRepoFile("Maliev.Web.Bff", "Program.cs");
        var project = ReadRepoFile("Maliev.Web.Bff", "Maliev.Web.Bff.csproj");

        Assert.Contains("IPdfServiceClient", service);
        Assert.Contains("RenderMaterialDatasheetAsync", service);
        Assert.Contains("Task<byte[]> RenderMaterialDatasheetAsync(MaterialDatasheetPdfRequest request, CancellationToken cancellationToken)", client);
        Assert.Contains("/pdf/v1/material-datasheets/render", client);
        Assert.Contains("builder.Services.AddScoped<MaterialDatasheetPdfService>();", program);
        Assert.Contains("builder.AddAuthenticatedServiceClient<IPdfServiceClient, PdfServiceClient>(\"PdfService\")", program);
        Assert.DoesNotContain("using QuestPDF", service);
    }

    /// <summary>
    /// Verifies the BFF maps localized catalog content into the PdfService datasheet request.
    /// </summary>
    [Fact]
    public void MaterialDatasheetPdfMapsLocalizedCatalogContent()
    {
        var service = ReadRepoFile("Maliev.Web.Bff", "Services", "MaterialDatasheetPdfService.cs");

        Assert.Contains("Name = material.Name", service);
        Assert.Contains("ProcessLabel = material.Process.For(culture)", service);
        Assert.Contains("Family = material.Family.For(culture)", service);
        Assert.Contains("PublicUrl = $\"{PublicSiteBaseUrl}/materials/{Uri.EscapeDataString(material.Slug)}\"", service);
        Assert.Contains("Disclaimer = SiteContent.MaterialDatasheetDisclaimer.For(culture)", service);
        Assert.Contains("material.Specs", service);
        Assert.Contains("Band(\"Best fit\"", service);
        Assert.Contains("Pros = material.Pros.For(culture)", service);
        Assert.Contains("Cons = material.Cons.For(culture)", service);
    }

    /// <summary>
    /// Verifies the public datasheet endpoint is anonymous, slug-routed, and degrades to 503 when PdfService is down.
    /// </summary>
    [Fact]
    public void MaterialDatasheetEndpointIsAnonymousSlugRoutedAndResilient()
    {
        var controller = ReadRepoFile("Maliev.Web.Bff", "Controllers", "MaterialsController.cs");

        Assert.Contains("[Route(\"web/v{version:apiVersion}/materials\")]", controller);
        Assert.Contains("[AllowAnonymous]", controller);
        Assert.Contains("[HttpGet(\"{slug}/datasheet.pdf\")]", controller);
        Assert.Contains("[Produces(\"application/pdf\")]", controller);
        Assert.Contains("SiteContent.FindMaterial(slug)", controller);
        Assert.Contains("return NotFound();", controller);
        Assert.Contains("SiteContent.BuildMaterialDatasheetFileName(material)", controller);
        Assert.Contains("catch (BackendUnavailableException)", controller);
        Assert.Contains("StatusCodes.Status503ServiceUnavailable", controller);
    }

    /// <summary>
    /// Verifies the catalog exposes a slug-addressable material library with datasheet helpers and same-category similarity.
    /// </summary>
    [Fact]
    public void MaterialCatalogExposesSlugsSpecsAndDatasheetHelpers()
    {
        var content = ReadRepoFile("Maliev.Web.Client", "Content", "SiteContent.cs");
        var detail = ReadRepoFile("Maliev.Web.Client", "Pages", "MaterialDetail.razor");

        Assert.Contains("IReadOnlyList<MaterialContent> Materials", content);
        Assert.Contains("internal sealed record MaterialContent(", content);
        Assert.Contains("internal sealed record MaterialSpec(LocalizedText Label, string Value, LocalizedText? Note = null);", content);
        Assert.Contains("MaterialsInCategory(string categoryKey)", content);
        Assert.Contains("SimilarMaterials(MaterialContent material)", content);
        Assert.Contains("candidate.CategoryKey == material.CategoryKey", content);
        Assert.Contains("BuildMaterialDatasheetHref(MaterialContent material, string cultureName)", content);
        Assert.Contains("/web/v1/materials/{Uri.EscapeDataString(material.Slug)}/datasheet.pdf?culture=", content);
        Assert.Contains("BuildMaterialDatasheetFileName(MaterialContent material)", content);
        Assert.Contains("MaterialDatasheetDisclaimer", content);
        Assert.Contains("\"pa12-nylon\"", content);
        Assert.Contains("\"stainless-sus304\"", content);
        // CNC metals carry JIS-primary naming with cross-standard equivalents.
        Assert.Contains("IReadOnlyList<string>? Standards", content);
        Assert.Contains("\"JIS SUS304\"", content);
        Assert.Contains("\"AISI 1045\"", content);

        Assert.Contains("@page \"/materials/{Slug}\"", detail);
        Assert.Contains("SiteContent.BuildMaterialDatasheetHref(Material, Preferences.Culture)", detail);
        Assert.Contains("malievBlog.downloadFile", detail);
        Assert.Contains("DownloadDatasheetAsync", detail);
    }

    /// <summary>
    /// Verifies the materials overview is a searchable, horizontal-card list grouped by category.
    /// </summary>
    [Fact]
    public void MaterialsOverviewIsSearchableHorizontalCardList()
    {
        var overview = ReadRepoFile("Maliev.Web.Client", "Pages", "Materials.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("@page \"/materials\"", overview);
        Assert.Contains("SiteContent.MaterialCategoryList", overview);
        Assert.Contains("VisibleMaterials(category.Key)", overview);
        Assert.Contains("href=\"/materials/@material.Slug\"", overview);

        // Live text search filters as the customer types.
        Assert.Contains("class=\"material-overview-search\"", overview);
        Assert.Contains("@oninput=\"OnSearchInput\"", overview);
        Assert.Contains("private bool MatchesSearch(SiteContent.MaterialContent material)", overview);
        Assert.Contains("material.Standards?.Any(Contains)", overview);
        Assert.Contains("material-overview-empty", overview);

        // Cards are horizontal: thumbnail column on the left, info on the right.
        Assert.Contains("material-overview-card-head", overview);
        Assert.Contains("material-overview-card-media", overview);
        Assert.Contains(".material-overview-card {\n  display: grid;\n  grid-template-columns: 132px minmax(0, 1fr);", styles);
        Assert.Contains(".material-overview-search", styles);
        Assert.Contains(".material-overview-search-input", styles);
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
