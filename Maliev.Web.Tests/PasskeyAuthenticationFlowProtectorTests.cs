using Maliev.Web.Bff.Security;
using Microsoft.AspNetCore.DataProtection;

namespace Maliev.Web.Tests;

/// <summary>
/// Unit tests for browser-bound passkey flow integrity and expiry.
/// </summary>
public sealed class PasskeyAuthenticationFlowProtectorTests
{
    private const string FlowId = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

    /// <summary>Verifies live protected state round-trips without exposing mutable fields.</summary>
    [Fact]
    public void TryUnprotect_LiveFlow_ReturnsOriginalServerState()
    {
        var timeProvider = new ManualTimeProvider(
            new DateTimeOffset(2026, 7, 11, 10, 0, 0, TimeSpan.Zero));
        var protector = CreateProtector(timeProvider);
        var protectedState = protector.Protect(
            FlowId,
            "/account/orders",
            timeProvider.GetUtcNow().AddMinutes(5));

        var success = protector.TryUnprotect(protectedState, out var state);

        Assert.True(success);
        Assert.NotNull(state);
        Assert.Equal(FlowId, state.FlowId);
        Assert.Equal("/account/orders", state.ReturnUrl);
        Assert.DoesNotContain(FlowId, protectedState, StringComparison.Ordinal);
    }

    /// <summary>Verifies altered protected state is rejected.</summary>
    [Fact]
    public void TryUnprotect_AlteredFlow_IsRejected()
    {
        var timeProvider = new ManualTimeProvider(
            new DateTimeOffset(2026, 7, 11, 10, 0, 0, TimeSpan.Zero));
        var protector = CreateProtector(timeProvider);
        var protectedState = protector.Protect(
            FlowId,
            "/account",
            timeProvider.GetUtcNow().AddMinutes(5));
        var altered = protectedState[..^1] + (protectedState[^1] == 'A' ? 'B' : 'A');

        var success = protector.TryUnprotect(altered, out var state);

        Assert.False(success);
        Assert.Null(state);
    }

    /// <summary>Verifies arbitrary malformed cookie state is rejected without escaping the boundary.</summary>
    [Fact]
    public void TryUnprotect_MalformedState_IsRejected()
    {
        var timeProvider = new ManualTimeProvider(
            new DateTimeOffset(2026, 7, 11, 10, 0, 0, TimeSpan.Zero));
        var protector = CreateProtector(timeProvider);

        var success = protector.TryUnprotect("%%%not-protected%%%", out var state);

        Assert.False(success);
        Assert.Null(state);
    }

    /// <summary>Verifies expired protected state is rejected before AuthService is called.</summary>
    [Fact]
    public void TryUnprotect_ExpiredFlow_IsRejected()
    {
        var timeProvider = new ManualTimeProvider(
            new DateTimeOffset(2026, 7, 11, 10, 0, 0, TimeSpan.Zero));
        var protector = CreateProtector(timeProvider);
        var protectedState = protector.Protect(
            FlowId,
            "/account",
            timeProvider.GetUtcNow().AddMinutes(5));
        timeProvider.Advance(TimeSpan.FromMinutes(6));

        var success = protector.TryUnprotect(protectedState, out var state);

        Assert.False(success);
        Assert.Null(state);
    }

    private static PasskeyAuthenticationFlowProtector CreateProtector(TimeProvider timeProvider) =>
        new(new EphemeralDataProtectionProvider(), timeProvider);

    private sealed class ManualTimeProvider(DateTimeOffset initialUtcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = initialUtcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan duration) => _utcNow = _utcNow.Add(duration);
    }
}
