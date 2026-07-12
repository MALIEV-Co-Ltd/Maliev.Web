using Maliev.Web.Shared.Quotes;

namespace Maliev.Web.Bff.Services;

/// <summary>
/// Proxies customer quote uploads to UploadService.
/// </summary>
public interface IQuoteUploadService
{
    /// <summary>Initiates an UploadService resumable upload session.</summary>
    Task<WebUploadInitiationResponse> InitiateAsync(WebUploadInitiationRequest request, CancellationToken cancellationToken);

    /// <summary>Completes an UploadService resumable upload session.</summary>
    Task<WebUploadCompleteResponse> CompleteAsync(string uploadId, CancellationToken cancellationToken);

    /// <summary>Streams a resumable upload body to UploadService without retrying the request body.</summary>
    Task<HttpResponseMessage> ResumeAsync(string uploadId, Stream content, string? contentType, long? contentLength, string contentRange, CancellationToken cancellationToken);

    /// <summary>Gets the current upload-backed analysis status for polling clients.</summary>
    Task<WebAnalysisStatusResponse> GetAnalysisStatusAsync(string uploadId, CancellationToken cancellationToken);

    /// <summary>Resolves and validates one completed Web upload from authoritative UploadService metadata.</summary>
    Task<WebUploadHandoffFileDto?> ResolveCompletedHandoffFileAsync(
        Guid quoteSessionId,
        WebUploadHandoffFileDto claimedFile,
        CancellationToken cancellationToken);
}
