using System.Net.Http.Json;

namespace Maliev.Web.Bff.Clients;

/// <summary>
/// Downstream CommerceService client used by the Web BFF storefront catalog.
/// </summary>
public interface ICommerceServiceClient
{
    /// <summary>Lists published CommerceService collections.</summary>
    Task<HttpResponseMessage> ListCollectionsAsync(CancellationToken cancellationToken);

    /// <summary>Gets a published CommerceService collection by handle.</summary>
    Task<HttpResponseMessage> GetCollectionAsync(string handle, CancellationToken cancellationToken);

    /// <summary>Lists published CommerceService storefront products.</summary>
    Task<HttpResponseMessage> ListProductsAsync(string? collection, CancellationToken cancellationToken);

    /// <summary>Gets a published CommerceService storefront product by handle.</summary>
    Task<HttpResponseMessage> GetProductAsync(string handle, CancellationToken cancellationToken);

    /// <summary>Creates a CommerceService cart.</summary>
    Task<HttpResponseMessage> CreateCartAsync(object request, CancellationToken cancellationToken);

    /// <summary>Adds or updates a CommerceService cart line.</summary>
    Task<HttpResponseMessage> UpsertCartLineAsync(Guid cartId, object request, CancellationToken cancellationToken);

    /// <summary>Creates a CommerceService checkout session.</summary>
    Task<HttpResponseMessage> CreateCheckoutSessionAsync(object request, CancellationToken cancellationToken);
}

internal sealed class CommerceServiceClient(HttpClient httpClient) : ICommerceServiceClient
{
    private const int StorefrontPageSize = 100;

    public Task<HttpResponseMessage> ListCollectionsAsync(CancellationToken cancellationToken)
    {
        return httpClient.GetAsync("/commerce/v1/collections", cancellationToken);
    }

    public Task<HttpResponseMessage> GetCollectionAsync(string handle, CancellationToken cancellationToken)
    {
        return httpClient.GetAsync($"/commerce/v1/collections/{Uri.EscapeDataString(handle)}", cancellationToken);
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

    public Task<HttpResponseMessage> CreateCartAsync(object request, CancellationToken cancellationToken)
    {
        return httpClient.PostAsJsonAsync("/commerce/v1/carts", request, cancellationToken);
    }

    public Task<HttpResponseMessage> UpsertCartLineAsync(Guid cartId, object request, CancellationToken cancellationToken)
    {
        return httpClient.PostAsJsonAsync($"/commerce/v1/carts/{cartId}/lines", request, cancellationToken);
    }

    public Task<HttpResponseMessage> CreateCheckoutSessionAsync(object request, CancellationToken cancellationToken)
    {
        return httpClient.PostAsJsonAsync("/commerce/v1/checkout-sessions", request, cancellationToken);
    }
}
