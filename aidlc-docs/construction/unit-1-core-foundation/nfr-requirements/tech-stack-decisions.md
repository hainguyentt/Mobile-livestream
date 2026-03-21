# Tech Stack Decisions — Unit 1: Core Foundation

**Unit**: Unit 1 — Core Foundation  
**Ngày tạo**: 2026-03-21

---

## 1. Backend Framework & Runtime

| Decision | Choice | Rationale |
|---|---|---|
| Runtime | .NET 8 (LTS) | Đã quyết định ở Inception. LTS support đến Nov 2026. |
| Web Framework | ASP.NET Core 8 Web API | Native, high performance, first-class DI |
| Language | C# 12 | Latest stable với .NET 8 |
| API Style | REST + `Asp.Versioning.Mvc` | URL path versioning `/api/v1/` |

### API Versioning Setup
```csharp
// Program.cs
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
```

**Controller structure**:
```
Controllers/
├── V1/
│   ├── AuthController.cs      [ApiVersion("1.0")]
│   └── ProfilesController.cs  [ApiVersion("1.0")]
```

---

## 2. ORM & Database Migrations

| Decision | Choice | Rationale |
|---|---|---|
| ORM | Entity Framework Core 8 | Native .NET, Code-First, strong typing |
| DB Provider | Npgsql.EntityFrameworkCore.PostgreSQL | Official PostgreSQL provider |
| Migration Strategy | EF Core Code-First + Auto-apply startup | Đơn giản, phù hợp team nhỏ |
| Connection Pool | Npgsql default (min=1, max=100) | Monitor và tune khi cần |

### Auto-apply Migration Pattern
```csharp
// Program.cs — chạy trước khi app nhận traffic
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync(); // idempotent — safe to run multiple times
}
// Health /startup probe chỉ pass sau khi migration xong
```

**Startup probe** đảm bảo ECS không route traffic vào instance đang migrate.

---

## 3. Authentication & Security

| Decision | Choice | Rationale |
|---|---|---|
| JWT Library | `Microsoft.AspNetCore.Authentication.JwtBearer` | Built-in, no extra dependency |
| JWT Algorithm | HS256 (MVP) → RS256 (khi multi-service) | HS256 đủ cho monolith |
| Password Hashing | BCrypt.Net-Next (cost=12) | Industry standard, slow by design |
| Token Storage | httpOnly Cookie | XSS-safe, không accessible bởi JS |
| External Token Encryption | AES-256-GCM via `System.Security.Cryptography` | Built-in .NET, no extra package |

### Cookie Configuration
```csharp
options.Cookie.HttpOnly = true;
options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
options.Cookie.SameSite = SameSiteMode.Strict;
options.Cookie.Name = "__Host-refresh_token"; // __Host- prefix = extra security
```

---

## 4. Caching — Redis

| Decision | Choice | Rationale |
|---|---|---|
| Redis Client | StackExchange.Redis | De-facto standard cho .NET |
| Cache Abstraction | `IDistributedCache` + custom `ICacheService` | Testable, swappable |
| Revoked Token Cache | Redis SET với TTL = token expiry | O(1) lookup, auto-expire |
| User Profile Cache | Redis STRING (JSON) với TTL = 15 phút | Configurable, invalidate on update |

### Cache Key Convention
```
revoked_token:{sha256_hash}          TTL: 30 days
user:profile:{userId}                TTL: 15 min (configurable)
otp:rate_limit:{email}               TTL: 1 hour
login:rate_limit:{ip}                TTL: 1 min
```

### TTL Configuration (appsettings.json)
```json
{
  "Cache": {
    "UserProfileTtlMinutes": 15,
    "OtpRateLimitWindowMinutes": 60
  }
}
```

---

## 5. File Storage — S3

| Decision | Choice | Rationale |
|---|---|---|
| Upload Strategy | Presigned URL + Server verify | Client upload trực tiếp → giảm tải API server |
| S3 Client | AWSSDK.S3 | Official AWS SDK |
| Image Processing | SixLabors.ImageSharp | Cross-platform, no GDI+ dependency |
| Presigned URL Expiry | 5 phút | Security — đủ thời gian upload |

### Upload Flow
```
1. Client → POST /api/v1/profiles/photos/presign
           { displayIndex: 0, contentType: "image/jpeg", fileSizeBytes: 1048576 }

2. Server → Validate (type, size, index range)
          → Generate presigned PUT URL (5 min expiry)
          → Return { uploadUrl, photoId }

3. Client → PUT {uploadUrl} (direct to S3/LocalStack)

4. Client → POST /api/v1/profiles/photos/confirm
           { photoId }

5. Server → Verify S3 object exists (HeadObject)
          → Resize image (ImageSharp) → re-upload optimized version
          → Save UserPhoto record to DB
          → Invalidate user profile cache
```

---

## 6. Rate Limiting

| Decision | Choice | Rationale |
|---|---|---|
| Library | ASP.NET Core built-in `RateLimiter` (.NET 8) | No extra package, production-ready |
| Algorithm | Fixed Window | Đơn giản, predictable |
| Storage | In-memory (per instance) | MVP — đủ dùng với 1-2 ECS tasks |
| Future upgrade | Redis-backed rate limiting | Khi scale > 2 instances |

> **Note**: In-memory rate limiting không shared giữa ECS instances. Với 2 instances, effective limit = 2x configured limit. Acceptable ở MVP (10K users). Upgrade sang Redis-backed khi scale.

### Rate Limit Policy Mapping
```csharp
// Auth endpoints
[EnableRateLimiting("auth")]        // 20/min per-IP + 1000/min global

// OTP endpoints  
[EnableRateLimiting("otp")]         // 5/min per-IP + 200/min global

// Password reset
[EnableRateLimiting("password-reset")] // 3/hour per-IP + 100/hour global

// Profile endpoints
[EnableRateLimiting("profile")]     // 60/min per-IP + 5000/min global

// Upload endpoints
[EnableRateLimiting("upload")]      // 10/min per-IP + 500/min global
```

---

## 7. Logging — Serilog

| Decision | Choice | Rationale |
|---|---|---|
| Framework | Serilog | Structured logging, rich ecosystem |
| Production Sink | AWS CloudWatch Logs | Native AWS, APPI-compliant (data stays in JP region) |
| Dev/Test Sinks | Console (colored) + File (rolling daily) | Developer experience |
| Log Format (prod) | JSON structured | Machine-parseable, CloudWatch Insights |
| Log Format (dev) | Plain text với color | Human-readable |

### NuGet Packages
```xml
<PackageReference Include="Serilog.AspNetCore" Version="8.*" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.*" />
<PackageReference Include="Serilog.Sinks.File" Version="5.*" />
<PackageReference Include="AWS.Logger.SeriLog" Version="3.*" />
```

### Environment-based Configuration
```csharp
// Production: CloudWatch only
// Development/Test: Console + File (logs/app-{date}.log, retain 7 days)
// Sensitive data: Destructuring policies strip passwords, tokens, OTP codes
```

---

## 8. Background Jobs — Hangfire

| Decision | Choice | Rationale |
|---|---|---|
| Library | Hangfire | Mature, PostgreSQL storage, dashboard UI |
| Storage | PostgreSQL (same DB) | Đơn giản, không cần thêm infra ở MVP |
| Dashboard | `/hangfire` (admin-only, IP whitelist) | Monitoring + manual trigger |

**Unit 1 Jobs**:
| Job | Cron | Description |
|---|---|---|
| `PurgeExpiredLoginAttempts` | `0 3 * * *` (03:00 JST) | DELETE WHERE AttemptedAt < now - 90 days |
| `ProcessPendingAccountDeletions` | `0 2 * * *` (02:00 JST) | Anonymize WHERE DeletedAt < now - 30 days |

---

## 9. Health Checks

| Decision | Choice | Rationale |
|---|---|---|
| Library | `Microsoft.Extensions.Diagnostics.HealthChecks` | Built-in .NET 8 |
| DB Check | `AspNetCore.HealthChecks.NpgSql` | PostgreSQL connectivity |
| Redis Check | `AspNetCore.HealthChecks.Redis` | Redis connectivity |

```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql", tags: ["ready"])
    .AddRedis(redisConnection, name: "redis", tags: ["ready"])
    .AddCheck("migrations", () =>
        migrationsApplied
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Migrations pending"),
        tags: ["startup"]);

// Endpoints
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready",
    new() { Predicate = check => check.Tags.Contains("ready") });
app.MapHealthChecks("/health/startup",
    new() { Predicate = check => check.Tags.Contains("startup") });
```

---

## 10. Frontend — PWA

| Decision | Choice | Rationale |
|---|---|---|
| Framework | Next.js 14+ (App Router) | SSR/SSG, PWA support, i18n built-in |
| Styling | Tailwind CSS | Utility-first, responsive, dark mode |
| State Management | Zustand | Lightweight, no boilerplate |
| HTTP Client | Axios + interceptor | Auto token refresh, error handling |
| i18n | `next-intl` | App Router compatible, JP/EN |
| PWA | `next-pwa` | Service worker, installable |
| Font | Noto Sans JP (Google Fonts) | Full JP character support |

### Token Handling (httpOnly Cookie)
```typescript
// Axios interceptor — auto refresh on 401
// Tokens NOT stored in Zustand/localStorage
// Only user object stored in Zustand (no sensitive data)
```

---

## 11. MockServices

| Decision | Choice | Rationale |
|---|---|---|
| Framework | ASP.NET Core 8 Minimal API | Lightweight, cùng solution |
| Port | 5001 | Không conflict với main API (5000) |
| Scope (Unit 1) | Stripe skeleton + LINE Pay skeleton | Success path only, Unit 3 mở rộng |
| Config | `MockMode` flag trong `appsettings.json` | Dễ switch sang real API |

### Docker Compose Integration
```yaml
services:
  mockservices:
    build: ./src/LivestreamApp.MockServices
    ports:
      - "5001:5001"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - MockMode=true
```

---

## 12. NuGet Package Summary (Unit 1)

### Backend
```xml
<!-- Core -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.*" />
<PackageReference Include="Asp.Versioning.Mvc" Version="8.*" />
<PackageReference Include="Asp.Versioning.Mvc.ApiExplorer" Version="8.*" />

<!-- Database -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.*" />

<!-- Cache -->
<PackageReference Include="StackExchange.Redis" Version="2.*" />
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="8.*" />

<!-- Security -->
<PackageReference Include="BCrypt.Net-Next" Version="4.*" />

<!-- Storage -->
<PackageReference Include="AWSSDK.S3" Version="3.*" />
<PackageReference Include="SixLabors.ImageSharp" Version="3.*" />

<!-- Email -->
<PackageReference Include="AWSSDK.SimpleEmail" Version="3.*" />

<!-- Background Jobs -->
<PackageReference Include="Hangfire.AspNetCore" Version="1.*" />
<PackageReference Include="Hangfire.PostgreSql" Version="1.*" />

<!-- Validation -->
<PackageReference Include="FluentValidation.AspNetCore" Version="11.*" />

<!-- Logging -->
<PackageReference Include="Serilog.AspNetCore" Version="8.*" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.*" />
<PackageReference Include="Serilog.Sinks.File" Version="5.*" />
<PackageReference Include="AWS.Logger.SeriLog" Version="3.*" />

<!-- Health Checks -->
<PackageReference Include="AspNetCore.HealthChecks.NpgSql" Version="8.*" />
<PackageReference Include="AspNetCore.HealthChecks.Redis" Version="8.*" />

<!-- LINE OAuth -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.OAuth" Version="8.*" />
```

### Frontend (PWA)
```json
{
  "dependencies": {
    "next": "^14",
    "react": "^18",
    "zustand": "^4",
    "axios": "^1",
    "next-intl": "^3",
    "next-pwa": "^5",
    "@microsoft/signalr": "^8",
    "tailwindcss": "^3"
  }
}
```
