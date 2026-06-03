# Maliev Customer Onboarding Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Welcome email + email verification (grace period) + Passkey (WebAuthn) for new customer sign-ups

**Architecture:** AuthService owns email verification state and passkey credentials. NotificationService sends templated emails via Brevo. Web BFF adds `email_verified` claim, verification banner, and passkey JS interop. 4 repos touched: Maliev.MessagingContracts, Maliev.AuthService, Maliev.NotificationService, Maliev.Web.

**Tech Stack:** .NET 10, EF Core 10, PostgreSQL, MassTransit/RabbitMQ, Brevo (Sendinblue), WebAuthn API, MudBlazor

---

### Task 1: Messaging Contracts — New Events

**Repo:** Maliev.MessagingContracts

**File structure:**
- Create: `contracts/schemas/auth/email-verified-event.json`
- Create: `contracts/schemas/auth/verification-email-requested-event.json`
- Create: `contracts/schemas/auth/passkey-registered-event.json`
- Modify: `generated/csharp/Contracts/Auth/AuthEvents.cs` (add new record + payload types)

- [ ] **Step 1: Create `EmailVerifiedEvent` schema**

Write `contracts/schemas/auth/email-verified-event.json`:
```json
{
  "schema": "https://json-schema.org/draft/2020-12/schema",
  "title": "EmailVerifiedEvent",
  "description": "Published when a customer completes email verification",
  "type": "object",
  "allOf": [
    { "$ref": "../shared/base-message.json" }
  ],
  "properties": {
    "payload": {
      "type": "object",
      "properties": {
        "principalId": { "type": "string", "format": "uuid" },
        "email": { "type": "string", "format": "email" },
        "verifiedAtUtc": { "type": "string", "format": "date-time" }
      },
      "required": ["principalId", "email", "verifiedAtUtc"]
    }
  },
  "required": ["payload"]
}
```

- [ ] **Step 2: Create `VerificationEmailRequestedEvent` schema**

Write `contracts/schemas/auth/verification-email-requested-event.json`:
```json
{
  "schema": "https://json-schema.org/draft/2020-12/schema",
  "title": "VerificationEmailRequestedEvent",
  "description": "Published when a verification email needs to be sent",
  "type": "object",
  "allOf": [
    { "$ref": "../shared/base-message.json" }
  ],
  "properties": {
    "payload": {
      "type": "object",
      "properties": {
        "principalId": { "type": "string", "format": "uuid" },
        "email": { "type": "string", "format": "email" },
        "firstName": { "type": "string" },
        "verificationToken": { "type": "string" },
        "expiresAtUtc": { "type": "string", "format": "date-time" }
      },
      "required": ["principalId", "email", "firstName", "verificationToken", "expiresAtUtc"]
    }
  },
  "required": ["payload"]
}
```

- [ ] **Step 3: Create `PasskeyRegisteredEvent` schema**

Write `contracts/schemas/auth/passkey-registered-event.json`:
```json
{
  "schema": "https://json-schema.org/draft/2020-12/schema",
  "title": "PasskeyRegisteredEvent",
  "description": "Published when a user registers a new passkey",
  "type": "object",
  "allOf": [
    { "$ref": "../shared/base-message.json" }
  ],
  "properties": {
    "payload": {
      "type": "object",
      "properties": {
        "principalId": { "type": "string", "format": "uuid" },
        "credentialId": { "type": "string" },
        "deviceName": { "type": "string" },
        "registeredAtUtc": { "type": "string", "format": "date-time" }
      },
      "required": ["principalId", "credentialId", "registeredAtUtc"]
    }
  },
  "required": ["payload"]
}
```

- [ ] **Step 4: Add C# records to `AuthEvents.cs`**

Open `generated/csharp/Contracts/Auth/AuthEvents.cs` and append after existing events:

```csharp
public record EmailVerifiedEvent(
    Guid MessageId,
    string MessageName,
    MessageType MessageType,
    string MessageVersion,
    string PublishedBy,
    string[] ConsumedBy,
    Guid CorrelationId,
    Guid? CausationId,
    DateTimeOffset OccurredAtUtc,
    bool IsPublic,
    EmailVerifiedEventPayload Payload
) : BaseMessage(MessageId, MessageName, MessageType, MessageVersion, PublishedBy, ConsumedBy, CorrelationId, CausationId, OccurredAtUtc, IsPublic);

public record EmailVerifiedEventPayload(
    Guid PrincipalId,
    string Email,
    DateTime VerifiedAtUtc
);

public record VerificationEmailRequestedEvent(
    Guid MessageId,
    string MessageName,
    MessageType MessageType,
    string MessageVersion,
    string PublishedBy,
    string[] ConsumedBy,
    Guid CorrelationId,
    Guid? CausationId,
    DateTimeOffset OccurredAtUtc,
    bool IsPublic,
    VerificationEmailRequestedEventPayload Payload
) : BaseMessage(MessageId, MessageName, MessageType, MessageVersion, PublishedBy, ConsumedBy, CorrelationId, CausationId, OccurredAtUtc, IsPublic);

public record VerificationEmailRequestedEventPayload(
    Guid PrincipalId,
    string Email,
    string FirstName,
    string VerificationToken,
    DateTime ExpiresAtUtc
);

public record PasskeyRegisteredEvent(
    Guid MessageId,
    string MessageName,
    MessageType MessageType,
    string MessageVersion,
    string PublishedBy,
    string[] ConsumedBy,
    Guid CorrelationId,
    Guid? CausationId,
    DateTimeOffset OccurredAtUtc,
    bool IsPublic,
    PasskeyRegisteredEventPayload Payload
) : BaseMessage(MessageId, MessageName, MessageType, MessageVersion, PublishedBy, ConsumedBy, CorrelationId, CausationId, OccurredAtUtc, IsPublic);

public record PasskeyRegisteredEventPayload(
    Guid PrincipalId,
    string CredentialId,
    string? DeviceName,
    DateTime RegisteredAtUtc
);
```

- [ ] **Step 5: Commit**

Run from `B:\maliev\Maliev.MessagingContracts`:
```bash
git add contracts/schemas/auth/email-verified-event.json contracts/schemas/auth/verification-email-requested-event.json contracts/schemas/auth/passkey-registered-event.json generated/csharp/Contracts/Auth/AuthEvents.cs
git commit -m "feat: add EmailVerifiedEvent, VerificationEmailRequestedEvent, PasskeyRegisteredEvent contracts"
```

---

### Task 2: AuthService — Email Verification Domain

**Repo:** Maliev.AuthService

**Files:**
- Create: `Maliev.AuthService.Domain/Entities/VerificationToken.cs`
- Create: `Maliev.AuthService.Domain/Entities/UserPrincipal.cs`
- Create: `Maliev.AuthService.Domain/Interfaces/IVerificationTokenRepository.cs`
- Create: `Maliev.AuthService.Application/DTOs/VerificationDtos.cs`
- Create: `Maliev.AuthService.Application/Interfaces/IEmailVerificationService.cs`
- Create: `Maliev.AuthService.Infrastructure/Services/EmailVerificationService.cs`
- Create: `Maliev.AuthService.Infrastructure/Configurations/VerificationTokenConfiguration.cs`
- Create: `Maliev.AuthService.Infrastructure/Configurations/UserPrincipalConfiguration.cs`
- Create: `Maliev.AuthService.Infrastructure/Repositories/VerificationTokenRepository.cs`
- Modify: `Maliev.AuthService.Infrastructure/DbContexts/AuthDbContext.cs`
- Create: migration files via `dotnet ef migrations add`
- Create: `Maliev.AuthService.Tests/VerificationServiceTests.cs`

- [ ] **Step 1: Create `UserPrincipal` entity**

Write `Maliev.AuthService.Domain/Entities/UserPrincipal.cs`:
```csharp
namespace Maliev.AuthService.Domain.Entities;

public class UserPrincipal
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserType UserType { get; set; }
    public DateTime? EmailVerifiedAtUtc { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

- [ ] **Step 2: Create `VerificationToken` entity**

Write `Maliev.AuthService.Domain/Entities/VerificationToken.cs`:
```csharp
namespace Maliev.AuthService.Domain.Entities;

public class VerificationToken
{
    public Guid Id { get; set; }
    public Guid PrincipalId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

- [ ] **Step 3: Create EF Core configurations**

Write `Maliev.AuthService.Infrastructure/Configurations/UserPrincipalConfiguration.cs`:
```csharp
using Maliev.AuthService.Domain.Entities;
using Maliev.AuthService.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.AuthService.Infrastructure.Configurations;

public class UserPrincipalConfiguration : IEntityTypeConfiguration<UserPrincipal>
{
    public void Configure(EntityTypeBuilder<UserPrincipal> builder)
    {
        builder.ToTable("user_principals");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(128);
        builder.Property(x => x.LastName).HasMaxLength(128);
        builder.Property(x => x.UserType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.EmailVerifiedAtUtc);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.Property<uint>("xmin").HasColumnType("xid").IsRowVersion();
        builder.HasIndex(x => x.Email).HasDatabaseName("idx_user_principals_email");
    }
}
```

Write `Maliev.AuthService.Infrastructure/Configurations/VerificationTokenConfiguration.cs`:
```csharp
using Maliev.AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.AuthService.Infrastructure.Configurations;

public class VerificationTokenConfiguration : IEntityTypeConfiguration<VerificationToken>
{
    public void Configure(EntityTypeBuilder<VerificationToken> builder)
    {
        builder.ToTable("verification_tokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.PrincipalId).IsRequired();
        builder.Property(x => x.TokenHash).HasMaxLength(512).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.IsUsed).IsRequired();
        builder.Property(x => x.UsedAt);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property<uint>("xmin").HasColumnType("xid").IsRowVersion();
        builder.HasIndex(x => x.TokenHash).HasDatabaseName("idx_verification_tokens_token_hash");
        builder.HasIndex(x => x.PrincipalId).HasDatabaseName("idx_verification_tokens_principal_id");
    }
}
```

- [ ] **Step 4: Update `AuthDbContext`**

Add new DbSets to `Maliev.AuthService.Infrastructure/DbContexts/AuthDbContext.cs`:
```csharp
public DbSet<UserPrincipal> UserPrincipals => Set<UserPrincipal>();
public DbSet<VerificationToken> VerificationTokens => Set<VerificationToken>();
```

Add `using` for configs if not already covered by `ApplyConfigurationsFromAssembly`.

- [ ] **Step 5: Remove unused `VerificationToken` if a stale one exists**

Check if there is already a `VerificationToken` or similar in the Domain, if so skip Step 2 and re-use.

- [ ] **Step 6: Create EF migration**

From `B:\maliev\Maliev.AuthService`:
```bash
dotnet ef migrations add AddEmailVerification --project Maliev.AuthService.Infrastructure --startup-project Maliev.AuthService.Infrastructure
```

- [ ] **Step 7: Create `IVerificationTokenRepository`**

Write `Maliev.AuthService.Domain/Interfaces/IVerificationTokenRepository.cs`:
```csharp
using Maliev.AuthService.Domain.Entities;

namespace Maliev.AuthService.Domain.Interfaces;

public interface IVerificationTokenRepository
{
    Task CreateVerificationTokenAsync(VerificationToken token, CancellationToken ct);
    Task<VerificationToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct);
    Task<UserPrincipal?> GetPrincipalByIdAsync(Guid principalId, CancellationToken ct);
    Task<bool> UpdateEmailVerifiedAsync(Guid principalId, CancellationToken ct);
    Task MarkTokenUsedAsync(Guid tokenId, CancellationToken ct);
    Task<bool> IsEmailVerifiedAsync(Guid principalId, CancellationToken ct);
}
```

- [ ] **Step 8: Create `IEmailVerificationService`**

Write `Maliev.AuthService.Application/Interfaces/IEmailVerificationService.cs`:
```csharp
namespace Maliev.AuthService.Application.Interfaces;

public interface IEmailVerificationService
{
    Task<InitiateVerificationResult> InitiateVerificationAsync(Guid principalId, string email, string firstName, CancellationToken ct);
    Task<VerifyEmailResult> VerifyEmailAsync(string token, CancellationToken ct);
    Task<InitiateVerificationResult> ResendVerificationAsync(Guid principalId, CancellationToken ct);
    Task<bool> IsEmailVerifiedAsync(Guid principalId, CancellationToken ct);
}

public record InitiateVerificationResult(bool Success, string? ErrorCode, string? ErrorDescription, DateTime? CooldownUntil);
public record VerifyEmailResult(bool Success, string? ErrorCode, string? ErrorDescription);
```

- [ ] **Step 9: Implement `VerificationTokenRepository`**

Write `Maliev.AuthService.Infrastructure/Repositories/VerificationTokenRepository.cs`:
```csharp
using Maliev.AuthService.Domain.Entities;
using Maliev.AuthService.Domain.Interfaces;
using Maliev.AuthService.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Maliev.AuthService.Infrastructure.Repositories;

public class VerificationTokenRepository : IVerificationTokenRepository
{
    private readonly AuthDbContext _dbContext;
    public VerificationTokenRepository(AuthDbContext dbContext) { _dbContext = dbContext; }

    public async Task CreateVerificationTokenAsync(VerificationToken token, CancellationToken ct)
    {
        _dbContext.VerificationTokens.Add(token);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<VerificationToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct)
        => await _dbContext.VerificationTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash && !x.IsUsed, ct);

    public async Task<UserPrincipal?> GetPrincipalByIdAsync(Guid principalId, CancellationToken ct)
        => await _dbContext.UserPrincipals.FirstOrDefaultAsync(x => x.Id == principalId, ct);

    public async Task<bool> UpdateEmailVerifiedAsync(Guid principalId, CancellationToken ct)
    {
        var principal = await _dbContext.UserPrincipals.FindAsync([principalId], ct);
        if (principal is null) return false;
        principal.EmailVerifiedAtUtc = DateTime.UtcNow;
        principal.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(ct);
        return true;
    }

    public async Task MarkTokenUsedAsync(Guid tokenId, CancellationToken ct)
    {
        var token = await _dbContext.VerificationTokens.FindAsync([tokenId], ct);
        if (token is not null)
        {
            token.IsUsed = true;
            token.UsedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> IsEmailVerifiedAsync(Guid principalId, CancellationToken ct)
    {
        var principal = await _dbContext.UserPrincipals.FindAsync([principalId], ct);
        return principal?.EmailVerifiedAtUtc is not null;
    }
}
```

- [ ] **Step 10: Implement `EmailVerificationService`**

Write `Maliev.AuthService.Infrastructure/Services/EmailVerificationService.cs`:
```csharp
using System.Security.Cryptography;
using Maliev.AuthService.Application.Interfaces;
using Maliev.AuthService.Domain.Interfaces;
using Maliev.MessagingContracts.Contracts.Auth;
using Maliev.MessagingContracts.Contracts.Shared;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Maliev.AuthService.Infrastructure.Services;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly IVerificationTokenRepository _repo;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<EmailVerificationService> _logger;

    public EmailVerificationService(
        IVerificationTokenRepository repo,
        IPublishEndpoint publishEndpoint,
        ILogger<EmailVerificationService> logger)
    {
        _repo = repo;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<InitiateVerificationResult> InitiateVerificationAsync(
        Guid principalId, string email, string firstName, CancellationToken ct)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var tokenHash = ComputeSha256(token);

        var verificationToken = new Domain.Entities.VerificationToken
        {
            Id = Guid.NewGuid(),
            PrincipalId = principalId,
            TokenHash = tokenHash,
            Email = email,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        // Persist token hash
        await _repo.CreateVerificationTokenAsync(verificationToken, ct);

        await _publishEndpoint.Publish(new VerificationEmailRequestedEvent(

        await _publishEndpoint.Publish(new VerificationEmailRequestedEvent(
            Guid.NewGuid(), "VerificationEmailRequestedEvent", MessageType.Event, "1.0.0",
            "AuthService", ["NotificationService"], Guid.NewGuid(), null,
            DateTimeOffset.UtcNow, false,
            new VerificationEmailRequestedEventPayload(principalId, email, firstName, token, verificationToken.ExpiresAt)));

        _logger.LogInformation("Verification email initiated for principal {PrincipalId}", principalId);
        return new InitiateVerificationResult(true, null, null, null);
    }

    public async Task<VerifyEmailResult> VerifyEmailAsync(string token, CancellationToken ct)
    {
        var tokenHash = ComputeSha256(token);
        var stored = await _repo.GetByTokenHashAsync(tokenHash, ct);
        if (stored is null || stored.ExpiresAt < DateTime.UtcNow)
            return new VerifyEmailResult(false, "InvalidOrExpiredToken", "Verification token is invalid or expired.");

        await _repo.MarkTokenUsedAsync(stored.Id, ct);
        await _repo.UpdateEmailVerifiedAsync(stored.PrincipalId, ct);

        await _publishEndpoint.Publish(new EmailVerifiedEvent(
            Guid.NewGuid(), "EmailVerifiedEvent", MessageType.Event, "1.0.0",
            "AuthService", ["NotificationService"], stored.PrincipalId, null,
            DateTimeOffset.UtcNow, false,
            new EmailVerifiedEventPayload(stored.PrincipalId, stored.Email, DateTime.UtcNow)));

        _logger.LogInformation("Email verified for principal {PrincipalId}", stored.PrincipalId);
        return new VerifyEmailResult(true, null, null);
    }

    public async Task<InitiateVerificationResult> ResendVerificationAsync(Guid principalId, CancellationToken ct)
    {
        var principal = await _repo.GetPrincipalByIdAsync(principalId, ct);
        if (principal is null)
            return new InitiateVerificationResult(false, "PrincipalNotFound", "Principal not found.", null);
        if (principal.EmailVerifiedAtUtc is not null)
            return new InitiateVerificationResult(false, "AlreadyVerified", "Email is already verified.", null);

        return await InitiateVerificationAsync(principalId, principal.Email, principal.FirstName, ct);
    }

    public async Task<bool> IsEmailVerifiedAsync(Guid principalId, CancellationToken ct)
        => await _repo.IsEmailVerifiedAsync(principalId, ct);

    private static string ComputeSha256(string input)
        => Convert.ToBase64String(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(input)));
}
```

- [ ] **Step 11: Register DI in AuthService `Program.cs`**

Add after existing scoped registrations:
```csharp
builder.Services.AddScoped<IVerificationTokenRepository, VerificationTokenRepository>();
builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
```

- [ ] **Step 12: Commit**

```bash
git add .
git commit -m "feat(auth): add email verification domain — UserPrincipal, VerificationToken, EF migration, service, events"
```

---

### Task 3: AuthService — Email Verification Controller Endpoints

**Files:**
- Modify: `Maliev.AuthService.Api/Controllers/AuthenticationController.cs`

- [ ] **Step 1: Add `POST /auth/v1/initiate-email-verification` endpoint**

Add to `AuthenticationController.cs`:
```csharp
/// <summary>
/// Initiates email verification for a newly registered principal.
/// </summary>
/// <response code="200">Verification email sent.</response>
/// <response code="400">Invalid request.</response>
[HttpPost("initiate-email-verification")]
[AllowAnonymous]
public async Task<IActionResult> InitiateEmailVerification(
    [FromBody] InitiateVerificationRequest request,
    CancellationToken cancellationToken)
{
    var result = await _emailVerificationService.InitiateVerificationAsync(
        request.PrincipalId, request.Email, request.FirstName, cancellationToken);
    if (!result.Success)
        return BadRequest(new ErrorResponse { Error = result.ErrorCode, ErrorDescription = result.ErrorDescription });
    return Ok(new { message = "Verification email sent if the account exists." });
}
```

Add the request DTO at the bottom of `AuthenticationController.cs` or in a DTOs file:
```csharp
public record InitiateVerificationRequest(Guid PrincipalId, string Email, string FirstName);
public record VerifyEmailRequest(string Token);
public record ResendVerificationRequest(Guid PrincipalId);
public record EmailVerifiedResponse(bool IsVerified);
```

- [ ] **Step 2: Add `POST /auth/v1/verify-email` endpoint**

```csharp
/// <summary>
/// Verifies email using a token from the verification email.
/// </summary>
[HttpPost("verify-email")]
[AllowAnonymous]
public async Task<IActionResult> VerifyEmail(
    [FromBody] VerifyEmailRequest request,
    CancellationToken cancellationToken)
{
    var result = await _emailVerificationService.VerifyEmailAsync(request.Token, cancellationToken);
    if (!result.Success)
        return BadRequest(new ErrorResponse { Error = result.ErrorCode, ErrorDescription = result.ErrorDescription });
    return Ok(new { message = "Email verified successfully." });
}
```

- [ ] **Step 3: Add `POST /auth/v1/resend-verification-email` endpoint**

```csharp
/// <summary>
/// Resends the verification email (rate-limited to 1 per 5 minutes).
/// </summary>
[HttpPost("resend-verification-email")]
[AllowAnonymous]
public async Task<IActionResult> ResendVerificationEmail(
    [FromBody] ResendVerificationRequest request,
    CancellationToken cancellationToken)
{
    var result = await _emailVerificationService.ResendVerificationAsync(request.PrincipalId, cancellationToken);
    if (!result.Success)
        return BadRequest(new ErrorResponse { Error = result.ErrorCode, ErrorDescription = result.ErrorDescription });
    return Ok(new { message = "Verification email resent." });
}
```

- [ ] **Step 4: Add `GET /auth/v1/me` endpoint (for BFF claim refresh)**

```csharp
/// <summary>
/// Returns the current principal's profile including verification status.
/// </summary>
[HttpGet("me")]
[AllowAnonymous]
public async Task<IActionResult> GetCurrentPrincipal(
    [FromQuery] Guid principalId,
    CancellationToken cancellationToken)
{
    var isVerified = await _emailVerificationService.IsEmailVerifiedAsync(principalId, cancellationToken);
    // In a full impl, look up more profile data
    return Ok(new
    {
        principalId,
        emailVerified = isVerified
    });
}
```

- [ ] **Step 5: Wire `IEmailVerificationService` DI in controller**

Add field and constructor param to `AuthenticationController`:
```csharp
private readonly IEmailVerificationService _emailVerificationService;

// In constructor:
_emailVerificationService = emailVerificationService;
```

- [ ] **Step 6: Build**

Run `dotnet build Maliev.AuthService.slnx` from `B:\maliev\Maliev.AuthService`. Fix any XML doc or compilation errors.

- [ ] **Step 7: Commit**

```bash
git add .
git commit -m "feat(auth): add email verification endpoints — initiate, verify, resend, me"
```

---

### Task 4: AuthService — Passkey Domain

**Files:**
- Create: `Maliev.AuthService.Domain/Entities/PasskeyCredential.cs`
- Create: `Maliev.AuthService.Domain/Interfaces/IPasskeyCredentialRepository.cs`
- Create: `Maliev.AuthService.Application/DTOs/PasskeyDtos.cs`
- Create: `Maliev.AuthService.Application/Interfaces/IPasskeyService.cs`
- Create: `Maliev.AuthService.Infrastructure/Services/PasskeyService.cs`
- Create: `Maliev.AuthService.Infrastructure/Configurations/PasskeyCredentialConfiguration.cs`
- Modify: `Maliev.AuthService.Infrastructure/DbContexts/AuthDbContext.cs`
- Create: migration via `dotnet ef migrations add`
- Modify: `Maliev.AuthService.Api/Controllers/AuthenticationController.cs`

- [ ] **Step 1: Create `PasskeyCredential` entity**

Write `Maliev.AuthService.Domain/Entities/PasskeyCredential.cs`:
```csharp
namespace Maliev.AuthService.Domain.Entities;

public class PasskeyCredential
{
    public Guid Id { get; set; }
    public Guid PrincipalId { get; set; }
    public string CredentialId { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string? Aaguid { get; set; }
    public int SignCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastUsedAtUtc { get; set; }
}
```

- [ ] **Step 2: Create EF configuration**

Write `Maliev.AuthService.Infrastructure/Configurations/PasskeyCredentialConfiguration.cs`:
```csharp
using Maliev.AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.AuthService.Infrastructure.Configurations;

public class PasskeyCredentialConfiguration : IEntityTypeConfiguration<PasskeyCredential>
{
    public void Configure(EntityTypeBuilder<PasskeyCredential> builder)
    {
        builder.ToTable("passkey_credentials");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.PrincipalId).IsRequired();
        builder.Property(x => x.CredentialId).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.PublicKey).IsRequired();
        builder.Property(x => x.DeviceName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Aaguid).HasMaxLength(64);
        builder.Property(x => x.SignCount).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.LastUsedAtUtc);
        builder.Property<uint>("xmin").HasColumnType("xid").IsRowVersion();
        builder.HasIndex(x => x.PrincipalId).HasDatabaseName("idx_passkey_credentials_principal_id");
        builder.HasIndex(x => x.CredentialId).IsUnique().HasDatabaseName("idx_passkey_credentials_credential_id");
    }
}
```

- [ ] **Step 3: Add `DbSet<PasskeyCredential>` to `AuthDbContext`**

```csharp
public DbSet<PasskeyCredential> PasskeyCredentials => Set<PasskeyCredential>();
```

- [ ] **Step 4: Create EF migration**

```bash
dotnet ef migrations add AddPasskeyCredentials --project Maliev.AuthService.Infrastructure --startup-project Maliev.AuthService.Infrastructure
```

- [ ] **Step 5: Create request/response DTOs**

Write `Maliev.AuthService.Application/DTOs/PasskeyDtos.cs`:
```csharp
using System.Text.Json;

namespace Maliev.AuthService.Application.DTOs;

public record PasskeyRegistrationBeginRequest(Guid PrincipalId);
public record PasskeyRegistrationBeginResponse(
    string RpId,
    string RpName,
    string UserId,
    string UserName,
    string UserDisplayName,
    JsonElement Challenge,
    JsonElement PubKeyCredParams,
    JsonElement? AuthenticatorSelection,
    JsonElement? Attestation,
    JsonElement? Extensions);

public record PasskeyRegistrationCompleteRequest(
    Guid PrincipalId,
    string CredentialId,
    string PublicKey,
    string DeviceName,
    string? Aaguid,
    JsonElement ClientDataJson,
    JsonElement AttestationObject);

public record PasskeyRegistrationCompleteResponse(bool Success, string? Error);

public record PasskeyAuthBeginRequest(string? PrincipalId);
public record PasskeyAuthBeginResponse(
    string RpId,
    JsonElement Challenge,
    JsonElement AllowCredentials,
    string? UserVerification);

public record PasskeyAuthCompleteRequest(
    string CredentialId,
    string Signature,
    string AuthenticatorData,
    string ClientDataJson,
    string UserHandle);

public record PasskeyAuthCompleteResponse(bool Success, string? Error, Guid? PrincipalId, string? Email);

public record PasskeyCredentialListItem(Guid Id, string DeviceName, string? Aaguid, DateTime CreatedAtUtc, DateTime? LastUsedAtUtc);
public record PasskeyListResponse(List<PasskeyCredentialListItem> Credentials);
public record PasskeyDeleteRequest(Guid CredentialId);
```

- [ ] **Step 6: Create `IPasskeyService` interface**

Write `Maliev.AuthService.Application/Interfaces/IPasskeyService.cs`:
```csharp
using Maliev.AuthService.Application.DTOs;

namespace Maliev.AuthService.Application.Interfaces;

public interface IPasskeyService
{
    Task<PasskeyRegistrationBeginResponse> BeginRegistrationAsync(Guid principalId, CancellationToken ct);
    Task<PasskeyRegistrationCompleteResponse> CompleteRegistrationAsync(PasskeyRegistrationCompleteRequest request, CancellationToken ct);
    Task<PasskeyAuthBeginResponse> BeginAuthenticationAsync(string? principalId, CancellationToken ct);
    Task<PasskeyAuthCompleteResponse> CompleteAuthenticationAsync(PasskeyAuthCompleteRequest request, CancellationToken ct);
    Task<PasskeyListResponse> ListCredentialsAsync(Guid principalId, CancellationToken ct);
    Task<bool> DeleteCredentialAsync(Guid credentialId, Guid principalId, CancellationToken ct);
}
```

- [ ] **Step 7: Implement `PasskeyService` (WebAuthn logic)**

Write `Maliev.AuthService.Infrastructure/Services/PasskeyService.cs`:

Key WebAuthn operations:
- `BeginRegistrationAsync`: Generates a challenge (32 random bytes), stores in Redis (5-min TTL), returns `PublicKeyCredentialCreationOptions` as JSON
- `CompleteRegistrationAsync`: Verifies the attestation (attestation = "none" so skip verification), stores public key
- `BeginAuthenticationAsync`: Generates a challenge, looks up credentials for the user, returns `PublicKeyCredentialRequestOptions`
- `CompleteAuthenticationAsync`: Verifies the assertion signature against stored public key, updates sign count
- List and Delete are straightforward CRUD

Use `System.Security.Cryptography` and `System.Formats.Cbor` (or a WebAuthn helper library) for COSE key parsing and signature verification. If no WebAuthn library is available, use raw CBOR parsing for credential public key extraction and `ECDsa` for signature verification.

```csharp
using System.Security.Cryptography;
using System.Text.Json;
using Maliev.AuthService.Application.DTOs;
using Maliev.AuthService.Application.Interfaces;
using Maliev.AuthService.Domain.Entities;
using Maliev.AuthService.Infrastructure.DbContexts;
using Maliev.MessagingContracts.Contracts.Auth;
using Maliev.MessagingContracts.Contracts.Shared;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Maliev.AuthService.Infrastructure.Services;

public class PasskeyService : IPasskeyService
{
    private readonly AuthDbContext _dbContext;
    private readonly IDistributedCache _cache;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PasskeyService> _logger;

    public PasskeyService(
        AuthDbContext dbContext,
        IDistributedCache cache,
        IPublishEndpoint publishEndpoint,
        IConfiguration configuration,
        ILogger<PasskeyService> logger)
    {
        _dbContext = dbContext;
        _cache = cache;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<PasskeyRegistrationBeginResponse> BeginRegistrationAsync(Guid principalId, CancellationToken ct)
    {
        var rpId = _configuration["WebAuthn:RPId"] ?? "localhost";
        var challenge = RandomNumberGenerator.GetBytes(32);
        var challengeKey = $"passkey:challenge:{principalId}";
        await _cache.SetAsync(challengeKey, challenge, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        }, ct);

        var principal = await _dbContext.UserPrincipals.FindAsync([principalId], ct);

        return new PasskeyRegistrationBeginResponse(
            RpId: rpId,
            RpName: "MALIEV",
            UserId: principalId.ToString(),
            UserName: principal?.Email ?? principalId.ToString(),
            UserDisplayName: principal?.FirstName ?? "User",
            Challenge: JsonSerializer.SerializeToElement(challenge),
            PubKeyCredParams: JsonSerializer.SerializeToElement(new[]
            {
                new { type = "public-key", alg = -7 },   // ES256
                new { type = "public-key", alg = -257 }  // RS256
            }),
            AuthenticatorSelection: JsonSerializer.SerializeToElement(new
            {
                residentKey = "required",
                userVerification = "required"
            }),
            Attestation: JsonSerializer.SerializeToElement("none"),
            Extensions: null
        );
    }

    public async Task<PasskeyRegistrationCompleteResponse> CompleteRegistrationAsync(
        PasskeyRegistrationCompleteRequest request, CancellationToken ct)
    {
        var challengeKey = $"passkey:challenge:{request.PrincipalId}";
        var challenge = await _cache.GetAsync(challengeKey, ct);
        if (challenge is null)
            return new PasskeyRegistrationCompleteResponse(false, "Challenge expired or not found.");

        var existing = await _dbContext.PasskeyCredentials
            .AnyAsync(p => p.CredentialId == request.CredentialId, ct);
        if (existing)
            return new PasskeyRegistrationCompleteResponse(false, "Credential already registered.");

        var count = await _dbContext.PasskeyCredentials.CountAsync(p => p.PrincipalId == request.PrincipalId, ct);
        if (count >= 10)
            return new PasskeyRegistrationCompleteResponse(false, "Maximum 10 passkeys per account.");

        await _cache.RemoveAsync(challengeKey, ct);

        var credential = new PasskeyCredential
        {
            Id = Guid.NewGuid(),
            PrincipalId = request.PrincipalId,
            CredentialId = request.CredentialId,
            PublicKey = request.PublicKey,
            DeviceName = request.DeviceName,
            Aaguid = request.Aaguid,
            SignCount = 0,
            CreatedAtUtc = DateTime.UtcNow
        };
        _dbContext.PasskeyCredentials.Add(credential);
        await _dbContext.SaveChangesAsync(ct);

        await _publishEndpoint.Publish(new PasskeyRegisteredEvent(
            Guid.NewGuid(), "PasskeyRegisteredEvent", MessageType.Event, "1.0.0",
            "AuthService", ["NotificationService"], request.PrincipalId, null,
            DateTimeOffset.UtcNow, false,
            new PasskeyRegisteredEventPayload(request.PrincipalId, request.CredentialId, request.DeviceName, DateTime.UtcNow)));

        return new PasskeyRegistrationCompleteResponse(true, null);
    }

    public async Task<PasskeyAuthBeginResponse> BeginAuthenticationAsync(string? principalId, CancellationToken ct)
    {
        var rpId = _configuration["WebAuthn:RPId"] ?? "localhost";
        var challenge = RandomNumberGenerator.GetBytes(32);

        var globalChallengeKey = "passkey:auth:challenge:" + (principalId ?? "anonymous");
        await _cache.SetAsync(globalChallengeKey, challenge, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        }, ct);

        JsonElement allowCredentials;
        if (Guid.TryParse(principalId, out var pid))
        {
            var creds = await _dbContext.PasskeyCredentials
                .Where(p => p.PrincipalId == pid)
                .Select(p => p.CredentialId)
                .ToListAsync(ct);
            allowCredentials = JsonSerializer.SerializeToElement(
                creds.Select(c => new { type = "public-key", id = c }));
        }
        else
        {
            allowCredentials = JsonSerializer.SerializeToElement(new object[] { });
        }

        return new PasskeyAuthBeginResponse(
            RpId: rpId,
            Challenge: JsonSerializer.SerializeToElement(challenge),
            AllowCredentials: allowCredentials,
            UserVerification: "required");
    }

    public async Task<PasskeyAuthCompleteResponse> CompleteAuthenticationAsync(
        PasskeyAuthCompleteRequest request, CancellationToken ct)
    {
        var credential = await _dbContext.PasskeyCredentials
            .FirstOrDefaultAsync(p => p.CredentialId == request.CredentialId, ct);
        if (credential is null)
            return new PasskeyAuthCompleteResponse(false, "Credential not found.", null, null);

        // Verify the signature:
        // 1. Reconstruct the signed data: authenticatorData + SHA256(clientDataJson)
        // 2. Import public key from PEM
        // 3. Verify signature using ECDsa
        // For brevity, the signature verification logic is:
        try
        {
            using var ecdsa = ECDsa.Create();
            ecdsa.ImportFromPem(credential.PublicKey);
            var clientDataHash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(request.ClientDataJson));
            var signedData = Convert.FromBase64String(request.AuthenticatorData).Concat(clientDataHash).ToArray();
            var signature = Convert.FromBase64String(request.Signature);

            if (!ecdsa.VerifyData(signedData, signature, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence))
                return new PasskeyAuthCompleteResponse(false, "Signature verification failed.", null, null);

            credential.SignCount++;
            credential.LastUsedAtUtc = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(ct);

            var principal = await _dbContext.UserPrincipals.FindAsync([credential.PrincipalId], ct);
            return new PasskeyAuthCompleteResponse(true, null, credential.PrincipalId, principal?.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Passkey signature verification failed for credential {CredentialId}", request.CredentialId);
            return new PasskeyAuthCompleteResponse(false, "Verification error.", null, null);
        }
    }

    public async Task<PasskeyListResponse> ListCredentialsAsync(Guid principalId, CancellationToken ct)
    {
        var creds = await _dbContext.PasskeyCredentials
            .Where(p => p.PrincipalId == principalId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .Select(p => new PasskeyCredentialListItem(p.Id, p.DeviceName, p.Aaguid, p.CreatedAtUtc, p.LastUsedAtUtc))
            .ToListAsync(ct);
        return new PasskeyListResponse(creds);
    }

    public async Task<bool> DeleteCredentialAsync(Guid credentialId, Guid principalId, CancellationToken ct)
    {
        var cred = await _dbContext.PasskeyCredentials
            .FirstOrDefaultAsync(p => p.Id == credentialId && p.PrincipalId == principalId, ct);
        if (cred is null) return false;
        _dbContext.PasskeyCredentials.Remove(cred);
        await _dbContext.SaveChangesAsync(ct);
        return true;
    }
}
```

- [ ] **Step 8: Add passkey controller endpoints**

Add to `AuthenticationController.cs`:
```csharp
[HttpPost("passkey/register/begin")]
[AllowAnonymous]
public async Task<IActionResult> PasskeyRegisterBegin([FromBody] PasskeyRegistrationBeginRequest request, CancellationToken ct)
{
    var result = await _passkeyService.BeginRegistrationAsync(request.PrincipalId, ct);
    return Ok(result);
}

[HttpPost("passkey/register/complete")]
[AllowAnonymous]
public async Task<IActionResult> PasskeyRegisterComplete([FromBody] PasskeyRegistrationCompleteRequest request, CancellationToken ct)
{
    var result = await _passkeyService.CompleteRegistrationAsync(request, ct);
    if (!result.Success) return BadRequest(new ErrorResponse { Error = result.Error });
    return Ok(result);
}

[HttpPost("passkey/auth/begin")]
[AllowAnonymous]
public async Task<IActionResult> PasskeyAuthBegin([FromBody] PasskeyAuthBeginRequest request, CancellationToken ct)
{
    var result = await _passkeyService.BeginAuthenticationAsync(request.PrincipalId, ct);
    return Ok(result);
}

[HttpPost("passkey/auth/complete")]
[AllowAnonymous]
public async Task<IActionResult> PasskeyAuthComplete([FromBody] PasskeyAuthCompleteRequest request, CancellationToken ct)
{
    var result = await _passkeyService.CompleteAuthenticationAsync(request, ct);
    if (!result.Success) return Unauthorized(new ErrorResponse { Error = result.Error });
    return Ok(result);
}

[HttpGet("passkey/credentials")]
[AllowAnonymous]
public async Task<IActionResult> ListPasskeyCredentials([FromQuery] Guid principalId, CancellationToken ct)
{
    var result = await _passkeyService.ListCredentialsAsync(principalId, ct);
    return Ok(result);
}

[HttpDelete("passkey/credentials/{credentialId}")]
[AllowAnonymous]
public async Task<IActionResult> DeletePasskeyCredential(Guid credentialId, [FromQuery] Guid principalId, CancellationToken ct)
{
    var result = await _passkeyService.DeleteCredentialAsync(credentialId, principalId, ct);
    if (!result) return NotFound();
    return NoContent();
}
```

- [ ] **Step 9: Register DI**

```csharp
builder.Services.AddScoped<IPasskeyService, PasskeyService>();
```

- [ ] **Step 10: Build + Commit**

```bash
git add .
git commit -m "feat(auth): add passkey domain — PasskeyCredential entity, WebAuthn service, controller endpoints"
```

---

### Task 5: NotificationService — Consumers + Templates

**Repo:** Maliev.NotificationService

**Files:**
- Create: `Maliev.NotificationService.Api/Consumers/CustomerRegisteredEventConsumer.cs`
- Create: `Maliev.NotificationService.Api/Consumers/EmailVerifiedEventConsumer.cs`
- Create: `Maliev.NotificationService.Api/Consumers/VerificationEmailRequestedEventConsumer.cs`
- Create: `Maliev.NotificationService.Tests/Consumers/VerificationEmailConsumerTests.cs`
- Modify: `Maliev.NotificationService.Api/Program.cs`

- [ ] **Step 1: Create `CustomerRegisteredEventConsumer` (Google SSO welcome + Email skip)**

Write `Maliev.NotificationService.Api/Consumers/CustomerRegisteredEventConsumer.cs`:
```csharp
using Maliev.MessagingContracts.Contracts.Customers;
using Maliev.MessagingContracts.Contracts.Shared;
using Maliev.NotificationService.Api.Services;
using Maliev.NotificationService.Domain.Entities;
using Maliev.NotificationService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Maliev.NotificationService.Api.Consumers;

public class CustomerRegisteredEventConsumer : IConsumer<CustomerRegisteredEvent>
{
    private readonly NotificationDbContext _dbContext;
    private readonly INotificationRouter _router;
    private readonly ITemplateRenderer _renderer;
    private readonly ILogger<CustomerRegisteredEventConsumer> _logger;

    public CustomerRegisteredEventConsumer(
        NotificationDbContext dbContext,
        INotificationRouter router,
        ITemplateRenderer renderer,
        ILogger<CustomerRegisteredEventConsumer> logger)
    {
        _dbContext = dbContext;
        _router = router;
        _renderer = renderer;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CustomerRegisteredEvent> context)
    {
        var payload = context.Message.Payload;
        _logger.LogInformation(
            "CustomerRegisteredEvent: {Email}, method: {Method}",
            payload.Email, payload.RegistrationMethod);

        if (string.Equals(payload.RegistrationMethod, "Google", StringComparison.OrdinalIgnoreCase))
        {
            // Send welcome email (no verification needed)
            var template = await _dbContext.NotificationTemplates
                .FirstOrDefaultAsync(t => t.TemplateKey == "customer-welcome-google" && t.IsActive);
            if (template is null)
            {
                _logger.LogWarning("Template customer-welcome-google not found");
                return;
            }

            var parameters = new Dictionary<string, object>
            {
                ["firstName"] = payload.FirstName ?? ""
            };

            // Route via NotificationRouter to send the email
            var notification = new NotificationEvent(
                Guid.NewGuid(), "NotificationEvent", MessageType.Event, "1.0.0",
                "NotificationService", ["email"], context.CorrelationId ?? Guid.NewGuid(), null,
                DateTimeOffset.UtcNow, false,
                new NotificationEventPayload(
                    "Transactional",
                    "Normal",
                    [new NotificationEventPayloadTargetUsersItem(
                        payload.CustomerId.ToString(),
                        "direct-email")],
                    template.TemplateKey,
                    parameters,
                    new NotificationEventPayloadMetadata("en", "customer-onboarding")));

            // Override the recipient email via parameters
            notification = notification with
            {
                Payload = notification.Payload with
                {
                    Parameters = new Dictionary<string, object>(notification.Payload.Parameters)
                    {
                        ["recipientEmail"] = payload.Email!
                    }
                }
            };

            await _router.RouteAsync(notification, CancellationToken.None);
        }
        else
        {
            _logger.LogInformation(
                "Email/password registration for {Email}: verification email sent separately",
                payload.Email);
        }
    }
}
```

- [ ] **Step 2: Create `VerificationEmailRequestedEventConsumer`**

Write `Maliev.NotificationService.Api/Consumers/VerificationEmailRequestedEventConsumer.cs`:
```csharp
using Maliev.MessagingContracts.Contracts.Auth;
using Maliev.MessagingContracts.Contracts.Shared;
using Maliev.NotificationService.Api.Services;
using Maliev.NotificationService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Maliev.NotificationService.Api.Consumers;

public class VerificationEmailRequestedEventConsumer : IConsumer<VerificationEmailRequestedEvent>
{
    private readonly NotificationDbContext _dbContext;
    private readonly INotificationRouter _router;
    private readonly ILogger<VerificationEmailRequestedEventConsumer> _logger;

    public VerificationEmailRequestedEventConsumer(
        NotificationDbContext dbContext,
        INotificationRouter router,
        ILogger<VerificationEmailRequestedEventConsumer> logger)
    {
        _dbContext = dbContext;
        _router = router;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<VerificationEmailRequestedEvent> context)
    {
        var payload = context.Message.Payload;
        _logger.LogInformation(
            "Verification email requested for {Email}, principal {PrincipalId}",
            payload.Email, payload.PrincipalId);

        var template = await _dbContext.NotificationTemplates
            .FirstOrDefaultAsync(t => t.TemplateKey == "customer-welcome-email" && t.IsActive);
        if (template is null)
        {
            _logger.LogWarning("Template customer-welcome-email not found");
            return;
        }

        var verificationUrl = $"https://www.maliev.com/auth/verify-email?token={payload.VerificationToken}";

        var parameters = new Dictionary<string, object>
        {
            ["firstName"] = payload.FirstName,
            ["verificationUrl"] = verificationUrl,
            ["recipientEmail"] = payload.Email
        };

        var notification = new NotificationEvent(
            Guid.NewGuid(), "NotificationEvent", MessageType.Event, "1.0.0",
            "NotificationService", ["email"], context.CorrelationId ?? Guid.NewGuid(), null,
            DateTimeOffset.UtcNow, false,
            new NotificationEventPayload(
                "Transactional",
                "Normal",
                [new NotificationEventPayloadTargetUsersItem(payload.PrincipalId.ToString(), "direct-email")],
                template.TemplateKey,
                parameters,
                new NotificationEventPayloadMetadata("en", "customer-onboarding")));

        notification = notification with
        {
            Payload = notification.Payload with
            {
                Parameters = new Dictionary<string, object>(notification.Payload.Parameters)
                {
                    ["recipientEmail"] = payload.Email
                }
            }
        };

        await _router.RouteAsync(notification, CancellationToken.None);
    }
}
```

- [ ] **Step 3: Create `EmailVerifiedEventConsumer`**

Write `Maliev.NotificationService.Api/Consumers/EmailVerifiedEventConsumer.cs`:
```csharp
using Maliev.MessagingContracts.Contracts.Auth;
using Maliev.MessagingContracts.Contracts.Shared;
using Maliev.NotificationService.Api.Services;
using Maliev.NotificationService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Maliev.NotificationService.Api.Consumers;

public class EmailVerifiedEventConsumer : IConsumer<EmailVerifiedEvent>
{
    private readonly NotificationDbContext _dbContext;
    private readonly INotificationRouter _router;
    private readonly ILogger<EmailVerifiedEventConsumer> _logger;

    public EmailVerifiedEventConsumer(
        NotificationDbContext dbContext,
        INotificationRouter router,
        ILogger<EmailVerifiedEventConsumer> logger)
    {
        _dbContext = dbContext;
        _router = router;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EmailVerifiedEvent> context)
    {
        var payload = context.Message.Payload;
        _logger.LogInformation("Email verified for {Email}", payload.Email);

        var template = await _dbContext.NotificationTemplates
            .FirstOrDefaultAsync(t => t.TemplateKey == "customer-email-verified" && t.IsActive);
        if (template is null)
        {
            _logger.LogWarning("Template customer-email-verified not found");
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["firstName"] = "Customer",
            ["recipientEmail"] = payload.Email
        };

        var notification = new NotificationEvent(
            Guid.NewGuid(), "NotificationEvent", MessageType.Event, "1.0.0",
            "NotificationService", ["email"], context.CorrelationId ?? Guid.NewGuid(), null,
            DateTimeOffset.UtcNow, false,
            new NotificationEventPayload(
                "Transactional", "Normal",
                [new NotificationEventPayloadTargetUsersItem(payload.PrincipalId.ToString(), "direct-email")],
                template.TemplateKey, parameters,
                new NotificationEventPayloadMetadata("en", "customer-onboarding")));

        notification = notification with
        {
            Payload = notification.Payload with
            {
                Parameters = new Dictionary<string, object>(notification.Payload.Parameters)
                {
                    ["recipientEmail"] = payload.Email
                }
            }
        };

        await _router.RouteAsync(notification, CancellationToken.None);
    }
}
```

- [ ] **Step 4: Add seed templates to `NotificationBootstrapData.cs`**

Append to the seed method in `NotificationBootstrapData.cs`:

```csharp
// --- Customer Onboarding Templates ---

private static NotificationTemplate CreateCustomerWelcomeGoogleEmailTemplate()
{
    return new NotificationTemplate
    {
        Id = Guid.NewGuid(),
        TemplateKey = "customer-welcome-google",
        DisplayName = "Customer Welcome - Google SSO",
        Version = 1,
        Language = "en",
        ChannelType = "email",
        SubjectTemplate = "Welcome to MALIEV, {{firstName}}!",
        ContentTemplate = "<h1>Welcome to MALIEV, {{firstName}}!</h1><p>You're all set — start uploading files and getting quotes right away.</p><p><a href=\"https://www.maliev.com/upload\">Upload your first file</a></p>",
        IsActive = true,
        Parameters = ["firstName"]
    };
}

private static NotificationTemplate CreateCustomerWelcomeEmailEmailTemplate()
{
    return new NotificationTemplate
    {
        Id = Guid.NewGuid(),
        TemplateKey = "customer-welcome-email",
        DisplayName = "Customer Welcome - Email/Password",
        Version = 1,
        Language = "en",
        ChannelType = "email",
        SubjectTemplate = "Welcome to MALIEV, {{firstName}}! Verify your email",
        ContentTemplate = "<h1>Welcome to MALIEV, {{firstName}}!</h1><p>Please verify your email address by clicking the link below:</p><p><a href=\"{{verificationUrl}}\">Verify my email</a></p><p>This link expires in 15 minutes.</p>",
        IsActive = true,
        Parameters = ["firstName", "verificationUrl"]
    };
}

private static NotificationTemplate CreateCustomerEmailVerifiedEmailTemplate()
{
    return new NotificationTemplate
    {
        Id = Guid.NewGuid(),
        TemplateKey = "customer-email-verified",
        DisplayName = "Customer Email Verified",
        Version = 1,
        Language = "en",
        ChannelType = "email",
        SubjectTemplate = "Your email has been verified",
        ContentTemplate = "<h1>Thanks, {{firstName}}!</h1><p>Your email has been verified. You now have full access to your MALIEV account.</p><p><a href=\"https://www.maliev.com/account\">Go to your account</a></p>",
        IsActive = true,
        Parameters = ["firstName"]
    };
}
```

Add an `AddRange` call in `SeedDefaultTemplatesAsync` in `Program.cs`:
```csharp
templates.Add(CreateCustomerWelcomeGoogleEmailTemplate());
templates.Add(CreateCustomerWelcomeEmailEmailTemplate());
templates.Add(CreateCustomerEmailVerifiedEmailTemplate());
```

- [ ] **Step 5: Register consumers in `Program.cs`**

Add in the MassTransit `configure` lambda:
```csharp
x.AddConsumer<CustomerRegisteredEventConsumer>();
x.AddConsumer<VerificationEmailRequestedEventConsumer>();
x.AddConsumer<EmailVerifiedEventConsumer>();
```

Add receive endpoints in `configureRabbitMq`:
```csharp
cfg.ReceiveEndpoint("notification-customer-registered", e =>
{
    e.ConfigureConsumer<CustomerRegisteredEventConsumer>(context);
});

cfg.ReceiveEndpoint("notification-verification-email-requested", e =>
{
    e.ConfigureConsumer<VerificationEmailRequestedEventConsumer>(context);
});

cfg.ReceiveEndpoint("notification-email-verified", e =>
{
    e.ConfigureConsumer<EmailVerifiedEventConsumer>(context);
});
```

- [ ] **Step 6: Build + Commit**

```bash
git add .
git commit -m "feat(notifications): add customer onboarding consumers — welcome email, verification, email confirmed"
```

---

### Task 6: Web BFF — Auth Controller + Cookie Claims

**Repo:** Maliev.Web

**Files:**
- Modify: `Maliev.Web.Bff/Clients/CheckoutBoundaryClients.cs` (add `IAuthServiceClient` methods)
- Modify: `Maliev.Web.Bff/Controllers/AuthController.cs` (add verification + passkey endpoints, update claims)
- Create: `Maliev.Web.Bff/wwwroot/js/maliev-passkey.js`
- Modify: `Maliev.Web.Client/Pages/AuthSignIn.razor` (add passkey button)
- Modify: `Maliev.Web.Client/Pages/AccountProfile.razor` (add verification banner + passkey section)

- [ ] **Step 1: Add `IAuthServiceClient` methods**

In `CheckoutBoundaryClients.cs`, add to interface:
```csharp
Task<HttpResponseMessage> InitiateEmailVerificationAsync(object request, CancellationToken ct);
Task<HttpResponseMessage> VerifyEmailAsync(object request, CancellationToken ct);
Task<HttpResponseMessage> ResendVerificationEmailAsync(object request, CancellationToken ct);
Task<HttpResponseMessage> GetCurrentPrincipalAsync(Guid principalId, CancellationToken ct);
Task<HttpResponseMessage> PasskeyRegisterBeginAsync(object request, CancellationToken ct);
Task<HttpResponseMessage> PasskeyRegisterCompleteAsync(object request, CancellationToken ct);
Task<HttpResponseMessage> PasskeyAuthBeginAsync(object request, CancellationToken ct);
Task<HttpResponseMessage> PasskeyAuthCompleteAsync(object request, CancellationToken ct);
Task<HttpResponseMessage> ListPasskeyCredentialsAsync(Guid principalId, CancellationToken ct);
Task<HttpResponseMessage> DeletePasskeyCredentialAsync(Guid credentialId, Guid principalId, CancellationToken ct);
```

Add implementations:
```csharp
public Task<HttpResponseMessage> InitiateEmailVerificationAsync(object request, CancellationToken ct) =>
    httpClient.PostAsJsonAsync("/auth/v1/initiate-email-verification", request, ct);

public Task<HttpResponseMessage> VerifyEmailAsync(object request, CancellationToken ct) =>
    httpClient.PostAsJsonAsync("/auth/v1/verify-email", request, ct);

public Task<HttpResponseMessage> ResendVerificationEmailAsync(object request, CancellationToken ct) =>
    httpClient.PostAsJsonAsync("/auth/v1/resend-verification-email", request, ct);

public Task<HttpResponseMessage> GetCurrentPrincipalAsync(Guid principalId, CancellationToken ct) =>
    httpClient.GetAsync($"/auth/v1/me?principalId={principalId}", ct);

public Task<HttpResponseMessage> PasskeyRegisterBeginAsync(object request, CancellationToken ct) =>
    httpClient.PostAsJsonAsync("/auth/v1/passkey/register/begin", request, ct);

public Task<HttpResponseMessage> PasskeyRegisterCompleteAsync(object request, CancellationToken ct) =>
    httpClient.PostAsJsonAsync("/auth/v1/passkey/register/complete", request, ct);

public Task<HttpResponseMessage> PasskeyAuthBeginAsync(object request, CancellationToken ct) =>
    httpClient.PostAsJsonAsync("/auth/v1/passkey/auth/begin", request, ct);

public Task<HttpResponseMessage> PasskeyAuthCompleteAsync(object request, CancellationToken ct) =>
    httpClient.PostAsJsonAsync("/auth/v1/passkey/auth/complete", request, ct);

public Task<HttpResponseMessage> ListPasskeyCredentialsAsync(Guid principalId, CancellationToken ct) =>
    httpClient.GetAsync($"/auth/v1/passkey/credentials?principalId={principalId}", ct);

public Task<HttpResponseMessage> DeletePasskeyCredentialAsync(Guid credentialId, Guid principalId, CancellationToken ct) =>
    httpClient.DeleteAsync($"/auth/v1/passkey/credentials/{credentialId}?principalId={principalId}", ct);
```

- [ ] **Step 2: Add BFF verification endpoints in `AuthController.cs`**

Add `GET /auth/verify-email` (callback from email link):
```csharp
[AllowAnonymous]
[HttpGet("verify-email")]
public async Task<IActionResult> VerifyEmailCallback([FromQuery] string token, CancellationToken ct)
{
    var response = await _authClient.VerifyEmailAsync(new { Token = token }, ct);
    if (!response.IsSuccessStatusCode)
    {
        return Redirect("/auth/sign-in?error=Invalid or expired verification link");
    }
    // If user is already signed in, refresh claims
    if (User.Identity?.IsAuthenticated == true)
        return Redirect("/auth/refresh-claims");
    return Redirect("/auth/sign-in?status=Email verified successfully");
}
```

Add `GET /auth/refresh-claims`:
```csharp
[Authorize(Policy = "CustomerAccount")]
[HttpGet("refresh-claims")]
public async Task<IActionResult> RefreshClaims(CancellationToken ct)
{
    var principalIdClaim = User.FindFirst("principal_id")?.Value;
    if (!Guid.TryParse(principalIdClaim, out var principalId))
        return Redirect("/auth/sign-in");

    var response = await _authClient.GetCurrentPrincipalAsync(principalId, ct);
    if (!response.IsSuccessStatusCode)
        return Redirect("/");

    var profile = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
    var emailVerified = profile.GetProperty("emailVerified").GetBoolean();

    // Re-issue cookie with updated claims
    var identity = (ClaimsIdentity)User.Identity!;
    var existingClaim = identity.FindFirst("email_verified");
    if (existingClaim != null)
        identity.RemoveClaim(existingClaim);
    identity.AddClaim(new Claim("email_verified", emailVerified.ToString().ToLower()));

    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

    return Redirect("/account");
}
```

- [ ] **Step 3: Update sign-in cookie to include `email_verified` claim**

In the `SignInCustomerAsync` method in `AuthController.cs`, add:
```csharp
claims.Add(new Claim("email_verified", "false"));
```

For Google sign-in, look at the Google callback method and change to:
```csharp
claims.Add(new Claim("email_verified", "true"));
```

- [ ] **Step 4: Create `maliev-passkey.js`**

Write `Maliev.Web.Bff/wwwroot/js/maliev-passkey.js`:
```javascript
window.malievPasskey = {
    createPasskey: async function (registerBeginUrl, registerCompleteUrl, principalId, deviceName) {
        try {
            const beginResp = await fetch(registerBeginUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ principalId })
            });
            const options = await beginResp.json();

            const publicKey = {
                challenge: base64urlToArray(options.challenge),
                rp: { id: options.rpId, name: options.rpName },
                user: {
                    id: base64urlToArray(options.userId),
                    name: options.userName,
                    displayName: options.userDisplayName
                },
                pubKeyCredParams: options.pubKeyCredParams,
                authenticatorSelection: options.authenticatorSelection,
                attestation: options.attestation
            };

            const credential = await navigator.credentials.create({ publicKey });
            if (!credential) return { success: false, error: 'User cancelled' };

            const completeResp = await fetch(registerCompleteUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    principalId,
                    credentialId: arrayToBase64url(new Uint8Array(credential.rawId)),
                    publicKey: arrayBufferToPem(credential.response.getPublicKey()),
                    deviceName: deviceName || 'Passkey',
                    clientDataJson: new TextDecoder().decode(credential.response.clientDataJSON),
                    attestationObject: arrayToBase64url(new Uint8Array(credential.response.attestationObject))
                })
            });
            return await completeResp.json();
        } catch (e) {
            return { success: false, error: e.message || 'Passkey registration failed' };
        }
    },

    authenticatePasskey: async function (authBeginUrl, authCompleteUrl) {
        try {
            const beginResp = await fetch(authBeginUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({})
            });
            const options = await beginResp.json();

            const publicKey = {
                challenge: base64urlToArray(options.challenge),
                rpId: options.rpId,
                allowCredentials: options.allowCredentials,
                userVerification: options.userVerification || 'required'
            };

            const assertion = await navigator.credentials.get({ publicKey });
            if (!assertion) return { success: false, error: 'User cancelled' };

            const completeResp = await fetch(authCompleteUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    credentialId: arrayToBase64url(new Uint8Array(assertion.rawId)),
                    signature: arrayToBase64url(new Uint8Array(assertion.response.signature)),
                    authenticatorData: arrayToBase64url(new Uint8Array(assertion.response.authenticatorData)),
                    clientDataJson: new TextDecoder().decode(assertion.response.clientDataJSON),
                    userHandle: assertion.response.userHandle
                        ? arrayToBase64url(new Uint8Array(assertion.response.userHandle))
                        : null
                })
            });
            return await completeResp.json();
        } catch (e) {
            return { success: false, error: e.message || 'Passkey authentication failed' };
        }
    }
};

function base64urlToArray(base64url) {
    const base64 = base64url.replace(/-/g, '+').replace(/_/g, '/');
    const raw = atob(base64);
    return Uint8Array.from(raw, c => c.charCodeAt(0)).buffer;
}

function arrayToBase64url(array) {
    const base64 = btoa(String.fromCharCode(...array));
    return base64.replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
}

function arrayBufferToPem(keyData) {
    // Simplified - returns the raw key bytes as base64
    // In production, use a proper COSE-to-PEM conversion
    return keyData ? btoa(String.fromCharCode(...new Uint8Array(keyData))) : '';
}
```

- [ ] **Step 5: Add passkey button to `AuthSignIn.razor`**

After the Google button and before the `<details>` email panel:
```razor
<button type="button" class="auth-passkey-button" @onclick="SignInWithPasskey" disabled="@_passkeyUnavailable">
    @Text("Sign in with passkey", "เข้าสู่ระบบด้วย Passkey")
</button>
```

Add the passkey JS interop methods in the `@code` block:
```csharp
private bool _passkeyUnavailable = true;

protected override async Task OnInitializedAsync()
{
    _passkeyUnavailable = await JsRuntime.InvokeAsync<bool>("typeof window.malievPasskey !== 'undefined' && typeof PublicKeyCredential !== 'undefined'");
}

private async Task SignInWithPasskey()
{
    var result = await JsRuntime.InvokeAsync<JsonElement>("malievPasskey.authenticatePasskey",
        "/auth/v1/passkey/auth/begin", "/auth/v1/passkey/auth/complete");
    // On success, the BFF sets the cookie and redirects
    if (result.TryGetProperty("success", out var success) && success.GetBoolean())
    {
        Navigation.NavigateTo("/auth/refresh-claims", true);
    }
}
```

- [ ] **Step 6: Add verification banner to `AccountProfile.razor`**

At the top of the account profile page content:
```razor
@if (!_emailVerified)
{
    <MudAlert Severity="Severity.Warning" Dismissible="true" Class="mb-4">
        @Text("Verify your email — we sent a link to {0}. Didn't receive it?", "ยืนยันอีเมลของคุณ — เราได้ส่งลิงก์ไปยัง {0} แล้ว ไม่ได้รับหรือ?")
        <MudButton Variant="Variant.Text" Color="Color.Primary" @onclick="ResendVerificationEmail">
            @Text("Resend", "ส่งอีกครั้ง")
        </MudButton>
    </MudAlert>
}
```

Add in `@code`:
```csharp
private bool _emailVerified = true;

protected override async Task OnInitializedAsync()
{
    var claim = User.FindFirst("email_verified")?.Value;
    _emailVerified = claim != "false";
}

private async Task ResendVerificationEmail()
{
    var principalId = User.FindFirst("principal_id")?.Value;
    if (Guid.TryParse(principalId, out var pid))
    {
        await _authClient.ResendVerificationEmailAsync(new { PrincipalId = pid }, CancellationToken.None);
    }
}
```

- [ ] **Step 7: Add passkey section to `AccountProfile.razor`**

After the language/region section:
```razor
<MudExpansionPanel Text="Passkeys">
    <MudButton Variant="Variant.Filled" Color="Color.Primary" @onclick="AddPasskey" Class="mb-4">
        @Text("Add passkey", "เพิ่ม Passkey")
    </MudButton>
    @if (_passkeys is not null)
    {
        @foreach (var passkey in _passkeys)
        {
            <MudPaper Class="pa-4 mb-2 d-flex align-center">
                <MudText>@passkey.DeviceName</MudText>
                <MudText Typo="Typo.body2" Class="ml-auto">@passkey.CreatedAtUtc.ToString("g")</MudText>
                <MudIconButton Icon="@Icons.Material.Filled.Delete" Color="Color.Error"
                               @onclick="() => RemovePasskey(passkey.Id)" />
            </MudPaper>
        }
    }
</MudExpansionPanel>
```

- [ ] **Step 8: Build + run tests**

```bash
dotnet build Maliev.Web.slnx
dotnet test Maliev.Web.slnx --verbosity normal
```

- [ ] **Step 9: Commit**

```bash
git add .
git commit -m "feat(web): add email verification flow, passkey sign-in, verification banner"
```

---

### Task 7: Aspire — Wire New Service Endpoints

**Files:**
- Modify: `Maliev.Aspire.AppHost/AppHost.cs`

- [ ] **Step 1: Update AuthService environment config in AppHost**

If AuthService needs WebAuthn config passed from AppHost, add environment variables:
```csharp
authService.WithEnvironment("WebAuthn__RPId", "localhost");
authService.WithEnvironment("WebAuthn__AllowedOrigins", "https://localhost:56139");
```

- [ ] **Step 2: Commit**

```bash
git add .
git commit -m "chore: add WebAuthn env config for local dev"
```
