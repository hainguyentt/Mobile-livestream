# NFR Design Patterns — Unit 1: Core Foundation

**Unit**: Unit 1 — Core Foundation  
**Ngày tạo**: 2026-03-21

---

## 1. Cache-Aside Pattern (Redis)

### 1.1 Blacklisted Token Cache

**Mục đích**: Tránh DB lookup mỗi lần validate JWT — revoked tokens được cache trong Redis.

```
Validate JWT Request
        │
        ├── Decode JWT (no DB call)
        ├── Check expiry
        └── Check blacklist:
                │
                ├── Redis GET revoked_token:{tokenHash}
                │       │
                │       ├── HIT  → Token revoked → 401 Unauthorized
                │       └── MISS → Token valid → proceed
                │
                └── [Fallback nếu Redis down]
                        └── DB query RefreshTokens WHERE IsRevoked=true
                            (degraded mode — acceptable)
```

**Write path** (khi revoke token):
```
RevokeToken(tokenHash)
    │
    ├── DB: UPDATE refresh_tokens SET IsRevoked=true
    └── Redis: SET revoked_token:{tokenHash} "1" EX {remainingTtlSeconds}
```

**TTL calculation**:
```csharp
var remainingTtl = token.ExpiresAt - DateTime.UtcNow;
if (remainingTtl > TimeSpan.Zero)
    await redis.SetAsync($"revoked_token:{tokenHash}", "1", remainingTtl);
// Nếu token đã expired → không cần cache (JWT validation sẽ reject trước)
```

---

### 1.2 User Profile Cache

**Mục đích**: Giảm DB reads cho profile data — đọc nhiều, ghi ít.

```
GET /api/v1/profiles/me
        │
        └── ProfileService.GetProfileAsync(userId)
                │
                ├── Redis GET user:profile:{userId}
                │       │
                │       ├── HIT  → Deserialize JSON → return
                │       └── MISS → DB query UserProfile + UserPhotos
                │                       │
                │                       └── Redis SET user:profile:{userId}
                │                               {json} EX 900 (15 min)
                │                               → return
                └── [Cache populated]
```

**Cache Invalidation Triggers** (Q1 = B):

| Trigger | Action | Lý do |
|---|---|---|
| `PUT /profiles/me` | `DEL user:profile:{userId}` | Profile data thay đổi |
| `POST /profiles/photos` | `DEL user:profile:{userId}` | Photos thay đổi |
| `DELETE /profiles/photos/{index}` | `DEL user:profile:{userId}` | Photos thay đổi |
| `PUT /profiles/photos/reorder` | `DEL user:profile:{userId}` | Photo order thay đổi |
| Admin lock/ban account | `DEL user:profile:{userId}` | Status thay đổi — client cần thấy ngay |
| Host verification approved/rejected | `DEL user:profile:{userId}` | IsVerified thay đổi |

**Pattern**: Write-invalidate (delete on write), không dùng write-through — tránh race condition.

```csharp
// ICacheInvalidationService
Task InvalidateUserProfileAsync(Guid userId);

// Gọi sau mỗi write operation
await _cacheInvalidation.InvalidateUserProfileAsync(userId);
// Cache sẽ được populate lại ở lần GET tiếp theo (lazy loading)
```

---

## 2. Token Rotation Pattern

**Mục đích**: Phát hiện token theft — nếu revoked token được dùng lại, revoke toàn bộ family.

```
POST /auth/refresh
        │
        ├── Extract refresh token từ httpOnly Cookie
        ├── Hash token (SHA-256)
        ├── DB lookup: RefreshToken WHERE TokenHash = hash
        │
        ├── [NOT FOUND] → 401 (token không tồn tại)
        │
        ├── [FOUND + IsRevoked = true] → REUSE DETECTED
        │       │
        │       └── Revoke toàn bộ token chain (theo ReplacedByTokenId)
        │           + Publish SecurityAlertEvent
        │           + 401 Unauthorized
        │
        └── [FOUND + IsRevoked = false + not expired]
                │
                ├── Revoke token hiện tại (IsRevoked = true)
                ├── Cache revoked token trong Redis
                ├── Issue new JWT (15 min)
                ├── Issue new RefreshToken (30 days)
                │   ReplacedByTokenId = old token Id
                ├── Save new token to DB
                └── Set new httpOnly Cookie → 200 OK
```

**Token Family Revocation** (reuse detection):
```csharp
// Traverse chain ngược lên để revoke tất cả
async Task RevokeTokenFamilyAsync(Guid tokenId)
{
    var token = await db.RefreshTokens.FindAsync(tokenId);
    while (token != null)
    {
        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;
        await redis.SetAsync($"revoked_token:{token.TokenHash}", "1",
            token.ExpiresAt - DateTime.UtcNow);
        token = token.ReplacedByTokenId.HasValue
            ? await db.RefreshTokens.FindAsync(token.ReplacedByTokenId)
            : null;
    }
    await db.SaveChangesAsync();
}
```

---

## 3. Retry Pattern (SES Email)

**Mục đích**: Xử lý transient failures khi gửi email OTP.

```
SendEmailAsync(to, subject, body)
        │
        ├── Attempt 1
        │       ├── SUCCESS → return
        │       └── FAIL (transient) → wait 1s
        │
        ├── Attempt 2
        │       ├── SUCCESS → return
        │       └── FAIL → wait 2s
        │
        ├── Attempt 3
        │       ├── SUCCESS → return
        │       └── FAIL → throw EmailDeliveryException
        │
        └── [Caller catches EmailDeliveryException]
                └── Return 503 Service Unavailable
                    "Email service temporarily unavailable. Please try again."
```

**Exponential backoff implementation**:
```csharp
// Dùng Polly (đã có trong .NET ecosystem)
var retryPolicy = Policy
    .Handle<AmazonSimpleEmailServiceException>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)),
        onRetry: (ex, delay, attempt, ctx) =>
            _logger.LogWarning("SES retry {Attempt}/3 after {Delay}ms: {Error}",
                attempt, delay.TotalMilliseconds, ex.Message)
    );
```

**Phân biệt transient vs permanent errors**:
| Error | Type | Action |
|---|---|---|
| `MessageRejected` | Permanent | Không retry — email address invalid |
| `MailFromDomainNotVerified` | Permanent | Không retry — config issue |
| `ThrottlingException` | Transient | Retry với backoff |
| `ServiceUnavailable` | Transient | Retry với backoff |
| Network timeout | Transient | Retry với backoff |

---

## 4. OTP Idempotency Pattern

**Mục đích**: Tạo OTP mới mỗi lần request, invalidate OTP cũ (Q2 = A).

```
POST /auth/resend-otp { email }
        │
        ├── Rate limit check (5 req/min per IP)
        ├── Load existing OtpCodes WHERE Target=email AND Purpose=EmailVerification
        │       AND IsUsed=false AND ExpiresAt > now
        │
        ├── [Existing OTP found] → Mark IsUsed=true (invalidate)
        │
        ├── Generate new OTP (6 digits, crypto-random)
        ├── Hash OTP (SHA-256)
        ├── Save new OtpCode (ExpiresAt = now + 10 min, AttemptCount = 0)
        └── Send via SES (with retry policy)
            → 200 OK "New OTP sent"
```

**Lý do chọn A** (invalidate cũ): Tránh confusion khi user nhận nhiều email OTP — chỉ OTP mới nhất hợp lệ. Đơn giản hơn C (multiple valid OTPs gây security risk).

---

## 5. Layered Rate Limiting Pattern

**Mục đích**: Defense-in-depth — 2 lớp bảo vệ độc lập.

```
Incoming Request
        │
        ├── Layer 1: Global Fixed Window
        │   Key: "global:{endpoint_group}"
        │   Limit: varies per group (xem NFR requirements)
        │   Storage: In-memory (per ECS instance)
        │   → 429 nếu global limit exceeded
        │
        └── Layer 2: Per-IP Fixed Window
            Key: "ip:{client_ip}:{endpoint_group}"
            Limit: varies per group
            Storage: In-memory (per ECS instance)
            → 429 nếu per-IP limit exceeded
```

**Response khi bị rate limit**:
```http
HTTP/1.1 429 Too Many Requests
Retry-After: 60
X-RateLimit-Limit: 20
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1711029600

{
  "error": "rate_limit_exceeded",
  "message": "Too many requests. Please try again in 60 seconds.",
  "retryAfter": 60
}
```

**IP extraction** (behind AWS ALB/CloudFront):
```csharp
// Lấy real IP từ X-Forwarded-For header (set bởi ALB)
var clientIp = httpContext.Request.Headers["X-Forwarded-For"]
    .FirstOrDefault()
    ?.Split(',').First().Trim()
    ?? httpContext.Connection.RemoteIpAddress?.ToString()
    ?? "unknown";
```

---

## 6. Soft Delete + Anonymization Pattern (APPI)

**Mục đích**: APPI Right to Erasure — xóa PII nhưng giữ audit trail.

```
DELETE /api/v1/auth/account
        │
        ├── Set User.Status = Deleted
        ├── Set User.DeletedAt = now
        ├── Revoke all RefreshTokens
        ├── Clear httpOnly Cookie
        └── 200 OK "Account deletion scheduled"

[30 ngày sau — Hangfire Job: ProcessPendingAccountDeletions]
        │
        ├── Load Users WHERE Status=Deleted AND DeletedAt < now-30days
        └── For each user:
                ├── Email → SHA-256("{email}_{userId}") + "@deleted.invalid"
                ├── PasswordHash → null
                ├── PhoneNumber → null
                ├── DisplayName → "Deleted User"
                ├── Bio → null
                ├── Interests → []
                ├── Delete UserPhotos (+ S3 objects)
                ├── Delete ExternalLogins
                ├── Delete RefreshTokens
                ├── Delete OtpCodes
                ├── Status → Anonymized
                └── [KEEP] CoinTransactions, PaymentHistory (financial audit)
```

**Anonymization là irreversible** — không có "undo". Grace period 30 ngày cho phép user đổi ý.

---

## 7. Health Check Pattern

**Mục đích**: ECS Fargate routing — chỉ route traffic khi app thực sự ready.

```
ECS Task Startup Sequence:
        │
        ├── [0s]   Container starts
        ├── [~5s]  .NET app initializes
        ├── [~10s] EF Core MigrateAsync() starts
        ├── [~20s] Migrations complete
        ├── [~25s] App starts listening
        │
        ├── Startup probe: GET /health/startup
        │   → 200 OK (migrations done + app initialized)
        │   ECS marks task as "started"
        │
        ├── Readiness probe: GET /health/ready
        │   → Check PostgreSQL connection
        │   → Check Redis connection
        │   → 200 OK → ECS adds task to load balancer target group
        │
        └── Liveness probe: GET /health/live
            → Simple 200 OK (process alive)
            → Fail → ECS restarts container
```

**Degraded mode**: Nếu Redis down nhưng PostgreSQL up → `/health/ready` trả `Degraded` (không phải `Unhealthy`) — app vẫn nhận traffic nhưng không có cache.

```csharp
// Redis check: degraded (không critical) vs PostgreSQL: unhealthy (critical)
.AddRedis(redisConn, name: "redis",
    failureStatus: HealthStatus.Degraded,  // Redis down → degraded, không stop traffic
    tags: ["ready"])
.AddNpgSql(pgConn, name: "postgresql",
    failureStatus: HealthStatus.Unhealthy, // DB down → unhealthy, stop traffic
    tags: ["ready"])
```

---

## 8. S3 Presigned URL Pattern

**Mục đích**: Client upload trực tiếp lên S3 — giảm tải API server, không stream file qua backend.

```
Phase 1: Get Upload URL
Client → POST /api/v1/profiles/photos/presign
              { displayIndex, contentType, fileSizeBytes }
                        │
                        ├── Validate: contentType ∈ {image/jpeg, image/png, image/webp}
                        ├── Validate: fileSizeBytes ≤ 10MB
                        ├── Validate: displayIndex ∈ [0..5]
                        ├── Generate photoId (ULID)
                        ├── S3 key: "profiles/{userId}/{photoId}.{ext}"
                        ├── Generate presigned PUT URL (5 min expiry)
                        │   Conditions: ContentType match, ContentLength ≤ 10MB
                        └── Return { uploadUrl, photoId, s3Key }

Phase 2: Direct Upload (client → S3, no API server involved)
Client → PUT {uploadUrl}
              Content-Type: image/jpeg
              [binary file data]
                        │
                        └── S3 stores object

Phase 3: Confirm Upload
Client → POST /api/v1/profiles/photos/confirm
              { photoId, displayIndex }
                        │
                        ├── S3 HeadObject(s3Key) → verify exists
                        ├── Download + ImageSharp resize (max 1080x1080)
                        ├── Re-upload optimized version (overwrite)
                        ├── [IF displayIndex already has photo] → Delete old S3 object
                        ├── Save/Update UserPhoto record
                        ├── Invalidate user profile cache
                        └── Return { photo }
```

**Security**: Presigned URL chỉ cho phép PUT method, đúng Content-Type, đúng Content-Length range. Không thể dùng URL để đọc file khác.

---

## 9. Admin Audit Log Pattern

**Mục đích**: Append-only audit trail cho tất cả admin actions — APPI compliance.

```
Admin Action (ví dụ: approve host verification)
        │
        └── AdminActionLog.Append({
                AdminId: currentAdmin.Id,
                Action: "HostVerificationApproved",
                TargetUserId: hostUserId,
                Details: { "previousStatus": "Pending", "newStatus": "Approved" },
                IpAddress: request.ClientIp,
                Timestamp: DateTime.UtcNow
            })
```

**Append-only enforcement**:
- Không có UPDATE/DELETE trên `AdminActionLog` table
- DB-level: `REVOKE UPDATE, DELETE ON admin_action_logs FROM app_user`
- Application-level: Repository chỉ expose `AppendAsync()`, không có `UpdateAsync()`/`DeleteAsync()`

**Actions được log trong Unit 1**:
| Action | Trigger |
|---|---|
| `HostVerificationApproved` | Admin approve verified badge |
| `HostVerificationRejected` | Admin reject verified badge |
| `AccountLocked` | Admin lock user account (Unit 5) |
| `AccountUnlocked` | Admin unlock user account (Unit 5) |
| `AccountBanned` | Admin ban user (Unit 5) |
