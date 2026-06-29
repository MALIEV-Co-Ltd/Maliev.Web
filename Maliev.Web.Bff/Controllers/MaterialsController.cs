using Asp.Versioning;
using Maliev.Web.Bff.Services;
using Maliev.Web.Client.Content;
using Maliev.Web.Shared.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.Web.Bff.Controllers;

/// <summary>
/// Customer-facing material datasheet downloads.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("web/v{version:apiVersion}/materials")]
[AllowAnonymous]
public sealed class MaterialsController(MaterialDatasheetPdfService pdfService) : ControllerBase
{
    /// <summary>
    /// Generates a professionally formatted material datasheet PDF.
    /// </summary>
    /// <param name="slug">The material slug.</param>
    /// <param name="culture">The requested culture.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generated PDF file.</returns>
    [HttpGet("{slug}/datasheet.pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetDatasheet(string slug, [FromQuery] string? culture, CancellationToken cancellationToken)
    {
        var material = SiteContent.FindMaterial(slug);
        if (material is null)
        {
            return NotFound();
        }

        try
        {
            var normalizedCulture = SupportedCultures.Normalize(culture);
            var pdfBytes = await pdfService.GenerateAsync(material, normalizedCulture, cancellationToken);
            return File(pdfBytes, "application/pdf", SiteContent.BuildMaterialDatasheetFileName(material));
        }
        catch (BackendUnavailableException)
        {
            return Problem(
                title: "Material datasheet PDF is temporarily unavailable",
                detail: "We could not render this material datasheet right now. Please refresh the page or contact MALIEV.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}
