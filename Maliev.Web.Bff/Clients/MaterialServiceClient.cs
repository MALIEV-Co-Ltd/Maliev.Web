using System.Text.Json;
using Maliev.Web.Bff.Services;

namespace Maliev.Web.Bff.Clients;

internal interface IMaterialServiceClient
{
    Task<IReadOnlyList<MaterialProcessCatalogResponse>> GetProcessesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<MaterialCatalogResponse>> GetMaterialsAsync(string processCode, CancellationToken cancellationToken);

    Task<IReadOnlyList<SurfaceFinishCatalogResponse>> GetFinishesAsync(string processCode, CancellationToken cancellationToken);

    Task<IReadOnlyList<SurfaceFinishCatalogResponse>> GetMaterialFinishesAsync(Guid materialId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProcessConfigOptionCatalogResponse>> GetConfigOptionsAsync(string processCode, CancellationToken cancellationToken);
}

internal sealed class MaterialServiceClient(HttpClient httpClient, ILogger<MaterialServiceClient> logger) : IMaterialServiceClient
{
    public async Task<IReadOnlyList<MaterialProcessCatalogResponse>> GetProcessesAsync(CancellationToken cancellationToken)
    {
        return await GetAsync<List<MaterialProcessCatalogResponse>>("/material/v1/manufacturing/processes", "manufacturing processes", cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<MaterialCatalogResponse>> GetMaterialsAsync(string processCode, CancellationToken cancellationToken)
    {
        return await GetAsync<List<MaterialCatalogResponse>>($"/material/v1/manufacturing/processes/{Uri.EscapeDataString(processCode)}/materials", $"materials for {processCode}", cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<SurfaceFinishCatalogResponse>> GetFinishesAsync(string processCode, CancellationToken cancellationToken)
    {
        return await GetAsync<List<SurfaceFinishCatalogResponse>>($"/material/v1/manufacturing/processes/{Uri.EscapeDataString(processCode)}/finishes", $"surface finishes for {processCode}", cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<SurfaceFinishCatalogResponse>> GetMaterialFinishesAsync(Guid materialId, CancellationToken cancellationToken)
    {
        return await GetAsync<List<SurfaceFinishCatalogResponse>>($"/material/v1/manufacturing/materials/{materialId}/finishes", $"surface finishes for material {materialId}", cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<ProcessConfigOptionCatalogResponse>> GetConfigOptionsAsync(string processCode, CancellationToken cancellationToken)
    {
        return await GetAsync<List<ProcessConfigOptionCatalogResponse>>($"/material/v1/manufacturing/processes/{Uri.EscapeDataString(processCode)}/config-options", $"configuration options for {processCode}", cancellationToken) ?? [];
    }

    private async Task<T?> GetAsync<T>(string path, string resourceName, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync(path, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new BackendUnavailableException("MaterialService", $"MaterialService returned {(int)response.StatusCode} while loading {resourceName}.");
            }

            return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
        }
        catch (BackendUnavailableException)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or InvalidOperationException)
        {
            logger.LogWarning(ex, "MaterialService failed while loading {ResourceName}", resourceName);
            throw new BackendUnavailableException("MaterialService", $"MaterialService is unavailable while loading {resourceName}.", ex);
        }
    }
}

internal sealed class MaterialProcessCatalogResponse
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

internal sealed class MaterialCatalogResponse
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

internal sealed class SurfaceFinishCatalogResponse
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public decimal? RaValueUm { get; set; }

    public decimal AdditionalCostPercent { get; set; }

    public string? Description { get; set; }

    public int SortOrder { get; set; }
}

internal sealed class ProcessConfigOptionCatalogResponse
{
    public Guid Id { get; set; }

    public string ConfigKey { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string ConfigType { get; set; } = string.Empty;

    public string? DefaultValue { get; set; }

    public string? OptionsJson { get; set; }

    public string? Unit { get; set; }

    public string? HelpText { get; set; }

    public bool IsRequired { get; set; }

    public int SortOrder { get; set; }
}
