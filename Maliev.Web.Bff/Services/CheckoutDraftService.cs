using System.Net.Http.Json;
using System.Security.Claims;
using Maliev.Web.Bff.Clients;
using Maliev.Web.Shared.Commerce;

namespace Maliev.Web.Bff.Services;

internal sealed class CheckoutDraftService(
    ICommerceServiceClient commerceClient,
    ICustomerServiceClient customerClient) : ICheckoutDraftService
{
    public async Task<CheckoutDraftResponse> CreateDraftAsync(CheckoutDraftRequest request, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var customerId = ResolveCustomerId(user);
        if (customerId is null)
        {
            throw new CheckoutRequiresSignInException();
        }

        try
        {
            using var customerResponse = await customerClient.GetCustomerAsync(customerId.Value, cancellationToken);
            if (!customerResponse.IsSuccessStatusCode)
            {
                var customerContent = await ReadFailureContentAsync(customerResponse, cancellationToken);
                throw new BackendUnavailableException("CustomerService", $"CustomerService returned {(int)customerResponse.StatusCode} while loading checkout customer.{customerContent}");
            }

            using var cartResponse = await commerceClient.CreateCartAsync(new CommerceCreateCartRequest(customerId.Value, "THB"), cancellationToken);
            var cart = await ReadCommerceResponseAsync<CommerceCartResponse>(cartResponse, "CommerceService", "creating storefront cart", cancellationToken);

            foreach (var item in request.Items.Where(item => item.Quantity > 0))
            {
                var variant = await ResolveProductVariantAsync(item, cancellationToken);
                using var lineResponse = await commerceClient.UpsertCartLineAsync(
                    cart.Id,
                    new CommerceUpsertCartLineRequest(variant.Id, Math.Clamp(item.Quantity, 1, 999)),
                    cancellationToken);
                cart = await ReadCommerceResponseAsync<CommerceCartResponse>(lineResponse, "CommerceService", $"adding storefront cart line {item.ProductHandle}", cancellationToken);
            }

            using var checkoutResponse = await commerceClient.CreateCheckoutSessionAsync(
                new CommerceCreateCheckoutSessionRequest(cart.Id, customerId.Value, ShippingAddressJson: null, BillingAddressJson: null),
                cancellationToken);
            var checkout = await ReadCommerceResponseAsync<CommerceCheckoutSessionResponse>(checkoutResponse, "CommerceService", "creating storefront checkout session", cancellationToken);

            return new CheckoutDraftResponse
            {
                CheckoutId = checkout.Id,
                SubtotalThb = cart.TotalAmount,
                TotalThb = checkout.TotalAmount,
                RequiresSignIn = false
            };
        }
        catch (BackendUnavailableException)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            throw new BackendUnavailableException("Checkout services", "A checkout downstream service is unavailable.", ex);
        }
    }

    private static Guid? ResolveCustomerId(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue("customer_id") ??
            user.FindFirstValue("customerId") ??
            user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out var customerId) ? customerId : null;
    }

    private async Task<CommerceProductVariantResponse> ResolveProductVariantAsync(CartItemDto item, CancellationToken cancellationToken)
    {
        using var productResponse = await commerceClient.GetProductAsync(item.ProductHandle, cancellationToken);
        var product = await ReadCommerceResponseAsync<CommerceProductResponse>(productResponse, "CommerceService", $"loading storefront product {item.ProductHandle}", cancellationToken);
        var variant = product.Variants.FirstOrDefault(candidate =>
                candidate.IsActive &&
                candidate.InventoryQuantity > 0 &&
                candidate.Sku.Equals(item.VariantSku, StringComparison.OrdinalIgnoreCase)) ??
            product.Variants.FirstOrDefault(candidate => candidate.IsActive && candidate.InventoryQuantity > 0);

        if (variant is null)
        {
            throw new BackendUnavailableException("CommerceService", $"CommerceService returned no active buyable variant for {item.ProductHandle}.");
        }

        return variant;
    }

    private static async Task<T> ReadCommerceResponseAsync<T>(
        HttpResponseMessage response,
        string serviceName,
        string operation,
        CancellationToken cancellationToken)
        where T : class
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await ReadFailureContentAsync(response, cancellationToken);
            throw new BackendUnavailableException(serviceName, $"{serviceName} returned {(int)response.StatusCode} while {operation}.{content}");
        }

        var value = await response.Content.ReadFromJsonAsync<T>(cancellationToken);
        if (value is null)
        {
            throw new BackendUnavailableException(serviceName, $"{serviceName} returned an empty response while {operation}.");
        }

        return value;
    }

    private static async Task<string> ReadFailureContentAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        var normalized = content.ReplaceLineEndings(" ");
        return $" Body: {normalized[..Math.Min(normalized.Length, 1_000)]}";
    }
}

internal sealed record CommerceCreateCartRequest(Guid CustomerId, string Currency);

internal sealed record CommerceUpsertCartLineRequest(Guid ProductVariantId, int Quantity);

internal sealed record CommerceCreateCheckoutSessionRequest(Guid CartId, Guid CustomerId, string? ShippingAddressJson, string? BillingAddressJson);

internal sealed record CommerceCartResponse(
    Guid Id,
    Guid? CustomerId,
    string? AnonymousKey,
    string Status,
    string Currency,
    IReadOnlyList<CommerceCartLineResponse> Lines,
    decimal TotalAmount);

internal sealed record CommerceCartLineResponse(
    Guid Id,
    Guid ProductVariantId,
    string Sku,
    string Title,
    int Quantity,
    decimal UnitPriceAmount,
    string Currency,
    decimal LineTotal);

internal sealed record CommerceCheckoutSessionResponse(
    Guid Id,
    Guid CartId,
    Guid CustomerId,
    string Status,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset ExpiresAtUtc);
