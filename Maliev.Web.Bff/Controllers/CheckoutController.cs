using System.Text.Json;
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
[ApiVersion("1.0")]
[Route("web/v{version:apiVersion}/checkout")]
public sealed class CheckoutController(
    ICheckoutDraftService checkoutDraftService,
    IHostEnvironment environment) : ControllerBase
{
    /// <summary>Creates a customer checkout draft through the CommerceService cart and checkout-session boundary.</summary>
    [HttpPost("draft")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CheckoutDraftResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CreateDraft([FromBody] CheckoutDraftRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await checkoutDraftService.CreateDraftAsync(request, User, cancellationToken));
        }
        catch (CheckoutRequiresSignInException)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Sign in required",
                Detail = "Sign in to continue checkout and save this order to your account.",
                Status = StatusCodes.Status401Unauthorized
            });
        }
        catch (CheckoutValidationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Checkout details required",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (BackendUnavailableException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, BuildCheckoutUnavailableProblem(ex));
        }
    }

    /// <summary>Creates a customer checkout draft from the cart page progressive-enhancement form.</summary>
    [HttpPost("draft-form")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> CreateDraftForm([FromForm] CheckoutDraftForm form, CancellationToken cancellationToken)
    {
        var items = DeserializeItems(form.ItemsJson);
        var request = new CheckoutDraftRequest
        {
            Culture = string.IsNullOrWhiteSpace(form.Culture) ? "en-US" : form.Culture,
            Items = items,
            Phone = form.Phone,
            CompanyName = form.CompanyName,
            VatId = form.VatId,
            BillingAddress = form.BillingAddress,
            ShippingAddress = form.ShippingAddress,
            TermsAccepted = form.TermsAccepted
        };

        try
        {
            await checkoutDraftService.CreateDraftAsync(request, User, cancellationToken);
            return LocalRedirect("/cart?checkout=ready");
        }
        catch (CheckoutRequiresSignInException)
        {
            return LocalRedirect("/auth/sign-in?returnUrl=%2Fcheckout");
        }
        catch (CheckoutValidationException)
        {
            return LocalRedirect("/checkout?checkout=terms");
        }
        catch (BackendUnavailableException ex)
        {
            var backendQuery = ExposesCheckoutDiagnostics()
                ? $"&backend={Uri.EscapeDataString(ex.BackendName)}"
                : string.Empty;
            return LocalRedirect($"/cart?checkout=unavailable{backendQuery}");
        }
    }

    private ProblemDetails BuildCheckoutUnavailableProblem(BackendUnavailableException exception)
    {
        var problem = new ProblemDetails
        {
            Title = "Checkout is temporarily unavailable",
            Detail = "We could not prepare checkout right now. Please refresh the page or contact MALIEV.",
            Status = StatusCodes.Status503ServiceUnavailable
        };

        if (ExposesCheckoutDiagnostics())
        {
            problem.Extensions["backend"] = exception.BackendName;
            problem.Extensions["reason"] = exception.Message;
        }

        return problem;
    }

    private bool ExposesCheckoutDiagnostics()
    {
        return environment.IsDevelopment() || environment.IsEnvironment("Testing");
    }

    private static List<CartItemDto> DeserializeItems(string? itemsJson)
    {
        if (string.IsNullOrWhiteSpace(itemsJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<CartItemDto>>(itemsJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    /// <summary>
    /// Posted cart checkout form.
    /// </summary>
    public sealed class CheckoutDraftForm
    {
        /// <summary>Gets or sets the browser culture.</summary>
        public string Culture { get; set; } = "en-US";

        /// <summary>Gets or sets the serialized cart item payload.</summary>
        public string ItemsJson { get; set; } = "[]";

        /// <summary>Gets or sets the customer phone number.</summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>Gets or sets the billing company name.</summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>Gets or sets the VAT or tax identifier.</summary>
        public string VatId { get; set; } = string.Empty;

        /// <summary>Gets or sets the billing address.</summary>
        public string BillingAddress { get; set; } = string.Empty;

        /// <summary>Gets or sets the shipping address.</summary>
        public string ShippingAddress { get; set; } = string.Empty;

        /// <summary>Gets or sets whether the customer accepted checkout terms.</summary>
        public bool TermsAccepted { get; set; }
    }
}
