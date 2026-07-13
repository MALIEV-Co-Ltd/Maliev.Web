using Maliev.Web.Bff.Clients;
using Maliev.Web.Bff.Services;
using Maliev.Web.Shared.Localization;
using Maliev.Web.Shared.Quotes;

namespace Maliev.Web.Tests;

/// <summary>Pricing payload regressions for server-owned geometry.</summary>
public sealed class WebQuoteAuthoritativePricingTests
{
    /// <summary>Every authoritative geometry field reaches PricingService unchanged.</summary>
    [Fact]
    public async Task EstimateAsync_AuthoritativeGeometry_MapsExactPricingPayload()
    {
        var processId = Guid.NewGuid();
        var materialId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var pricing = new CapturingPricingClient();
        var service = new WebQuoteService(
            pricing,
            new FixedCatalog(processId, materialId));

        await service.EstimateAsync(new QuoteEstimateRequest
        {
            Parts =
            [
                new QuotePartDraftDto
                {
                    FileId = fileId,
                    File = new QuoteFileDraftDto { Name = "fixture.step" },
                    ManufacturingProcessId = processId,
                    MaterialId = materialId,
                    AuthoritativeVolumeCc = 12.5m,
                    AuthoritativeSupportVolumeCc = 1.25m,
                    AuthoritativeSurfaceAreaCm2 = 42.5m,
                    AuthoritativeBoundingBoxX = 10m,
                    AuthoritativeBoundingBoxY = 20m,
                    AuthoritativeBoundingBoxZ = 30m,
                    AuthoritativeIsManifold = false,
                    AuthoritativeTriangleCount = 456,
                    Quantity = 2,
                    DfmAcknowledged = true
                }
            ]
        }, CancellationToken.None);

        var request = Assert.IsType<PricingCalculationRequest>(pricing.Request);
        Assert.Equal(fileId, request.FileId);
        Assert.Equal(12.5m, request.Geometry.VolumeCm3);
        Assert.Equal(1.25m, request.Geometry.SupportVolumeCm3);
        Assert.Equal(42.5m, request.Geometry.SurfaceAreaCm2);
        Assert.Equal(10m, request.Geometry.BoundingBoxX);
        Assert.Equal(20m, request.Geometry.BoundingBoxY);
        Assert.Equal(30m, request.Geometry.BoundingBoxZ);
        Assert.False(request.Geometry.IsManifold);
        Assert.Equal(456, request.Geometry.TriangleCount);
    }

    private sealed class FixedCatalog(Guid processId, Guid materialId) : IManufacturingCatalogService
    {
        public Task<QuoteReferenceDataDto> GetReferenceDataAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new QuoteReferenceDataDto
            {
                Processes =
                [
                    new ServiceProcessDto
                    {
                        Id = processId,
                        Code = "FDM",
                        Name = new LocalizedText { En = "FDM" }
                    }
                ],
                Materials =
                [
                    new MaterialOptionDto
                    {
                        Id = materialId,
                        Code = "PLA",
                        ProcessCode = "FDM",
                        Name = new LocalizedText { En = "PLA" }
                    }
                ]
            });
    }

    private sealed class CapturingPricingClient : IPricingServiceClient
    {
        public PricingCalculationRequest? Request { get; private set; }

        public Task<IReadOnlyList<PricingLeadTimeResponse>> GetLeadTimesAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<PricingLeadTimeResponse>>([]);

        public Task<PricingCalculationResponse> CalculateAsync(
            PricingCalculationRequest request,
            CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(new PricingCalculationResponse
            {
                UnitPrice = 100,
                TotalAmount = 200,
                UnitPriceBeforeVolumeDiscount = 100
            });
        }
    }
}
