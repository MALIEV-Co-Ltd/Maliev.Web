using System.ComponentModel.DataAnnotations;

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

    /// <summary>Gets or sets the signed-in customer profile image URL.</summary>
    public string? ProfileImageUrl { get; set; }
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

    /// <summary>Gets or sets the customer profile image URL.</summary>
    public string? ProfileImageUrl { get; set; }

    /// <summary>Gets or sets the customer mobile phone number.</summary>
    public string? Mobile { get; set; }

    /// <summary>Gets or sets the company name linked to the customer.</summary>
    public string? CompanyName { get; set; }

    /// <summary>Gets or sets the company identifier linked to the customer.</summary>
    public Guid? CompanyId { get; set; }

    /// <summary>Gets or sets the company VAT or tax identifier.</summary>
    public string? CompanyVatNumber { get; set; }

    /// <summary>Gets or sets the company registration number.</summary>
    public string? CompanyRegistrationNumber { get; set; }

    /// <summary>Gets or sets the company contact email address.</summary>
    public string? CompanyContactEmail { get; set; }

    /// <summary>Gets or sets the company contact phone number.</summary>
    public string? CompanyContactPhone { get; set; }

    /// <summary>Gets or sets the concurrency token from the linked company.</summary>
    public uint CompanyVersion { get; set; }

    /// <summary>Gets or sets the customer-facing segment.</summary>
    public string Segment { get; set; } = string.Empty;

    /// <summary>Gets or sets the customer-facing tier.</summary>
    public string Tier { get; set; } = string.Empty;

    /// <summary>Gets or sets the customer-facing NDA status.</summary>
    public string NdaStatus { get; set; } = string.Empty;

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
    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the customer mobile phone number.</summary>
    public string? Mobile { get; set; }

    /// <summary>Gets or sets the company name to link or update.</summary>
    [StringLength(255)]
    public string? CompanyName { get; set; }

    /// <summary>Gets or sets the company VAT or tax identifier.</summary>
    [StringLength(50)]
    public string? CompanyVatNumber { get; set; }

    /// <summary>Gets or sets the company registration number.</summary>
    [StringLength(100)]
    public string? CompanyRegistrationNumber { get; set; }

    /// <summary>Gets or sets the company contact email address.</summary>
    [EmailAddress]
    [StringLength(255)]
    public string? CompanyContactEmail { get; set; }

    /// <summary>Gets or sets the company contact phone number.</summary>
    [StringLength(20)]
    public string? CompanyContactPhone { get; set; }

    /// <summary>Gets or sets the company branch type: "HQ" for head office, "BRANCH" for a branch.</summary>
    [StringLength(10)]
    public string? CompanyBranchType { get; set; }

    /// <summary>Gets or sets the 5-digit branch code when CompanyBranchType is "BRANCH".</summary>
    [StringLength(5)]
    public string? CompanyBranchCode { get; set; }

    /// <summary>Gets or sets the preferred language code.</summary>
    public string PreferredLanguage { get; set; } = "th";

    /// <summary>Gets or sets the preferred timezone.</summary>
    public string Timezone { get; set; } = "Asia/Bangkok";

    /// <summary>Gets or sets the concurrency token from CustomerService.</summary>
    public uint Version { get; set; }

    /// <summary>Gets or sets the concurrency token from the linked company.</summary>
    public uint CompanyVersion { get; set; }
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

    /// <summary>Gets or sets the Google Maps business or location display name.</summary>
    public string? GooglePlaceName { get; set; }

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
/// Customer address create or update request from the account address book.
/// </summary>
public sealed class CustomerAddressUpsertRequest
{
    /// <summary>Gets or sets the address type.</summary>
    [Required(ErrorMessage = "Address type is required")]
    [StringLength(50)]
    public string Type { get; set; } = "Shipping";

    /// <summary>Gets or sets whether this is the default address for its type.</summary>
    public bool IsDefault { get; set; }

    /// <summary>Gets or sets the user-facing place label.</summary>
    [StringLength(50)]
    public string? PlaceLabel { get; set; }

    /// <summary>Gets or sets the custom place label when the label is Other.</summary>
    [StringLength(100)]
    public string? PlaceLabelOther { get; set; }

    /// <summary>Gets or sets the first address line.</summary>
    [Required(ErrorMessage = "Address line 1 is required")]
    [StringLength(255)]
    public string AddressLine1 { get; set; } = string.Empty;

    /// <summary>Gets or sets the second address line.</summary>
    [StringLength(255)]
    public string? AddressLine2 { get; set; }

    /// <summary>Gets or sets the third address line.</summary>
    [StringLength(255)]
    public string? AddressLine3 { get; set; }

    /// <summary>Gets or sets the district name.</summary>
    [StringLength(100)]
    public string? District { get; set; }

    /// <summary>Gets or sets the city name.</summary>
    [Required(ErrorMessage = "City is required")]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    /// <summary>Gets or sets the state or province.</summary>
    [Required(ErrorMessage = "State/Province is required")]
    [StringLength(100)]
    public string StateProvince { get; set; } = string.Empty;

    /// <summary>Gets or sets the postal code.</summary>
    [Required(ErrorMessage = "Postal code is required")]
    [StringLength(20)]
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>Gets or sets the country identifier.</summary>
    public Guid CountryId { get; set; }

    /// <summary>Gets or sets the delivery recipient name.</summary>
    [StringLength(200)]
    public string? RecipientName { get; set; }

    /// <summary>Gets or sets the delivery recipient phone.</summary>
    [StringLength(20)]
    public string? RecipientPhone { get; set; }

    /// <summary>Gets or sets the optional delivery note for the driver.</summary>
    [StringLength(500)]
    public string? DriverNote { get; set; }

    /// <summary>Gets or sets the Google Maps business or location display name.</summary>
    public string? GooglePlaceName { get; set; }

    /// <summary>Gets or sets the address source: Manual, GooglePlace, or GoogleMapPin.</summary>
    [StringLength(50)]
    public string AddressSource { get; set; } = "Manual";

    /// <summary>Gets or sets the Google Places identifier.</summary>
    [StringLength(255)]
    public string? GooglePlaceId { get; set; }

    /// <summary>Gets or sets the formatted Google address.</summary>
    [StringLength(500)]
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

    /// <summary>
    /// A company result returned by the Thai DBD registry search.
    /// </summary>
    public sealed class CompanySearchResultDto
    {
        /// <summary>Gets or sets the 13-digit juristic (registration/tax) ID.</summary>
        public string JuristicId { get; set; } = string.Empty;

        /// <summary>Gets or sets the company name in Thai.</summary>
        public string NameTh { get; set; } = string.Empty;

        /// <summary>Gets or sets the company name in English.</summary>
        public string? NameEn { get; set; }

        /// <summary>Gets or sets the full legal company name in Thai with prefix (e.g. บริษัท XXX จำกัด).</summary>
        public string FullNameTh { get; set; } = string.Empty;

        /// <summary>Gets or sets the juristic status.</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Gets or sets the juristic type.</summary>
        public string? JuristicType { get; set; }

        /// <summary>Gets or sets the business objectives description.</summary>
        public string BusinessObjectives { get; set; } = string.Empty;
    }

/// <summary>
/// Response wrapper from the internal RegistryService company search API.
/// </summary>
public sealed class RegistryCompanySearchResponse
{
    /// <summary>Gets or sets a value indicating whether the request was successful.</summary>
    public bool Success { get; set; }

    /// <summary>Gets or sets the list of matching company profiles.</summary>
    public List<RegistryCompanySearchItemDto>? Data { get; set; }

    /// <summary>Gets or sets the response message.</summary>
    public string? Message { get; set; }

    /// <summary>Gets or sets the list of error descriptions.</summary>
    public List<string>? Errors { get; set; }
}

/// <summary>
/// A single company profile returned by the RegistryService company search.
/// </summary>
public sealed class RegistryCompanySearchItemDto
{
    /// <summary>Gets or sets the company status code.</summary>
    public string StatusCode { get; set; } = string.Empty;

    /// <summary>Gets or sets the company status description in Thai.</summary>
    public string StatusNameTh { get; set; } = string.Empty;

    /// <summary>Gets or sets the 13-digit tax identification number.</summary>
    public string TaxId { get; set; } = string.Empty;

    /// <summary>Gets or sets the company name in Thai.</summary>
    public string CompanyNameTh { get; set; } = string.Empty;

    /// <summary>Gets or sets the business objectives description.</summary>
    public string BusinessObjectives { get; set; } = string.Empty;

    /// <summary>Gets or sets the company type code.</summary>
    public string CompanyTypeCode { get; set; } = string.Empty;

    /// <summary>Gets or sets the stock exchange symbol name.</summary>
    public string? StockName { get; set; }

    /// <summary>Gets or sets the full company name in Thai with prefix.</summary>
    public string FullNameTh { get; set; } = string.Empty;
}
