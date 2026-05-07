using Maliev.Web.Shared.Quotes;

namespace Maliev.Web.Bff.Services;

/// <summary>
/// Creates customer-facing quote estimates through downstream pricing contracts.
/// </summary>
public interface IWebQuoteService
{
    /// <summary>Calculates a quote estimate through PricingService.</summary>
    Task<QuoteEstimateResponse> EstimateAsync(QuoteEstimateRequest request, CancellationToken cancellationToken);
}
