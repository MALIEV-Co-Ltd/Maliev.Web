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
    PasskeyAuthenticationFlowProtector passkeyFlowProtector,
    TimeProvider timeProvider,
    IWebHostEnvironment environment,
    ILogger<AuthController> logger) : Controller
{
    private const string GoogleApplication = "web";
    private const string GoogleFlowCookiePrefix = "maliev.google.flow.";
    private const string GoogleFlowCookiePath = "/auth/google";
    private const string PasskeyApplication = "web";
    private const string PasskeyFlowCookieName = "maliev.passkey.flow";
    private const string PasskeyFlowCookiePath = "/auth/passkey";
    private static readonly TimeSpan GoogleFlowLifetime = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan MaximumPasskeyFlowLifetime = TimeSpan.FromMinutes(10);
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
    /// Starts a browser-bound passkey ceremony without exposing AuthService flow state.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("passkey/begin")]
    [Consumes("application/json")]
    [IgnoreAntiforgeryToken]
    [RequestSizeLimit(4 * 1024)]
    public async Task<IActionResult> BeginPasskeyAuthentication(
        [FromBody] PasskeyBrowserBeginRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null || request.ReturnUrl is { Length: > 2048 })
        {
            return BadRequest(PasskeyProblem(
                "passkey_flow_invalid",
                "Passkey sign-in could not be started. Reload this page and try again.",
                StatusCodes.Status400BadRequest));
        }

        HttpResponseMessage authResponse;
        try
        {
            authResponse = await authClient.PasskeyAuthBeginAsync(
                new { application = PasskeyApplication },
                cancellationToken);
        }
        catch (Exception exception) when (IsDownstreamUnavailable(exception, cancellationToken))
        {
            logger.LogWarning(
                "AuthService passkey begin was unavailable with trace {TraceIdentifier}",
                HttpContext.TraceIdentifier);
            return PasskeyUnavailable();
        }

        using (authResponse)
        {
            if (!authResponse.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "AuthService passkey begin failed with status {StatusCode} and trace {TraceIdentifier}",
                    authResponse.StatusCode,
                    HttpContext.TraceIdentifier);
                return PasskeyUnavailable();
            }

            var options = await ReadJsonAsync<PasskeyAuthBeginResponse>(
                authResponse,
                cancellationToken);
            var now = timeProvider.GetUtcNow();
            if (!IsValidBeginResponse(options, now))
            {
                logger.LogWarning(
                    "AuthService returned incomplete passkey options with trace {TraceIdentifier}",
                    HttpContext.TraceIdentifier);
                return PasskeyUnavailable();
            }

            var lifetime = options!.ExpiresAtUtc - now;
            var protectedFlow = passkeyFlowProtector.Protect(
                options.FlowId,
                NormalizeReturnUrl(request.ReturnUrl),
                options.ExpiresAtUtc);
            Response.Cookies.Append(
                PasskeyFlowCookieName,
                protectedFlow,
                CreatePasskeyFlowCookieOptions(lifetime));

            return Ok(new
            {
                publicKey = new
                {
                    rpId = options.RpId,
                    challenge = options.Challenge,
                    allowCredentials = options.AllowCredentials,
                    userVerification = options.UserVerification,
                    timeout = options.Timeout
                }
            });
        }
    }

    /// <summary>
    /// Completes a protected passkey flow and creates a session from canonical customer data.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("passkey/complete")]
    [Consumes("application/json")]
    [IgnoreAntiforgeryToken]
    [RequestSizeLimit(32 * 1024)]
    public async Task<IActionResult> CompletePasskeyAuthentication(
        [FromBody] PasskeyBrowserCompleteRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null || !IsValidAssertionRequest(request))
        {
            return BadRequest(PasskeyProblem(
                "passkey_flow_invalid",
                "Passkey sign-in data was incomplete. Reload this page and try again.",
                StatusCodes.Status400BadRequest));
        }

        Request.Cookies.TryGetValue(PasskeyFlowCookieName, out var protectedState);
        DeletePasskeyFlowCookie();
        if (!passkeyFlowProtector.TryUnprotect(protectedState, out var flow) || flow is null)
        {
            return BadRequest(PasskeyProblem(
                "passkey_flow_invalid",
                "Passkey sign-in expired or was already used. Reload this page and try again.",
                StatusCodes.Status400BadRequest));
        }

        HttpResponseMessage authResponse;
        try
        {
            authResponse = await authClient.PasskeyAuthCompleteAsync(new
            {
                application = PasskeyApplication,
                flow_id = flow.FlowId,
                credential_id = request.CredentialId,
                authenticator_data = request.AuthenticatorData,
                client_data_json = request.ClientDataJson,
                signature = request.Signature,
                user_handle = request.UserHandle
            }, cancellationToken);
        }
        catch (Exception exception) when (IsDownstreamUnavailable(exception, cancellationToken))
        {
            logger.LogWarning(
                "AuthService passkey completion was unavailable with trace {TraceIdentifier}",
                HttpContext.TraceIdentifier);
            return PasskeyUnavailable();
        }

        PasskeyAuthCompleteResponse? verifiedIdentity;
        using (authResponse)
        {
            if (!authResponse.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "AuthService passkey completion failed with status {StatusCode} and trace {TraceIdentifier}",
                    authResponse.StatusCode,
                    HttpContext.TraceIdentifier);
                if (authResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return Unauthorized(PasskeyProblem(
                        "passkey_identity_invalid",
                        "MALIEV could not verify this passkey. Reload this page and try again.",
                        StatusCodes.Status401Unauthorized));
                }

                return PasskeyUnavailable();
            }

            verifiedIdentity = await ReadJsonAsync<PasskeyAuthCompleteResponse>(
                authResponse,
                cancellationToken);
        }

        if (verifiedIdentity is not
            {
                Success: true,
                PrincipalId: { } principalId,
                Email: { Length: > 0 and <= 320 } verifiedEmail
            } || principalId == Guid.Empty)
        {
            logger.LogWarning(
                "AuthService returned incomplete verified passkey identity with trace {TraceIdentifier}",
                HttpContext.TraceIdentifier);
            return PasskeyIdentityIncomplete();
        }

        HttpResponseMessage customerResponse;
        try
        {
            customerResponse = await customerClient.GetCustomerByPrincipalIdAsync(
                principalId,
                cancellationToken);
        }
        catch (Exception exception) when (IsDownstreamUnavailable(exception, cancellationToken))
        {
            logger.LogWarning(
                "CustomerService passkey identity lookup was unavailable with trace {TraceIdentifier}",
                HttpContext.TraceIdentifier);
            return PasskeyUnavailable();
        }

        CustomerAuthenticationContextResponse? customer;
        using (customerResponse)
        {
            if (!customerResponse.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "CustomerService passkey identity lookup failed with status {StatusCode} and trace {TraceIdentifier}",
                    customerResponse.StatusCode,
                    HttpContext.TraceIdentifier);
                return (int)customerResponse.StatusCode >= StatusCodes.Status500InternalServerError
                    ? PasskeyUnavailable()
                    : PasskeyIdentityIncomplete();
            }

            customer = await ReadJsonAsync<CustomerAuthenticationContextResponse>(
                customerResponse,
                cancellationToken);
        }

        if (customer is null ||
            customer.CustomerId == Guid.Empty ||
            customer.PrincipalId != principalId ||
            string.IsNullOrWhiteSpace(customer.CustomerEmail) ||
            customer.CustomerEmail.Length > 320 ||
            string.IsNullOrWhiteSpace(customer.AccountEmail) ||
            customer.AccountEmail.Length > 320 ||
            string.IsNullOrWhiteSpace(customer.AccountStatus) ||
            customer.AccountEmailVerified is not { } accountEmailVerified ||
            !string.Equals(customer.CustomerEmail, customer.AccountEmail, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(customer.AccountEmail, verifiedEmail, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning(
                "AuthService and CustomerService passkey identities did not match with trace {TraceIdentifier}",
                HttpContext.TraceIdentifier);
            return PasskeyIdentityIncomplete();
        }

        if (!string.Equals(customer.AccountStatus, "Active", StringComparison.Ordinal))
        {
            logger.LogWarning(
                "Passkey session rejected because the canonical customer account is not active with trace {TraceIdentifier}",
                HttpContext.TraceIdentifier);
            return Unauthorized(PasskeyProblem(
                "passkey_identity_invalid",
                "MALIEV could not sign in this account with this passkey. Continue with another sign-in method or contact support.",
                StatusCodes.Status401Unauthorized));
        }

        var canonicalName = !string.IsNullOrWhiteSpace(customer.Name)
            ? customer.Name.Trim()
            : $"{customer.FirstName} {customer.LastName}".Trim();
        await SignInCustomerAsync(new AuthUser
        {
            UserId = principalId.ToString(),
            PrincipalId = principalId.ToString(),
            CustomerId = customer.CustomerId.ToString(),
            Email = customer.CustomerEmail,
            Name = canonicalName,
            ProfileImageUrl = customer.ProfileImageUrl,
            EmailVerified = accountEmailVerified
        });
        logger.LogInformation(
            "Passkey customer session issued with trace {TraceIdentifier}",
            HttpContext.TraceIdentifier);
        return Ok(new { redirectUrl = flow.ReturnUrl });
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

    private static ProblemDetails PasskeyProblem(string code, string detail, int statusCode)
    {
        var problem = new ProblemDetails
        {
            Type = $"https://www.maliev.com/problems/{code.Replace('_', '-')}",
            Title = "Passkey sign-in could not be completed",
            Detail = detail,
            Status = statusCode
        };
        problem.Extensions["code"] = code;
        return problem;
    }

    private ObjectResult PasskeyUnavailable() => StatusCode(
        StatusCodes.Status503ServiceUnavailable,
        PasskeyProblem(
            "passkey_temporarily_unavailable",
            "Passkey sign-in is temporarily unavailable. Continue with Google or email, or try again shortly.",
            StatusCodes.Status503ServiceUnavailable));

    private ObjectResult PasskeyIdentityIncomplete() => StatusCode(
        StatusCodes.Status502BadGateway,
        PasskeyProblem(
            "passkey_identity_incomplete",
            "Your passkey was verified, but MALIEV could not safely start the customer session. Try again shortly.",
            StatusCodes.Status502BadGateway));

    private static async Task<T?> ReadJsonAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        }
        catch (Exception exception) when (
            exception is JsonException or NotSupportedException or HttpRequestException or IOException)
        {
            return default;
        }
    }

    private static bool IsDownstreamUnavailable(
        Exception exception,
        CancellationToken requestCancellationToken) =>
        exception is HttpRequestException ||
        exception is TaskCanceledException && !requestCancellationToken.IsCancellationRequested;

    private static bool IsValidBeginResponse(
        PasskeyAuthBeginResponse? response,
        DateTimeOffset now) =>
        response is not null &&
        IsCanonicalBase64Url(response.FlowId, 32, 32) &&
        response.ExpiresAtUtc - now >= TimeSpan.FromSeconds(30) &&
        response.ExpiresAtUtc - now <= MaximumPasskeyFlowLifetime &&
        response.RpId is { Length: > 0 and <= 253 } &&
        Uri.CheckHostName(response.RpId) != UriHostNameType.Unknown &&
        IsCanonicalBase64Url(response.Challenge, 32, 64) &&
        response.AllowCredentials is { Count: <= 64 } &&
        response.AllowCredentials.All(credential =>
            string.Equals(credential.Type, "public-key", StringComparison.Ordinal) &&
            IsCanonicalBase64Url(credential.Id, 1, 1024)) &&
        string.Equals(response.UserVerification, "required", StringComparison.Ordinal) &&
        response.Timeout is >= 30_000 and <= 600_000;

    private static bool IsValidAssertionRequest(PasskeyBrowserCompleteRequest request) =>
        IsCanonicalBase64Url(request.CredentialId, 1, 1024) &&
        IsCanonicalBase64Url(request.AuthenticatorData, 37, 4096) &&
        IsCanonicalBase64Url(request.ClientDataJson, 2, 8192) &&
        IsCanonicalBase64Url(request.Signature, 1, 2048) &&
        IsCanonicalBase64Url(request.UserHandle, 1, 64);

    private static bool IsCanonicalBase64Url(
        string? value,
        int minimumBytes,
        int maximumBytes)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > ((maximumBytes + 2) / 3 * 4) ||
            value.Contains('=') ||
            value.Contains('+') ||
            value.Contains('/') ||
            value.Any(character =>
                !(character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '-' or '_')))
        {
            return false;
        }

        try
        {
            var padded = value.Replace('-', '+').Replace('_', '/');
            padded += new string('=', (4 - padded.Length % 4) % 4);
            var decoded = Convert.FromBase64String(padded);
            return decoded.Length >= minimumBytes &&
                decoded.Length <= maximumBytes &&
                string.Equals(ToBase64Url(decoded), value, StringComparison.Ordinal);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static string ToBase64Url(byte[] value) =>
        Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');

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

    private CookieOptions CreatePasskeyFlowCookieOptions(TimeSpan lifetime) => new()
    {
        HttpOnly = true,
        Secure = !environment.IsEnvironment("Testing"),
        SameSite = SameSiteMode.Strict,
        IsEssential = true,
        Path = PasskeyFlowCookiePath,
        MaxAge = lifetime
    };

    private void DeletePasskeyFlowCookie()
    {
        Response.Cookies.Delete(PasskeyFlowCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsEnvironment("Testing"),
            SameSite = SameSiteMode.Strict,
            IsEssential = true,
            Path = PasskeyFlowCookiePath
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

    /// <summary>Browser request to start a protected passkey flow.</summary>
    public sealed class PasskeyBrowserBeginRequest
    {
        /// <summary>Requested post-authentication return location.</summary>
        public string? ReturnUrl { get; set; }
    }

    /// <summary>Browser WebAuthn assertion without trusted identity or flow fields.</summary>
    public sealed class PasskeyBrowserCompleteRequest
    {
        /// <summary>Base64URL credential identifier.</summary>
        public string CredentialId { get; set; } = string.Empty;

        /// <summary>Base64URL authenticator data.</summary>
        public string AuthenticatorData { get; set; } = string.Empty;

        /// <summary>Base64URL raw client data JSON bytes.</summary>
        public string ClientDataJson { get; set; } = string.Empty;

        /// <summary>Base64URL authenticator signature.</summary>
        public string Signature { get; set; } = string.Empty;

        /// <summary>Base64URL discoverable-credential user handle.</summary>
        public string UserHandle { get; set; } = string.Empty;
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

    private sealed class PasskeyAuthBeginResponse
    {
        [JsonPropertyName("flow_id")]
        public string FlowId { get; set; } = string.Empty;

        [JsonPropertyName("expires_at_utc")]
        public DateTimeOffset ExpiresAtUtc { get; set; }

        [JsonPropertyName("rp_id")]
        public string RpId { get; set; } = string.Empty;

        [JsonPropertyName("challenge")]
        public string Challenge { get; set; } = string.Empty;

        [JsonPropertyName("allow_credentials")]
        public List<PasskeyAllowedCredentialResponse> AllowCredentials { get; set; } = [];

        [JsonPropertyName("user_verification")]
        public string UserVerification { get; set; } = string.Empty;

        [JsonPropertyName("timeout")]
        public int Timeout { get; set; }
    }

    private sealed class PasskeyAllowedCredentialResponse
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    private sealed class PasskeyAuthCompleteResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("principal_id")]
        public Guid? PrincipalId { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }
    }

    private sealed class CustomerAuthenticationContextResponse
    {
        [JsonPropertyName("customerId")]
        public Guid CustomerId { get; set; }

        [JsonPropertyName("principalId")]
        public Guid? PrincipalId { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("customerEmail")]
        public string CustomerEmail { get; set; } = string.Empty;

        [JsonPropertyName("accountEmail")]
        public string AccountEmail { get; set; } = string.Empty;

        [JsonPropertyName("accountStatus")]
        public string AccountStatus { get; set; } = string.Empty;

        [JsonPropertyName("accountEmailVerified")]
        public bool? AccountEmailVerified { get; set; }

        [JsonPropertyName("profileImageUrl")]
        public string? ProfileImageUrl { get; set; }
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
