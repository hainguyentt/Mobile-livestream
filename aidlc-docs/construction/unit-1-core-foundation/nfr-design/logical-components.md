# Logical Components — Unit 1: Core Foundation

**Unit**: Unit 1 — Core Foundation  
**Ngày tạo**: 2026-03-21

---

## 1. Tổng Quan Logical Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        PWA (Next.js)                            │
│  AuthPages | OnboardingPages | ProfilePages | AdminLoginPage    │
│  Zustand AuthStore | Axios + TokenRefreshInterceptor            │
└──────────────────────────┬──────────────────────────────────────┘
                           │ HTTPS (TLS 1.3)
                           │ httpOnly Cookie (JWT + RefreshToken)
┌──────────────────────────▼──────────────────────────────────────┐
│                   AWS ALB / CloudFront                          │
│              (X-Forwarded-For, SSL termination)                 │
└──────────────────────────┬──────────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────────┐
│              LivestreamApp.API (ASP.NET Core 8)                 │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                   Middleware Pipeline                    │   │
│  │  RateLimiter → Auth → CORS → ErrorHandler → Logging     │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  ┌──────────────────┐    ┌──────────────────────────────────┐  │
│  │  Controllers/V1  │    │         Health Checks            │  │
│  │  ├── Auth        │    │  /health/live (liveness)         │  │
│  │  └── Profiles    │    │  /health/ready (DB + Redis)      │  │
│  └────────┬─────────┘    │  /health/startup (migrations)    │  │
│           │              └──────────────────────────────────┘  │
│  ┌────────▼─────────────────────────────────────────────────┐  │
│  │                    Application Layer                      │  │
│  │  IAuthService | IProfileService | ICacheService          │  │
│  │  IEmailService | IStorageService | IAdminAuditService    │  │
│  └────────┬─────────────────────────────────────────────────┘  │
│           │                                                     │
│  ┌────────▼─────────────────────────────────────────────────┐  │
│  │                    Domain Layer                           │  │
│  │  User | UserProfile | HostProfile | UserPhoto            │  │
│  │  RefreshToken | OtpCode | ExternalLogin                  │  │
│  │  Domain Events | Business Rules                          │  │
│  └────────┬─────────────────────────────────────────────────┘  │
│           │                                                     │
│  ┌────────▼─────────────────────────────────────────────────┐  │
│  │                 Infrastructure Layer                      │  │
│  │  AppDbContext (EF Core) | RedisCache | S3Storage         │  │
│  │  SesEmailService | HangfireJobScheduler                  │  │
│  └──────────────────────────────────────────────────────────┘  │
└──────────────────────────┬──────────────────────────────────────┘
                           │
        ┌──────────────────┼──────────────────┐
        │                  │                  │
┌───────▼──────┐  ┌────────▼───────┐  ┌──────▼──────────┐
│  PostgreSQL  │  │     Redis      │  │   AWS S3        │
│  (RDS)       │  │ (ElastiCache)  │  │ (LocalStack)    │
│              │  │                │  │                 │
│  users       │  │ revoked_token  │  │ profiles/       │
│  user_profiles│  │ user:profile   │  │ {userId}/       │
│  host_profiles│  │ otp:rate_limit │  │ {photoId}.jpg   │
│  user_photos │  │ login:rate_lim │  └─────────────────┘
│  refresh_tok │  └────────────────┘
│  otp_codes   │
│  ext_logins  │  ┌────────────────┐
│  login_att   │  │   AWS SES      │
│  admin_logs  │  │ (LocalStack)   │
│  hangfire_*  │  │                │
└──────────────┘  │ OTP emails     │
                  │ Reset emails   │
                  └────────────────┘
```

---

## 2. Application Services (Logical Components)

### 2.1 IAuthService

**Trách nhiệm**: Toàn bộ authentication flows

| Method | NFR Patterns Applied |
|---|---|
| `RegisterAsync(email, password)` | Rate limiting, BCrypt hash, OTP generation |
| `VerifyEmailOtpAsync(email, otp)` | OTP idempotency (invalidate on use), JWT issue |
| `LoginAsync(email, password, captchaToken?)` | Brute-force protection, rate limiting |
| `LineLoginAsync(code, state)` | OAuth state validation, account merge |
| `RefreshTokenAsync(refreshToken)` | Token rotation, reuse detection, Redis blacklist |
| `RevokeTokenAsync(refreshToken)` | Redis blacklist write |
| `ForgotPasswordAsync(email)` | Rate limiting (3/hour), always-200 response |
| `ResetPasswordAsync(token, newPassword)` | Token validation, revoke all sessions |
| `ResendOtpAsync(email, purpose)` | Invalidate old OTP, create new, SES retry |
| `RequestPhoneOtpAsync(phone)` | Rate limiting (5/min), SMS via SNS |
| `VerifyPhoneOtpAsync(phone, otp)` | OTP validation, IsPhoneVerified = true |
| `DeleteAccountAsync(userId)` | Soft delete, revoke all tokens |

### 2.2 IProfileService

**Trách nhiệm**: Profile CRUD + photo management

| Method | NFR Patterns Applied |
|---|---|
| `GetProfileAsync(userId)` | Cache-Aside (Redis, TTL 15 min) |
| `UpdateProfileAsync(userId, dto)` | Cache invalidation (B: update + admin events) |
| `GetPresignedUploadUrlAsync(userId, dto)` | S3 presigned URL (5 min expiry) |
| `ConfirmPhotoUploadAsync(userId, photoId)` | S3 HeadObject verify, ImageSharp resize |
| `DeletePhotoAsync(userId, displayIndex)` | S3 delete, cache invalidation |
| `ReorderPhotosAsync(userId, newOrder)` | Cache invalidation |
| `RequestVerificationAsync(userId)` | HostProfile update, domain event |
| `ApproveVerificationAsync(adminId, userId)` | Cache invalidation, AdminAuditLog |
| `RejectVerificationAsync(adminId, userId, note)` | Cache invalidation, AdminAuditLog |

### 2.3 ICacheService

**Trách nhiệm**: Redis abstraction — testable, swappable

```csharp
interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null);
    Task DeleteAsync(string key);
    Task<bool> ExistsAsync(string key);
    Task SetStringAsync(string key, string value, TimeSpan? ttl = null);
}
```

**Cache key builder** (centralized, no magic strings):
```csharp
static class CacheKeys
{
    public static string UserProfile(Guid userId) => $"user:profile:{userId}";
    public static string RevokedToken(string hash) => $"revoked_token:{hash}";
    public static string OtpRateLimit(string email) => $"otp:rate_limit:{email}";
    public static string LoginRateLimit(string ip) => $"login:rate_limit:{ip}";
}
```

### 2.4 IEmailService

**Trách nhiệm**: Email delivery với retry policy

```csharp
interface IEmailService
{
    Task SendOtpEmailAsync(string to, string otp, OtpPurpose purpose);
    Task SendPasswordResetEmailAsync(string to, string resetLink);
    Task SendAccountLockedEmailAsync(string to, DateTime lockoutUntil);
}
```

**Polly retry policy** (injected via DI):
- 3 retries, exponential backoff (1s → 2s → 4s)
- Retry only on transient errors (ThrottlingException, ServiceUnavailable, timeout)
- No retry on permanent errors (MessageRejected, invalid address)

### 2.5 IStorageService

**Trách nhiệm**: S3 operations abstraction

```csharp
interface IStorageService
{
    Task<string> GeneratePresignedUploadUrlAsync(string key, string contentType,
        long maxSizeBytes, TimeSpan expiry);
    Task<bool> ObjectExistsAsync(string key);
    Task DeleteObjectAsync(string key);
    Task<Stream> GetObjectAsync(string key);
    Task PutObjectAsync(string key, Stream content, string contentType);
}
```

### 2.6 IAdminAuditService

**Trách nhiệm**: Append-only admin action logging

```csharp
interface IAdminAuditService
{
    Task LogAsync(Guid adminId, string action, Guid targetUserId,
        object? details = null, string? ipAddress = null);
}
```

---

## 3. Infrastructure Components

### 3.1 PostgreSQL (RDS / LocalStack)

| Schema Object | Mục đích | NFR Pattern |
|---|---|---|
| `users` | Core user entity | Soft delete (Status=Deleted) |
| `user_profiles` | Profile data | Cache-Aside source |
| `host_profiles` | Host extension | Verification workflow |
| `user_photos` | Photo metadata | S3 key references |
| `refresh_tokens` | Token persistence | Rotation chain (ReplacedByTokenId) |
| `otp_codes` | OTP storage | Idempotency (IsUsed flag) |
| `external_logins` | OAuth linkage | LINE account merge |
| `login_attempts` | Brute-force audit | 90-day retention |
| `admin_action_logs` | Admin audit | Append-only, 1-year retention |
| `hangfire_*` | Job persistence | Background job state |

**Key indexes**:
```sql
-- Performance indexes
CREATE UNIQUE INDEX ix_users_email ON users (lower(email));
CREATE UNIQUE INDEX ix_user_profiles_display_name ON user_profiles (lower(display_name));
CREATE UNIQUE INDEX ix_external_logins_provider ON external_logins (provider, provider_user_id);
CREATE UNIQUE INDEX ix_user_photos_index ON user_photos (user_id, display_index);
CREATE INDEX ix_refresh_tokens_user ON refresh_tokens (user_id) WHERE is_revoked = false;
CREATE INDEX ix_otp_codes_target ON otp_codes (target, purpose) WHERE is_used = false;
CREATE INDEX ix_login_attempts_cleanup ON login_attempts (attempted_at);  -- for purge job
```

### 3.2 Redis (ElastiCache / LocalStack)

| Key Pattern | Type | TTL | Mục đích |
|---|---|---|---|
| `revoked_token:{hash}` | STRING | Token remaining expiry | Blacklist check |
| `user:profile:{userId}` | STRING (JSON) | 15 min (configurable) | Profile cache |
| `otp:rate_limit:{email}` | STRING | 1 hour | OTP request rate limit |
| `login:rate_limit:{ip}` | STRING | 1 min | Login rate limit |

**Redis failure handling**: App tiếp tục hoạt động khi Redis down (degraded mode):
- Profile cache miss → fallback to DB
- Blacklist check miss → fallback to DB query
- Rate limit miss → allow request (fail-open, acceptable trade-off)
- Health check → `/health/ready` trả `Degraded` (không `Unhealthy`)

### 3.3 AWS SES (LocalStack)

| Template | Trigger | Retry |
|---|---|---|
| `otp-email` | Register, resend OTP | 3x exponential backoff |
| `password-reset` | Forgot password | 3x exponential backoff |
| `account-locked` | Brute-force lockout | 3x exponential backoff |

**LocalStack config** (`docker-compose.yml`):
```yaml
localstack:
  image: localstack/localstack
  environment:
    - SERVICES=s3,ses,sns,sqs
  volumes:
    - ./localstack-init:/etc/localstack/init/ready.d
```

**Init script** (`localstack-init/init-ses.sh`):
```bash
awslocal ses verify-email-identity --email-address noreply@livestream-app.jp
```

### 3.4 AWS S3 (LocalStack)

| Bucket | Path Pattern | Access |
|---|---|---|
| `livestream-app-profiles` | `profiles/{userId}/{photoId}.{ext}` | Private (presigned URL only) |

**Bucket policy**: Block all public access. Chỉ API service role có quyền GetObject/PutObject/DeleteObject.

### 3.5 Hangfire (Background Jobs)

| Job | Schedule (JST) | Logic |
|---|---|---|
| `PurgeExpiredLoginAttempts` | Daily 03:00 | `DELETE FROM login_attempts WHERE attempted_at < NOW() - INTERVAL '90 days'` |
| `ProcessPendingAccountDeletions` | Daily 02:00 | Anonymize users WHERE `status = 'Deleted' AND deleted_at < NOW() - INTERVAL '30 days'` |

**Hangfire Dashboard**: `/hangfire` — accessible chỉ từ admin IP range (middleware IP filter).

---

## 4. MockServices Logical Components

### 4.1 Stripe Mock (Port 5001/stripe)

```
LivestreamApp.MockServices
├── Stripe/
│   ├── StripePaymentIntentController
│   │   └── POST /stripe/v1/payment_intents → success response
│   └── StripeWebhookController
│       └── POST /stripe/webhooks → payment_intent.succeeded event
└── LinePay/
    ├── LinePayRequestController
    │   └── POST /linepay/v3/payments/request → success response
    └── LinePayConfirmController
        └── POST /linepay/v3/payments/{transactionId}/confirm → success response
```

**Config switch** (`appsettings.Development.json`):
```json
{
  "ExternalServices": {
    "Stripe": {
      "BaseUrl": "http://localhost:5001/stripe",
      "MockMode": true
    },
    "LinePay": {
      "BaseUrl": "http://localhost:5001/linepay",
      "MockMode": true
    }
  }
}
```

Production (`appsettings.Production.json`):
```json
{
  "ExternalServices": {
    "Stripe": {
      "BaseUrl": "https://api.stripe.com",
      "MockMode": false
    }
  }
}
```

---

## 5. Docker Compose Stack (Development)

```yaml
services:
  api:
    build: ./src/LivestreamApp.API
    ports: ["5000:8080"]
    depends_on: [postgres, redis, localstack, mockservices]
    volumes: ["./logs:/app/logs"]
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Default=Host=postgres;Database=livestream;...
      - Redis__ConnectionString=redis:6379
      - AWS__ServiceURL=http://localstack:4566

  mockservices:
    build: ./src/LivestreamApp.MockServices
    ports: ["5001:5001"]

  postgres:
    image: postgres:16-alpine
    ports: ["5432:5432"]
    volumes: ["postgres_data:/var/lib/postgresql/data"]

  redis:
    image: redis:7-alpine
    ports: ["6379:6379"]

  localstack:
    image: localstack/localstack:3
    ports: ["4566:4566"]
    environment:
      - SERVICES=s3,ses,sns,sqs
    volumes:
      - "./localstack-init:/etc/localstack/init/ready.d"

volumes:
  postgres_data:
```

**Khởi động toàn bộ stack**: `docker-compose up` — 1 lệnh duy nhất.

---

## 6. Security Logical Components

### 6.1 JWT Middleware Pipeline

```
Request
  │
  ├── RateLimiterMiddleware (Layer 1: Global + Layer 2: Per-IP)
  ├── AuthenticationMiddleware (validate JWT từ httpOnly Cookie)
  ├── AuthorizationMiddleware (check [Authorize] attributes)
  ├── CorsMiddleware (whitelist origins)
  └── Controller Action
```

### 6.2 Cookie Configuration Component

```csharp
// Centralized cookie config — applied consistently
CookieOptions AccessTokenCookie => new()
{
    HttpOnly = true,
    Secure = true,
    SameSite = SameSiteMode.Strict,
    MaxAge = TimeSpan.FromMinutes(15),
    Path = "/",
    Domain = null  // current domain only
};

CookieOptions RefreshTokenCookie => new()
{
    HttpOnly = true,
    Secure = true,
    SameSite = SameSiteMode.Strict,
    MaxAge = TimeSpan.FromDays(30),
    Path = "/api/v1/auth/refresh"  // scoped path — không gửi kèm mọi request
};
```

> **Lưu ý**: Refresh token cookie scoped tới `/api/v1/auth/refresh` — không bị gửi kèm theo mọi API request, giảm exposure.
