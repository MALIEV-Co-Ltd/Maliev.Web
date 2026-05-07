using Asp.Versioning;
using Maliev.Web.Bff.Services;
using Maliev.Web.Shared.Commerce;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.Web.Bff.Controllers;

/// <summary>
/// Customer checkout API.
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("web/v{version:apiVersion}/checkout")]
public sealed class CheckoutController(ICheckoutDraftService checkoutDraftService) : ControllerBase
{
    /// <summary>Creates a customer checkout draft through downstream order and payment boundaries.</summary>
    [HttpPost("draft")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CheckoutDraftResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CreateDraft([FromBody] CheckoutDraftRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await checkoutDraftService.CreateDraftAsync(request, User, cancellationToken));
        }
        catch (CheckoutRequiresSignInException ex)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Sign in required",
                Detail = ex.Message,
                Status = StatusCodes.Status401Unauthorized
            });
        }
        catch (BackendUnavailableException ex)
        {
            var problem = new ProblemDetails
            {
                Title = "Checkout backend unavailable",
                Detail = ex.Message,
                Status = StatusCodes.Status503ServiceUnavailable
            };
            problem.Extensions["backend"] = ex.BackendName;
            return StatusCode(StatusCodes.Status503ServiceUnavailable, problem);
        }
    }
}
