# Deployment Architecture — Unit 2: Livestream Engine

**Unit**: Unit 2 — Livestream Engine  
**Ngày tạo**: 2026-03-22  
**Kế thừa**: Deployment flow từ Unit 1 vẫn áp dụng — document này chỉ ghi các additions và changes.

---

## 1. Production Architecture — Unit 2 View

```
Internet
    │
    │ HTTPS + WSS (TLS 1.3)
    ▼
┌─────────────────────────────────────────────────────────────┐
│                  AWS CloudFront (CDN)                       │
│  - Static assets (PWA build)                                │
│  - S3 profile photos (/profiles/*)                          │
│  - NOTE: WebSocket (/hubs/*) KHÔNG qua CloudFront           │
│    → Direct to ALB (CloudFront không support WebSocket)     │
└──────────────┬──────────────────────────────────────────────┘
               │ API requests (/api/*)
               │ WebSocket (/hubs/*) → direct to ALB
               ▼
┌─────────────────────────────────────────────────────────────┐
│           Application Load Balancer (ALB)                   │
│  - HTTPS:443 + WebSocket upgrade support                    │
│  - Sticky sessions: lb_cookie (1 day) for /hubs/*           │
│  - Health check: GET /health/ready                          │
└──────────────┬──────────────────────────────────────────────┘
               │
┌──────────────▼──────────────────────────────────────────────┐
│              ECS Fargate Cluster                            │
│                                                             │
│  ┌──────────────────────┐  ┌──────────────────────┐        │
│  │     API Task #1      │  │     API Task #2      │        │
│  │  1vCPU / 2GB RAM     │  │  1vCPU / 2GB RAM     │        │
│  │                      │  │                      │        │
│  │  REST Controllers    │  │  REST Controllers    │        │
│  │  LivestreamHub       │  │  LivestreamHub       │        │
│  │  ChatHub             │  │  ChatHub             │        │
│  │  Hangfire Workers    │  │  Hangfire Workers    │        │
│  │  IHostedServices     │  │  IHostedServices     │        │
│  └──────────┬───────────┘  └──────────┬───────────┘        │
│             │                         │                     │
│             └────────────┬────────────┘                     │
│                          │ SignalR Redis Backplane           │
│  Auto Scaling:           │ (cross-task broadcast)           │
│  CPU > 70% OR            │                                  │
│  Connections > 4,000     │                                  │
│  → scale out (+2 tasks)  │                                  │
└──────────────────────────┼──────────────────────────────────┘
                           │
        ┌──────────────────┼──────────────────────┐
        │                  │                      │
        ▼                  ▼                      ▼
┌──────────────┐  ┌────────────────┐  ┌──────────────────────┐
│  RDS         │  │  ElastiCache   │  │  AWS Services        │
│  PostgreSQL  │  │  Redis 7       │  │                      │
│  db.t3.small │  │ cache.t3.micro │  │  S3:                 │
│  Multi-AZ    │  │                │  │  ├── profiles/       │
│              │  │  Unit 1 keys   │  │  └── chat-archive/   │
│  Unit 1 tbls │  │  viewer_count  │  │                      │
│  + Unit 2:   │  │  room:*:chat   │  │  Agora RTC (ext)     │
│  rooms       │  │  call_session  │  │  ├── Token gen       │
│  viewer_sess │  │  feature_flag  │  │  └── Channel mgmt    │
│  call_reqs   │  │  agora_quota   │  │                      │
│  call_sess   │  │  SignalR bkpl  │  │  CloudWatch:         │
│  billing_tks │  └────────────────┘  │  ├── Custom metrics  │
│  direct_msgs │                      │  └── Alarms          │
│  (partitioned│                      │                      │
│  by sent_at) │                      │  Secrets Manager:    │
│  convs       │                      │  └── agora-creds     │
│  blocks      │                      └──────────────────────┘
│  gifts       │
└──────────────┘
```

---

## 2. SignalR WebSocket Flow

```
Client (PWA)
    │
    │ 1. GET /hubs/livestream?roomId={id}&access_token={jwt}
    │    (WebSocket upgrade request)
    │
    ▼
ALB (sticky session cookie set)
    │
    │ 2. Route to Task #1 (sticky — same task for lifetime of connection)
    │    HTTP 101 Switching Protocols
    │
    ▼
ECS Task #1 — LivestreamHub
    │
    │ 3. OnConnectedAsync:
    │    - Validate JWT
    │    - Groups.AddToGroupAsync("room:{roomId}")
    │    - Groups.AddToGroupAsync("direct:{userId}")
    │    - ConnectionTracker.Add(connectionId, userId, roomId)
    │
    │ 4. Broadcast events:
    │    - Task #1 → Redis backplane → Task #2 → clients on Task #2
    │    (cross-task broadcast via Redis pub/sub)
    │
    │ 5. OnDisconnectedAsync:
    │    - Groups.RemoveFromGroupAsync(...)
    │    - ConnectionTracker.Remove(connectionId)
    │    - DECR viewer_count:{roomId}
    │    - UPDATE viewer_sessions SET left_at = now
```

**WebSocket vs Long Polling**: SignalR tự động negotiate. Production: WebSocket. Fallback: Long Polling (nếu WebSocket blocked bởi corporate firewall).

---

## 3. Private Call Flow — Infrastructure View

```
Viewer (PWA)                    API (ECS)                    Host (PWA)
    │                               │                             │
    │── POST /calls/request ────────▶                             │
    │                               │── SignalR: CallRequest ────▶│
    │                               │   (direct:{hostId} group)   │
    │                               │                             │
    │                               │◀── POST /calls/{id}/accept ─│
    │                               │                             │
    │                               │── Generate Agora tokens     │
    │                               │   (server-side, 4h TTL)     │
    │                               │                             │
    │◀── SignalR: CallAccepted ──────│── SignalR: CallAccepted ───▶│
    │    { agoraToken, channelName } │   { agoraToken, channelName}│
    │                               │                             │
    │                               │── Schedule billing ticks    │
    │                               │   (Hangfire, mỗi 10s)       │
    │                               │                             │
    │◀══ Agora RTC (P2P video) ══════════════════════════════════▶│
    │    (direct connection via Agora infrastructure)             │
    │                               │                             │
    │                               │── ProcessBillingTick ───────│
    │◀── SignalR: BalanceUpdated ────│                             │
    │                               │                             │
    │── POST /calls/{id}/end ────────▶                             │
    │                               │── Agora: kick channel       │
    │                               │── Redis: DEL call_session   │
    │                               │── Hangfire: cancel ticks    │
    │◀── SignalR: CallEnded ─────────│── SignalR: CallEnded ──────▶│
```

---

## 4. Billing Tick — Infrastructure View

```
Hangfire Scheduler (in ECS Task)
    │
    │ Every 10 seconds per active call session
    ▼
ProcessBillingTickJob(sessionId, tickNumber)
    │
    ├── Redis GET call_session:{sessionId}
    │       └── MISS → session ended → skip
    │
    ├── PostgreSQL: SELECT balance FROM coin_wallets WHERE user_id = viewerId
    │       └── balance < cost_per_tick → EndCall
    │
    ├── PostgreSQL Transaction:
    │       INSERT INTO billing_ticks (session_id, tick_number, ...)
    │       ON CONFLICT (session_id, tick_number) DO NOTHING
    │       → IF rows_affected = 1:
    │           UPDATE coin_wallets SET balance -= cost_per_tick
    │       COMMIT
    │
    ├── SignalR: Broadcast BalanceUpdated to direct:{viewerId}
    │
    └── IF balance < 100: Broadcast LowBalanceWarning

Retry policy (Hangfire):
    Attempt 1 → fail → wait 1s
    Attempt 2 → fail → wait 2s
    Attempt 3 → fail → wait 4s
    All fail → EndCall(reason=BillingError)
```

---

## 5. Chat Export — Infrastructure View

```
Hangfire Scheduler
    │
    │ Daily 02:00 UTC (11:00 JST)
    ▼
ExportRoomChatToS3Job
    │
    ├── Redis SCAN room:*:chat (find all active streams)
    │
    └── For each room:
            │
            ├── Redis XRANGE room:{roomId}:chat - +
            │       (read all messages, up to ~1000)
            │
            ├── Transform to JSON Lines
            │
            ├── S3 PutObject:
            │       Bucket: livestream-app-profiles-{account}
            │       Key: chat-archive/2026/03/{roomId}/2026-03-22.jsonl
            │       ContentType: application/x-ndjson
            │
            ├── SUCCESS → Log "Exported {count} messages"
            │
            └── FAIL (3 retries) → Hangfire Dead Letter Queue
                    → Admin retry từ Hangfire Dashboard (/hangfire)
```

---

## 6. Partition Maintenance — Infrastructure View

```
ECS Task Startup (IHostedService)
    │
    └── PartitionMaintenanceService.StartAsync()
            │
            ├── EnsurePartitionExists("direct_messages", 2026-03)
            ├── EnsurePartitionExists("direct_messages", 2026-04)
            └── EnsurePartitionExists("direct_messages", 2026-05)
            
            SQL: CREATE TABLE IF NOT EXISTS direct_messages_2026_03
                 PARTITION OF direct_messages
                 FOR VALUES FROM ('2026-03-01') TO ('2026-04-01')

Hangfire Job (ngày 25 hàng tháng, 00:00 UTC)
    │
    └── CreateNextMonthPartitionJob
            │
            └── EnsurePartitionExists("direct_messages", next_month)
```

---

## 7. Deployment Flow — Unit 2 Changes

Kế thừa rolling update từ Unit 1. Thêm considerations:

```
Unit 2 Deployment Checklist:
    │
    ├── Pre-deploy:
    │   ├── [ ] Agora credentials đã có trong Secrets Manager
    │   ├── [ ] ALB sticky sessions đã enable
    │   └── [ ] CloudWatch custom metric alarms đã tạo
    │
    ├── Deploy (same rolling update process):
    │   ├── EF Core migrations: Tạo 8 tables mới
    │   ├── PartitionMaintenanceService: Tạo partitions khi startup
    │   └── Hangfire: Register new jobs (billing, export, partition, quota)
    │
    └── Post-deploy verification:
        ├── [ ] /health/ready returns 200
        ├── [ ] SignalR hub accessible: wss://api.livestream-app.jp/hubs/livestream
        ├── [ ] Hangfire dashboard: /hangfire → new jobs visible
        ├── [ ] CloudWatch: custom metrics appearing
        └── [ ] Redis: viewer_count keys created on first room join
```

**Migration safety** (Unit 2):
- Tất cả 8 tables mới → không có breaking changes với Unit 1 tables
- `direct_messages` partition table: `CREATE TABLE IF NOT EXISTS` → idempotent
- Rollback: Redeploy Unit 1 image → Unit 2 tables tồn tại nhưng không được dùng (safe)

---

## 8. Staging Environment (Unit 2)

```
Staging additions (same as production, scaled down):
  - Agora: Dùng cùng free tier account (staging + prod share 10K min/month)
    → Staging: Limit to 1,000 min/month via feature flag
  - Redis Streams: TTL 1 ngày (thay vì 7 ngày) để tiết kiệm memory
  - Partition: Tạo cho current month only (không tạo 2 tháng tới)
  - Billing ticks: Dùng test coin values (không phải real money)
```

---

## 9. Local Development (Unit 2)

```
docker-compose up (existing + additions)
    │
    ├── api (port 5000) — thêm:
    │   ├── SignalR hubs: ws://localhost:5000/hubs/livestream
    │   │                 ws://localhost:5000/hubs/chat
    │   ├── Agora: MockMode=true (fake tokens)
    │   ├── Redis Streams: redis:6379 (existing container)
    │   └── Partitions: tạo tự động khi startup
    │
    ├── postgres (port 5432) — thêm:
    │   └── 8 tables mới + partitions (via EF Core migrations)
    │
    ├── redis (port 6379) — thêm:
    │   └── Redis Streams support (Redis 7 đã có sẵn)
    │
    └── localstack (port 4566) — thêm:
        └── S3: chat-archive/ prefix (tự tạo khi PutObject)

Không cần thêm container mới cho Unit 2.
```

**Test SignalR locally**:
```javascript
// Browser console hoặc test client
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/hubs/livestream?roomId=test-room-id", {
        accessTokenFactory: () => "your-jwt-token"
    })
    .build();

await connection.start();
connection.on("ViewerCountUpdated", (count) => console.log("Viewers:", count));
await connection.invoke("JoinRoom", "test-room-id");
```
