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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generated PDF file.</returns>
    [HttpGet("{slug}/ebook.pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetEbook(string slug, [FromQuery] string? culture, CancellationToken cancellationToken)
    {
        var post = SiteContent.BlogPosts.FirstOrDefault(item => item.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
        if (post is null)
        {
            return NotFound();
        }

        try
        {
            var normalizedCulture = SupportedCultures.Normalize(culture);
            var pdfBytes = await pdfService.GenerateAsync(post, normalizedCulture, cancellationToken);
            return File(pdfBytes, "application/pdf", SiteContent.BuildBlogPdfFileName(post));
        }
        catch (BackendUnavailableException)
        {
            return Problem(
                title: "Practical note PDF is temporarily unavailable",
                detail: "We could not render this practical note PDF right now. Please refresh the page or contact MALIEV.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}
