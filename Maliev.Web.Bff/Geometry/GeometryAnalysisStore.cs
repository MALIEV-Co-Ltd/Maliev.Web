using System.Collections.Concurrent;
using System.Text.Json;
using Maliev.Web.Bff.Services;
using StackExchange.Redis;

namespace Maliev.Web.Bff.Geometry;

internal enum GeometryAnalysisState
{
    Ready,
    Failed
}

internal sealed record GeometryPricingMetrics(
    double VolumeCm3,
    double SupportVolumeCm3,
    double SurfaceAreaCm2,
    double BoundingBoxX,
    double BoundingBoxY,
    double BoundingBoxZ,
    bool IsManifold,
    int TriangleCount)
{
    public bool IsUsable =>
        IsPositiveFinite(VolumeCm3) &&
        IsNonNegativeFinite(SupportVolumeCm3) &&
        IsPositiveFinite(SurfaceAreaCm2) &&
        IsPositiveFinite(BoundingBoxX) &&
        IsPositiveFinite(BoundingBoxY) &&
        IsPositiveFinite(BoundingBoxZ) &&
        TriangleCount > 0;

    private static bool IsPositiveFinite(double value) => double.IsFinite(value) && value > 0;

    private static bool IsNonNegativeFinite(double value) => double.IsFinite(value) && value >= 0;
}

internal sealed record GeometryAnalysisSnapshot(
    string FileId,
    string StoragePath,
    GeometryAnalysisState State,
    GeometryPricingMetrics? Metrics,
    string? FailureCode,
    DateTimeOffset EventTime,
    Guid EventId)
{
    public static GeometryAnalysisSnapshot Ready(
        string fileId,
        string storagePath,
        GeometryPricingMetrics metrics,
        DateTimeOffset eventTime,
        Guid eventId) =>
        new(fileId, CanonicalizePath(storagePath), GeometryAnalysisState.Ready, metrics, null, eventTime, eventId);

    public static GeometryAnalysisSnapshot Failed(
        string fileId,
        string storagePath,
        string failureCode,
        DateTimeOffset eventTime,
        Guid eventId) =>
        new(fileId, CanonicalizePath(storagePath), GeometryAnalysisState.Failed, null, failureCode, eventTime, eventId);

    internal static string CanonicalizePath(string storagePath) => storagePath.Trim().Replace('\\', '/');
}

internal interface IGeometryAnalysisStore
{
    ValueTask<GeometryAnalysisSnapshot?> GetAsync(
        string fileId,
        string canonicalStoragePath,
        CancellationToken cancellationToken);

    ValueTask SetAsync(GeometryAnalysisSnapshot snapshot, CancellationToken cancellationToken);
}

/// <summary>
/// Provides the public dependency-injection boundary for server-owned geometry state.
/// </summary>
public sealed class GeometryAnalysisAccessor
{
    private readonly IGeometryAnalysisStore _store;

    internal GeometryAnalysisAccessor(IGeometryAnalysisStore store)
    {
        _store = store;
    }

    internal ValueTask<GeometryAnalysisSnapshot?> GetAsync(
        string fileId,
        string canonicalStoragePath,
        CancellationToken cancellationToken) =>
        _store.GetAsync(fileId, canonicalStoragePath, cancellationToken);
}

internal sealed class InMemoryGeometryAnalysisStore : IGeometryAnalysisStore
{
    internal static readonly TimeSpan Retention = TimeSpan.FromHours(2);

    private readonly ConcurrentDictionary<string, Entry> _entries = new(StringComparer.Ordinal);
    private readonly TimeProvider _timeProvider;
    private readonly int _capacity;
    private readonly object _writeLock = new();

    public InMemoryGeometryAnalysisStore(TimeProvider timeProvider, int capacity = 4_096)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);
        _timeProvider = timeProvider;
        _capacity = capacity;
    }

    public ValueTask<GeometryAnalysisSnapshot?> GetAsync(
        string fileId,
        string canonicalStoragePath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_entries.TryGetValue(fileId, out var entry))
        {
            return ValueTask.FromResult<GeometryAnalysisSnapshot?>(null);
        }

        if (entry.ExpiresAt <= _timeProvider.GetUtcNow())
        {
            _entries.TryRemove(new KeyValuePair<string, Entry>(fileId, entry));
            return ValueTask.FromResult<GeometryAnalysisSnapshot?>(null);
        }

        var requestedPath = GeometryAnalysisSnapshot.CanonicalizePath(canonicalStoragePath);
        return ValueTask.FromResult(
            string.Equals(entry.Snapshot.StoragePath, requestedPath, StringComparison.Ordinal)
                ? entry.Snapshot
                : null);
    }

    public ValueTask SetAsync(GeometryAnalysisSnapshot snapshot, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(snapshot.FileId) || string.IsNullOrWhiteSpace(snapshot.StoragePath))
        {
            return ValueTask.CompletedTask;
        }

        lock (_writeLock)
        {
            var normalized = snapshot with
            {
                FileId = snapshot.FileId.Trim(),
                StoragePath = GeometryAnalysisSnapshot.CanonicalizePath(snapshot.StoragePath)
            };
            if (_entries.TryGetValue(normalized.FileId, out var current) &&
                IsNewerThan(normalized, current.Snapshot) is false)
            {
                return ValueTask.CompletedTask;
            }

            EvictExpiredAndOldestIfNeeded();
            _entries[normalized.FileId] = new Entry(normalized, _timeProvider.GetUtcNow() + Retention);
        }

        return ValueTask.CompletedTask;
    }

    private static bool IsNewerThan(GeometryAnalysisSnapshot candidate, GeometryAnalysisSnapshot current) =>
        candidate.EventTime > current.EventTime ||
        (candidate.EventTime == current.EventTime && candidate.EventId.CompareTo(current.EventId) > 0);

    private void EvictExpiredAndOldestIfNeeded()
    {
        var now = _timeProvider.GetUtcNow();
        foreach (var pair in _entries.Where(pair => pair.Value.ExpiresAt <= now))
        {
            _entries.TryRemove(pair);
        }

        if (_entries.Count < _capacity)
        {
            return;
        }

        var oldest = _entries.MinBy(pair => pair.Value.Snapshot.EventTime);
        _entries.TryRemove(oldest);
    }

    private sealed record Entry(GeometryAnalysisSnapshot Snapshot, DateTimeOffset ExpiresAt);
}

internal sealed class RedisGeometryAnalysisStore(
    IConnectionMultiplexer connectionMultiplexer) : IGeometryAnalysisStore
{
    private const string KeyPrefix = "maliev:web:geometry:v1:";
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async ValueTask<GeometryAnalysisSnapshot?> GetAsync(
        string fileId,
        string canonicalStoragePath,
        CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(fileId) || string.IsNullOrWhiteSpace(canonicalStoragePath))
            {
                return null;
            }

            var value = await connectionMultiplexer.GetDatabase().StringGetAsync(Key(fileId));
            cancellationToken.ThrowIfCancellationRequested();
            if (!value.HasValue)
            {
                return null;
            }

            var snapshot = JsonSerializer.Deserialize<GeometryAnalysisSnapshot>((string)value!, SerializerOptions);
            return snapshot is not null && string.Equals(
                snapshot.StoragePath,
                GeometryAnalysisSnapshot.CanonicalizePath(canonicalStoragePath),
                StringComparison.Ordinal)
                ? snapshot
                : null;
        }
        catch (Exception ex) when (ex is RedisException or JsonException)
        {
            throw new BackendUnavailableException(
                "Redis",
                "Authoritative geometry analysis state is temporarily unavailable.",
                ex);
        }
    }

    public async ValueTask SetAsync(GeometryAnalysisSnapshot snapshot, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(snapshot.FileId) || string.IsNullOrWhiteSpace(snapshot.StoragePath))
            {
                return;
            }

            var database = connectionMultiplexer.GetDatabase();
            var key = Key(snapshot.FileId);
            var normalized = snapshot with
            {
                FileId = snapshot.FileId.Trim(),
                StoragePath = GeometryAnalysisSnapshot.CanonicalizePath(snapshot.StoragePath)
            };
            var serialized = JsonSerializer.Serialize(normalized, SerializerOptions);
            for (var attempt = 0; attempt < 5; attempt++)
            {
                var currentValue = await database.StringGetAsync(key);
                cancellationToken.ThrowIfCancellationRequested();
                if (currentValue.HasValue)
                {
                    var current = JsonSerializer.Deserialize<GeometryAnalysisSnapshot>((string)currentValue!, SerializerOptions);
                    if (current is not null &&
                        (current.EventTime > snapshot.EventTime ||
                         (current.EventTime == snapshot.EventTime && current.EventId.CompareTo(snapshot.EventId) >= 0)))
                    {
                        return;
                    }
                }

                var transaction = database.CreateTransaction();
                transaction.AddCondition(currentValue.HasValue
                    ? Condition.StringEqual(key, currentValue)
                    : Condition.KeyNotExists(key));
                _ = transaction.StringSetAsync(key, serialized, InMemoryGeometryAnalysisStore.Retention);
                if (await transaction.ExecuteAsync())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return;
                }
            }

            throw new BackendUnavailableException(
                "Redis",
                "Authoritative geometry analysis state could not be updated after concurrent events.");
        }
        catch (BackendUnavailableException)
        {
            throw;
        }
        catch (Exception ex) when (ex is RedisException or JsonException)
        {
            throw new BackendUnavailableException(
                "Redis",
                "Authoritative geometry analysis state is temporarily unavailable.",
                ex);
        }
    }

    private static string Key(string fileId) => KeyPrefix + fileId.Trim();
}
