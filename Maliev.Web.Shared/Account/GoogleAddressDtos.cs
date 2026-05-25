namespace Maliev.Web.Shared.Account;

/// <summary>
/// Browser-safe Google Maps configuration for account address picking.
/// </summary>
public sealed class GoogleAddressConfigResponse
{
    /// <summary>Gets or sets the domain-restricted Google Maps browser API key.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional Google Maps map ID.</summary>
    public string? MapId { get; set; }

    /// <summary>Gets or sets the default map latitude.</summary>
    public double DefaultLatitude { get; set; } = 13.7563;

    /// <summary>Gets or sets the default map longitude.</summary>
    public double DefaultLongitude { get; set; } = 100.5018;

    /// <summary>Gets or sets the default map zoom.</summary>
    public int DefaultZoom { get; set; } = 12;

    /// <summary>Gets or sets optional included region codes for Places autocomplete.</summary>
    public string[] IncludedRegionCodes { get; set; } = [];
}

/// <summary>
/// Structured address returned by Google Places or reverse geocoding.
/// </summary>
public sealed class GoogleAddressSelection
{
    /// <summary>Gets or sets the address source.</summary>
    public string Source { get; set; } = "GooglePlace";

    /// <summary>Gets or sets the Google Places identifier.</summary>
    public string? PlaceId { get; set; }

    /// <summary>Gets or sets the formatted address.</summary>
    public string? FormattedAddress { get; set; }

    /// <summary>Gets or sets address number, moo, soi, and road.</summary>
    public string? AddressLine1 { get; set; }

    /// <summary>Gets or sets the sub district.</summary>
    public string? District { get; set; }

    /// <summary>Gets or sets the district.</summary>
    public string? City { get; set; }

    /// <summary>Gets or sets the province.</summary>
    public string? StateProvince { get; set; }

    /// <summary>Gets or sets the postal code.</summary>
    public string? PostalCode { get; set; }

    /// <summary>Gets or sets the ISO 3166-1 alpha-2 country code.</summary>
    public string? CountryIso2 { get; set; }

    /// <summary>Gets or sets the latitude.</summary>
    public decimal? Latitude { get; set; }

    /// <summary>Gets or sets the longitude.</summary>
    public decimal? Longitude { get; set; }
}

/// <summary>
/// Customer-facing country option for address entry.
/// </summary>
public sealed class AddressCountryOptionDto
{
    /// <summary>Gets or sets the country identifier from CountryService.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the ISO 3166-1 alpha-2 country code.</summary>
    public string Iso2 { get; set; } = string.Empty;

    /// <summary>Gets or sets the display country name.</summary>
    public string Name { get; set; } = string.Empty;
}
