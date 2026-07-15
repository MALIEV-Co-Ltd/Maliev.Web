using Maliev.Web.Bff.Configuration;
using Maliev.Web.Shared.Deployment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Maliev.Web.Bff.Controllers;

/// <summary>
/// Exposes non-secret immutable metadata for deployment verification.
/// </summary>
[ApiController]
[AllowAnonymous]
public sealed class BuildMetadataController(IOptions<BuildMetadataOptions> metadata) : ControllerBase
{
    /// <summary>Returns the version, source commit, and optional runtime image digest.</summary>
    [HttpGet("/web/version")]
    [ProducesResponseType(typeof(BuildMetadataResponse), StatusCodes.Status200OK)]
    public ActionResult<BuildMetadataResponse> GetVersion()
    {
        var value = metadata.Value;
        return Ok(new BuildMetadataResponse(
            value.Version,
            value.CommitSha,
            value.GetSafeImageDigest()));
    }
}
