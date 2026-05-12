using System.Net;
using System.Net.Http.Json;
using Maliev.Web.Bff.Clients;
using Maliev.Web.Shared.Commerce;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Maliev.Web.Tests;

/// <summary>
/// Contract tests for the Web BFF storefront catalog boundary.
/// </summary>
public sealed class CommerceCatalogBoundaryTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly FakeCommerceServiceClient _commerceServiceClient = new();

    /// <summary>
    /// Initializes a new instance of the catalog boundary tests.
    /// </summary>
    public CommerceCatalogBoundaryTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder
            .UseEnvironment("Testing")
            .ConfigureServices(services =>
            {
                services.RemoveAll<ICommerceServiceClient>();
                services.AddSingleton<ICommerceServiceClient>(_commerceServiceClient);
            }));
    }

    /// <summary>
    /// Verifies public shop products are mapped from CommerceService storefront listings.
    /// </summary>
    [Fact]
    public async Task GET_CatalogProducts_MapsPublishedCommerceServiceListings()
    {
        using var client = _factory.CreateClient();

        var products = await client.GetFromJsonAsync<List<ProductSummaryDto>>(
            "/web/v1/catalog/products?collection=injection-molding-machines");

        Assert.NotNull(products);
        var product = Assert.Single(products);
        Assert.Equal("injection-molding-machines", _commerceServiceClient.LastRequestedCollection);
        Assert.Equal("pneumatic-injection-molding-machine-30g", product.Handle);
        Assert.Equal("Pneumatic Injection Molding Machine 30g", product.Title.En);
        Assert.Equal(product.Title.En, product.Title.Th);
        Assert.Equal("injection-molding-machines", product.CollectionSlug);
        Assert.Equal(99000m, product.PriceThb);
        Assert.Equal("https://cdn.maliev.test/pimm-30.jpg", product.ImageUrl);
        Assert.True(product.IsPublished);
    }

    /// <summary>
    /// Verifies product details include CommerceService media, variants, and collection handles.
    /// </summary>
    [Fact]
    public async Task GET_CatalogProduct_MapsCommerceServiceProductDetail()
    {
        using var client = _factory.CreateClient();

        var product = await client.GetFromJsonAsync<ProductDetailDto>(
            "/web/v1/catalog/products/pneumatic-injection-molding-machine-30g");

        Assert.NotNull(product);
        Assert.Equal("injection-molding-machines", product.CollectionSlug);
        Assert.Equal("Detailed machine description.", product.Body.En);
        Assert.Equal("PIMM-30-STD", Assert.Single(product.Variants).Sku);
        Assert.Equal(8, product.AvailableQuantity);
        Assert.Equal("Available", product.InventoryStatus);
        Assert.Equal("https://cdn.maliev.test/pimm-30.jpg", Assert.Single(product.Media).Url);
    }

    private sealed class FakeCommerceServiceClient : ICommerceServiceClient
    {
        public string? LastRequestedCollection { get; private set; }

        public Task<HttpResponseMessage> ListCollectionsAsync(CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new[]
                {
                    new
                    {
                        id = Guid.Parse("9a9279a5-1038-4644-944e-8e950295aef1"),
                        handle = "injection-molding-machines",
                        title = "Injection molding machines",
                        description = "Pneumatic machine listings.",
                        isPublished = true
                    }
                })
            };
            return Task.FromResult(response);
        }

        public Task<HttpResponseMessage> ListProductsAsync(string? collection, CancellationToken cancellationToken)
        {
            LastRequestedCollection = collection;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    items = new[]
                    {
                        new
                        {
                            id = Guid.Parse("8acf067b-82c6-4916-8c7e-e43c34c1f4b8"),
                            handle = "pneumatic-injection-molding-machine-30g",
                            title = "Pneumatic Injection Molding Machine 30g",
                            brand = "MALIEV",
                            summary = "Compact pneumatic injection molding machine.",
                            productType = "Injection molding machine",
                            status = "Published",
                            startingPrice = 99000m,
                            currency = "THB",
                            thumbnailUrl = "https://cdn.maliev.test/pimm-30.jpg"
                        }
                    },
                    page = 1,
                    pageSize = 24,
                    totalCount = 1
                })
            };
            return Task.FromResult(response);
        }

        public Task<HttpResponseMessage> GetProductAsync(string handle, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    id = Guid.Parse("8acf067b-82c6-4916-8c7e-e43c34c1f4b8"),
                    handle,
                    title = "Pneumatic Injection Molding Machine 30g",
                    brand = "MALIEV",
                    summary = "Compact pneumatic injection molding machine.",
                    description = "Detailed machine description.",
                    productType = "Injection molding machine",
                    status = "Published",
                    variants = new[]
                    {
                        new
                        {
                            id = Guid.Parse("38981515-d989-4f4d-99d9-889d242ff20c"),
                            sku = "PIMM-30-STD",
                            title = "30g starter package",
                            priceAmount = 99000m,
                            currency = "THB",
                            inventoryQuantity = 8,
                            isActive = true,
                            optionValuesJson = (string?)null
                        }
                    },
                    media = new[]
                    {
                        new
                        {
                            id = Guid.Parse("51ef44be-813c-4d7f-b083-ebde0519da98"),
                            url = "https://cdn.maliev.test/pimm-30.jpg",
                            altText = "Pneumatic injection molding machine",
                            sortOrder = 0
                        }
                    },
                    collections = new[]
                    {
                        new
                        {
                            id = Guid.Parse("9a9279a5-1038-4644-944e-8e950295aef1"),
                            handle = "injection-molding-machines",
                            title = "Injection molding machines"
                        }
                    }
                })
            };
            return Task.FromResult(response);
        }
    }
}
