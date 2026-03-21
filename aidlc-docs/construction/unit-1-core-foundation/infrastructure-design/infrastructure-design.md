# Infrastructure Design — Unit 1: Core Foundation

**Unit**: Unit 1 — Core Foundation  
**Cloud Provider**: AWS (ap-northeast-1 — Tokyo)  
**Ngày tạo**: 2026-03-21

---

## 1. Infrastructure Services Map

| Logical Component | AWS Service | Tier | Config |
|---|---|---|---|
| API Application | ECS Fargate | Compute | 1 vCPU / 2GB RAM, min 2 tasks |
| MockServices | ECS Fargate (dev only) | Compute | 0.25 vCPU / 512MB (dev/staging) |
| PostgreSQL | RDS PostgreSQL 16 | Data | db.t3.small, Multi-AZ |
| Redis | ElastiCache Redis 7 | Cache | cache.t3.micro, single node (MVP) |
| Object Storage | S3 | Storage | Standard class, SSE-S3 |
| CDN | CloudFront | CDN | Origin: S3 + ALB |
| Email | SES | Messaging | ap-northeast-1, verified domain |
| Load Balancer | ALB | Networking | HTTPS listener, health check |
| Container Registry | ECR | Registry | Private repo per service |
| Secrets | Secrets Manager | Security | DB credentials, JWT secret, API keys |
| Logs | CloudWatch Logs | Observability | Log group `/livestream-app/api` |
| Metrics | CloudWatch Metrics | Observability | ECS + RDS + Redis metrics |
| Background Jobs | Hangfire (in-process) | Compute | Chạy trong ECS task, PostgreSQL storage |

---

## 2. Compute — ECS Fargate

### 2.1 API Service Task Definition

```json
{
  "family": "livestream-api",
  "cpu": "1024",
  "memory": "2048",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "containerDefinitions": [
    {
      "name": "api",
      "image": "{account}.dkr.ecr.ap-northeast-1.amazonaws.com/livestream-api:latest",
      "portMappings": [{ "containerPort": 8080, "protocol": "tcp" }],
      "environment": [
        { "name": "ASPNETCORE_ENVIRONMENT", "value": "Production" },
        { "name": "ASPNETCORE_URLS", "value": "http://+:8080" }
      ],
      "secrets": [
        { "name": "ConnectionStrings__Default",
          "valueFrom": "arn:aws:secretsmanager:...:livestream/db-connection" },
        { "name": "Redis__ConnectionString",
          "valueFrom": "arn:aws:secretsmanager:...:livestream/redis-connection" },
        { "name": "Jwt__Secret",
          "valueFrom": "arn:aws:secretsmanager:...:livestream/jwt-secret" },
        { "name": "LineLogin__ClientSecret",
          "valueFrom": "arn:aws:secretsmanager:...:livestream/line-client-secret" }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/livestream-app/api",
          "awslogs-region": "ap-northeast-1",
          "awslogs-stream-prefix": "ecs"
        }
      },
      "healthCheck": {
        "command": ["CMD-SHELL", "curl -f http://localhost:8080/health/live || exit 1"],
        "interval": 30,
        "timeout": 5,
        "retries": 3,
        "startPeriod": 60
      }
    }
  ]
}
```

### 2.2 ECS Service Config

```
Service: livestream-api-service
  Cluster: livestream-cluster
  Launch type: FARGATE
  Desired count: 2 (minimum for HA)
  Min healthy percent: 100  (rolling deploy — no downtime)
  Max percent: 200

Auto Scaling:
  Min: 2 tasks
  Max: 10 tasks
  Scale out: CPU > 70% for 2 minutes → +2 tasks
  Scale in:  CPU < 30% for 5 minutes → -1 task
```

### 2.3 Cost Estimate (MVP — 2 tasks running 24/7)

| Resource | Config | Cost/month |
|---|---|---|
| ECS Fargate (2 tasks) | 1vCPU × 2GB × 2 tasks | ~$72 |
| RDS db.t3.small Multi-AZ | 2 vCPU, 2GB | ~$50 |
| ElastiCache cache.t3.micro | 0.5 vCPU, 0.5GB | ~$13 |
| ALB | 1 ALB + LCU | ~$20 |
| S3 + CloudFront | 10GB storage + transfer | ~$5 |
| SES | 10K emails/month | ~$1 |
| CloudWatch | Logs + metrics | ~$5 |
| ECR | 2 repos | ~$1 |
| Secrets Manager | 5 secrets | ~$2 |
| **Total MVP** | | **~$169/tháng** |

---

## 3. Database — RDS PostgreSQL

### 3.1 Instance Config

```
Engine: PostgreSQL 16
Instance: db.t3.small (2 vCPU, 2GB RAM)
Storage: 20GB gp3 SSD (auto-scale to 100GB)
Multi-AZ: Yes (standby in ap-northeast-1c)
Backup: Automated daily, retention 30 days
PITR: Enabled
Encryption: AES-256 at-rest
Parameter group: Custom
  - max_connections: 100
  - shared_buffers: 512MB
  - log_min_duration_statement: 1000ms (log slow queries)
```

### 3.2 Connection String Pattern

```
Host=livestream-db.xxxxx.ap-northeast-1.rds.amazonaws.com;
Port=5432;
Database=livestream;
Username=app_user;
Password={from Secrets Manager};
SSL Mode=Require;
Trust Server Certificate=false;
Maximum Pool Size=50;
Connection Idle Lifetime=300;
```

### 3.3 Database Users

| User | Permissions | Mục đích |
|---|---|---|
| `app_user` | SELECT, INSERT, UPDATE, DELETE | Application runtime |
| `migration_user` | ALL (schema changes) | EF Core migrations only |
| `readonly_user` | SELECT only | Analytics, debugging |

> **Security**: `app_user` không có DROP/ALTER quyền. Migrations chạy với `migration_user` credentials riêng.

---

## 4. Cache — ElastiCache Redis

### 4.1 Instance Config

```
Engine: Redis 7.x
Node type: cache.t3.micro (0.5 vCPU, 0.5GB RAM)
Cluster mode: Disabled (single node, MVP)
Multi-AZ: No (MVP — upgrade khi cần)
Encryption in-transit: Yes (TLS)
Encryption at-rest: Yes
Auth token: Yes (from Secrets Manager)
Backup: Daily snapshot, retention 7 days
```

### 4.2 Memory Estimation (MVP)

| Key Pattern | Estimated Count | Size/key | Total |
|---|---|---|---|
| `revoked_token:*` | ~1,000 active | 50 bytes | ~50KB |
| `user:profile:*` | ~500 cached | 2KB | ~1MB |
| `otp:rate_limit:*` | ~100 active | 20 bytes | ~2KB |
| `login:rate_limit:*` | ~200 active | 20 bytes | ~4KB |
| **Total** | | | **~1.1MB** (well within 0.5GB) |

---

## 5. Storage — S3

### 5.1 Bucket Config

```
Bucket: livestream-app-profiles-{account-id}
Region: ap-northeast-1
Versioning: Disabled (photos replaced, not versioned)
Encryption: SSE-S3 (AES-256)
Public access: Block all public access
Lifecycle rules:
  - Incomplete multipart uploads: Delete after 1 day
CORS: Not needed (presigned URL upload, no browser direct access)
```

### 5.2 IAM Policy cho ECS Task Role

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": ["s3:PutObject", "s3:GetObject", "s3:DeleteObject", "s3:HeadObject"],
      "Resource": "arn:aws:s3:::livestream-app-profiles-*/*"
    },
    {
      "Effect": "Allow",
      "Action": ["s3:GeneratePresignedUrl"],
      "Resource": "arn:aws:s3:::livestream-app-profiles-*/*"
    }
  ]
}
```

### 5.3 CloudFront Distribution

```
Origin: S3 bucket (OAC — Origin Access Control)
Price class: PriceClass_200 (US, EU, Asia — bao gồm Nhật)
Cache behavior:
  - /profiles/*: Cache 1 ngày (ảnh không thay đổi sau upload)
  - Default TTL: 86400s
Viewer protocol: HTTPS only
```

---

## 6. Networking — VPC (Simple Layout)

### 6.1 VPC Structure

```
VPC: 10.0.0.0/16 (ap-northeast-1)
│
├── Public Subnet: 10.0.1.0/24 (ap-northeast-1a)
│   └── ALB (internet-facing)
│
├── Public Subnet: 10.0.2.0/24 (ap-northeast-1c)
│   └── ALB (second AZ for HA)
│
└── Private Subnet: 10.0.10.0/24 (ap-northeast-1a)
    ├── ECS Fargate Tasks
    ├── RDS Primary
    ├── RDS Standby (Multi-AZ, same private subnet range)
    └── ElastiCache Redis
```

### 6.2 Security Groups

```
sg-alb (ALB):
  Inbound:  443 (HTTPS) from 0.0.0.0/0
            80 (HTTP) from 0.0.0.0/0 → redirect to 443
  Outbound: 8080 to sg-ecs

sg-ecs (ECS Tasks):
  Inbound:  8080 from sg-alb only
  Outbound: 5432 to sg-rds
            6379 to sg-redis
            443 to 0.0.0.0/0 (SES, S3, Secrets Manager, ECR)

sg-rds (RDS):
  Inbound:  5432 from sg-ecs only
  Outbound: None

sg-redis (ElastiCache):
  Inbound:  6379 from sg-ecs only
  Outbound: None
```

### 6.3 ALB Config

```
Scheme: internet-facing
Listeners:
  - HTTP:80 → Redirect to HTTPS:443
  - HTTPS:443 → Forward to ECS target group

Target Group:
  Protocol: HTTP
  Port: 8080
  Health check: GET /health/ready
  Healthy threshold: 2
  Unhealthy threshold: 3
  Interval: 30s
  Timeout: 10s

SSL Certificate: ACM (*.livestream-app.jp)
```

---

## 7. Secrets Management

| Secret Name | Content | Rotation |
|---|---|---|
| `livestream/db-connection` | PostgreSQL connection string | Manual (quarterly) |
| `livestream/redis-connection` | Redis connection + auth token | Manual |
| `livestream/jwt-secret` | JWT signing key (256-bit) | Manual (yearly) |
| `livestream/line-client-secret` | LINE Login client secret | Manual |
| `livestream/ses-config` | SES region + sender email | Static |

**Access**: ECS Task Role có `secretsmanager:GetSecretValue` permission cho prefix `livestream/*`.

---

## 8. CI/CD Pipeline (Skeleton)

```
GitHub Actions / AWS CodePipeline:

1. Code push → main branch
2. Build: dotnet build + test
3. Docker build: docker build -t livestream-api .
4. Push to ECR: docker push {ecr-url}/livestream-api:latest
5. ECS Deploy: aws ecs update-service --force-new-deployment
   → ECS rolling update (min 100% healthy → new tasks start → old tasks stop)
6. Health check: ALB confirms /health/ready returns 200
```

**Dockerfile skeleton**:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/LivestreamApp.API/LivestreamApp.API.csproj", "src/LivestreamApp.API/"]
RUN dotnet restore "src/LivestreamApp.API/LivestreamApp.API.csproj"
COPY . .
RUN dotnet publish "src/LivestreamApp.API/LivestreamApp.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LivestreamApp.API.dll"]
```

---

## 9. Monitoring & Alerting

### 9.1 CloudWatch Alarms (Unit 1)

| Alarm | Metric | Threshold | Action |
|---|---|---|---|
| API High CPU | ECS CPUUtilization | > 80% for 5 min | SNS → Email |
| API High Memory | ECS MemoryUtilization | > 85% for 5 min | SNS → Email |
| RDS High CPU | RDS CPUUtilization | > 80% for 5 min | SNS → Email |
| RDS Low Storage | RDS FreeStorageSpace | < 5GB | SNS → Email |
| ALB 5xx Errors | ALB HTTPCode_Target_5XX | > 10/min | SNS → Email |
| ALB High Latency | ALB TargetResponseTime | > 1s p95 | SNS → Email |
| Redis High Memory | ElastiCache DatabaseMemoryUsagePercentage | > 80% | SNS → Email |

### 9.2 CloudWatch Log Insights Queries

```sql
-- Top errors in last 1 hour
fields @timestamp, @message
| filter level = "Error"
| sort @timestamp desc
| limit 50

-- Slow API requests (> 500ms)
fields @timestamp, requestPath, durationMs
| filter durationMs > 500
| sort durationMs desc
| limit 20

-- Failed login attempts by IP
fields @timestamp, clientIp, email
| filter message like "Failed login"
| stats count() as attempts by clientIp
| sort attempts desc
```

---

## 10. Technical Risk Mitigation (Production Readiness)

### Risk 1: DB Write Bottleneck

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

### Risk 2: Read/Write Separation

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

### Risk 3: EF Core Query Performance

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

### Risk Summary & Monitoring Triggers

| Risk | MVP Mitigation | Scale Trigger | Scale Action |
|---|---|---|---|
| Write bottleneck | Batch writes, optimistic concurrency, async audit logs | RDS IOPS > 70% hoặc write latency > 50ms | Tăng IOPS → RDS Proxy → Vertical partition |
| Read/Write contention | Dual DbContext pattern (same conn MVP) | RDS CPU > 60% sustained | Bật RDS Read Replica, đổi ReadOnly conn string |
| EF Core slow queries | Hybrid EF+Dapper, AsSplitQuery, slow query logging | Query > 500ms p95 | Optimize với Dapper, add indexes, query hints |


---

### Risk 4: SignalR Connection Scalability (Unit 2+)

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

### Risk 5: Redis Memory Exhaustion

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

### Risk 6: S3 Cost Explosion (Photo Storage)

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

### Risk 7: JWT Secret Rotation Downtime

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

### Risk 8: Agora.io Free Tier Limits (Unit 2)

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

### Risk 9: APPI Data Breach Response Plan

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

### Risk Summary Table (All 9 Risks)

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

**Implementation priority**: Tất cả 9 risks đều có mitigation ở MVP level — không có "implement sau". Cost: thêm ~2-3 ngày development time cho safeguards.
