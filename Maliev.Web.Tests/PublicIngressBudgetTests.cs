using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Maliev.Web.Bff.Clients;
using Maliev.Web.Bff.Controllers;
using Maliev.Web.Bff.Geometry;
using Maliev.Web.Bff.Security;
using Maliev.Web.Bff.Services;
using Maliev.Web.Shared.Chatbot;
using Maliev.Web.Shared.Commerce;
using Maliev.Web.Shared.Contact;
using Maliev.Web.Shared.Quotes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Maliev.Web.Tests;

/// <summary>
/// Regression coverage for bounded, anonymous endpoints that allocate downstream work.
/// </summary>
public sealed class PublicIngressBudgetTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const int MaxContactAttachmentBytes = 10 * 1024 * 1024;

    private readonly WebApplicationFactory<Program> _factory;
    private readonly CapturingWebQuoteService _quoteService = new();
    private readonly CapturingContactMessageService _contactService = new();
    private readonly CapturingCheckoutDraftService _checkoutService = new();
    private readonly CapturingDeliveryServiceClient _deliveryService = new();

    /// <summary>
    /// Creates a test host whose downstream boundaries record whether invalid input escaped the BFF.
    /// </summary>
    public PublicIngressBudgetTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder
            .UseEnvironment("Testing")
            .ConfigureServices(services =>
            {
                services.RemoveAll<IWebQuoteService>();
                services.RemoveAll<IContactMessageService>();
                services.RemoveAll<ICheckoutDraftService>();
                services.RemoveAll<IDeliveryServiceClient>();
                services.AddSingleton<IWebQuoteService>(_quoteService);
                services.AddSingleton<IContactMessageService>(_contactService);
                services.AddSingleton<ICheckoutDraftService>(_checkoutService);
                services.AddSingleton<IDeliveryServiceClient>(_deliveryService);
            }));
    }

    /// <summary>Endpoints and maximum request bodies expected at the public ingress boundary.</summary>
    public static TheoryData<Type, string, string, long> BoundedEndpoints => new()
    {
        { typeof(ChatbotController), nameof(ChatbotController.StartSession), WebRateLimiterPolicies.ChatbotSession, 4_096 },
        { typeof(ChatbotController), nameof(ChatbotController.Send), WebRateLimiterPolicies.ChatbotMessage, 8_192 },
        { typeof(ContactController), nameof(ContactController.Submit), WebRateLimiterPolicies.Contact, 72L * 1024 * 1024 },
        { typeof(QuoteController), nameof(QuoteController.Estimate), WebRateLimiterPolicies.QuoteEstimate, 256_000 },
        { typeof(QuoteController), nameof(QuoteController.InitiateUpload), WebRateLimiterPolicies.UploadInitiate, 16_384 },
        { typeof(QuoteController), nameof(QuoteController.ResumeUpload), WebRateLimiterPolicies.UploadStream, WebQuoteUploadConstraints.MaxFileSizeBytes },
        { typeof(QuoteController), nameof(QuoteController.CompleteUpload), WebRateLimiterPolicies.UploadFinalize, 16_384 },
        { typeof(QuoteController), nameof(QuoteController.CreateHandoffToken), WebRateLimiterPolicies.UploadHandoff, 512_000 },
        { typeof(CheckoutController), nameof(CheckoutController.CreateDraft), WebRateLimiterPolicies.Checkout, 256_000 },
        { typeof(CheckoutController), nameof(CheckoutController.CreateDraftForm), WebRateLimiterPolicies.Checkout, 256_000 },
        { typeof(ShippingController), nameof(ShippingController.GetRates), WebRateLimiterPolicies.ShippingRate, 32_000 }
    };

    /// <summary>Anonymous read endpoints that allocate downstream work and therefore require an IP budget.</summary>
    public static TheoryData<Type, string, string> RateLimitedReadEndpoints => new()
    {
        { typeof(QuoteController), nameof(QuoteController.GetReferenceData), WebRateLimiterPolicies.QuoteReference },
        { typeof(QuoteController), nameof(QuoteController.GetAnalysisStatus), WebRateLimiterPolicies.UploadStatus },
        { typeof(ShippingController), nameof(ShippingController.GetCouriers), WebRateLimiterPolicies.ShippingRead },
        { typeof(ShippingController), nameof(ShippingController.GetTracking), WebRateLimiterPolicies.ShippingRead }
    };

    /// <summary>
    /// Verifies each cost-bearing anonymous endpoint opts into a named limiter and an explicit body budget.
    /// </summary>
    [Theory]
    [MemberData(nameof(BoundedEndpoints))]
    public void AnonymousCostBearingEndpoint_DeclaresRateAndRequestSizeBudgets(
        Type controllerType,
        string methodName,
        string expectedPolicy,
        long expectedBodyLimit)
    {
        var method = Assert.Single(
            controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly),
            candidate => candidate.Name == methodName);
        var limiter = Assert.Single(method.GetCustomAttributes<EnableRateLimitingAttribute>());
        var requestLimit = Assert.Single(method.GetCustomAttributes<RequestSizeLimitAttribute>());
        var metadata = Assert.IsAssignableFrom<IRequestSizeLimitMetadata>(requestLimit);

        Assert.Equal(expectedPolicy, limiter.PolicyName);
        Assert.Equal(expectedBodyLimit, metadata.MaxRequestBodySize);
    }

    /// <summary>Verifies cost-bearing anonymous reads cannot bypass the shared client-IP floor.</summary>
    [Theory]
    [MemberData(nameof(RateLimitedReadEndpoints))]
    public void AnonymousCostBearingRead_DeclaresRateBudget(
        Type controllerType,
        string methodName,
        string expectedPolicy)
    {
        var method = Assert.Single(
            controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly),
            candidate => candidate.Name == methodName);
        var limiter = Assert.Single(method.GetCustomAttributes<EnableRateLimitingAttribute>());

        Assert.Equal(expectedPolicy, limiter.PolicyName);
    }

    /// <summary>Production refuses to start when ingress client IPs cannot be resolved safely.</summary>
    [Fact]
    public void ProductionWithoutTrustedProxyConfiguration_FailsClosed()
    {
        using var productionFactory = _factory.WithWebHostBuilder(builder => builder.UseEnvironment("Production"));

        var exception = Assert.Throws<InvalidOperationException>(() => productionFactory.CreateClient());

        Assert.Contains("trusted reverse proxy", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Production refuses pod-local Data Protection keys that would invalidate upload capabilities.</summary>
    [Fact]
    public void ProductionWithoutSharedRedisKeyRing_FailsClosed()
    {
        using var productionFactory = _factory.WithWebHostBuilder(builder => builder
            .UseEnvironment("Production")
            .UseSetting("ReverseProxy:KnownProxies:0", "127.0.0.1"));

        var exception = Assert.Throws<InvalidOperationException>(() => productionFactory.CreateClient());

        Assert.Contains("shared Redis", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies a public caller cannot select the PricingService customer identity in JSON.
    /// </summary>
    [Fact]
    public async Task QuoteEstimate_BrowserCustomerId_IsRemovedBeforeDownstream()
    {
        _quoteService.Reset();
        using var client = _factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/web/v1/quote/estimate", new QuoteEstimateRequest
        {
            CustomerId = Guid.NewGuid(),
            Parts = []
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, _quoteService.CallCount);
        Assert.NotNull(_quoteService.Request);
        Assert.Null(_quoteService.Request.CustomerId);
    }

    /// <summary>
    /// Verifies an authenticated estimate uses the session claim instead of a browser-selected customer id.
    /// </summary>
    [Fact]
    public async Task QuoteEstimate_AuthenticatedCustomer_UsesClaimInsteadOfBrowserCustomerId()
    {
        var claimedCustomerId = Guid.NewGuid();
        var quoteService = new CapturingWebQuoteService();
        var controller = new QuoteController(
            null!, quoteService, null!, null!, CreateUploadCapabilityProtector(), CreateGeometryAccessor())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim("customer_id", claimedCustomerId.ToString("D"))
                    ], "test"))
                }
            }
        };

        var result = await controller.Estimate(new QuoteEstimateRequest
        {
            CustomerId = Guid.NewGuid(),
            Parts = []
        }, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, quoteService.CallCount);
        Assert.NotNull(quoteService.Request);
        Assert.Equal(claimedCustomerId, quoteService.Request.CustomerId);
    }

    /// <summary>
    /// Verifies quote fan-out is bounded before one downstream pricing call can be allocated per part.
    /// </summary>
    [Fact]
    public async Task QuoteEstimate_MoreThanTwentyParts_IsRejectedBeforeDownstream()
    {
        _quoteService.Reset();
        using var client = _factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/web/v1/quote/estimate", new QuoteEstimateRequest
        {
            Parts = Enumerable.Range(0, 21).Select(_ => new QuotePartDraftDto()).ToList()
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, _quoteService.CallCount);
    }

    /// <summary>Null process-option values from JSON produce validation errors instead of server failures.</summary>
    [Fact]
    public async Task QuoteEstimate_NullProcessOptionValue_IsRejectedBeforeDownstream()
    {
        _quoteService.Reset();
        using var client = _factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/web/v1/quote/estimate", new QuoteEstimateRequest
        {
            Parts =
            [
                new QuotePartDraftDto
                {
                    ProcessOptionValues = new Dictionary<string, string> { ["infill"] = null! }
                }
            ]
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, _quoteService.CallCount);
    }

    /// <summary>
    /// Verifies the configured quote budget rejects excess work with a retry contract before downstream execution.
    /// </summary>
    [Fact]
    public async Task QuoteEstimate_ExceedsIpBudget_ReturnsProblemAndDoesNotCallDownstream()
    {
        var quoteService = new CapturingWebQuoteService();
        await using var scopedFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Security:PublicIngressRateLimiting:QuoteEstimatePermitLimit", "1");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IWebQuoteService>();
                services.AddSingleton<IWebQuoteService>(quoteService);
            });
        });
        using var client = scopedFactory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });
        var request = new QuoteEstimateRequest { Parts = [] };

        using var accepted = await client.PostAsJsonAsync("/web/v1/quote/estimate", request);
        using var rejected = await client.PostAsJsonAsync("/web/v1/quote/estimate", request);

        Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.Equal(1, quoteService.CallCount);
        Assert.NotNull(rejected.Headers.RetryAfter?.Delta);
        Assert.True(rejected.Headers.RetryAfter!.Delta >= TimeSpan.FromSeconds(1));
        Assert.Equal("application/problem+json", rejected.Content.Headers.ContentType?.MediaType);
    }

    /// <summary>
    /// Verifies the request-size budget returns a problem response before chatbot parsing or downstream execution.
    /// </summary>
    [Fact]
    public async Task ChatbotMessage_ExceedsBodyBudget_ReturnsProblemAndDoesNotCallDownstream()
    {
        var chatbotService = new CapturingCustomerChatbotService();
        await using var scopedFactory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ICustomerChatbotService>();
                services.AddSingleton<ICustomerChatbotService>(chatbotService);
            }));
        using var client = scopedFactory.CreateClient();
        using var content = JsonContent.Create(new
        {
            message = new string('x', 9_000),
            language = "en"
        });

        using var response = await client.PostAsync("/web/v1/chatbot/messages", content);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(0, chatbotService.CallCount);
    }

    /// <summary>
    /// Verifies malformed and oversized byte ranges never reach the streaming upload boundary.
    /// </summary>
    [Theory]
    [InlineData("bytes malformed")]
    [InlineData("bytes 0-0/209715201")]
    public async Task ResumeUpload_InvalidContentRange_IsRejectedBeforeDownstream(string contentRange)
    {
        var uploadService = new CapturingQuoteUploadService();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.ContentRange = contentRange;
        httpContext.Request.ContentLength = 1;
        httpContext.Request.Body = new MemoryStream([0x01]);
        var protector = CreateUploadCapabilityProtector();
        httpContext.Request.Headers[UploadCapabilityProtector.HeaderName] = protector.Create(
            "upload-id",
            Guid.NewGuid(),
            "quotes/temp/test/file.step",
            1);
        var controller = new QuoteController(null!, null!, uploadService, null!, protector, CreateGeometryAccessor())
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };

        var result = await controller.ResumeUpload("upload-id", CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(0, uploadService.ResumeCallCount);
    }

    private static GeometryAnalysisAccessor CreateGeometryAccessor() =>
        new(new InMemoryGeometryAnalysisStore(TimeProvider.System));

    /// <summary>
    /// Verifies attachment count is bounded before contact payloads are decoded or forwarded.
    /// </summary>
    [Fact]
    public async Task ContactMessage_MoreThanFiveAttachments_IsRejectedBeforeDownstream()
    {
        _contactService.Reset();
        using var client = _factory.CreateClient();
        var request = ValidContactRequest();
        request.Files = Enumerable.Range(0, 6).Select(index => new ContactAttachmentDto
        {
            FileName = $"attachment-{index}.txt",
            ContentType = "text/plain",
            Base64Content = Convert.ToBase64String([(byte)index])
        }).ToList();

        using var response = await client.PostAsJsonAsync("/web/v1/contact/messages", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, _contactService.CallCount);
    }

    /// <summary>
    /// Verifies malformed base64 cannot reach the contact-service mapping boundary.
    /// </summary>
    [Fact]
    public async Task ContactMessage_InvalidBase64_IsRejectedBeforeDownstream()
    {
        _contactService.Reset();
        using var client = _factory.CreateClient();
        var request = ValidContactRequest();
        request.Files =
        [
            new ContactAttachmentDto
            {
                FileName = "invalid.txt",
                ContentType = "text/plain",
                Base64Content = "not-base64!"
            }
        ];

        using var response = await client.PostAsJsonAsync("/web/v1/contact/messages", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, _contactService.CallCount);
    }

    /// <summary>Null attachment entries cannot crash recursive request validation.</summary>
    [Theory]
    [InlineData("null")]
    [InlineData("[null]")]
    [InlineData("[{\"fileName\":\"x.txt\",\"base64Content\":null}]")]
    public async Task ContactMessage_NullAttachmentShape_IsRejectedBeforeDownstream(string filesJson)
    {
        _contactService.Reset();
        using var client = _factory.CreateClient();
        using var content = new StringContent(
            $$"""{"fullName":"Boundary Test","email":"boundary@example.test","subject":"Request","message":"Review","files":{{filesJson}}}""",
            System.Text.Encoding.UTF8,
            "application/json");

        using var response = await client.PostAsync("/web/v1/contact/messages", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, _contactService.CallCount);
    }

    /// <summary>
    /// Verifies the decoded per-file attachment budget is enforced before downstream allocation.
    /// </summary>
    [Fact]
    public async Task ContactMessage_AttachmentLargerThanTenMegabytes_IsRejectedBeforeDownstream()
    {
        _contactService.Reset();
        using var client = _factory.CreateClient();
        var request = ValidContactRequest();
        request.Files =
        [
            new ContactAttachmentDto
            {
                FileName = "too-large.bin",
                ContentType = "application/octet-stream",
                Base64Content = Convert.ToBase64String(new byte[MaxContactAttachmentBytes + 1])
            }
        ];

        using var response = await client.PostAsJsonAsync("/web/v1/contact/messages", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, _contactService.CallCount);
    }

    /// <summary>
    /// Verifies checkout cannot turn an unbounded browser cart into repeated product/cart downstream calls.
    /// </summary>
    [Fact]
    public async Task CheckoutDraft_MoreThanFiftyItems_IsRejectedBeforeDownstream()
    {
        _checkoutService.Reset();
        using var client = _factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/web/v1/checkout/draft", new CheckoutDraftRequest
        {
            Items = Enumerable.Range(0, 51).Select(index => new CartItemDto
            {
                ProductHandle = $"product-{index}",
                VariantSku = $"sku-{index}",
                Quantity = 1
            }).ToList(),
            TermsAccepted = true
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, _checkoutService.CallCount);
    }

    /// <summary>
    /// Verifies progressive-enhancement checkout applies the same fan-out cap after parsing its JSON field.
    /// </summary>
    [Fact]
    public async Task CheckoutDraftForm_MoreThanFiftyItems_IsRejectedBeforeDownstream()
    {
        var checkoutService = new CapturingCheckoutDraftService();
        var controller = new CheckoutController(checkoutService, null!)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        var items = Enumerable.Range(0, 51).Select(index => new CartItemDto
        {
            ProductHandle = $"product-{index}",
            VariantSku = $"sku-{index}",
            Quantity = 1
        }).ToList();

        var result = await controller.CreateDraftForm(new CheckoutController.CheckoutDraftForm
        {
            ItemsJson = JsonSerializer.Serialize(items),
            TermsAccepted = true
        }, CancellationToken.None);

        Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal(0, checkoutService.CallCount);
    }

    /// <summary>Progressive form items receive the same recursive validation as JSON checkout.</summary>
    [Fact]
    public async Task CheckoutDraftForm_InvalidItem_IsRejectedBeforeDownstream()
    {
        var checkoutService = new CapturingCheckoutDraftService();
        var controller = new CheckoutController(checkoutService, null!)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = await controller.CreateDraftForm(new CheckoutController.CheckoutDraftForm
        {
            ItemsJson = JsonSerializer.Serialize(new[]
            {
                new CartItemDto { ProductHandle = new string('x', 161), VariantSku = "sku", Quantity = 0 }
            }),
            TermsAccepted = true
        }, CancellationToken.None);

        Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal(0, checkoutService.CallCount);
    }

    /// <summary>
    /// Verifies courier filtering cannot create an unbounded DeliveryService request.
    /// </summary>
    [Fact]
    public async Task ShippingRates_MoreThanTwentyCourierCodes_IsRejectedBeforeDownstream()
    {
        _deliveryService.Reset();
        using var client = _factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/web/v1/shipping/rates", new CheckoutShippingRateRequest
        {
            ShippingDetails = ValidShippingDetails(),
            CourierCodes = Enumerable.Range(0, 21).Select(index => $"courier-{index}").ToList()
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, _deliveryService.RateCallCount);
    }

    private static ContactMessageRequest ValidContactRequest() => new()
    {
        FullName = "Boundary Test",
        Email = "boundary@example.test",
        Subject = "Manufacturing request",
        Message = "Please review the attached manufacturing files."
    };

    private static UploadCapabilityProtector CreateUploadCapabilityProtector() =>
        new(new EphemeralDataProtectionProvider(), TimeProvider.System);

    private static CheckoutShippingDetailsDto ValidShippingDetails() => new()
    {
        RecipientName = "Boundary Test",
        Phone = "0800000000",
        Address = "1 Test Road",
        District = "Bang Rak",
        State = "Bang Rak",
        Province = "Bangkok",
        Postcode = "10500",
        CountryCode = "TH",
        WeightGrams = 1_000,
        LengthCm = 20,
        WidthCm = 15,
        HeightCm = 10
    };

    private sealed class CapturingWebQuoteService : IWebQuoteService
    {
        public int CallCount { get; private set; }

        public QuoteEstimateRequest? Request { get; private set; }

        public Task<QuoteEstimateResponse> EstimateAsync(QuoteEstimateRequest request, CancellationToken cancellationToken)
        {
            CallCount++;
            Request = request;
            return Task.FromResult(new QuoteEstimateResponse { PricingSource = "test" });
        }

        public void Reset()
        {
            CallCount = 0;
            Request = null;
        }
    }

    private sealed class CapturingContactMessageService : IContactMessageService
    {
        public int CallCount { get; private set; }

        public Task<ContactMessageResponse> SubmitAsync(ContactMessageRequest request, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(new ContactMessageResponse("test", "Received"));
        }

        public void Reset() => CallCount = 0;
    }

    private sealed class CapturingCheckoutDraftService : ICheckoutDraftService
    {
        public int CallCount { get; private set; }

        public Task<CheckoutDraftResponse> CreateDraftAsync(
            CheckoutDraftRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(new CheckoutDraftResponse());
        }

        public void Reset() => CallCount = 0;
    }

    private sealed class CapturingDeliveryServiceClient : IDeliveryServiceClient
    {
        public int RateCallCount { get; private set; }

        public Task<HttpResponseMessage> GetShippingCouriersAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(Array.Empty<object>()) });

        public Task<HttpResponseMessage> GetShippingRatesAsync(object request, CancellationToken cancellationToken)
        {
            RateCallCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(Array.Empty<object>()) });
        }

        public Task<HttpResponseMessage> GetShippingTrackingAsync(string trackingCode, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));

        public void Reset() => RateCallCount = 0;
    }

    private sealed class CapturingQuoteUploadService : IQuoteUploadService
    {
        public int ResumeCallCount { get; private set; }

        public Task<WebUploadInitiationResponse> InitiateAsync(
            WebUploadInitiationRequest request,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<WebUploadCompleteResponse> CompleteAsync(
            string uploadId,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<HttpResponseMessage> ResumeAsync(
            string uploadId,
            Stream content,
            string? contentType,
            long? contentLength,
            string contentRange,
            CancellationToken cancellationToken)
        {
            ResumeCallCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }

        public Task<WebAnalysisStatusResponse> GetAnalysisStatusAsync(
            string uploadId,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<WebUploadHandoffFileDto?> ResolveCompletedHandoffFileAsync(
            Guid quoteSessionId,
            WebUploadHandoffFileDto claimedFile,
            CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class CapturingCustomerChatbotService : ICustomerChatbotService
    {
        public int CallCount { get; private set; }

        public Task<CustomerChatbotResponse> StartSessionAsync(
            CustomerChatbotStartRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(new CustomerChatbotResponse());
        }

        public Task<CustomerChatbotResponse> SendAsync(
            CustomerChatbotRequest request,
            ClaimsPrincipal caller,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(new CustomerChatbotResponse());
        }
    }
}
