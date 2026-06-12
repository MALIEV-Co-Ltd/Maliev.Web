using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Maliev.Web.Shared.Quotes;
using Microsoft.AspNetCore.WebUtilities;

namespace Maliev.Web.Bff.Security;

/// <summary>
/// Issues signed upload handoff tokens for QuoteEngine.
/// </summary>
public sealed class QuoteUploadHandoffToken(IConfiguration configuration, IHostEnvironment hostEnvironment)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Creates a signed token containing completed Web upload metadata.
    /// </summary>
    public string Create(WebUploadHandoffTokenRequest request)
    {
        var now = DateTimeOffset.UtcNow;
        var payload = new QuoteUploadHandoffTokenPayload(
            request.QuoteSessionId.ToString("D"),
            request.Files,
            now,
            now.AddMinutes(15));

        var json = JsonSerializer.Serialize(payload, JsonOptions);
        var encodedPayload = Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes(json));
        return $"{encodedPayload}.{Sign(encodedPayload)}";
    }

    private string Sign(string encodedPayload)
    {
        using var hmac = new HMACSHA256(GetSigningKey());
        return Base64UrlTextEncoder.Encode(hmac.ComputeHash(Encoding.UTF8.GetBytes(encodedPayload)));
    }

    private byte[] GetSigningKey()
    {
        var configured = configuration["QuoteUploadHandoff:SigningKey"];
        if (string.IsNullOrWhiteSpace(configured))
        {
            if (hostEnvironment.IsProduction())
            {
                throw new InvalidOperationException("QuoteUploadHandoff:SigningKey must be configured in production.");
            }

            configured = "maliev-local-development-quote-upload-handoff-key";
        }

        return Encoding.UTF8.GetBytes(configured);
    }
}

internal sealed record QuoteUploadHandoffTokenPayload(
    string QuoteSessionId,
    List<WebUploadHandoffFileDto> Files,
    DateTimeOffset IssuedAt,
    DateTimeOffset ExpiresAt);
