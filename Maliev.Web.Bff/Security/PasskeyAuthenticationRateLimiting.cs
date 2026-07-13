using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;

namespace Maliev.Web.Bff.Security;

/// <summary>
/// Configures bounded anonymous passkey traffic and trusted reverse-proxy forwarding.
/// </summary>
internal static class PasskeyAuthenticationRateLimiting
{
    private const string ConfigurationSection = "Security:PasskeyRateLimiting";
    private const string TrustedProxySection = "ReverseProxy";
    private const string BeginPath = "/auth/passkey/begin";
    private const string CompletePath = "/auth/passkey/complete";

    /// <summary>Adds per-client rate limits and one shared passkey concurrency gate.</summary>
    internal static IServiceCollection AddPasskeyAuthenticationRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            var settings = configuration.GetSection(ConfigurationSection)
                .Get<PasskeyRateLimitSettings>() ?? new PasskeyRateLimitSettings();
            settings.Validate();

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                CreatePerClientLimiter(settings),
                CreateConcurrencyLimiter(settings));
        });

        return services;
    }

    /// <summary>
    /// Accepts forwarded client IP and scheme values only from explicitly configured immediate proxies.
    /// With no trusted proxy configuration, forwarded headers stay disabled and the socket peer is used.
    /// In GKE, configure <c>ReverseProxy:KnownProxies</c> with exact ingress proxy addresses or
    /// <c>ReverseProxy:KnownNetworks</c> with narrowly scoped ingress source CIDRs.
    /// </summary>
    internal static IServiceCollection AddTrustedProxyForwarding(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var trustedProxies = ParseTrustedProxies(
            configuration.GetSection($"{TrustedProxySection}:KnownProxies").Get<string[]>() ?? []);
        var trustedNetworks = ParseTrustedNetworks(
            configuration.GetSection($"{TrustedProxySection}:KnownNetworks").Get<string[]>() ?? []);

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardLimit = 1;
            if (trustedProxies.Count == 0 && trustedNetworks.Count == 0)
            {
                options.ForwardedHeaders = ForwardedHeaders.None;
                return;
            }

            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownProxies.Clear();
            options.KnownIPNetworks.Clear();
            foreach (var proxy in trustedProxies)
            {
                options.KnownProxies.Add(proxy);
            }

            foreach (var network in trustedNetworks)
            {
                options.KnownIPNetworks.Add(network);
            }
        });

        return services;
    }

    /// <summary>Returns whether at least one explicit trusted proxy address or network is configured.</summary>
    internal static bool HasTrustedProxyConfiguration(IConfiguration configuration) =>
        HasConfiguredValue(configuration.GetSection($"{TrustedProxySection}:KnownProxies")) ||
        HasConfiguredValue(configuration.GetSection($"{TrustedProxySection}:KnownNetworks"));

    private static PartitionedRateLimiter<HttpContext> CreatePerClientLimiter(
        PasskeyRateLimitSettings settings) =>
        PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            if (!TryResolveEndpoint(context.Request.Path, settings, out var endpoint, out var permitLimit))
            {
                return RateLimitPartition.GetNoLimiter("non-passkey");
            }

            var remoteAddress = context.Connection.RemoteIpAddress;
            var clientKey = remoteAddress is null
                ? "unknown"
                : remoteAddress.MapToIPv6().ToString();
            return RateLimitPartition.GetFixedWindowLimiter(
                $"{endpoint}:{clientKey}",
                _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = permitLimit,
                    QueueLimit = 0,
                    Window = TimeSpan.FromSeconds(settings.WindowSeconds)
                });
        });

    private static PartitionedRateLimiter<HttpContext> CreateConcurrencyLimiter(
        PasskeyRateLimitSettings settings) =>
        PartitionedRateLimiter.Create<HttpContext, string>(context =>
            IsPasskeyEndpoint(context.Request.Path)
                ? RateLimitPartition.GetConcurrencyLimiter(
                    "passkey-authentication",
                    _ => new ConcurrencyLimiterOptions
                    {
                        PermitLimit = settings.ConcurrencyPermitLimit,
                        QueueLimit = 0
                    })
                : RateLimitPartition.GetNoLimiter("non-passkey"));

    private static bool TryResolveEndpoint(
        PathString requestPath,
        PasskeyRateLimitSettings settings,
        out string endpoint,
        out int permitLimit)
    {
        if (MatchesEndpointPath(requestPath, BeginPath))
        {
            endpoint = "begin";
            permitLimit = settings.BeginPermitLimit;
            return true;
        }

        if (MatchesEndpointPath(requestPath, CompletePath))
        {
            endpoint = "complete";
            permitLimit = settings.CompletePermitLimit;
            return true;
        }

        endpoint = string.Empty;
        permitLimit = 0;
        return false;
    }

    private static bool IsPasskeyEndpoint(PathString requestPath) =>
        MatchesEndpointPath(requestPath, BeginPath) ||
        MatchesEndpointPath(requestPath, CompletePath);

    private static bool MatchesEndpointPath(PathString requestPath, string expectedPath) =>
        requestPath.Equals(expectedPath, StringComparison.OrdinalIgnoreCase) ||
        requestPath.Equals($"{expectedPath}/", StringComparison.OrdinalIgnoreCase);

    private static List<IPAddress> ParseTrustedProxies(IEnumerable<string> configuredProxies)
    {
        var proxies = new List<IPAddress>();
        foreach (var configuredProxy in configuredProxies)
        {
            if (!IPAddress.TryParse(configuredProxy, out var proxy))
            {
                throw new InvalidOperationException(
                    $"ReverseProxy:KnownProxies contains invalid IP address '{configuredProxy}'.");
            }

            proxies.Add(proxy);
        }

        return proxies;
    }

    private static List<System.Net.IPNetwork> ParseTrustedNetworks(IEnumerable<string> configuredNetworks)
    {
        var networks = new List<System.Net.IPNetwork>();
        foreach (var configuredNetwork in configuredNetworks)
        {
            if (!System.Net.IPNetwork.TryParse(configuredNetwork, out var network))
            {
                throw new InvalidOperationException(
                    $"ReverseProxy:KnownNetworks contains invalid CIDR '{configuredNetwork}'.");
            }

            networks.Add(network);
        }

        return networks;
    }

    private static bool HasConfiguredValue(IConfigurationSection section) =>
        section.GetChildren().Any(value => !string.IsNullOrWhiteSpace(value.Value));

    private sealed class PasskeyRateLimitSettings
    {
        public int BeginPermitLimit { get; init; } = 6;

        public int CompletePermitLimit { get; init; } = 12;

        public int WindowSeconds { get; init; } = 60;

        public int ConcurrencyPermitLimit { get; init; } = 16;

        public void Validate()
        {
            if (BeginPermitLimit is < 1 or > 100 ||
                CompletePermitLimit is < 1 or > 200 ||
                WindowSeconds is < 1 or > 3600 ||
                ConcurrencyPermitLimit is < 1 or > 100)
            {
                throw new InvalidOperationException(
                    $"{ConfigurationSection} contains an unsafe or invalid limit.");
            }
        }
    }
}
