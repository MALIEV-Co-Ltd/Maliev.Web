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
                CountryCode = "AU",
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
        Assert.Equal("AU", client.Payload.RootElement.GetProperty("to").GetProperty("countryCode").GetString());
        Assert.Equal("flash", client.Payload.RootElement.GetProperty("courierCodes")[0].GetString());
    }

    /// <summary>
    /// Verifies shipment tracking is proxied through DeliveryService.
    /// </summary>
    [Fact]
    public async Task GetTracking_MapsDeliveryServiceTrackingStatus()
    {
        var client = new CapturingDeliveryServiceClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new CheckoutShippingTrackingDto
            {
                TrackingCode = "TH-E2E-001",
                CourierCode = "thaipost",
                CourierName = "Thailand Post",
                Status = "in_transit",
                Description = "Parcel is in transit",
                Provider = "Shippop"
            })
        });
        var controller = new ShippingController(client);

        var result = await controller.GetTracking("TH-E2E-001", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var tracking = Assert.IsType<CheckoutShippingTrackingDto>(ok.Value);
        Assert.Equal("TH-E2E-001", tracking.TrackingCode);
        Assert.Equal("thaipost", tracking.CourierCode);
        Assert.Equal("in_transit", tracking.Status);
        Assert.Equal("Shippop", tracking.Provider);
        Assert.Equal("TH-E2E-001", client.TrackingCode);
    }

    private sealed class CapturingDeliveryServiceClient(HttpResponseMessage response) : IDeliveryServiceClient
    {
        public JsonDocument? Payload { get; private set; }

        public string? TrackingCode { get; private set; }

        public Task<HttpResponseMessage> GetShippingCouriersAsync(CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public async Task<HttpResponseMessage> GetShippingRatesAsync(object request, CancellationToken cancellationToken)
        {
            Payload = JsonDocument.Parse(JsonSerializer.Serialize(request));
            await Task.Yield();
            return response;
        }

        public async Task<HttpResponseMessage> GetShippingTrackingAsync(string trackingCode, CancellationToken cancellationToken)
        {
            TrackingCode = trackingCode;
            await Task.Yield();
            return response;
        }
    }
}
