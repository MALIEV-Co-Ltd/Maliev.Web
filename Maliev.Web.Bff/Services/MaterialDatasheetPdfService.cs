using Maliev.Web.Bff.Clients;
using Maliev.Web.Client.Content;
using Maliev.Web.Shared.Localization;

namespace Maliev.Web.Bff.Services;

/// <summary>
/// Builds public material datasheet PDF requests and delegates rendering to PdfService.
/// </summary>
public sealed class MaterialDatasheetPdfService(IWebHostEnvironment environment, IPdfServiceClient pdfServiceClient)
{
    private const string PublicSiteBaseUrl = "https://www.maliev.com";

    private readonly string _webRootPath = string.IsNullOrWhiteSpace(environment.WebRootPath)
        ? Path.Combine(environment.ContentRootPath, "wwwroot")
        : environment.WebRootPath;

    internal async Task<byte[]> GenerateAsync(SiteContent.MaterialContent material, string cultureName, CancellationToken cancellationToken)
    {
        var culture = SupportedCultures.Normalize(cultureName);

        var specs = material.Specs
            .Select(spec => new MaterialDatasheetPdfSpec
            {
                Label = spec.Label.For(culture),
                Value = spec.Value,
                Note = spec.Note?.For(culture) ?? string.Empty
            })
            .ToList();

        if (material.Standards is { Count: > 0 } standards)
        {
            specs.Insert(0, new MaterialDatasheetPdfSpec
            {
                Label = SiteContent.Text("Standard equivalents", "มาตรฐานเทียบเท่า").For(culture),
                Value = string.Join(" · ", standards)
            });
        }

        var bands = new List<MaterialDatasheetPdfBand>
        {
            Band("Best fit", "เหมาะกับ", material.BestFor, culture),
            Band("Mechanical", "เชิงกล", material.Mechanical, culture),
            Band("Heat", "ความร้อน", material.Heat, culture),
            Band("Chemical", "สารเคมี", material.Chemical, culture),
            Band("Finish", "ผิวงาน", material.Finish, culture)
        };

        var request = new MaterialDatasheetPdfRequest
        {
            Slug = material.Slug,
            CultureName = culture,
            Name = material.Name,
            CategoryLabel = SiteContent.FindMaterialCategory(material.CategoryKey)?.Title.For(culture)
                ?? material.Process.For(culture),
            ProcessLabel = material.Process.For(culture),
            Family = material.Family.For(culture),
            PublicUrl = $"{PublicSiteBaseUrl}/materials/{Uri.EscapeDataString(material.Slug)}",
            Disclaimer = SiteContent.MaterialDatasheetDisclaimer.For(culture),
            CoverImage = MapImage(material.ImageUrl, material.ImageAlt.For(culture), material.Family.For(culture), cancellationToken),
            Specs = specs,
            Bands = bands,
            Pros = material.Pros.For(culture),
            Cons = material.Cons.For(culture)
        };

        return await pdfServiceClient.RenderMaterialDatasheetAsync(request, cancellationToken);
    }

    private static MaterialDatasheetPdfBand Band(string en, string th, LocalizedText value, string culture) =>
        new() { Label = SiteContent.Text(en, th).For(culture), Value = value.For(culture) };

    private MaterialDatasheetPdfImage? MapImage(string url, string alt, string caption, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        var imagePath = ResolveImagePath(url);
        if (imagePath is null)
        {
            // Remote/unresolved images are not embedded; the datasheet renders text-only.
            return null;
        }

        var bytes = File.ReadAllBytes(imagePath);
        if (bytes.Length == 0)
        {
            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();

        return new MaterialDatasheetPdfImage
        {
            Url = url,
            Alt = alt,
            Caption = caption,
            ContentType = ResolveContentType(imagePath),
            Bytes = bytes
        };
    }

    private string? ResolveImagePath(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            return null;
        }

        var pathOnly = url.Split('?', '#')[0].TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var imagePath = Path.Combine(_webRootPath, pathOnly);
        if (File.Exists(imagePath))
        {
            return imagePath;
        }

        return ResolveContentRootImagePath(pathOnly);
    }

    private static string? ResolveContentRootImagePath(string pathOnly)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "Maliev.Web.Bff", "wwwroot", pathOnly);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static string ResolveContentType(string imagePath)
    {
        return Path.GetExtension(imagePath).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            _ => "image/jpeg"
        };
    }
}
