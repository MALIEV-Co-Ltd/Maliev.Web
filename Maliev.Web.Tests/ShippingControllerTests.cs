using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Maliev.Web.Bff.Clients;
using Maliev.Web.Bff.Controllers;
using Maliev.Web.Shared.Commerce;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.Web.Tests;

/// <summary>
/// Unit tests for checkout shipping controller mapping.
/// </summary>
public sealed class ShippingControllerTests
{
    /// <summary>
    /// Verifies DeliveryService provider metadata is exposed on checkout rate options.
    /// </summary>
    [Fact]
    public async Task GetRates_MapsDeliveryServiceProviderToCheckoutRate()
    {
        var client = new CapturingDeliveryServiceClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new[]
            {
                new
                {
                    courierCode = "flash",
                    courierName = "Flash Express",
                    price = 82.25m,
                    currency = "THB",
                    serviceLevel = "standard",
                    estimatedDelivery = "2026-06-22",
                    provider = "GoShip"
                }
            })
        });
        var controller = new ShippingController(client);

        var result = await controller.GetRates(new CheckoutShippingRateRequest
        {
            ShippingDetails = new CheckoutShippingDetailsDto
            {
                RecipientName = "Customer",
                Phone = "0800000000",
                Address = "Dock 2",
                District = "Bang Rak",
                State = "Bang Rak",
                Province = "Bangkok",
                Postcode = "10500",
                WeightGrams = 1250m,
                LengthCm = 20m,
                WidthCm = 15m,
                HeightCm = 10m
            },
            CourierCodes = ["flash"]
        }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<CheckoutShippingRateResponse>(ok.Value);
        var rate = Assert.Single(response.Rates);
        Assert.Equal("flash", rate.CourierCode);
        Assert.Equal("Flash Express", rate.ProductName);
        Assert.Equal(82.25m, rate.TotalPrice);
        Assert.Equal("GoShip", rate.Provider);
        Assert.NotNull(client.Payload);
        Assert.Equal("10500", client.Payload.RootElement.GetProperty("to").GetProperty("postcode").GetString());
        Assert.Equal("flash", client.Payload.RootElement.GetProperty("courierCodes")[0].GetString());
    }

    private sealed class CapturingDeliveryServiceClient(HttpResponseMessage ratesResponse) : IDeliveryServiceClient
    {
        public JsonDocument? Payload { get; private set; }

        public Task<HttpResponseMessage> GetShippingCouriersAsync(CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public async Task<HttpResponseMessage> GetShippingRatesAsync(object request, CancellationToken cancellationToken)
        {
            Payload = JsonDocument.Parse(JsonSerializer.Serialize(request));
            await Task.Yield();
            return ratesResponse;
        }

        public Task<HttpResponseMessage> GetShippingTrackingAsync(string trackingCode, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
