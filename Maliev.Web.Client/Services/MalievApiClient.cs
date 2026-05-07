using System.Net.Http.Json;
using Maliev.Web.Shared.Commerce;
using Maliev.Web.Shared.Quotes;

namespace Maliev.Web.Client.Services;

internal sealed class MalievApiClient(HttpClient httpClient)
{
    internal async Task<IReadOnlyList<ProductCollectionDto>> GetCollectionsAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<ProductCollectionDto>>("web/v1/catalog/collections", cancellationToken) ?? [];
    }

    internal async Task<IReadOnlyList<ProductSummaryDto>> GetProductsAsync(string? collection = null, CancellationToken cancellationToken = default)
    {
        var path = string.IsNullOrWhiteSpace(collection)
            ? "web/v1/catalog/products"
            : $"web/v1/catalog/products?collection={Uri.EscapeDataString(collection)}";
        return await httpClient.GetFromJsonAsync<List<ProductSummaryDto>>(path, cancellationToken) ?? [];
    }

    internal async Task<ProductDetailDto?> GetProductAsync(string handle, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<ProductDetailDto>($"web/v1/catalog/products/{Uri.EscapeDataString(handle)}", cancellationToken);
    }

    internal async Task<ShopifyImportPreviewDto?> GetShopifyImportPreviewAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<ShopifyImportPreviewDto>("web/v1/shopify/import-preview", cancellationToken);
    }

    internal async Task<QuoteReferenceDataDto> GetQuoteReferenceDataAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<QuoteReferenceDataDto>("web/v1/quote/reference-data", cancellationToken) ?? new QuoteReferenceDataDto();
    }

    internal async Task<QuoteEstimateResponse> EstimateQuoteAsync(QuoteEstimateRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("web/v1/quote/estimate", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<QuoteEstimateResponse>(cancellationToken) ?? new QuoteEstimateResponse();
    }

    internal async Task<WebUploadInitiationResponse> InitiateUploadAsync(WebUploadInitiationRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("web/v1/quote/uploads/resumable", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<WebUploadInitiationResponse>(cancellationToken) ?? new WebUploadInitiationResponse();
    }

    internal async Task<CheckoutDraftResponse> CreateCheckoutDraftAsync(CheckoutDraftRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("web/v1/checkout/draft", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CheckoutDraftResponse>(cancellationToken) ?? new CheckoutDraftResponse();
    }
}
