using Asp.Versioning;
using Maliev.Web.Bff.Services;
using Maliev.Web.Shared.Commerce;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.Web.Bff.Controllers;

/// <summary>
/// Shopify migration readiness API.
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("web/v{version:apiVersion}/shopify")]
[AllowAnonymous]
public sealed class ShopifyController(ICommerceCatalogService catalogService) : ControllerBase
{
    /// <summary>Gets migration readiness based on live Shopify Admin API data.</summary>
    [HttpGet("import-preview")]
    [ProducesResponseType(typeof(ShopifyImportPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetImportPreview(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await catalogService.GetImportPreviewAsync(cancellationToken));
        }
        catch (BackendUnavailableException ex)
        {
            var problem = new ProblemDetails
            {
                Title = "Shopify backend unavailable",
                Detail = ex.Message,
                Status = StatusCodes.Status503ServiceUnavailable
            };
            problem.Extensions["backend"] = ex.BackendName;
            return StatusCode(StatusCodes.Status503ServiceUnavailable, problem);
        }
    }
}
