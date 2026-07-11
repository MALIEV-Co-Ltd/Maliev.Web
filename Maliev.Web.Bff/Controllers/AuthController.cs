using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Maliev.Web.Bff.Clients;
using Maliev.Web.Bff.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
    GoogleIdentityFlowProtector googleIdentityFlowProtector,
    ILogger<AuthController> logger) : Controller
{
    private const string GoogleApplication = "web";
    private const string GoogleFlowCookiePrefix = "maliev.google.flow.";
    private const string GoogleFlowCookiePath = "/auth/google";
    private static readonly TimeSpan GoogleFlowLifetime = TimeSpan.FromMinutes(10);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly string[] CustomerAccountPermissions =
    [
        "customer.profile.read",
        "customer.profile.write",
        "customer.addresses.manage",
        "order.orders.read"
    ];

    /// <summary>
    /// Issues a one-time AuthService nonce for an official Google Identity Services button.
    /// </summary>
    [HttpPost("google/nonce")]
    [AllowAnonymous]
    [Consumes("application/json")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> IssueGoogleNonce(
        [FromBody] GoogleIdentityBrowserNonceRequest request,
        CancellationToken cancellationToken)
    {
        var clientId = configuration["Authentication:Google:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, GoogleProblem(
                "google_not_configured",
                "Google sign-in is not configured. Continue with email instead.",
                StatusCodes.Status503ServiceUnavailable));
        }

        using var response = await authClient.IssueCustomerGoogleNonceAsync(
            new { application = GoogleApplication },
            cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("AuthService Google nonce issuance failed with status {StatusCode}", response.StatusCode);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, GoogleProblem(
                "google_temporarily_unavailable",
                "Google sign-in is temporarily unavailable. Continue with email or try again shortly.",
                StatusCodes.Status503ServiceUnavailable));
        }

        var nonce = await response.Content.ReadFromJsonAsync<GoogleIdentityNonceResponse>(JsonOptions, cancellationToken);
        if (nonce is null ||
            nonce.Nonce.Length is < 32 or > 256 ||
            nonce.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            logger.LogWarning("AuthService returned an incomplete Google nonce response");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, GoogleProblem(
                "google_temporarily_unavailable",
                "Google sign-in is temporarily unavailable. Continue with email or try again shortly.",
                StatusCodes.Status503ServiceUnavailable));
        }

        var flowId = Guid.NewGuid().ToString("N");
        var protectedState = googleIdentityFlowProtector.Protect(
            nonce.Nonce,
            NormalizeReturnUrl(request.ReturnUrl),
            GoogleFlowLifetime);
        Response.Cookies.Append(
            GetGoogleFlowCookieName(flowId),
            protectedState,
            CreateGoogleFlowCookieOptions(GoogleFlowLifetime));

        return Ok(new
        {
            clientId,
            nonce = nonce.Nonce,
            flowId,
            nonce.ExpiresAtUtc
        });
    }

    /// <summary>
    /// Exchanges a GIS credential after verifying its nonce belongs to this browser flow.
    /// </summary>
    [HttpPost("google/exchange")]
    [AllowAnonymous]
    [Consumes("application/json")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> ExchangeGoogleCredential(
        [FromBody] GoogleIdentityBrowserExchangeRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Credential) ||
            request.Credential.Length > 8192 ||
            string.IsNullOrWhiteSpace(request.Nonce) ||
            request.Nonce.Length > 256 ||
            !Guid.TryParseExact(request.FlowId, "N", out _))
        {
            return BadRequest(GoogleProblem(
                "google_flow_invalid",
                "Google sign-in could not be completed. Reload this page and try again.",
                StatusCodes.Status400BadRequest));
        }

        var cookieName = GetGoogleFlowCookieName(request.FlowId);
        Request.Cookies.TryGetValue(cookieName, out var protectedState);
        DeleteGoogleFlowCookie(cookieName);
        if (!googleIdentityFlowProtector.TryUnprotect(protectedState, out var flow) ||
            flow is null ||
            !GoogleIdentityFlowProtector.NonceMatches(flow.Nonce, request.Nonce))
        {
            return BadRequest(GoogleProblem(
                "google_flow_invalid",
                "Google sign-in expired or was already used. Reload this page and try again.",
                StatusCodes.Status400BadRequest));
        }

        using var response = await authClient.ExchangeCustomerGoogleAsync(new
        {
            credential = request.Credential,
            application = GoogleApplication,
            nonce = request.Nonce,
            preferred_language = GetLanguageCode(Request.Cookies["maliev.culture"]),
            timezone = "Asia/Bangkok"
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Customer Google exchange failed with status {StatusCode}", response.StatusCode);
            var error = await ReadGoogleExchangeErrorAsync(response, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Conflict &&
                string.Equals(error?.Error, "account_verification_required", StringComparison.Ordinal))
            {
                return Conflict(GoogleProblem(
                    "account_verification_required",
                    "This email already has a MALIEV account. Sign in with email and password first, then connect Google from your account.",
                    StatusCodes.Status409Conflict));
            }

            var statusCode = response.StatusCode == HttpStatusCode.ServiceUnavailable
                ? StatusCodes.Status503ServiceUnavailable
                : StatusCodes.Status401Unauthorized;
            return StatusCode(statusCode, GoogleProblem(
                statusCode == StatusCodes.Status503ServiceUnavailable
                    ? "google_temporarily_unavailable"
                    : "google_identity_invalid",
                statusCode == StatusCodes.Status503ServiceUnavailable
                    ? "Google sign-in is temporarily unavailable. Continue with email or try again shortly."
                    : "Google could not verify this sign-in. Reload this page and try again.",
                statusCode));
        }

        var session = await response.Content.ReadFromJsonAsync<AuthLoginResponse>(JsonOptions, cancellationToken);
        if (session?.User is null)
        {
            return StatusCode(StatusCodes.Status502BadGateway, GoogleProblem(
                "google_session_incomplete",
                "Google sign-in completed, but MALIEV could not start your session. Try again shortly.",
                StatusCodes.Status502BadGateway));
        }

        session.User.EmailVerified = true;
        await SignInCustomerAsync(session.User);
        return Ok(new { redirectUrl = flow.ReturnUrl });
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
    /// Rejects the retired browser-authored passkey identity handoff.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("passkey-sign-in")]
    [ValidateAntiForgeryToken]
    public IActionResult PasskeySignIn()
    {
        var problem = new ProblemDetails
        {
            Type = "https://www.maliev.com/problems/passkey-flow-retired",
            Title = "Passkey sign-in is temporarily unavailable",
            Detail = "Use Google or email sign-in while secure passkey verification is being restored.",
            Status = StatusCodes.Status410Gone
        };
        problem.Extensions["code"] = "passkey_flow_retired";
        return StatusCode(StatusCodes.Status410Gone, problem);
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

    private static async Task<GoogleExchangeErrorResponse?> ReadGoogleExchangeErrorAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<GoogleExchangeErrorResponse>(
                JsonOptions,
                cancellationToken);
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException)
        {
            return null;
        }
    }

    private static ProblemDetails GoogleProblem(string code, string detail, int statusCode)
    {
        var problem = new ProblemDetails
        {
            Type = $"https://www.maliev.com/problems/{code.Replace('_', '-')}",
            Title = "Google sign-in could not be completed",
            Detail = detail,
            Status = statusCode
        };
        problem.Extensions["code"] = code;
        return problem;
    }

    private static string GetGoogleFlowCookieName(string flowId) => $"{GoogleFlowCookiePrefix}{flowId}";

    private CookieOptions CreateGoogleFlowCookieOptions(TimeSpan lifetime) => new()
    {
        HttpOnly = true,
        Secure = Request.IsHttps,
        SameSite = SameSiteMode.Strict,
        IsEssential = true,
        Path = GoogleFlowCookiePath,
        MaxAge = lifetime
    };

    private void DeleteGoogleFlowCookie(string cookieName)
    {
        Response.Cookies.Delete(cookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            IsEssential = true,
            Path = GoogleFlowCookiePath
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
            && uri.Scheme.Equals(quoteUri.Scheme, StringComparison.OrdinalIgnoreCase)
            && uri.Host.Equals(quoteUri.Host, StringComparison.OrdinalIgnoreCase)
            && uri.Port == quoteUri.Port;
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

    /// <summary>Browser request for a short-lived official GIS flow.</summary>
    public sealed class GoogleIdentityBrowserNonceRequest
    {
        /// <summary>Requested post-authentication return location.</summary>
        public string? ReturnUrl { get; set; }
    }

    /// <summary>Browser GIS credential exchange request.</summary>
    public sealed class GoogleIdentityBrowserExchangeRequest
    {
        /// <summary>Raw GIS ID-token credential returned by Google.</summary>
        public string Credential { get; set; } = string.Empty;

        /// <summary>Nonce supplied to GIS and echoed in the verified ID token.</summary>
        public string Nonce { get; set; } = string.Empty;

        /// <summary>Opaque browser-flow identifier whose state is held in an HttpOnly cookie.</summary>
        public string FlowId { get; set; } = string.Empty;
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

    private sealed class GoogleIdentityNonceResponse
    {
        [JsonPropertyName("nonce")]
        public string Nonce { get; set; } = string.Empty;

        [JsonPropertyName("expires_at_utc")]
        public DateTimeOffset ExpiresAtUtc { get; set; }
    }

    private sealed class GoogleExchangeErrorResponse
    {
        [JsonPropertyName("error")]
        public string? Error { get; set; }
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
