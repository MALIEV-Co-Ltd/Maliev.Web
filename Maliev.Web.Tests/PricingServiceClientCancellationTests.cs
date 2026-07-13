using Maliev.Web.Bff.Clients;
using Microsoft.Extensions.Logging.Abstractions;

namespace Maliev.Web.Tests;

/// <summary>Cancellation regressions for PricingService HTTP operations.</summary>
public sealed class PricingServiceClientCancellationTests
{
    /// <summary>Lead-time cancellation remains caller cancellation.</summary>
    [Fact]
    public async Task GetLeadTimesAsync_CallerCancellation_Propagates()
    {
        using var httpClient = CreateBlockingClient();
        var client = new PricingServiceClient(httpClient, NullLogger<PricingServiceClient>.Instance);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.GetLeadTimesAsync(cancellation.Token));
    }

    /// <summary>Quote-price cancellation remains caller cancellation.</summary>
    [Fact]
    public async Task CalculateAsync_CallerCancellation_Propagates()
    {
        using var httpClient = CreateBlockingClient();
        var client = new PricingServiceClient(httpClient, NullLogger<PricingServiceClient>.Instance);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.CalculateAsync(new PricingCalculationRequest(), cancellation.Token));
    }

    private static HttpClient CreateBlockingClient() => new(new BlockingHandler())
    {
        BaseAddress = new Uri("http://pricing-service")
    };

    private sealed class BlockingHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            throw new InvalidOperationException("unreachable");
        }
    }
}
