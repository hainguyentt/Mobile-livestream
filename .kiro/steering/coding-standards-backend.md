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
├── LivestreamApp.Tests.Unit/    # xUnit unit tests
└── LivestreamApp.Tests.Integration/ # xUnit integration tests (Testcontainers)
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

### Controllers
- Route prefix: `[Route("api/v1/{resource}")]` — lowercase, plural noun
- Class-level `[Authorize]` cho controllers yêu cầu auth; override bằng `[AllowAnonymous]` cho endpoints public
- `[ProducesResponseType]` trên mỗi action — ít nhất happy path và error path
- Không chứa business logic — chỉ: extract input → gọi service → map response
- Private helpers (`GetCurrentUserId`, `GetClientIp`) ở cuối, sau `// --- Private helpers ---`
- DTOs đặt trong `LivestreamApp.API/DTOs/{Module}/`

```csharp
[ApiController]
[Route("api/v1/profiles")]
[Authorize]
public class ProfilesController : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var profile = await _profileService.GetProfileAsync(userId, ct);
        return Ok(profile);
    }

    // --- Private helpers ---
    private Guid GetCurrentUserId() { ... }
}
```

### Middleware
- Đặt trong `LivestreamApp.API/Middleware/`
- Tên class: `{Purpose}Middleware` (ví dụ: `ExceptionHandlingMiddleware`, `RateLimitingMiddleware`)
- Thứ tự đăng ký trong `Program.cs` (từ ngoài vào trong):
  1. `ExceptionHandlingMiddleware` — phải đứng đầu để catch tất cả exceptions
  2. `RateLimitingMiddleware`
  3. `UseAuthentication` / `UseAuthorization`
  4. `MapControllers`

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

- Comment *why*, không comment *what*
- XML doc `/// <summary>` bắt buộc trên tất cả `public` classes, interfaces, methods
- `<param>` — chỉ khi tên parameter không self-explanatory
- `<exception>` — bắt buộc trên interface methods có throw
- `<returns>` — khi return value cần giải thích thêm ngoài type name
- `<remarks>` — cho business rules quan trọng hoặc side effects không hiển nhiên
- `<inheritdoc/>` — trên implementation class thay vì duplicate doc từ interface
- Không để commented-out code — xóa, Git history lưu lại
- TODO format: `// TODO: {description} — Refs: {ticket}`

```csharp
/// <summary>Handles user authentication including email, LINE OAuth, and OTP flows.</summary>
public interface IAuthService
{
    /// <summary>Registers a new user with email and password.</summary>
    /// <param name="password">Plain text password — will be BCrypt hashed internally.</param>
    /// <returns>The newly created user in PendingVerification status.</returns>
    /// <exception cref="DomainException">Thrown when email is already registered.</exception>
    Task<User> RegisterWithEmailAsync(string email, string password, CancellationToken ct = default);
}

/// <inheritdoc/>
public class AuthService : IAuthService { ... }
```
