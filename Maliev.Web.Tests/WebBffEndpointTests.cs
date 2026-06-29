using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Maliev.Web.Bff.Clients;
using Maliev.Web.Bff.Services;
using Maliev.Web.Shared.Account;
using Maliev.Web.Shared.Chatbot;
using Maliev.Web.Shared.Commerce;
using Maliev.Web.Shared.Contact;
using Maliev.Web.Shared.Localization;
using Maliev.Web.Shared.Quotes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
                services.RemoveAll<IQuoteUploadService>();
                services.RemoveAll<ICheckoutDraftService>();
                services.RemoveAll<IContactMessageService>();
                services.RemoveAll<ICustomerChatbotService>();
                services.RemoveAll<IPdfServiceClient>();
                services.AddSingleton<ICommerceCatalogService, FakeCommerceCatalogService>();
                services.AddSingleton<IManufacturingCatalogService, FakeManufacturingCatalogService>();
                services.AddSingleton<IWebQuoteService, FakeWebQuoteService>();
                services.AddSingleton<IQuoteUploadService, FakeQuoteUploadService>();
                services.AddSingleton<ICheckoutDraftService, FakeCheckoutDraftService>();
                services.AddSingleton<IContactMessageService, FakeContactMessageService>();
                services.AddSingleton<ICustomerChatbotService, FakeCustomerChatbotService>();
                services.AddSingleton<IPdfServiceClient, FakePdfServiceClient>();
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
    /// Verifies practical notes are downloaded as generated PDF booklets instead of browser print output.
    /// </summary>
    [Fact]
    public async Task GET_BlogEbookPdf_ReturnsGeneratedPdfDownload()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/web/v1/blog/fdm-print-orientation/ebook.pdf?culture=en");
        var bytes = await response.Content.ReadAsByteArrayAsync();
        var downloadFileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"');

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("attachment", response.Content.Headers.ContentDisposition?.DispositionType);
        Assert.Equal("Practical note - FDM - Print Orientation - MALIEV.pdf", downloadFileName);
        Assert.Equal(FakePdfServiceClient.PdfBytes, bytes);
        Assert.Equal((byte)'%', bytes[0]);
        Assert.Equal((byte)'P', bytes[1]);
        Assert.Equal((byte)'D', bytes[2]);
        Assert.Equal((byte)'F', bytes[3]);
    }

    /// <summary>
    /// Verifies unknown practical note download slugs fail cleanly.
    /// </summary>
    [Fact]
    public async Task GET_BlogEbookPdf_UnknownSlug_ReturnsNotFound()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/web/v1/blog/not-a-real-note/ebook.pdf?culture=en");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// Verifies material datasheets are downloaded as generated PDFs from the catalog.
    /// </summary>
    [Fact]
    public async Task GET_MaterialDatasheetPdf_ReturnsGeneratedPdfDownload()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/web/v1/materials/pa12-nylon/datasheet.pdf?culture=en");
        var bytes = await response.Content.ReadAsByteArrayAsync();
        var downloadFileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"');

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("attachment", response.Content.Headers.ContentDisposition?.DispositionType);
        Assert.Equal("Datasheet - PA12 nylon - MALIEV.pdf", downloadFileName);
        Assert.Equal(FakePdfServiceClient.PdfBytes, bytes);
    }

    /// <summary>
    /// Verifies unknown material datasheet slugs fail cleanly.
    /// </summary>
    [Fact]
    public async Task GET_MaterialDatasheetPdf_UnknownSlug_ReturnsNotFound()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/web/v1/materials/not-a-real-material/datasheet.pdf?culture=en");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
    /// Verifies public quote upload initiation rejects file extensions outside the website upload contract.
    /// </summary>
    [Fact]
    public async Task POST_QuoteUploadInitiation_UnsupportedExtension_ReturnsBadRequest()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/web/v1/quote/uploads/resumable", new WebUploadInitiationRequest
        {
            FileName = "malicious.exe",
            ContentType = "application/octet-stream",
            FileSize = 1024,
            QuoteSessionId = Guid.NewGuid()
        });
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal("Unsupported upload format", problem.Title);
    }

    /// <summary>
    /// Verifies public quote upload initiation accepts the same CAD extensions accepted by QuoteEngine handoff.
    /// </summary>
    [Theory]
    [InlineData("bracket.x_t")]
    [InlineData("housing.sldprt")]
    [InlineData("fixture.catpart")]
    [InlineData("mesh.ply")]
    public async Task POST_QuoteUploadInitiation_QuoteEngineCadExtension_ReturnsUploadSession(string fileName)
    {
        using var client = _factory.CreateClient();
        var quoteSessionId = Guid.NewGuid();

        var response = await client.PostAsJsonAsync("/web/v1/quote/uploads/resumable", new WebUploadInitiationRequest
        {
            FileName = fileName,
            ContentType = "application/octet-stream",
            FileSize = 1024,
            QuoteSessionId = quoteSessionId
        });
        var session = await response.Content.ReadFromJsonAsync<WebUploadInitiationResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(session);
        Assert.Equal("web-upload-1", session.UploadId);
        Assert.Contains(fileName, session.StoragePath, StringComparison.Ordinal);
        Assert.Contains(quoteSessionId.ToString("N"), session.StoragePath, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies public quote upload initiation accepts Make Studio supplemental context files accepted by QuoteEngine.
    /// </summary>
    [Theory]
    [InlineData("requirements.pdf", "application/pdf")]
    [InlineData("sketch.png", "image/png")]
    [InlineData("photo.jpeg", "image/jpeg")]
    [InlineData("drawing.dxf", "application/dxf")]
    public async Task POST_QuoteUploadInitiation_SupplementalAttachment_ReturnsUploadSession(string fileName, string contentType)
    {
        using var client = _factory.CreateClient();
        var quoteSessionId = Guid.NewGuid();

        var response = await client.PostAsJsonAsync("/web/v1/quote/uploads/resumable", new WebUploadInitiationRequest
        {
            FileName = fileName,
            ContentType = contentType,
            FileSize = 1024,
            QuoteSessionId = quoteSessionId
        });
        var session = await response.Content.ReadFromJsonAsync<WebUploadInitiationResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(session);
        Assert.Equal("web-upload-1", session.UploadId);
        Assert.Contains(fileName, session.StoragePath, StringComparison.Ordinal);
        Assert.Contains(quoteSessionId.ToString("N"), session.StoragePath, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies public quote upload initiation rejects files above the customer upload limit before UploadService allocation.
    /// </summary>
    [Fact]
    public async Task POST_QuoteUploadInitiation_FileTooLarge_ReturnsBadRequest()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/web/v1/quote/uploads/resumable", new WebUploadInitiationRequest
        {
            FileName = "large.step",
            ContentType = "application/octet-stream",
            FileSize = 201L * 1024 * 1024,
            QuoteSessionId = Guid.NewGuid()
        });
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal("Upload is too large", problem.Title);
    }

    /// <summary>
    /// Verifies Web signs completed quote uploads before handing storage paths to QuoteEngine.
    /// </summary>
    [Fact]
    public async Task POST_QuoteUploadHandoffToken_ReturnsSignedTokenForAllFiles()
    {
        using var client = _factory.CreateClient();
        var quoteSessionId = Guid.NewGuid();

        var response = await client.PostAsJsonAsync("/web/v1/quote/uploads/handoff-token", new WebUploadHandoffTokenRequest
        {
            QuoteSessionId = quoteSessionId,
            Files =
            [
                new WebUploadHandoffFileDto
                {
                    UploadId = "upload-a",
                    FileName = "bracket.step",
                    StoragePath = $"quotes/temp/{quoteSessionId:N}/420000/bracket.step",
                    ContentType = "application/step",
                    FileSizeBytes = 420_000,
                    Status = "Completed"
                },
                new WebUploadHandoffFileDto
                {
                    UploadId = "upload-b",
                    FileName = "cover.stl",
                    StoragePath = $"quotes/temp/{quoteSessionId:N}/120000/cover.stl",
                    ContentType = "model/stl",
                    FileSizeBytes = 120_000,
                    Status = "Completed"
                }
            ]
        });
        var handoff = await response.Content.ReadFromJsonAsync<WebUploadHandoffTokenResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(handoff);
        Assert.Contains('.', handoff.HandoffToken);
        Assert.DoesNotContain("bracket.step", handoff.HandoffToken, StringComparison.Ordinal);
        Assert.DoesNotContain("cover.stl", handoff.HandoffToken, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies Web signs completed supplemental quote context before handing storage paths to QuoteEngine.
    /// </summary>
    [Fact]
    public async Task POST_QuoteUploadHandoffToken_SupplementalAttachment_ReturnsSignedToken()
    {
        using var client = _factory.CreateClient();
        var quoteSessionId = Guid.NewGuid();

        var response = await client.PostAsJsonAsync("/web/v1/quote/uploads/handoff-token", new WebUploadHandoffTokenRequest
        {
            QuoteSessionId = quoteSessionId,
            Files =
            [
                new WebUploadHandoffFileDto
                {
                    UploadId = "upload-pdf",
                    FileName = "requirements.pdf",
                    StoragePath = $"quotes/temp/{quoteSessionId:N}/420000/requirements.pdf",
                    ContentType = "application/pdf",
                    FileSizeBytes = 420_000,
                    Status = "Completed"
                }
            ]
        });
        var handoff = await response.Content.ReadFromJsonAsync<WebUploadHandoffTokenResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(handoff);
        Assert.Contains('.', handoff.HandoffToken);
        Assert.DoesNotContain("requirements.pdf", handoff.HandoffToken, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies Web refuses to create handoff tokens that exceed QuoteEngine's wire contract.
    /// </summary>
    [Fact]
    public async Task POST_QuoteUploadHandoffToken_TooManyFilesForQuoteEngineToken_ReturnsBadRequest()
    {
        using var client = _factory.CreateClient();
        var quoteSessionId = Guid.NewGuid();
        var files = Enumerable.Range(1, 80)
            .Select(index =>
            {
                var fileName = $"customer-uploaded-production-bracket-with-long-name-{index:D3}.step";
                return new WebUploadHandoffFileDto
                {
                    UploadId = $"upload-{index:D3}-{Guid.NewGuid():N}",
                    FileName = fileName,
                    StoragePath = $"quotes/temp/{quoteSessionId:N}/{index:D3}/{fileName}",
                    ContentType = "application/step",
                    FileSizeBytes = 420_000,
                    Status = "Completed"
                };
            })
            .ToList();

        var response = await client.PostAsJsonAsync("/web/v1/quote/uploads/handoff-token", new WebUploadHandoffTokenRequest
        {
            QuoteSessionId = quoteSessionId,
            Files = files
        });
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal("Too many uploaded files", problem.Title);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
    }

    /// <summary>
    /// Verifies Web does not sign handoffs for files that have not completed upload.
    /// </summary>
    [Fact]
    public async Task POST_QuoteUploadHandoffToken_IncompleteUpload_ReturnsBadRequest()
    {
        using var client = _factory.CreateClient();
        var quoteSessionId = Guid.NewGuid();

        var response = await client.PostAsJsonAsync("/web/v1/quote/uploads/handoff-token", new WebUploadHandoffTokenRequest
        {
            QuoteSessionId = quoteSessionId,
            Files =
            [
                new WebUploadHandoffFileDto
                {
                    UploadId = "upload-a",
                    FileName = "bracket.step",
                    StoragePath = $"quotes/temp/{quoteSessionId:N}/420000/bracket.step",
                    ContentType = "application/step",
                    FileSizeBytes = 420_000,
                    Status = "Processing"
                }
            ]
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
    /// Verifies contact backend failures are returned as problem details instead of developer exception text.
    /// </summary>
    [Fact]
    public async Task POST_ContactMessage_BackendUnavailable_ReturnsProblemDetails()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/web/v1/contact/messages", new ContactMessageRequest
        {
            FullName = "Website Customer",
            Email = "unavailable@example.com",
            Subject = "Manufacturing question",
            Message = "Can MALIEV review this project?"
        });
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal("ContactService unavailable", problem.Title);
        Assert.Contains("did not respond", problem.Detail, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies customer chatbot messages are routed through the website chatbot boundary.
    /// </summary>
    [Fact]
    public async Task POST_ChatbotMessage_RoutesThroughChatbotBoundary()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/web/v1/chatbot/messages", new CustomerChatbotRequest
        {
            Message = "Can MALIEV help with CNC aluminum parts?",
            Language = "en"
        });
        var chat = await response.Content.ReadFromJsonAsync<CustomerChatbotResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(chat);
        Assert.Equal("assistant", chat.Role);
        Assert.False(chat.IsOutOfScope);
        Assert.Contains("CNC", chat.Content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            response.Headers.GetValues("Set-Cookie"),
            value => value.Contains("maliev_customer_assistant_handoff=", StringComparison.Ordinal)
                && value.Contains("path=/", StringComparison.OrdinalIgnoreCase)
                && value.Contains("HttpOnly", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifies opening the customer chatbot starts a verified website assistant session.
    /// </summary>
    [Fact]
    public async Task POST_ChatbotSession_StartsChatbotBoundarySession()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/web/v1/chatbot/sessions", new CustomerChatbotStartRequest
        {
            Language = "en"
        });
        var chat = await response.Content.ReadFromJsonAsync<CustomerChatbotResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(chat);
        Assert.Equal(Guid.Parse("8d7d1778-f352-4701-8803-2305ca7bb9f2"), chat.SessionId);
        Assert.Equal("assistant", chat.Role);
        Assert.Contains("connected", chat.Content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            response.Headers.GetValues("Set-Cookie"),
            value => value.Contains("maliev_customer_assistant_handoff=", StringComparison.Ordinal)
                && value.Contains("path=/", StringComparison.OrdinalIgnoreCase)
                && value.Contains("HttpOnly", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifies the account session endpoint can be polled by the chatbot before login without redirecting the active page.
    /// </summary>
    [Fact]
    public async Task GET_AccountSession_Anonymous_ReturnsUnauthenticatedSession()
    {
        using var client = _factory.CreateClient();

        var session = await client.GetFromJsonAsync<CustomerAccountSessionDto>("/web/v1/account/session");

        Assert.NotNull(session);
        Assert.False(session.IsAuthenticated);
        Assert.Null(session.CustomerId);
        Assert.Equal(string.Empty, session.Email);
    }

    /// <summary>
    /// Verifies account pages cannot be rendered without a valid authenticated browser session.
    /// </summary>
    [Theory]
    [InlineData("/account")]
    [InlineData("/account/profile")]
    [InlineData("/account/addresses")]
    [InlineData("/account/preferences")]
    [InlineData("/account/orders")]
    public async Task GET_AccountPage_Anonymous_RedirectsToSignIn(string route)
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var response = await client.GetAsync(route);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal("/auth/sign-in", response.Headers.Location.LocalPath);
        Assert.Contains($"returnUrl={Uri.EscapeDataString(route)}", response.Headers.Location.Query, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies customer auth form handlers are registered with MVC services required by antiforgery validation.
    /// </summary>
    [Theory]
    [InlineData("/auth/sign-in/email")]
    [InlineData("/auth/sign-up/email")]
    [InlineData("/auth/forgot-password/request")]
    [InlineData("/auth/reset-password/confirm")]
    public async Task POST_AuthFormActionWithoutToken_DoesNotFailFromMissingMvcServices(string route)
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var response = await client.PostAsync(route, new FormUrlEncodedContent(new Dictionary<string, string>()));

        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
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
        Assert.Contains("Disallow: /account", robots);
        Assert.Contains("Disallow: /cart", robots);
        Assert.Contains("https://www.maliev.com/services", sitemap);
        Assert.Contains("<loc>https://www.maliev.com/services/3d-printing</loc><lastmod>2026-05-18</lastmod>", sitemap);
        Assert.Contains("<loc>https://www.maliev.com/services/cnc-machining</loc><lastmod>2026-05-18</lastmod>", sitemap);
        Assert.Contains("<loc>https://www.maliev.com/services/3d-scanning</loc><lastmod>2026-05-18</lastmod>", sitemap);
        Assert.Contains("<loc>https://www.maliev.com/services/3d-design</loc><lastmod>2026-05-18</lastmod>", sitemap);
        Assert.Contains("<loc>https://www.maliev.com/services/silicone-casting</loc><lastmod>2026-05-18</lastmod>", sitemap);
        Assert.Contains("<loc>https://www.maliev.com/services/rapid-prototyping</loc><lastmod>2026-05-18</lastmod>", sitemap);
        Assert.Contains("<loc>https://www.maliev.com/services/deviation-analysis</loc><lastmod>2026-05-18</lastmod>", sitemap);
        Assert.Contains("https://www.maliev.com/materials", sitemap);
        Assert.Contains("https://www.maliev.com/case-studies/fixture-turnaround", sitemap);
        Assert.Contains("https://www.maliev.com/blog", sitemap);
        Assert.Contains("https://www.maliev.com/blog/design-for-manufacturing", sitemap);
        Assert.Contains("https://www.maliev.com/blog/instant-part-pricing", sitemap);
        Assert.Contains("https://www.maliev.com/privacy", sitemap);
        Assert.Contains("https://www.maliev.com/cookie-policy", sitemap);
        Assert.Contains("https://www.maliev.com/refund-policy", sitemap);
        Assert.Contains("https://www.maliev.com/warranty-policy", sitemap);
        Assert.Contains("https://www.maliev.com/terms", sitemap);
        Assert.Contains("https://www.maliev.com/quote", sitemap);
        Assert.DoesNotContain("https://www.maliev.com/cart", sitemap);
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

        public Task<string?> GetCollectionImageRedirectUrlAsync(string collectionSlug, CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>("https://cdn.example.test/collections/machines.jpg");
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

        public Task<string?> GetProductMediaRedirectUrlAsync(string uploadId, CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>($"https://cdn.example.test/products/media/{uploadId}");
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

    private sealed class FakeQuoteUploadService : IQuoteUploadService
    {
        public Task<WebUploadInitiationResponse> InitiateAsync(WebUploadInitiationRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new WebUploadInitiationResponse
            {
                UploadId = "web-upload-1",
                ProxyUploadUrl = "/web/v1/quote/uploads/resumable/web-upload-1",
                StoragePath = $"quotes/temp/{request.QuoteSessionId:N}/{request.FileSize}/{request.FileName}"
            });
        }

        public Task<WebUploadCompleteResponse> CompleteAsync(string uploadId, CancellationToken cancellationToken)
        {
            return Task.FromResult(new WebUploadCompleteResponse
            {
                UploadId = uploadId,
                FileName = "part.step",
                StoragePath = $"quotes/temp/session/part.step",
                Status = "Completed"
            });
        }

        public Task<HttpResponseMessage> ResumeAsync(string uploadId, Stream content, string? contentType, long? contentLength, string contentRange, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }

        public Task<WebAnalysisStatusResponse> GetAnalysisStatusAsync(string uploadId, CancellationToken cancellationToken)
        {
            return Task.FromResult(new WebAnalysisStatusResponse
            {
                UploadId = uploadId,
                Status = "Uploaded",
                Message = "File upload is complete."
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
            if (request.Email == "unavailable@example.com")
            {
                throw new BackendUnavailableException("ContactService", "ContactService did not respond while creating the contact message.");
            }

            Assert.Equal("customer@example.com", request.Email);
            return Task.FromResult(new ContactMessageResponse("e9f63ee7-5711-4392-893a-5380b90f80e5", "Received"));
        }
    }

    private sealed class FakeCustomerChatbotService : ICustomerChatbotService
    {
        public Task<CustomerChatbotResponse> StartSessionAsync(CustomerChatbotStartRequest request, CancellationToken cancellationToken)
        {
            Assert.Equal("en", request.Language);
            return Task.FromResult(new CustomerChatbotResponse
            {
                SessionId = Guid.Parse("8d7d1778-f352-4701-8803-2305ca7bb9f2"),
                Content = "Hi, Mali here. I am connected and ready to help with MALIEV manufacturing.",
                Role = "assistant",
                Language = "en",
                CreatedAt = DateTimeOffset.Parse("2026-05-17T00:00:00+07:00")
            });
        }

        public Task<CustomerChatbotResponse> SendAsync(CustomerChatbotRequest request, CancellationToken cancellationToken)
        {
            Assert.Equal("Can MALIEV help with CNC aluminum parts?", request.Message);
            return Task.FromResult(new CustomerChatbotResponse
            {
                SessionId = Guid.Parse("8d7d1778-f352-4701-8803-2305ca7bb9f2"),
                MessageId = Guid.Parse("80adf440-8f28-4a4c-9ac9-a7f8ae9d5362"),
                Content = "Yes. MALIEV can support CNC aluminum prototypes, fixtures, and production aids.",
                Role = "assistant",
                Language = "en",
                CreatedAt = DateTimeOffset.Parse("2026-05-17T00:00:00+07:00")
            });
        }
    }

    private sealed class FakePdfServiceClient : IPdfServiceClient
    {
        public static readonly byte[] PdfBytes = [(byte)'%', (byte)'P', (byte)'D', (byte)'F'];

        public Task<byte[]> RenderBlogPracticalNoteAsync(BlogPracticalNotePdfRequest request, CancellationToken cancellationToken)
        {
            Assert.Equal("fdm-print-orientation", request.Slug);
            Assert.Equal(SupportedCultures.DefaultCulture, request.CultureName);
            Assert.Contains("FDM", request.Title, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("https://www.maliev.com/blog/fdm-print-orientation", request.PublicUrl);
            Assert.NotEmpty(request.Sections);
            Assert.NotEmpty(request.Takeaways);
            Assert.NotNull(request.CoverImage);
            Assert.Equal("/images/blog/fdm-print-orientation.jpg", request.CoverImage.Url);
            Assert.Equal("image/jpeg", request.CoverImage.ContentType);
            return Task.FromResult(PdfBytes);
        }

        public Task<byte[]> RenderMaterialDatasheetAsync(MaterialDatasheetPdfRequest request, CancellationToken cancellationToken)
        {
            Assert.Equal("pa12-nylon", request.Slug);
            Assert.Equal(SupportedCultures.DefaultCulture, request.CultureName);
            Assert.Equal("PA12 nylon", request.Name);
            Assert.Equal("MJF / SLS", request.ProcessLabel);
            Assert.Contains("nylon", request.CategoryLabel, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("https://www.maliev.com/materials/pa12-nylon", request.PublicUrl);
            Assert.NotEmpty(request.Specs);
            Assert.NotEmpty(request.Bands);
            Assert.False(string.IsNullOrWhiteSpace(request.Pros));
            Assert.False(string.IsNullOrWhiteSpace(request.Cons));
            Assert.False(string.IsNullOrWhiteSpace(request.Disclaimer));
            return Task.FromResult(PdfBytes);
        }
    }
}
