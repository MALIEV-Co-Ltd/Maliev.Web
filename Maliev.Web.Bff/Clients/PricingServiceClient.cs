using System.Text.Json;
using Maliev.Web.Bff.Services;

namespace Maliev.Web.Bff.Clients;

internal interface IPricingServiceClient
{
    Task<IReadOnlyList<PricingLeadTimeResponse>> GetLeadTimesAsync(CancellationToken cancellationToken);

    Task<PricingCalculationResponse> CalculateAsync(PricingCalculationRequest request, CancellationToken cancellationToken);
}

internal sealed class PricingServiceClient(HttpClient httpClient, ILogger<PricingServiceClient> logger) : IPricingServiceClient
{
    public async Task<IReadOnlyList<PricingLeadTimeResponse>> GetLeadTimesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<List<PricingLeadTimeResponse>>("/pricing/v1/catalog/lead-times", cancellationToken) ?? [];
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or InvalidOperationException)
        {
            logger.LogWarning(ex, "PricingService failed while loading lead times");
            throw new BackendUnavailableException("PricingService", "PricingService is unavailable while loading lead-time catalog.", ex);
        }
    }

    public async Task<PricingCalculationResponse> CalculateAsync(PricingCalculationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync("/pricing/v1/calculate", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new BackendUnavailableException("PricingService", $"PricingService returned {(int)response.StatusCode} while calculating a price.");
            }

            return await response.Content.ReadFromJsonAsync<PricingCalculationResponse>(cancellationToken)
                ?? throw new BackendUnavailableException("PricingService", "PricingService returned an empty pricing response.");
        }
        catch (BackendUnavailableException)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or InvalidOperationException)
        {
            logger.LogWarning(ex, "PricingService failed while calculating price");
            throw new BackendUnavailableException("PricingService", "PricingService is unavailable while calculating a quote.", ex);
        }
    }
}

internal sealed record PricingLeadTimeResponse(string Code, string Name, int MinDays, int MaxDays, decimal PriceMultiplier, bool IsDefault);

internal sealed record PricingCalculationRequest
{
    public Guid FileId { get; init; }

    public Guid CustomerId { get; init; }

    public Guid MaterialId { get; init; }

    public string MaterialCode { get; init; } = string.Empty;

    public Guid ManufacturingProcessId { get; init; }

    public string ManufacturingProcessName { get; init; } = string.Empty;

    public decimal Quantity { get; init; }

    public string Currency { get; init; } = "THB";

    public PricingGeometryMetrics Geometry { get; init; } = new();

    public string? StoragePath { get; init; }

    public string? LeadTimeCode { get; init; }
}

internal sealed record PricingGeometryMetrics
{
    public decimal VolumeCm3 { get; init; }

    public bool IsManifold { get; init; } = true;
}

internal sealed record PricingCalculationResponse
{
    public decimal UnitPrice { get; init; }

    public decimal TotalAmount { get; init; }

    public decimal UnitPriceBeforeVolumeDiscount { get; init; }

    public decimal VolumeDiscountUnitAmount { get; init; }

    public decimal VolumeDiscountPercent { get; init; }

    public decimal ConfidenceScore { get; init; }

    public string EngineName { get; init; } = string.Empty;

    public int EstimatedLeadTimeDays { get; init; }
}
