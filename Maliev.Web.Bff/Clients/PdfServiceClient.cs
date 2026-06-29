using System.Net.Http.Json;
using Maliev.Web.Bff.Services;

namespace Maliev.Web.Bff.Clients;

/// <summary>
/// Downstream PdfService client used by the Web BFF for public PDF downloads.
/// </summary>
public interface IPdfServiceClient
{
    /// <summary>
    /// Renders a blog practical note PDF through PdfService.
    /// </summary>
    /// <param name="request">The practical note render request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The rendered PDF bytes.</returns>
    Task<byte[]> RenderBlogPracticalNoteAsync(BlogPracticalNotePdfRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Renders a material datasheet PDF through PdfService.
    /// </summary>
    /// <param name="request">The material datasheet render request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The rendered PDF bytes.</returns>
    Task<byte[]> RenderMaterialDatasheetAsync(MaterialDatasheetPdfRequest request, CancellationToken cancellationToken);
}

internal sealed class PdfServiceClient(HttpClient httpClient, ILogger<PdfServiceClient> logger) : IPdfServiceClient
{
    public async Task<byte[]> RenderBlogPracticalNoteAsync(BlogPracticalNotePdfRequest request, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync("/pdf/v1/blog-practical-notes/render", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new BackendUnavailableException("PdfService", $"PdfService returned {(int)response.StatusCode} while rendering a practical note PDF.");
            }

            var mediaType = response.Content.Headers.ContentType?.MediaType;
            if (!string.Equals(mediaType, "application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                throw new BackendUnavailableException("PdfService", "PdfService returned a non-PDF practical note response.");
            }

            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
        catch (BackendUnavailableException)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or TaskCanceledException)
        {
            logger.LogWarning(ex, "PdfService failed while rendering blog practical note PDF");
            throw new BackendUnavailableException("PdfService", "PdfService is unavailable while rendering the practical note PDF.", ex);
        }
    }

    public async Task<byte[]> RenderMaterialDatasheetAsync(MaterialDatasheetPdfRequest request, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync("/pdf/v1/material-datasheets/render", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new BackendUnavailableException("PdfService", $"PdfService returned {(int)response.StatusCode} while rendering a material datasheet PDF.");
            }

            var mediaType = response.Content.Headers.ContentType?.MediaType;
            if (!string.Equals(mediaType, "application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                throw new BackendUnavailableException("PdfService", "PdfService returned a non-PDF material datasheet response.");
            }

            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
        catch (BackendUnavailableException)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or TaskCanceledException)
        {
            logger.LogWarning(ex, "PdfService failed while rendering material datasheet PDF");
            throw new BackendUnavailableException("PdfService", "PdfService is unavailable while rendering the material datasheet PDF.", ex);
        }
    }
}

/// <summary>
/// Web-to-PdfService request for a blog practical note PDF.
/// </summary>
public sealed record BlogPracticalNotePdfRequest
{
    /// <summary>Gets the source blog post slug.</summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>Gets the requested culture name.</summary>
    public string CultureName { get; init; } = "en";

    /// <summary>Gets the localized title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the localized summary.</summary>
    public string Summary { get; init; } = string.Empty;

    /// <summary>Gets the localized category.</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Gets the canonical public HTTPS URL.</summary>
    public string PublicUrl { get; init; } = string.Empty;

    /// <summary>Gets the optional cover image.</summary>
    public BlogPracticalNotePdfImage? CoverImage { get; init; }

    /// <summary>Gets the article sections.</summary>
    public IReadOnlyList<BlogPracticalNotePdfSection> Sections { get; init; } = [];

    /// <summary>Gets the takeaway checklist items.</summary>
    public IReadOnlyList<string> Takeaways { get; init; } = [];
}

/// <summary>
/// Web-to-PdfService request section for a blog practical note PDF.
/// </summary>
public sealed record BlogPracticalNotePdfSection
{
    /// <summary>Gets the localized section title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the localized section body.</summary>
    public string Body { get; init; } = string.Empty;

    /// <summary>Gets the localized section points.</summary>
    public IReadOnlyList<string> Items { get; init; } = [];

    /// <summary>Gets the optional section image.</summary>
    public BlogPracticalNotePdfImage? Image { get; init; }
}

/// <summary>
/// Web-to-PdfService embedded image for a blog practical note PDF.
/// </summary>
public sealed record BlogPracticalNotePdfImage
{
    /// <summary>Gets the source URL or path used by the Web content catalog.</summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>Gets the localized image alternative text.</summary>
    public string Alt { get; init; } = string.Empty;

    /// <summary>Gets the localized image caption.</summary>
    public string Caption { get; init; } = string.Empty;

    /// <summary>Gets the image content type.</summary>
    public string ContentType { get; init; } = "image/jpeg";

    /// <summary>Gets the raw image bytes.</summary>
    public byte[] Bytes { get; init; } = [];
}

/// <summary>
/// Web-to-PdfService request for a material datasheet PDF.
/// </summary>
public sealed record MaterialDatasheetPdfRequest
{
    /// <summary>Gets the source material slug.</summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>Gets the requested culture name.</summary>
    public string CultureName { get; init; } = "en";

    /// <summary>Gets the localized material name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the localized category label.</summary>
    public string CategoryLabel { get; init; } = string.Empty;

    /// <summary>Gets the process label (e.g. CNC, MJF / SLS).</summary>
    public string ProcessLabel { get; init; } = string.Empty;

    /// <summary>Gets the localized material family/subtitle.</summary>
    public string Family { get; init; } = string.Empty;

    /// <summary>Gets the canonical public HTTPS URL.</summary>
    public string PublicUrl { get; init; } = string.Empty;

    /// <summary>Gets the localized typical-values disclaimer.</summary>
    public string Disclaimer { get; init; } = string.Empty;

    /// <summary>Gets the optional cover image.</summary>
    public MaterialDatasheetPdfImage? CoverImage { get; init; }

    /// <summary>Gets the numeric/typical specification rows.</summary>
    public IReadOnlyList<MaterialDatasheetPdfSpec> Specs { get; init; } = [];

    /// <summary>Gets the qualitative selection-guidance rows.</summary>
    public IReadOnlyList<MaterialDatasheetPdfBand> Bands { get; init; } = [];

    /// <summary>Gets the localized advantage copy.</summary>
    public string Pros { get; init; } = string.Empty;

    /// <summary>Gets the localized check-before-use copy.</summary>
    public string Cons { get; init; } = string.Empty;
}

/// <summary>
/// Web-to-PdfService specification row for a material datasheet PDF.
/// </summary>
public sealed record MaterialDatasheetPdfSpec
{
    /// <summary>Gets the localized property label.</summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>Gets the property value with units.</summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>Gets the optional localized note.</summary>
    public string Note { get; init; } = string.Empty;
}

/// <summary>
/// Web-to-PdfService selection-guidance row for a material datasheet PDF.
/// </summary>
public sealed record MaterialDatasheetPdfBand
{
    /// <summary>Gets the localized band label.</summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>Gets the localized band value.</summary>
    public string Value { get; init; } = string.Empty;
}

/// <summary>
/// Web-to-PdfService embedded image for a material datasheet PDF.
/// </summary>
public sealed record MaterialDatasheetPdfImage
{
    /// <summary>Gets the source URL or path used by the Web content catalog.</summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>Gets the localized image alternative text.</summary>
    public string Alt { get; init; } = string.Empty;

    /// <summary>Gets the localized image caption.</summary>
    public string Caption { get; init; } = string.Empty;

    /// <summary>Gets the image content type.</summary>
    public string ContentType { get; init; } = "image/jpeg";

    /// <summary>Gets the raw image bytes.</summary>
    public byte[] Bytes { get; init; } = [];
}
