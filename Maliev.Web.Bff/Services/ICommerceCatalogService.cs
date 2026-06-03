using Maliev.Web.Shared.Commerce;

namespace Maliev.Web.Bff.Services;

/// <summary>
/// Provides customer-visible shop catalog data.
/// </summary>
public interface ICommerceCatalogService
{
    /// <summary>Gets customer-visible product collections.</summary>
    Task<IReadOnlyList<ProductCollectionDto>> GetCollectionsAsync(CancellationToken cancellationToken);

    /// <summary>Gets a redirect URL for a published collection image.</summary>
    Task<string?> GetCollectionImageRedirectUrlAsync(string collectionSlug, CancellationToken cancellationToken);

    /// <summary>Gets customer-visible product cards.</summary>
    Task<IReadOnlyList<ProductSummaryDto>> GetProductsAsync(string? collectionSlug, CancellationToken cancellationToken);

    /// <summary>Gets a product detail by canonical handle.</summary>
    Task<ProductDetailDto?> GetProductAsync(string handle, CancellationToken cancellationToken);

    /// <summary>Gets a redirect URL for a product media upload reference.</summary>
    Task<string?> GetProductMediaRedirectUrlAsync(string uploadId, CancellationToken cancellationToken);
}
