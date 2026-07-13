namespace Maliev.Web.Tests;

/// <summary>Stable topology metadata checks for the externally published geometry topics.</summary>
public sealed class GeometryMessagingTopologyTests
{
    /// <summary>MassTransit is registered before IAM and binds one durable queue to all runtime publisher keys.</summary>
    [Fact]
    public void Program_RegistersGeometryQueueBeforeIam_WithAllPublisherRoutingKeys()
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "Maliev.Web.Bff", "Program.cs"));

        var massTransitIndex = source.IndexOf("builder.Services.AddMassTransit", StringComparison.Ordinal);
        var iamIndex = source.IndexOf("builder.AddIAMServiceClient", StringComparison.Ordinal);
        Assert.True(massTransitIndex >= 0 && massTransitIndex < iamIndex);
        Assert.Equal(1, Count(source, "ReceiveEndpoint(\"web-bff-geometry-analysis-v1\""));
        Assert.Contains("maliev.geometryservice.v1.metrics.ready", source, StringComparison.Ordinal);
        Assert.Contains("maliev.geometryservice.v1.analysis.completed", source, StringComparison.Ordinal);
        Assert.Contains("maliev.geometryservice.v1.analysis.failed", source, StringComparison.Ordinal);
        Assert.Contains("UsingInMemory", source, StringComparison.Ordinal);
    }

    private static int Count(string value, string expected)
    {
        var count = 0;
        var offset = 0;
        while ((offset = value.IndexOf(expected, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += expected.Length;
        }

        return count;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.Web.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate Maliev.Web repository root.");
    }
}
