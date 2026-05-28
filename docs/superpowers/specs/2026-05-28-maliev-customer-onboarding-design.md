# Maliev Customer Onboarding — Design Doc

**Date:** 2026-05-28
**Scope:** Welcome email, email verification (grace period), Passkey (WebAuthn) integration

---

## Overview

Three connected features for new customer sign-up:
1. **Welcome email** sent on successful registration (Google SSO or email/password)
2. **Email verification** with grace period for email/password sign-ups
3. **Passkey (WebAuthn)** as an additional authentication method

All three shipped together as one work package. Approach 1 — AuthService owns verification state and passkey credentials.

---

## Architecture

### Services

| Service | Changes |
|---------|---------|
| **AuthService** | Adds `email_verified` column to user principal. New endpoints: `POST /auth/v1/verify-email`, `POST /auth/v1/resend-verification-email`, `POST /auth/v1/passkey/register/begin`, `POST /auth/v1/passkey/register/complete`, `POST /auth/v1/passkey/auth/begin`, `POST /auth/v1/passkey/auth/complete`. New DB entity: `PasskeyCredential`. Publishes `EmailVerifiedEvent`, `VerificationEmailRequestedEvent`. |
| **CustomerService** | No changes needed. Already publishes `CustomerRegisteredEvent` with `registrationMethod`. |
| **NotificationService** | New consumer for `CustomerRegisteredEvent` (welcome email). New consumer for `EmailVerifiedEvent` (verification confirmation). New seed templates. |
| **Web BFF** | Adds `email_verified` claim to auth cookie. Verification banner component. Passkey JS interop. Account security page section for passkeys. |

### New DB Entities (AuthService)

**User principal** — existing table gets `EmailVerifiedAtUtc: DateTime?` column (null = not verified)

**PasskeyCredential:**
| Column | Type | Notes |
|--------|------|-------|
| Id | Guid | PK |
| PrincipalId | Guid | FK to user principal |
| CredentialId | string | base64-encoded credential ID |
| PublicKey | string | PEM-encoded public key |
| DeviceName | string | User-friendly label (e.g., "Touch ID on MacBook") |
| CreatedAtUtc | DateTime | |
| LastUsedAtUtc | DateTime? | |

Max 10 passkeys per principal.

### New Messaging Contracts

**`EmailVerifiedEvent`** (Auth domain):
- `PrincipalId`, `Email`, `VerifiedAtUtc`
- Published by AuthService when user completes verification
- Consumed by NotificationService → sends "Welcome, you're verified!" email

**`VerificationEmailRequestedEvent`** (Auth domain):
- `PrincipalId`, `Email`, `VerificationToken`, `ExpiresAtUtc`
- Published by AuthService when verification email is requested (initial sign-up or resend)
- Consumed by NotificationService → renders and sends verification email via template

---

## Email Verification Flow (email/password)

### Sign-up
1. User fills sign-up form → POST `/auth/sign-up/email` (BFF)
2. BFF → `customerClient.RegisterCustomerAsync()` (CustomerService)
3. CustomerService creates customer with `registrationMethod = "Email"`, publishes `CustomerRegisteredEvent`
4. BFF signs user in with `email_verified = false` claim in cookie
5. BFF calls `POST /auth/v1/initiate-email-verification` on AuthService (new endpoint, takes `principalId` and `email`)
6. AuthService generates signed JWT verification token (15-min expiry), stores hash in DB, publishes `VerificationEmailRequestedEvent`
7. NotificationService consumes event, renders "Verify your email" template via Brevo
8. User sees verification banner: *"Check your email — we sent a verification link to [email]"*

### Verification
9. User clicks link: `GET /auth/verify-email?token=xxx` (BFF)
10. BFF validates token, POSTs `{token}` to `POST /auth/v1/verify-email` (AuthService)
11. AuthService sets `EmailVerifiedAtUtc = now`, publishes `EmailVerifiedEvent`
12. NotificationService sends "Welcome, you're verified!" email
13. BFF refreshes cookie with `email_verified = true` claim
14. If BFF sign-in (step 14a): redirect to original ReturnUrl or home with banner dismissed
15. If direct link click (step 14b): redirect to `/auth/sign-in?status=Email verified successfully`

### Resend
- "Resend verification email" button on banner → POST `/auth/v1/resend-verification-email`
- Rate-limited: max 1 request per 5 minutes per principal
- Same flow as steps 5-7 above

### Google SSO
1. Google callback → `authClient.ExchangeCustomerGoogleAsync()` (AuthService)
2. AuthService creates customer with `EmailVerifiedAtUtc = now` (Google vouched for email)
3. AuthService publishes `CustomerRegisteredEvent` with `registrationMethod = "Google"`
4. NotificationService sends "Welcome to MALIEV" email (no verification prompt)

---

## Passkey Integration

### WebAuthn Configuration

| Setting | Production | Local Dev |
|---------|-----------|-----------|
| RP ID | `www.maliev.com` | `localhost` |
| Allowed origins | `https://www.maliev.com` | `https://localhost:56139` |
| User verification | `required` | same |
| Resident keys | `required` | same |
| Attestation | `none` | same |

Configured via `WebAuthn:RPId` and `WebAuthn:AllowedOrigins` in AuthService's config. Challenge state stored in Redis (5-min TTL).

### Registration Flow (signed-in user only)
1. User clicks "Add passkey" on Account Security page
2. BFF JS calls `POST /auth/v1/passkey/register/begin` → returns `PublicKeyCredentialCreationOptions`
3. Browser calls `navigator.credentials.create({ publicKey })`
4. BFF JS sends result to `POST /auth/v1/passkey/register/complete`
5. AuthService validates, stores `PasskeyCredential`
6. UI shows new passkey in list

### Authentication Flow
1. User clicks "Sign in with passkey" on sign-in page
2. BFF JS calls `POST /auth/v1/passkey/auth/begin` → returns `PublicKeyCredentialRequestOptions`
3. Browser calls `navigator.credentials.get({ publicKey })`
4. BFF JS sends assertion to `POST /auth/v1/passkey/auth/complete`
5. AuthService verifies signature, returns session token
6. BFF signs user into cookie

### UI Placement
- **Sign-in page**: "Sign in with passkey" button below Google button, before email panel
- **Account Security page** (AccountProfile.razor): "Passkeys" section showing registered devices with "Add passkey" / "Remove" buttons

---

## BFF Changes

### Cookie Claims
- Add `email_verified` (true/false) to the auth cookie on sign-in
- For Google SSO: `email_verified = true`
- For email/password: `email_verified = false` until verification completes
- BFF endpoint `GET /auth/refresh-claims` → re-reads `email_verified` from AuthService (`GET /auth/v1/me`), re-issues cookie with updated claims
- Called by the verification callback page after success, and on account page load if banner is dismissed

### Verification Banner
- `MudAlert` component shown on all account pages when `email_verified == false`
- Dismissible (per session, not permanent)
- Text: "Verify your email — we sent a link to {email}. Didn't receive it? [Resend]"
- The account/profile API endpoint returns the masked email for display

### Passkey JS Interop
- `maliev-passkey.js` module with:
  - `createPasskey(registerBeginUrl, registerCompleteUrl)`
  - `authenticatePasskey(authBeginUrl, authCompleteUrl)`
- Standard `navigator.credentials.create()` / `.get()` wrappers with error handling
- Fallback: if WebAuthn not available, hide passkey buttons

---

## Notification Templates

### New seed templates

| Key | Type | When Sent | Parameters |
|-----|------|-----------|------------|
| `customer-welcome-google` | en/th email | Google SSO registration | `firstName` |
| `customer-welcome-email` | en/th email | Email/password registration | `firstName`, `verificationUrl` |
| `customer-email-verified` | en/th email | Email verification completed | `firstName` |
| `customer-email-resend` | en/th email | Resend verification requested | `firstName`, `verificationUrl` |

Templates stored in NotificationService's DB, seeded via `NotificationBootstrapData.cs`.

### Template content (English versions)
- **customer-welcome-google**: "Hi {firstName}, welcome to MALIEV. You're all set — start uploading files and getting quotes right away."
- **customer-welcome-email**: "Hi {firstName}, welcome to MALIEV. Please verify your email address by clicking this link: {verificationUrl}"
- **customer-email-verified**: "Hi {firstName}, your email has been verified. Thanks!"
- **customer-email-resend**: "Hi {firstName}, here's a new verification link: {verificationUrl}"
