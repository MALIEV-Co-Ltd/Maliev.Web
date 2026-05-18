using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace Maliev.Web.Bff.Security;

/// <summary>
/// Writes the signed customer-assistant handoff cookie shared by Web and QuoteEngine.
/// </summary>
public sealed class CustomerAssistantHandoffCookie(IConfiguration configuration, IHostEnvironment hostEnvironment)
{
    /// <summary>Signed handoff cookie name.</summary>
    public const string CookieName = "maliev_customer_assistant_handoff";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Appends a signed handoff cookie to the response.
    /// </summary>
    public void Append(
        HttpRequest request,
        HttpResponse response,
        Guid sessionId,
        string? userKey,
        string language,
        bool isAuthenticated)
    {
        var now = DateTimeOffset.UtcNow;
        var payload = new CustomerAssistantHandoffPayload(
            sessionId,
            string.IsNullOrWhiteSpace(userKey) ? null : userKey.Trim(),
            string.Equals(language, "th", StringComparison.OrdinalIgnoreCase) ? "th" : "en",
            isAuthenticated,
            now,
            now.AddDays(30));

        var json = JsonSerializer.Serialize(payload, JsonOptions);
        var encodedPayload = Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes(json));
        var encodedSignature = Sign(encodedPayload);
        var domain = request.Host.Host.EndsWith(".maliev.com", StringComparison.OrdinalIgnoreCase)
            ? ".maliev.com"
            : null;

        response.Cookies.Append(CookieName, $"{encodedPayload}.{encodedSignature}", new CookieOptions
        {
            Domain = domain,
            HttpOnly = true,
            IsEssential = true,
            MaxAge = TimeSpan.FromDays(30),
            SameSite = SameSiteMode.Lax,
            Secure = request.IsHttps
        });
    }

    private string Sign(string encodedPayload)
    {
        using var hmac = new HMACSHA256(GetSigningKey());
        return Base64UrlTextEncoder.Encode(hmac.ComputeHash(Encoding.UTF8.GetBytes(encodedPayload)));
    }

    private byte[] GetSigningKey()
    {
        var configured = configuration["CustomerAssistant:HandoffSigningKey"];
        if (string.IsNullOrWhiteSpace(configured))
        {
            if (hostEnvironment.IsProduction())
            {
                throw new InvalidOperationException("CustomerAssistant:HandoffSigningKey must be configured in production.");
            }

            configured = "maliev-local-development-customer-assistant-handoff-key";
        }

        return Encoding.UTF8.GetBytes(configured);
    }
}

/// <summary>
/// Signed handoff cookie payload.
/// </summary>
public sealed record CustomerAssistantHandoffPayload(
    Guid SessionId,
    string? UserKey,
    string Language,
    bool IsAuthenticated,
    DateTimeOffset IssuedAt,
    DateTimeOffset ExpiresAt);
