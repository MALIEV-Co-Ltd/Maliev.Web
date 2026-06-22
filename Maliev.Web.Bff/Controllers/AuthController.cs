using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Maliev.Web.Bff.Clients;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maliev.Web.Shared.Security;

namespace Maliev.Web.Bff.Controllers;

/// <summary>
/// Handles customer authentication browser flows for the public web app.
/// </summary>
[Route("auth")]
public sealed class AuthController(
    IAuthServiceClient authClient,
    ICustomerServiceClient customerClient,
    IConfiguration configuration,
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
            preferred_language = GetLanguageCode(Request.Cookies["maliev.culture"]),
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

        session.User.EmailVerified = true;
        await SignInCustomerAsync(session.User);
        return RedirectToReturnUrl(returnUrl);
    }

    /// <summary>
    /// Sends the user to the QuoteEngine. The shared identity cookie means no explicit
    /// handoff token is needed because the session is already valid on Make Studio.
    /// </summary>
    [HttpGet("/quote/start")]
    [AllowAnonymous]
    public IActionResult QuoteStart([FromQuery] string? returnUrl = null)
    {
        var quoteEngineUrl = ResolveQuoteEngineUrl().TrimEnd('/');
        var quoteReturnUrl = NormalizeQuoteEngineReturnUrl(returnUrl);
        return Redirect($"{quoteEngineUrl}{quoteReturnUrl}");
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

        session.User.EmailVerified = false;
        await SignInCustomerAsync(session.User);
        return RedirectToReturnUrl(form.ReturnUrl);
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
            firstName = ResolveSignUpFirstName(form),
            lastName = ResolveSignUpLastName(form),
            password = form.Password,
            registrationMethod = "Email",
            preferredLanguage = GetLanguageCode(Request.Cookies["maliev.culture"]),
            timezone = "Asia/Bangkok"
        }, cancellationToken);

        if (register.StatusCode == HttpStatusCode.Conflict || !register.IsSuccessStatusCode)
        {
            return RedirectWithError($"/auth/sign-up?returnUrl={Uri.EscapeDataString(NormalizeReturnUrl(form.ReturnUrl))}", "This email cannot be registered. It may already have a MALIEV account.");
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

    /// <summary>
    /// Handles the email verification link callback from verification emails.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmailCallback([FromQuery] string token, CancellationToken ct)
    {
        var response = await authClient.VerifyEmailAsync(new { Token = token }, ct);

        if (!response.IsSuccessStatusCode)
            return RedirectWithError("/auth/sign-in", "Invalid or expired verification link");

        if (User.Identity?.IsAuthenticated == true)
            return Redirect("/auth/refresh-claims");

        return Redirect("/auth/sign-in?status=Email verified successfully");
    }

    /// <summary>
    /// Refreshes the cookie claims from AuthService after email verification or passkey auth.
    /// </summary>
    [Authorize(Policy = WebAuthorizationPolicies.CustomerAccount)]
    [HttpGet("refresh-claims")]
    public async Task<IActionResult> RefreshClaims(CancellationToken ct)
    {
        var principalIdClaim = User.FindFirst("principal_id")?.Value;
        if (!Guid.TryParse(principalIdClaim, out var principalId))
            return Redirect("/auth/sign-in");

        var response = await authClient.GetCurrentPrincipalAsync(principalId, ct);
        if (!response.IsSuccessStatusCode)
            return Redirect("/");

        var profile = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        var emailVerified = profile.GetProperty("emailVerified").GetBoolean();
        var authEmail = profile.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;

        var identity = (ClaimsIdentity)User.Identity!;

        var existingVerifiedClaim = identity.FindFirst("email_verified");
        if (existingVerifiedClaim is not null)
            identity.TryRemoveClaim(existingVerifiedClaim);
        identity.AddClaim(new Claim("email_verified", emailVerified.ToString().ToLowerInvariant()));

        var currentEmail = identity.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrWhiteSpace(authEmail) && !string.Equals(currentEmail, authEmail, StringComparison.OrdinalIgnoreCase))
        {
            var existingEmailClaim = identity.FindFirst(ClaimTypes.Email);
            if (existingEmailClaim is not null)
                identity.TryRemoveClaim(existingEmailClaim);
            identity.AddClaim(new Claim(ClaimTypes.Email, authEmail));

            var customerIdClaim = identity.FindFirst("customer_id")?.Value;
            if (Guid.TryParse(customerIdClaim, out var customerId))
            {
                try
                {
                    using var _ = await customerClient.UpdateCustomerAsync(customerId, new { email = authEmail }, ct);
                }
                catch
                {
                    // CustomerService sync is best-effort; profile page will re-sync on load
                }
            }
        }

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return Redirect("/account/profile");
    }

    /// <summary>
    /// Resends the verification email for the current user.
    /// </summary>
    [Authorize(Policy = WebAuthorizationPolicies.CustomerAccount)]
    [HttpPost("resend-verification")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendVerificationEmail(CancellationToken ct)
    {
        var principalIdClaim = User.FindFirst("principal_id")?.Value;
        if (!Guid.TryParse(principalIdClaim, out var principalId))
            return Redirect("/auth/sign-in");

        await authClient.ResendVerificationEmailAsync(new { PrincipalId = principalId }, ct);

        return Redirect("/account/profile?status=Verification email resent");
    }

    /// <summary>
    /// Handles passkey sign-in — receives principalId from PasskeyService auth result and creates a cookie session.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("passkey-sign-in")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PasskeySignIn([FromForm] PasskeySignInRequest request, CancellationToken ct)
    {
        var customerResponse = await customerClient.GetCustomerByPrincipalIdAsync(request.PrincipalId, ct);
        if (!customerResponse.IsSuccessStatusCode)
            return BadRequest(new { error = "Customer not found" });

        var customer = await customerResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);

        var user = new AuthUser
        {
            Sub = request.PrincipalId.ToString(),
            PrincipalId = request.PrincipalId.ToString(),
            Email = request.Email ?? (customer.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null),
            EmailVerified = true,
            Name = customer.TryGetProperty("firstName", out var firstNameProp) ? firstNameProp.GetString() ?? string.Empty : string.Empty,
            ProfileImageUrl = customer.TryGetProperty("profileImageUrl", out var pictureProp) ? pictureProp.GetString() : null
        };
        if (!string.IsNullOrWhiteSpace(request.ReturnUrl) && !request.ReturnUrl.Contains("/auth/sign-in", StringComparison.Ordinal))
            user.ReturnUrl = request.ReturnUrl;

        await SignInCustomerAsync(user);

        return Ok(new { redirectUrl = user.ReturnUrl ?? "/account/profile" });
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

        claims.Add(new Claim("email_verified", user.EmailVerified.ToString().ToLowerInvariant()));

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
        if (string.IsNullOrWhiteSpace(returnUrl))
            return "/account";
        if (Url.IsLocalUrl(returnUrl))
            return returnUrl;
        // Allow cross-app return to the QuoteEngine so the user lands back after sign-in.
        if (IsTrustedCrossAppReturnUrl(returnUrl))
            return returnUrl;
        return "/account";
    }

    private IActionResult RedirectToReturnUrl(string? returnUrl)
    {
        var normalized = NormalizeReturnUrl(returnUrl);
        return Url.IsLocalUrl(normalized)
            ? LocalRedirect(normalized)
            : Redirect(normalized);
    }

    private bool IsTrustedCrossAppReturnUrl(string returnUrl)
    {
        if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri))
            return false;
        if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
            return false;
        var quoteEngineUrl = ResolveQuoteEngineUrl();
        return Uri.TryCreate(quoteEngineUrl, UriKind.Absolute, out var quoteUri)
            && uri.Host.Equals(quoteUri.Host, StringComparison.OrdinalIgnoreCase);
    }

    private string ResolveQuoteEngineUrl()
    {
        var configured = configuration["QuoteEngine:BaseUrl"]
            ?? configuration["QuoteEngine__BaseUrl"]
            ?? Environment.GetEnvironmentVariable("QuoteEngine__BaseUrl")
            ?? Environment.GetEnvironmentVariable("QUOTEENGINE_BASE_URL")
            ?? "https://make.maliev.com";
        return configured.TrimEnd('/');
    }

    private static string NormalizeQuoteEngineReturnUrl(string? returnUrl)
    {
        return !string.IsNullOrWhiteSpace(returnUrl) &&
            returnUrl.StartsWith("/", StringComparison.Ordinal) &&
            !returnUrl.StartsWith("//", StringComparison.Ordinal)
            ? returnUrl
            : "/quotes/new";
    }

    private static string GetLanguageCode(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
            return "th";
        var dash = culture.IndexOf('-', StringComparison.Ordinal);
        return (dash > 0 ? culture[..dash] : culture).ToLowerInvariant();
    }

    private static string ResolveSignUpFirstName(SignUpForm form)
    {
        if (!string.IsNullOrWhiteSpace(form.FirstName))
        {
            return form.FirstName.Trim();
        }

        var localPart = form.Email.Split('@', 2)[0]
            .Replace(".", " ", StringComparison.Ordinal)
            .Replace("_", " ", StringComparison.Ordinal)
            .Replace("-", " ", StringComparison.Ordinal)
            .Trim();
        var parts = localPart.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : "MALIEV";
    }

    private static string ResolveSignUpLastName(SignUpForm form)
    {
        if (!string.IsNullOrWhiteSpace(form.LastName))
        {
            return form.LastName.Trim();
        }

        return "Customer";
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

    /// <summary>
    /// Posted passkey sign-in form with principalId and optional return URL.
    /// </summary>
    public class PasskeySignInRequest
    {
        /// <summary>The authenticated principal id.</summary>
        public Guid PrincipalId { get; set; }

        /// <summary>The principal email.</summary>
        public string? Email { get; set; }

        /// <summary>Local return URL.</summary>
        public string? ReturnUrl { get; set; }
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

        [JsonPropertyName("sub")]
        public string? Sub { get; set; }

        [JsonPropertyName("email_verified")]
        public bool EmailVerified { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
