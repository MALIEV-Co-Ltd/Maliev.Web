using Maliev.Web.Bff.Services;
using Maliev.Web.Shared.Commerce;
using Maliev.Web.Shared.Localization;
using Maliev.Web.Shared.Quotes;

namespace Maliev.Web.Bff.Endpoints;

internal static class WebApiEndpoints
{
    internal static IEndpointRouteBuilder MapWebApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/web/v1");

        api.MapGet("/catalog/collections", () => Results.Ok(CatalogSeed.Collections));
        api.MapGet("/catalog/products", (string? collection) =>
        {
            var products = CatalogSeed.Products;
            if (!string.IsNullOrWhiteSpace(collection))
            {
                products = products.Where(product => product.CollectionSlug == collection).ToList();
            }

            return Results.Ok(products.Select(product => (ProductSummaryDto)product));
        });

        api.MapGet("/catalog/products/{handle}", (string handle) =>
        {
            var product = CatalogSeed.Products.FirstOrDefault(p => p.Handle.Equals(handle, StringComparison.OrdinalIgnoreCase));
            return product is null ? Results.NotFound() : Results.Ok(product);
        });

        api.MapGet("/shopify/import-preview", () => Results.Ok(CatalogSeed.ShopifyPreview()));
        api.MapGet("/quote/reference-data", () => Results.Ok(QuoteReferenceDataProvider.Get()));
        api.MapPost("/quote/estimate", (QuoteEstimateRequest request) => Results.Ok(QuoteEstimator.Estimate(request)));
        api.MapPost("/quote/uploads/resumable", (WebUploadInitiationRequest request) =>
        {
            var uploadId = $"web-{Guid.NewGuid():N}";
            var safeName = Path.GetFileName(request.FileName).Replace(' ', '-');
            var storagePath = $"quotes/{request.QuoteSessionId:N}/{uploadId}/{safeName}";
            return Results.Ok(new WebUploadInitiationResponse
            {
                UploadId = uploadId,
                ProxyUploadUrl = $"/web/v1/quote/uploads/resumable/{uploadId}",
                StoragePath = storagePath
            });
        });

        api.MapGet("/quote/uploads/{uploadId}/analysis-status", (string uploadId) => Results.Ok(new WebAnalysisStatusResponse
        {
            UploadId = uploadId,
            Status = "Ready",
            IsTerminal = true,
            Message = "Analysis event contract is ready. Production wiring should subscribe to UploadService, GeometryService, and NotificationService."
        }));

        api.MapPost("/checkout/draft", (CheckoutDraftRequest request) =>
        {
            var subtotal = request.Items.Sum(item =>
            {
                var product = CatalogSeed.Products.FirstOrDefault(p => p.Handle == item.ProductHandle);
                return (product?.PriceThb ?? 0m) * Math.Max(1, item.Quantity);
            });

            return Results.Ok(new CheckoutDraftResponse
            {
                SubtotalThb = subtotal,
                TotalThb = subtotal,
                RequiresSignIn = true
            });
        });

        api.MapPost("/preferences/culture", (CulturePreferenceRequest request) =>
        {
            var culture = SupportedCultures.Normalize(request.Culture);
            return Results.Ok(new CulturePreferenceResponse { Culture = culture, CurrencyCode = culture == SupportedCultures.ThaiCulture ? "THB" : "THB" });
        });

        return app;
    }
}

internal sealed class CulturePreferenceRequest
{
    public string Culture { get; set; } = SupportedCultures.DefaultCulture;
}

internal sealed class CulturePreferenceResponse
{
    public string Culture { get; set; } = SupportedCultures.DefaultCulture;

    public string CurrencyCode { get; set; } = "THB";
}
