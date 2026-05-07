using Maliev.Web.Bff.Clients;
using Maliev.Web.Shared.Commerce;

namespace Maliev.Web.Bff.Services;

internal sealed class ShopifyCommerceCatalogService(IShopifyAdminCatalogClient shopifyClient) : ICommerceCatalogService
{
    public async Task<IReadOnlyList<ProductCollectionDto>> GetCollectionsAsync(CancellationToken cancellationToken)
    {
        var snapshot = await shopifyClient.GetCatalogAsync(cancellationToken);
        return snapshot.Collections;
    }

    public async Task<IReadOnlyList<ProductSummaryDto>> GetProductsAsync(string? collectionSlug, CancellationToken cancellationToken)
    {
        var snapshot = await shopifyClient.GetCatalogAsync(cancellationToken);
        var products = snapshot.Products.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(collectionSlug))
        {
            products = products.Where(product => product.CollectionSlug.Equals(collectionSlug, StringComparison.OrdinalIgnoreCase));
        }

        return products
            .OrderBy(product => product.Title.En, StringComparer.OrdinalIgnoreCase)
            .Cast<ProductSummaryDto>()
            .ToList();
    }

    public async Task<ProductDetailDto?> GetProductAsync(string handle, CancellationToken cancellationToken)
    {
        var snapshot = await shopifyClient.GetCatalogAsync(cancellationToken);
        return snapshot.Products.FirstOrDefault(product => product.Handle.Equals(handle, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<ShopifyImportPreviewDto> GetImportPreviewAsync(CancellationToken cancellationToken)
    {
        var snapshot = await shopifyClient.GetCatalogAsync(cancellationToken);
        var products = snapshot.Products
            .OrderBy(product => product.Title.En, StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .Cast<ProductSummaryDto>()
            .ToList();

        return new ShopifyImportPreviewDto
        {
            SourceStorefrontUrl = "https://shop.maliev.com/collections/all",
            ExpectedProductCount = snapshot.Products.Count,
            HighestObservedPriceThb = snapshot.Products.Select(product => product.PriceThb).DefaultIfEmpty().Max(),
            ObservedProducts = products,
            Notes =
            [
                "Catalog data is read from Shopify Admin GraphQL. Configure Shopify:ShopDomain and Shopify:AdminAccessToken in the BFF.",
                "Old Shopify handles remain the canonical product handles for redirect preservation."
            ]
        };
    }
}
