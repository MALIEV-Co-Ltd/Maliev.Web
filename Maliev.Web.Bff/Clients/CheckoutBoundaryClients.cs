using Maliev.Web.Shared.Commerce;

namespace Maliev.Web.Bff.Clients;

internal interface IOrderServiceClient
{
    Task<HttpResponseMessage> CreateOrderAsync(object request, CancellationToken cancellationToken);
}

internal interface IPaymentServiceClient
{
    Task<HttpResponseMessage> CreatePaymentIntentAsync(object request, CancellationToken cancellationToken);
}

internal interface IDeliveryServiceClient
{
    Task<HttpResponseMessage> EstimateDeliveryAsync(object request, CancellationToken cancellationToken);
}

internal interface ICustomerServiceClient
{
    Task<HttpResponseMessage> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken);
}

internal sealed class OrderServiceClient(HttpClient httpClient) : IOrderServiceClient
{
    public Task<HttpResponseMessage> CreateOrderAsync(object request, CancellationToken cancellationToken) =>
        httpClient.PostAsJsonAsync("/order/v1/orders", request, cancellationToken);
}

internal sealed class PaymentServiceClient(HttpClient httpClient) : IPaymentServiceClient
{
    public Task<HttpResponseMessage> CreatePaymentIntentAsync(object request, CancellationToken cancellationToken) =>
        httpClient.PostAsJsonAsync("/payment/v1/payments", request, cancellationToken);
}

internal sealed class DeliveryServiceClient(HttpClient httpClient) : IDeliveryServiceClient
{
    public Task<HttpResponseMessage> EstimateDeliveryAsync(object request, CancellationToken cancellationToken) =>
        httpClient.PostAsJsonAsync("/delivery/v1/shipments/estimate", request, cancellationToken);
}

internal sealed class CustomerServiceClient(HttpClient httpClient) : ICustomerServiceClient
{
    public Task<HttpResponseMessage> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken) =>
        httpClient.GetAsync($"/customer/v1/customers/{customerId}", cancellationToken);
}

internal sealed record CustomerCheckoutDraft(Guid CustomerId, IReadOnlyList<CartItemDto> Items, string Culture);
