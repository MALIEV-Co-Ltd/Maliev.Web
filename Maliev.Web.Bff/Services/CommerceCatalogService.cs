using System.Net;
using System.Text.Json;
using Maliev.Web.Bff.Clients;
using Maliev.Web.Shared.Commerce;
using Maliev.Web.Shared.Localization;

namespace Maliev.Web.Bff.Services;

internal sealed class CommerceCatalogService(
    ICommerceServiceClient commerceServiceClient,
    IUploadServiceClient uploadServiceClient,
    ILogger<CommerceCatalogService> logger) : ICommerceCatalogService
{
    private const string IntranetCollectionMediaPrefix = "api/v1/commerce/collections/media/";

    public async Task<IReadOnlyList<ProductCollectionDto>> GetCollectionsAsync(CancellationToken cancellationToken)
    {
        using var response = await SendAsync(
            () => commerceServiceClient.ListCollectionsAsync(cancellationToken),
            "collections",
            cancellationToken);

        var collections = await ReadJsonAsync<List<CommerceCollectionResponse>>(response, "collections", cancellationToken);
        return collections
            .Where(collection => collection.IsPublished)
            .Select(collection => new ProductCollectionDto
            {
                Slug = collection.Handle,
                Name = Localize(collection.Title),
                Summary = Localize(collection.Description ?? string.Empty),
                ImageUrl = BuildCollectionImageUrl(collection),
                ImageAltText = Localize(string.IsNullOrWhiteSpace(collection.ImageAltText) ? collection.Title : collection.ImageAltText),
                ExpectedProductCount = 0
            })
            .ToList();
    }

    public async Task<string?> GetCollectionImageRedirectUrlAsync(string collectionSlug, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(
            () => commerceServiceClient.GetCollectionAsync(collectionSlug, cancellationToken),
            $"collection {collectionSlug}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        var collection = await ReadJsonAsync<CommerceCollectionResponse>(response, $"collection {collectionSlug}", cancellationToken);
        if (!collection.IsPublished || string.IsNullOrWhiteSpace(collection.ImageUrl))
        {
            return null;
        }

        var imageUrl = collection.ImageUrl.Trim();
        if (!TryExtractCollectionMediaUploadId(imageUrl, out var uploadId))
        {
            return imageUrl;
        }

        return await uploadServiceClient.GetSignedUrlAsync(uploadId, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductSummaryDto>> GetProductsAsync(string? collectionSlug, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(
            () => commerceServiceClient.ListProductsAsync(collectionSlug, cancellationToken),
            "products",
            cancellationToken);

        var products = await ReadJsonAsync<CommercePagedResponse<CommerceProductSummaryResponse>>(response, "products", cancellationToken);
        return products.Items
            .Where(product => IsPublished(product.Status))
            .Select(product => MapSummary(product, collectionSlug))
            .ToList();
    }

    public async Task<ProductDetailDto?> GetProductAsync(string handle, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(
            () => commerceServiceClient.GetProductAsync(handle, cancellationToken),
            $"product {handle}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        var product = await ReadJsonAsync<CommerceProductResponse>(response, $"product {handle}", cancellationToken);
        return IsPublished(product.Status) ? MapDetail(product) : null;
    }

    private async Task<HttpResponseMessage> SendAsync(
        Func<Task<HttpResponseMessage>> send,
        string resourceName,
        CancellationToken cancellationToken)
    {
        try
        {
            return await send();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(ex, "CommerceService failed while loading {ResourceName}", resourceName);
            }

            throw new BackendUnavailableException("CommerceService", $"CommerceService is unavailable while loading {resourceName}.", ex);
        }
    }

    private async Task<T> ReadJsonAsync<T>(HttpResponseMessage response, string resourceName, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new BackendUnavailableException("CommerceService", $"CommerceService returned {(int)response.StatusCode} while loading {resourceName}.");
        }

        try
        {
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken)
                ?? throw new BackendUnavailableException("CommerceService", $"CommerceService returned an empty response while loading {resourceName}.");
        }
        catch (BackendUnavailableException)
        {
            throw;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException)
        {
            logger.LogWarning(ex, "CommerceService returned invalid catalog JSON while loading {ResourceName}", resourceName);
            throw new BackendUnavailableException("CommerceService", $"CommerceService returned invalid data while loading {resourceName}.", ex);
        }
    }

    private static ProductSummaryDto MapSummary(CommerceProductSummaryResponse product, string? collectionSlug)
    {
        return new ProductSummaryDto
        {
            Handle = product.Handle,
            Title = Localize(product.Title),
            Summary = Localize(product.Summary),
            CollectionSlug = collectionSlug?.Trim() ?? string.Empty,
            PriceThb = product.Currency.Equals("THB", StringComparison.OrdinalIgnoreCase) ? product.StartingPrice : 0,
            PriceStartsAt = false,
            LeadTimeDays = 0,
            ImageUrl = product.ThumbnailUrl ?? string.Empty,
            IsPublished = IsPublished(product.Status),
            AvailableQuantity = null,
            InventoryStatus = product.StartingPrice > 0 ? "Available" : "Made to order"
        };
    }

    private static ProductDetailDto MapDetail(CommerceProductResponse product)
    {
        var activeVariants = product.Variants.Where(variant => variant.IsActive).ToList();
        var prices = activeVariants
            .Where(variant => variant.Currency.Equals("THB", StringComparison.OrdinalIgnoreCase))
            .Select(variant => variant.PriceAmount)
            .Where(price => price > 0)
            .Order()
            .ToList();
        var availableQuantity = activeVariants.Sum(variant => Math.Max(0, variant.InventoryQuantity));
        var media = product.Media
            .OrderBy(item => item.SortOrder)
            .Select(item => new ProductMediaDto
            {
                Url = item.Url,
                Alt = item.AltText ?? product.Title
            })
            .ToList();

        return new ProductDetailDto
        {
            Handle = product.Handle,
            Title = Localize(product.Title),
            Summary = Localize(product.Summary),
            Body = Localize(string.IsNullOrWhiteSpace(product.Description) ? product.Summary : product.Description),
            CollectionSlug = product.Collections.FirstOrDefault()?.Handle ?? string.Empty,
            PriceThb = prices.FirstOrDefault(),
            PriceStartsAt = prices.Distinct().Skip(1).Any(),
            LeadTimeDays = ParseLeadTimeDays(product.Variants),
            ImageUrl = media.FirstOrDefault()?.Url ?? string.Empty,
            IsPublished = IsPublished(product.Status),
            AvailableQuantity = availableQuantity,
            InventoryStatus = InventoryStatus(availableQuantity, activeVariants.Count > 0),
            Media = media,
            Variants = product.Variants
                .Select(variant => new ProductVariantDto
                {
                    Sku = variant.Sku,
                    Title = Localize(variant.Title),
                    PriceThb = variant.Currency.Equals("THB", StringComparison.OrdinalIgnoreCase) ? variant.PriceAmount : 0,
                    Available = variant.IsActive && variant.InventoryQuantity > 0
                })
                .ToList(),
            SeoKeywords = product.Collections.Select(collection => collection.Handle)
                .Append(product.ProductType)
                .Append(product.Brand ?? string.Empty)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
        };
    }

    private static LocalizedText Localize(string value)
    {
        return new LocalizedText
        {
            En = value,
            Th = value
        };
    }

    private static string BuildCollectionImageUrl(CommerceCollectionResponse collection)
    {
        if (string.IsNullOrWhiteSpace(collection.ImageUrl))
        {
            return string.Empty;
        }

        var imageUrl = collection.ImageUrl.Trim();
        return TryExtractCollectionMediaUploadId(imageUrl, out _)
            ? $"/web/v1/catalog/collections/{Uri.EscapeDataString(collection.Handle)}/image"
            : imageUrl;
    }

    private static bool TryExtractCollectionMediaUploadId(string imageUrl, out string uploadId)
    {
        var normalized = imageUrl.Trim().TrimStart('/');
        if (!normalized.StartsWith(IntranetCollectionMediaPrefix, StringComparison.OrdinalIgnoreCase))
        {
            uploadId = string.Empty;
            return false;
        }

        var rawUploadId = normalized[IntranetCollectionMediaPrefix.Length..]
            .Split(['?', '#'], StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();

        uploadId = string.IsNullOrWhiteSpace(rawUploadId)
            ? string.Empty
            : Uri.UnescapeDataString(rawUploadId);

        return !string.IsNullOrWhiteSpace(uploadId);
    }

    private static bool IsPublished(string status)
    {
        return status.Equals("Published", StringComparison.OrdinalIgnoreCase) ||
            status.Equals("Active", StringComparison.OrdinalIgnoreCase);
    }

    private static string InventoryStatus(int quantity, bool hasActiveVariants)
    {
        if (!hasActiveVariants || quantity <= 0)
        {
            return "Made to order";
        }

        return quantity <= 5 ? "Low stock" : "Available";
    }

    private static int ParseLeadTimeDays(IEnumerable<CommerceProductVariantResponse> variants)
    {
        foreach (var variant in variants)
        {
            if (string.IsNullOrWhiteSpace(variant.OptionValuesJson))
            {
                continue;
            }

            try
            {
                var options = JsonSerializer.Deserialize<Dictionary<string, string>>(variant.OptionValuesJson);
                var leadTime = options?.FirstOrDefault(option =>
                    option.Key.Equals("Lead time", StringComparison.OrdinalIgnoreCase) ||
                    option.Key.Equals("LeadTime", StringComparison.OrdinalIgnoreCase) ||
                    option.Key.Equals("lead_time", StringComparison.OrdinalIgnoreCase)).Value;

                if (!string.IsNullOrWhiteSpace(leadTime))
                {
                    var digits = new string(leadTime
                        .SkipWhile(character => !char.IsDigit(character))
                        .TakeWhile(char.IsDigit)
                        .ToArray());
                    if (int.TryParse(digits, out var days))
                    {
                        return days;
                    }
                }
            }
            catch (JsonException)
            {
            }
        }

        return 0;
    }
}

internal sealed class CommercePagedResponse<T>
{
    public List<T> Items { get; set; } = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }
}

internal sealed class CommerceCollectionResponse
{
    public Guid Id { get; set; }

    public string Handle { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public string? ImageAltText { get; set; }

    public bool IsPublished { get; set; }
}

internal sealed class CommerceProductSummaryResponse
{
    public Guid Id { get; set; }

    public string Handle { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Brand { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string ProductType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal StartingPrice { get; set; }

    public string Currency { get; set; } = "THB";

    public string? ThumbnailUrl { get; set; }
}

internal sealed class CommerceProductResponse
{
    public Guid Id { get; set; }

    public string Handle { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Brand { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ProductType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public List<CommerceProductVariantResponse> Variants { get; set; } = [];

    public List<CommerceProductMediaResponse> Media { get; set; } = [];

    public List<CommerceCollectionSummaryResponse> Collections { get; set; } = [];
}

internal sealed class CommerceProductVariantResponse
{
    public Guid Id { get; set; }

    public string Sku { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public decimal PriceAmount { get; set; }

    public string Currency { get; set; } = "THB";

    public int InventoryQuantity { get; set; }

    public bool IsActive { get; set; }

    public string? OptionValuesJson { get; set; }
}

internal sealed class CommerceProductMediaResponse
{
    public Guid Id { get; set; }

    public string Url { get; set; } = string.Empty;

    public string? AltText { get; set; }

    public int SortOrder { get; set; }
}

internal sealed class CommerceCollectionSummaryResponse
{
    public Guid Id { get; set; }

    public string Handle { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}
