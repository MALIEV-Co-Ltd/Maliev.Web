using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Maliev.Web.Bff.Clients;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Maliev.Web.Tests;

/// <summary>
/// Browser-to-BFF contract tests for the official Google Identity Services customer flow.
/// </summary>
public sealed class GoogleIdentityServicesFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string GoogleClientId = "web-client.apps.googleusercontent.com";
    private const string IssuedNonce = "test-google-identity-nonce-0123456789abcdef";
    private readonly FakeAuthServiceClient _authServiceClient = new();
    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>Initializes the official GIS browser-flow test host.</summary>
    public GoogleIdentityServicesFlowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Authentication:Google:ClientId"] = GoogleClientId
                }));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAuthServiceClient>();
                services.AddSingleton<IAuthServiceClient>(_authServiceClient);
            });
        });
    }

    /// <summary>
    /// Verifies nonce issuance is application-bound and creates an HttpOnly browser-flow cookie.
    /// </summary>
    [Fact]
    public async Task POST_GoogleNonce_BindsOfficialGisFlowToProtectedCookie()
    {
        using var client = CreateClient();

        using var response = await client.PostAsJsonAsync("/auth/google/nonce", new
        {
            returnUrl = "https://attacker.example/steal"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(GoogleClientId, payload.GetProperty("clientId").GetString());
        Assert.Equal(IssuedNonce, payload.GetProperty("nonce").GetString());
        Assert.True(Guid.TryParse(payload.GetProperty("flowId").GetString(), out _));

        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"));
        Assert.Contains("maliev.google.flow.", cookie, StringComparison.Ordinal);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(IssuedNonce, cookie, StringComparison.Ordinal);

        Assert.Equal("web", _authServiceClient.LastNonceRequest.GetProperty("application").GetString());
        Assert.False(_authServiceClient.LastNonceRequest.TryGetProperty("returnUrl", out _));
    }

    /// <summary>
    /// Verifies a GIS credential is forwarded without browser-authored identity claims and issues the MALIEV session.
    /// </summary>
    [Fact]
    public async Task POST_GoogleExchange_ForwardsOnlyVerifiedCredentialContractAndNormalizesReturnUrl()
    {
        using var client = CreateClient();
        var flow = await IssueFlowAsync(client, "https://attacker.example/steal");

        using var response = await client.PostAsJsonAsync("/auth/google/exchange", new
        {
            credential = "google.jwt.credential",
            nonce = flow.Nonce,
            flowId = flow.FlowId
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("/account", payload.GetProperty("redirectUrl").GetString());

        var exchange = _authServiceClient.LastExchangeRequest;
        Assert.Equal("google.jwt.credential", exchange.GetProperty("credential").GetString());
        Assert.Equal("web", exchange.GetProperty("application").GetString());
        Assert.Equal(IssuedNonce, exchange.GetProperty("nonce").GetString());
        Assert.Equal("Asia/Bangkok", exchange.GetProperty("timezone").GetString());
        Assert.True(exchange.TryGetProperty("preferred_language", out _));
        Assert.False(exchange.TryGetProperty("email", out _));
        Assert.False(exchange.TryGetProperty("google_user_id", out _));
        Assert.False(exchange.TryGetProperty("profile_image_url", out _));

        var sessionCookie = response.Headers.GetValues("Set-Cookie")
            .FirstOrDefault(value => value.Contains("Maliev.Identity", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(sessionCookie);
    }

    /// <summary>Verifies cross-app returns require the configured QuoteEngine origin, including scheme and port.</summary>
    [Theory]
    [InlineData("http://make.maliev.com/quotes/new")]
    [InlineData("https://make.maliev.com:444/quotes/new")]
    public async Task POST_GoogleExchange_RejectsDowngradedOrPortChangedCrossAppReturnUrls(string returnUrl)
    {
        using var client = CreateClient();
        var flow = await IssueFlowAsync(client, returnUrl);

        using var response = await client.PostAsJsonAsync("/auth/google/exchange", new
        {
            credential = "google.jwt.credential",
            nonce = flow.Nonce,
            flowId = flow.FlowId
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("/account", payload.GetProperty("redirectUrl").GetString());
    }

    /// <summary>Verifies nonce mismatch and replay attempts never reach AuthService.</summary>
    [Fact]
    public async Task POST_GoogleExchange_RejectsNonceMismatchAndReplay()
    {
        using var mismatchClient = CreateClient();
        var mismatchFlow = await IssueFlowAsync(mismatchClient, "/account/orders");

        using var mismatch = await mismatchClient.PostAsJsonAsync("/auth/google/exchange", new
        {
            credential = "google.jwt.credential",
            nonce = "different-google-identity-nonce-0123456789",
            flowId = mismatchFlow.FlowId
        });

        Assert.Equal(HttpStatusCode.BadRequest, mismatch.StatusCode);
        Assert.Equal(0, _authServiceClient.ExchangeCount);

        using var replayClient = CreateClient();
        var replayFlow = await IssueFlowAsync(replayClient, "/account/orders");
        var request = new
        {
            credential = "google.jwt.credential",
            nonce = replayFlow.Nonce,
            flowId = replayFlow.FlowId
        };

        using var first = await replayClient.PostAsJsonAsync("/auth/google/exchange", request);
        using var replay = await replayClient.PostAsJsonAsync("/auth/google/exchange", request);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, replay.StatusCode);
        Assert.Equal(1, _authServiceClient.ExchangeCount);
    }

    /// <summary>Verifies existing password accounts receive an actionable Google-linking failure.</summary>
    [Fact]
    public async Task POST_GoogleExchange_MapsAccountVerificationRequired()
    {
        _authServiceClient.ExchangeResponseFactory = () => new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = JsonContent.Create(new
            {
                error = "account_verification_required",
                error_description = "Verify the existing MALIEV account before linking Google."
            })
        };
        using var client = CreateClient();
        var flow = await IssueFlowAsync(client, "/checkout");

        using var response = await client.PostAsJsonAsync("/auth/google/exchange", new
        {
            credential = "google.jwt.credential",
            nonce = flow.Nonce,
            flowId = flow.FlowId
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("account_verification_required", problem.GetProperty("code").GetString());
        Assert.Contains("email and password", problem.GetProperty("detail").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Verifies the Web UI delegates button rendering and personalization to Google's GIS library.</summary>
    [Fact]
    public void OfficialGisButton_IsRenderedByGoogleWithoutAHandDrawnBrandButton()
    {
        var root = FindRepoRoot();
        var component = File.ReadAllText(Path.Combine(root, "Maliev.Web.Client", "Components", "AuthGoogleButton.razor"));
        var script = File.ReadAllText(Path.Combine(root, "Maliev.Web.Bff", "wwwroot", "js", "maliev-google-identity.js"));
        var app = File.ReadAllText(Path.Combine(root, "Maliev.Web.Bff", "Components", "App.razor"));
        var program = File.ReadAllText(Path.Combine(root, "Maliev.Web.Bff", "Program.cs"));
        var project = File.ReadAllText(Path.Combine(root, "Maliev.Web.Bff", "Maliev.Web.Bff.csproj"));
        var chatbot = File.ReadAllText(Path.Combine(root, "Maliev.Web.Client", "Components", "CustomerChatbot.razor"));

        Assert.Contains("auth-google-official-host", component, StringComparison.Ordinal);
        Assert.Contains("malievGoogleIdentity.renderButton", component, StringComparison.Ordinal);
        Assert.DoesNotContain("<svg", component, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Href", component, StringComparison.Ordinal);
        Assert.Contains("https://accounts.google.com/gsi/client", app, StringComparison.Ordinal);
        Assert.Contains("google.accounts.id.initialize", script, StringComparison.Ordinal);
        Assert.Contains("nonce: flow.nonce", script, StringComparison.Ordinal);
        Assert.Contains("google.accounts.id.renderButton", script, StringComparison.Ordinal);
        Assert.Contains("/auth/google/nonce", script, StringComparison.Ordinal);
        Assert.Contains("/auth/google/exchange", script, StringComparison.Ordinal);
        Assert.DoesNotContain("AddGoogle", program, StringComparison.Ordinal);
        Assert.DoesNotContain("Authentication:Google:ClientSecret", program, StringComparison.Ordinal);
        Assert.DoesNotContain("Microsoft.AspNetCore.Authentication.Google", project, StringComparison.Ordinal);
        Assert.DoesNotContain("auth-google-icon", chatbot, StringComparison.Ordinal);
        Assert.DoesNotContain("/auth/google?returnUrl", chatbot, StringComparison.Ordinal);
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = true
    });

    private static async Task<(string Nonce, string FlowId)> IssueFlowAsync(HttpClient client, string returnUrl)
    {
        using var response = await client.PostAsJsonAsync("/auth/google/nonce", new { returnUrl });
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        return (
            payload.GetProperty("nonce").GetString()!,
            payload.GetProperty("flowId").GetString()!);
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

    private sealed class FakeAuthServiceClient : IAuthServiceClient
    {
        public JsonElement LastNonceRequest { get; private set; }
        public JsonElement LastExchangeRequest { get; private set; }
        public int ExchangeCount { get; private set; }
        public Func<HttpResponseMessage>? ExchangeResponseFactory { get; set; }

        public Task<HttpResponseMessage> IssueCustomerGoogleNonceAsync(object request, CancellationToken cancellationToken)
        {
            LastNonceRequest = JsonSerializer.SerializeToElement(request);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    nonce = IssuedNonce,
                    expires_at_utc = DateTimeOffset.UtcNow.AddMinutes(10)
                })
            });
        }

        public Task<HttpResponseMessage> ExchangeCustomerGoogleAsync(object request, CancellationToken cancellationToken)
        {
            LastExchangeRequest = JsonSerializer.SerializeToElement(request);
            ExchangeCount++;
            return Task.FromResult(ExchangeResponseFactory?.Invoke() ?? new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    user = new
                    {
                        user_id = "66f3abec-44d6-4c8f-8196-7bc7ec169b91",
                        principal_id = "66f3abec-44d6-4c8f-8196-7bc7ec169b91",
                        customer_id = "41ebf5d6-8e31-437e-a260-29df50fbf6b4",
                        email = "customer@example.com",
                        name = "Customer Example",
                        profile_image_url = "https://lh3.googleusercontent.com/avatar",
                        email_verified = true
                    }
                })
            });
        }

        public Task<HttpResponseMessage> LoginAsync(object request, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<HttpResponseMessage> RequestPasswordResetAsync(object request, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<HttpResponseMessage> ConfirmPasswordResetAsync(object request, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<HttpResponseMessage> InitiateEmailVerificationAsync(object request, CancellationToken ct) => throw new NotSupportedException();
        public Task<HttpResponseMessage> VerifyEmailAsync(object request, CancellationToken ct) => throw new NotSupportedException();
        public Task<HttpResponseMessage> ResendVerificationEmailAsync(object request, CancellationToken ct) => throw new NotSupportedException();
        public Task<HttpResponseMessage> GetCurrentPrincipalAsync(Guid principalId, CancellationToken ct) => throw new NotSupportedException();
        public Task<HttpResponseMessage> PasskeyRegisterBeginAsync(object request, CancellationToken ct) => throw new NotSupportedException();
        public Task<HttpResponseMessage> PasskeyRegisterCompleteAsync(object request, CancellationToken ct) => throw new NotSupportedException();
        public Task<HttpResponseMessage> PasskeyAuthBeginAsync(object request, CancellationToken ct) => throw new NotSupportedException();
        public Task<HttpResponseMessage> PasskeyAuthCompleteAsync(object request, CancellationToken ct) => throw new NotSupportedException();
        public Task<HttpResponseMessage> ListPasskeyCredentialsAsync(Guid principalId, CancellationToken ct) => throw new NotSupportedException();
        public Task<HttpResponseMessage> DeletePasskeyCredentialAsync(Guid credentialId, Guid principalId, CancellationToken ct) => throw new NotSupportedException();
    }
}
