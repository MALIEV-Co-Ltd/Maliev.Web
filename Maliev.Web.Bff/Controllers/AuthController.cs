using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Maliev.Web.Bff.Clients;
using Maliev.Web.Bff.Security;
using Maliev.Web.Shared.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.Web.Bff.Controllers;

/// <summary>
/// Handles customer authentication browser flows for the public web app.
/// </summary>
[Route("auth")]
public sealed class AuthController(
    IAuthServiceClient authClient,
    ICustomerServiceClient customerClient,
    IConfiguration configuration,
    CustomerSessionHandoffToken sessionHandoffToken,
    ILogger<AuthController> logger) : Controller
{
    private const string ExternalScheme = "MalievExternal";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly string[] CustomerAccountPermissions =
    [
        "customer.profile.read",
        "customer.profile.write",
        "customer.addresses.manage",
        "order.orders.read"
    ];

    /// <summary>
    /// Starts customer Google sign-in.
    /// </summary>
    [HttpGet("google")]
    [AllowAnonymous]
    public IActionResult Google([FromQuery] string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientId"]) ||
            string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientSecret"]))
        {
            return RedirectWithError("/auth/sign-in", "Google sign-in is not configured yet.");
        }

        var redirect = Url.Action(nameof(GoogleCallback), new { returnUrl = NormalizeReturnUrl(returnUrl) })!;
        return Challenge(new AuthenticationProperties { RedirectUri = redirect }, GoogleDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Completes customer Google sign-in after Google validates the browser identity.
    /// </summary>
    [HttpGet("google/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleCallback([FromQuery] string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        var external = await HttpContext.AuthenticateAsync(ExternalScheme);
        if (!external.Succeeded || external.Principal is null)
        {
            return RedirectWithError("/auth/sign-in", "Google sign-in could not be completed.");
        }

        var email = external.Principal.FindFirstValue(ClaimTypes.Email);
        var name = external.Principal.FindFirstValue(ClaimTypes.Name) ?? email;
        var googleUserId = external.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var profileImageUrl = GetExternalProfileImageUrl(external.Principal);

        await HttpContext.SignOutAsync(ExternalScheme);

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(googleUserId))
        {
            return RedirectWithError("/auth/sign-in", "Google did not return a verified customer identity.");
        }

        using var response = await authClient.ExchangeCustomerGoogleAsync(new
        {
            email,
            full_name = name,
            google_user_id = googleUserId,
            email_verified = true,
            profile_image_url = profileImageUrl,
            preferred_language = Request.Cookies["maliev.culture"] ?? "th",
            timezone = "Asia/Bangkok"
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Customer Google exchange failed with status {StatusCode}", response.StatusCode);
            return RedirectWithError("/auth/sign-in", "Google sign-in could not create a MALIEV customer session.");
        }

        var session = await response.Content.ReadFromJsonAsync<AuthLoginResponse>(JsonOptions, cancellationToken);
        if (session?.User is null)
        {
            return RedirectWithError("/auth/sign-in", "Google sign-in returned an incomplete customer session.");
        }

        await SignInCustomerAsync(session.User);
        return LocalRedirect(NormalizeReturnUrl(returnUrl));
    }

    /// <summary>
    /// Creates a short-lived customer session handoff and redirects to the QuoteEngine.
    /// </summary>
    [HttpGet("quote-engine")]
    [Authorize(Policy = WebAuthorizationPolicies.CustomerAccount)]
    public IActionResult QuoteEngine([FromQuery] string? returnUrl = null)
    {
        if (!Guid.TryParse(User.FindFirstValue("customer_id"), out var customerId))
        {
            return RedirectWithError("/auth/sign-in", "Sign in again before opening the quote portal.");
        }

        var now = DateTimeOffset.UtcNow;
        var token = sessionHandoffToken.Create(new CustomerSessionHandoffPayload(
            customerId,
            User.FindFirstValue("principal_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier),
            User.FindFirstValue(ClaimTypes.Email),
            User.FindFirstValue(ClaimTypes.Name),
            now,
            now.AddMinutes(2)));
        var quoteEngineUrl = ResolveQuoteEngineUrl();
        var quoteReturnUrl = NormalizeQuoteEngineReturnUrl(returnUrl);
        var redirect = $"{quoteEngineUrl}/auth/web-handoff?token={Uri.EscapeDataString(token)}&returnUrl={Uri.EscapeDataString(quoteReturnUrl)}";
        return Redirect(redirect);
    }

    /// <summary>
    /// Signs a customer in with email and password.
    /// </summary>
    [HttpPost("sign-in/email")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignIn([FromForm] SignInForm form, CancellationToken cancellationToken)
    {
        using var response = await authClient.LoginAsync(new
        {
            username = form.Email,
            password = form.Password,
            user_type = "customer"
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return RedirectWithError($"/auth/sign-in?returnUrl={Uri.EscapeDataString(NormalizeReturnUrl(form.ReturnUrl))}", "Email or password is incorrect.");
        }

        var session = await response.Content.ReadFromJsonAsync<AuthLoginResponse>(JsonOptions, cancellationToken);
        if (session?.User is null)
        {
            return RedirectWithError("/auth/sign-in", "Sign-in returned an incomplete customer session.");
        }

        await SignInCustomerAsync(session.User);
        return LocalRedirect(NormalizeReturnUrl(form.ReturnUrl));
    }

    /// <summary>
    /// Registers a customer account and signs the customer in.
    /// </summary>
    [HttpPost("sign-up/email")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignUp([FromForm] SignUpForm form, CancellationToken cancellationToken)
    {
        using var register = await customerClient.RegisterCustomerAsync(new
        {
            email = form.Email,
            firstName = form.FirstName,
            lastName = form.LastName,
            password = form.Password,
            registrationMethod = "Email",
            preferredLanguage = Request.Cookies["maliev.culture"] ?? "th",
            timezone = "Asia/Bangkok"
        }, cancellationToken);

        if (register.StatusCode == HttpStatusCode.Conflict || !register.IsSuccessStatusCode)
        {
            return RedirectWithError("/auth/sign-up", "This email cannot be registered. It may already have a MALIEV account.");
        }

        return await SignIn(new SignInForm
        {
            Email = form.Email,
            Password = form.Password,
            ReturnUrl = NormalizeReturnUrl(form.ReturnUrl)
        }, cancellationToken);
    }

    /// <summary>
    /// Starts the password reset flow for a customer account.
    /// </summary>
    [HttpPost("forgot-password/request")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword([FromForm] ForgotPasswordForm form, CancellationToken cancellationToken)
    {
        using var response = await authClient.RequestPasswordResetAsync(new { email = form.Email }, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return RedirectWithError("/auth/forgot-password", "We could not start password reset right now.");
        }

        return Redirect("/auth/forgot-password?status=Password reset instructions were sent if the account exists.");
    }

    /// <summary>
    /// Confirms a password reset token for a customer account.
    /// </summary>
    [HttpPost("reset-password/confirm")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordForm form, CancellationToken cancellationToken)
    {
        using var response = await authClient.ConfirmPasswordResetAsync(new
        {
            email = form.Email,
            token = form.Token,
            new_password = form.Password
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return RedirectWithError("/auth/reset-password", "This reset link is invalid or expired.");
        }

        return Redirect("/auth/sign-in?status=Password reset complete. Sign in with your new password.");
    }

    /// <summary>
    /// Signs the current customer out.
    /// </summary>
    [HttpPost("sign-out")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignOutCustomer()
    {
        if (User.Identity is { IsAuthenticated: true })
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
        return Redirect("/");
    }

    private async Task SignInCustomerAsync(AuthUser user)
    {
        var principalId = user.PrincipalId ?? user.UserId;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, principalId),
            new("principal_id", principalId),
            new("user_type", "customer")
        };

        if (!string.IsNullOrWhiteSpace(user.CustomerId))
        {
            claims.Add(new Claim("customer_id", user.CustomerId));
        }

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        if (!string.IsNullOrWhiteSpace(user.Name))
        {
            claims.Add(new Claim(ClaimTypes.Name, user.Name));
        }

        if (!string.IsNullOrWhiteSpace(user.ProfileImageUrl))
        {
            claims.Add(new Claim("profile_image_url", user.ProfileImageUrl));
        }

        foreach (var permission in CustomerAccountPermissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
            });
    }

    private IActionResult RedirectWithError(string path, string error)
    {
        var separator = path.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        return Redirect($"{path}{separator}error={Uri.EscapeDataString(error)}");
    }

    private string NormalizeReturnUrl(string? returnUrl)
    {
        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : "/account";
    }

    private string ResolveQuoteEngineUrl()
    {
        var configured = configuration["QuoteEngine:BaseUrl"]
            ?? configuration["QuoteEngine__BaseUrl"]
            ?? Environment.GetEnvironmentVariable("QuoteEngine__BaseUrl")
            ?? Environment.GetEnvironmentVariable("QUOTEENGINE_BASE_URL")
            ?? "https://quote.maliev.com";
        return configured.TrimEnd('/');
    }

    private static string NormalizeQuoteEngineReturnUrl(string? returnUrl)
    {
        return !string.IsNullOrWhiteSpace(returnUrl) &&
            returnUrl.StartsWith("/", StringComparison.Ordinal) &&
            !returnUrl.StartsWith("//", StringComparison.Ordinal)
            ? returnUrl
            : "/projects/new";
    }

    private static string? GetExternalProfileImageUrl(ClaimsPrincipal principal)
    {
        return principal.FindFirstValue("picture")
            ?? principal.FindFirstValue("urn:google:picture")
            ?? principal.FindFirstValue("profile_image_url");
    }

    /// <summary>
    /// Posted email/password sign-in form.
    /// </summary>
    public class SignInForm
    {
        /// <summary>Customer email.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Customer password.</summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>Local return URL.</summary>
        public string? ReturnUrl { get; set; }
    }

    /// <summary>
    /// Posted email/password sign-up form.
    /// </summary>
    public sealed class SignUpForm : SignInForm
    {
        /// <summary>Customer first name.</summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>Customer last name.</summary>
        public string LastName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Posted password reset request form.
    /// </summary>
    public class ForgotPasswordForm
    {
        /// <summary>Customer email.</summary>
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Posted password reset confirmation form.
    /// </summary>
    public sealed class ResetPasswordForm : ForgotPasswordForm
    {
        /// <summary>Password reset token.</summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>New password.</summary>
        public string Password { get; set; } = string.Empty;
    }

    private sealed class AuthLoginResponse
    {
        [JsonPropertyName("user")]
        public AuthUser? User { get; set; }
    }

    private sealed class AuthUser
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("principal_id")]
        public string? PrincipalId { get; set; }

        [JsonPropertyName("customer_id")]
        public string? CustomerId { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("profile_image_url")]
        public string? ProfileImageUrl { get; set; }
    }
}
