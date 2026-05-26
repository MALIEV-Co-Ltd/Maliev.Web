using Maliev.Web.Shared.Localization;

namespace Maliev.Web.Shared.Commerce;

/// <summary>
/// A customer-visible product collection.
/// </summary>
public sealed class ProductCollectionDto
{
    /// <summary>Gets or sets the collection slug.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Gets or sets the localized collection name.</summary>
    public LocalizedText Name { get; set; } = new();

    /// <summary>Gets or sets the localized collection summary.</summary>
    public LocalizedText Summary { get; set; } = new();

    /// <summary>Gets or sets the collection image URL.</summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets the localized collection image alt text.</summary>
    public LocalizedText ImageAltText { get; set; } = new();

    /// <summary>Gets or sets the expected migrated product count.</summary>
    public int ExpectedProductCount { get; set; }
}

/// <summary>
/// A product card used by customer-facing catalog pages.
/// </summary>
public class ProductSummaryDto
{
    /// <summary>Gets or sets the canonical product handle.</summary>
    public string Handle { get; set; } = string.Empty;

    /// <summary>Gets or sets the localized product title.</summary>
    public LocalizedText Title { get; set; } = new();

    /// <summary>Gets or sets the localized product summary.</summary>
    public LocalizedText Summary { get; set; } = new();

    /// <summary>Gets or sets the collection slug.</summary>
    public string CollectionSlug { get; set; } = string.Empty;

    /// <summary>Gets or sets the current price in Thai baht.</summary>
    public decimal PriceThb { get; set; }

    /// <summary>Gets or sets the original compare-at price in Thai baht.</summary>
    public decimal? CompareAtPriceThb { get; set; }

    /// <summary>Gets or sets whether the product has variants with different prices.</summary>
    public bool PriceStartsAt { get; set; }

    /// <summary>Gets or sets the lead time in business days.</summary>
    public int LeadTimeDays { get; set; }

    /// <summary>Gets or sets the primary product image URL.</summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional external source URL for legacy redirects.</summary>
    public string SourceUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets whether the product is visible in the customer storefront.</summary>
    public bool IsPublished { get; set; } = true;

    /// <summary>Gets or sets the available sellable quantity when inventory is tracked.</summary>
    public int? AvailableQuantity { get; set; }

    /// <summary>Gets or sets the customer-visible inventory status.</summary>
    public string InventoryStatus { get; set; } = "Available";
}

/// <summary>
/// Detailed product information for the product page.
/// </summary>
public sealed class ProductDetailDto : ProductSummaryDto
{
    /// <summary>Gets or sets the localized product body.</summary>
    public LocalizedText Body { get; set; } = new();

    /// <summary>Gets or sets product media URLs.</summary>
    public List<ProductMediaDto> Media { get; set; } = [];

    /// <summary>Gets or sets product variants.</summary>
    public List<ProductVariantDto> Variants { get; set; } = [];

    /// <summary>Gets or sets SEO keywords for the product page.</summary>
    public List<string> SeoKeywords { get; set; } = [];
}

/// <summary>
/// Product media metadata.
/// </summary>
public sealed class ProductMediaDto
{
    /// <summary>Gets or sets the media URL.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Gets or sets the media alt text.</summary>
    public string Alt { get; set; } = string.Empty;
}

/// <summary>
/// A buyable product variant.
/// </summary>
public sealed class ProductVariantDto
{
    /// <summary>Gets or sets the variant SKU.</summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>Gets or sets the localized variant title.</summary>
    public LocalizedText Title { get; set; } = new();

    /// <summary>Gets or sets the variant price in Thai baht.</summary>
    public decimal PriceThb { get; set; }

    /// <summary>Gets or sets whether the variant can be sold.</summary>
    public bool Available { get; set; } = true;
}
