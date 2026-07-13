using System.Globalization;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Maliev.Web.Bff.Security;

/// <summary>Configures per-IP request budgets for anonymous, cost-bearing endpoints.</summary>
internal static class PublicIngressRateLimiting
{
    private const string ConfigurationSection = "Security:PublicIngressRateLimiting";

    internal static IServiceCollection AddPublicIngressRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration.GetSection(ConfigurationSection)
            .Get<PublicIngressRateLimitSettings>() ?? new PublicIngressRateLimitSettings();
        settings.Validate();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            AddFixedWindowPolicy(options, WebRateLimiterPolicies.ChatbotSession, settings.ChatbotSessionPermitLimit, settings.WindowSeconds);
            AddFixedWindowPolicy(options, WebRateLimiterPolicies.ChatbotMessage, settings.ChatbotMessagePermitLimit, settings.WindowSeconds);
            AddFixedWindowPolicy(options, WebRateLimiterPolicies.Contact, settings.ContactPermitLimit, settings.WindowSeconds);
            AddFixedWindowPolicy(options, WebRateLimiterPolicies.QuoteEstimate, settings.QuoteEstimatePermitLimit, settings.WindowSeconds);
            AddFixedWindowPolicy(options, WebRateLimiterPolicies.QuoteReference, settings.QuoteReferencePermitLimit, settings.WindowSeconds);
            AddFixedWindowPolicy(options, WebRateLimiterPolicies.UploadInitiate, settings.UploadInitiatePermitLimit, settings.WindowSeconds);
            AddUploadStreamPolicy(options, settings);
            AddFixedWindowPolicy(options, WebRateLimiterPolicies.UploadFinalize, settings.UploadFinalizePermitLimit, settings.WindowSeconds);
            AddFixedWindowPolicy(options, WebRateLimiterPolicies.UploadHandoff, settings.UploadHandoffPermitLimit, settings.WindowSeconds);
            AddFixedWindowPolicy(options, WebRateLimiterPolicies.UploadStatus, settings.UploadStatusPermitLimit, settings.WindowSeconds);
            AddFixedWindowPolicy(options, WebRateLimiterPolicies.Checkout, settings.CheckoutPermitLimit, settings.WindowSeconds);
            AddFixedWindowPolicy(options, WebRateLimiterPolicies.ShippingRate, settings.ShippingRatePermitLimit, settings.WindowSeconds);
            AddFixedWindowPolicy(options, WebRateLimiterPolicies.ShippingRead, settings.ShippingReadPermitLimit, settings.WindowSeconds);

            options.OnRejected = WriteRateLimitProblemAsync;
        });

        return services;
    }

    private static void AddFixedWindowPolicy(
        Microsoft.AspNetCore.RateLimiting.RateLimiterOptions options,
        string policyName,
        int permitLimit,
        int windowSeconds) =>
        options.AddPolicy(policyName, context =>
            RateLimitPartition.GetFixedWindowLimiter(
                $"{policyName}:{ResolveClientKey(context)}",
                _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = permitLimit,
                    QueueLimit = 0,
                    Window = TimeSpan.FromSeconds(windowSeconds)
                }));

    private static void AddUploadStreamPolicy(
        Microsoft.AspNetCore.RateLimiting.RateLimiterOptions options,
        PublicIngressRateLimitSettings settings) =>
        options.AddPolicy(WebRateLimiterPolicies.UploadStream, context =>
        {
            var partitionKey = $"{WebRateLimiterPolicies.UploadStream}:{ResolveClientKey(context)}";
            return RateLimitPartition.Get(
                partitionKey,
                _ => RateLimiter.CreateChained(
                    new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = settings.UploadStreamPermitLimit,
                        QueueLimit = 0,
                        Window = TimeSpan.FromSeconds(settings.WindowSeconds)
                    }),
                    new ConcurrencyLimiter(new ConcurrencyLimiterOptions
                    {
                        PermitLimit = settings.UploadStreamConcurrencyPermitLimit,
                        QueueLimit = 0
                    })));
        });

    private static string ResolveClientKey(HttpContext context)
    {
        var address = context.Connection.RemoteIpAddress;
        return address is null ? "unknown" : address.MapToIPv6().ToString();
    }

    private static async ValueTask WriteRateLimitProblemAsync(
        OnRejectedContext context,
        CancellationToken cancellationToken)
    {
        var retryAfterSeconds = 1;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));
        }

        context.HttpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString(CultureInfo.InvariantCulture);
        context.HttpContext.Response.ContentType = "application/problem+json";
        var isPasskey = context.HttpContext.Request.Path.StartsWithSegments(
            "/auth/passkey",
            StringComparison.OrdinalIgnoreCase);
        await JsonSerializer.SerializeAsync(
            context.HttpContext.Response.Body,
            new
            {
                type = isPasskey
                    ? "https://www.maliev.com/problems/passkey-rate-limit"
                    : "https://www.maliev.com/problems/rate-limit",
                title = isPasskey ? "Too many passkey sign-in attempts" : "Too many requests",
                status = StatusCodes.Status429TooManyRequests,
                detail = isPasskey
                    ? "Wait briefly before trying passkey sign-in again."
                    : "Wait briefly before trying again.",
                code = isPasskey ? "passkey_rate_limited" : "rate_limited"
            },
            cancellationToken: cancellationToken);
    }

    private sealed class PublicIngressRateLimitSettings
    {
        public int WindowSeconds { get; init; } = 60;
        public int ChatbotSessionPermitLimit { get; init; } = 6;
        public int ChatbotMessagePermitLimit { get; init; } = 30;
        public int ContactPermitLimit { get; init; } = 5;
        public int QuoteEstimatePermitLimit { get; init; } = 12;
        public int QuoteReferencePermitLimit { get; init; } = 30;
        public int UploadInitiatePermitLimit { get; init; } = 20;
        public int UploadStreamPermitLimit { get; init; } = 40;
        public int UploadStreamConcurrencyPermitLimit { get; init; } = 2;
        public int UploadFinalizePermitLimit { get; init; } = 40;
        public int UploadHandoffPermitLimit { get; init; } = 10;
        public int UploadStatusPermitLimit { get; init; } = 120;
        public int CheckoutPermitLimit { get; init; } = 10;
        public int ShippingRatePermitLimit { get; init; } = 20;
        public int ShippingReadPermitLimit { get; init; } = 30;

        public void Validate()
        {
            var limits = new[]
            {
                ChatbotSessionPermitLimit, ChatbotMessagePermitLimit, ContactPermitLimit,
                QuoteEstimatePermitLimit, QuoteReferencePermitLimit, UploadInitiatePermitLimit,
                UploadStreamPermitLimit, UploadFinalizePermitLimit, UploadHandoffPermitLimit,
                UploadStatusPermitLimit, CheckoutPermitLimit, ShippingRatePermitLimit,
                ShippingReadPermitLimit
            };
            if (WindowSeconds is < 1 or > 3600 ||
                UploadStreamConcurrencyPermitLimit is < 1 or > 20 ||
                limits.Any(limit => limit is < 1 or > 1000))
            {
                throw new InvalidOperationException($"{ConfigurationSection} contains an unsafe or invalid limit.");
            }
        }
    }
}
