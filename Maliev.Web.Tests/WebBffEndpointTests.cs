using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Maliev.Web.Bff.Services;
using Maliev.Web.Shared.Commerce;
using Maliev.Web.Shared.Contact;
using Maliev.Web.Shared.Localization;
using Maliev.Web.Shared.Quotes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Maliev.Web.Tests;

/// <summary>
/// Contract tests for customer-facing Web BFF controllers.
/// </summary>
public sealed class WebBffEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>
    /// Initializes a new instance of the endpoint tests.
    /// </summary>
    /// <param name="factory">The application factory.</param>
    public WebBffEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder
            .UseEnvironment("Testing")
            .ConfigureServices(services =>
            {
                services.RemoveAll<ICommerceCatalogService>();
                services.RemoveAll<IManufacturingCatalogService>();
                services.RemoveAll<IWebQuoteService>();
                services.RemoveAll<ICheckoutDraftService>();
                services.RemoveAll<IContactMessageService>();
                services.AddSingleton<ICommerceCatalogService, FakeCommerceCatalogService>();
                services.AddSingleton<IManufacturingCatalogService, FakeManufacturingCatalogService>();
                services.AddSingleton<IWebQuoteService, FakeWebQuoteService>();
                services.AddSingleton<ICheckoutDraftService, FakeCheckoutDraftService>();
                services.AddSingleton<IContactMessageService, FakeContactMessageService>();
            }));
    }

    /// <summary>
    /// Verifies the catalog controller returns products from the configured catalog source.
    /// </summary>
    [Fact]
    public async Task GET_CatalogProducts_ReturnsCatalogProducts()
    {
        using var client = _factory.CreateClient();

        var products = await client.GetFromJsonAsync<List<ProductSummaryDto>>("/web/v1/catalog/products");

        Assert.NotNull(products);
        var product = Assert.Single(products);
        Assert.Equal("catalog-product", product.Handle);
        Assert.Equal(12000m, product.PriceThb);
    }

    /// <summary>
    /// Verifies reference data is routed through the manufacturing catalog service.
    /// </summary>
    [Fact]
    public async Task GET_QuoteReferenceData_ReturnsBackendReferenceData()
    {
        using var client = _factory.CreateClient();

        var reference = await client.GetFromJsonAsync<QuoteReferenceDataDto>("/web/v1/quote/reference-data");

        Assert.NotNull(reference);
        Assert.Equal("FDM", Assert.Single(reference.Processes).Code);
        Assert.Equal("PLA", Assert.Single(reference.Materials).Code);
        Assert.Equal("STANDARD", Assert.Single(reference.LeadTimeCodes));
    }

    /// <summary>
    /// Verifies quote estimates preserve explicit bulk-discount fields returned by pricing.
    /// </summary>
    [Fact]
    public async Task POST_QuoteEstimate_RoutesThroughQuoteService()
    {
        using var client = _factory.CreateClient();
        var request = new QuoteEstimateRequest
        {
            Parts =
            [
                new QuotePartDraftDto
                {
                    File = new QuoteFileDraftDto { Name = "bracket.stl", SizeBytes = 2_000_000 },
                    ProcessCode = "FDM",
                    MaterialCode = "PLA",
                    Quantity = 10,
                    EstimatedVolumeCc = 20,
                    DfmAcknowledged = true,
                    FileId = Guid.NewGuid(),
                    MaterialId = Guid.NewGuid(),
                    ManufacturingProcessId = Guid.NewGuid()
                }
            ]
        };

        var response = await client.PostAsJsonAsync("/web/v1/quote/estimate", request);
        var estimate = await response.Content.ReadFromJsonAsync<QuoteEstimateResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(estimate);
        Assert.Equal("test-pricing", estimate.PricingSource);
        Assert.Equal(150m, estimate.BulkDiscountTotal);
    }

    /// <summary>
    /// Verifies customer website contact messages are routed through the contact boundary.
    /// </summary>
    [Fact]
    public async Task POST_ContactMessage_RoutesThroughContactBoundary()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/web/v1/contact/messages", new ContactMessageRequest
        {
            FullName = "Website Customer",
            Email = "customer@example.com",
            Subject = "Manufacturing question",
            Message = "Can MALIEV review this project?"
        });
        var contact = await response.Content.ReadFromJsonAsync<ContactMessageResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(contact);
        Assert.Equal("Received", contact.Status);
    }

    /// <summary>
    /// Verifies public SEO endpoints expose crawler metadata for customer pages.
    /// </summary>
    [Fact]
    public async Task GET_SeoEndpoints_ReturnCrawlerMetadata()
    {
        using var client = _factory.CreateClient();

        var robots = await client.GetStringAsync("/robots.txt");
        var sitemap = await client.GetStringAsync("/sitemap.xml");

        Assert.Contains("Sitemap: https://www.maliev.com/sitemap.xml", robots);
        Assert.Contains("https://www.maliev.com/materials", sitemap);
        Assert.Contains("https://www.maliev.com/blog", sitemap);
    }

    private sealed class FakeCommerceCatalogService : ICommerceCatalogService
    {
        private readonly List<ProductDetailDto> _products =
        [
            new()
            {
                Handle = "catalog-product",
                Title = new LocalizedText { En = "Catalog Product" },
                CollectionSlug = "machines",
                PriceThb = 12000m,
                IsPublished = true
            }
        ];

        public Task<IReadOnlyList<ProductCollectionDto>> GetCollectionsAsync(CancellationToken cancellationToken)
        {
            IReadOnlyList<ProductCollectionDto> collections =
            [
                new()
                {
                    Slug = "machines",
                    Name = new LocalizedText { En = "Machines" },
                    ExpectedProductCount = 1
                }
            ];
            return Task.FromResult(collections);
        }

        public Task<IReadOnlyList<ProductSummaryDto>> GetProductsAsync(string? collectionSlug, CancellationToken cancellationToken)
        {
            IReadOnlyList<ProductSummaryDto> products = _products
                .Where(product => product.IsPublished)
                .Cast<ProductSummaryDto>()
                .ToList();
            return Task.FromResult(products);
        }

        public Task<ProductDetailDto?> GetProductAsync(string handle, CancellationToken cancellationToken)
        {
            var product = _products.FirstOrDefault(product => product.Handle == handle && product.IsPublished);
            return Task.FromResult(product);
        }

    }

    private sealed class FakeManufacturingCatalogService : IManufacturingCatalogService
    {
        public Task<QuoteReferenceDataDto> GetReferenceDataAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new QuoteReferenceDataDto
            {
                Processes =
                [
                    new ServiceProcessDto
                    {
                        Id = Guid.NewGuid(),
                        Code = "FDM",
                        Name = new LocalizedText { En = "FDM" },
                        SupportsInstantQuote = true
                    }
                ],
                Materials =
                [
                    new MaterialOptionDto
                    {
                        Id = Guid.NewGuid(),
                        Code = "PLA",
                        ProcessCode = "FDM",
                        Name = new LocalizedText { En = "PLA" }
                    }
                ],
                LeadTimeCodes = ["STANDARD"]
            });
        }
    }

    private sealed class FakeWebQuoteService : IWebQuoteService
    {
        public Task<QuoteEstimateResponse> EstimateAsync(QuoteEstimateRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new QuoteEstimateResponse
            {
                Lines =
                [
                    new QuoteLineEstimateDto
                    {
                        PartId = request.Parts[0].Id,
                        Name = request.Parts[0].File.Name,
                        BaseUnitPrice = 100m,
                        UnitPrice = 85m,
                        BulkDiscountAmount = 150m,
                        Total = 850m
                    }
                ],
                Subtotal = 850m,
                BulkDiscountTotal = 150m,
                Total = 850m,
                PricingSource = "test-pricing"
            });
        }
    }

    private sealed class FakeCheckoutDraftService : ICheckoutDraftService
    {
        public Task<CheckoutDraftResponse> CreateDraftAsync(CheckoutDraftRequest request, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            return Task.FromResult(new CheckoutDraftResponse
            {
                CheckoutId = Guid.Parse("d49229e6-d57b-4f94-aa95-391ef3fb44af"),
                RequiresSignIn = false
            });
        }
    }

    private sealed class FakeContactMessageService : IContactMessageService
    {
        public Task<ContactMessageResponse> SubmitAsync(ContactMessageRequest request, CancellationToken cancellationToken)
        {
            Assert.Equal("customer@example.com", request.Email);
            return Task.FromResult(new ContactMessageResponse(Guid.Parse("e9f63ee7-5711-4392-893a-5380b90f80e5"), "Received"));
        }
    }
}
