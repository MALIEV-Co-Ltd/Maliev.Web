using Maliev.Web.Bff.Geometry;

namespace Maliev.Web.Tests;

/// <summary>
/// Regression tests for authoritative geometry state retained by the Web BFF.
/// </summary>
public sealed class GeometryAnalysisStoreTests
{
    /// <summary>Matching canonical paths resolve the newest event by authoritative file id.</summary>
    [Fact]
    public async Task SetAsync_MatchingPath_ReturnsAuthoritativeMetrics()
    {
        var time = new ManualTimeProvider(DateTimeOffset.Parse("2026-07-13T00:00:00Z"));
        var store = new InMemoryGeometryAnalysisStore(time, capacity: 8);
        var snapshot = GeometryAnalysisSnapshot.Ready(
            "file-123",
            "quotes/temp/session/part.step",
            new GeometryPricingMetrics(12.5, 1.25, 42, 10, 20, 30, false, 456),
            time.GetUtcNow(),
            Guid.NewGuid());

        await store.SetAsync(snapshot, CancellationToken.None);
        var result = await store.GetAsync(
            "file-123",
            "quotes\\temp\\session\\part.step",
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(12.5, result.Metrics?.VolumeCm3);
        Assert.False(result.Metrics?.IsManifold);
    }

    /// <summary>A different canonical path cannot resolve geometry for a reused file id.</summary>
    [Fact]
    public async Task GetAsync_MismatchedPath_ReturnsNull()
    {
        var time = new ManualTimeProvider(DateTimeOffset.Parse("2026-07-13T00:00:00Z"));
        var store = new InMemoryGeometryAnalysisStore(time, capacity: 8);
        await store.SetAsync(
            GeometryAnalysisSnapshot.Failed(
                "file-123",
                "quotes/temp/owner/part.step",
                "analysis_failed",
                time.GetUtcNow(),
                Guid.NewGuid()),
            CancellationToken.None);

        var result = await store.GetAsync(
            "file-123",
            "quotes/temp/other/part.step",
            CancellationToken.None);

        Assert.Null(result);
    }

    /// <summary>Expired analysis state is not returned after the two-hour ownership window.</summary>
    [Fact]
    public async Task GetAsync_AfterTwoHours_ReturnsNull()
    {
        var time = new ManualTimeProvider(DateTimeOffset.Parse("2026-07-13T00:00:00Z"));
        var store = new InMemoryGeometryAnalysisStore(time, capacity: 8);
        await store.SetAsync(
            GeometryAnalysisSnapshot.Failed(
                "file-123",
                "quotes/temp/owner/part.step",
                "analysis_failed",
                time.GetUtcNow(),
                Guid.NewGuid()),
            CancellationToken.None);

        time.Advance(TimeSpan.FromHours(2) + TimeSpan.FromSeconds(1));

        Assert.Null(await store.GetAsync(
            "file-123",
            "quotes/temp/owner/part.step",
            CancellationToken.None));
    }

    /// <summary>An older event cannot overwrite newer authoritative state.</summary>
    [Fact]
    public async Task SetAsync_OlderEvent_DoesNotOverwriteNewerState()
    {
        var time = new ManualTimeProvider(DateTimeOffset.Parse("2026-07-13T00:00:00Z"));
        var store = new InMemoryGeometryAnalysisStore(time, capacity: 8);
        var newer = GeometryAnalysisSnapshot.Ready(
            "file-123",
            "quotes/temp/owner/part.step",
            new GeometryPricingMetrics(20, 0, 30, 1, 2, 3, true, 100),
            time.GetUtcNow(),
            Guid.NewGuid());
        var older = GeometryAnalysisSnapshot.Failed(
            "file-123",
            "quotes/temp/owner/part.step",
            "stale_failure",
            time.GetUtcNow() - TimeSpan.FromMinutes(1),
            Guid.NewGuid());

        await store.SetAsync(newer, CancellationToken.None);
        await store.SetAsync(older, CancellationToken.None);

        var result = await store.GetAsync(
            "file-123",
            "quotes/temp/owner/part.step",
            CancellationToken.None);
        Assert.Equal(GeometryAnalysisState.Ready, result?.State);
        Assert.Equal(20, result?.Metrics?.VolumeCm3);
    }

    /// <summary>Caller cancellation is observed before any store lookup.</summary>
    [Fact]
    public async Task GetAsync_CallerCancellation_Propagates()
    {
        var store = new InMemoryGeometryAnalysisStore(TimeProvider.System, capacity: 8);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await store.GetAsync("file-123", "quotes/temp/part.step", cancellation.Token));
    }

    private sealed class ManualTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan duration) => _utcNow += duration;
    }
}
