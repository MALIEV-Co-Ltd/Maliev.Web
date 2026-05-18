using Maliev.Web.Bff.Clients;
using Maliev.Web.Shared.Quotes;

namespace Maliev.Web.Bff.Services;

internal sealed class QuoteUploadService(IUploadServiceClient uploadClient) : IQuoteUploadService
{
    private const string WebTemporaryQuoteRetentionPolicyId = "quote-temp-uploads";

    public async Task<WebUploadInitiationResponse> InitiateAsync(WebUploadInitiationRequest request, CancellationToken cancellationToken)
    {
        var safeName = Path.GetFileName(request.FileName).Replace(" ", "-", StringComparison.Ordinal);
        var storagePath = $"quotes/temp/{request.QuoteSessionId:N}/{request.FileSize}/{safeName}";
        var session = await uploadClient.InitiateResumableUploadAsync(new UploadInitiationRequest(
            Path: storagePath,
            FileName: request.FileName,
            ServiceName: "WebBff",
            ContentType: request.ContentType,
            TotalSize: request.FileSize,
            Overwrite: true,
            Metadata: $$"""{"quoteSessionId":"{{request.QuoteSessionId}}","fileSize":{{request.FileSize}},"uploadScope":"temporary-quote-handoff"}""",
            RetentionPolicyId: WebTemporaryQuoteRetentionPolicyId), cancellationToken);

        return new WebUploadInitiationResponse
        {
            UploadId = session.UploadId,
            ProxyUploadUrl = $"/web/v1/quote/uploads/resumable/{session.UploadId}",
            StoragePath = storagePath
        };
    }

    public async Task<WebUploadCompleteResponse> CompleteAsync(string uploadId, CancellationToken cancellationToken)
    {
        var upload = await uploadClient.CompleteResumableUploadAsync(uploadId, cancellationToken);
        return MapUpload(upload);
    }

    public Task<HttpResponseMessage> ResumeAsync(string uploadId, Stream content, string? contentType, long? contentLength, string contentRange, CancellationToken cancellationToken)
    {
        return uploadClient.ResumeResumableUploadAsync(uploadId, content, contentType, contentLength, contentRange, cancellationToken);
    }

    public async Task<WebAnalysisStatusResponse> GetAnalysisStatusAsync(string uploadId, CancellationToken cancellationToken)
    {
        var upload = await uploadClient.GetFileAsync(uploadId, cancellationToken);
        if (upload is null)
        {
            return new WebAnalysisStatusResponse
            {
                UploadId = uploadId,
                Status = "NotFound",
                IsTerminal = true,
                Message = "We could not find upload details for this file."
            };
        }

        var completed = upload.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase);
        return new WebAnalysisStatusResponse
        {
            UploadId = upload.UploadId,
            Status = completed ? "Uploaded" : upload.Status,
            IsTerminal = false,
            Message = completed
                ? "File upload is complete. Analysis results will appear when processing finishes."
                : "This file is still uploading."
        };
    }

    private static WebUploadCompleteResponse MapUpload(UploadResponse upload)
    {
        return new WebUploadCompleteResponse
        {
            UploadId = upload.UploadId,
            FileId = Guid.TryParse(upload.UploadId, out var fileId) ? fileId : null,
            FileName = upload.FileName,
            StoragePath = upload.StoragePath,
            Status = upload.Status
        };
    }
}
