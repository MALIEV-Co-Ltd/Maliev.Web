using System.Net;
using System.Net.Http.Json;
using Maliev.Web.Client.Services;

namespace Maliev.Web.Tests;

/// <summary>
/// Regression tests for the customer-facing API client.
/// </summary>
public sealed class MalievApiClientTests
{
    /// <summary>
    /// Verifies protected account profile lookups do not turn not-found responses into blank account DTOs.
    /// </summary>
    [Fact]
    public async Task GetAccountProfileAsync_ProfileNotFound_ThrowsInsteadOfReturningBlankProfile()
    {
        using var client = new HttpClient(new StubHandler(request =>
        {
            Assert.Equal("/web/v1/account/profile", request.RequestUri?.AbsolutePath);
            return new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = JsonContent.Create(new
                {
                    title = "Customer session invalid",
                    detail = "Sign in again so MALIEV can resolve your customer profile."
                })
            };
        }))
        {
            BaseAddress = new Uri("https://web.test/")
        };
        var api = new MalievApiClient(client);

        var exception = await Assert.ThrowsAsync<MalievApiException>(() => api.GetAccountProfileAsync());

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Contains("Sign in again", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(respond(request));
        }
    }
}
