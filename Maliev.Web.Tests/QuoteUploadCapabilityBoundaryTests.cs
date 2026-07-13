using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Maliev.Web.Bff.Security;
using Maliev.Web.Bff.Services;
using Maliev.Web.Shared.Quotes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Maliev.Web.Tests;

/// <summary>
/// Endpoint regressions for the anonymous browser-to-upload capability boundary.
/// </summary>
public sealed class QuoteUploadCapabilityBoundaryTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>Initializes upload-capability endpoint tests.</summary>
    public QuoteUploadCapabilityBoundaryTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    /// <summary>Verifies upload initiation returns a two-hour, opaque server capability.</summary>
    [Fact]
    public async Task POST_UploadInitiation_ReturnsOpaqueCapabilityBoundToUpload()
    {
        var uploadService = new TrackingQuoteUploadService();
        using var factory = CreateFactory(uploadService);
        using var client = factory.CreateClient();
        var quoteSessionId = Guid.NewGuid();

        var session = await InitiateAsync(client, quoteSessionId);

        Assert.Equal(1, uploadService.InitiateCalls);
        Assert.False(string.IsNullOrWhiteSpace(session.UploadCapability));
        Assert.DoesNotContain(session.UploadId, session.UploadCapability, StringComparison.Ordinal);
        Assert.DoesNotContain(quoteSessionId.ToString("D"), session.UploadCapability, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Verifies every upload follow-up endpoint rejects a missing capability before downstream I/O.</summary>
    [Theory]
    [InlineData("resume")]
    [InlineData("complete")]
    [InlineData("status")]
    public async Task UploadFollowUp_MissingCapability_ReturnsUnauthorizedWithoutDownstream(string operation)
    {
        var uploadService = new TrackingQuoteUploadService();
        using var factory = CreateFactory(uploadService);
        using var client = factory.CreateClient();

        using var response = await SendFollowUpAsync(client, operation, "web-upload-123", capability: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(0, uploadService.FollowUpCalls);
    }

    /// <summary>Verifies an altered capability is rejected before UploadService completion.</summary>
    [Fact]
    public async Task POST_UploadComplete_TamperedCapability_ReturnsUnauthorizedWithoutDownstream()
    {
        var uploadService = new TrackingQuoteUploadService();
        using var factory = CreateFactory(uploadService);
        using var client = factory.CreateClient();
        var session = await InitiateAsync(client, Guid.NewGuid());
        var tampered = session.UploadCapability[..^1] +
            (session.UploadCapability[^1] == 'A' ? 'B' : 'A');

        using var response = await SendFollowUpAsync(client, "complete", session.UploadId, tampered);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(0, uploadService.CompleteCalls);
    }

    /// <summary>Verifies a capability for one upload cannot authorize status disclosure for another upload.</summary>
    [Fact]
    public async Task GET_UploadStatus_CapabilityForDifferentUpload_ReturnsUnauthorizedWithoutDownstream()
    {
        var uploadService = new TrackingQuoteUploadService();
        using var factory = CreateFactory(uploadService);
        using var client = factory.CreateClient();
        var session = await InitiateAsync(client, Guid.NewGuid());

        using var response = await SendFollowUpAsync(
            client,
            "status",
            "web-upload-other",
            session.UploadCapability);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(0, uploadService.StatusCalls);
    }

    /// <summary>Verifies a file capability cannot cross the anonymous quote session used for handoff.</summary>
    [Fact]
    public async Task POST_HandoffToken_CapabilityForDifferentQuoteSession_ReturnsUnauthorizedWithoutDownstream()
    {
        var uploadService = new TrackingQuoteUploadService();
        using var factory = CreateFactory(uploadService);
        using var client = factory.CreateClient();
        var owningSessionId = Guid.NewGuid();
        var requestedSessionId = Guid.NewGuid();
        var session = await InitiateAsync(client, owningSessionId);

        using var response = await client.PostAsJsonAsync(
            "/web/v1/quote/uploads/handoff-token",
            new WebUploadHandoffTokenRequest
            {
                QuoteSessionId = requestedSessionId,
                Files =
                [
                    new WebUploadHandoffFileDto
                    {
                        UploadId = session.UploadId,
                        UploadCapability = session.UploadCapability,
                        FileName = "fixture.step",
                        StoragePath = $"quotes/temp/{requestedSessionId:N}/420000/fixture.step",
                        ContentType = "application/step",
                        FileSizeBytes = 420_000,
                        Status = "Completed"
                    }
                ]
            });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(0, uploadService.ResolveCalls);
    }

    /// <summary>Verifies a missing per-file capability blocks handoff before UploadService lookup.</summary>
    [Fact]
    public async Task POST_HandoffToken_MissingFileCapability_ReturnsUnauthorizedWithoutDownstream()
    {
        var uploadService = new TrackingQuoteUploadService();
        using var factory = CreateFactory(uploadService);
        using var client = factory.CreateClient();
        var quoteSessionId = Guid.NewGuid();

        using var response = await client.PostAsJsonAsync(
            "/web/v1/quote/uploads/handoff-token",
            new WebUploadHandoffTokenRequest
            {
                QuoteSessionId = quoteSessionId,
                Files =
                [
                    new WebUploadHandoffFileDto
                    {
                        UploadId = "web-upload-123",
                        FileName = "fixture.step",
                        StoragePath = $"quotes/temp/{quoteSessionId:N}/420000/fixture.step",
                        ContentType = "application/step",
                        FileSizeBytes = 420_000,
                        Status = "Completed"
                    }
                ]
            });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(0, uploadService.ResolveCalls);
    }

    /// <summary>Verifies PricingService is not called for a browser-authored upload lacking its capability.</summary>
    [Fact]
    public async Task POST_Estimate_MissingPartCapability_ReturnsUnauthorizedWithoutPricingCall()
    {
        var uploadService = new TrackingQuoteUploadService();
        var quoteService = new TrackingWebQuoteService();
        using var factory = CreateFactory(uploadService, quoteService);
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/web/v1/quote/estimate",
            new QuoteEstimateRequest
            {
                Parts =
                [
                    new QuotePartDraftDto
                    {
                        UploadId = "web-upload-123",
                        StoragePath = "quotes/temp/session/420000/fixture.step",
                        File = new QuoteFileDraftDto
                        {
                            Name = "fixture.step",
                            ContentType = "application/step",
                            SizeBytes = 420_000
                        },
                        FileId = Guid.NewGuid(),
                        ManufacturingProcessId = Guid.NewGuid(),
                        MaterialId = Guid.NewGuid(),
                        DfmAcknowledged = true
                    }
                ]
            });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(0, quoteService.EstimateCalls);
    }

    /// <summary>Verifies a valid capability authorizes each upload follow-up without becoming browser identity.</summary>
    [Theory]
    [InlineData("resume")]
    [InlineData("complete")]
    [InlineData("status")]
    public async Task UploadFollowUp_ValidCapability_ReachesExpectedDownstreamOperation(string operation)
    {
        var uploadService = new TrackingQuoteUploadService();
        using var factory = CreateFactory(uploadService);
        using var client = factory.CreateClient();
        var session = await InitiateAsync(client, Guid.NewGuid());

        using var response = await SendFollowUpAsync(
            client,
            operation,
            session.UploadId,
            session.UploadCapability);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, operation switch
        {
            "resume" => uploadService.ResumeCalls,
            "complete" => uploadService.CompleteCalls,
            "status" => uploadService.StatusCalls,
            _ => 0
        });
    }

    /// <summary>Protected file size must match the upload range total before streaming.</summary>
    [Fact]
    public async Task PUT_UploadRangeTotalMismatch_ReturnsBadRequestWithoutDownstream()
    {
        var uploadService = new TrackingQuoteUploadService();
        using var factory = CreateFactory(uploadService);
        using var client = factory.CreateClient();
        var session = await InitiateAsync(client, Guid.NewGuid());
        using var content = new ByteArrayContent([0x01]);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Headers.ContentLength = 1;
        content.Headers.ContentRange = new ContentRangeHeaderValue(0, 0, 1);
        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"/web/v1/quote/uploads/resumable/{Uri.EscapeDataString(session.UploadId)}")
        {
            Content = content
        };
        request.Headers.Add(UploadCapabilityProtector.HeaderName, session.UploadCapability);

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, uploadService.ResumeCalls);
    }

    /// <summary>Browser file identifiers and geometry never reach pricing without authoritative geometry.</summary>
    [Fact]
    public async Task POST_Estimate_BrowserFileAndVolumeClaims_DoNotReachPricing()
    {
        var uploadService = new TrackingQuoteUploadService();
        var quoteService = new TrackingWebQuoteService();
        using var factory = CreateFactory(uploadService, quoteService);
        using var client = factory.CreateClient();
        var session = await InitiateAsync(client, Guid.NewGuid());

        using var response = await client.PostAsJsonAsync(
            "/web/v1/quote/estimate",
            new QuoteEstimateRequest
            {
                Parts =
                [
                    new QuotePartDraftDto
                    {
                        UploadId = session.UploadId,
                        UploadCapability = session.UploadCapability,
                        StoragePath = session.StoragePath,
                        FileId = Guid.NewGuid(),
                        File = new QuoteFileDraftDto
                        {
                            Name = "fixture.step",
                            ContentType = "application/step",
                            SizeBytes = 420_000
                        },
                        EstimatedVolumeCc = 999_999m,
                        ManufacturingProcessId = Guid.NewGuid(),
                        MaterialId = Guid.NewGuid(),
                        DfmAcknowledged = true
                    }
                ]
            });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(1, uploadService.ResolveCalls);
        Assert.Equal(0, quoteService.EstimateCalls);
    }

    private WebApplicationFactory<Program> CreateFactory(
        TrackingQuoteUploadService uploadService,
        TrackingWebQuoteService? quoteService = null)
    {
        return _factory.WithWebHostBuilder(builder => builder
            .UseEnvironment("Testing")
            .ConfigureServices(services =>
            {
                services.RemoveAll<IQuoteUploadService>();
                services.AddSingleton<IQuoteUploadService>(uploadService);
                if (quoteService is not null)
                {
                    services.RemoveAll<IWebQuoteService>();
                    services.AddSingleton<IWebQuoteService>(quoteService);
                }
            }));
    }

    private static async Task<WebUploadInitiationResponse> InitiateAsync(
        HttpClient client,
        Guid quoteSessionId)
    {
        using var response = await client.PostAsJsonAsync(
            "/web/v1/quote/uploads/resumable",
            new WebUploadInitiationRequest
            {
                FileName = "fixture.step",
                ContentType = "application/step",
                FileSize = 420_000,
                QuoteSessionId = quoteSessionId
            });
        var session = await response.Content.ReadFromJsonAsync<WebUploadInitiationResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return Assert.IsType<WebUploadInitiationResponse>(session);
    }

    private static Task<HttpResponseMessage> SendFollowUpAsync(
        HttpClient client,
        string operation,
        string uploadId,
        string? capability)
    {
        var request = operation switch
        {
            "resume" => CreateResumeRequest(uploadId),
            "complete" => new HttpRequestMessage(
                HttpMethod.Post,
                $"/web/v1/quote/uploads/resumable/{Uri.EscapeDataString(uploadId)}/complete"),
            "status" => new HttpRequestMessage(
                HttpMethod.Get,
                $"/web/v1/quote/uploads/{Uri.EscapeDataString(uploadId)}/analysis-status"),
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };
        if (capability is not null)
        {
            request.Headers.Add(UploadCapabilityProtector.HeaderName, capability);
        }

        return client.SendAsync(request);
    }

    private static HttpRequestMessage CreateResumeRequest(string uploadId)
    {
        var content = new ByteArrayContent([0x01]);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Headers.ContentLength = 1;
        content.Headers.ContentRange = new ContentRangeHeaderValue(0, 0, 420_000);
        return new HttpRequestMessage(
            HttpMethod.Put,
            $"/web/v1/quote/uploads/resumable/{Uri.EscapeDataString(uploadId)}")
        {
            Content = content
        };
    }

    private sealed class TrackingQuoteUploadService : IQuoteUploadService
    {
        public int InitiateCalls { get; private set; }

        public int ResumeCalls { get; private set; }

        public int CompleteCalls { get; private set; }

        public int StatusCalls { get; private set; }

        public int ResolveCalls { get; private set; }

        public int FollowUpCalls => ResumeCalls + CompleteCalls + StatusCalls + ResolveCalls;

        public Task<WebUploadInitiationResponse> InitiateAsync(
            WebUploadInitiationRequest request,
            CancellationToken cancellationToken)
        {
            InitiateCalls++;
            return Task.FromResult(new WebUploadInitiationResponse
            {
                UploadId = "web-upload-123",
                ProxyUploadUrl = "/web/v1/quote/uploads/resumable/web-upload-123",
                StoragePath = $"quotes/temp/{request.QuoteSessionId:N}/{request.FileSize}/{request.FileName}"
            });
        }

        public Task<WebUploadCompleteResponse> CompleteAsync(
            string uploadId,
            CancellationToken cancellationToken)
        {
            CompleteCalls++;
            return Task.FromResult(new WebUploadCompleteResponse
            {
                UploadId = uploadId,
                FileName = "fixture.step",
                StoragePath = "quotes/temp/session/fixture.step",
                Status = "Completed"
            });
        }

        public Task<HttpResponseMessage> ResumeAsync(
            string uploadId,
            Stream content,
            string? contentType,
            long? contentLength,
            string contentRange,
            CancellationToken cancellationToken)
        {
            ResumeCalls++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }

        public Task<WebAnalysisStatusResponse> GetAnalysisStatusAsync(
            string uploadId,
            CancellationToken cancellationToken)
        {
            StatusCalls++;
            return Task.FromResult(new WebAnalysisStatusResponse
            {
                UploadId = uploadId,
                Status = "Uploaded"
            });
        }

        public Task<WebUploadHandoffFileDto?> ResolveCompletedHandoffFileAsync(
            Guid quoteSessionId,
            WebUploadHandoffFileDto claimedFile,
            CancellationToken cancellationToken)
        {
            ResolveCalls++;
            return Task.FromResult<WebUploadHandoffFileDto?>(claimedFile);
        }
    }

    private sealed class TrackingWebQuoteService : IWebQuoteService
    {
        public int EstimateCalls { get; private set; }

        public Task<QuoteEstimateResponse> EstimateAsync(
            QuoteEstimateRequest request,
            CancellationToken cancellationToken)
        {
            EstimateCalls++;
            return Task.FromResult(new QuoteEstimateResponse());
        }
    }
}
