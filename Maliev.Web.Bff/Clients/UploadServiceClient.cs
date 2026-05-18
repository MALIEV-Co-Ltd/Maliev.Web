using System.Net.Http.Headers;
using System.Net;
using System.Text.Json;
using Maliev.Web.Bff.Services;

namespace Maliev.Web.Bff.Clients;

internal interface IUploadServiceClient
{
    Task<UploadInitiationResponse> InitiateResumableUploadAsync(UploadInitiationRequest request, CancellationToken cancellationToken);

    Task<UploadResponse> CompleteResumableUploadAsync(string uploadId, CancellationToken cancellationToken);

    Task<HttpResponseMessage> ResumeResumableUploadAsync(string uploadId, Stream content, string? contentType, long? contentLength, string contentRange, CancellationToken cancellationToken);

    Task<UploadResponse?> GetFileAsync(string uploadId, CancellationToken cancellationToken);
}

internal sealed class UploadServiceClient(HttpClient httpClient, IHttpClientFactory httpClientFactory, ILogger<UploadServiceClient> logger) : IUploadServiceClient
{
    public async Task<UploadInitiationResponse> InitiateResumableUploadAsync(UploadInitiationRequest request, CancellationToken cancellationToken)
    {
        return await SendJsonAsync<UploadInitiationRequest, UploadInitiationResponse>("/upload/v1/uploads/resumable", request, "initiate upload", cancellationToken);
    }

    public async Task<UploadResponse> CompleteResumableUploadAsync(string uploadId, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync($"/upload/v1/uploads/resumable/{Uri.EscapeDataString(uploadId)}/complete", new { }, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new BackendUnavailableException("UploadService", $"UploadService returned {(int)response.StatusCode} while completing upload.");
        }

        return await response.Content.ReadFromJsonAsync<UploadResponse>(cancellationToken)
            ?? throw new BackendUnavailableException("UploadService", "UploadService returned an empty upload completion response.");
    }

    public async Task<HttpResponseMessage> ResumeResumableUploadAsync(string uploadId, Stream content, string? contentType, long? contentLength, string contentRange, CancellationToken cancellationToken)
    {
        var streamingClient = httpClientFactory.CreateClient("UploadServiceStreaming");
        var request = new HttpRequestMessage(HttpMethod.Put, $"/upload/v1/uploads/resumable/{Uri.EscapeDataString(uploadId)}");
        var uploadContent = new StreamContent(content);
        if (!string.IsNullOrWhiteSpace(contentType) && MediaTypeHeaderValue.TryParse(contentType, out var mediaTypeHeader))
        {
            uploadContent.Headers.ContentType = mediaTypeHeader;
        }

        if (contentLength.HasValue)
        {
            uploadContent.Headers.ContentLength = contentLength.Value;
        }

        uploadContent.Headers.ContentRange = ContentRangeHeaderValue.Parse(contentRange);
        request.Content = uploadContent;
        return await streamingClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }

    public async Task<UploadResponse?> GetFileAsync(string uploadId, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync($"/upload/v1/files/{Uri.EscapeDataString(uploadId)}", cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new BackendUnavailableException("UploadService", $"UploadService returned {(int)response.StatusCode} while loading upload metadata.");
            }

            return await response.Content.ReadFromJsonAsync<UploadResponse>(cancellationToken);
        }
        catch (BackendUnavailableException)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or InvalidOperationException)
        {
            logger.LogWarning(ex, "UploadService failed while loading upload {UploadId}", uploadId);
            throw new BackendUnavailableException("UploadService", "UploadService is unavailable while loading upload metadata.", ex);
        }
    }

    private async Task<TResponse> SendJsonAsync<TRequest, TResponse>(string path, TRequest request, string action, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(path, request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new BackendUnavailableException("UploadService", $"UploadService returned {(int)response.StatusCode} while attempting to {action}.");
            }

            return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken)
                ?? throw new BackendUnavailableException("UploadService", $"UploadService returned an empty response while attempting to {action}.");
        }
        catch (BackendUnavailableException)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or InvalidOperationException)
        {
            logger.LogWarning(ex, "UploadService failed while attempting to {Action}", action);
            throw new BackendUnavailableException("UploadService", $"UploadService is unavailable while attempting to {action}.", ex);
        }
    }
}

internal sealed record UploadInitiationRequest(
    string Path,
    string FileName,
    string ServiceName,
    string ContentType,
    long TotalSize,
    bool Overwrite,
    string? Metadata,
    string? RetentionPolicyId = null);

internal sealed record UploadInitiationResponse(string UploadId, string SessionUri, DateTime ExpiresAt, long TotalSize);

internal sealed record UploadResponse
{
    public string UploadId { get; init; } = string.Empty;

    public string FileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public long FileSize { get; init; }

    public string StoragePath { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;
}
