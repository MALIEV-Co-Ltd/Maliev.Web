using System.Text.Json;
using Maliev.MessagingContracts.Contracts.Geometry;
using Maliev.MessagingContracts.Contracts.Shared;
using Maliev.Web.Bff.Geometry;

namespace Maliev.Web.Tests;

/// <summary>Contract-mapping regressions for generated GeometryService events.</summary>
public sealed class GeometryAnalysisConsumerTests
{
    /// <summary>The early metrics event supplies every pricing-relevant geometry field.</summary>
    [Fact]
    public async Task ConsumeAsync_FileMetricsReady_StoresFullPricingMetrics()
    {
        var store = new RecordingGeometryStore();
        var consumer = new GeometryAnalysisConsumer(store);
        var messageId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.Parse("2026-07-13T01:02:03Z");

        await consumer.ConsumeAsync(
            new FileMetricsReadyEvent(
                messageId, "FileMetricsReadyEvent", MessageType.Event, "1.0", "GeometryService", ["WebBff"],
                Guid.NewGuid(), null, occurredAt, false,
                new FileMetricsReadyEventPayload(
                    "file-123", "quotes/temp/session/part.step",
                    new FileMetricsReadyEventPayloadMetrics(
                        12.5, 1.25, 42.5,
                        new FileMetricsReadyEventPayloadMetricsBoundingBox(10, 20, 30),
                        false, 456, 0, "open mesh", 3),
                    occurredAt, 1, [])),
            CancellationToken.None);

        var snapshot = Assert.Single(store.Writes);
        Assert.Equal(GeometryAnalysisState.Ready, snapshot.State);
        Assert.Equal(messageId, snapshot.EventId);
        Assert.Equal(12.5, snapshot.Metrics?.VolumeCm3);
        Assert.Equal(1.25, snapshot.Metrics?.SupportVolumeCm3);
        Assert.Equal(42.5, snapshot.Metrics?.SurfaceAreaCm2);
        Assert.Equal(10, snapshot.Metrics?.BoundingBoxX);
        Assert.Equal(20, snapshot.Metrics?.BoundingBoxY);
        Assert.Equal(30, snapshot.Metrics?.BoundingBoxZ);
        Assert.False(snapshot.Metrics?.IsManifold);
        Assert.Equal(456, snapshot.Metrics?.TriangleCount);
    }

    /// <summary>The completion event remains a compatible fallback for metrics.</summary>
    [Fact]
    public async Task ConsumeAsync_FileAnalyzed_StoresFallbackMetrics()
    {
        var store = new RecordingGeometryStore();
        var consumer = new GeometryAnalysisConsumer(store);
        var occurredAt = DateTimeOffset.Parse("2026-07-13T01:02:03Z");

        await consumer.ConsumeAsync(
            new FileAnalyzedEvent(
                Guid.NewGuid(), "FileAnalyzedEvent", MessageType.Event, "1.0", "GeometryService", ["WebBff"],
                Guid.NewGuid(), null, occurredAt, false,
                new FileAnalyzedEventPayload(
                    "file-123", Guid.Empty,
                    new FileAnalyzedEventPayloadMetrics(
                        8, 0.5, 22,
                        new FileAnalyzedEventPayloadMetricsBoundingBox(4, 5, 6),
                        true, 120, 0, null, null),
                    null, null, null, null, "quotes/temp/session/part.step", occurredAt,
                    new object(), Guid.Empty, "", Guid.Empty, "", 1, [])),
            CancellationToken.None);

        var snapshot = Assert.Single(store.Writes);
        Assert.Equal(8, snapshot.Metrics?.VolumeCm3);
        Assert.True(snapshot.Metrics?.IsManifold);
    }

    /// <summary>Failure events retain only a safe failure code and ownership keys.</summary>
    [Fact]
    public async Task ConsumeAsync_FileAnalysisFailed_StoresFailureWithoutProviderDetails()
    {
        var store = new RecordingGeometryStore();
        var consumer = new GeometryAnalysisConsumer(store);
        var occurredAt = DateTimeOffset.Parse("2026-07-13T01:02:03Z");

        await consumer.ConsumeAsync(
            new FileAnalysisFailedEvent(
                Guid.NewGuid(), "FileAnalysisFailedEvent", MessageType.Event, "1.0", "GeometryService", ["WebBff"],
                Guid.NewGuid(), null, occurredAt, false,
                new FileAnalysisFailedEventPayload(
                    "file-123", "quotes/temp/session/part.step", "invalid_mesh", "secret provider trace")),
            CancellationToken.None);

        var snapshot = Assert.Single(store.Writes);
        Assert.Equal(GeometryAnalysisState.Failed, snapshot.State);
        Assert.Equal("invalid_mesh", snapshot.FailureCode);
        Assert.Null(snapshot.Metrics);
        Assert.DoesNotContain("secret", snapshot.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>The generated contract deserializes the runtime JSON wire shape consumed by Web.</summary>
    [Fact]
    public async Task FileMetricsReadyEvent_RuntimeJson_DeserializesAndMaps()
    {
        var messageId = Guid.NewGuid();
        var json = $$"""
            {
              "messageId": "{{messageId}}",
              "messageName": "FileMetricsReadyEvent",
              "messageType": "Event",
              "messageVersion": "1.0",
              "publishedBy": "GeometryService",
              "consumedBy": ["WebBff"],
              "correlationId": "{{Guid.NewGuid()}}",
              "causationId": null,
              "occurredAtUtc": "2026-07-13T01:02:03Z",
              "isPublic": false,
              "payload": {
                "fileId": "file-wire-123",
                "storagePath": "quotes/temp/session/part.step",
                "metrics": {
                  "volumeCm3": 12.5,
                  "supportVolumeCm3": 1.25,
                  "surfaceAreaCm2": 42.5,
                  "boundingBox": { "x": 10, "y": 20, "z": 30 },
                  "isManifold": true,
                  "triangleCount": 456,
                  "eulerNumber": 2,
                  "nonManifoldReason": null,
                  "nonManifoldFaceCount": null
                },
                "processedAt": "2026-07-13T01:02:03Z",
                "bodyCount": 1,
                "bodies": []
              }
            }
            """;
        var message = JsonSerializer.Deserialize<FileMetricsReadyEvent>(json);
        var store = new RecordingGeometryStore();

        await new GeometryAnalysisConsumer(store).ConsumeAsync(
            Assert.IsType<FileMetricsReadyEvent>(message),
            CancellationToken.None);

        var snapshot = Assert.Single(store.Writes);
        Assert.Equal(messageId, snapshot.EventId);
        Assert.Equal("file-wire-123", snapshot.FileId);
        Assert.Equal(12.5, snapshot.Metrics?.VolumeCm3);
    }

    private sealed class RecordingGeometryStore : IGeometryAnalysisStore
    {
        public List<GeometryAnalysisSnapshot> Writes { get; } = [];

        public ValueTask<GeometryAnalysisSnapshot?> GetAsync(
            string fileId,
            string canonicalStoragePath,
            CancellationToken cancellationToken) =>
            ValueTask.FromResult<GeometryAnalysisSnapshot?>(null);

        public ValueTask SetAsync(GeometryAnalysisSnapshot snapshot, CancellationToken cancellationToken)
        {
            Writes.Add(snapshot);
            return ValueTask.CompletedTask;
        }
    }
}
