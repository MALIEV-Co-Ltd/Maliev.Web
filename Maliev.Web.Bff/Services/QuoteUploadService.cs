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

        if (!string.Equals(upload.UploadId, uploadId, StringComparison.Ordinal) ||
            !string.Equals(upload.ServiceId, "WebBff", StringComparison.Ordinal))
        {
            return new WebAnalysisStatusResponse
            {
                UploadId = uploadId,
                Status = "NotFound",
                IsTerminal = true,
                Message = "We could not find upload details for this file."
            };
        }

        return new WebAnalysisStatusResponse
        {
            UploadId = upload.UploadId,
            Status = "Uploaded",
            IsTerminal = false,
            Message = "File upload is complete. Analysis results will appear when processing finishes.",
            AuthoritativeFileId = upload.FileId,
            CanonicalStoragePath = upload.StoragePath,
            CanonicalFileSizeBytes = upload.FileSize
        };
    }

    public async Task<WebUploadHandoffFileDto?> ResolveCompletedHandoffFileAsync(
        Guid quoteSessionId,
        WebUploadHandoffFileDto claimedFile,
        CancellationToken cancellationToken)
    {
        if (quoteSessionId == Guid.Empty || string.IsNullOrWhiteSpace(claimedFile.UploadId))
        {
            return null;
        }

        var uploadId = claimedFile.UploadId.Trim();
        var upload = await uploadClient.GetFileAsync(uploadId, cancellationToken);
        if (upload is null ||
            !string.Equals(upload.UploadId, uploadId, StringComparison.Ordinal) ||
            !string.Equals(upload.ServiceId, "WebBff", StringComparison.Ordinal))
        {
            return null;
        }

        var canonicalPath = upload.StoragePath.Replace('\\', '/');
        var expectedPrefix = $"quotes/temp/{quoteSessionId:N}/";
        var canonicalName = Path.GetFileName(canonicalPath);
        var claimedName = Path.GetFileName(claimedFile.FileName.Replace('\\', '/'))
            .Replace(" ", "-", StringComparison.Ordinal);
        if (!string.Equals(upload.StoragePath, canonicalPath, StringComparison.Ordinal) ||
            !canonicalPath.StartsWith(expectedPrefix, StringComparison.Ordinal) ||
            !WebQuoteUploadConstraints.IsSupportedFileName(canonicalName) ||
            upload.FileSize is <= 0 or > WebQuoteUploadConstraints.MaxFileSizeBytes ||
            string.IsNullOrWhiteSpace(upload.ContentType) ||
            !Guid.TryParse(upload.FileId, out var fileId) ||
            !string.Equals(claimedFile.UploadId, uploadId, StringComparison.Ordinal) ||
            !string.Equals(claimedName, canonicalName, StringComparison.Ordinal) ||
            !string.Equals(claimedFile.StoragePath, canonicalPath, StringComparison.Ordinal) ||
            !string.Equals(claimedFile.ContentType, upload.ContentType, StringComparison.OrdinalIgnoreCase) ||
            claimedFile.FileSizeBytes != upload.FileSize ||
            !string.Equals(claimedFile.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return new WebUploadHandoffFileDto
        {
            UploadId = upload.UploadId,
            FileId = fileId,
            FileName = canonicalName,
            StoragePath = canonicalPath,
            ContentType = upload.ContentType,
            FileSizeBytes = upload.FileSize,
            Status = "Completed"
        };
    }

    private static WebUploadCompleteResponse MapUpload(UploadResponse upload)
    {
        return new WebUploadCompleteResponse
        {
            UploadId = upload.UploadId,
            FileId = Guid.TryParse(upload.FileId, out var fileId)
                ? fileId
                : Guid.TryParse(upload.UploadId, out var uploadFileId)
                    ? uploadFileId
                    : null,
            FileName = upload.FileName,
            StoragePath = upload.StoragePath,
            Status = upload.Status
        };
    }
}
