using Asp.Versioning;
using Maliev.Web.Bff.Security;
using Maliev.Web.Bff.Services;
using Maliev.Web.Shared.Quotes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    [ProducesResponseType(typeof(QuoteEstimateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Estimate([FromBody] QuoteEstimateRequest request, CancellationToken cancellationToken)
    {
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
    [ProducesResponseType(typeof(WebUploadInitiationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> InitiateUpload([FromBody] WebUploadInitiationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FileName) ||
            !WebQuoteUploadConstraints.IsSupportedFileName(request.FileName))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unsupported upload format",
                Detail = $"Use one of these CAD formats: {WebQuoteUploadConstraints.SupportedExtensionLabel}.",
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
    [DisableRequestSizeLimit]
    [ProducesResponseType(typeof(WebUploadCompleteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status308PermanentRedirect)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ResumeUpload(string uploadId, CancellationToken cancellationToken)
    {
        var contentRange = Request.Headers.ContentRange.ToString();
        if (string.IsNullOrWhiteSpace(contentRange))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Content-Range header is required",
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
    [ProducesResponseType(typeof(WebUploadHandoffTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult CreateHandoffToken([FromBody] WebUploadHandoffTokenRequest request)
    {
        var validationError = ValidateHandoffTokenRequest(request);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        return Ok(new WebUploadHandoffTokenResponse
        {
            HandoffToken = handoffToken.Create(request)
        });
    }

    /// <summary>Gets upload analysis status for quote polling.</summary>
    [HttpGet("uploads/{uploadId}/analysis-status")]
    [ProducesResponseType(typeof(WebAnalysisStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetAnalysisStatus(string uploadId, CancellationToken cancellationToken)
    {
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
            return HandoffProblem("Uploaded files are required", "Upload at least one CAD file before continuing to QuoteEngine.");
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
