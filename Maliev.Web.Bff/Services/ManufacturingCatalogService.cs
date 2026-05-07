using Maliev.Web.Bff.Clients;
using Maliev.Web.Shared.Localization;
using Maliev.Web.Shared.Quotes;

namespace Maliev.Web.Bff.Services;

internal sealed class ManufacturingCatalogService(IMaterialServiceClient materialClient, IPricingServiceClient pricingClient) : IManufacturingCatalogService
{
    public async Task<QuoteReferenceDataDto> GetReferenceDataAsync(CancellationToken cancellationToken)
    {
        var processes = await materialClient.GetProcessesAsync(cancellationToken);
        var materials = new List<MaterialOptionDto>();
        foreach (var process in processes)
        {
            var processMaterials = await materialClient.GetMaterialsAsync(process.Code, cancellationToken);
            materials.AddRange(processMaterials.Select(material => new MaterialOptionDto
            {
                Id = material.Id,
                Code = material.Code,
                ProcessCode = process.Code,
                Name = new LocalizedText { En = material.Name }
            }));
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
            LeadTimeCodes = leadTimes.Select(leadTime => leadTime.Code).ToList()
        };
    }

    private static bool IsInstantQuoteProcess(string code)
    {
        return code.Equals("FDM", StringComparison.OrdinalIgnoreCase) ||
            code.Equals("SLA", StringComparison.OrdinalIgnoreCase) ||
            code.Equals("SLS", StringComparison.OrdinalIgnoreCase) ||
            code.Equals("CNC", StringComparison.OrdinalIgnoreCase) ||
            code.Equals("CNC_MILLING", StringComparison.OrdinalIgnoreCase) ||
            code.Equals("CNC_TURNING", StringComparison.OrdinalIgnoreCase);
    }
}
