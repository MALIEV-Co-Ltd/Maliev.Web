using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Maliev.Web.Bff.Clients;
using Maliev.Web.Bff.Services;
using Maliev.Web.Shared.Commerce;

namespace Maliev.Web.Tests;

/// <summary>
/// Unit tests for checkout draft boundary mapping.
/// </summary>
public sealed class CheckoutDraftServiceTests
{
    /// <summary>
    /// Verifies customer checkout details are snapshotted into CommerceService checkout-session JSON fields.
    /// </summary>
    [Fact]
    public async Task CreateDraftAsync_WithCheckoutDetails_SendsBillingAndShippingSnapshotsToCommerceService()
    {
        var customerId = Guid.Parse("db64cbdf-bfb7-4ffc-9aa1-ff3fd7bc9188");
        var commerceClient = new CapturingCommerceServiceClient(customerId);
        var service = new CheckoutDraftService(commerceClient, new SuccessfulCustomerServiceClient());

        var response = await service.CreateDraftAsync(new CheckoutDraftRequest
        {
            Culture = "en-US",
            Phone = "+66 81 000 1111",
            CompanyName = "MALIEV Customer Co., Ltd.",
            VatId = "0105566000000",
            BillingAddress = "Billing Tower, Bangkok",
            ShippingAddress = "Factory Dock 7, Chonburi",
            TermsAccepted = true,
            Items =
            [
                new CartItemDto
                {
                    ProductHandle = "pneumatic-injection-molding-machine-30g",
                    VariantSku = "PIMM-30-STD",
                    Quantity = 2
                }
            ]
        }, CreateCustomerPrincipal(customerId), CancellationToken.None);

        Assert.False(response.RequiresSignIn);
        Assert.NotNull(commerceClient.LastCheckoutSessionRequest);
        var shippingJson = ReadStringProperty(commerceClient.LastCheckoutSessionRequest, "ShippingAddressJson");
        var billingJson = ReadStringProperty(commerceClient.LastCheckoutSessionRequest, "BillingAddressJson");

        Assert.NotNull(shippingJson);
        Assert.NotNull(billingJson);
        using var shipping = JsonDocument.Parse(shippingJson);
        using var billing = JsonDocument.Parse(billingJson);
        Assert.Equal("Factory Dock 7, Chonburi", shipping.RootElement.GetProperty("address").GetString());
        Assert.Equal("+66 81 000 1111", shipping.RootElement.GetProperty("phone").GetString());
        Assert.Equal("MALIEV Customer Co., Ltd.", shipping.RootElement.GetProperty("companyName").GetString());
        Assert.Equal("0105566000000", billing.RootElement.GetProperty("vatId").GetString());
        Assert.True(billing.RootElement.GetProperty("termsAccepted").GetBoolean());
    }

    /// <summary>
    /// Verifies checkout cannot create downstream cart or checkout-session state until terms are accepted.
    /// </summary>
    [Fact]
    public async Task CreateDraftAsync_WithoutAcceptedTerms_RejectsBeforeDownstreamCalls()
    {
        var customerId = Guid.Parse("db64cbdf-bfb7-4ffc-9aa1-ff3fd7bc9188");
        var commerceClient = new CapturingCommerceServiceClient(customerId);
        var customerClient = new CapturingCustomerServiceClient();
        var service = new CheckoutDraftService(commerceClient, customerClient);

        var exception = await Assert.ThrowsAsync<CheckoutValidationException>(() =>
            service.CreateDraftAsync(new CheckoutDraftRequest
            {
                Culture = "en-US",
                TermsAccepted = false,
                Items =
                [
                    new CartItemDto
                    {
                        ProductHandle = "pneumatic-injection-molding-machine-30g",
                        VariantSku = "PIMM-30-STD",
                        Quantity = 1
                    }
                ]
            }, CreateCustomerPrincipal(customerId), CancellationToken.None));

        Assert.Contains("terms", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(customerClient.WasCalled);
        Assert.Equal(0, commerceClient.CallCount);
    }

    private static ClaimsPrincipal CreateCustomerPrincipal(Guid customerId)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("customer_id", customerId.ToString())
        ], "Test"));
    }

    private static string? ReadStringProperty(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        return property?.GetValue(instance) as string;
    }

    private sealed class SuccessfulCustomerServiceClient : ICustomerServiceClient
    {
        public Task<HttpResponseMessage> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { id = customerId }) });

        public Task<HttpResponseMessage> UpdateCustomerAsync(Guid customerId, object request, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<HttpResponseMessage> GetCompanyAsync(Guid companyId, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<HttpResponseMessage> CreateCompanyAsync(object request, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<HttpResponseMessage> UpdateCompanyAsync(Guid companyId, object request, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<HttpResponseMessage> RegisterCustomerAsync(object request, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<HttpResponseMessage> GetCustomerAddressesAsync(Guid customerId, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<HttpResponseMessage> CreateCustomerAddressAsync(object request, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<HttpResponseMessage> UpdateCustomerAddressAsync(Guid addressId, object request, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<HttpResponseMessage> DeleteCustomerAddressAsync(Guid addressId, object request, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<HttpResponseMessage> GetCustomerByPrincipalIdAsync(Guid principalId, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class CapturingCustomerServiceClient : ICustomerServiceClient
    {
        public bool WasCalled { get; private set; }

        public Task<HttpResponseMessage> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken)
        {
            WasCalled = true;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { id = customerId }) });
        }

        public Task<HttpResponseMessage> UpdateCustomerAsync(Guid customerId, object request, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<HttpResponseMessage> GetCompanyAsync(Guid companyId, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<HttpResponseMessage> CreateCompanyAsync(object request, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<HttpResponseMessage> UpdateCompanyAsync(Guid companyId, object request, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<HttpResponseMessage> RegisterCustomerAsync(object request, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<HttpResponseMessage> GetCustomerAddressesAsync(Guid customerId, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<HttpResponseMessage> CreateCustomerAddressAsync(object request, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<HttpResponseMessage> UpdateCustomerAddressAsync(Guid addressId, object request, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<HttpResponseMessage> DeleteCustomerAddressAsync(Guid addressId, object request, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<HttpResponseMessage> GetCustomerByPrincipalIdAsync(Guid principalId, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class CapturingCommerceServiceClient(Guid customerId) : ICommerceServiceClient
    {
        public object? LastCheckoutSessionRequest { get; private set; }

        public int CallCount { get; private set; }

        public Task<HttpResponseMessage> ListCollectionsAsync(CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<HttpResponseMessage> GetCollectionAsync(string handle, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<HttpResponseMessage> ListProductsAsync(string? collection, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<HttpResponseMessage> GetProductAsync(string handle, CancellationToken cancellationToken)
        {
            CallCount++;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    id = Guid.NewGuid(),
                    handle,
                    variants = new[]
                    {
                        new
                        {
                            id = Guid.Parse("7d7f1ec0-2480-43ad-aecf-ee0831b2cfdb"),
                            sku = "PIMM-30-STD",
                            title = "30g starter package",
                            priceAmount = 99000m,
                            currency = "THB",
                            inventoryQuantity = 8,
                            isActive = true
                        }
                    }
                })
            };
            return Task.FromResult(response);
        }

        public Task<HttpResponseMessage> CreateCartAsync(object request, CancellationToken cancellationToken)
        {
            CallCount++;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    id = Guid.Parse("f67b51f2-8d75-4858-86a7-d838263716e8"),
                    customerId,
                    anonymousKey = (string?)null,
                    status = "Active",
                    currency = "THB",
                    lines = Array.Empty<object>(),
                    totalAmount = 198000m
                })
            };
            return Task.FromResult(response);
        }

        public Task<HttpResponseMessage> UpsertCartLineAsync(Guid cartId, object request, CancellationToken cancellationToken)
        {
            CallCount++;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    id = cartId,
                    customerId,
                    anonymousKey = (string?)null,
                    status = "Active",
                    currency = "THB",
                    lines = Array.Empty<object>(),
                    totalAmount = 198000m
                })
            };
            return Task.FromResult(response);
        }

        public Task<HttpResponseMessage> CreateCheckoutSessionAsync(object request, CancellationToken cancellationToken)
        {
            CallCount++;
            LastCheckoutSessionRequest = request;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    id = Guid.Parse("ef8631ca-8718-4379-b344-bb63a52e2d2c"),
                    cartId = Guid.Parse("f67b51f2-8d75-4858-86a7-d838263716e8"),
                    customerId,
                    status = "Pending",
                    totalAmount = 198000m,
                    currency = "THB",
                    expiresAtUtc = DateTimeOffset.UtcNow.AddHours(2)
                })
            };
            return Task.FromResult(response);
        }
    }
}
