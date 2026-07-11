using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace Maliev.Web.Bff.Security;

/// <summary>
/// Protects the AuthService flow identifier and validated return URL from browser modification.
/// </summary>
public sealed class PasskeyAuthenticationFlowProtector(
    IDataProtectionProvider dataProtectionProvider,
    TimeProvider timeProvider)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector(
        "Maliev.Web.PasskeyAuthentication.Flow.v1");

    /// <summary>Protects one live AuthService ceremony for this browser.</summary>
    internal string Protect(string flowId, string returnUrl, DateTimeOffset expiresAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(flowId);
        ArgumentException.ThrowIfNullOrWhiteSpace(returnUrl);
        if (!IsCanonicalFlowId(flowId) || expiresAtUtc <= timeProvider.GetUtcNow())
        {
            throw new ArgumentException("A live canonical passkey flow is required.", nameof(flowId));
        }

        var state = new PasskeyAuthenticationFlowState(flowId, returnUrl, expiresAtUtc);
        return _protector.Protect(JsonSerializer.Serialize(state, JsonOptions));
    }

    /// <summary>Unprotects an unexpired flow and rejects altered or malformed state.</summary>
    internal bool TryUnprotect(
        string? protectedState,
        out PasskeyAuthenticationFlowState? state)
    {
        state = null;
        if (string.IsNullOrWhiteSpace(protectedState))
        {
            return false;
        }

        try
        {
            var json = _protector.Unprotect(protectedState);
            var candidate = JsonSerializer.Deserialize<PasskeyAuthenticationFlowState>(json, JsonOptions);
            if (candidate is null ||
                !IsCanonicalFlowId(candidate.FlowId) ||
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

    private static bool IsCanonicalFlowId(string value)
    {
        if (value is not { Length: 43 } ||
            value.Any(character =>
                !(character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '-' or '_')))
        {
            return false;
        }

        try
        {
            var decoded = Convert.FromBase64String(
                value.Replace('-', '+').Replace('_', '/') + "=");
            return decoded.Length == 32 &&
                string.Equals(ToBase64Url(decoded), value, StringComparison.Ordinal);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static string ToBase64Url(byte[] value) =>
        Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}

/// <summary>Protected state for one browser-bound passkey authentication ceremony.</summary>
internal sealed record PasskeyAuthenticationFlowState(
    string FlowId,
    string ReturnUrl,
    DateTimeOffset ExpiresAtUtc);
