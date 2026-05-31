using Maliev.Web.Bff.Clients;
using Maliev.Web.Bff.Services;
using Maliev.Web.Shared.Quotes;

namespace Maliev.Web.Tests;

/// <summary>
/// Tests for the quote reference data adapter that fronts MaterialService.
/// </summary>
public sealed class ManufacturingCatalogServiceTests
{
    /// <summary>
    /// Verifies Web quote reference data includes the visual configurator options exposed by MaterialService.
    /// </summary>
    [Fact]
    public async Task GetReferenceDataAsync_CombinesFinishesAndConfigOptionsByProcess()
    {
        var materialId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var finishId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var service = new ManufacturingCatalogService(
            new FakeMaterialServiceClient(materialId, finishId),
            new FakePricingServiceClient());

        var reference = await service.GetReferenceDataAsync(CancellationToken.None);

        var finish = Assert.Single(reference.SurfaceFinishes);
        Assert.Equal(finishId, finish.Id);
        Assert.Equal("FDM", finish.ProcessCode);
        Assert.Equal("AS_PRINTED", finish.Code);
        Assert.Contains(materialId, finish.CompatibleMaterialIds);

        var option = Assert.Single(reference.ProcessOptions);
        Assert.Equal("FDM", option.ProcessCode);
        Assert.Equal("color", option.ConfigKey);
        Assert.Equal("dropdown", option.ConfigType);
        Assert.Equal("""["WHITE","BLACK"]""", option.OptionsJson);
    }

    private sealed class FakeMaterialServiceClient(Guid materialId, Guid finishId) : IMaterialServiceClient
    {
        public Task<IReadOnlyList<MaterialProcessCatalogResponse>> GetProcessesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<MaterialProcessCatalogResponse>>(
            [
                new()
                {
                    Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    Code = "FDM",
                    Name = "3D Printing (FDM)"
                }
            ]);
        }

        public Task<IReadOnlyList<MaterialCatalogResponse>> GetMaterialsAsync(string processCode, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<MaterialCatalogResponse>>(
            [
                new()
                {
                    Id = materialId,
                    Code = "PLA",
                    Name = "PLA"
                }
            ]);
        }

        public Task<IReadOnlyList<SurfaceFinishCatalogResponse>> GetFinishesAsync(string processCode, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<SurfaceFinishCatalogResponse>>(
            [
                new()
                {
                    Id = finishId,
                    Code = "AS_PRINTED",
                    Name = "As-printed",
                    SortOrder = 10
                }
            ]);
        }

        public Task<IReadOnlyList<SurfaceFinishCatalogResponse>> GetMaterialFinishesAsync(Guid materialId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<SurfaceFinishCatalogResponse>>(
            [
                new()
                {
                    Id = finishId,
                    Code = "AS_PRINTED",
                    Name = "As-printed",
                    SortOrder = 10
                }
            ]);
        }

        public Task<IReadOnlyList<ProcessConfigOptionCatalogResponse>> GetConfigOptionsAsync(string processCode, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<ProcessConfigOptionCatalogResponse>>(
            [
                new()
                {
                    Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                    ConfigKey = "color",
                    Label = "Color",
                    ConfigType = "dropdown",
                    DefaultValue = "WHITE",
                    OptionsJson = """["WHITE","BLACK"]""",
                    SortOrder = 10
                }
            ]);
        }
    }

    private sealed class FakePricingServiceClient : IPricingServiceClient
    {
        public Task<IReadOnlyList<PricingLeadTimeResponse>> GetLeadTimesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<PricingLeadTimeResponse>>(
            [
                new("STANDARD", "Standard", 5, 7, 1m, true)
            ]);
        }

        public Task<PricingCalculationResponse> CalculateAsync(PricingCalculationRequest request, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
