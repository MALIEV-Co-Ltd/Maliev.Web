using Maliev.MessagingContracts.Contracts.Geometry;
using MassTransit;

namespace Maliev.Web.Bff.Geometry;

internal sealed class GeometryAnalysisConsumer(IGeometryAnalysisStore store) :
    IConsumer<FileMetricsReadyEvent>,
    IConsumer<FileAnalyzedEvent>,
    IConsumer<FileAnalysisFailedEvent>
{
    public Task Consume(ConsumeContext<FileMetricsReadyEvent> context) =>
        ConsumeAsync(context.Message, context.CancellationToken).AsTask();

    public Task Consume(ConsumeContext<FileAnalyzedEvent> context) =>
        ConsumeAsync(context.Message, context.CancellationToken).AsTask();

    public Task Consume(ConsumeContext<FileAnalysisFailedEvent> context) =>
        ConsumeAsync(context.Message, context.CancellationToken).AsTask();

    internal ValueTask ConsumeAsync(FileMetricsReadyEvent message, CancellationToken cancellationToken)
    {
        var payload = message.Payload;
        var metrics = payload.Metrics;
        return store.SetAsync(
            GeometryAnalysisSnapshot.Ready(
                payload.FileId,
                payload.StoragePath,
                new GeometryPricingMetrics(
                    metrics.VolumeCm3,
                    metrics.SupportVolumeCm3,
                    metrics.SurfaceAreaCm2,
                    metrics.BoundingBox.X,
                    metrics.BoundingBox.Y,
                    metrics.BoundingBox.Z,
                    metrics.IsManifold,
                    metrics.TriangleCount),
                payload.ProcessedAt == default ? message.OccurredAtUtc : payload.ProcessedAt,
                message.MessageId),
            cancellationToken);
    }

    internal ValueTask ConsumeAsync(FileAnalyzedEvent message, CancellationToken cancellationToken)
    {
        var payload = message.Payload;
        var metrics = payload.Metrics;
        return store.SetAsync(
            GeometryAnalysisSnapshot.Ready(
                payload.FileId,
                payload.StoragePath,
                new GeometryPricingMetrics(
                    metrics.VolumeCm3,
                    metrics.SupportVolumeCm3,
                    metrics.SurfaceAreaCm2,
                    metrics.BoundingBox.X,
                    metrics.BoundingBox.Y,
                    metrics.BoundingBox.Z,
                    metrics.IsManifold,
                    metrics.TriangleCount),
                payload.ProcessedAt == default ? message.OccurredAtUtc : payload.ProcessedAt,
                message.MessageId),
            cancellationToken);
    }

    internal ValueTask ConsumeAsync(FileAnalysisFailedEvent message, CancellationToken cancellationToken)
    {
        var payload = message.Payload;
        return store.SetAsync(
            GeometryAnalysisSnapshot.Failed(
                payload.FileId,
                payload.StoragePath,
                SanitizeFailureCode(payload.ErrorCode),
                message.OccurredAtUtc,
                message.MessageId),
            cancellationToken);
    }

    private static string SanitizeFailureCode(string failureCode)
    {
        var value = new string(failureCode
            .Where(character => char.IsAsciiLetterOrDigit(character) || character is '_' or '-')
            .Take(64)
            .ToArray());
        return string.IsNullOrWhiteSpace(value) ? "analysis_failed" : value;
    }
}
