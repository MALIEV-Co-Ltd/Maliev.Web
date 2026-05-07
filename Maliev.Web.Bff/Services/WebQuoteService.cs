using Maliev.Web.Bff.Clients;
using Maliev.Web.Shared.Quotes;

namespace Maliev.Web.Bff.Services;

internal sealed class WebQuoteService(IPricingServiceClient pricingClient, IManufacturingCatalogService manufacturingCatalog) : IWebQuoteService
{
    public async Task<QuoteEstimateResponse> EstimateAsync(QuoteEstimateRequest request, CancellationToken cancellationToken)
    {
        if (request.Parts.Count == 0)
        {
            return new QuoteEstimateResponse { PricingSource = "PricingService" };
        }

        var referenceData = await manufacturingCatalog.GetReferenceDataAsync(cancellationToken);
        var lines = new List<QuoteLineEstimateDto>();
        foreach (var part in request.Parts)
        {
            var fileId = ResolveFileId(part);
            var process = referenceData.Processes.FirstOrDefault(option =>
                option.Code.Equals(part.ProcessCode, StringComparison.OrdinalIgnoreCase) ||
                option.Id == part.ManufacturingProcessId);
            var material = referenceData.Materials.FirstOrDefault(option =>
                option.Code.Equals(part.MaterialCode, StringComparison.OrdinalIgnoreCase) ||
                option.Id == part.MaterialId);

            if (fileId is null || process?.Id is null || material?.Id is null || part.EstimatedVolumeCc <= 0m)
            {
                throw new QuoteNotReadyException("Pricing requires a completed UploadService upload, downstream process/material identifiers, and GeometryService volume metrics.");
            }

            var quantity = Math.Max(1, part.Quantity);
            var pricing = await pricingClient.CalculateAsync(new PricingCalculationRequest
            {
                FileId = fileId.Value,
                CustomerId = request.CustomerId ?? Guid.Empty,
                MaterialId = material.Id.Value,
                MaterialCode = material.Code,
                ManufacturingProcessId = process.Id.Value,
                ManufacturingProcessName = process.Code,
                Quantity = quantity,
                Currency = request.CurrencyCode,
                LeadTimeCode = request.LeadTimeCode,
                StoragePath = part.StoragePath,
                Geometry = new PricingGeometryMetrics
                {
                    VolumeCm3 = part.EstimatedVolumeCc,
                    IsManifold = true
                }
            }, cancellationToken);

            var baseUnitPrice = pricing.UnitPriceBeforeVolumeDiscount > 0m
                ? pricing.UnitPriceBeforeVolumeDiscount
                : pricing.UnitPrice + pricing.VolumeDiscountUnitAmount;

            lines.Add(new QuoteLineEstimateDto
            {
                PartId = part.Id,
                Name = part.File.Name,
                BaseUnitPrice = baseUnitPrice,
                UnitPrice = pricing.UnitPrice,
                BulkDiscountAmount = pricing.VolumeDiscountUnitAmount * quantity,
                Total = pricing.TotalAmount,
                Warning = part.DfmAcknowledged ? null : "DFM acknowledgement is required before this quote can be submitted."
            });
        }

        var subtotal = lines.Sum(line => line.Total);
        return new QuoteEstimateResponse
        {
            Lines = lines,
            Subtotal = subtotal,
            BulkDiscountTotal = lines.Sum(line => line.BulkDiscountAmount),
            TaxAmount = 0m,
            Total = subtotal,
            RequiresManualReview = lines.Any(line => line.Warning is not null),
            PricingSource = "PricingService"
        };
    }

    private static Guid? ResolveFileId(QuotePartDraftDto part)
    {
        if (part.FileId.HasValue)
        {
            return part.FileId.Value;
        }

        return Guid.TryParse(part.UploadId, out var parsed) ? parsed : null;
    }
}
