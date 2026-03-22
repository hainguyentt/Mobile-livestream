---
inclusion: fileMatch
fileMatchPattern: "app/backend/**,app/tests/**"
---

# Backend Coding Standards — .NET 8 / C# 12

## Language & Framework
- Target: .NET 8, C# 12, ASP.NET Core
- Nullable reference types: **enabled** (`<Nullable>enable</Nullable>`)
- Implicit usings: **enabled**
- Language version: **12** (use collection expressions `[]`, primary constructors where appropriate)

## Project Structure

```
app/backend/
├── LivestreamApp.Shared/        # Domain primitives, interfaces, events, exceptions, utilities
├── LivestreamApp.Auth/          # Auth module (entities, services, repositories, options)
├── LivestreamApp.Profiles/      # Profiles module
├── LivestreamApp.API/           # Entry point — controllers, middleware, DI, EF config
app/mock/
└── LivestreamApp.MockServices/  # Stripe + LINE Pay mock servers
app/tests/
└── LivestreamApp.Tests.Unit/    # xUnit unit tests
```

## Naming Conventions

| Element | Convention | Example |
|---|---|---|
| Class, Interface, Enum | PascalCase | `AuthService`, `IUserRepository` |
| Method | PascalCase | `RegisterWithEmailAsync` |
| Property | PascalCase | `PasswordHash`, `IsEmailVerified` |
| Private field | `_camelCase` | `_userRepository`, `_jwtOptions` |
| Local variable | camelCase | `existingUser`, `tokenHash` |
| Constant | PascalCase | `SectionName`, `MinLength` |
| Async method | Suffix `Async` | `GetByEmailAsync`, `SaveChangesAsync` |
| Interface | Prefix `I` | `IAuthService`, `ICacheService` |
| Options class | Suffix `Options` | `JwtOptions`, `LineOptions` |

## Domain-Driven Design Patterns

### Entities
- Inherit from `Entity<TId>` (Shared project)
- Constructor is **private** — use static factory method `Create(...)`
- Provide a **private parameterless constructor** for EF Core
- All state changes via **domain methods** (never set properties directly from outside)
- Raise domain events via `RaiseDomainEvent(...)` inside entity methods
- Always update `UpdatedAt = DateTime.UtcNow` on state change

```csharp
// CORRECT
public sealed class User : Entity<Guid>
{
    private User(Guid id, Email email) : base(id) { ... }
    private User() : base(Guid.Empty) { Email = null!; } // EF Core

    public static User Create(string email, string passwordHash) { ... }
    public void VerifyEmail() { ... RaiseDomainEvent(...); }
}

// WRONG — never expose public setters
public string Email { get; set; }
```

### Value Objects
- Inherit from `ValueObject` (Shared project)
- Constructor is **private** — use static factory `Create(...)`
- Implement `GetEqualityComponents()`
- Validate in `Create()`, throw `DomainException` on invalid input
- Override `ToString()` to return the value

### Domain Events
- Use `sealed record` implementing `IDomainEvent`
- Always include `EventId = Guid.NewGuid()` and `OccurredAt = DateTime.UtcNow`

```csharp
public sealed record UserRegisteredEvent(Guid UserId, string Email) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
```

### Services
- Define interface first (`IAuthService`), then implementation (`AuthService`)
- All dependencies injected via constructor
- All public methods are `async Task<T>` with `CancellationToken ct = default`
- Private helpers at the bottom, separated by `// --- Private helpers ---`

### Repositories
- Extend `IRepository<TEntity, TId>` for standard CRUD
- Add domain-specific query methods to the interface
- Never expose `IQueryable` outside the repository

### Options / Configuration
- One class per config section
- `public const string SectionName = "..."` for DI registration
- Register with `services.Configure<TOptions>(config.GetSection(TOptions.SectionName))`

## Exception Handling
- `DomainException` — business rule violations
- `NotFoundException` — entity not found
- `ValidationException` — input validation failures
- `UnauthorizedException` — auth failures
- Never throw raw `Exception` or `ApplicationException`
- Never catch and swallow exceptions silently

## Async / Await
- All I/O operations must be async
- Always pass `CancellationToken` through the call chain
- Never use `.Result` or `.Wait()` — always `await`
- Use `ConfigureAwait(false)` only in library code (not needed in ASP.NET Core)

## Security Rules
- Passwords: BCrypt with work factor ≥ 12 (`PasswordHasher.Hash`)
- Tokens stored in DB: SHA-256 hashed (`HashToken` helper)
- OTP: 6-digit, max 10 min expiry, max 3 attempts before invalidation
- JWT: HS256, access token ≤ 15 min, refresh token ≤ 30 days
- Never log passwords, tokens, or OTP codes
- Always use parameterized queries (EF Core handles this)

## Code Style
- Prefer expression-bodied members for single-line methods
- Use `var` when type is obvious from right-hand side
- Use collection expressions `[]` instead of `new List<T>()`
- Use `is not null` / `is null` pattern matching over `!= null`
- Use `??` and `?.` operators to reduce null checks
- Use `record` for DTOs and immutable data transfer objects
- Use `sealed` on leaf classes (entities, value objects) unless inheritance is needed

## Comment Standards

### When to comment
- **Do** comment *why*, not *what* — code should be self-explanatory for the *what*
- **Do** use `/// <summary>` XML doc comments on all `public` classes and interfaces
- **Do** comment non-obvious business rules or domain logic decisions
- **Do** use `// TODO: {description} — Refs: {ticket}` for known gaps (never leave bare `// TODO`)
- **Don't** comment obvious code — `// increment counter` above `count++` is noise
- **Don't** leave commented-out code blocks — delete them, Git history preserves them

### XML Doc Comments — required on public API surface

**`<summary>`** — bắt buộc trên tất cả public classes, interfaces, methods:
```csharp
/// <summary>Base class for all domain entities with strongly-typed ID.</summary>
public abstract class Entity<TId> { ... }

/// <summary>Generates a signed JWT access token.</summary>
public static string GenerateAccessToken(...) { ... }
```

**`<param>`** — chỉ khi tên parameter không self-explanatory (generic type, flags, hidden constraints):
```csharp
/// <param name="expiryMinutes">Token lifetime in minutes. Default: 15.</param>
/// <param name="purpose">Determines OTP email template and expiry duration.</param>
// SKIP: email, password, userId, ct — tên đã đủ rõ
```

**`<exception>`** — bắt buộc trên interface methods có throw:
```csharp
/// <exception cref="DomainException">Thrown when email is already registered.</exception>
/// <exception cref="UnauthorizedException">Thrown when credentials are invalid or account is locked.</exception>
/// <exception cref="NotFoundException">Thrown when user does not exist.</exception>
```

**`<returns>`** — khi return value cần giải thích thêm ngoài type name:
```csharp
/// <returns>True if OTP is valid, not expired, and attempt count is under limit; false otherwise.</returns>
/// <returns>Hashed token string (SHA-256, Base64 encoded).</returns>
// SKIP: Task<User>, Task<AuthTokens> — đã rõ từ type
```

**`<remarks>`** — cho business rules quan trọng hoặc side effects không hiển nhiên:
```csharp
/// <remarks>
/// Refresh token is rotated on each use (one-time use).
/// The old token is revoked and linked to the new token hash for audit trail.
/// </remarks>
```

**`<inheritdoc/>`** — trên implementation class thay vì duplicate doc từ interface:
```csharp
// Interface có full doc — implementation dùng inheritdoc
/// <inheritdoc/>
public async Task<User> RegisterWithEmailAsync(string email, string password, CancellationToken ct = default)
{ ... }
```

### Full example — interface với complete doc
```csharp
/// <summary>Handles user authentication including email, LINE OAuth, and OTP flows.</summary>
public interface IAuthService
{
    /// <summary>Registers a new user with email and password.</summary>
    /// <param name="password">Plain text password — will be BCrypt hashed internally.</param>
    /// <returns>The newly created user in PendingVerification status.</returns>
    /// <exception cref="DomainException">Thrown when email is already registered.</exception>
    Task<User> RegisterWithEmailAsync(string email, string password, CancellationToken ct = default);

    /// <summary>Authenticates a user and issues JWT + refresh token pair.</summary>
    /// <param name="ipAddress">Caller IP address, recorded for audit and lockout tracking.</param>
    /// <returns>Access token (JWT) and refresh token (opaque).</returns>
    /// <exception cref="UnauthorizedException">Thrown when credentials are invalid, email unverified, or account is locked.</exception>
    Task<AuthTokens> LoginWithEmailAsync(string email, string password, string ipAddress, CancellationToken ct = default);

    /// <summary>Verifies a 6-digit OTP code for the given target and purpose.</summary>
    /// <remarks>
    /// OTP is invalidated after 3 failed attempts regardless of expiry.
    /// For EmailVerification purpose, also activates the user account on success.
    /// </remarks>
    /// <returns>True if code matches and is still valid; false otherwise.</returns>
    Task<bool> VerifyEmailOtpAsync(string email, string code, OtpPurpose purpose, CancellationToken ct = default);
}
```

### Inline comments — explain *why*
```csharp
// CORRECT — explains a non-obvious decision
// SHA-256 hash before storage to prevent token enumeration attacks
var tokenHash = HashToken(refreshToken);

// Invalidate after 3 failed attempts to prevent brute-force on OTP
if (AttemptCount >= 3)
    IsUsed = true;

// WRONG — restates what the code already says
// Get user by email
var user = await _userRepository.GetByEmailAsync(email, ct);
```

### Section separators — only in long service classes
```csharp
// --- Private helpers ---
private async Task<AuthTokens> IssueTokensAsync(...) { ... }
```

### TODO format
```csharp
// TODO: add Polly retry policy for transient HTTP failures — Refs: US-03-01
// TODO: replace in-memory rate limiter with Redis for multi-instance support — Refs: NFR-02
```
