namespace Maliev.Web.Bff.Clients;

/// <summary>
/// Downstream CommerceService client used by the Web BFF storefront catalog.
/// </summary>
public interface ICommerceServiceClient
{
    /// <summary>Lists published CommerceService collections.</summary>
    Task<HttpResponseMessage> ListCollectionsAsync(CancellationToken cancellationToken);

    /// <summary>Lists published CommerceService storefront products.</summary>
    Task<HttpResponseMessage> ListProductsAsync(string? collection, CancellationToken cancellationToken);

    /// <summary>Gets a published CommerceService storefront product by handle.</summary>
    Task<HttpResponseMessage> GetProductAsync(string handle, CancellationToken cancellationToken);
}

internal sealed class CommerceServiceClient(HttpClient httpClient) : ICommerceServiceClient
{
    private const int StorefrontPageSize = 100;

    public Task<HttpResponseMessage> ListCollectionsAsync(CancellationToken cancellationToken)
    {
        return httpClient.GetAsync("/commerce/v1/collections", cancellationToken);
    }

    public Task<HttpResponseMessage> ListProductsAsync(string? collection, CancellationToken cancellationToken)
    {
        var path = $"/commerce/v1/products?page=1&pageSize={StorefrontPageSize}";
        if (!string.IsNullOrWhiteSpace(collection))
        {
            path += $"&collection={Uri.EscapeDataString(collection.Trim())}";
        }

        return httpClient.GetAsync(path, cancellationToken);
    }

    public Task<HttpResponseMessage> GetProductAsync(string handle, CancellationToken cancellationToken)
    {
        return httpClient.GetAsync($"/commerce/v1/products/{Uri.EscapeDataString(handle)}", cancellationToken);
    }
}
