# Email Change Verification — Design Spec

**Date:** 2026-05-29  
**Status:** Approved  
**Scope:** Maliev.Web (BFF + Blazor WASM Client)

## Problem

The `/account/profile` page currently allows customers to freely type any email address into a text field and save it. There is no email verification step, no pending/confirmed state, and the identity claims in the auth cookie are never updated after an email change. This means a customer can change their login email to an address they don't control, locking themselves out or creating a security gap.

## Goals

1. Every email change must be verified by the customer clicking a token link sent to the new address
2. The old email remains the active login email until verification completes
3. On verification, the auth cookie and CustomerService record are both updated atomically
4. Customers can re-initiate a change with a different email at any time (old pending token is invalidated)
5. The profile form remains fully editable during verification — only the email field shows pending state

## Architecture Decision

**Approach: AuthService-driven (thin BFF)**

AuthService is the single source of truth for email state. It stores the pending email, generates verification tokens, sends emails, and promotes the pending email to active on successful verification. The Web BFF delegates to AuthService and only adds CustomerService sync after verification completes.

## Data Flow

### Initiation

1. Customer clicks "Change email" → read-only email label becomes an editable `New email` input + "Send verification" button
2. Customer enters new email, clicks "Send verification"
3. Client calls `POST /web/v1/account/email/change` with body `{ "email": "new@example.com" }`
4. BFF calls AuthService `POST /auth/v1/initiate-email-verification` with `{ email, principalId }`
5. AuthService stores pending email, generates a token, sends verification email to **new** address, invalidates any prior pending token for this principal
6. BFF returns success → Client shows pending state: "Verification sent to new@example.com"
7. CustomerService is **not** updated — old email remains for login

### Verification (customer clicks link in email)

8. Browser hits `GET /auth/verify-email?token=...` (existing endpoint, no change)
9. AuthService validates token, promotes pending email to active principal email
10. BFF redirects to `GET /auth/refresh-claims` (existing endpoint, enhanced)
11. `RefreshClaims` detects the email change and calls `CustomerService.UpdateCustomerAsync` to sync the new email
12. Cookie claims are refreshed: new email in `ClaimTypes.Email`, `email_verified=true`

### Re-initiation (different email before first one verified)

13. Customer clicks "Change to different email", enters a new address, clicks "Send verification"
14. Steps 3-7 repeat — AuthService invalidates the previous pending token, sends new one

### Profile re-load after verification redirect

15. `AccountProfile.razor` loads with new email and `_emailVerified = true`

## Endpoints

### New: `POST /web/v1/account/email/change`

| Aspect | Detail |
|--------|--------|
| Auth | `[RequirePermission("customer.profile.write")]` |
| Body | `{ "email": "string" }` — validated for format, length ≤ 320, not empty |
| Success | 200 OK |
| Error | 400 if same as current email; 409 if email already registered; 503 if AuthService unavailable |
| Side effects | Calls `authClient.InitiateEmailVerificationAsync({ email, principalId })` only |

### New: `POST /web/v1/account/email/resend-change`

| Aspect | Detail |
|--------|--------|
| Auth | `[RequirePermission("customer.profile.write")]` |
| Body | none |
| Success | 200 OK |
| Error | 400 if no pending change; 503 if AuthService unavailable |
| Side effects | Calls `authClient.ResendVerificationEmailAsync({ principalId })` |

### Modified: `PATCH /web/v1/account/profile`

Remove `email` from the anonymous payload sent to `CustomerService.UpdateCustomerAsync`. The email field is no longer freely writable via profile save — it is managed exclusively through the email change flow.

### Modified: `GET /auth/refresh-claims`

Add CustomerService sync after claim refresh:
- Compare new email from AuthService response to current cookie `ClaimTypes.Email`
- If different, call `customerClient.UpdateCustomerAsync(customerId, { email })`
- Update `ClaimTypes.Email` claim in cookie

## Client API (MalievApiClient)

```csharp
internal async Task InitiateEmailChangeAsync(string email, CancellationToken ct = default)
{
    var response = await httpClient.PostAsJsonAsync("web/v1/account/email/change", new { email }, ct);
    await EnsureSuccessAsync(response, ct);
}

internal async Task ResendEmailChangeAsync(CancellationToken ct = default)
{
    var response = await httpClient.PostAsync("web/v1/account/email/resend-change", null, ct);
    await EnsureSuccessAsync(response, ct);
}
```

## DTO

```csharp
// Maliev.Web.Shared/Account/EmailChangeDtos.cs (new file)
public sealed class EmailChangeRequest
{
    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string Email { get; set; } = string.Empty;
}
```

## Profile Page UX States

### Normal (verified, no change in progress)
```
Email: user@example.com  [verified badge]
[Change email]
```

### Editing new email
```
Email: user@example.com  [verified badge]

New email: [_____________]  [Send verification]
[Cancel]
```

### Pending verification
```
Email: user@example.com  [verified badge]

[info] Verification sent to new@example.com — check your inbox
[Resend verification] [Change to different email] [Cancel]
```

### Verification expired (shown on page load if AuthService indicates expiry)
```
Email: user@example.com  [verified badge]

[warning] Verification link for new@example.com has expired
[Change email]
```
Note: The current page cannot query "is there a pending change?" — we rely on AuthService returning a specific status if we add a status endpoint, OR we show a generic state after a timeout. For initial implementation, the cancellation flow is: customer clicks "Cancel" to clear UI state (token expires naturally in AuthService).

### Verification banner (existing, unchanged)
Already handled by `_emailVerified` from cookie claim and the `MudAlert` at top of page.

## Error Handling

| Scenario | Response | UI |
|---|---|---|
| New email = current email | 400 | "This is already your current email" |
| Email owned by another account | 409 | "This email is already registered" |
| Invalid email format | 400 | Client-side validation prevents submit; server validates too |
| AuthService unavailable | 503 | "We could not send the verification right now. Please try again." |
| Token expired | AuthService 400 | "This verification link has expired. Request a new one from your profile." |
| CustomerService sync fails after verification | Caught, logged, non-blocking | On next profile load, detect email mismatch and re-sync automatically |
| Rapid re-send clicks | N/A | Button disabled with `_sendingVerification` flag during request |
| Signed out during pending verification | N/A | Token link still works; sign-in with new email after verification |
| Cancel pending change | N/A | UI clears pending state; AuthService token expires naturally |

## Files to Modify

| File | Change |
|---|---|
| `Maliev.Web.Shared/Account/EmailChangeDtos.cs` | New — `EmailChangeRequest` DTO |
| `Maliev.Web.Bff/Controllers/AccountController.cs` | Add `ChangeEmail` and `ResendEmailChange` endpoints; remove `email` from `UpdateProfile` payload |
| `Maliev.Web.Bff/Controllers/AuthController.cs` | Enhance `RefreshClaims` to sync CustomerService on email change |
| `Maliev.Web.Client/Services/MalievApiClient.cs` | Add `InitiateEmailChangeAsync`, `ResendEmailChangeAsync` |
| `Maliev.Web.Client/Pages/AccountProfile.razor` | Replace inline email editing with verification workflow UI |
| `Maliev.Web.Client/Pages/AccountProfile.razor.cs` | (if code-behind exists) or `@code` block — add email change state management |
| `Maliev.Web.Tests/` | New tests for email change endpoints and flows |

### Files NOT touched

- `CheckoutBoundaryClients.cs` — AuthService client already has the needed methods
- `AccountDtos.cs` — existing profile DTOs kept; email field stays for display but not write
- `Program.cs` — no new service registrations needed
- AuthService itself — uses existing `InitiateEmailVerification`, `VerifyEmail`, `ResendVerificationEmail` endpoints

## Verification & Testing

### Unit Tests (AccountController)
- `ChangeEmail_Returns200_WhenNewEmailProvided`
- `ChangeEmail_Returns400_WhenEmailSameAsCurrent`
- `ChangeEmail_Returns503_WhenAuthServiceUnavailable`
- `ResendEmailChange_Returns200_WhenCalled`
- `UpdateProfile_DoesNotSendEmailField`

### Integration Tests
- Full flow: initiate change → mock verification callback → assert CustomerService was synced
- Re-initiate: call change twice with different emails → assert second call succeeds
- Cookie claims updated after refresh-claims with changed email

### Manual E2E
- Navigate to `/account/profile`, click "Change email", enter new email, click "Send verification"
- Check inbox for verification email, click link
- Verify redirect back to profile shows new email
- Verify signing in with new email works
