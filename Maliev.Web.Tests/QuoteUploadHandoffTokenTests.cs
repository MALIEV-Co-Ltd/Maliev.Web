using Maliev.Web.Bff.Security;
using Maliev.Web.Shared.Quotes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Maliev.Web.Tests;

/// <summary>
/// Regression tests for the Web-to-QuoteEngine upload-handoff signing boundary.
/// </summary>
public sealed class QuoteUploadHandoffTokenTests
{
    /// <summary>
    /// Verifies non-local deployments cannot sign upload handoffs with the known compatibility key.
    /// </summary>
    [Fact]
    public void Create_StagingWithoutSigningKey_ThrowsConfigurationError()
    {
        var issuer = CreateIssuer(Environments.Staging);

        var exception = Assert.Throws<InvalidOperationException>(() => issuer.Create(CreateRequest()));

        Assert.Contains("QuoteUploadHandoff:SigningKey", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies the local compatibility key remains available in Development and Testing only.
    /// </summary>
    /// <param name="environmentName">The explicitly allowed local environment.</param>
    [Theory]
    [InlineData("Development")]
    [InlineData("Testing")]
    public void Create_LocalEnvironmentWithoutSigningKey_ReturnsSignedToken(string environmentName)
    {
        var issuer = CreateIssuer(environmentName);

        var token = issuer.Create(CreateRequest());

        Assert.Equal(2, token.Split('.').Length);
    }

    /// <summary>
    /// Verifies Staging continues to sign handoffs when its deployment secret is configured.
    /// </summary>
    [Fact]
    public void Create_StagingWithSigningKey_ReturnsSignedToken()
    {
        var issuer = CreateIssuer(Environments.Staging, "staging-test-key-with-enough-entropy");

        var token = issuer.Create(CreateRequest());

        Assert.Equal(2, token.Split('.').Length);
    }

    private static QuoteUploadHandoffToken CreateIssuer(string environmentName, string? signingKey = null)
    {
        var values = new Dictionary<string, string?>();
        if (signingKey is not null)
        {
            values["QuoteUploadHandoff:SigningKey"] = signingKey;
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
        return new QuoteUploadHandoffToken(configuration, new FakeHostEnvironment(environmentName));
    }

    private static WebUploadHandoffTokenRequest CreateRequest()
    {
        return new WebUploadHandoffTokenRequest
        {
            QuoteSessionId = Guid.NewGuid(),
            Files =
            [
                new WebUploadHandoffFileDto
                {
                    UploadId = "upload-1",
                    FileName = "fixture.step",
                    StoragePath = "quotes/temp/fixture.step",
                    ContentType = "model/step",
                    FileSizeBytes = 128
                }
            ]
        };
    }

    private sealed class FakeHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = "Maliev.Web.Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
