using Maliev.Web.Bff.Clients;
using Maliev.Web.Client.Content;
using Maliev.Web.Shared.Localization;

namespace Maliev.Web.Bff.Services;

/// <summary>
/// Builds public practical note PDF requests and delegates rendering to PdfService.
/// </summary>
public sealed class BlogEbookPdfService(IWebHostEnvironment environment, IPdfServiceClient pdfServiceClient)
{
    private const string PublicSiteBaseUrl = "https://www.maliev.com";

    private readonly string _webRootPath = string.IsNullOrWhiteSpace(environment.WebRootPath)
        ? Path.Combine(environment.ContentRootPath, "wwwroot")
        : environment.WebRootPath;

    internal async Task<byte[]> GenerateAsync(BlogPostContent post, string cultureName, CancellationToken cancellationToken)
    {
        var normalizedCulture = SupportedCultures.Normalize(cultureName);
        var sections = new List<BlogPracticalNotePdfSection>(post.Sections.Count);
        foreach (var section in post.Sections)
        {
            sections.Add(new BlogPracticalNotePdfSection
            {
                Title = section.Title.For(normalizedCulture),
                Body = section.Body.For(normalizedCulture),
                Items = section.Items.Select(item => item.For(normalizedCulture)).ToList(),
                Image = section.Image is null ? null : await MapImage(section.Image.Url, section.Image.Alt.For(normalizedCulture), section.Image.Caption.For(normalizedCulture), cancellationToken)
            });
        }

        var request = new BlogPracticalNotePdfRequest
        {
            Slug = post.Slug,
            CultureName = normalizedCulture,
            Title = post.Title.For(normalizedCulture),
            Summary = post.Summary.For(normalizedCulture),
            Category = post.Category.For(normalizedCulture),
            PublicUrl = $"{PublicSiteBaseUrl}/blog/{Uri.EscapeDataString(post.Slug)}",
            CoverImage = await MapImage(post.ImageUrl, post.Title.For(normalizedCulture), post.Summary.For(normalizedCulture), cancellationToken),
            Sections = sections,
            Takeaways = post.Takeaways.Select(item => item.For(normalizedCulture)).ToList()
        };

        return await pdfServiceClient.RenderBlogPracticalNoteAsync(request, cancellationToken);
    }

    private async Task<BlogPracticalNotePdfImage?> MapImage(string url, string alt, string caption, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        var imagePath = ResolveImagePath(url);

        return new BlogPracticalNotePdfImage
        {
            Url = url,
            Alt = alt,
            Caption = caption,
            ContentType = imagePath is null ? ResolveContentType(url) : ResolveContentType(imagePath),
            Bytes = imagePath is null ? [] : await File.ReadAllBytesAsync(imagePath, cancellationToken)
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

    private string? ResolveContentRootImagePath(string pathOnly)
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
