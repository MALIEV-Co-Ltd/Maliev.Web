using System.Net.Http.Headers;
using Asp.Versioning;
using Maliev.Web.Bff.Security;
using Maliev.Web.Bff.Services;
using Maliev.Web.Shared.Quotes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Maliev.Web.Bff.Controllers;

/// <summary>
/// Customer-facing instant quote API.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("web/v{version:apiVersion}/quote")]
[AllowAnonymous]
public sealed class QuoteController(
    IManufacturingCatalogService manufacturingCatalog,
    IWebQuoteService quoteService,
    IQuoteUploadService uploadService,
    QuoteUploadHandoffToken handoffToken) : ControllerBase
{
    /// <summary>Gets manufacturing and pricing reference data from downstream services.</summary>
    [HttpGet("reference-data")]
    [EnableRateLimiting(WebRateLimiterPolicies.QuoteReference)]
    [ProducesResponseType(typeof(QuoteReferenceDataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetReferenceData(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await manufacturingCatalog.GetReferenceDataAsync(cancellationToken));
        }
        catch (BackendUnavailableException ex)
        {
            return BackendUnavailable(ex);
        }
    }

    /// <summary>Calculates a quote estimate through PricingService.</summary>
    [HttpPost("estimate")]
    [EnableRateLimiting(WebRateLimiterPolicies.QuoteEstimate)]
    [RequestSizeLimit(256_000)]
    [ProducesResponseType(typeof(QuoteEstimateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Estimate([FromBody] QuoteEstimateRequest request, CancellationToken cancellationToken)
    {
        request.CustomerId = ResolveCurrentCustomerId();
        try
        {
            return Ok(await quoteService.EstimateAsync(request, cancellationToken));
        }
        catch (QuoteNotReadyException)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Quote is not ready yet",
                Detail = "We need a completed upload and manufacturing selections before calculating this quote.",
                Status = StatusCodes.Status409Conflict
            });
        }
        catch (BackendUnavailableException ex)
        {
            return BackendUnavailable(ex);
        }
    }

    /// <summary>Initiates a resumable upload session in UploadService.</summary>
    [HttpPost("uploads/resumable")]
    [EnableRateLimiting(WebRateLimiterPolicies.UploadInitiate)]
    [RequestSizeLimit(16_384)]
    [ProducesResponseType(typeof(WebUploadInitiationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> InitiateUpload([FromBody] WebUploadInitiationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FileName) ||
            request.FileName.Length > WebQuoteUploadConstraints.MaxFileNameLength ||
            !WebQuoteUploadConstraints.IsSupportedFileName(request.FileName))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unsupported upload format",
                Detail = $"Use one of these Make Studio attachment formats: {WebQuoteUploadConstraints.SupportedExtensionLabel}.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (request.FileSize <= 0 || request.FileSize > WebQuoteUploadConstraints.MaxFileSizeBytes)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Upload is too large",
                Detail = $"Quote uploads must be between 1 byte and {WebQuoteUploadConstraints.MaxFileSizeMegabytes} MB.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (request.QuoteSessionId == Guid.Empty)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Quote session is required",
                Detail = "Create a quote session before uploading files.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            return Ok(await uploadService.InitiateAsync(request, cancellationToken));
        }
        catch (BackendUnavailableException ex)
        {
            return BackendUnavailable(ex);
        }
    }

    /// <summary>Proxies a raw resumable upload body to UploadService without request-body retries.</summary>
    [HttpPut("uploads/resumable/{uploadId}")]
    [EnableRateLimiting(WebRateLimiterPolicies.UploadStream)]
    [RequestSizeLimit(WebQuoteUploadConstraints.MaxFileSizeBytes)]
    [ProducesResponseType(typeof(WebUploadCompleteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status308PermanentRedirect)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ResumeUpload(string uploadId, CancellationToken cancellationToken)
    {
        var contentRange = Request.Headers.ContentRange.ToString();
        if (!TryValidateContentRange(contentRange, Request.ContentLength))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Valid Content-Range header is required",
                Detail = $"Upload chunks must declare a byte range and a total size no larger than {WebQuoteUploadConstraints.MaxFileSizeMegabytes} MB.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            using var downstream = await uploadService.ResumeAsync(
                uploadId,
                Request.Body,
                Request.ContentType,
                Request.ContentLength,
                contentRange,
                cancellationToken);

            Response.StatusCode = (int)downstream.StatusCode;
            CopyContentHeaders(downstream, Response);
            await downstream.Content.CopyToAsync(Response.Body, cancellationToken);
            return new EmptyResult();
        }
        catch (BackendUnavailableException ex)
        {
            return BackendUnavailable(ex);
        }
    }

    /// <summary>Completes a resumable UploadService session after bytes have reached storage.</summary>
    [HttpPost("uploads/resumable/{uploadId}/complete")]
    [EnableRateLimiting(WebRateLimiterPolicies.UploadFinalize)]
    [RequestSizeLimit(16_384)]
    [ProducesResponseType(typeof(WebUploadCompleteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CompleteUpload(string uploadId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await uploadService.CompleteAsync(uploadId, cancellationToken));
        }
        catch (BackendUnavailableException ex)
        {
            return BackendUnavailable(ex);
        }
    }

    /// <summary>Signs completed Web uploads for QuoteEngine handoff.</summary>
    [HttpPost("uploads/handoff-token")]
    [EnableRateLimiting(WebRateLimiterPolicies.UploadHandoff)]
    [RequestSizeLimit(512_000)]
    [ProducesResponseType(typeof(WebUploadHandoffTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CreateHandoffToken(
        [FromBody] WebUploadHandoffTokenRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateHandoffTokenRequest(request);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var canonicalFiles = new List<WebUploadHandoffFileDto>(request.Files.Count);
        try
        {
            foreach (var file in request.Files)
            {
                var canonicalFile = await uploadService.ResolveCompletedHandoffFileAsync(
                    request.QuoteSessionId,
                    file,
                    cancellationToken);
                if (canonicalFile is null)
                {
                    return BadRequest(HandoffProblem(
                        "Invalid uploaded file handoff",
                        "One or more uploaded files could not be verified for QuoteEngine handoff."));
                }

                canonicalFiles.Add(canonicalFile);
            }
        }
        catch (BackendUnavailableException ex)
        {
            return BackendUnavailable(ex);
        }

        var token = handoffToken.Create(new WebUploadHandoffTokenRequest
        {
            QuoteSessionId = request.QuoteSessionId,
            Files = canonicalFiles
        });
        if (token.Length > WebQuoteUploadConstraints.MaxQuoteEngineHandoffTokenLength)
        {
            return BadRequest(HandoffProblem(
                "Too many uploaded files",
                "Upload fewer files at a time so QuoteEngine can import the handoff securely."));
        }

        return Ok(new WebUploadHandoffTokenResponse
        {
            HandoffToken = token
        });
    }

    /// <summary>Gets upload analysis status for quote polling.</summary>
    [HttpGet("uploads/{uploadId}/analysis-status")]
    [EnableRateLimiting(WebRateLimiterPolicies.UploadStatus)]
    [ProducesResponseType(typeof(WebAnalysisStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetAnalysisStatus(string uploadId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(uploadId) || uploadId.Length > 128)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid upload identifier",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            return Ok(await uploadService.GetAnalysisStatusAsync(uploadId, cancellationToken));
        }
        catch (BackendUnavailableException ex)
        {
            return BackendUnavailable(ex);
        }
    }

    private static ProblemDetails? ValidateHandoffTokenRequest(WebUploadHandoffTokenRequest request)
    {
        if (request.QuoteSessionId == Guid.Empty)
        {
            return HandoffProblem("Quote session is required", "Create a quote session before handing files to QuoteEngine.");
        }

        if (request.Files.Count == 0)
        {
            return HandoffProblem("Uploaded files are required", "Upload at least one manufacturing file or supplemental attachment before continuing to Make Studio.");
        }

        if (request.Files.Count > WebQuoteUploadConstraints.MaxFilesPerHandoff)
        {
            return HandoffProblem(
                "Too many uploaded files",
                $"Upload no more than {WebQuoteUploadConstraints.MaxFilesPerHandoff} files at a time.");
        }

        var expectedPrefix = $"quotes/temp/{request.QuoteSessionId:N}/";
        foreach (var file in request.Files)
        {
            if (string.IsNullOrWhiteSpace(file.UploadId) ||
                string.IsNullOrWhiteSpace(file.FileName) ||
                !WebQuoteUploadConstraints.IsSupportedFileName(file.FileName) ||
                file.FileSizeBytes <= 0 ||
                file.FileSizeBytes > WebQuoteUploadConstraints.MaxFileSizeBytes ||
                !file.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(file.StoragePath) ||
                !file.StoragePath.Replace('\\', '/').StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return HandoffProblem(
                    "Invalid uploaded file handoff",
                    "One or more uploaded files could not be verified for QuoteEngine handoff.");
            }
        }

        return null;
    }

    private Guid? ResolveCurrentCustomerId() =>
        Guid.TryParse(User.FindFirst("customer_id")?.Value, out var customerId)
            ? customerId
            : null;

    private static bool TryValidateContentRange(string contentRange, long? contentLength)
    {
        if (!ContentRangeHeaderValue.TryParse(contentRange, out var parsed) ||
            !parsed.HasRange ||
            !parsed.HasLength ||
            parsed.From is null ||
            parsed.To is null ||
            parsed.Length is null ||
            parsed.From < 0 ||
            parsed.To < parsed.From ||
            parsed.Length <= 0 ||
            parsed.Length > WebQuoteUploadConstraints.MaxFileSizeBytes ||
            parsed.To >= parsed.Length)
        {
            return false;
        }

        var declaredChunkLength = parsed.To.Value - parsed.From.Value + 1;
        return contentLength is not null && contentLength == declaredChunkLength;
    }

    private static ProblemDetails HandoffProblem(string title, string detail)
        => new()
        {
            Title = title,
            Detail = detail,
            Status = StatusCodes.Status400BadRequest
        };

    private static void CopyContentHeaders(HttpResponseMessage downstream, HttpResponse response)
    {
        foreach (var header in downstream.Content.Headers)
        {
            response.Headers[header.Key] = header.Value.ToArray();
        }
    }

    private ObjectResult BackendUnavailable(BackendUnavailableException _)
    {
        return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
        {
            Title = "Instant quote is temporarily unavailable",
            Detail = "We could not load quotation tools right now. Please refresh the page or contact MALIEV.",
            Status = StatusCodes.Status503ServiceUnavailable
        });
    }
}
