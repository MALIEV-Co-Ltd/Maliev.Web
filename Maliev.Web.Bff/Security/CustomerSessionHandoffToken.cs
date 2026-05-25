using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace Maliev.Web.Bff.Security;

/// <summary>
/// Creates short-lived signed customer session handoff tokens for trusted MALIEV apps.
/// </summary>
public sealed class CustomerSessionHandoffToken(IConfiguration configuration, IHostEnvironment hostEnvironment)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Creates a signed token for the provided customer session payload.</summary>
    public string Create(CustomerSessionHandoffPayload payload)
    {
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        var encodedPayload = Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes(json));
        using var hmac = new HMACSHA256(GetSigningKey());
        var signature = Base64UrlTextEncoder.Encode(hmac.ComputeHash(Encoding.UTF8.GetBytes(encodedPayload)));
        return $"{encodedPayload}.{signature}";
    }

    private byte[] GetSigningKey()
    {
        var configured = configuration["CustomerSession:HandoffSigningKey"]
            ?? configuration["CustomerAssistant:HandoffSigningKey"];
        if (string.IsNullOrWhiteSpace(configured))
        {
            if (hostEnvironment.IsProduction())
            {
                throw new InvalidOperationException("CustomerSession:HandoffSigningKey must be configured in production.");
            }

            configured = "maliev-local-development-customer-session-handoff-key";
        }

        return Encoding.UTF8.GetBytes(configured);
    }
}

/// <summary>
/// Signed customer session payload passed from Web to QuoteEngine.
/// </summary>
public sealed record CustomerSessionHandoffPayload(
    Guid CustomerId,
    string? PrincipalId,
    string? Email,
    string? DisplayName,
    DateTimeOffset IssuedAt,
    DateTimeOffset ExpiresAt);
