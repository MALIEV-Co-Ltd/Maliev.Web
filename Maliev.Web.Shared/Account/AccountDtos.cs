namespace Maliev.Web.Shared.Account;

/// <summary>
/// Authenticated customer account session exposed to the Web client.
/// </summary>
public sealed class CustomerAccountSessionDto
{
    /// <summary>Gets or sets whether the browser has an authenticated customer session.</summary>
    public bool IsAuthenticated { get; set; }

    /// <summary>Gets or sets the IAM principal identifier.</summary>
    public Guid? PrincipalId { get; set; }

    /// <summary>Gets or sets the canonical CustomerService customer identifier.</summary>
    public Guid? CustomerId { get; set; }

    /// <summary>Gets or sets the signed-in customer email.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the signed-in customer display name.</summary>
    public string DisplayName { get; set; } = string.Empty;
}

/// <summary>
/// Customer profile data owned by CustomerService and rendered by the public account area.
/// </summary>
public sealed class CustomerAccountProfileDto
{
    /// <summary>Gets or sets the customer identifier.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the IAM principal identifier.</summary>
    public Guid? PrincipalId { get; set; }

    /// <summary>Gets or sets the customer first name.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Gets or sets the customer last name.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Gets or sets the customer display name.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the customer email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the customer mobile phone number.</summary>
    public string? Mobile { get; set; }

    /// <summary>Gets or sets the company name linked to the customer.</summary>
    public string? CompanyName { get; set; }

    /// <summary>Gets or sets the customer status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the preferred language code.</summary>
    public string PreferredLanguage { get; set; } = string.Empty;

    /// <summary>Gets or sets the preferred timezone.</summary>
    public string Timezone { get; set; } = string.Empty;

    /// <summary>Gets or sets the concurrency token from CustomerService.</summary>
    public uint Version { get; set; }
}

/// <summary>
/// Request to update a customer profile from the Web account area.
/// </summary>
public sealed class CustomerAccountProfileUpdateRequest
{
    /// <summary>Gets or sets the customer first name.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Gets or sets the customer last name.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Gets or sets the customer email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the customer mobile phone number.</summary>
    public string? Mobile { get; set; }

    /// <summary>Gets or sets the preferred language code.</summary>
    public string PreferredLanguage { get; set; } = "th";

    /// <summary>Gets or sets the preferred timezone.</summary>
    public string Timezone { get; set; } = "Asia/Bangkok";

    /// <summary>Gets or sets the concurrency token from CustomerService.</summary>
    public uint Version { get; set; }
}

/// <summary>
/// Customer address data displayed in the account address book.
/// </summary>
public sealed class CustomerAddressDto
{
    /// <summary>Gets or sets the address identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the address type.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Gets or sets whether this is the default address for its type.</summary>
    public bool IsDefault { get; set; }

    /// <summary>Gets or sets the user-facing place label.</summary>
    public string? PlaceLabel { get; set; }

    /// <summary>Gets or sets the custom place label when the label is Other.</summary>
    public string? PlaceLabelOther { get; set; }

    /// <summary>Gets or sets the first address line.</summary>
    public string AddressLine1 { get; set; } = string.Empty;

    /// <summary>Gets or sets the second address line.</summary>
    public string? AddressLine2 { get; set; }

    /// <summary>Gets or sets the third address line.</summary>
    public string? AddressLine3 { get; set; }

    /// <summary>Gets or sets the district name.</summary>
    public string? District { get; set; }

    /// <summary>Gets or sets the city name.</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>Gets or sets the state or province.</summary>
    public string StateProvince { get; set; } = string.Empty;

    /// <summary>Gets or sets the postal code.</summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>Gets or sets the country identifier.</summary>
    public Guid CountryId { get; set; }

    /// <summary>Gets or sets the delivery recipient name.</summary>
    public string? RecipientName { get; set; }

    /// <summary>Gets or sets the delivery recipient phone.</summary>
    public string? RecipientPhone { get; set; }

    /// <summary>Gets or sets the optional delivery note for the driver.</summary>
    public string? DriverNote { get; set; }

    /// <summary>Gets or sets the address source: Manual, GooglePlace, or GoogleMapPin.</summary>
    public string AddressSource { get; set; } = "Manual";

    /// <summary>Gets or sets the Google Places identifier.</summary>
    public string? GooglePlaceId { get; set; }

    /// <summary>Gets or sets the formatted Google address.</summary>
    public string? FormattedAddress { get; set; }

    /// <summary>Gets or sets the address latitude.</summary>
    public decimal? Latitude { get; set; }

    /// <summary>Gets or sets the address longitude.</summary>
    public decimal? Longitude { get; set; }

    /// <summary>Gets or sets the concurrency token from CustomerService.</summary>
    public uint Version { get; set; }
}

/// <summary>
/// Customer address create or update request from the account address book.
/// </summary>
public sealed class CustomerAddressUpsertRequest
{
    /// <summary>Gets or sets the address type.</summary>
    public string Type { get; set; } = "Shipping";

    /// <summary>Gets or sets whether this is the default address for its type.</summary>
    public bool IsDefault { get; set; }

    /// <summary>Gets or sets the user-facing place label.</summary>
    public string? PlaceLabel { get; set; }

    /// <summary>Gets or sets the custom place label when the label is Other.</summary>
    public string? PlaceLabelOther { get; set; }

    /// <summary>Gets or sets the first address line.</summary>
    public string AddressLine1 { get; set; } = string.Empty;

    /// <summary>Gets or sets the second address line.</summary>
    public string? AddressLine2 { get; set; }

    /// <summary>Gets or sets the third address line.</summary>
    public string? AddressLine3 { get; set; }

    /// <summary>Gets or sets the district name.</summary>
    public string? District { get; set; }

    /// <summary>Gets or sets the city name.</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>Gets or sets the state or province.</summary>
    public string StateProvince { get; set; } = string.Empty;

    /// <summary>Gets or sets the postal code.</summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>Gets or sets the country identifier.</summary>
    public Guid CountryId { get; set; }

    /// <summary>Gets or sets the delivery recipient name.</summary>
    public string? RecipientName { get; set; }

    /// <summary>Gets or sets the delivery recipient phone.</summary>
    public string? RecipientPhone { get; set; }

    /// <summary>Gets or sets the optional delivery note for the driver.</summary>
    public string? DriverNote { get; set; }

    /// <summary>Gets or sets the address source: Manual, GooglePlace, or GoogleMapPin.</summary>
    public string AddressSource { get; set; } = "Manual";

    /// <summary>Gets or sets the Google Places identifier.</summary>
    public string? GooglePlaceId { get; set; }

    /// <summary>Gets or sets the formatted Google address.</summary>
    public string? FormattedAddress { get; set; }

    /// <summary>Gets or sets the address latitude.</summary>
    public decimal? Latitude { get; set; }

    /// <summary>Gets or sets the address longitude.</summary>
    public decimal? Longitude { get; set; }

    /// <summary>Gets or sets the concurrency token from CustomerService for updates.</summary>
    public uint Version { get; set; }
}

/// <summary>
/// Customer address delete request from the account address book.
/// </summary>
public sealed class CustomerAddressDeleteRequest
{
    /// <summary>Gets or sets the concurrency token from CustomerService.</summary>
    public uint Version { get; set; }
}

/// <summary>
/// Account order summary for customer-facing order history.
/// </summary>
public sealed class CustomerOrderSummaryDto
{
    /// <summary>Gets or sets the order identifier.</summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>Gets or sets the order number.</summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the order source.</summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>Gets or sets the order status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the order total in Thai baht.</summary>
    public decimal TotalThb { get; set; }

    /// <summary>Gets or sets when the order was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>
/// Account order collection response.
/// </summary>
public sealed class CustomerOrdersResponse
{
    /// <summary>Gets or sets shop orders from the commerce boundary.</summary>
    public List<CustomerOrderSummaryDto> ShopOrders { get; set; } = [];

    /// <summary>Gets or sets the QuoteEngine project and manufacturing order URL.</summary>
    public string ManufacturingOrdersUrl { get; set; } = "https://quote.maliev.com/orders";
}
