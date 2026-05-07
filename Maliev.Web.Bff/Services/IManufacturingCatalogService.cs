using Maliev.Web.Shared.Quotes;

namespace Maliev.Web.Bff.Services;

/// <summary>
/// Provides manufacturing quote reference data from MaterialService and PricingService.
/// </summary>
public interface IManufacturingCatalogService
{
    /// <summary>Gets manufacturing process, material, and lead-time options.</summary>
    Task<QuoteReferenceDataDto> GetReferenceDataAsync(CancellationToken cancellationToken);
}
