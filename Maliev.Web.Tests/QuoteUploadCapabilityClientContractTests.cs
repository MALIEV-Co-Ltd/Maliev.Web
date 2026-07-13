namespace Maliev.Web.Tests;

/// <summary>
/// Source-level checks for both browser upload implementations that consume the signed capability contract.
/// </summary>
public sealed class QuoteUploadCapabilityClientContractTests
{
    /// <summary>Verifies the landing-page JavaScript forwards proof to every capability-protected operation.</summary>
    [Fact]
    public void LandingDropzone_ForwardsUploadCapabilityAcrossUploadLifecycle()
    {
        var script = ReadRepositoryFile("Maliev.Web.Bff", "wwwroot", "js", "maliev-quote-dropzone.js");

        Assert.Contains("X-Maliev-Upload-Capability", script, StringComparison.Ordinal);
        Assert.Contains("session.uploadCapability", script, StringComparison.Ordinal);
        Assert.Contains("uploadCapability: session.uploadCapability", script, StringComparison.Ordinal);
    }

    /// <summary>Verifies the Blazor API client forwards proof for raw upload and analysis status polling.</summary>
    [Fact]
    public void MalievApiClient_ForwardsUploadCapabilityAcrossUploadLifecycle()
    {
        var source = ReadRepositoryFile("Maliev.Web.Client", "Services", "MalievApiClient.cs");

        Assert.Contains("X-Maliev-Upload-Capability", source, StringComparison.Ordinal);
        Assert.Contains("session.UploadCapability", source, StringComparison.Ordinal);
        Assert.Contains("GetAnalysisStatusAsync(", source, StringComparison.Ordinal);
        Assert.Contains("string uploadCapability", source, StringComparison.Ordinal);
    }

    /// <summary>Verifies the quote component preserves the capability until protected status polling finishes.</summary>
    [Fact]
    public void InstantQuotePanel_UsesInitiationCapabilityForStatusPolling()
    {
        var source = ReadRepositoryFile(
            "Maliev.Web.Client",
            "Components",
            "Quote",
            "InstantQuotePanel.razor");

        Assert.Contains("GetAnalysisStatusAsync(upload.UploadId, session.UploadCapability)", source, StringComparison.Ordinal);
    }

    private static string ReadRepositoryFile(params string[] segments)
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        return File.ReadAllText(Path.Combine([root, .. segments]));
    }
}
