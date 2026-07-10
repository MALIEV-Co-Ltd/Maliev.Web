using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace Maliev.Web.Bff.Security;

/// <summary>
/// Protects the server-owned state that binds a browser GIS button to one AuthService nonce.
/// </summary>
public sealed class GoogleIdentityFlowProtector(
    IDataProtectionProvider dataProtectionProvider,
    TimeProvider timeProvider)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector(
        "Maliev.Web.GoogleIdentityServices.Flow.v1");

    /// <summary>Protects a nonce and validated return URL for a short-lived browser flow.</summary>
    internal string Protect(string nonce, string returnUrl, TimeSpan lifetime)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nonce);
        ArgumentException.ThrowIfNullOrWhiteSpace(returnUrl);

        var state = new GoogleIdentityFlowState(
            nonce,
            returnUrl,
            timeProvider.GetUtcNow().Add(lifetime));
        return _protector.Protect(JsonSerializer.Serialize(state, JsonOptions));
    }

    /// <summary>Unprotects a live browser flow and rejects altered or expired values.</summary>
    internal bool TryUnprotect(string? protectedState, out GoogleIdentityFlowState? state)
    {
        state = null;
        if (string.IsNullOrWhiteSpace(protectedState))
        {
            return false;
        }

        try
        {
            var json = _protector.Unprotect(protectedState);
            var candidate = JsonSerializer.Deserialize<GoogleIdentityFlowState>(json, JsonOptions);
            if (candidate is null ||
                string.IsNullOrWhiteSpace(candidate.Nonce) ||
                string.IsNullOrWhiteSpace(candidate.ReturnUrl) ||
                candidate.ExpiresAtUtc <= timeProvider.GetUtcNow())
            {
                return false;
            }

            state = candidate;
            return true;
        }
        catch (Exception exception) when (exception is CryptographicException or JsonException)
        {
            return false;
        }
    }

    /// <summary>Compares browser and protected nonces without timing-dependent early exits.</summary>
    internal static bool NonceMatches(string expected, string actual)
    {
        var expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
        var actualHash = SHA256.HashData(Encoding.UTF8.GetBytes(actual));
        return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
    }
}

/// <summary>Protected state for one official GIS browser flow.</summary>
internal sealed record GoogleIdentityFlowState(
    string Nonce,
    string ReturnUrl,
    DateTimeOffset ExpiresAtUtc);
