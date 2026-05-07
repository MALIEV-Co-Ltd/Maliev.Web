using Maliev.Web.Shared.Quotes;

namespace Maliev.Web.Bff.Services;

internal static class QuoteEstimator
{
    internal static QuoteEstimateResponse Estimate(QuoteEstimateRequest request)
    {
        var lines = request.Parts.Select(part =>
        {
            var baseUnit = ResolveBaseUnit(part);
            var discountRate = part.Quantity >= 50 ? 0.15m : part.Quantity >= 10 ? 0.08m : part.Quantity >= 5 ? 0.04m : 0m;
            var leadTimeMultiplier = request.LeadTimeCode.Equals("RUSH", StringComparison.OrdinalIgnoreCase) ? 1.35m : 1m;
            var discountedUnit = Math.Round(baseUnit * leadTimeMultiplier * (1 - discountRate), 2);
            var total = Math.Round(discountedUnit * Math.Max(1, part.Quantity), 2);
            var bulkDiscount = Math.Round((baseUnit * leadTimeMultiplier - discountedUnit) * Math.Max(1, part.Quantity), 2);

            return new QuoteLineEstimateDto
            {
                PartId = part.Id,
                Name = part.File.Name,
                BaseUnitPrice = Math.Round(baseUnit * leadTimeMultiplier, 2),
                UnitPrice = discountedUnit,
                BulkDiscountAmount = bulkDiscount,
                Total = total,
                Warning = ResolveWarning(part)
            };
        }).ToList();

        var subtotal = lines.Sum(line => line.Total);
        var tax = Math.Round(subtotal * 0.07m, 2);
        return new QuoteEstimateResponse
        {
            Lines = lines,
            Subtotal = subtotal,
            BulkDiscountTotal = lines.Sum(line => line.BulkDiscountAmount),
            TaxAmount = tax,
            Total = subtotal + tax,
            RequiresManualReview = request.Parts.Any(part => !SupportsInstantQuote(part.ProcessCode) || !part.DfmAcknowledged && part.EstimatedVolumeCc > 220m),
            PricingSource = "Web BFF contract placeholder; production path must delegate to PricingService pricing/v{version}/calculate and pricing/v{version}/catalog/bulk-pricing."
        };
    }

    private static decimal ResolveBaseUnit(QuotePartDraftDto part)
    {
        var processBase = part.ProcessCode switch
        {
            "SLA" => 700m,
            "CNC_MILL" => 1850m,
            "SCAN" => 2500m,
            "DESIGN" => 1800m,
            _ => 420m
        };

        return Math.Round(processBase + Math.Max(1m, part.EstimatedVolumeCc) * 12.5m, 2);
    }

    private static bool SupportsInstantQuote(string processCode)
    {
        return processCode is "FDM" or "SLA" or "CNC_MILL";
    }

    private static string? ResolveWarning(QuotePartDraftDto part)
    {
        if (part.EstimatedVolumeCc > 220m && !part.DfmAcknowledged)
        {
            return "Large part requires DFM acknowledgement before a formal quote can be issued.";
        }

        return SupportsInstantQuote(part.ProcessCode) ? null : "This service needs manual review before final pricing.";
    }
}
