using Maliev.Web.Shared.Commerce;

namespace Maliev.Web.Bff.Services;

/// <summary>
/// Provides customer-visible shop catalog data from the configured catalog backend.
/// </summary>
public interface ICommerceCatalogService
{
    /// <summary>Gets customer-visible product collections.</summary>
    Task<IReadOnlyList<ProductCollectionDto>> GetCollectionsAsync(CancellationToken cancellationToken);

    /// <summary>Gets customer-visible product cards.</summary>
    Task<IReadOnlyList<ProductSummaryDto>> GetProductsAsync(string? collectionSlug, CancellationToken cancellationToken);

    /// <summary>Gets a product detail by canonical handle.</summary>
    Task<ProductDetailDto?> GetProductAsync(string handle, CancellationToken cancellationToken);

    /// <summary>Gets Shopify import readiness using live Admin API data.</summary>
    Task<ShopifyImportPreviewDto> GetImportPreviewAsync(CancellationToken cancellationToken);
}
