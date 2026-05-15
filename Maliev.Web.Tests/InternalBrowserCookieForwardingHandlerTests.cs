using System.Net;
using Maliev.Web.Bff.Services;
using Microsoft.AspNetCore.Http;

namespace Maliev.Web.Tests;

/// <summary>
/// Contract tests for the internal Web BFF HttpClient used during server-side rendering.
/// </summary>
public sealed class InternalBrowserCookieForwardingHandlerTests
{
    /// <summary>
    /// Verifies server-side component calls to protected same-origin BFF endpoints preserve the browser auth cookie.
    /// </summary>
    [Fact]
    public async Task SendAsync_CurrentRequestHasCookie_ForwardsCookieToInternalBffRequest()
    {
        const string cookie = "__Host-Maliev.Web=test-session";
        using var inner = new CapturingHandler();
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
        accessor.HttpContext.Request.Headers.Cookie = cookie;
        using var forwardingHandler = new InternalBrowserCookieForwardingHandler(accessor)
        {
            InnerHandler = inner
        };
        using var client = new HttpMessageInvoker(forwardingHandler);

        using var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://web.test/web/v1/account/profile"), CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(inner.Request);
        Assert.True(inner.Request.Headers.TryGetValues("Cookie", out var values));
        Assert.Contains(cookie, values);
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
