using System.Net;
using System.Net.Http.Json;
using Maliev.Web.Shared.Commerce;
using Maliev.Web.Shared.Quotes;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;

namespace Maliev.Web.Tests;

/// <summary>
/// Contract tests for customer-facing Web BFF endpoints.
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
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));
    }

    /// <summary>
    /// Verifies the Shopify import preview keeps the public product count contract.
    /// </summary>
    [Fact]
    public async Task GET_ShopifyImportPreview_ReturnsExpectedProductCount()
    {
        using var client = _factory.CreateClient();

        var preview = await client.GetFromJsonAsync<ShopifyImportPreviewDto>("/web/v1/shopify/import-preview");

        Assert.NotNull(preview);
        Assert.Equal("https://shop.maliev.com/collections/all", preview.SourceStorefrontUrl);
        Assert.Equal(57, preview.ExpectedProductCount);
        Assert.True(preview.SeededProducts.Count > 0);
    }

    /// <summary>
    /// Verifies quote estimates preserve explicit bulk-discount semantics.
    /// </summary>
    [Fact]
    public async Task POST_QuoteEstimate_BulkQuantity_ReturnsExplicitBulkDiscount()
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
                    DfmAcknowledged = true
                }
            ]
        };

        var response = await client.PostAsJsonAsync("/web/v1/quote/estimate", request);
        var estimate = await response.Content.ReadFromJsonAsync<QuoteEstimateResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(estimate);
        Assert.True(estimate.BulkDiscountTotal > 0);
        Assert.Equal(estimate.Lines.Sum(line => line.Total) + estimate.TaxAmount, estimate.Total);
    }
}
