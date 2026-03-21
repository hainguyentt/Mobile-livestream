# Technical Risk Mitigation — Unit 1: Core Foundation

**Unit**: Unit 1 — Core Foundation  
**Ngày tạo**: 2026-03-21  
**Scope**: Production readiness risks và mitigation strategies

---

## Overview

Document này xác định 9 technical risks chính có thể ảnh hưởng đến production system khi scale, cùng với mitigation strategies từ MVP đến extreme scale. Tất cả risks đều có MVP-level mitigation — không có "implement sau".

**Implementation priority**: Thêm ~2-3 ngày development time cho safeguards trong Code Generation stage.

---

## Risk 1: DB Write Bottleneck

**Mức độ**: High — ảnh hưởng trực tiếp khi scale (billing ticks, chat, coin transactions)

**Nguyên nhân**: Nhiều write-heavy operations đồng thời:
- `ProcessBillingTicks` job: 1 write/10s per active call session
- `direct_messages`: mỗi tin nhắn = 1 INSERT
- `coin_transactions`: mỗi gift/payment = INSERT + UPDATE balance
- `login_attempts`: ghi mỗi lần login attempt

**Giải pháp theo tầng (áp dụng dần khi scale)**:

| Tầng | Giải pháp | Trigger | Effort |
|---|---|---|---|
| T1 (MVP) | Write batching cho billing — gom ticks vào Redis, flush mỗi 30s | Ngay từ đầu | Thấp |
| T1 (MVP) | Optimistic concurrency cho `coin_balances` (`xmin`/`RowVersion`) | Ngay từ đầu | Thấp |
| T1 (MVP) | `login_attempts` write async (fire-and-forget) | Ngay từ đầu | Thấp |
| T2 (Scale) | Tăng RDS gp3 IOPS: 3,000 → 16,000 (~$0.02/IOPS/tháng) | IOPS > 70% sustained | Zero code change |
| T3 (Scale) | RDS Proxy — connection pooling tập trung | > 200 concurrent connections | Config only |
| T4 (High Scale) | Tách write-heavy tables sang separate RDS instance | Single RDS không đủ | Medium |
| T5 (Extreme) | Event Sourcing cho billing — append-only log, materialize async | > 10K concurrent sessions | High |

**Thiết kế phòng ngừa trong code (áp dụng ngay từ Unit 1)**:
```csharp
// Optimistic concurrency cho coin balance — tránh lock contention
public class CoinBalance
{
    public uint RowVersion { get; set; }  // PostgreSQL xmin column
}

// Billing batch accumulation — Redis buffer, flush mỗi 30s
// Thay vì: 1 DB write mỗi 10s per session
// Thành: 1 DB write mỗi 30s per session (3x reduction)
```

---

## Risk 2: Read/Write Separation

**Mức độ**: Medium — cần chuẩn bị code pattern ngay, bật infrastructure khi cần

**Vấn đề**: EF Core mặc định dùng 1 connection → tất cả SELECT đi vào primary write instance, cạnh tranh với writes.

**Giải pháp**: Dual DbContext pattern — chuẩn bị code ngay, bật Read Replica khi cần

```csharp
// Write context — primary instance
services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(config["ConnectionStrings:Write"]));

// Read-only context — read replica (MVP: same connection string as Write)
// Khi cần scale: chỉ đổi ConnectionStrings:ReadOnly → không cần refactor
services.AddDbContext<ReadOnlyDbContext>(opt =>
    opt.UseNpgsql(config["ConnectionStrings:ReadOnly"])
       .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
```

**Convention trong service layer**:
- Inject `ReadOnlyDbContext` cho tất cả GET/query operations
- Inject `AppDbContext` chỉ cho write operations (INSERT/UPDATE/DELETE)
- MVP: `ReadOnly` = `Write` (same connection string) → zero overhead
- Scale: Thêm RDS Read Replica + đổi `ConnectionStrings:ReadOnly` → done

**Trigger bật Read Replica**: RDS CPU > 60% sustained hoặc read latency > 100ms p95.
**Cost**: +~$25/tháng (thêm 1 db.t3.small read replica).

---

## Risk 3: EF Core Query Performance

**Mức độ**: Medium — ảnh hưởng khi data volume tăng và queries phức tạp hơn

**Vấn đề**: EF Core có thể generate suboptimal SQL cho complex queries (N+1, cartesian explosion, inefficient aggregations).

**Giải pháp: Hybrid EF Core + Dapper**

| Query Type | Tool | Lý do |
|---|---|---|
| Simple CRUD (find by ID, insert, update) | EF Core | Productivity, type-safe |
| Complex JOIN (≤ 5 tables) | EF Core + `.AsSplitQuery()` | Tránh cartesian explosion |
| Aggregation / Analytics / Reporting | Dapper (raw SQL) | Kiểm soát hoàn toàn execution plan |
| Full-text search (user/host search) | PostgreSQL `tsvector` + EF Core | Native PG capability |
| Leaderboard queries | Redis Sorted Sets | Không query DB |
| Admin reports (DAU/MAU, revenue) | Dapper + ReadOnlyDbContext | Không ảnh hưởng write path |

**Phòng ngừa N+1 — convention bắt buộc**:
```csharp
// FORBIDDEN — N+1 pattern
var users = await db.Users.ToListAsync();
foreach (var u in users) u.Profile = await db.UserProfiles.FindAsync(u.Id);

// REQUIRED — explicit eager loading
var users = await db.Users
    .Include(u => u.Profile)
    .Include(u => u.Photos.Where(p => p.DisplayIndex == 0))
    .AsSplitQuery()  // tránh cartesian explosion khi nhiều collections
    .ToListAsync();
```

**Slow query monitoring** (bật từ đầu):
```csharp
// Log EF Core queries > 500ms vào CloudWatch
// Cấu hình trong appsettings.json:
// "EFCore:SlowQueryThresholdMs": 500
```

**Dapper cho analytics** (ví dụ Unit 5):
```csharp
public class AnalyticsRepository(IDbConnection readConn)
{
    public Task<IEnumerable<TopHostDto>> GetTopHostsAsync(DateRange range)
        => readConn.QueryAsync<TopHostDto>("""
            SELECT u.id, up.display_name, SUM(ct.amount) as total_coins
            FROM coin_transactions ct
            JOIN users u ON ct.recipient_id = u.id
            JOIN user_profiles up ON u.id = up.user_id
            WHERE ct.created_at BETWEEN @Start AND @End
            GROUP BY u.id, up.display_name
            ORDER BY total_coins DESC LIMIT 50
            """, new { range.Start, range.End });
}
```

**NuGet cần thêm**:
```xml
<PackageReference Include="Dapper" Version="2.*" />
```

---

## Risk 4: SignalR Connection Scalability (Unit 2+)

**Mức độ**: High — critical cho real-time features (livestream, chat, notifications)

**Vấn đề**: SignalR connections là stateful, sticky sessions. Khi scale ECS tasks:
- 1 task = max ~10,000 concurrent WebSocket connections (memory limit)
- Khi user reconnect sau deploy → có thể connect vào task khác → mất state
- SignalR backplane qua Redis có latency overhead (~10-50ms per message)

**Giải pháp theo tầng**:

| Tầng | Giải pháp | Trigger | Implementation |
|---|---|---|---|
| T1 (MVP) | Redis backplane — scale-out qua Redis Pub/Sub | Ngay từ Unit 2 | `AddStackExchangeRedis()` |
| T2 (Scale) | Connection limit per task: 5,000 → reject new khi đạt | > 3,000 connections/task | Middleware check |
| T3 (Scale) | Tách SignalR hubs ra separate ECS service | > 20K total connections | Medium effort |
| T4 (High Scale) | Azure SignalR Service (managed) hoặc self-hosted SignalR cluster | > 100K connections | High cost/effort |

**Thiết kế phòng ngừa (Unit 2)**:
```csharp
// SignalR với Redis backplane — bắt buộc từ đầu
services.AddSignalR()
    .AddStackExchangeRedis(config["Redis:ConnectionString"], options =>
    {
        options.Configuration.ChannelPrefix = "signalr:";
    });

// Connection limit middleware
app.Use(async (context, next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var currentConnections = SignalRMetrics.GetConnectionCount();
        if (currentConnections > 5000)
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsync("Server at capacity");
            return;
        }
    }
    await next();
});
```

**Monitoring metrics**:
- `signalr_connections_current` — alert khi > 4,000 per task
- `signalr_backplane_latency_ms` — alert khi > 100ms p95
- `signalr_reconnect_rate` — alert khi > 10% of connections/min

---

## Risk 5: Redis Memory Exhaustion

**Mức độ**: High — Redis là single point of failure cho cache + SignalR + room chat

**Vấn đề**: cache.t3.micro chỉ có 0.5GB RAM. Khi scale:
- Room chat (Redis Streams): 100 active rooms × 1MB history = 100MB
- SignalR backplane: 5,000 connections × 2KB state = 10MB
- User profile cache: 2,000 cached × 2KB = 4MB
- Revoked tokens: 10,000 tokens × 50 bytes = 0.5MB
- **Total**: ~115MB (đã 23% capacity)

**Giải pháp theo tầng**:

| Tầng | Giải pháp | Trigger | Cost Impact |
|---|---|---|---|
| T1 (MVP) | Eviction policy: `allkeys-lru` — auto-evict least recently used | Ngay từ đầu | Zero |
| T1 (MVP) | Maxmemory-policy: `volatile-lru` cho keys có TTL | Ngay từ đầu | Zero |
| T2 (Scale) | Upgrade: cache.t3.micro → cache.t3.small (1.5GB) | Memory > 80% | +$13/tháng |
| T3 (Scale) | Tách Redis instances: Cache + SignalR + RoomChat riêng | Memory pressure | +$26/tháng (2 instances) |
| T4 (High Scale) | Redis Cluster mode (sharding) | > 10GB data | Medium effort |

**Thiết kế phòng ngừa**:
```bash
# Redis config (ElastiCache parameter group)
maxmemory-policy allkeys-lru
maxmemory 450mb  # 90% of 0.5GB, leave headroom

# TTL bắt buộc cho mọi key (không có key vĩnh viễn)
# Convention: mọi SET phải có EXPIRE
```

```csharp
// Wrapper bắt buộc TTL
public class SafeCacheService : ICacheService
{
    public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        if (ttl == null)
            throw new ArgumentException("TTL is mandatory for all cache keys");
        return _redis.SetAsync(key, value, ttl.Value);
    }
}
```

**Monitoring**:
- `redis_memory_used_percent` — alert > 80%
- `redis_evicted_keys` — alert khi > 100/min (sign of memory pressure)
- `redis_keyspace_misses_rate` — track cache effectiveness

---

## Risk 6: S3 Cost Explosion (Photo Storage)

**Mức độ**: Medium — cost risk, không phải technical failure

**Vấn đề**: User upload ảnh không giới hạn → cost tăng không kiểm soát:
- 10,000 users × 6 ảnh × 2MB = 120GB storage (~$3/tháng)
- Nhưng: user upload rồi xóa rồi upload lại → orphaned objects
- CloudFront data transfer: 1TB/tháng = ~$85

**Giải pháp phòng ngừa**:

| Layer | Giải pháp | Implementation |
|---|---|---|
| **Prevention** | Hard limit: 6 ảnh per user, 10MB per ảnh | API validation |
| **Prevention** | Image optimization: resize 1080x1080, compress 80% quality | ImageSharp processing |
| **Cleanup** | S3 Lifecycle: delete incomplete multipart uploads after 1 day | S3 bucket policy |
| **Cleanup** | Orphaned object cleanup: Hangfire job scan DB vs S3, delete orphans | Weekly job |
| **Cost Control** | S3 Intelligent-Tiering: auto-move infrequent access → cheaper tier | S3 storage class |
| **Monitoring** | CloudWatch billing alarm: > $50/tháng → SNS alert | CloudWatch alarm |

**Orphaned object cleanup job** (Unit 1):
```csharp
// Hangfire job: mỗi tuần
public async Task CleanupOrphanedPhotosAsync()
{
    // 1. List all S3 objects in profiles/ prefix
    var s3Objects = await _s3.ListObjectsV2Async("profiles/");
    
    // 2. Load all UserPhoto records from DB
    var dbPhotoKeys = await _db.UserPhotos.Select(p => p.S3Key).ToListAsync();
    
    // 3. Find orphans: in S3 but not in DB
    var orphans = s3Objects.Except(dbPhotoKeys);
    
    // 4. Delete orphans (batch 1000)
    foreach (var batch in orphans.Chunk(1000))
        await _s3.DeleteObjectsAsync(batch);
    
    _logger.LogInformation("Cleaned up {Count} orphaned photos", orphans.Count());
}
```

**Cost estimation với safeguards**:
- Storage: 120GB × $0.023/GB = ~$3/tháng
- CloudFront transfer: 500GB × $0.085/GB = ~$43/tháng (với optimization)
- Total S3+CloudFront: ~$46/tháng (trong budget)

---

## Risk 7: JWT Secret Rotation Downtime

**Mức độ**: Medium — security best practice, nhưng có thể gây downtime nếu không chuẩn bị

**Vấn đề**: JWT signed bằng 1 secret key. Khi rotate key (security requirement hàng năm):
- Tất cả JWT hiện tại invalid ngay lập tức
- User bị logout hàng loạt → bad UX
- Refresh token vẫn valid nhưng không issue được JWT mới

**Giải pháp: Multi-Key JWT Validation**

```csharp
// Secrets Manager: lưu array of keys thay vì 1 key
{
  "JwtKeys": [
    { "KeyId": "2024-01", "Secret": "old-key-256-bit", "ValidUntil": "2025-01-01" },
    { "KeyId": "2025-01", "Secret": "new-key-256-bit", "ValidUntil": "2026-01-01" }
  ]
}

// JWT signing: dùng key mới nhất
var latestKey = jwtKeys.OrderByDescending(k => k.ValidUntil).First();
var token = new JwtSecurityToken(
    claims: claims,
    signingCredentials: new SigningCredentials(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(latestKey.Secret)),
        SecurityAlgorithms.HmacSha256)
);
token.Header.Add("kid", latestKey.KeyId);  // Key ID trong header

// JWT validation: thử tất cả keys còn valid
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                // Tìm key theo kid trong header
                var key = jwtKeys.FirstOrDefault(k => k.KeyId == kid);
                if (key == null || key.ValidUntil < DateTime.UtcNow)
                    return null;
                return new[] { new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key.Secret)) };
            }
        };
    });
```

**Rotation process (zero downtime)**:
1. Thêm key mới vào Secrets Manager (giữ key cũ)
2. Deploy app → app sign JWT bằng key mới, validate cả 2 keys
3. Đợi 15 phút (JWT expiry) → tất cả JWT cũ hết hạn tự nhiên
4. Xóa key cũ khỏi Secrets Manager
5. Deploy lại (optional cleanup)

**Automation**: Hangfire job check `ValidUntil` → alert admin 30 ngày trước khi key expire.

---

## Risk 8: Agora.io Free Tier Limits (Unit 2)

**Mức độ**: High — blocking risk khi vượt free tier

**Vấn đề**: Agora Free Tier limits:
- 10,000 phút/tháng miễn phí
- Sau đó: $0.99/1,000 phút (voice) hoặc $3.99/1,000 phút (video HD)
- 100 concurrent channels max

**Ước tính thực tế**:
- 1,000 users × 30 phút/tháng = 30,000 phút → $20-80/tháng (vượt free tier ngay)
- 50 concurrent livestreams × 20 viewers × 30 phút = 30,000 phút

**Giải pháp phòng ngừa**:

| Layer | Giải pháp | Implementation |
|---|---|---|
| **Monitoring** | Track Agora usage real-time qua webhook | Agora callback → log CloudWatch |
| **Cost Control** | Hard limit: 50 concurrent channels (MVP) | Middleware check trước `StartLivestream` |
| **Cost Control** | Billing alert: > $50/tháng → SNS | CloudWatch billing alarm |
| **Fallback** | Downgrade video quality: HD → SD khi approaching limit | Dynamic quality adjustment |
| **Business** | Coin cost cho private call đủ cover Agora cost | Pricing model |

**Usage tracking**:
```csharp
// Agora webhook handler
[HttpPost("/webhooks/agora")]
public async Task AgoraWebhook([FromBody] AgoraEvent evt)
{
    if (evt.EventType == "channel_duration")
    {
        await _metrics.RecordAgoraUsage(
            channelId: evt.ChannelId,
            durationMinutes: evt.Duration / 60,
            participantCount: evt.ParticipantCount
        );
        
        // Alert nếu approaching limit
        var monthlyUsage = await _metrics.GetMonthlyAgoraUsage();
        if (monthlyUsage > 8000) // 80% of free tier
            await _alerts.SendAsync("Agora usage > 80% free tier");
    }
}
```

**Concurrent channel limit**:
```csharp
public async Task<Result> StartLivestreamAsync(Guid hostId)
{
    var activeChannels = await _db.Rooms.CountAsync(r => r.Status == RoomStatus.Live);
    if (activeChannels >= 50)
        return Result.Fail("System at capacity. Please try again later.");
    
    // Proceed with Agora token generation...
}
```

---

## Risk 9: APPI Data Breach Response Plan

**Mức độ**: Critical — legal/compliance risk, không phải technical

**Vấn đề**: APPI (Act on Protection of Personal Information) yêu cầu:
- Báo cáo data breach cho PPC (Personal Information Protection Commission) trong 72 giờ
- Thông báo affected users
- Document incident response process

**Giải pháp: Incident Response Playbook**

**Detection mechanisms** (implement ngay):
```csharp
// Audit log cho tất cả PII access
public class PiiAccessAuditMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api/v1/profiles") ||
            context.Request.Path.StartsWithSegments("/api/v1/admin/users"))
        {
            await _auditLog.LogAsync(new PiiAccessEvent
            {
                UserId = context.User.GetUserId(),
                Action = context.Request.Method,
                Path = context.Request.Path,
                IpAddress = context.Connection.RemoteIpAddress,
                Timestamp = DateTime.UtcNow
            });
        }
        await _next(context);
    }
}

// Anomaly detection: alert khi 1 admin access > 100 user profiles trong 1 giờ
// CloudWatch Insights query scheduled mỗi giờ
```

**Response playbook** (document trong `docs/security/`):
1. **Detection** (0-1h): CloudWatch alarm → on-call engineer
2. **Containment** (1-4h): Revoke compromised credentials, block IPs, isolate affected systems
3. **Assessment** (4-24h): Determine scope (which users, what data), check audit logs
4. **Notification** (24-72h): Report to PPC, email affected users (template prepared)
5. **Remediation** (1-4 weeks): Patch vulnerability, rotate secrets, security audit
6. **Post-mortem** (4 weeks): Document lessons learned, update playbook

**Prepared artifacts** (create in Unit 1):
- `docs/security/incident-response-playbook.md`
- `docs/security/ppc-breach-notification-template.md` (Japanese)
- `docs/security/user-breach-notification-email-template.md` (Japanese + English)
- Runbook: "How to extract affected user list from audit logs"

**Compliance automation**:
```csharp
// Hangfire job: daily security audit report
public async Task DailySecurityAuditAsync()
{
    var report = new SecurityAuditReport
    {
        Date = DateTime.UtcNow.Date,
        FailedLoginAttempts = await _db.LoginAttempts.CountAsync(/* last 24h */),
        AdminActionsCount = await _db.AdminActionLogs.CountAsync(/* last 24h */),
        PiiAccessCount = await _db.PiiAccessLogs.CountAsync(/* last 24h */),
        AnomaliesDetected = await DetectAnomaliesAsync()
    };
    
    await _storage.UploadAsync($"security-audits/{report.Date:yyyy-MM-dd}.json", report);
    
    if (report.AnomaliesDetected.Any())
        await _alerts.SendAsync("Security anomalies detected", report);
}
```

---

## Risk Summary Table (All 9 Risks)

| # | Risk | Severity | MVP Mitigation | Scale Trigger | Effort |
|---|---|---|---|---|---|
| 1 | DB Write Bottleneck | High | Batch writes, optimistic concurrency | IOPS > 70% | Low |
| 2 | Read/Write Contention | Medium | Dual DbContext pattern | CPU > 60% | Low |
| 3 | EF Core Slow Queries | Medium | Hybrid EF+Dapper, monitoring | Query > 500ms | Low |
| 4 | SignalR Scalability | High | Redis backplane, connection limit | > 3K conn/task | Medium |
| 5 | Redis Memory Exhaustion | High | LRU eviction, mandatory TTL | Memory > 80% | Low |
| 6 | S3 Cost Explosion | Medium | Hard limits, orphan cleanup | Cost > $50/mo | Low |
| 7 | JWT Secret Rotation | Medium | Multi-key validation | Annual rotation | Low |
| 8 | Agora Free Tier | High | Usage tracking, channel limit | > 8K min/mo | Low |
| 9 | APPI Data Breach | Critical | Audit logs, response playbook | Incident occurs | Medium |

---

## Monitoring Triggers Summary

| Risk | Metric | Threshold | Action |
|---|---|---|---|
| Write Bottleneck | RDS IOPS Utilization | > 70% sustained | Tăng IOPS hoặc batch writes |
| Read/Write Contention | RDS CPU Utilization | > 60% sustained | Bật Read Replica |
| EF Core Performance | Query Duration p95 | > 500ms | Optimize với Dapper, add indexes |
| SignalR Scalability | Connections per task | > 3,000 | Scale out tasks hoặc tách service |
| Redis Memory | Memory Used % | > 80% | Upgrade instance hoặc tách Redis |
| S3 Cost | Monthly S3+CloudFront bill | > $50 | Review usage, optimize images |
| JWT Rotation | Key ValidUntil date | < 30 days | Rotate key theo playbook |
| Agora Usage | Monthly minutes | > 8,000 (80% free) | Alert team, review pricing |
| APPI Breach | Anomaly detection | > 100 profiles/hour | Trigger incident response |

---

## Cross-Unit Patterns

Các patterns sau áp dụng cho **tất cả units** — implement ngay từ Unit 1:

1. **Dual DbContext**: Read/Write separation pattern
2. **Hybrid EF+Dapper**: Complex queries dùng Dapper
3. **Write Batching**: High-frequency operations buffer qua Redis
4. **Optimistic Concurrency**: Shared resources dùng `xmin`/`RowVersion`
5. **Mandatory TTL**: Tất cả Redis keys phải có expiry
6. **Slow Query Logging**: Log queries > 500ms vào CloudWatch
7. **PII Access Audit**: Middleware log tất cả PII access

Chi tiết implementation: xem `shared-infrastructure.md` Section 6.
