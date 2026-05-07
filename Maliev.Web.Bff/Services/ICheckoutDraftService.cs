using System.Security.Claims;
using Maliev.Web.Shared.Commerce;

namespace Maliev.Web.Bff.Services;

/// <summary>
/// Coordinates customer checkout draft creation with order, payment, delivery, and customer boundaries.
/// </summary>
public interface ICheckoutDraftService
{
    /// <summary>Creates a checkout draft for the current customer session.</summary>
    Task<CheckoutDraftResponse> CreateDraftAsync(CheckoutDraftRequest request, ClaimsPrincipal user, CancellationToken cancellationToken);
}
