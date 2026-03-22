# NFR Requirements — Unit 2: Livestream Engine

**Ngày tạo**: 2026-03-22  
**Unit**: Unit 2 — Livestream Engine  
**Kế thừa từ**: Unit 1 NFR Requirements (tất cả baseline NFRs vẫn áp dụng)

---

## 1. Performance Requirements

### 1.1 SignalR Real-time Messaging

| Requirement | Target | Measurement |
|---|---|---|
| Room chat message latency | < 500ms p95 | Server send → client receive |
| DirectChat message latency | < 500ms p95 | Server send → recipient receive |
| SignalR connection establishment | < 2s p95 | Client connect → hub connected |
| Viewer count update broadcast | < 1s p95 | INCR/DECR → all clients updated |

**Rationale**: 500ms p95 là achievable với SignalR + Redis backplane trong cùng VPC. Consistent với Unit 1 API latency target.

### 1.2 Agora Video Call

| Requirement | Target | Measurement |
|---|---|---|
| Private call connection time | < 5s p95 | Request accepted → video connected |
| Agora token generation | < 200ms p95 | API call → token returned |
| Token TTL | 4 giờ | Giảm refresh frequency, ít failure points |

### 1.3 Billing System

| Requirement | Target | Measurement |
|---|---|---|
| Billing tick execution | 10s ± 5s | Scheduled time → coin deducted |
| Coin balance update | < 500ms p95 | Deduction → balance reflected in UI |
| Billing tick idempotency | 100% | Zero duplicate charges |

### 1.4 Database Performance

| Requirement | Target | Measurement |
|---|---|---|
| DirectMessage query (with partition key) | < 100ms p95 | Query with SentAt filter |
| Room chat Redis XADD | < 10ms p95 | Message append to stream |
| Viewer count Redis INCR/DECR | < 5ms p95 | Atomic counter operation |

---

## 2. Scalability Requirements

### 2.1 Concurrent Connections

| Requirement | Value | Notes |
|---|---|---|
| Max viewers per room | 1,000 | Business rule BR-LS-06 |
| Max SignalR connections per ECS task | 5,000 | ~100MB memory at 20KB/connection |
| ECS task scale-out trigger | 80% of 5,000 (4,000 connections) | Custom CloudWatch metric |
| Min ECS tasks | 2 | High availability |
| Max ECS tasks | 10 | Cost control |

### 2.2 Redis Streams

| Requirement | Value | Notes |
|---|---|---|
| Max messages per room stream | ~1,000 (MAXLEN ~ 1000) | Approximate trimming |
| Stream TTL | 7 ngày | S3 export backup |
| Memory per room stream | ~200KB | 1,000 × 200 bytes |

### 2.3 PostgreSQL Partitioning (DirectChat)

| Requirement | Value | Notes |
|---|---|---|
| Partition strategy | Monthly range on `SentAt` | `direct_messages_YYYY_MM` |
| Partition creation | Startup check + Hangfire job ngày 25 | Self-healing |
| Query safeguard | Throw exception nếu không có SentAt filter | Fail-fast pattern |
| Partition retention | Indefinite (archive to S3 future) | MVP: keep all |

---

## 3. Reliability Requirements

### 3.1 Uptime (kế thừa từ Unit 1)

| Requirement | Target |
|---|---|
| API uptime | 99.9% (8.7 giờ downtime/năm) |
| SignalR hub uptime | 99.9% |
| Billing system uptime | 99.9% |

### 3.2 Billing Integrity

| Requirement | Behavior |
|---|---|
| Billing tick failure | Retry 3× (1s, 2s, 4s) → End call nếu vẫn fail |
| Duplicate tick prevention | `UNIQUE(call_session_id, tick_number)` DB constraint |
| Partial tick on call end | Không charge tick cuối nếu end do BillingError |
| Coin deduction atomicity | DB transaction: insert billing_tick + deduct coin |

### 3.3 Agora Reliability

| Requirement | Behavior |
|---|---|
| Token TTL | 4 giờ (giảm refresh frequency) |
| Quota monitoring | Alert tại 80%, disable private call tại 90% (9,000 phút) |
| Quota disable scope | Private call trước, public stream sau |
| Token revocation | Server kick all users khi stream/call end |

### 3.4 Chat Data Reliability

| Requirement | Behavior |
|---|---|
| Redis Stream backup | Hangfire job export to S3 hàng ngày |
| S3 export failure | Dead letter queue (Hangfire failed jobs) → manual retry |
| Redis restart recovery | Lazy resync viewer count từ DB khi query |
| Chat history on join | Không load history (BR-RC-04) |

---

## 4. Security Requirements (Unit 2 specific)

### 4.1 SignalR Authorization

| Requirement | Implementation |
|---|---|
| Hub connection auth | JWT validation tại `OnConnectedAsync` |
| Room join authorization | Check: đăng nhập + đủ tuổi + không bị block (BR-LS-03) |
| Private call authorization | Check: Viewer → Host only, không bị block |
| DirectChat authorization | Check: không bị block lẫn nhau |

### 4.2 Rate Limiting (Unit 2 specific)

| Requirement | Limit | Scope |
|---|---|---|
| Room chat messages | 3 messages/giây | Per user per room |
| DirectChat messages | 10 messages/phút | Per user per conversation |
| Private call requests | 1 pending request | Per Viewer (BR-PC-01) |
| Stream start | 1 active stream | Per Host (BR-LS-02) |

### 4.3 Agora Security

| Requirement | Implementation |
|---|---|
| Token scope | Channel-specific (1 token = 1 channel) |
| Token generation | Server-side only, không expose Agora App Secret |
| Call recording | Disabled (privacy) |
| Token revocation | Agora API kick on stream/call end |

---

## 5. Infrastructure Requirements

### 5.1 Redis Configuration

| Requirement | Value | Notes |
|---|---|---|
| Redis instance | Shared (cache + SignalR + Streams) | MVP — 1 ElastiCache node |
| Eviction policy | `allkeys-lru` | Cache và Streams share policy |
| SignalR backplane | `AddStackExchangeRedis` | Cùng connection string |
| Redis Streams key pattern | `room:{roomId}:chat` | TTL 7 ngày |
| Viewer count key pattern | `viewer_count:{roomId}` | TTL 1 giờ |
| Upgrade trigger | Redis memory > 70% hoặc SignalR latency > 100ms | Tách instance |

### 5.2 ECS Fargate (Unit 2 additions)

| Requirement | Value | Notes |
|---|---|---|
| Task size | 1 vCPU / 2GB RAM | Đủ cho 5,000 SignalR connections |
| Custom CloudWatch metric | `LivestreamApp/SignalR/ConnectionCount` | Publish mỗi 30 giây |
| Scale-out trigger | ConnectionCount > 4,000 | 80% of 5,000 |
| Scale-in trigger | ConnectionCount < 2,000 | |

### 5.3 Hangfire Jobs (Unit 2)

| Job | Schedule | Queue | Retry |
|---|---|---|---|
| `ProcessBillingTick` | Mỗi 10 giây (per active session) | `billing` (high priority) | 3× exponential |
| `ExportRoomChatToS3` | Hàng ngày 02:00 UTC | `default` | 3× → Dead Letter |
| `CreateNextMonthPartition` | Ngày 25 hàng tháng 00:00 UTC | `maintenance` | 3× |
| `CheckAgoraQuota` | Hàng ngày 00:00 UTC | `default` | 3× |

---

## 6. Observability Requirements (Unit 2 specific)

### 6.1 Custom Metrics

| Metric | Namespace | Unit | Alert Threshold |
|---|---|---|---|
| `SignalR.ConnectionCount` | `LivestreamApp/SignalR` | Count | > 4,000/task |
| `SignalR.MessageLatency` | `LivestreamApp/SignalR` | Milliseconds | > 500ms p95 |
| `Billing.TickLatency` | `LivestreamApp/Billing` | Seconds | > 30s |
| `Billing.FailedTicks` | `LivestreamApp/Billing` | Count | > 0 |
| `Agora.UsageMinutes` | `LivestreamApp/Agora` | Count | > 8,000 (alert), > 9,000 (disable) |
| `Redis.StreamLength` | `LivestreamApp/Redis` | Count | > 900/room |

### 6.2 Structured Logging (kế thừa Serilog từ Unit 1)

| Event | Log Level | Fields |
|---|---|---|
| Stream started/ended | Information | `roomId`, `hostId`, `duration` |
| Private call started/ended | Information | `sessionId`, `hostId`, `viewerId`, `totalCoins` |
| Billing tick processed | Debug | `sessionId`, `tickNumber`, `coinsCharged` |
| Billing tick failed | Error | `sessionId`, `tickNumber`, `error`, `retryCount` |
| User banned from room | Warning | `roomId`, `userId`, `bannedBy` |
| Agora quota threshold | Warning | `usageMinutes`, `threshold` |
| Partition created | Information | `tableName`, `partitionName` |

---

## 7. Compliance Requirements

### 7.1 Financial Compliance (APPI + Business)

| Requirement | Implementation |
|---|---|
| Coin transaction audit trail | Mọi deduction/refund có `billing_ticks` record |
| Transaction immutability | Không update/delete billing records |
| Balance consistency | DB transaction atomic: deduct + record |
| Refund policy | Không refund partial ticks (trừ BillingError) |

### 7.2 Privacy (APPI)

| Requirement | Implementation |
|---|---|
| Chat data retention | Redis 7 ngày + S3 archive |
| DirectMessage visibility | Ẩn cả 2 phía khi block (BR-DC-03) |
| Call recording | Disabled |
| User data on account delete | Cascade delete conversations + messages |

---

## 8. NFR Acceptance Criteria

| NFR | Acceptance Test |
|---|---|
| SignalR latency < 500ms p95 | Load test: 1,000 concurrent viewers, measure message round-trip |
| Billing idempotency | Simulate duplicate Hangfire execution, verify 0 duplicate charges |
| Partition safeguard | Unit test: query without SentAt filter throws exception |
| Agora quota disable | Integration test: mock 9,000 minutes → verify private call disabled |
| Viewer count accuracy | Load test: 1,000 concurrent join/leave, verify final count = 0 |
| Redis MAXLEN | Integration test: send 1,100 messages, verify stream length ≤ 1,100 |
