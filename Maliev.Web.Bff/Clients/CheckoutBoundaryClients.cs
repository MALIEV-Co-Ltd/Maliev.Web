using System.Net.Http.Json;

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

/// <summary>
/// Downstream CustomerService client used by the Web BFF.
/// </summary>
public interface ICustomerServiceClient
{
    /// <summary>Gets a customer by id.</summary>
    Task<HttpResponseMessage> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken);

    /// <summary>Updates a customer by id.</summary>
    Task<HttpResponseMessage> UpdateCustomerAsync(Guid customerId, object request, CancellationToken cancellationToken);

    /// <summary>Registers a customer account.</summary>
    Task<HttpResponseMessage> RegisterCustomerAsync(object request, CancellationToken cancellationToken);

    /// <summary>Gets customer-owned addresses.</summary>
    Task<HttpResponseMessage> GetCustomerAddressesAsync(Guid customerId, CancellationToken cancellationToken);

    /// <summary>Creates a customer-owned address.</summary>
    Task<HttpResponseMessage> CreateCustomerAddressAsync(object request, CancellationToken cancellationToken);

    /// <summary>Updates a customer-owned address.</summary>
    Task<HttpResponseMessage> UpdateCustomerAddressAsync(Guid addressId, object request, CancellationToken cancellationToken);

    /// <summary>Deletes a customer-owned address.</summary>
    Task<HttpResponseMessage> DeleteCustomerAddressAsync(Guid addressId, object request, CancellationToken cancellationToken);
}

/// <summary>
/// Downstream AuthService client used by the Web BFF.
/// </summary>
public interface IAuthServiceClient
{
    /// <summary>Signs in with AuthService email/password login.</summary>
    Task<HttpResponseMessage> LoginAsync(object request, CancellationToken cancellationToken);

    /// <summary>Exchanges a verified customer Google identity for a MALIEV customer token.</summary>
    Task<HttpResponseMessage> ExchangeCustomerGoogleAsync(object request, CancellationToken cancellationToken);

    /// <summary>Starts a customer password reset.</summary>
    Task<HttpResponseMessage> RequestPasswordResetAsync(object request, CancellationToken cancellationToken);

    /// <summary>Confirms a customer password reset.</summary>
    Task<HttpResponseMessage> ConfirmPasswordResetAsync(object request, CancellationToken cancellationToken);
}

/// <summary>
/// Downstream CountryService client used by the Web BFF.
/// </summary>
public interface ICountryServiceClient
{
    /// <summary>Gets a country by ISO 3166-1 alpha-2 code.</summary>
    Task<HttpResponseMessage> GetCountryByIso2Async(string iso2, CancellationToken cancellationToken);
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

    public Task<HttpResponseMessage> UpdateCustomerAsync(Guid customerId, object request, CancellationToken cancellationToken) =>
        httpClient.PatchAsJsonAsync($"/customer/v1/customers/{customerId}", request, cancellationToken);

    public Task<HttpResponseMessage> RegisterCustomerAsync(object request, CancellationToken cancellationToken) =>
        httpClient.PostAsJsonAsync("/customer/v1/customers/register", request, cancellationToken);

    public Task<HttpResponseMessage> GetCustomerAddressesAsync(Guid customerId, CancellationToken cancellationToken) =>
        httpClient.GetAsync($"/customer/v1/addresses?ownerType=Customer&ownerId={customerId}", cancellationToken);

    public Task<HttpResponseMessage> CreateCustomerAddressAsync(object request, CancellationToken cancellationToken) =>
        httpClient.PostAsJsonAsync("/customer/v1/addresses", request, cancellationToken);

    public Task<HttpResponseMessage> UpdateCustomerAddressAsync(Guid addressId, object request, CancellationToken cancellationToken) =>
        httpClient.PatchAsJsonAsync($"/customer/v1/addresses/{addressId}", request, cancellationToken);

    public Task<HttpResponseMessage> DeleteCustomerAddressAsync(Guid addressId, object request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Delete, $"/customer/v1/addresses/{addressId}")
        {
            Content = JsonContent.Create(request)
        };
        return httpClient.SendAsync(message, cancellationToken);
    }
}

internal sealed class AuthServiceClient(HttpClient httpClient) : IAuthServiceClient
{
    public Task<HttpResponseMessage> LoginAsync(object request, CancellationToken cancellationToken) =>
        httpClient.PostAsJsonAsync("/auth/v1/login", request, cancellationToken);

    public Task<HttpResponseMessage> ExchangeCustomerGoogleAsync(object request, CancellationToken cancellationToken) =>
        httpClient.PostAsJsonAsync("/auth/v1/exchange/google/customer", request, cancellationToken);

    public Task<HttpResponseMessage> RequestPasswordResetAsync(object request, CancellationToken cancellationToken) =>
        httpClient.PostAsJsonAsync("/auth/v1/password-reset/request", request, cancellationToken);

    public Task<HttpResponseMessage> ConfirmPasswordResetAsync(object request, CancellationToken cancellationToken) =>
        httpClient.PostAsJsonAsync("/auth/v1/password-reset/confirm", request, cancellationToken);
}

internal sealed class CountryServiceClient(HttpClient httpClient) : ICountryServiceClient
{
    public Task<HttpResponseMessage> GetCountryByIso2Async(string iso2, CancellationToken cancellationToken) =>
        httpClient.GetAsync($"/country/v1/countries/iso2/{Uri.EscapeDataString(iso2)}", cancellationToken);
}
