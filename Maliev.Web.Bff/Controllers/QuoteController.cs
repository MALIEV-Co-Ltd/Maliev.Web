using Asp.Versioning;
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
    IQuoteUploadService uploadService) : ControllerBase
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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> InitiateUpload([FromBody] WebUploadInitiationRequest request, CancellationToken cancellationToken)
    {
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
