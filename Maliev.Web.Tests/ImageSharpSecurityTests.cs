using System.Net;
using System.Xml.Linq;
using Maliev.Web.Bff.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Maliev.Web.Tests;

/// <summary>
/// Security regression tests for the ImageSharp dependency and Web image decoding boundary.
/// </summary>
public sealed class ImageSharpSecurityTests
{
    private static readonly byte[] MalformedAdvisoryGif =
    [
        0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x7C, 0x00, 0x41, 0x00,
        0xF0, 0x74, 0x09, 0xFF, 0xBE, 0xBD, 0xAF, 0xD7, 0xAB, 0x21,
        0x01, 0x0C, 0x27, 0x00, 0x34, 0x00, 0x3E, 0x00, 0x38, 0x00,
        0x9A, 0x23, 0x85, 0x87, 0x00, 0x21, 0xFE, 0x56, 0x00, 0x3B
    ];

    /// <summary>
    /// Verifies Web uses the latest patched 3.x ImageSharp release without suppressing its advisory.
    /// </summary>
    [Fact]
    public void BffProject_UsesPatchedImageSharpWithoutAuditSuppression()
    {
        var repositoryRoot = FindRepositoryRoot();
        var bffProject = XDocument.Load(Path.Combine(repositoryRoot, "Maliev.Web.Bff", "Maliev.Web.Bff.csproj"));
        var testProject = XDocument.Load(Path.Combine(repositoryRoot, "Maliev.Web.Tests", "Maliev.Web.Tests.csproj"));

        var imageSharpReference = Assert.Single(
            bffProject.Descendants("PackageReference"),
            reference => string.Equals(
                    reference.Attribute("Include")?.Value,
                    "SixLabors.ImageSharp",
                    StringComparison.Ordinal));

        Assert.Equal("3.1.12", imageSharpReference.Attribute("Version")?.Value);
        Assert.DoesNotContain(
            bffProject.Descendants("NuGetAuditSuppress"),
            suppression => string.Equals(
                suppression.Attribute("Include")?.Value,
                "https://github.com/advisories/GHSA-rxmq-m78w-7wmc",
                StringComparison.Ordinal));
        Assert.DoesNotContain(
            testProject.Descendants("NuGetAuditSuppress"),
            suppression => string.Equals(
                suppression.Attribute("Include")?.Value,
                "https://github.com/advisories/GHSA-rxmq-m78w-7wmc",
                StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies the exact malformed GIF from GHSA-rxmq-m78w-7wmc is rejected by Web's tile decoder.
    /// </summary>
    [Fact]
    public async Task RenderAsync_MalformedAdvisoryGif_RejectsWithoutHanging()
    {
        using var httpClientFactory = new StaticResponseHttpClientFactory(MalformedAdvisoryGif);
        var service = new StaticMapService(httpClientFactory);

        await Assert.ThrowsAsync<InvalidImageContentException>(
            () => service
                .RenderAsync(0, 0, 1, 1, 1, CancellationToken.None)
                .WaitAsync(TimeSpan.FromSeconds(2)));
    }

    /// <summary>
    /// Verifies supported PNG tiles still compose into the requested PNG output dimensions.
    /// </summary>
    [Fact]
    public async Task RenderAsync_SupportedPng_ComposesRequestedOutput()
    {
        byte[] tileBytes;
        using (var tile = new Image<Rgba32>(1, 1, new Rgba32(0x30, 0x7D, 0xB8)))
        using (var stream = new MemoryStream())
        {
            tile.Save(stream, new PngEncoder());
            tileBytes = stream.ToArray();
        }

        using var httpClientFactory = new StaticResponseHttpClientFactory(tileBytes);
        var service = new StaticMapService(httpClientFactory);

        var output = await service.RenderAsync(0, 0, 1, 12, 8, CancellationToken.None);

        using var rendered = Image.Load<Rgba32>(output);
        Assert.Equal(12, rendered.Width);
        Assert.Equal(8, rendered.Height);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Maliev.Web.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the Maliev.Web repository root.");
    }

    private sealed class StaticResponseHttpClientFactory(byte[] payload) : IHttpClientFactory, IDisposable
    {
        private readonly HttpClient _httpClient = new(new StaticResponseHandler(payload));

        public HttpClient CreateClient(string name) => _httpClient;

        public void Dispose() => _httpClient.Dispose();
    }

    private sealed class StaticResponseHandler(byte[] payload) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(payload)
            });
        }
    }
}
