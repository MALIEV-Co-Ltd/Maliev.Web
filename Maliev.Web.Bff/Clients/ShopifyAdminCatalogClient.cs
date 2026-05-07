using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net.Http.Json;
using Maliev.Web.Bff.Services;
using Maliev.Web.Shared.Commerce;
using Maliev.Web.Shared.Localization;

namespace Maliev.Web.Bff.Clients;

internal interface IShopifyAdminCatalogClient
{
    Task<ShopifyCatalogSnapshot> GetCatalogAsync(CancellationToken cancellationToken);
}

internal sealed class ShopifyAdminCatalogClient(HttpClient httpClient, IConfiguration configuration, ILogger<ShopifyAdminCatalogClient> logger)
    : IShopifyAdminCatalogClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly Regex HtmlRegex = new("<.*?>", RegexOptions.Compiled | RegexOptions.Singleline);

    public async Task<ShopifyCatalogSnapshot> GetCatalogAsync(CancellationToken cancellationToken)
    {
        var shopDomain = configuration["Shopify:ShopDomain"];
        var accessToken = configuration["Shopify:AdminAccessToken"];
        var apiVersion = configuration["Shopify:ApiVersion"];

        if (string.IsNullOrWhiteSpace(shopDomain) || string.IsNullOrWhiteSpace(accessToken))
        {
            throw new BackendUnavailableException(
                "Shopify Admin API",
                "Shopify Admin API is not configured. Set Shopify:ShopDomain and Shopify:AdminAccessToken for catalog migration data.");
        }

        apiVersion = string.IsNullOrWhiteSpace(apiVersion) ? "2026-04" : apiVersion;
        var endpoint = BuildAdminEndpoint(shopDomain, apiVersion);

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.TryAddWithoutValidation("X-Shopify-Access-Token", accessToken);
        request.Content = JsonContent.Create(new ShopifyGraphQlRequest(ProductsQuery), options: JsonOptions);

        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning(
                "Shopify Admin API returned {StatusCode}: {Body}",
                (int)response.StatusCode,
                body);

            throw new BackendUnavailableException(
                "Shopify Admin API",
                $"Shopify Admin API returned {(int)response.StatusCode} while loading product catalog.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;
        if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Array && errors.GetArrayLength() > 0)
        {
            throw new BackendUnavailableException(
                "Shopify Admin API",
                $"Shopify Admin API returned GraphQL errors: {errors}");
        }

        if (!root.TryGetProperty("data", out var data) || !data.TryGetProperty("products", out var productsConnection))
        {
            throw new BackendUnavailableException("Shopify Admin API", "Shopify Admin API response did not contain products data.");
        }

        var products = ParseProducts(productsConnection, shopDomain).ToList();
        var collections = ParseCollections(data, products).ToList();
        return new ShopifyCatalogSnapshot(products, collections);
    }

    private static Uri BuildAdminEndpoint(string shopDomain, string apiVersion)
    {
        var normalized = shopDomain.Trim().TrimEnd('/');
        if (!normalized.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !normalized.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            normalized = $"https://{normalized}";
        }

        return new Uri($"{normalized}/admin/api/{apiVersion}/graphql.json");
    }

    private static IEnumerable<ProductDetailDto> ParseProducts(JsonElement productsConnection, string shopDomain)
    {
        if (!productsConnection.TryGetProperty("edges", out var edges) || edges.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        foreach (var edge in edges.EnumerateArray())
        {
            if (!edge.TryGetProperty("node", out var node))
            {
                continue;
            }

            var handle = GetString(node, "handle");
            if (string.IsNullOrWhiteSpace(handle))
            {
                continue;
            }

            var title = GetString(node, "title");
            var description = StripHtml(GetString(node, "descriptionHtml"));
            var collectionSlug = GetFirstCollectionHandle(node);
            var media = ParseMedia(node).ToList();
            var variants = ParseVariants(node).ToList();
            var prices = variants.Select(v => v.PriceThb).Where(price => price > 0m).ToList();
            var minPrice = prices.Count == 0 ? 0m : prices.Min();
            var maxPrice = prices.Count == 0 ? 0m : prices.Max();
            var productType = GetString(node, "productType");
            var keywords = ParseTags(node).ToList();
            if (!string.IsNullOrWhiteSpace(productType))
            {
                keywords.Insert(0, productType);
            }

            yield return new ProductDetailDto
            {
                Handle = handle,
                Title = new LocalizedText { En = title },
                Summary = new LocalizedText { En = description.Length > 220 ? $"{description[..220].Trim()}..." : description },
                Body = new LocalizedText { En = description },
                CollectionSlug = string.IsNullOrWhiteSpace(collectionSlug) ? Slugify(productType, "products") : collectionSlug,
                PriceThb = minPrice,
                CompareAtPriceThb = maxPrice == minPrice ? null : maxPrice,
                PriceStartsAt = prices.Distinct().Count() > 1,
                LeadTimeDays = 0,
                ImageUrl = media.FirstOrDefault()?.Url ?? string.Empty,
                SourceUrl = BuildProductUrl(shopDomain, handle),
                Media = media,
                Variants = variants,
                SeoKeywords = keywords
            };
        }
    }

    private static IEnumerable<ProductCollectionDto> ParseCollections(JsonElement data, IReadOnlyList<ProductDetailDto> products)
    {
        var collections = new Dictionary<string, ProductCollectionDto>(StringComparer.OrdinalIgnoreCase);

        if (data.TryGetProperty("collections", out var collectionsConnection) &&
            collectionsConnection.TryGetProperty("edges", out var edges) &&
            edges.ValueKind == JsonValueKind.Array)
        {
            foreach (var edge in edges.EnumerateArray())
            {
                if (!edge.TryGetProperty("node", out var node))
                {
                    continue;
                }

                var slug = GetString(node, "handle");
                if (string.IsNullOrWhiteSpace(slug))
                {
                    continue;
                }

                collections[slug] = new ProductCollectionDto
                {
                    Slug = slug,
                    Name = new LocalizedText { En = GetString(node, "title") },
                    Summary = new LocalizedText { En = StripHtml(GetString(node, "descriptionHtml")) },
                    ExpectedProductCount = products.Count(product => product.CollectionSlug.Equals(slug, StringComparison.OrdinalIgnoreCase))
                };
            }
        }

        foreach (var group in products.GroupBy(product => product.CollectionSlug, StringComparer.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(group.Key))
            {
                continue;
            }

            if (!collections.ContainsKey(group.Key))
            {
                collections[group.Key] = new ProductCollectionDto
                {
                    Slug = group.Key,
                    Name = new LocalizedText { En = HumanizeSlug(group.Key) },
                    Summary = new LocalizedText { En = string.Empty },
                    ExpectedProductCount = group.Count()
                };
            }
            else
            {
                collections[group.Key].ExpectedProductCount = group.Count();
            }
        }

        return collections.Values.OrderBy(collection => collection.Name.En, StringComparer.OrdinalIgnoreCase);
    }

    private static IEnumerable<ProductMediaDto> ParseMedia(JsonElement productNode)
    {
        if (!productNode.TryGetProperty("media", out var mediaConnection) ||
            !mediaConnection.TryGetProperty("edges", out var edges) ||
            edges.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        foreach (var edge in edges.EnumerateArray())
        {
            if (!edge.TryGetProperty("node", out var node) ||
                !node.TryGetProperty("image", out var image) ||
                image.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                continue;
            }

            var url = GetString(image, "url");
            if (string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            yield return new ProductMediaDto
            {
                Url = url,
                Alt = GetString(image, "altText")
            };
        }
    }

    private static IEnumerable<ProductVariantDto> ParseVariants(JsonElement productNode)
    {
        if (!productNode.TryGetProperty("variants", out var variantsConnection) ||
            !variantsConnection.TryGetProperty("edges", out var edges) ||
            edges.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        foreach (var edge in edges.EnumerateArray())
        {
            if (!edge.TryGetProperty("node", out var node))
            {
                continue;
            }

            yield return new ProductVariantDto
            {
                Sku = GetString(node, "sku"),
                Title = new LocalizedText { En = GetString(node, "title") },
                PriceThb = GetDecimal(node, "price"),
                Available = GetBool(node, "availableForSale", true)
            };
        }
    }

    private static IEnumerable<string> ParseTags(JsonElement productNode)
    {
        if (!productNode.TryGetProperty("tags", out var tags) || tags.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        foreach (var tag in tags.EnumerateArray())
        {
            var value = tag.GetString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                yield return value;
            }
        }
    }

    private static string GetFirstCollectionHandle(JsonElement productNode)
    {
        if (!productNode.TryGetProperty("collections", out var collectionsConnection) ||
            !collectionsConnection.TryGetProperty("edges", out var edges) ||
            edges.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        foreach (var edge in edges.EnumerateArray())
        {
            if (edge.TryGetProperty("node", out var node))
            {
                var handle = GetString(node, "handle");
                if (!string.IsNullOrWhiteSpace(handle))
                {
                    return handle;
                }
            }
        }

        return string.Empty;
    }

    private static string BuildProductUrl(string shopDomain, string handle)
    {
        var host = shopDomain.Trim().TrimEnd('/');
        if (!host.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !host.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            host = $"https://{host}";
        }

        return $"{host}/products/{handle}";
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return string.Empty;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.ToString();
    }

    private static decimal GetDecimal(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return 0m;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetDecimal(out var number) => number,
            JsonValueKind.String when decimal.TryParse(value.GetString(), out var parsed) => parsed,
            _ => 0m
        };
    }

    private static bool GetBool(JsonElement element, string propertyName, bool fallback)
    {
        if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return fallback;
        }

        return value.ValueKind == JsonValueKind.True || (value.ValueKind != JsonValueKind.False && fallback);
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var text = HtmlRegex.Replace(html, " ");
        return System.Net.WebUtility.HtmlDecode(text).Replace("\n", " ", StringComparison.Ordinal).Trim();
    }

    private static string Slugify(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var builder = new StringBuilder(value.Length);
        foreach (var character in value.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
            }
            else if (builder.Length > 0 && builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        return builder.ToString().Trim('-');
    }

    private static string HumanizeSlug(string slug)
    {
        return string.Join(' ', slug.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
    }

    private const string ProductsQuery = """
        query CustomerWebCatalog {
          collections(first: 50) {
            edges {
              node {
                handle
                title
                descriptionHtml
              }
            }
          }
          products(first: 100) {
            edges {
              node {
                handle
                title
                descriptionHtml
                productType
                tags
                media(first: 10) {
                  edges {
                    node {
                      ... on MediaImage {
                        image {
                          url
                          altText
                        }
                      }
                    }
                  }
                }
                variants(first: 25) {
                  edges {
                    node {
                      sku
                      title
                      price
                      availableForSale
                    }
                  }
                }
                collections(first: 5) {
                  edges {
                    node {
                      handle
                    }
                  }
                }
              }
            }
          }
        }
        """;
}

internal sealed record ShopifyGraphQlRequest(string Query);

internal sealed record ShopifyCatalogSnapshot(
    IReadOnlyList<ProductDetailDto> Products,
    IReadOnlyList<ProductCollectionDto> Collections);
