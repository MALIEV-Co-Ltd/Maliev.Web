using Asp.Versioning;
using Maliev.Web.Bff.Services;
using Maliev.Web.Client.Content;
using Maliev.Web.Shared.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.Web.Bff.Controllers;

/// <summary>
/// Customer-facing practical note downloads.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("web/v{version:apiVersion}/blog")]
[AllowAnonymous]
public sealed class BlogController(BlogEbookPdfService pdfService) : ControllerBase
{
    /// <summary>
    /// Generates a professionally formatted practical note PDF booklet.
    /// </summary>
    /// <param name="slug">The practical note slug.</param>
    /// <param name="culture">The requested culture.</param>
    /// <returns>The generated PDF file.</returns>
    [HttpGet("{slug}/ebook.pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetEbook(string slug, [FromQuery] string? culture)
    {
        var post = SiteContent.BlogPosts.FirstOrDefault(item => item.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
        if (post is null)
        {
            return NotFound();
        }

        var normalizedCulture = SupportedCultures.Normalize(culture);
        var pdfBytes = pdfService.Generate(post, normalizedCulture);
        return File(pdfBytes, "application/pdf", $"{SanitizeFileName(post.Slug)}-maliev-practical-note.pdf");
    }

    private static string SanitizeFileName(string value)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars();
        var safe = new string(value.Select(character => invalidCharacters.Contains(character) ? '-' : character).ToArray());
        return string.IsNullOrWhiteSpace(safe) ? "maliev-practical-note" : safe;
    }
}
