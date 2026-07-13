using System.ComponentModel.DataAnnotations;

namespace Maliev.Web.Shared.Commerce;

/// <summary>
/// Cart item sent to the Web BFF checkout draft endpoint.
/// </summary>
public sealed class CartItemDto
{
    /// <summary>Gets or sets the product handle.</summary>
    [Required]
    [MaxLength(160)]
    public string ProductHandle { get; set; } = string.Empty;

    /// <summary>Gets or sets the variant SKU.</summary>
    [Required]
    [MaxLength(160)]
    public string VariantSku { get; set; } = string.Empty;

    /// <summary>Gets or sets the requested quantity.</summary>
    [Range(1, 10_000)]
    public int Quantity { get; set; } = 1;

    /// <summary>Gets or sets the product title captured for cart display.</summary>
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the selected variant title captured for cart display.</summary>
    [MaxLength(300)]
    public string VariantTitle { get; set; } = string.Empty;

    /// <summary>Gets or sets the product image URL captured for cart display.</summary>
    [MaxLength(2_048)]
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets the unit price captured for cart display.</summary>
    public decimal UnitPriceThb { get; set; }
}

/// <summary>
/// Draft checkout request.
/// </summary>
public sealed class CheckoutDraftRequest
{
    /// <summary>Gets or sets the requested culture.</summary>
    [MaxLength(16)]
    public string Culture { get; set; } = "en-US";

    /// <summary>Gets or sets the cart lines.</summary>
    [MaxLength(50)]
    public List<CartItemDto> Items { get; set; } = [];

    /// <summary>Gets or sets the customer phone number for checkout contact and delivery.</summary>
    [MaxLength(80)]
    public string Phone { get; set; } = string.Empty;

    /// <summary>Gets or sets the legal company name for billing.</summary>
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>Gets or sets the VAT or tax identifier for invoice records.</summary>
    [MaxLength(80)]
    public string VatId { get; set; } = string.Empty;

    /// <summary>Gets or sets the billing address entered at checkout.</summary>
    [MaxLength(2_000)]
    public string BillingAddress { get; set; } = string.Empty;

    /// <summary>Gets or sets the shipping address entered at checkout.</summary>
    [MaxLength(2_000)]
    public string ShippingAddress { get; set; } = string.Empty;

    /// <summary>Gets or sets structured shipping details entered at checkout.</summary>
    public CheckoutShippingDetailsDto ShippingDetails { get; set; } = new();

    /// <summary>Gets or sets the selected shipping rate snapshot.</summary>
    public CheckoutShippingRateDto? SelectedShippingRate { get; set; }

    /// <summary>Gets or sets whether the customer accepted MALIEV terms before checkout.</summary>
    public bool TermsAccepted { get; set; }
}

/// <summary>
/// Draft checkout response.
/// </summary>
public sealed class CheckoutDraftResponse
{
    /// <summary>Gets or sets the draft checkout id.</summary>
    public Guid CheckoutId { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the subtotal in Thai baht.</summary>
    public decimal SubtotalThb { get; set; }

    /// <summary>Gets or sets the estimated total in Thai baht.</summary>
    public decimal TotalThb { get; set; }

    /// <summary>Gets or sets whether account sign-in is required before final checkout.</summary>
    public bool RequiresSignIn { get; set; } = true;
}

/// <summary>
/// Structured shipping details collected during checkout.
/// </summary>
public sealed class CheckoutShippingDetailsDto
{
    /// <summary>Gets or sets recipient name.</summary>
    [MaxLength(200)]
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>Gets or sets recipient phone.</summary>
    [MaxLength(80)]
    public string Phone { get; set; } = string.Empty;

    /// <summary>Gets or sets street address.</summary>
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    /// <summary>Gets or sets district or subdistrict.</summary>
    [MaxLength(120)]
    public string District { get; set; } = string.Empty;

    /// <summary>Gets or sets state or amphoe.</summary>
    [MaxLength(120)]
    public string State { get; set; } = string.Empty;

    /// <summary>Gets or sets province.</summary>
    [MaxLength(120)]
    public string Province { get; set; } = string.Empty;

    /// <summary>Gets or sets postal code.</summary>
    [MaxLength(20)]
    public string Postcode { get; set; } = string.Empty;

    /// <summary>Gets or sets ISO 3166-1 alpha-2 destination country code.</summary>
    [StringLength(2, MinimumLength = 2)]
    public string CountryCode { get; set; } = "TH";

    /// <summary>Gets or sets parcel weight in grams.</summary>
    [Range(1, 1_000_000)]
    public decimal WeightGrams { get; set; } = 1000m;

    /// <summary>Gets or sets parcel length in centimeters.</summary>
    [Range(0.1, 1_000)]
    public decimal LengthCm { get; set; } = 20m;

    /// <summary>Gets or sets parcel width in centimeters.</summary>
    [Range(0.1, 1_000)]
    public decimal WidthCm { get; set; } = 15m;

    /// <summary>Gets or sets parcel height in centimeters.</summary>
    [Range(0.1, 1_000)]
    public decimal HeightCm { get; set; } = 10m;
}

/// <summary>
/// Available courier option.
/// </summary>
public sealed class CheckoutShippingCourierDto
{
    /// <summary>Gets or sets courier code.</summary>
    public string CourierCode { get; set; } = string.Empty;

    /// <summary>Gets or sets courier display name.</summary>
    public string CourierName { get; set; } = string.Empty;

    /// <summary>Gets or sets optional courier note.</summary>
    public string? Note { get; set; }

    /// <summary>Gets or sets shipping scope.</summary>
    public string Scope { get; set; } = "domestic";

    /// <summary>Gets or sets the shipping gateway that served this courier option.</summary>
    public string Provider { get; set; } = string.Empty;
}

/// <summary>
/// Selected or available shipping rate option.
/// </summary>
public sealed class CheckoutShippingRateDto
{
    /// <summary>Gets or sets courier code.</summary>
    public string CourierCode { get; set; } = string.Empty;

    /// <summary>Gets or sets carrier product name.</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Gets or sets total price.</summary>
    public decimal TotalPrice { get; set; }

    /// <summary>Gets or sets currency code.</summary>
    public string CurrencyCode { get; set; } = "THB";

    /// <summary>Gets or sets estimated delivery date.</summary>
    public string? EstimatedDeliveryDate { get; set; }

    /// <summary>Gets or sets service level.</summary>
    public string? ServiceLevel { get; set; }

    /// <summary>Gets or sets the shipping gateway that served this rate option.</summary>
    public string Provider { get; set; } = string.Empty;
}

/// <summary>
/// Request for live checkout shipping rates.
/// </summary>
public sealed class CheckoutShippingRateRequest
{
    /// <summary>Gets or sets destination and parcel details.</summary>
    public CheckoutShippingDetailsDto ShippingDetails { get; set; } = new();

    /// <summary>Gets or sets optional courier codes.</summary>
    [MaxLength(20)]
    public List<string> CourierCodes { get; set; } = [];
}

/// <summary>
/// Response containing live checkout shipping rates.
/// </summary>
public sealed class CheckoutShippingRateResponse
{
    /// <summary>Gets or sets available rates.</summary>
    public List<CheckoutShippingRateDto> Rates { get; set; } = [];
}

/// <summary>
/// Current tracking status for a checkout shipment.
/// </summary>
public sealed class CheckoutShippingTrackingDto
{
    /// <summary>Gets or sets the carrier tracking code.</summary>
    public string TrackingCode { get; set; } = string.Empty;

    /// <summary>Gets or sets courier code.</summary>
    public string CourierCode { get; set; } = string.Empty;

    /// <summary>Gets or sets courier display name.</summary>
    public string CourierName { get; set; } = string.Empty;

    /// <summary>Gets or sets latest tracking status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets latest tracking description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the shipping gateway that served this tracking status.</summary>
    public string Provider { get; set; } = string.Empty;
}
