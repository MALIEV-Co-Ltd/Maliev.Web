using Asp.Versioning;
using Maliev.Web.Shared.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.Web.Bff.Controllers;

/// <summary>
/// Customer preference API.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("web/v{version:apiVersion}/preferences")]
[AllowAnonymous]
public sealed class PreferencesController : ControllerBase
{
    /// <summary>Stores an anonymous culture preference response for client-side persistence.</summary>
    [HttpPost("culture")]
    [ProducesResponseType(typeof(CulturePreferenceResponse), StatusCodes.Status200OK)]
    public ActionResult<CulturePreferenceResponse> SetCulture([FromBody] CulturePreferenceRequest request)
    {
        var culture = SupportedCultures.Normalize(request.Culture);
        return Ok(new CulturePreferenceResponse
        {
            Culture = culture,
            CurrencyCode = "THB"
        });
    }
}

/// <summary>
/// Culture preference request.
/// </summary>
public sealed class CulturePreferenceRequest
{
    /// <summary>Gets or sets the requested culture.</summary>
    public string Culture { get; set; } = SupportedCultures.DefaultCulture;
}

/// <summary>
/// Culture preference response.
/// </summary>
public sealed class CulturePreferenceResponse
{
    /// <summary>Gets or sets the normalized culture.</summary>
    public string Culture { get; set; } = SupportedCultures.DefaultCulture;

    /// <summary>Gets or sets the selected currency code.</summary>
    public string CurrencyCode { get; set; } = "THB";
}
