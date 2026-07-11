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

/// <summary>
/// Downstream DeliveryService client used by Web checkout shipping.
/// </summary>
public interface IDeliveryServiceClient
{
    /// <summary>Gets available shipping couriers.</summary>
    Task<HttpResponseMessage> GetShippingCouriersAsync(CancellationToken cancellationToken);

    /// <summary>Gets live shipping rates.</summary>
    Task<HttpResponseMessage> GetShippingRatesAsync(object request, CancellationToken cancellationToken);

    /// <summary>Gets tracking status for a shipment.</summary>
    Task<HttpResponseMessage> GetShippingTrackingAsync(string trackingCode, CancellationToken cancellationToken);
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

    /// <summary>Gets a company by id.</summary>
    Task<HttpResponseMessage> GetCompanyAsync(Guid companyId, CancellationToken cancellationToken);

    /// <summary>Creates a company.</summary>
    Task<HttpResponseMessage> CreateCompanyAsync(object request, CancellationToken cancellationToken);

    /// <summary>Updates a company by id.</summary>
    Task<HttpResponseMessage> UpdateCompanyAsync(Guid companyId, object request, CancellationToken cancellationToken);

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

    /// <summary>Gets the authoritative customer portal authentication context by principal id.</summary>
    Task<HttpResponseMessage> GetCustomerByPrincipalIdAsync(Guid principalId, CancellationToken cancellationToken);
}

/// <summary>
/// Downstream AuthService client used by the Web BFF.
/// </summary>
public interface IAuthServiceClient
{
    /// <summary>Signs in with AuthService email/password login.</summary>
    Task<HttpResponseMessage> LoginAsync(object request, CancellationToken cancellationToken);

    /// <summary>Issues a one-time AuthService nonce for a customer GIS browser flow.</summary>
    Task<HttpResponseMessage> IssueCustomerGoogleNonceAsync(object request, CancellationToken cancellationToken);

    /// <summary>Exchanges a verified customer Google identity for a MALIEV customer token.</summary>
    Task<HttpResponseMessage> ExchangeCustomerGoogleAsync(object request, CancellationToken cancellationToken);

    /// <summary>Starts a customer password reset.</summary>
    Task<HttpResponseMessage> RequestPasswordResetAsync(object request, CancellationToken cancellationToken);

    /// <summary>Confirms a customer password reset.</summary>
    Task<HttpResponseMessage> ConfirmPasswordResetAsync(object request, CancellationToken cancellationToken);

    /// <summary>Initiates email verification for the current principal.</summary>
    Task<HttpResponseMessage> InitiateEmailVerificationAsync(object request, CancellationToken ct);

    /// <summary>Verifies an email address using a verification token.</summary>
    Task<HttpResponseMessage> VerifyEmailAsync(object request, CancellationToken ct);

    /// <summary>Resends the verification email for the current principal.</summary>
    Task<HttpResponseMessage> ResendVerificationEmailAsync(object request, CancellationToken ct);

    /// <summary>Gets the current principal profile from the AuthService.</summary>
    Task<HttpResponseMessage> GetCurrentPrincipalAsync(Guid principalId, CancellationToken ct);

    /// <summary>Begins passkey registration, returning WebAuthn credential creation options.</summary>
    Task<HttpResponseMessage> PasskeyRegisterBeginAsync(object request, CancellationToken ct);

    /// <summary>Completes passkey registration with the created credential.</summary>
    Task<HttpResponseMessage> PasskeyRegisterCompleteAsync(object request, CancellationToken ct);

    /// <summary>Begins passkey authentication, returning WebAuthn credential request options.</summary>
    Task<HttpResponseMessage> PasskeyAuthBeginAsync(object request, CancellationToken ct);

    /// <summary>Completes passkey authentication with the assertion.</summary>
    Task<HttpResponseMessage> PasskeyAuthCompleteAsync(object request, CancellationToken ct);

    /// <summary>Lists passkey credentials for a principal.</summary>
    Task<HttpResponseMessage> ListPasskeyCredentialsAsync(Guid principalId, CancellationToken ct);

    /// <summary>Deletes a passkey credential.</summary>
    Task<HttpResponseMessage> DeletePasskeyCredentialAsync(Guid credentialId, Guid principalId, CancellationToken ct);
}

/// <summary>
/// Downstream CountryService client used by the Web BFF.
/// </summary>
public interface ICountryServiceClient
{
    /// <summary>Gets a country by ISO 3166-1 alpha-2 code.</summary>
    Task<HttpResponseMessage> GetCountryByIso2Async(string iso2, CancellationToken cancellationToken);

    /// <summary>Gets a page of active countries.</summary>
    Task<HttpResponseMessage> GetCountriesAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Downstream RegistryService client used by customer address entry and company search.
/// </summary>
public interface IRegistryServiceClient
{
    /// <summary>Searches Thai administrative address locations.</summary>
    Task<HttpResponseMessage> SearchThaiLocationsAsync(string query, int limit, CancellationToken cancellationToken);

    /// <summary>Searches Thai companies by name or tax ID via Creden.co (primary) with BDEX fallback.</summary>
    Task<HttpResponseMessage> SearchCompaniesAsync(string query, int limit, CancellationToken cancellationToken);
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
    public Task<HttpResponseMessage> GetShippingCouriersAsync(CancellationToken cancellationToken) =>
        httpClient.GetAsync("/delivery/v1/shipping/couriers", cancellationToken);

    public Task<HttpResponseMessage> GetShippingRatesAsync(object request, CancellationToken cancellationToken) =>
        httpClient.PostAsJsonAsync("/delivery/v1/shipping/rates", request, cancellationToken);

    public Task<HttpResponseMessage> GetShippingTrackingAsync(string trackingCode, CancellationToken cancellationToken) =>
        httpClient.GetAsync($"/delivery/v1/shipping/tracking/{Uri.EscapeDataString(trackingCode)}", cancellationToken);
}

internal sealed class CustomerServiceClient(HttpClient httpClient) : ICustomerServiceClient
{
    public Task<HttpResponseMessage> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken) =>
        httpClient.GetAsync($"/customer/v1/customers/{customerId}", cancellationToken);

    public Task<HttpResponseMessage> UpdateCustomerAsync(Guid customerId, object request, CancellationToken cancellationToken) =>
        httpClient.PatchAsJsonAsync($"/customer/v1/customers/{customerId}", request, cancellationToken);

    public Task<HttpResponseMessage> GetCompanyAsync(Guid companyId, CancellationToken cancellationToken) =>
        httpClient.GetAsync($"/customer/v1/companies/{companyId}", cancellationToken);

    public Task<HttpResponseMessage> CreateCompanyAsync(object request, CancellationToken cancellationToken) =>
        httpClient.PostAsJsonAsync("/customer/v1/companies", request, cancellationToken);

    public Task<HttpResponseMessage> UpdateCompanyAsync(Guid companyId, object request, CancellationToken cancellationToken) =>
        httpClient.PatchAsJsonAsync($"/customer/v1/companies/{companyId}", request, cancellationToken);

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

    public Task<HttpResponseMessage> GetCustomerByPrincipalIdAsync(Guid principalId, CancellationToken cancellationToken) =>
        httpClient.GetAsync(
            $"/customer/v1/customers/by-principal/{principalId}/authentication-context",
            cancellationToken);
}

internal sealed class AuthServiceClient(HttpClient httpClient) : IAuthServiceClient
{
    public Task<HttpResponseMessage> LoginAsync(object request, CancellationToken cancellationToken) =>
        httpClient.PostAsJsonAsync("/auth/v1/login", request, cancellationToken);

    public Task<HttpResponseMessage> IssueCustomerGoogleNonceAsync(object request, CancellationToken cancellationToken) =>
        httpClient.PostAsJsonAsync("/auth/v1/exchange/google/customer/nonce", request, cancellationToken);

    public Task<HttpResponseMessage> ExchangeCustomerGoogleAsync(object request, CancellationToken cancellationToken) =>
        httpClient.PostAsJsonAsync("/auth/v1/exchange/google/customer", request, cancellationToken);

    public Task<HttpResponseMessage> RequestPasswordResetAsync(object request, CancellationToken cancellationToken) =>
        httpClient.PostAsJsonAsync("/auth/v1/password-reset/request", request, cancellationToken);

    public Task<HttpResponseMessage> ConfirmPasswordResetAsync(object request, CancellationToken cancellationToken) =>
        httpClient.PostAsJsonAsync("/auth/v1/password-reset/confirm", request, cancellationToken);

    public Task<HttpResponseMessage> InitiateEmailVerificationAsync(object request, CancellationToken ct) =>
        httpClient.PostAsJsonAsync("/auth/v1/initiate-email-verification", request, ct);

    public Task<HttpResponseMessage> VerifyEmailAsync(object request, CancellationToken ct) =>
        httpClient.PostAsJsonAsync("/auth/v1/verify-email", request, ct);

    public Task<HttpResponseMessage> ResendVerificationEmailAsync(object request, CancellationToken ct) =>
        httpClient.PostAsJsonAsync("/auth/v1/resend-verification-email", request, ct);

    public Task<HttpResponseMessage> GetCurrentPrincipalAsync(Guid principalId, CancellationToken ct) =>
        httpClient.GetAsync($"/auth/v1/me?principalId={principalId}", ct);

    public Task<HttpResponseMessage> PasskeyRegisterBeginAsync(object request, CancellationToken ct) =>
        httpClient.PostAsJsonAsync("/auth/v1/passkey/register/begin", request, ct);

    public Task<HttpResponseMessage> PasskeyRegisterCompleteAsync(object request, CancellationToken ct) =>
        httpClient.PostAsJsonAsync("/auth/v1/passkey/register/complete", request, ct);

    public Task<HttpResponseMessage> PasskeyAuthBeginAsync(object request, CancellationToken ct) =>
        httpClient.PostAsJsonAsync("/auth/v2/passkey/auth/begin", request, ct);

    public Task<HttpResponseMessage> PasskeyAuthCompleteAsync(object request, CancellationToken ct) =>
        httpClient.PostAsJsonAsync("/auth/v2/passkey/auth/complete", request, ct);

    public Task<HttpResponseMessage> ListPasskeyCredentialsAsync(Guid principalId, CancellationToken ct) =>
        httpClient.GetAsync($"/auth/v1/passkey/credentials?principalId={principalId}", ct);

    public Task<HttpResponseMessage> DeletePasskeyCredentialAsync(Guid credentialId, Guid principalId, CancellationToken ct) =>
        httpClient.DeleteAsync($"/auth/v1/passkey/credentials/{credentialId}?principalId={principalId}", ct);
}

internal sealed class CountryServiceClient(HttpClient httpClient) : ICountryServiceClient
{
    public Task<HttpResponseMessage> GetCountryByIso2Async(string iso2, CancellationToken cancellationToken) =>
        httpClient.GetAsync($"/country/v1/countries/iso2/{Uri.EscapeDataString(iso2)}", cancellationToken);

    public Task<HttpResponseMessage> GetCountriesAsync(CancellationToken cancellationToken) =>
        httpClient.GetAsync("/country/v1/countries?pageSize=1000&sortBy=name&sortOrder=asc", cancellationToken);
}

internal sealed class RegistryServiceClient(HttpClient httpClient) : IRegistryServiceClient
{
    public Task<HttpResponseMessage> SearchThaiLocationsAsync(string query, int limit, CancellationToken cancellationToken) =>
        httpClient.GetAsync($"/registry/v1/thai/addresses/autocomplete?query={Uri.EscapeDataString(query)}&limit={limit}", cancellationToken);

    public Task<HttpResponseMessage> SearchCompaniesAsync(string query, int limit, CancellationToken cancellationToken) =>
        httpClient.GetAsync($"/registry/v1/thai/companies/search?query={Uri.EscapeDataString(query)}&limit={limit}", cancellationToken);
}
