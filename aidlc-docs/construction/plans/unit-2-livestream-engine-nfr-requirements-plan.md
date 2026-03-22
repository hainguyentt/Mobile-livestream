# NFR Requirements Plan — Unit 2: Livestream Engine

**Status**: IN PROGRESS  
**Created**: 2026-03-22  
**Unit**: Unit 2 — Livestream Engine

---

## Execution Steps

- [x] Step 1: Phân tích Functional Design artifacts Unit 2
- [x] Step 2: Tạo câu hỏi NFR
- [x] Step 3: Thu thập câu trả lời từ user (recommendations từ tradeoff-analysis.md)
- [x] Step 4: Generate nfr-requirements.md
- [x] Step 5: Generate tech-stack-decisions.md
- [x] Step 6: Present completion message và chờ approval

---

## Context từ Unit 1 (đã quyết định — KHÔNG hỏi lại)

| Quyết định | Giá trị |
|---|---|
| Database | PostgreSQL (RDS), EF Core Code-First |
| Cache | Redis (ElastiCache) |
| Logging | Serilog → CloudWatch (prod) |
| Auth | JWT httpOnly Cookie |
| Rate limiting | Per-IP + Global |
| Security Baseline | 15 rules — tất cả enabled |
| Uptime target | 99.9% |
| Health checks | live + ready + startup |
| i18n | JP/EN, default ja |

---

## Clarifying Questions

### A. SignalR & Real-time Performance

**Q-A1**: SignalR backplane — Unit 1 đã dùng Redis. Unit 2 có nhiều hubs hơn (LivestreamHub + ChatHub). Cần xác nhận cấu hình scale-out:

- A. Dùng chung 1 Redis instance cho cả SignalR backplane + cache + Redis Streams (đơn giản, MVP)
- B. Tách Redis instance riêng cho SignalR backplane (isolation, tránh memory contention)
- C. Dùng Redis Cluster với separate databases (db0=cache, db1=SignalR, db2=Streams)

[Answer]: A

---

**Q-A2**: SignalR connection limit — với 1000 viewers per room, mỗi viewer có 1 SignalR connection. Khi có nhiều rooms đồng thời:

- A. Không cần lo — ECS Fargate auto-scale khi CPU/memory cao (reactive scaling)
- B. Set target: tối đa 5,000 concurrent SignalR connections per ECS task, scale out khi đạt 80%
- C. Set target: tối đa 10,000 concurrent connections per task

[Answer]: B

---

**Q-A3**: Viewer count cache (Redis) — cập nhật mỗi 5 giây theo thiết kế. Khi có 1000 viewers join/leave đồng thời (thundering herd):

- A. Dùng Redis INCR/DECR atomic operations (đơn giản, đủ cho MVP)
- B. Dùng Redis INCR/DECR + Lua script để batch update (tránh race condition)
- C. Dùng Hangfire job đọc ViewerSessions count từ DB mỗi 5 giây (consistent nhưng chậm hơn)

[Answer]: A

---

### B. Agora Performance & Reliability

**Q-B1**: Agora token refresh — token TTL 1 giờ. Nếu Agora service down khi client cần refresh:

- A. Return cached token (nếu chưa hết hạn) — client tự retry
- B. Return error → client hiển thị "Kết nối bị gián đoạn, thử lại"
- C. Extend token TTL lên 4 giờ để giảm tần suất refresh (trade-off: security vs reliability)

[Answer]: C

---

**Q-B2**: Agora Free Tier monitoring — 10,000 phút/tháng. Khi nào disable private call?

- A. Disable khi đạt 90% quota (9,000 phút) — feature flag
- B. Disable khi đạt 80% quota (8,000 phút) — conservative
- C. Không disable tự động — chỉ alert admin, admin quyết định

[Answer]: A

---

### C. Billing Tick Reliability

**Q-C1**: Hangfire billing tick job — nếu job fail (exception, DB timeout) trong khi đang charge coin:

- A. Retry 3 lần với exponential backoff → nếu vẫn fail → end call (safe: không charge thiếu)
- B. Retry 3 lần → nếu fail → skip tick (không charge) → tiếp tục call (user-friendly)
- C. Retry 3 lần → nếu fail → log error + alert → tiếp tục call (manual investigation)

[Answer]: A

---

**Q-C2**: Billing tick idempotency — nếu job chạy 2 lần cho cùng 1 tick (Hangfire duplicate execution):

- A. Dùng `TickNumber` unique constraint trong `billing_ticks` table để prevent duplicate charge
- B. Dùng distributed lock (Redis) trước khi process tick
- C. Cả A và B (defense in depth)

[Answer]: A

---

### D. Redis Streams (RoomChat) Performance

**Q-D1**: Redis Streams MAXLEN — thiết kế đặt 1000 messages per room. Khi room có nhiều chat (>1000 messages):

- A. MAXLEN 1000 với `~` (approximate trimming) — đủ cho MVP, trim không chính xác nhưng nhanh hơn
- B. MAXLEN 1000 với exact trimming — chính xác hơn nhưng chậm hơn một chút
- C. Không giới hạn MAXLEN — chỉ dựa vào TTL 7 ngày để cleanup

[Answer]: A

---

**Q-D2**: ExportRoomChatToS3 job — nếu S3 upload fail:

- A. Retry 3 lần → nếu fail → log error, skip (chat data vẫn trong Redis cho đến khi TTL expire)
- B. Retry 3 lần → nếu fail → alert admin, giữ Redis key thêm 24 giờ rồi retry
- C. Retry 3 lần → nếu fail → dead letter queue (Hangfire failed jobs) để manual retry

[Answer]: C

---

### E. PostgreSQL Partitioning (DirectChat)

**Q-E1**: Partition creation strategy — cần tạo partition trước khi insert data:

- A. Tạo partition tự động khi startup (EF Core migration tạo partition cho tháng hiện tại + 1 tháng tới)
- B. Hangfire job tạo partition vào ngày 25 hàng tháng (cho tháng tới)
- C. Cả A và B (startup tạo nếu thiếu + monthly job tạo trước)

[Answer]: C

---

**Q-E2**: Khi query messages không có `SentAt` range (developer mistake) — cần safeguard:

- A. Middleware/interceptor tự động thêm `SentAt >= now - 30 days` nếu không có filter
- B. Throw exception nếu query không có `SentAt` filter (fail-fast, catch trong dev)
- C. Không cần safeguard — document rõ trong coding standards

[Answer]: B

---

### F. Performance Targets (Unit 2 specific)

**Q-F1**: SignalR message latency target cho room chat:

- A. < 500ms p95 (từ send đến receive) — đã mention trong unit-of-work DoD
- B. < 200ms p95 (aggressive)
- C. < 1000ms p95 (relaxed, MVP)

[Answer]: A

---

**Q-F2**: Private call billing tick — Hangfire job mỗi 10 giây. Acceptable latency cho coin deduction:

- A. Coin deduction phải xảy ra trong vòng 10 giây ± 2 giây (tight)
- B. Coin deduction trong vòng 10 giây ± 5 giây (relaxed, Hangfire default)
- C. Không có SLA cụ thể — best effort

[Answer]: B

---

**Q-F3**: Direct message delivery latency (SignalR):

- A. < 500ms p95 (từ send đến recipient nhận)
- B. < 1000ms p95
- C. Không có SLA — best effort

[Answer]: A

---
