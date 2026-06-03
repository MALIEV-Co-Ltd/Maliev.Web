using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Maliev.Web.Bff.Clients;
using Maliev.Web.Bff.Controllers;
using Maliev.Web.Client.Services;
using Maliev.Web.Shared.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.Web.Tests;

/// <summary>
/// Contract tests for the session and profile data that drives contact page auto-fill.
/// </summary>
public sealed class ContactPageAutoFillTests
{
    /// <summary>
    /// Verifies the session endpoint returns all profile fields the contact page uses to populate the profile card.
    /// </summary>
    [Fact]
    public void GetSession_AuthenticatedUser_ReturnsIsAuthenticatedWithDisplayNameEmailAndProfileImage()
    {
        var customerId = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000001");
        var principalId = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000002");
        var controller = CreateSessionController(
        [
            new Claim(ClaimTypes.NameIdentifier, principalId.ToString()),
            new Claim("principal_id", principalId.ToString()),
            new Claim("customer_id", customerId.ToString()),
            new Claim(ClaimTypes.Name, "Somchai Maliev"),
            new Claim(ClaimTypes.Email, "somchai@example.com"),
            new Claim("profile_image_url", "https://example.com/avatar.jpg"),
            new Claim("user_type", "customer")
        ]);

        var result = controller.GetSession();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var session = Assert.IsType<CustomerAccountSessionDto>(ok.Value);
        Assert.True(session.IsAuthenticated);
        Assert.Equal("Somchai Maliev", session.DisplayName);
        Assert.Equal("somchai@example.com", session.Email);
        Assert.Equal("https://example.com/avatar.jpg", session.ProfileImageUrl);
        Assert.Equal(customerId, session.CustomerId);
        Assert.Equal(principalId, session.PrincipalId);
    }

    /// <summary>
    /// Verifies the session endpoint returns a non-authenticated session for anonymous users
    /// so the contact form shows the standard empty fields.
    /// </summary>
    [Fact]
    public void GetSession_AnonymousUser_ReturnsIsAuthenticatedFalseWithEmptyFields()
    {
        var controller = new AccountController(new StubCustomerServiceClient(), new StubCountryServiceClient(), new StubRegistryServiceClient(), new StubAuthServiceClient())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = controller.GetSession();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var session = Assert.IsType<CustomerAccountSessionDto>(ok.Value);
        Assert.False(session.IsAuthenticated);
        Assert.Equal(string.Empty, session.DisplayName);
        Assert.Equal(string.Empty, session.Email);
        Assert.Null(session.ProfileImageUrl);
        Assert.Null(session.CustomerId);
    }

    /// <summary>
    /// Verifies the session endpoint falls back to email as the display name when the Name claim is absent,
    /// so the contact page can still prefill the FullName field from the session baseline.
    /// </summary>
    [Fact]
    public void GetSession_AuthenticatedUserWithoutNameClaim_FallsBackToEmailAsDisplayName()
    {
        var controller = CreateSessionController(
        [
            new Claim(ClaimTypes.Email, "noname@example.com"),
            new Claim("customer_id", Guid.NewGuid().ToString()),
            new Claim("user_type", "customer")
        ]);

        var result = controller.GetSession();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var session = Assert.IsType<CustomerAccountSessionDto>(ok.Value);
        Assert.True(session.IsAuthenticated);
        Assert.Equal("noname@example.com", session.DisplayName);
        Assert.Equal("noname@example.com", session.Email);
    }

    /// <summary>
    /// Verifies the API client correctly deserialises an authenticated session response.
    /// </summary>
    [Fact]
    public async Task GetAccountSessionAsync_AuthenticatedSessionResponse_ReturnsPopulatedSession()
    {
        using var client = new HttpClient(new StubHandler(request =>
        {
            Assert.Equal("/web/v1/account/session", request.RequestUri?.AbsolutePath);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    isAuthenticated = true,
                    displayName = "Somchai Maliev",
                    email = "somchai@example.com",
                    profileImageUrl = "https://example.com/avatar.jpg"
                })
            };
        })) { BaseAddress = new Uri("https://web.test/") };
        var api = new MalievApiClient(client);

        var session = await api.GetAccountSessionAsync();

        Assert.True(session.IsAuthenticated);
        Assert.Equal("Somchai Maliev", session.DisplayName);
        Assert.Equal("somchai@example.com", session.Email);
        Assert.Equal("https://example.com/avatar.jpg", session.ProfileImageUrl);
    }

    /// <summary>
    /// Verifies the API client returns a non-authenticated session for anonymous users
    /// so the contact page can determine it should not attempt a profile load.
    /// </summary>
    [Fact]
    public async Task GetAccountSessionAsync_AnonymousSessionResponse_ReturnsIsAuthenticatedFalse()
    {
        using var client = new HttpClient(new StubHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new { isAuthenticated = false, displayName = "", email = "" })
            })) { BaseAddress = new Uri("https://web.test/") };
        var api = new MalievApiClient(client);

        var session = await api.GetAccountSessionAsync();

        Assert.False(session.IsAuthenticated);
        Assert.Equal(string.Empty, session.DisplayName);
        Assert.Equal(string.Empty, session.Email);
    }

    /// <summary>
    /// Verifies the API client throws MalievApiException on server errors, confirming
    /// the contact page's outer try/catch is the correct fallback boundary.
    /// </summary>
    [Fact]
    public async Task GetAccountSessionAsync_ServerError_ThrowsMalievApiException()
    {
        using var client = new HttpClient(new StubHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.InternalServerError)))
        { BaseAddress = new Uri("https://web.test/") };
        var api = new MalievApiClient(client);

        await Assert.ThrowsAsync<MalievApiException>(() => api.GetAccountSessionAsync());
    }

    private static AccountController CreateSessionController(IEnumerable<Claim> claims)
    {
        return new AccountController(new StubCustomerServiceClient(), new StubCountryServiceClient(), new StubRegistryServiceClient(), new StubAuthServiceClient())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
                }
            }
        };
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(respond(request));
    }

    private sealed class StubCustomerServiceClient : ICustomerServiceClient
    {
        public Task<HttpResponseMessage> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> UpdateCustomerAsync(Guid customerId, object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> GetCompanyAsync(Guid companyId, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> CreateCompanyAsync(object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created));

        public Task<HttpResponseMessage> UpdateCompanyAsync(Guid companyId, object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> RegisterCustomerAsync(object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> GetCustomerAddressesAsync(Guid customerId, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> CreateCustomerAddressAsync(object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> UpdateCustomerAddressAsync(Guid addressId, object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> DeleteCustomerAddressAsync(Guid addressId, object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> GetCustomerByPrincipalIdAsync(Guid principalId, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }

    private sealed class StubCountryServiceClient : ICountryServiceClient
    {
        public Task<HttpResponseMessage> GetCountryByIso2Async(string iso2, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> GetCountriesAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }

    private sealed class StubRegistryServiceClient : IRegistryServiceClient
    {
        public Task<HttpResponseMessage> SearchThaiLocationsAsync(string query, int limit, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> SearchCompaniesAsync(string query, int limit, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }

    private sealed class StubAuthServiceClient : IAuthServiceClient
    {
        public Task<HttpResponseMessage> LoginAsync(object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> ExchangeCustomerGoogleAsync(object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> RequestPasswordResetAsync(object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> ConfirmPasswordResetAsync(object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> InitiateEmailVerificationAsync(object request, CancellationToken ct) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> VerifyEmailAsync(object request, CancellationToken ct) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> ResendVerificationEmailAsync(object request, CancellationToken ct) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> GetCurrentPrincipalAsync(Guid principalId, CancellationToken ct) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> PasskeyRegisterBeginAsync(object request, CancellationToken ct) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> PasskeyRegisterCompleteAsync(object request, CancellationToken ct) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> PasskeyAuthBeginAsync(object request, CancellationToken ct) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> PasskeyAuthCompleteAsync(object request, CancellationToken ct) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> ListPasskeyCredentialsAsync(Guid principalId, CancellationToken ct) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> DeletePasskeyCredentialAsync(Guid credentialId, Guid principalId, CancellationToken ct) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }
}
