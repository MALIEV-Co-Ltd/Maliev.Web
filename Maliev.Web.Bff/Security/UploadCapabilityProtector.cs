using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace Maliev.Web.Bff.Security;

/// <summary>Protects short-lived bearer proof for one browser-created quote upload.</summary>
public sealed class UploadCapabilityProtector(
    IDataProtectionProvider dataProtectionProvider,
    TimeProvider timeProvider)
{
    /// <summary>Request header carrying one upload capability.</summary>
    public const string HeaderName = "X-Maliev-Upload-Capability";

    private const string Purpose = "Maliev.Web.QuoteUploadCapability.v1";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector(Purpose);

    /// <summary>Protects one upload capability for the standard two-hour upload window.</summary>
    public string Create(
        string uploadId,
        Guid quoteSessionId,
        string storagePath,
        long fileSizeBytes) =>
        Create(uploadId, quoteSessionId, storagePath, fileSizeBytes, timeProvider.GetUtcNow().AddHours(2));

    /// <summary>Protects one upload capability until the supplied expiry.</summary>
    public string Create(
        string uploadId,
        Guid quoteSessionId,
        string storagePath,
        long fileSizeBytes,
        DateTimeOffset expiresAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uploadId);
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);
        if (uploadId.Length > 128 ||
            quoteSessionId == Guid.Empty ||
            storagePath.Length > 512 ||
            fileSizeBytes is <= 0 or > Maliev.Web.Shared.Quotes.WebQuoteUploadConstraints.MaxFileSizeBytes ||
            expiresAtUtc <= timeProvider.GetUtcNow())
        {
            throw new ArgumentException("Canonical upload capability data is required.", nameof(uploadId));
        }

        var state = new UploadCapabilityState(
            uploadId,
            quoteSessionId,
            storagePath.Replace('\\', '/'),
            fileSizeBytes,
            timeProvider.GetUtcNow(),
            expiresAtUtc);
        return _protector.Protect(JsonSerializer.Serialize(state, JsonOptions));
    }

    /// <summary>Validates an unexpired capability for one upload route.</summary>
    public bool TryValidate(
        string? capability,
        string uploadId,
        out UploadCapabilityState? state) =>
        TryValidateCore(capability, uploadId, null, null, null, out state);

    /// <summary>Validates an unexpired capability against a complete browser handoff claim.</summary>
    public bool TryValidate(
        string? capability,
        string uploadId,
        Guid quoteSessionId,
        string storagePath,
        long fileSizeBytes,
        out UploadCapabilityState? state) =>
        TryValidateCore(capability, uploadId, quoteSessionId, storagePath, fileSizeBytes, out state);

    private bool TryValidateCore(
        string? capability,
        string uploadId,
        Guid? quoteSessionId,
        string? storagePath,
        long? fileSizeBytes,
        out UploadCapabilityState? state)
    {
        state = null;
        if (string.IsNullOrWhiteSpace(capability) || string.IsNullOrWhiteSpace(uploadId))
        {
            return false;
        }

        try
        {
            var json = _protector.Unprotect(capability);
            var candidate = JsonSerializer.Deserialize<UploadCapabilityState>(json, JsonOptions);
            if (candidate is null ||
                candidate.ExpiresAtUtc <= timeProvider.GetUtcNow() ||
                candidate.IssuedAtUtc > timeProvider.GetUtcNow() ||
                !string.Equals(candidate.UploadId, uploadId, StringComparison.Ordinal) ||
                (quoteSessionId.HasValue && candidate.QuoteSessionId != quoteSessionId.Value) ||
                (storagePath is not null && !string.Equals(
                    candidate.StoragePath,
                    storagePath.Replace('\\', '/'),
                    StringComparison.Ordinal)) ||
                (fileSizeBytes.HasValue && candidate.FileSizeBytes != fileSizeBytes.Value))
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
}

/// <summary>Authenticated upload capability state.</summary>
public sealed record UploadCapabilityState(
    string UploadId,
    Guid QuoteSessionId,
    string StoragePath,
    long FileSizeBytes,
    DateTimeOffset IssuedAtUtc,
    DateTimeOffset ExpiresAtUtc);
