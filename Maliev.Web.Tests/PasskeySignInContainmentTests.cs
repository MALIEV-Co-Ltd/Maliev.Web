using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Maliev.Web.Bff.Clients;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Maliev.Web.Tests;

/// <summary>
/// Regression tests for the retired browser-authored passkey identity handoff.
/// </summary>
public sealed class PasskeySignInContainmentTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly Guid ForgedPrincipalId = Guid.Parse("c03c5c1f-a1cf-49fb-a361-9576117a69e9");
    private readonly RecordingCustomerServiceClient _customerServiceClient = new();
    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>Initializes the fail-closed passkey containment test host.</summary>
    public PasskeySignInContainmentTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder
            .UseEnvironment("Testing")
            .ConfigureServices(services =>
            {
                services.RemoveAll<ICustomerServiceClient>();
                services.AddSingleton<ICustomerServiceClient>(_customerServiceClient);
            }));
    }

    /// <summary>
    /// Verifies caller-authored identity fields cannot create a customer session through the legacy handoff.
    /// </summary>
    [Fact]
    public async Task POST_LegacyPasskeySignIn_WithForgedIdentity_IsRetiredWithoutIssuingSession()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
        var antiforgeryToken = await GetAntiforgeryTokenAsync(client);

        using var response = await client.PostAsync(
            "/auth/passkey-sign-in",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = antiforgeryToken,
                ["PrincipalId"] = ForgedPrincipalId.ToString(),
                ["Email"] = "attacker-controlled@example.test",
                ["ReturnUrl"] = "/account/profile"
            }));
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        var code = payload.TryGetProperty("code", out var codeElement) ? codeElement.GetString() : null;
        var identityCookieIssued = response.Headers.TryGetValues("Set-Cookie", out var setCookies)
            && setCookies.Any(value => value.Contains("Maliev.Identity", StringComparison.OrdinalIgnoreCase));

        using var sessionResponse = await client.GetAsync("/web/v1/account/session");
        sessionResponse.EnsureSuccessStatusCode();
        var session = await sessionResponse.Content.ReadFromJsonAsync<JsonElement>();
        var isAuthenticated = session.GetProperty("isAuthenticated").GetBoolean();

        var failedClosed = response.StatusCode == HttpStatusCode.Gone
            && string.Equals(code, "passkey_flow_retired", StringComparison.Ordinal)
            && !identityCookieIssued
            && !isAuthenticated
            && _customerServiceClient.LookupCount == 0;

        Assert.True(
            failedClosed,
            $"Expected legacy passkey handoff to fail closed. Actual status={(int)response.StatusCode} " +
            $"({response.StatusCode}), code={code ?? "<missing>"}, " +
            $"identityCookieIssued={identityCookieIssued}, isAuthenticated={isAuthenticated}, " +
            $"downstreamCustomerLookups={_customerServiceClient.LookupCount}.");
    }

    /// <summary>Verifies browser code and MVC binding no longer expose the caller-authored identity handoff.</summary>
    [Fact]
    public void LegacyPasskeyIdentityHandoff_IsNotExposedByBrowserOrMvcBinding()
    {
        var root = FindRepoRoot();
        var script = File.ReadAllText(Path.Combine(root, "Maliev.Web.Bff", "wwwroot", "js", "maliev-passkey.js"));
        var controller = File.ReadAllText(Path.Combine(root, "Maliev.Web.Bff", "Controllers", "AuthController.cs"));

        Assert.DoesNotContain("window.submitPasskeySignIn", script, StringComparison.Ordinal);
        Assert.DoesNotContain("class PasskeySignInRequest", controller, StringComparison.Ordinal);
    }

    private static async Task<string> GetAntiforgeryTokenAsync(HttpClient client)
    {
        using var page = await client.GetAsync("/auth/forgot-password");
        page.EnsureSuccessStatusCode();
        var html = await page.Content.ReadAsStringAsync();
        var match = Regex.Match(
            html,
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
            RegexOptions.CultureInvariant);

        Assert.True(match.Success, "The rendered password-reset form did not contain an antiforgery request token.");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Maliev.Web.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the Maliev.Web repository root.");
    }

    private sealed class RecordingCustomerServiceClient : ICustomerServiceClient
    {
        public int LookupCount { get; private set; }

        public Task<HttpResponseMessage> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));

        public Task<HttpResponseMessage> UpdateCustomerAsync(
            Guid customerId,
            object request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));

        public Task<HttpResponseMessage> GetCompanyAsync(Guid companyId, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));

        public Task<HttpResponseMessage> CreateCompanyAsync(object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created));

        public Task<HttpResponseMessage> UpdateCompanyAsync(
            Guid companyId,
            object request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));

        public Task<HttpResponseMessage> RegisterCustomerAsync(object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created));

        public Task<HttpResponseMessage> GetCustomerAddressesAsync(Guid customerId, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> CreateCustomerAddressAsync(object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created));

        public Task<HttpResponseMessage> UpdateCustomerAddressAsync(
            Guid addressId,
            object request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));

        public Task<HttpResponseMessage> DeleteCustomerAddressAsync(
            Guid addressId,
            object request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));

        public Task<HttpResponseMessage> GetCustomerByPrincipalIdAsync(
            Guid principalId,
            CancellationToken cancellationToken)
        {
            LookupCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    email = "customer@example.test",
                    firstName = "Existing customer",
                    profileImageUrl = "https://cdn.example.test/avatar.png"
                })
            });
        }
    }
}
