using System.Security.Claims;
using Maliev.Web.Bff.Clients;
using Maliev.Web.Shared.Commerce;

namespace Maliev.Web.Bff.Services;

internal sealed class CheckoutDraftService(
    IOrderServiceClient orderClient,
    IPaymentServiceClient paymentClient,
    IDeliveryServiceClient deliveryClient,
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
                throw new BackendUnavailableException("CustomerService", $"CustomerService returned {(int)customerResponse.StatusCode} while loading checkout customer.");
            }

            var checkoutDraft = new CustomerCheckoutDraft(customerId.Value, request.Items, request.Culture);
            using var deliveryResponse = await deliveryClient.EstimateDeliveryAsync(checkoutDraft, cancellationToken);
            if (!deliveryResponse.IsSuccessStatusCode)
            {
                throw new BackendUnavailableException("DeliveryService", $"DeliveryService returned {(int)deliveryResponse.StatusCode} while estimating delivery.");
            }

            using var orderResponse = await orderClient.CreateOrderAsync(checkoutDraft, cancellationToken);
            if (!orderResponse.IsSuccessStatusCode)
            {
                throw new BackendUnavailableException("OrderService", $"OrderService returned {(int)orderResponse.StatusCode} while creating checkout draft.");
            }

            using var paymentResponse = await paymentClient.CreatePaymentIntentAsync(checkoutDraft, cancellationToken);
            if (!paymentResponse.IsSuccessStatusCode)
            {
                throw new BackendUnavailableException("PaymentService", $"PaymentService returned {(int)paymentResponse.StatusCode} while creating payment intent.");
            }

            return new CheckoutDraftResponse
            {
                CheckoutId = Guid.NewGuid(),
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
}
