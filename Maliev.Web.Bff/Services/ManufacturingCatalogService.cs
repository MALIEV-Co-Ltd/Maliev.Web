using Maliev.Web.Bff.Clients;
using Maliev.Web.Shared.Localization;
using Maliev.Web.Shared.Quotes;

namespace Maliev.Web.Bff.Services;

internal sealed class ManufacturingCatalogService(IMaterialServiceClient materialClient, IPricingServiceClient pricingClient) : IManufacturingCatalogService
{
    public async Task<QuoteReferenceDataDto> GetReferenceDataAsync(CancellationToken cancellationToken)
    {
        var processes = await materialClient.GetProcessesAsync(cancellationToken);
        var processCatalogs = await Task.WhenAll(processes.Select(process => LoadProcessCatalogAsync(process, cancellationToken)));
        var materials = processCatalogs.SelectMany(catalog => catalog.Materials).ToList();
        var surfaceFinishes = processCatalogs.SelectMany(catalog => catalog.SurfaceFinishes).ToList();
        var processOptions = processCatalogs.SelectMany(catalog => catalog.ProcessOptions).ToList();
        var finishCompatibility = await LoadFinishCompatibilityAsync(materials, cancellationToken);

        foreach (var surfaceFinish in surfaceFinishes.Where(finish => finish.Id.HasValue))
        {
            if (finishCompatibility.TryGetValue(surfaceFinish.Id!.Value, out var materialIds))
            {
                surfaceFinish.CompatibleMaterialIds = materialIds;
            }
        }

        var leadTimes = await pricingClient.GetLeadTimesAsync(cancellationToken);

        return new QuoteReferenceDataDto
        {
            Processes = processes.Select(process => new ServiceProcessDto
            {
                Id = process.Id,
                Code = process.Code,
                Name = new LocalizedText { En = process.Name },
                Summary = new LocalizedText { En = process.Description ?? string.Empty },
                SupportsInstantQuote = IsInstantQuoteProcess(process.Code)
            }).ToList(),
            Materials = materials,
            SurfaceFinishes = surfaceFinishes,
            ProcessOptions = processOptions,
            LeadTimeCodes = leadTimes.Select(leadTime => leadTime.Code).ToList()
        };
    }

    private async Task<ProcessCatalogData> LoadProcessCatalogAsync(MaterialProcessCatalogResponse process, CancellationToken cancellationToken)
    {
        var materialsTask = materialClient.GetMaterialsAsync(process.Code, cancellationToken);
        var finishesTask = materialClient.GetFinishesAsync(process.Code, cancellationToken);
        var configOptionsTask = materialClient.GetConfigOptionsAsync(process.Code, cancellationToken);

        await Task.WhenAll(materialsTask, finishesTask, configOptionsTask);

        var materials = materialsTask.Result.Select(material => new MaterialOptionDto
        {
            Id = material.Id,
            Code = material.Code,
            ProcessCode = process.Code,
            Name = new LocalizedText { En = material.Name }
        }).ToList();

        var surfaceFinishes = finishesTask.Result.Select(finish => new SurfaceFinishOptionDto
        {
            Id = finish.Id,
            Code = finish.Code,
            ProcessCode = process.Code,
            Name = new LocalizedText { En = finish.Name },
            Summary = new LocalizedText { En = finish.Description ?? string.Empty },
            RaValueUm = finish.RaValueUm,
            AdditionalCostPercent = finish.AdditionalCostPercent,
            SortOrder = finish.SortOrder
        }).ToList();

        var processOptions = configOptionsTask.Result.Select(option => new ProcessConfigOptionDto
        {
            Id = option.Id,
            ProcessCode = process.Code,
            ConfigKey = option.ConfigKey,
            Label = new LocalizedText { En = option.Label },
            ConfigType = option.ConfigType,
            DefaultValue = option.DefaultValue,
            OptionsJson = option.OptionsJson,
            Unit = option.Unit,
            HelpText = new LocalizedText { En = option.HelpText ?? string.Empty },
            IsRequired = option.IsRequired,
            SortOrder = option.SortOrder
        }).ToList();

        return new ProcessCatalogData(materials, surfaceFinishes, processOptions);
    }

    private async Task<Dictionary<Guid, List<Guid>>> LoadFinishCompatibilityAsync(
        IReadOnlyCollection<MaterialOptionDto> materials,
        CancellationToken cancellationToken)
    {
        var materialFinishTasks = materials
            .Where(material => material.Id.HasValue)
            .Select(async material =>
            {
                var finishes = await materialClient.GetMaterialFinishesAsync(material.Id!.Value, cancellationToken);
                return new
                {
                    MaterialId = material.Id.Value,
                    FinishIds = finishes.Select(finish => finish.Id).ToArray()
                };
            });

        var materialFinishes = await Task.WhenAll(materialFinishTasks);
        return materialFinishes
            .SelectMany(entry => entry.FinishIds.Select(finishId => new { finishId, entry.MaterialId }))
            .GroupBy(entry => entry.finishId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(entry => entry.MaterialId).Distinct().ToList());
    }

    private static bool IsInstantQuoteProcess(string code)
    {
        return code.Equals("FDM", StringComparison.OrdinalIgnoreCase) ||
            code.Equals("SLA", StringComparison.OrdinalIgnoreCase) ||
            code.Equals("SLA_DLP", StringComparison.OrdinalIgnoreCase) ||
            code.Equals("SLS", StringComparison.OrdinalIgnoreCase) ||
            code.Equals("CNC", StringComparison.OrdinalIgnoreCase) ||
            code.Equals("CNC_MILL", StringComparison.OrdinalIgnoreCase) ||
            code.Equals("CNC_MILLING", StringComparison.OrdinalIgnoreCase) ||
            code.Equals("CNC_TURN", StringComparison.OrdinalIgnoreCase) ||
            code.Equals("CNC_TURNING", StringComparison.OrdinalIgnoreCase);
    }

    private sealed record ProcessCatalogData(
        List<MaterialOptionDto> Materials,
        List<SurfaceFinishOptionDto> SurfaceFinishes,
        List<ProcessConfigOptionDto> ProcessOptions);
}
