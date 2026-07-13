using System.Net;
using System.Text;
using Maliev.Web.Bff.Clients;
using Microsoft.Extensions.Logging.Abstractions;

namespace Maliev.Web.Tests;

/// <summary>Wire-contract tests for UploadService metadata reads.</summary>
public sealed class UploadServiceClientContractTests
{
    /// <summary>The real metadata response has no completion status or file-name property.</summary>
    [Fact]
    public async Task GetFileAsync_RealMetadataJson_MapsCanonicalCompletionProof()
    {
        const string json = """
            {
              "fileId": "6a462246-f720-4a30-a95d-d4cd65ecb848",
              "uploadId": "upload-123",
              "serviceId": "WebBff",
              "storagePath": "quotes\\temp\\session\\420000\\fixture.step",
              "versionETag": "v1",
              "fileSize": 420000,
              "contentType": "application/step",
              "checksum": "sha256"
            }
            """;
        using var httpClient = new HttpClient(new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        })))
        {
            BaseAddress = new Uri("http://upload-service")
        };
        var client = new UploadServiceClient(
            httpClient,
            new SingleClientFactory(httpClient),
            NullLogger<UploadServiceClient>.Instance);

        var result = await client.GetFileAsync("upload-123", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("quotes/temp/session/420000/fixture.step", result.StoragePath);
        Assert.Equal("WebBff", result.ServiceId);
        Assert.Equal(420_000, result.FileSize);
    }

    /// <summary>Caller cancellation is never translated into dependency unavailability.</summary>
    [Fact]
    public async Task GetFileAsync_CallerCancellation_PropagatesOperationCanceledException()
    {
        using var httpClient = new HttpClient(new StubHandler(async (_, token) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            throw new InvalidOperationException("unreachable");
        }))
        {
            BaseAddress = new Uri("http://upload-service")
        };
        var client = new UploadServiceClient(
            httpClient,
            new SingleClientFactory(httpClient),
            NullLogger<UploadServiceClient>.Instance);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.GetFileAsync("upload-123", cancellation.Token));
    }

    private sealed class StubHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => respond(request, cancellationToken);
    }

    private sealed class SingleClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }
}
