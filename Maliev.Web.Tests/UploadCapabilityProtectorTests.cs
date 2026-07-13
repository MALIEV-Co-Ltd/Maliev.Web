using Maliev.Web.Bff.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace Maliev.Web.Tests;

/// <summary>
/// Unit tests for signed quote-upload capabilities.
/// </summary>
public sealed class UploadCapabilityProtectorTests
{
    private const string UploadId = "web-upload-123";
    private const string StoragePath = "quotes/temp/session/420000/fixture.step";
    private const long FileSizeBytes = 420_000;

    /// <summary>Verifies a live capability preserves its server-owned upload and quote-session binding.</summary>
    [Fact]
    public void TryValidate_LiveCapability_ReturnsBoundUploadState()
    {
        var quoteSessionId = Guid.NewGuid();
        var timeProvider = new ManualTimeProvider(
            new DateTimeOffset(2026, 7, 13, 10, 0, 0, TimeSpan.Zero));
        var protector = CreateProtector(timeProvider);
        var expiresAtUtc = timeProvider.GetUtcNow().AddHours(2);

        var capability = protector.Create(
            UploadId,
            quoteSessionId,
            StoragePath,
            FileSizeBytes,
            expiresAtUtc);
        var success = protector.TryValidate(
            capability,
            UploadId,
            quoteSessionId,
            StoragePath,
            FileSizeBytes,
            out var state);

        Assert.True(success);
        Assert.NotNull(state);
        Assert.Equal(UploadId, state.UploadId);
        Assert.Equal(quoteSessionId, state.QuoteSessionId);
        Assert.Equal(StoragePath, state.StoragePath);
        Assert.Equal(FileSizeBytes, state.FileSizeBytes);
        Assert.Equal(expiresAtUtc, state.ExpiresAtUtc);
        Assert.DoesNotContain(UploadId, capability, StringComparison.Ordinal);
        Assert.DoesNotContain(quoteSessionId.ToString("D"), capability, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Verifies server-issued capabilities use the bounded two-hour upload window.</summary>
    [Fact]
    public void Create_DefaultLifetime_IsTwoHours()
    {
        var quoteSessionId = Guid.NewGuid();
        var timeProvider = new ManualTimeProvider(
            new DateTimeOffset(2026, 7, 13, 10, 0, 0, TimeSpan.Zero));
        var protector = CreateProtector(timeProvider);

        var capability = protector.Create(UploadId, quoteSessionId, StoragePath, FileSizeBytes);
        var success = protector.TryValidate(capability, UploadId, out var state);

        Assert.True(success);
        Assert.NotNull(state);
        Assert.Equal(TimeSpan.FromHours(2), state.ExpiresAtUtc - state.IssuedAtUtc);
    }

    /// <summary>Verifies browser modification invalidates a capability.</summary>
    [Fact]
    public void TryValidate_TamperedCapability_IsRejected()
    {
        var quoteSessionId = Guid.NewGuid();
        var timeProvider = new ManualTimeProvider(
            new DateTimeOffset(2026, 7, 13, 10, 0, 0, TimeSpan.Zero));
        var protector = CreateProtector(timeProvider);
        var capability = protector.Create(
            UploadId,
            quoteSessionId,
            StoragePath,
            FileSizeBytes,
            timeProvider.GetUtcNow().AddHours(2));
        var tampered = capability[..^1] + (capability[^1] == 'A' ? 'B' : 'A');

        var success = protector.TryValidate(tampered, UploadId, out var state);

        Assert.False(success);
        Assert.Null(state);
    }

    /// <summary>Verifies a valid capability cannot authorize a different UploadService session.</summary>
    [Fact]
    public void TryValidate_DifferentUploadId_IsRejected()
    {
        var quoteSessionId = Guid.NewGuid();
        var timeProvider = new ManualTimeProvider(
            new DateTimeOffset(2026, 7, 13, 10, 0, 0, TimeSpan.Zero));
        var protector = CreateProtector(timeProvider);
        var capability = protector.Create(
            UploadId,
            quoteSessionId,
            StoragePath,
            FileSizeBytes,
            timeProvider.GetUtcNow().AddHours(2));

        var success = protector.TryValidate(capability, "web-upload-other", out var state);

        Assert.False(success);
        Assert.Null(state);
    }

    /// <summary>Verifies a valid capability cannot cross an anonymous quote-session boundary.</summary>
    [Fact]
    public void TryValidate_DifferentQuoteSession_IsRejected()
    {
        var quoteSessionId = Guid.NewGuid();
        var timeProvider = new ManualTimeProvider(
            new DateTimeOffset(2026, 7, 13, 10, 0, 0, TimeSpan.Zero));
        var protector = CreateProtector(timeProvider);
        var capability = protector.Create(
            UploadId,
            quoteSessionId,
            StoragePath,
            FileSizeBytes,
            timeProvider.GetUtcNow().AddHours(2));

        var success = protector.TryValidate(
            capability,
            UploadId,
            Guid.NewGuid(),
            StoragePath,
            FileSizeBytes,
            out var state);

        Assert.False(success);
        Assert.Null(state);
    }

    /// <summary>Verifies browser-authored handoff metadata cannot diverge from the protected upload claim.</summary>
    [Theory]
    [InlineData("storagePath")]
    [InlineData("fileSize")]
    public void TryValidate_DifferentCanonicalClaim_IsRejected(string changedClaim)
    {
        var quoteSessionId = Guid.NewGuid();
        var timeProvider = new ManualTimeProvider(
            new DateTimeOffset(2026, 7, 13, 10, 0, 0, TimeSpan.Zero));
        var protector = CreateProtector(timeProvider);
        var capability = protector.Create(
            UploadId,
            quoteSessionId,
            StoragePath,
            FileSizeBytes,
            timeProvider.GetUtcNow().AddHours(2));

        var success = protector.TryValidate(
            capability,
            UploadId,
            quoteSessionId,
            changedClaim == "storagePath" ? "quotes/temp/other/fixture.step" : StoragePath,
            changedClaim == "fileSize" ? FileSizeBytes + 1 : FileSizeBytes,
            out var state);

        Assert.False(success);
        Assert.Null(state);
    }

    /// <summary>Verifies a capability becomes unusable at its server-defined expiry.</summary>
    [Fact]
    public void TryValidate_ExpiredCapability_IsRejected()
    {
        var quoteSessionId = Guid.NewGuid();
        var timeProvider = new ManualTimeProvider(
            new DateTimeOffset(2026, 7, 13, 10, 0, 0, TimeSpan.Zero));
        var protector = CreateProtector(timeProvider);
        var capability = protector.Create(
            UploadId,
            quoteSessionId,
            StoragePath,
            FileSizeBytes,
            timeProvider.GetUtcNow().AddHours(2));
        timeProvider.Advance(TimeSpan.FromHours(2));

        var success = protector.TryValidate(capability, UploadId, out var state);

        Assert.False(success);
        Assert.Null(state);
    }

    /// <summary>Verifies replicas sharing one key repository can validate each other's capabilities.</summary>
    [Fact]
    public void TryValidate_SharedKeyRingAcrossProviders_Succeeds()
    {
        var keyDirectory = Directory.CreateTempSubdirectory("maliev-web-capability-keys-");
        try
        {
            using var issuerServices = CreateSharedKeyRingServices(keyDirectory);
            using var verifierServices = CreateSharedKeyRingServices(keyDirectory);
            var now = new DateTimeOffset(2026, 7, 13, 10, 0, 0, TimeSpan.Zero);
            var issuer = new UploadCapabilityProtector(
                issuerServices.GetRequiredService<IDataProtectionProvider>(),
                new ManualTimeProvider(now));
            var verifier = new UploadCapabilityProtector(
                verifierServices.GetRequiredService<IDataProtectionProvider>(),
                new ManualTimeProvider(now));
            var quoteSessionId = Guid.NewGuid();

            var capability = issuer.Create(UploadId, quoteSessionId, StoragePath, FileSizeBytes);
            var success = verifier.TryValidate(capability, UploadId, out var state);

            Assert.True(success);
            Assert.NotNull(state);
            Assert.Equal(quoteSessionId, state.QuoteSessionId);
        }
        finally
        {
            keyDirectory.Delete(recursive: true);
        }
    }

    private static UploadCapabilityProtector CreateProtector(TimeProvider timeProvider) =>
        new(new EphemeralDataProtectionProvider(), timeProvider);

    private static ServiceProvider CreateSharedKeyRingServices(DirectoryInfo keyDirectory)
    {
        var services = new ServiceCollection();
        services.AddDataProtection()
            .SetApplicationName("Maliev.Identity.Shared")
            .PersistKeysToFileSystem(keyDirectory);
        return services.BuildServiceProvider();
    }

    private sealed class ManualTimeProvider(DateTimeOffset initialUtcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = initialUtcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan duration) => _utcNow = _utcNow.Add(duration);
    }
}
