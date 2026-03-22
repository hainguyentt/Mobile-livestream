# Infrastructure Design — Unit 2: Livestream Engine

**Unit**: Unit 2 — Livestream Engine  
**Cloud Provider**: AWS (ap-northeast-1 — Tokyo)  
**Ngày tạo**: 2026-03-22  
**Kế thừa**: Toàn bộ infrastructure từ Unit 1 vẫn dùng chung — document này chỉ ghi các additions và changes.

---

## 1. Infrastructure Services Map — Unit 2 Additions

| Logical Component | AWS Service | Tier | Config | Thay đổi so với Unit 1 |
|---|---|---|---|---|
| API Application | ECS Fargate | Compute | 1 vCPU / 2GB RAM, min 2 tasks | **Thêm**: SignalR WebSocket support, custom CloudWatch metrics |
| PostgreSQL | RDS PostgreSQL 16 | Data | db.t3.small, Multi-AZ | **Thêm**: 8 tables mới, monthly partitions cho `direct_messages` |
| Redis | ElastiCache Redis 7 | Cache | cache.t3.micro | **Thêm**: Redis Streams, feature flags, Agora quota keys |
| Object Storage | S3 | Storage | Standard class | **Thêm**: `chat-archive/` prefix cho chat export |
| Agora RTC | External SaaS | Video | Free tier 10K min/month | **Mới** — external service |
| CloudWatch | CloudWatch Metrics | Observability | Custom namespace | **Thêm**: 6 custom metrics cho SignalR + Billing + Agora |
| ALB | ALB | Networking | HTTPS + WebSocket | **Thêm**: WebSocket upgrade support, sticky sessions |

**Không thay đổi**: VPC, subnets, security groups, SES, Secrets Manager, ECR, CloudFront, IAM baseline.

---

## 2. Compute — ECS Fargate (Unit 2 Changes)

### 2.1 ALB — WebSocket Support

SignalR yêu cầu WebSocket upgrade. ALB hỗ trợ WebSocket natively — chỉ cần đảm bảo:

```
ALB Target Group (thêm vào Unit 1 config):
  Stickiness: Enabled (duration: 1 day)
  Stickiness type: lb_cookie
  
  Lý do: SignalR connection phải stick đến cùng 1 ECS task
  trong suốt lifetime của connection. Redis backplane đảm bảo
  broadcast cross-task, nhưng connection state phải ở 1 task.
```

**ALB Listener Rule thêm**:
```
Rule: WebSocket upgrade
  Condition: Path is /hubs/*
  Action: Forward to ECS target group (same group, stickiness enabled)
  
  Note: ALB tự động handle WebSocket upgrade (101 Switching Protocols)
  Không cần config thêm — chỉ cần stickiness.
```

### 2.2 ECS Task Definition — Unit 2 Additions

Thêm secrets và environment variables vào task definition hiện có:

```json
{
  "secrets": [
    { "name": "Agora__AppId",
      "valueFrom": "arn:aws:secretsmanager:...:livestream/agora-credentials" },
    { "name": "Agora__AppCertificate",
      "valueFrom": "arn:aws:secretsmanager:...:livestream/agora-credentials" }
  ],
  "environment": [
    { "name": "SignalR__MaxConnections", "value": "5000" },
    { "name": "Hangfire__BillingQueueWorkers", "value": "4" }
  ]
}
```

### 2.3 ECS Auto Scaling — Unit 2 Changes

Unit 1 dùng CPU-based scaling. Unit 2 thêm **connection-based scaling**:

```
Auto Scaling Policies (2 policies, OR logic):

Policy 1 (kế thừa Unit 1):
  Metric: ECSService/CPUUtilization
  Scale out: > 70% for 2 min → +2 tasks
  Scale in:  < 30% for 5 min → -1 task

Policy 2 (Unit 2 mới):
  Metric: LivestreamApp/SignalR/ConnectionCount (custom)
  Scale out: > 4,000 connections (avg across tasks) → +2 tasks
  Scale in:  < 2,000 connections → -1 task
  Cooldown: 120s (scale out), 300s (scale in)

Min tasks: 2 | Max tasks: 10
```

**Custom metric publishing** (MetricsPublisherService, mỗi 30 giây):
```csharp
await _cloudWatch.PutMetricDataAsync(new PutMetricDataRequest {
    Namespace = "LivestreamApp/SignalR",
    MetricData = [
        new MetricDatum {
            MetricName = "ConnectionCount",
            Value = _connectionTracker.GetConnectionCount(),
            Unit = StandardUnit.Count,
            Dimensions = [new Dimension { Name = "Service", Value = "API" }]
        }
    ]
});
```

---

## 3. Database — RDS PostgreSQL (Unit 2 Additions)

### 3.1 New Tables

8 tables mới trong Unit 2 (cùng RDS instance, cùng `livestream` database):

```sql
-- Rooms
rooms                    -- Livestream room metadata
viewer_sessions          -- Viewer join/leave tracking

-- Private Call
call_requests            -- Pending call requests (TTL via Hangfire cleanup)
call_sessions            -- Active/ended call sessions
billing_ticks            -- Coin deduction records (idempotency key)

-- DirectChat
conversations            -- 1-1 conversation threads
direct_messages          -- Messages (PARTITIONED BY RANGE sent_at)
blocks                   -- Block relationships

-- Gifts
gifts                    -- Gift catalog + transaction history
```

### 3.2 PostgreSQL Partitioning

`direct_messages` table dùng range partitioning theo `sent_at`:

```sql
CREATE TABLE direct_messages (
    id          UUID NOT NULL,
    conversation_id UUID NOT NULL,
    sender_id   UUID NOT NULL,
    content     TEXT NOT NULL,
    sent_at     TIMESTAMPTZ NOT NULL,
    is_read     BOOLEAN NOT NULL DEFAULT false,
    PRIMARY KEY (id, sent_at)  -- partition key phải trong PK
) PARTITION BY RANGE (sent_at);

-- Partitions tạo tự động bởi PartitionMaintenanceService
-- Naming: direct_messages_2026_03, direct_messages_2026_04, ...
```

**Partition maintenance** (IHostedService + Hangfire):
```
Startup: Tạo partitions cho current month + 2 tháng tới
Monthly job (ngày 25): Tạo partition tháng tới
```

### 3.3 RDS Parameter Group Updates

Thêm vào parameter group hiện có:

```
max_connections: 100 → 150
  Lý do: Unit 2 thêm Hangfire billing jobs (nhiều concurrent DB connections)

work_mem: 4MB → 8MB
  Lý do: Partition queries cần thêm memory cho sort operations

enable_partition_pruning: on (default, confirm enabled)
  Lý do: Đảm bảo partition pruning hoạt động cho direct_messages queries
```

### 3.4 Memory Estimation — Unit 2 DB

| Table | Estimated rows (6 tháng) | Size estimate |
|---|---|---|
| `rooms` | 10,000 | ~5MB |
| `viewer_sessions` | 500,000 | ~50MB |
| `call_sessions` | 50,000 | ~10MB |
| `billing_ticks` | 2,000,000 | ~200MB |
| `direct_messages` | 5,000,000 | ~500MB (partitioned) |
| `conversations` | 100,000 | ~10MB |
| **Total Unit 2** | | **~775MB** |

**RDS storage**: 20GB (Unit 1) → vẫn đủ cho 6 tháng đầu. Auto-scale trigger tại 80% (16GB).

---

## 4. Cache — ElastiCache Redis (Unit 2 Additions)

### 4.1 New Key Patterns

| Key Pattern | Type | TTL | Estimated Count | Size/key | Total |
|---|---|---|---|---|---|
| `viewer_count:{roomId}` | STRING | 1 giờ | ~100 active rooms | 20 bytes | ~2KB |
| `room:{roomId}:chat` | STREAM | 7 ngày | ~100 active rooms | ~200KB | ~20MB |
| `call_session:{sessionId}` | HASH | 4 giờ | ~50 active calls | 500 bytes | ~25KB |
| `feature_flag:{feature}` | STRING | None | ~5 flags | 10 bytes | ~50 bytes |
| `agora_quota:current_month` | STRING | End of month | 1 | 20 bytes | ~20 bytes |
| `room:info:{roomId}` | STRING (JSON) | 60s | ~100 | 1KB | ~100KB |
| SignalR backplane | Internal | Session | N/A | ~1KB/conn | ~5MB (5K conn) |

**Total Unit 2 Redis additions**: ~25MB

**Updated memory estimation**:
| Source | Memory |
|---|---|
| Unit 1 keys | ~1.1MB |
| Unit 2 keys | ~25MB |
| SignalR backplane (5K connections) | ~5MB |
| **Total** | **~31MB** (well within 512MB) |

**Upgrade trigger**: Redis memory > 70% (360MB) → upgrade to `cache.t3.small` (1.37GB).

### 4.2 Redis Streams Config

```
XADD room:{roomId}:chat MAXLEN ~ 1000 * ...
  MAXLEN ~ 1000: Approximate trimming (Redis best practice)
  TTL: EXPIRE room:{roomId}:chat 604800 (7 ngày)
  
Memory per stream: ~1000 entries × 200 bytes = ~200KB
100 active rooms: ~20MB total
```

### 4.3 Eviction Policy

Unit 1 đã set `allkeys-lru`. Unit 2 thêm concern:

```
Vấn đề: Redis Streams với TTL 7 ngày có thể bị evict sớm nếu memory pressure.

Giải pháp: 
  - Giữ allkeys-lru (evict least recently used keys)
  - Active rooms có viewer activity → stream được access thường xuyên → ít bị evict
  - Inactive rooms (không có viewer) → stream ít được access → có thể bị evict
  - Acceptable: Inactive room chat bị evict → S3 export đã backup rồi

Upgrade trigger: Redis memory > 70% → tách SignalR backplane ra instance riêng (TD-U2-01 upgrade path)
```

---

## 5. Storage — S3 (Unit 2 Additions)

### 5.1 New S3 Prefix

Dùng chung bucket `livestream-app-profiles-{account-id}`, thêm prefix mới:

```
Bucket: livestream-app-profiles-{account-id} (existing)
│
├── profiles/           # Unit 1 — user photos
│   └── {userId}/{photoId}.jpg
│
└── chat-archive/       # Unit 2 — room chat export
    └── {year}/{month}/{roomId}/{date}.jsonl
```

### 5.2 IAM Policy Update

Thêm vào ECS Task Role policy hiện có:

```json
{
  "Effect": "Allow",
  "Action": ["s3:PutObject", "s3:GetObject"],
  "Resource": "arn:aws:s3:::livestream-app-profiles-*chat-archive/*"
}
```

### 5.3 S3 Lifecycle Rules (Unit 2)

```
Rule: chat-archive-retention
  Prefix: chat-archive/
  Transition to S3 Glacier: After 90 days
  Expiration: After 365 days (1 năm)
  
  Lý do: Chat archive là compliance data — giữ 1 năm, 
  sau 90 ngày chuyển sang Glacier để tiết kiệm chi phí.
```

---

## 6. Secrets Management (Unit 2 Additions)

| Secret Name | Content | Rotation |
|---|---|---|
| `livestream/agora-credentials` | `{ "AppId": "...", "AppCertificate": "..." }` | Manual (yearly) |

**Tổng secrets sau Unit 2**: 6 secrets (Unit 1: 5 + Unit 2: 1).

---

## 7. Monitoring & Alerting (Unit 2 Additions)

### 7.1 Custom CloudWatch Metrics

| Metric | Namespace | Unit | Alarm Threshold | Action |
|---|---|---|---|---|
| `SignalR/ConnectionCount` | `LivestreamApp/SignalR` | Count | > 4,000/task | ECS scale out |
| `SignalR/MessageLatency` | `LivestreamApp/SignalR` | Milliseconds | > 500ms p95 | SNS alert |
| `Billing/TickLatency` | `LivestreamApp/Billing` | Seconds | > 30s | SNS alert (critical) |
| `Billing/FailedTicks` | `LivestreamApp/Billing` | Count | > 0 | SNS alert (critical) |
| `Agora/UsageMinutes` | `LivestreamApp/Agora` | Count | > 8,000 (warn) / > 9,000 (critical) | SNS alert + auto-disable |
| `Redis/StreamLength` | `LivestreamApp/Redis` | Count | > 900/room | SNS alert |

### 7.2 CloudWatch Alarms (Unit 2)

```
Alarm: SignalR-HighConnections
  Metric: LivestreamApp/SignalR/ConnectionCount
  Threshold: > 4,000 (avg per task, 2 consecutive periods of 1 min)
  Action: ECS Auto Scaling scale-out policy

Alarm: Billing-FailedTick
  Metric: LivestreamApp/Billing/FailedTicks
  Threshold: >= 1 (any failed tick)
  Action: SNS → ops-critical topic → PagerDuty/email

Alarm: Billing-HighLatency
  Metric: LivestreamApp/Billing/TickLatency
  Threshold: > 30s (p95, 5 min window)
  Action: SNS → ops-warning topic

Alarm: Agora-QuotaWarning
  Metric: LivestreamApp/Agora/UsageMinutes
  Threshold: > 8,000
  Action: SNS → ops-warning topic

Alarm: Agora-QuotaCritical
  Metric: LivestreamApp/Agora/UsageMinutes
  Threshold: > 9,000
  Action: SNS → ops-critical topic
  Note: Auto-disable via CheckAgoraQuotaJob (không phải alarm action)
```

### 7.3 CloudWatch Log Insights Queries (Unit 2)

```sql
-- Failed billing ticks
fields @timestamp, sessionId, tickNumber, error
| filter message like "BillingTickFailed"
| sort @timestamp desc
| limit 20

-- SignalR connection errors
fields @timestamp, connectionId, userId, error
| filter message like "SignalR" and level = "Error"
| sort @timestamp desc
| limit 50

-- Agora token generation failures
fields @timestamp, sessionId, userId, error
| filter message like "AgoraToken" and level = "Error"
| sort @timestamp desc

-- Active rooms count over time
fields @timestamp
| filter message like "ViewerJoined"
| stats count() as joins by bin(5m)
```

---

## 8. Cost Estimate — Unit 2 Additions

| Resource | Change | Additional Cost/month |
|---|---|---|
| ECS Fargate | Thêm SignalR connections (same task size) | ~$0 (same 2 tasks) |
| RDS | Thêm 8 tables, partitions | ~$0 (same instance) |
| ElastiCache | Thêm ~25MB Redis data | ~$0 (same instance) |
| S3 | Chat archive ~1GB/tháng | ~$0.02 |
| S3 Glacier | After 90 days transition | ~$0.01 |
| CloudWatch | 6 custom metrics + alarms | ~$3 |
| Agora RTC | Free tier 10K min/month | ~$0 (MVP) |
| Secrets Manager | +1 secret | ~$0.40 |
| **Unit 2 additions** | | **~$3.43/tháng** |
| **Total (Unit 1 + Unit 2)** | | **~$172/tháng** |

---

## 9. Local Development (Unit 2 Additions)

### 9.1 Docker Compose Updates

Thêm vào `docker-compose.yml` hiện có:

```yaml
services:
  api:
    environment:
      # Unit 2 additions
      - Agora__AppId=test_app_id
      - Agora__AppCertificate=test_app_certificate
      - SignalR__MaxConnections=5000
    # Ports đã có: 5000:8080
    # WebSocket: không cần config thêm — Docker expose port 5000 đủ

  # Không cần thêm service mới cho Unit 2
  # Agora: dùng mock trong dev (AgoraTokenService có stub mode)
  # Redis Streams: Redis container hiện có đã support
  # PostgreSQL partitions: tạo tự động khi startup
```

### 9.2 Agora Mock cho Local Dev

```csharp
// appsettings.Development.json
{
  "Agora": {
    "AppId": "test_app_id",
    "AppCertificate": "test_app_certificate",
    "MockMode": true  // Trả về fake token, không gọi Agora SDK thực
  }
}

// IAgoraTokenService implementation
// MockMode = true → return "mock_token_{sessionId}_{userId}"
// MockMode = false → dùng Agora.Rtc.Token SDK thực
```

### 9.3 LocalStack Additions

```bash
# localstack-init/init-unit2.sh
# Tạo S3 prefix cho chat archive (không cần tạo riêng — prefix tự tạo khi PutObject)
# Không cần thêm service mới vào LocalStack
```

---

## 10. IaC Additions (Unit 2)

Thêm vào `infrastructure/` hiện có:

```
infrastructure/
├── ... (Unit 1 files)
├── ecs-autoscaling-signalr.ts   # Custom metric scaling policy
├── cloudwatch-unit2.ts          # Unit 2 alarms + dashboards
└── s3-lifecycle-chat.ts         # Chat archive lifecycle rules
```

**ALB sticky sessions** (update `alb.ts`):
```typescript
// Thêm stickiness vào target group
const targetGroup = new elbv2.ApplicationTargetGroup(this, 'ApiTargetGroup', {
  // ... existing config
  stickinessCookieDuration: Duration.days(1),
  stickinessCookieEnabled: true,
});
```
