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
