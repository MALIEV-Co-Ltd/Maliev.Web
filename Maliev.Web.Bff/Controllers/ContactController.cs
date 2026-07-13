using Asp.Versioning;
using Maliev.Web.Bff.Security;
using Maliev.Web.Bff.Services;
using Maliev.Web.Shared.Contact;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Maliev.Web.Bff.Controllers;

/// <summary>
/// Customer-facing contact API.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("web/v{version:apiVersion}/contact")]
[AllowAnonymous]
public sealed class ContactController(IContactMessageService contactMessageService) : ControllerBase
{
    /// <summary>
    /// Submits a customer contact message to ContactService.
    /// </summary>
    [HttpPost("messages")]
    [EnableRateLimiting(WebRateLimiterPolicies.Contact)]
    [RequestSizeLimit(72 * 1024 * 1024)]
    public async Task<IActionResult> Submit([FromBody] ContactMessageRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            return Ok(await contactMessageService.SubmitAsync(request, cancellationToken));
        }
        catch (BackendUnavailableException ex)
        {
            return Problem(
                title: $"{ex.BackendName} unavailable",
                detail: ex.Message,
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}
