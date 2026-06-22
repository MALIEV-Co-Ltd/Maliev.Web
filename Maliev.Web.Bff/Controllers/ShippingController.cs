using System.Net.Http.Json;
using Asp.Versioning;
using Maliev.Web.Bff.Clients;
using Maliev.Web.Shared.Commerce;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.Web.Bff.Controllers;

/// <summary>
/// Customer storefront shipping API backed by DeliveryService.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("web/v{version:apiVersion}/shipping")]
public sealed class ShippingController(IDeliveryServiceClient deliveryServiceClient) : ControllerBase
{
    /// <summary>Gets available courier options.</summary>
    [HttpGet("couriers")]
    public async Task<ActionResult<IReadOnlyList<CheckoutShippingCourierDto>>> GetCouriers(CancellationToken cancellationToken)
    {
        using var response = await deliveryServiceClient.GetShippingCouriersAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        var couriers = await response.Content.ReadFromJsonAsync<List<CheckoutShippingCourierDto>>(cancellationToken) ?? [];
        return Ok(couriers);
    }

    /// <summary>Gets live courier rates for checkout.</summary>
    [HttpPost("rates")]
    public async Task<ActionResult<CheckoutShippingRateResponse>> GetRates(
        [FromBody] CheckoutShippingRateRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateRateRequest(request);
        if (validationError is not null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Shipping details required",
                Detail = validationError,
                Status = StatusCodes.Status400BadRequest
            });
        }

        using var response = await deliveryServiceClient.GetShippingRatesAsync(BuildDeliveryServiceRateRequest(request), cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        var downstreamRates = await response.Content.ReadFromJsonAsync<List<DeliveryServiceShippingRate>>(cancellationToken) ?? [];
        return Ok(new CheckoutShippingRateResponse
        {
            Rates = downstreamRates.Select(rate => new CheckoutShippingRateDto
            {
                CourierCode = rate.CourierCode,
                ProductName = FirstNonEmpty(rate.CourierName, rate.CourierCode),
                TotalPrice = rate.Price,
                CurrencyCode = FirstNonEmpty(rate.Currency, "THB"),
                EstimatedDeliveryDate = rate.EstimatedDelivery,
                ServiceLevel = rate.ServiceLevel,
                Provider = rate.Provider
            }).ToList()
        });
    }

    /// <summary>Gets current tracking status for a shipment.</summary>
    [HttpGet("tracking/{trackingCode}")]
    public async Task<ActionResult<CheckoutShippingTrackingDto>> GetTracking(
        [FromRoute] string trackingCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(trackingCode))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Tracking code required",
                Detail = "Enter a tracking code before checking shipment status.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        using var response = await deliveryServiceClient.GetShippingTrackingAsync(trackingCode.Trim(), cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        var tracking = await response.Content.ReadFromJsonAsync<CheckoutShippingTrackingDto>(cancellationToken);
        return tracking is null ? NotFound() : Ok(tracking);
    }

    private static string? ValidateRateRequest(CheckoutShippingRateRequest request)
    {
        var details = request.ShippingDetails;
        if (string.IsNullOrWhiteSpace(details.Phone) ||
            string.IsNullOrWhiteSpace(details.Address) ||
            string.IsNullOrWhiteSpace(details.District) ||
            string.IsNullOrWhiteSpace(details.State) ||
            string.IsNullOrWhiteSpace(details.Province) ||
            string.IsNullOrWhiteSpace(details.Postcode))
        {
            return "Enter destination phone, address, district, state, province, and postal code before checking courier rates.";
        }

        if (details.WeightGrams <= 0 || details.LengthCm <= 0 || details.WidthCm <= 0 || details.HeightCm <= 0)
        {
            return "Enter positive parcel weight and dimensions before checking courier rates.";
        }

        return null;
    }

    private static object BuildDeliveryServiceRateRequest(CheckoutShippingRateRequest request)
    {
        var details = request.ShippingDetails;
        return new
        {
            from = new
            {
                name = "MALIEV",
                address = "MALIEV",
                district = "Pathum Wan",
                state = "Pathum Wan",
                province = "Bangkok",
                postcode = "10400",
                countryCode = "TH",
                tel = "020000000"
            },
            to = new
            {
                name = FirstNonEmpty(details.RecipientName, "Customer"),
                address = details.Address.Trim(),
                district = details.District.Trim(),
                state = details.State.Trim(),
                province = details.Province.Trim(),
                postcode = details.Postcode.Trim(),
                countryCode = FirstNonEmpty(details.CountryCode, "TH"),
                tel = details.Phone.Trim()
            },
            parcel = new
            {
                name = "MALIEV checkout shipment",
                weight = details.WeightGrams,
                length = details.LengthCm,
                width = details.WidthCm,
                height = details.HeightCm
            },
            courierCodes = request.CourierCodes
        };
    }

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;

    private sealed class DeliveryServiceShippingRate
    {
        public string CourierCode { get; set; } = string.Empty;

        public string CourierName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string Currency { get; set; } = "THB";

        public string? ServiceLevel { get; set; }

        public string? EstimatedDelivery { get; set; }

        public string Provider { get; set; } = string.Empty;
    }
}
