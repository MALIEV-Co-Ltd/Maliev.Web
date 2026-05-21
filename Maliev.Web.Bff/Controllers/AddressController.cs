using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.Web.Shared.Account;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.Web.Bff.Controllers;

/// <summary>
/// Provides browser-safe address configuration for customer account pages.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("web/v{version:apiVersion}/address")]
public sealed class AddressController(IConfiguration configuration) : ControllerBase
{
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
            IncludedRegionCodes = section.GetSection("IncludedRegionCodes").Get<string[]>() ?? ["th"]
        });
    }
}
