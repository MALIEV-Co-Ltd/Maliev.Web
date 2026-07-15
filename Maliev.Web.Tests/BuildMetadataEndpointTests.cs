using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Maliev.Web.Tests;

/// <summary>
/// Contract tests for the public, non-secret Web build metadata endpoint.
/// </summary>
public sealed class BuildMetadataEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>
    /// Initializes a Web host with deterministic non-secret build metadata.
    /// </summary>
    public BuildMetadataEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder
            .UseEnvironment("Testing")
            .ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["BuildMetadata:Version"] = "1.4.0-rc.2",
                    ["BuildMetadata:CommitSha"] = "0123456789abcdef0123456789abcdef01234567",
                    ["BuildMetadata:ImageDigest"] = "sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"
                })));
    }

    /// <summary>
    /// Verifies anonymous callers receive only the three approved metadata fields.
    /// </summary>
    [Fact]
    public async Task GET_WebVersion_ReturnsOnlyConfiguredBuildMetadata()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/web/version");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var properties = document.RootElement.EnumerateObject().ToArray();
        Assert.Equal(["version", "commitSha", "imageDigest"], properties.Select(property => property.Name));
        Assert.Equal("1.4.0-rc.2", document.RootElement.GetProperty("version").GetString());
        Assert.Equal(
            "0123456789abcdef0123456789abcdef01234567",
            document.RootElement.GetProperty("commitSha").GetString());
        Assert.Equal(
            "sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
            document.RootElement.GetProperty("imageDigest").GetString());
    }

    /// <summary>
    /// Verifies a missing runtime digest is represented as null instead of fabricated metadata.
    /// </summary>
    [Fact]
    public async Task GET_WebVersion_WithoutRuntimeDigest_ReturnsNullDigest()
    {
        using var factory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["BuildMetadata:ImageDigest"] = string.Empty
                })));
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/web/version");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("imageDigest").ValueKind);
    }
}
