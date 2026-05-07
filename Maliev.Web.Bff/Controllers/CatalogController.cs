using Asp.Versioning;
using Maliev.Web.Bff.Services;
using Maliev.Web.Shared.Commerce;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.Web.Bff.Controllers;

/// <summary>
/// Customer-facing product catalog API.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("web/v{version:apiVersion}/catalog")]
[AllowAnonymous]
public sealed class CatalogController(ICommerceCatalogService catalogService) : ControllerBase
{
    /// <summary>Gets product collections from the catalog source.</summary>
    [HttpGet("collections")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductCollectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetCollections(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await catalogService.GetCollectionsAsync(cancellationToken));
        }
        catch (BackendUnavailableException ex)
        {
            return BackendUnavailable(ex);
        }
    }

    /// <summary>Gets product cards from the catalog source.</summary>
    [HttpGet("products")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetProducts([FromQuery] string? collection, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await catalogService.GetProductsAsync(collection, cancellationToken));
        }
        catch (BackendUnavailableException ex)
        {
            return BackendUnavailable(ex);
        }
    }

    /// <summary>Gets a product detail by canonical handle.</summary>
    [HttpGet("products/{handle}")]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetProduct(string handle, CancellationToken cancellationToken)
    {
        try
        {
            var product = await catalogService.GetProductAsync(handle, cancellationToken);
            return product is null ? NotFound() : Ok(product);
        }
        catch (BackendUnavailableException ex)
        {
            return BackendUnavailable(ex);
        }
    }

    private ObjectResult BackendUnavailable(BackendUnavailableException _)
    {
        return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
        {
            Title = "Shop is temporarily unavailable",
            Detail = "We could not load product listings right now. Please refresh the page or contact MALIEV.",
            Status = StatusCodes.Status503ServiceUnavailable
        });
    }
}
