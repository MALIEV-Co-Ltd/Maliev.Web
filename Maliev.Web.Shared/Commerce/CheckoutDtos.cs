namespace Maliev.Web.Shared.Commerce;

/// <summary>
/// Cart item sent to the Web BFF checkout draft endpoint.
/// </summary>
public sealed class CartItemDto
{
    /// <summary>Gets or sets the product handle.</summary>
    public string ProductHandle { get; set; } = string.Empty;

    /// <summary>Gets or sets the variant SKU.</summary>
    public string VariantSku { get; set; } = string.Empty;

    /// <summary>Gets or sets the requested quantity.</summary>
    public int Quantity { get; set; } = 1;

    /// <summary>Gets or sets the product title captured for cart display.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the selected variant title captured for cart display.</summary>
    public string VariantTitle { get; set; } = string.Empty;

    /// <summary>Gets or sets the product image URL captured for cart display.</summary>
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
    public string Culture { get; set; } = "en-US";

    /// <summary>Gets or sets the cart lines.</summary>
    public List<CartItemDto> Items { get; set; } = [];

    /// <summary>Gets or sets the customer phone number for checkout contact and delivery.</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>Gets or sets the legal company name for billing.</summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>Gets or sets the VAT or tax identifier for invoice records.</summary>
    public string VatId { get; set; } = string.Empty;

    /// <summary>Gets or sets the billing address entered at checkout.</summary>
    public string BillingAddress { get; set; } = string.Empty;

    /// <summary>Gets or sets the shipping address entered at checkout.</summary>
    public string ShippingAddress { get; set; } = string.Empty;

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
