using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Maliev.Web.Bff.Clients;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Maliev.Web.Tests;

/// <summary>
/// Browser-to-BFF contract tests for verified, one-time passkey authentication.
/// </summary>
public sealed class PasskeyAuthenticationFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly Guid PrincipalId = Guid.Parse("66f3abec-44d6-4c8f-8196-7bc7ec169b91");
    private static readonly Guid CustomerId = Guid.Parse("41ebf5d6-8e31-437e-a260-29df50fbf6b4");
    private const string IssuedFlowId = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
    private static readonly string Challenge = ToBase64Url(Enumerable.Repeat((byte)7, 32).ToArray());
    private const string CustomerEmail = "customer@example.test";
    private readonly FakeAuthServiceClient _authServiceClient = new();
    private readonly FakeCustomerServiceClient _customerServiceClient = new();
    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>Initializes a real HTTP host with controlled AuthService and CustomerService boundaries.</summary>
    public PasskeyAuthenticationFlowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder
            .UseEnvironment("Testing")
            .ConfigureServices(services =>
            {
                services.RemoveAll<IAuthServiceClient>();
                services.RemoveAll<ICustomerServiceClient>();
                services.AddSingleton<IAuthServiceClient>(_authServiceClient);
                services.AddSingleton<ICustomerServiceClient>(_customerServiceClient);
            }));
    }

    /// <summary>Verifies begin hides AuthService state in a protected HttpOnly cookie.</summary>
    [Fact]
    public async Task POST_PasskeyBegin_UsesFixedAudienceAndHidesFlowId()
    {
        using var client = CreateClient();

        using var response = await client.PostAsJsonAsync("/auth/passkey/begin", new
        {
            returnUrl = "https://attacker.example/steal"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        var publicKey = payload.GetProperty("publicKey");
        Assert.Equal("maliev.test", publicKey.GetProperty("rpId").GetString());
        Assert.Equal(Challenge, publicKey.GetProperty("challenge").GetString());
        Assert.Equal("AQ", publicKey.GetProperty("allowCredentials")[0].GetProperty("id").GetString());
        Assert.Equal("required", publicKey.GetProperty("userVerification").GetString());
        Assert.Equal(300_000, publicKey.GetProperty("timeout").GetInt32());
        Assert.False(payload.TryGetProperty("flowId", out _));
        Assert.False(payload.TryGetProperty("flow_id", out _));

        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"));
        Assert.Contains("maliev.passkey.flow=", cookie, StringComparison.Ordinal);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/auth/passkey", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("max-age=", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(IssuedFlowId, cookie, StringComparison.Ordinal);

        Assert.Equal("web", _authServiceClient.LastBeginRequest.GetProperty("application").GetString());
        Assert.Single(_authServiceClient.LastBeginRequest.EnumerateObject());
    }

    /// <summary>Verifies non-test deployments require transport security for flow creation and deletion cookies.</summary>
    [Fact]
    public async Task PasskeyFlowCookies_OutsideTesting_AreAlwaysSecure()
    {
        using var productionFactory = _factory.WithWebHostBuilder(builder =>
            builder
                .UseEnvironment("SecurityTest"));
        using var client = productionFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("http://localhost"),
            HandleCookies = true
        });

        using var begin = await client.PostAsJsonAsync(
            "/auth/passkey/begin",
            new { returnUrl = "/account" });

        Assert.Equal(HttpStatusCode.OK, begin.StatusCode);
        var createdFlowCookie = Assert.Single(begin.Headers.GetValues("Set-Cookie"));
        Assert.Contains("secure", createdFlowCookie, StringComparison.OrdinalIgnoreCase);

        using var complete = await client.PostAsJsonAsync(
            "/auth/passkey/complete",
            AssertionRequest());
        var deletedFlowCookie = complete.Headers.GetValues("Set-Cookie")
            .Single(value => value.StartsWith("maliev.passkey.flow=", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("secure", deletedFlowCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("expires=Thu, 01 Jan 1970", deletedFlowCookie, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Verifies a JSON null body is rejected before AuthService allocates a ceremony.</summary>
    [Fact]
    public async Task POST_PasskeyBegin_NullBody_ReturnsBadRequestWithoutAuthServiceCall()
    {
        using var client = CreateClient();
        using var content = new StringContent("null", Encoding.UTF8, "application/json");

        using var response = await client.PostAsync("/auth/passkey/begin", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, _authServiceClient.BeginCount);
    }

    /// <summary>Verifies a JSON null assertion is rejected before protected state is consumed.</summary>
    [Fact]
    public async Task POST_PasskeyComplete_NullBody_ReturnsBadRequestWithoutAuthServiceCall()
    {
        using var client = CreateClient();
        using var content = new StringContent("null", Encoding.UTF8, "application/json");

        using var response = await client.PostAsync("/auth/passkey/complete", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, _authServiceClient.CompleteCount);
    }

    /// <summary>Verifies spoofed forwarding headers cannot evade the per-client ceremony limit.</summary>
    [Fact]
    public async Task POST_PasskeyBegin_ExceedsPerClientLimit_ReturnsTooManyRequestsWithoutDownstreamCall()
    {
        using var rateLimitedFactory = CreateRateLimitedFactory(
            beginPermitLimit: 2,
            completePermitLimit: 10,
            concurrencyPermitLimit: 10);
        using var client = rateLimitedFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        using var firstRequest = CreateBeginRequest("203.0.113.1");
        using var secondRequest = CreateBeginRequest("203.0.113.2", "/auth/passkey/begin/");
        using var rejectedRequest = CreateBeginRequest("203.0.113.3");
        using var first = await client.SendAsync(firstRequest);
        using var second = await client.SendAsync(secondRequest);
        using var rejected = await client.SendAsync(rejectedRequest);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.Equal(2, _authServiceClient.BeginCount);
    }

    /// <summary>Verifies completion attempts are bounded before AuthService is contacted.</summary>
    [Fact]
    public async Task POST_PasskeyComplete_ExceedsPerClientLimit_ReturnsTooManyRequestsWithoutDownstreamCall()
    {
        using var rateLimitedFactory = CreateRateLimitedFactory(
            beginPermitLimit: 10,
            completePermitLimit: 2,
            concurrencyPermitLimit: 10);
        using var client = rateLimitedFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = false
        });

        using var first = await client.PostAsJsonAsync("/auth/passkey/complete", AssertionRequest());
        using var second = await client.PostAsJsonAsync("/auth/passkey/complete", AssertionRequest());
        using var rejected = await client.PostAsJsonAsync("/auth/passkey/complete", AssertionRequest());

        Assert.Equal(HttpStatusCode.BadRequest, first.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.Equal(0, _authServiceClient.CompleteCount);
    }

    /// <summary>Verifies the global passkey concurrency gate rejects work before AuthService fan-out.</summary>
    [Fact]
    public async Task POST_PasskeyBegin_ExceedsGlobalConcurrency_ReturnsTooManyRequestsWithoutDownstreamCall()
    {
        var releaseFirstRequest = new TaskCompletionSource<HttpResponseMessage>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var firstRequestEntered = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        _authServiceClient.BeginAsyncFactory = (_, _) =>
        {
            if (_authServiceClient.BeginCount == 1)
            {
                firstRequestEntered.TrySetResult();
                return releaseFirstRequest.Task;
            }

            return Task.FromResult(FakeAuthServiceClient.CreateSuccessfulBeginResponse());
        };
        using var rateLimitedFactory = CreateRateLimitedFactory(
            beginPermitLimit: 10,
            completePermitLimit: 10,
            concurrencyPermitLimit: 1);
        using var firstClient = rateLimitedFactory.CreateClient();
        using var secondClient = rateLimitedFactory.CreateClient();

        var firstResponseTask = firstClient.PostAsJsonAsync(
            "/auth/passkey/begin",
            new { returnUrl = "/account" });
        await firstRequestEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        using var rejected = await secondClient.PostAsJsonAsync(
            "/auth/passkey/begin",
            new { returnUrl = "/account" });

        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.Equal(1, _authServiceClient.BeginCount);

        releaseFirstRequest.SetResult(FakeAuthServiceClient.CreateSuccessfulBeginResponse());
        using var first = await firstResponseTask;
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
    }

    /// <summary>Verifies only server-owned flow state can produce a canonical customer session.</summary>
    [Fact]
    public async Task POST_PasskeyComplete_InjectsServerStateAndUsesCanonicalCustomerIdentity()
    {
        using var client = CreateClient();
        await BeginAsync(client, "/account/orders");

        using var response = await client.PostAsJsonAsync(
            "/auth/passkey/complete",
            AssertionRequest());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("/account/orders", payload.GetProperty("redirectUrl").GetString());

        var exchange = _authServiceClient.LastCompleteRequest;
        Assert.Equal("web", exchange.GetProperty("application").GetString());
        Assert.Equal(IssuedFlowId, exchange.GetProperty("flow_id").GetString());
        Assert.Equal("AQ", exchange.GetProperty("credential_id").GetString());
        Assert.Equal(new string('A', 50), exchange.GetProperty("authenticator_data").GetString());
        Assert.False(exchange.TryGetProperty("principal_id", out _));
        Assert.False(exchange.TryGetProperty("email", out _));
        Assert.Equal(PrincipalId, _customerServiceClient.LastPrincipalId);

        var identityCookie = response.Headers.GetValues("Set-Cookie")
            .SingleOrDefault(value => value.Contains("Maliev.Identity", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(identityCookie);

        using var sessionResponse = await client.GetAsync("/web/v1/account/session");
        sessionResponse.EnsureSuccessStatusCode();
        var session = await sessionResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(session.GetProperty("isAuthenticated").GetBoolean());
        Assert.Equal(PrincipalId, session.GetProperty("principalId").GetGuid());
        Assert.Equal(CustomerId, session.GetProperty("customerId").GetGuid());
        Assert.Equal(CustomerEmail, session.GetProperty("email").GetString());
        Assert.Equal("Canonical Customer", session.GetProperty("displayName").GetString());
        Assert.Equal("https://cdn.example.test/customer.png", session.GetProperty("profileImageUrl").GetString());
    }

    /// <summary>Verifies the protected browser flow is consumed before downstream completion.</summary>
    [Fact]
    public async Task POST_PasskeyComplete_ReplayFailsBeforeAuthService()
    {
        using var client = CreateClient();
        await BeginAsync(client, "/account");
        var assertion = AssertionRequest();

        using var first = await client.PostAsJsonAsync("/auth/passkey/complete", assertion);
        using var replay = await client.PostAsJsonAsync("/auth/passkey/complete", assertion);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, replay.StatusCode);
        Assert.Equal(1, _authServiceClient.CompleteCount);
        Assert.Equal(1, _customerServiceClient.LookupCount);
    }

    /// <summary>Verifies malformed assertions do not consume an otherwise live protected flow.</summary>
    [Fact]
    public async Task POST_PasskeyComplete_OversizedAssertionIsRejectedWithoutBurningFlow()
    {
        using var client = CreateClient();
        await BeginAsync(client, "/account");

        using var malformed = await client.PostAsJsonAsync("/auth/passkey/complete", new
        {
            credentialId = "AQ",
            authenticatorData = new string('A', 50),
            clientDataJson = new string('A', 10_925),
            signature = "AQ",
            userHandle = "AQ"
        });
        using var validShape = await client.PostAsJsonAsync(
            "/auth/passkey/complete",
            AssertionRequest());

        Assert.Equal(HttpStatusCode.BadRequest, malformed.StatusCode);
        Assert.Equal(HttpStatusCode.OK, validShape.StatusCode);
        Assert.Equal(1, _authServiceClient.CompleteCount);
    }

    /// <summary>Verifies an invalid AuthService assertion never reaches CustomerService or session issuance.</summary>
    [Fact]
    public async Task POST_PasskeyComplete_InvalidAssertionDoesNotIssueSession()
    {
        _authServiceClient.CompleteResponseFactory = () => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = JsonContent.Create(new { error = "authentication_failed" })
        };
        using var client = CreateClient();
        await BeginAsync(client, "/account");

        using var response = await client.PostAsJsonAsync(
            "/auth/passkey/complete",
            AssertionRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(0, _customerServiceClient.LookupCount);
        Assert.False(HasIdentityCookie(response));
    }

    /// <summary>Verifies expired AuthService options never create browser flow state.</summary>
    [Fact]
    public async Task POST_PasskeyBegin_ExpiredOptionsDoNotCreateFlowCookie()
    {
        _authServiceClient.BeginResponseFactory = () => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                flow_id = IssuedFlowId,
                expires_at_utc = DateTimeOffset.UtcNow.AddSeconds(-1),
                rp_id = "maliev.test",
                challenge = Challenge,
                allow_credentials = Array.Empty<object>(),
                user_verification = "required",
                timeout = 300_000
            })
        };
        using var client = CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/auth/passkey/begin",
            new { returnUrl = "/account" });

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.False(response.Headers.TryGetValues("Set-Cookie", out var cookies) &&
            cookies.Any(value => value.Contains("maliev.passkey.flow", StringComparison.Ordinal)));
    }

    /// <summary>Verifies incomplete verified identity cannot reach CustomerService.</summary>
    [Fact]
    public async Task POST_PasskeyComplete_IncompleteVerifiedIdentityDoesNotIssueSession()
    {
        _authServiceClient.CompleteResponseFactory = () => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                success = true,
                error = (string?)null,
                principal_id = (Guid?)null,
                email = CustomerEmail
            })
        };
        using var client = CreateClient();
        await BeginAsync(client, "/account");

        using var response = await client.PostAsJsonAsync(
            "/auth/passkey/complete",
            AssertionRequest());

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.Equal(0, _customerServiceClient.LookupCount);
        Assert.False(HasIdentityCookie(response));
    }

    /// <summary>Verifies an unavailable canonical customer lookup returns a retryable failure.</summary>
    [Fact]
    public async Task POST_PasskeyComplete_CustomerServiceUnavailableDoesNotIssueSession()
    {
        _customerServiceClient.ResponseFactory = () =>
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        using var client = CreateClient();
        await BeginAsync(client, "/account");

        using var response = await client.PostAsJsonAsync(
            "/auth/passkey/complete",
            AssertionRequest());

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.False(HasIdentityCookie(response));
    }

    /// <summary>Verifies a non-active portal account cannot exchange a retained passkey for a session.</summary>
    [Theory]
    [InlineData("Disabled")]
    [InlineData("InvitationPending")]
    public async Task POST_PasskeyComplete_NonActiveAccountDoesNotIssueSession(string accountStatus)
    {
        _customerServiceClient.AccountStatus = accountStatus;
        using var client = CreateClient();
        await BeginAsync(client, "/account");

        using var response = await client.PostAsJsonAsync(
            "/auth/passkey/complete",
            AssertionRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(HasIdentityCookie(response));
    }

    /// <summary>Verifies incomplete account security metadata cannot produce a session.</summary>
    [Fact]
    public async Task POST_PasskeyComplete_MissingAccountVerificationStateDoesNotIssueSession()
    {
        _customerServiceClient.AccountEmailVerified = null;
        using var client = CreateClient();
        await BeginAsync(client, "/account");

        using var response = await client.PostAsJsonAsync(
            "/auth/passkey/complete",
            AssertionRequest());

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.False(HasIdentityCookie(response));
    }

    /// <summary>Verifies AuthService and CustomerService identity mismatches fail closed.</summary>
    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public async Task POST_PasskeyComplete_CanonicalIdentityMismatchDoesNotIssueSession(
        bool principalMismatch,
        bool customerEmailMismatch,
        bool accountEmailMismatch)
    {
        _customerServiceClient.PrincipalId = principalMismatch ? Guid.NewGuid() : PrincipalId;
        _customerServiceClient.CustomerEmail = customerEmailMismatch
            ? "different@example.test"
            : CustomerEmail;
        _customerServiceClient.AccountEmail = accountEmailMismatch
            ? "different-account@example.test"
            : CustomerEmail;
        using var client = CreateClient();
        await BeginAsync(client, "/account");

        using var response = await client.PostAsJsonAsync(
            "/auth/passkey/complete",
            AssertionRequest());

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.False(HasIdentityCookie(response));
    }

    /// <summary>Verifies the browser sends raw WebAuthn bytes and never supplies trusted boundary fields.</summary>
    [Fact]
    public void PasskeyBrowserAdapter_UsesRawBytesAndFixedSameOriginEndpoints()
    {
        var root = FindRepoRoot();
        var script = File.ReadAllText(Path.Combine(root, "Maliev.Web.Bff", "wwwroot", "js", "maliev-passkey.js"));
        var app = File.ReadAllText(Path.Combine(root, "Maliev.Web.Bff", "Components", "App.razor"));
        var clients = File.ReadAllText(Path.Combine(root, "Maliev.Web.Bff", "Clients", "CheckoutBoundaryClients.cs"));

        Assert.Contains("/auth/passkey/begin", script, StringComparison.Ordinal);
        Assert.Contains("/auth/passkey/complete", script, StringComparison.Ordinal);
        Assert.Contains("base64urlToArrayBuffer(credential.id)", script, StringComparison.Ordinal);
        Assert.Contains("arrayBufferToBase64url(assertion.response.clientDataJSON)", script, StringComparison.Ordinal);
        Assert.DoesNotContain("TextDecoder", script, StringComparison.Ordinal);
        Assert.DoesNotContain("principalId", script, StringComparison.Ordinal);
        Assert.DoesNotContain("flowId", script, StringComparison.Ordinal);
        Assert.DoesNotContain("application:", script, StringComparison.Ordinal);
        Assert.DoesNotContain("\"application\"", script, StringComparison.Ordinal);
        Assert.Contains("js/maliev-passkey.js", app, StringComparison.Ordinal);
        Assert.Contains("/auth/v2/passkey/auth/begin", clients, StringComparison.Ordinal);
        Assert.Contains("/auth/v2/passkey/auth/complete", clients, StringComparison.Ordinal);
        Assert.Contains(
            "/customer/v1/customers/by-principal/{principalId}/authentication-context",
            clients,
            StringComparison.Ordinal);
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = true
    });

    private WebApplicationFactory<Program> CreateRateLimitedFactory(
        int beginPermitLimit,
        int completePermitLimit,
        int concurrencyPermitLimit) =>
        _factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Security:PasskeyRateLimiting:BeginPermitLimit"] = beginPermitLimit.ToString(),
                ["Security:PasskeyRateLimiting:CompletePermitLimit"] = completePermitLimit.ToString(),
                ["Security:PasskeyRateLimiting:WindowSeconds"] = "60",
                ["Security:PasskeyRateLimiting:ConcurrencyPermitLimit"] = concurrencyPermitLimit.ToString()
            })));

    private static HttpRequestMessage CreateBeginRequest(
        string spoofedForwardedFor,
        string path = "/auth/passkey/begin")
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(new { returnUrl = "/account" })
        };
        request.Headers.TryAddWithoutValidation("X-Forwarded-For", spoofedForwardedFor);
        return request;
    }

    private static object AssertionRequest() => new
    {
        credentialId = "AQ",
        authenticatorData = new string('A', 50),
        clientDataJson = "e30",
        signature = "AQ",
        userHandle = "AQ"
    };

    private static bool HasIdentityCookie(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Set-Cookie", out var values) &&
        values.Any(value => value.Contains("Maliev.Identity", StringComparison.OrdinalIgnoreCase));

    private static string ToBase64Url(byte[] value) =>
        Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static async Task BeginAsync(HttpClient client, string returnUrl)
    {
        using var response = await client.PostAsJsonAsync(
            "/auth/passkey/begin",
            new { returnUrl });
        response.EnsureSuccessStatusCode();
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
        public JsonElement LastBeginRequest { get; private set; }
        public JsonElement LastCompleteRequest { get; private set; }
        public int BeginCount { get; private set; }
        public int CompleteCount { get; private set; }
        public Func<HttpResponseMessage>? BeginResponseFactory { get; set; }
        public Func<object, CancellationToken, Task<HttpResponseMessage>>? BeginAsyncFactory { get; set; }
        public Func<HttpResponseMessage>? CompleteResponseFactory { get; set; }

        public Task<HttpResponseMessage> PasskeyAuthBeginAsync(object request, CancellationToken ct)
        {
            LastBeginRequest = JsonSerializer.SerializeToElement(request);
            BeginCount++;
            return BeginAsyncFactory?.Invoke(request, ct) ??
                Task.FromResult(BeginResponseFactory?.Invoke() ?? CreateSuccessfulBeginResponse());
        }

        public static HttpResponseMessage CreateSuccessfulBeginResponse() =>
            new(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    flow_id = IssuedFlowId,
                    expires_at_utc = DateTimeOffset.UtcNow.AddMinutes(5),
                    rp_id = "maliev.test",
                    challenge = Challenge,
                    allow_credentials = new[] { new { type = "public-key", id = "AQ" } },
                    user_verification = "required",
                    timeout = 300_000
                })
            };

        public Task<HttpResponseMessage> PasskeyAuthCompleteAsync(object request, CancellationToken ct)
        {
            LastCompleteRequest = JsonSerializer.SerializeToElement(request);
            CompleteCount++;
            return Task.FromResult(CompleteResponseFactory?.Invoke() ?? new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    success = true,
                    error = (string?)null,
                    principal_id = PrincipalId,
                    email = CustomerEmail.ToUpperInvariant()
                })
            });
        }

        public Task<HttpResponseMessage> LoginAsync(object request, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<HttpResponseMessage> IssueCustomerGoogleNonceAsync(object request, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<HttpResponseMessage> ExchangeCustomerGoogleAsync(object request, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<HttpResponseMessage> RequestPasswordResetAsync(object request, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<HttpResponseMessage> ConfirmPasswordResetAsync(object request, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<HttpResponseMessage> InitiateEmailVerificationAsync(object request, CancellationToken ct) => throw new NotSupportedException();
        public Task<HttpResponseMessage> VerifyEmailAsync(object request, CancellationToken ct) => throw new NotSupportedException();
        public Task<HttpResponseMessage> ResendVerificationEmailAsync(object request, CancellationToken ct) => throw new NotSupportedException();
        public Task<HttpResponseMessage> GetCurrentPrincipalAsync(Guid principalId, CancellationToken ct) => throw new NotSupportedException();
        public Task<HttpResponseMessage> PasskeyRegisterBeginAsync(object request, CancellationToken ct) => throw new NotSupportedException();
        public Task<HttpResponseMessage> PasskeyRegisterCompleteAsync(object request, CancellationToken ct) => throw new NotSupportedException();
        public Task<HttpResponseMessage> ListPasskeyCredentialsAsync(Guid principalId, CancellationToken ct) => throw new NotSupportedException();
        public Task<HttpResponseMessage> DeletePasskeyCredentialAsync(Guid credentialId, Guid principalId, CancellationToken ct) => throw new NotSupportedException();
    }

    private sealed class FakeCustomerServiceClient : ICustomerServiceClient
    {
        public int LookupCount { get; private set; }
        public Guid LastPrincipalId { get; private set; }
        public Guid PrincipalId { get; set; } = PasskeyAuthenticationFlowTests.PrincipalId;
        public string CustomerEmail { get; set; } = PasskeyAuthenticationFlowTests.CustomerEmail;
        public string AccountEmail { get; set; } = PasskeyAuthenticationFlowTests.CustomerEmail;
        public string AccountStatus { get; set; } = "Active";
        public bool? AccountEmailVerified { get; set; } = true;
        public Func<HttpResponseMessage>? ResponseFactory { get; set; }

        public Task<HttpResponseMessage> GetCustomerByPrincipalIdAsync(
            Guid principalId,
            CancellationToken cancellationToken)
        {
            LastPrincipalId = principalId;
            LookupCount++;
            return Task.FromResult(ResponseFactory?.Invoke() ?? new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    customerId = CustomerId,
                    principalId = PrincipalId,
                    firstName = "Canonical",
                    lastName = "Customer",
                    name = "Canonical Customer",
                    customerEmail = CustomerEmail,
                    accountEmail = AccountEmail,
                    accountStatus = AccountStatus,
                    accountEmailVerified = AccountEmailVerified,
                    profileImageUrl = "https://cdn.example.test/customer.png"
                })
            });
        }

        public Task<HttpResponseMessage> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<HttpResponseMessage> UpdateCustomerAsync(Guid customerId, object request, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<HttpResponseMessage> GetCompanyAsync(Guid companyId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<HttpResponseMessage> CreateCompanyAsync(object request, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<HttpResponseMessage> UpdateCompanyAsync(Guid companyId, object request, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<HttpResponseMessage> RegisterCustomerAsync(object request, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<HttpResponseMessage> GetCustomerAddressesAsync(Guid customerId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<HttpResponseMessage> CreateCustomerAddressAsync(object request, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<HttpResponseMessage> UpdateCustomerAddressAsync(Guid addressId, object request, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<HttpResponseMessage> DeleteCustomerAddressAsync(Guid addressId, object request, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
