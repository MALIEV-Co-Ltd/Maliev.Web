using System.Globalization;
using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.Web.Bff.Clients;
using Maliev.Web.Bff.Services;
using Maliev.Web.Shared.Account;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Maliev.Web.Bff.Controllers;

/// <summary>
/// Provides browser-safe address configuration for customer account pages.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("web/v{version:apiVersion}/address")]
public sealed class AddressController(
    IConfiguration configuration,
    ICountryServiceClient countryClient,
    IRegistryServiceClient registryClient,
    IHttpClientFactory httpClientFactory,
    StaticMapService staticMapService) : ControllerBase
{
    private static readonly string[] FallbackCountryIso2Codes =
    [
        "TH", "SG", "MY", "VN", "ID", "PH", "JP", "KR", "CN", "HK", "TW",
        "US", "GB", "DE", "FR", "AU"
    ];

    /// <summary>Gets Google Maps browser configuration for address entry.</summary>
    [HttpGet("google-config")]
    [RequirePermission("customer.profile.read")]
    [ProducesResponseType(typeof(GoogleAddressConfigResponse), StatusCodes.Status200OK)]
    public ActionResult<GoogleAddressConfigResponse> GetGoogleConfig()
    {
        var section = configuration.GetSection("GoogleMaps");
        return Ok(new GoogleAddressConfigResponse
        {
            ApiKey = section["BrowserApiKey"] ?? string.Empty,
            MapId = section["MapId"],
            DefaultLatitude = section.GetValue("DefaultLatitude", 13.7563),
            DefaultLongitude = section.GetValue("DefaultLongitude", 100.5018),
            DefaultZoom = section.GetValue("DefaultZoom", 12),
            IncludedRegionCodes = section.GetSection("IncludedRegionCodes").Get<string[]>() ?? []
        });
    }

    /// <summary>
    /// Returns a static map image for a lat/lng pair via Google Static Maps API.
    /// Falls back to OpenStreetMap tile-based rendering when no Google key is configured.
    /// </summary>
    [HttpGet("static-map")]
    [ResponseCache(Duration = 86400)]
    [RequirePermission("customer.profile.read")]
    public async Task<IActionResult> GetStaticMap(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] int width = 320,
        [FromQuery] int height = 160,
        CancellationToken cancellationToken = default)
    {
        var apiKey = configuration["GoogleMaps:ServerApiKey"]
            ?? configuration["GoogleMaps:BrowserApiKey"];

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            var latStr = lat.ToString(CultureInfo.InvariantCulture);
            var lngStr = lng.ToString(CultureInfo.InvariantCulture);
            var googleUrl = "https://maps.googleapis.com/maps/api/staticmap" +
                $"?center={latStr},{lngStr}&zoom=16&size={width}x{height}&scale=2" +
                $"&markers=color:red%7C{latStr},{lngStr}&key={apiKey}";
            try
            {
                var img = await httpClientFactory.CreateClient().GetByteArrayAsync(googleUrl, cancellationToken);
                return File(img, "image/png");
            }
            catch
            {
                // Fall through to OSM tile rendering
            }
        }

        try
        {
            var png = await staticMapService.RenderAsync(lat, lng, 16, width, height, cancellationToken);
            return File(png, "image/png");
        }
        catch
        {
            var fallback = staticMapService.RenderFallback(width, height);
            return File(fallback, "image/png");
        }
    }

    /// <summary>Gets country options supported by the shared address record.</summary>
    [HttpGet("countries")]
    [RequirePermission("customer.profile.read")]
    [ProducesResponseType(typeof(List<AddressCountryOptionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AddressCountryOptionDto>>> GetCountryOptions(CancellationToken cancellationToken)
    {
        var countries = await GetCountriesFromListAsync(cancellationToken);
        if (countries.Count == 0)
        {
            countries = await GetFallbackCountriesAsync(cancellationToken);
        }

        return Ok(SortCountryOptions(countries));
    }

    /// <summary>Searches Thai administrative address locations from RegistryService.</summary>
    [HttpGet("thai-locations")]
    [RequirePermission("customer.profile.read")]
    [ProducesResponseType(typeof(List<ThaiAddressRegistryLocationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ThaiAddressRegistryLocationDto>>> SearchThaiLocationsAsync(
        [FromQuery] string? query,
        [FromQuery] int limit = 8,
        CancellationToken cancellationToken = default)
    {
        var normalizedQuery = query?.Trim() ?? string.Empty;
        if (normalizedQuery.Length < 2)
        {
            return Ok(new List<ThaiAddressRegistryLocationDto>());
        }

        var normalizedLimit = Math.Clamp(limit, 1, 20);
        try
        {
            using var response = await registryClient.SearchThaiLocationsAsync(normalizedQuery, normalizedLimit, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Ok(new List<ThaiAddressRegistryLocationDto>());
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var data = document.RootElement.TryGetProperty("data", out var dataElement)
                ? dataElement
                : document.RootElement;

            return Ok(data.ValueKind == JsonValueKind.Array
                ? data.EnumerateArray().Select(MapThaiLocation).OfType<ThaiAddressRegistryLocationDto>().ToList()
                : []);
        }
        catch (HttpRequestException)
        {
            return Ok(new List<ThaiAddressRegistryLocationDto>());
        }
        catch (JsonException)
        {
            return Ok(new List<ThaiAddressRegistryLocationDto>());
        }
    }

    private async Task<List<AddressCountryOptionDto>> GetCountriesFromListAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await countryClient.GetCountriesAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return [];
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var data = document.RootElement.TryGetProperty("data", out var dataElement)
                ? dataElement
                : document.RootElement;

            return data.ValueKind == JsonValueKind.Array
                ? data.EnumerateArray().Select(MapCountryOption).OfType<AddressCountryOptionDto>().ToList()
                : [];
        }
        catch (HttpRequestException)
        {
            return [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private async Task<List<AddressCountryOptionDto>> GetFallbackCountriesAsync(CancellationToken cancellationToken)
    {
        var configuredIso2Codes = configuration.GetSection("Address:CountryOptions").Get<string[]>() ?? FallbackCountryIso2Codes;
        var countries = new List<AddressCountryOptionDto>();
        foreach (var iso2 in configuredIso2Codes.Select(NormalizeIso2).Where(code => code.Length == 2).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                using var response = await countryClient.GetCountryByIso2Async(iso2, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    continue;
                }

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
                var country = MapCountryOption(document.RootElement);
                if (country is not null)
                {
                    countries.Add(country);
                }
            }
            catch (HttpRequestException)
            {
            }
            catch (JsonException)
            {
            }
        }

        return countries;
    }

    private static List<AddressCountryOptionDto> SortCountryOptions(IEnumerable<AddressCountryOptionDto> countries)
    {
        return countries
            .GroupBy(country => country.Iso2, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(country => string.Equals(country.Iso2, "TH", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(country => country.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static AddressCountryOptionDto? MapCountryOption(JsonElement root)
    {
        var id = GetGuid(root, "id", "Id");
        var iso2 = GetString(root, "iso2", "Iso2");
        var name = GetString(root, "name", "Name", "officialName", "OfficialName");
        if (id is null || string.IsNullOrWhiteSpace(iso2) || string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return new AddressCountryOptionDto
        {
            Id = id.Value,
            Iso2 = NormalizeIso2(iso2),
            Name = name
        };
    }

    private static ThaiAddressRegistryLocationDto? MapThaiLocation(JsonElement root)
    {
        var location = new ThaiAddressRegistryLocationDto
        {
            Id = GetGuid(root, "id", "Id") ?? Guid.Empty,
            PostalCode = GetString(root, "postalCode", "PostalCode") ?? string.Empty,
            SubDistrictTh = GetString(root, "subDistrictTh", "SubDistrictTh") ?? string.Empty,
            DistrictTh = GetString(root, "districtTh", "DistrictTh") ?? string.Empty,
            ProvinceTh = GetString(root, "provinceTh", "ProvinceTh") ?? string.Empty,
            SubDistrictEn = GetString(root, "subDistrictEn", "SubDistrictEn") ?? string.Empty,
            DistrictEn = GetString(root, "districtEn", "DistrictEn") ?? string.Empty,
            ProvinceEn = GetString(root, "provinceEn", "ProvinceEn") ?? string.Empty
        };

        return string.IsNullOrWhiteSpace(location.PostalCode)
            && string.IsNullOrWhiteSpace(location.SubDistrictTh)
            && string.IsNullOrWhiteSpace(location.SubDistrictEn)
            && string.IsNullOrWhiteSpace(location.DistrictTh)
            && string.IsNullOrWhiteSpace(location.DistrictEn)
            && string.IsNullOrWhiteSpace(location.ProvinceTh)
            && string.IsNullOrWhiteSpace(location.ProvinceEn)
                ? null
                : location;
    }

    private static string NormalizeIso2(string? iso2)
    {
        return (iso2 ?? string.Empty).Trim().ToUpperInvariant();
    }

    private static string? GetString(JsonElement root, params string[] names)
    {
        foreach (var name in names)
        {
            if (root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
            {
                return value.GetString();
            }
        }

        return null;
    }

    private static Guid? GetGuid(JsonElement root, params string[] names)
    {
        foreach (var name in names)
        {
            if (root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String && Guid.TryParse(value.GetString(), out var guid))
            {
                return guid;
            }
        }

        return null;
    }
}
