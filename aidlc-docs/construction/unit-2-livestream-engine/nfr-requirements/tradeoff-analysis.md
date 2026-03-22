# NFR Requirements — Trade-off Analysis — Unit 2: Livestream Engine

**Ngày tạo**: 2026-03-22  
**Mục đích**: Phân tích trade-off cho tất cả NFR architectural decisions trong Unit 2  
**Recommendation Priority**: Reliability > Performance > Scalability > Simplicity

---

## Evaluation Criteria

| Tiêu chí | Trọng số | Mô tả |
|---|---|---|
| **Reliability** | ⭐⭐⭐⭐⭐ | Hệ thống hoạt động đúng, không mất dữ liệu, không charge sai |
| **Performance** | ⭐⭐⭐⭐⭐ | Latency thấp, throughput cao cho real-time features |
| **Scalability** | ⭐⭐⭐⭐ | Scale tốt khi tăng users và rooms |
| **Simplicity** | ⭐⭐⭐ | Đơn giản, ít moving parts, dễ debug |
| **Cost** | ⭐⭐⭐ | Chi phí infrastructure hợp lý cho MVP |

**Scoring**: 1-5 stars, 5 = best

---

## Question A1: SignalR Backplane — Redis Configuration

**Context**: Unit 2 có 2 hubs (LivestreamHub + ChatHub) với potentially thousands of concurrent connections. Redis đã dùng cho cache và sẽ dùng cho Redis Streams. Cần quyết định cách tổ chức Redis instances.

### Option A: Shared Redis Instance (Cache + SignalR + Streams)

**Structure**:
```
Redis (single instance)
├── db0: Cache (user profiles, viewer counts, rate limits)
├── db0: SignalR backplane (default — không tách database)
└── db0: Redis Streams (room:{roomId}:chat)
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐ | Single point of failure — Redis down = mất cả 3 functions |
| Performance | ⭐⭐⭐ | Memory contention giữa cache, backplane, streams |
| Scalability | ⭐⭐ | Khó scale riêng từng workload |
| Simplicity | ⭐⭐⭐⭐⭐ | 1 connection string, 1 instance để manage |
| Cost | ⭐⭐⭐⭐⭐ | Rẻ nhất — 1 ElastiCache node |

**Ưu điểm**:
- Đơn giản nhất — 1 Redis instance, 1 config
- Chi phí thấp nhất cho MVP
- Không cần manage multiple connections

**Nhược điểm**:
- Memory contention: Streams TTL 7 ngày có thể chiếm nhiều memory
- SignalR backplane và cache cùng eviction policy → conflict
- Redis down = mất SignalR + cache + chat history cùng lúc

---

### Option B: Tách Redis Instance Riêng cho SignalR Backplane

**Structure**:
```
Redis Instance 1 (cache + streams):
├── Cache: user profiles, viewer counts, rate limits
└── Streams: room:{roomId}:chat (TTL 7 ngày)

Redis Instance 2 (SignalR backplane):
└── SignalR: hub connections, group memberships
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐ | SignalR isolated — cache failure không ảnh hưởng real-time |
| Performance | ⭐⭐⭐⭐⭐ | No memory contention, dedicated throughput |
| Scalability | ⭐⭐⭐⭐⭐ | Scale SignalR backplane độc lập |
| Simplicity | ⭐⭐⭐ | 2 connection strings, 2 instances |
| Cost | ⭐⭐⭐ | 2 ElastiCache nodes (~$30-50/tháng thêm) |

**Ưu điểm**:
- SignalR backplane isolated — không bị ảnh hưởng bởi cache eviction
- Có thể tune memory policy riêng (backplane: noeviction, cache: allkeys-lru)
- Scale SignalR tier độc lập khi cần

**Nhược điểm**:
- 2 Redis instances để manage
- Chi phí cao hơn Option A

---

### Option C: Redis Cluster với Separate Databases

**Structure**:
```
Redis (single instance, multiple databases)
├── db0: Cache
├── db1: SignalR backplane
└── db2: Redis Streams
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐ | Vẫn single instance — db separation không giúp reliability |
| Performance | ⭐⭐⭐ | Logical separation nhưng vẫn share memory/CPU |
| Scalability | ⭐⭐ | Redis Cluster không support multiple databases |
| Simplicity | ⭐⭐⭐ | Phức tạp hơn Option A, ít benefit hơn Option B |
| Cost | ⭐⭐⭐⭐⭐ | Rẻ như Option A |

**Ưu điểm**:
- Logical separation giúp debug
- 1 instance, chi phí thấp

**Nhược điểm**:
- Redis Cluster (production) không support SELECT (multiple databases)
- Vẫn share memory — không giải quyết contention
- False sense of isolation

---

### 🏆 Recommendation: **Option A — Shared Redis (MVP)**

**Lý do**:
- **MVP scope**: 10K-100K users năm đầu — 1 Redis instance đủ capacity
- **Cost**: Tiết kiệm ~$30-50/tháng so với Option B
- **Simplicity**: Ít moving parts hơn, dễ debug
- **Migration path**: Khi cần scale, tách ra Option B dễ dàng (chỉ thay connection string)
- **Risk mitigation**: Dùng Redis Sentinel (Multi-AZ ElastiCache) để giảm single-point-of-failure

**Implementation note**:
```csharp
// Dùng chung IConnectionMultiplexer
builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = config["Redis:ConnectionString"];
});
builder.Services.AddSignalR().AddStackExchangeRedis(config["Redis:ConnectionString"]);
// Redis Streams cũng dùng cùng connection
```

**Upgrade trigger**: Khi Redis memory > 70% hoặc SignalR latency > 100ms → tách Option B.

---

## Question A2: SignalR Connection Limit per ECS Task

**Context**: 1000 viewers per room × N rooms đồng thời = potentially tens of thousands of concurrent SignalR connections. Cần xác định scaling strategy.

### Option A: Reactive Scaling (ECS Auto-Scale on CPU/Memory)

**Structure**:
```
ECS Auto-Scaling Policy:
- Scale out khi CPU > 70% hoặc Memory > 80%
- Scale in khi CPU < 30%
- Min tasks: 2, Max tasks: 10
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐ | Scale lag — có thể bị overload trước khi scale out |
| Performance | ⭐⭐⭐ | Reactive — không proactive |
| Scalability | ⭐⭐⭐⭐ | Auto-scale hoạt động nhưng có delay |
| Simplicity | ⭐⭐⭐⭐⭐ | ECS default behavior, không cần custom metrics |
| Cost | ⭐⭐⭐⭐ | Scale down khi ít traffic |

**Ưu điểm**:
- Đơn giản — ECS built-in
- Không cần custom CloudWatch metrics
- Cost-efficient (scale down khi ít traffic)

**Nhược điểm**:
- Scale lag ~2-3 phút → có thể bị overload trong spike
- CPU/Memory không phải indicator tốt cho SignalR connections
- Không biết connection count thực tế

---

### Option B: Proactive Scaling (Target: 5,000 connections/task)

**Structure**:
```
Custom CloudWatch Metric: SignalR.ConnectionCount per task
ECS Auto-Scaling Policy:
- Scale out khi avg connections > 4,000 (80% of 5,000)
- Scale in khi avg connections < 2,000
- Min tasks: 2, Max tasks: 10
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐⭐ | Proactive — scale trước khi overload |
| Performance | ⭐⭐⭐⭐⭐ | Connection-based scaling — đúng metric |
| Scalability | ⭐⭐⭐⭐⭐ | Predictable capacity planning |
| Simplicity | ⭐⭐⭐ | Cần custom metric, CloudWatch dashboard |
| Cost | ⭐⭐⭐⭐ | Scale down khi ít connections |

**Ưu điểm**:
- Đúng metric — scale dựa trên actual load
- Proactive — không bị overload
- Predictable: biết chính xác capacity

**Nhược điểm**:
- Cần implement custom CloudWatch metric
- Phức tạp hơn Option A

---

### Option C: Proactive Scaling (Target: 10,000 connections/task)

**Structure**: Giống Option B nhưng target cao hơn.

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐ | Ít tasks hơn → ít overhead hơn |
| Performance | ⭐⭐⭐ | 10K connections/task có thể gây memory pressure |
| Scalability | ⭐⭐⭐ | Ít tasks → ít Redis backplane overhead |
| Simplicity | ⭐⭐⭐ | Cần custom metric |
| Cost | ⭐⭐⭐⭐⭐ | Ít tasks nhất → rẻ nhất |

**Ưu điểm**: Chi phí thấp hơn Option B  
**Nhược điểm**: 10K connections/task với 1vCPU/2GB RAM có thể gây memory pressure (mỗi SignalR connection ~10-20KB)

---

### 🏆 Recommendation: **Option B — 5,000 connections/task**

**Lý do**:
- **Memory safety**: 5,000 connections × 20KB = ~100MB → an toàn với 2GB RAM task
- **Proactive**: Scale trước khi overload — quan trọng cho real-time app
- **Predictable**: Biết chính xác capacity (5K connections = 5 rooms × 1000 viewers)
- **Custom metric**: Implement đơn giản qua `IHubContext` connection count

**Implementation**:
```csharp
// Background service publish metric mỗi 30 giây
var connectionCount = _hubContext.Clients.All; // count via custom tracker
await _cloudWatch.PutMetricDataAsync(new PutMetricDataRequest {
    Namespace = "LivestreamApp/SignalR",
    MetricData = [new MetricDatum {
        MetricName = "ConnectionCount",
        Value = connectionCount,
        Unit = StandardUnit.Count
    }]
});
```

---

## Question A3: Viewer Count Cache — Thundering Herd Protection

**Context**: 1000 viewers join/leave đồng thời → 1000 INCR/DECR Redis operations/giây. Cần strategy để tránh race condition và thundering herd.

### Option A: Redis INCR/DECR Atomic Operations

**Structure**:
```
Join: INCR viewer_count:{roomId}
Leave: DECR viewer_count:{roomId}
Sync to DB: Background timer mỗi 5 giây
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐ | Atomic — không race condition |
| Performance | ⭐⭐⭐⭐⭐ | Redis INCR/DECR O(1), cực nhanh |
| Scalability | ⭐⭐⭐⭐ | Redis handles high throughput |
| Simplicity | ⭐⭐⭐⭐⭐ | 2 lines code |
| Cost | ⭐⭐⭐⭐⭐ | Minimal Redis operations |

**Ưu điểm**:
- Atomic — Redis đảm bảo không race condition
- Cực đơn giản
- Performance tốt nhất (O(1) per operation)

**Nhược điểm**:
- Nếu Redis restart → count về 0 (cần resync từ DB)
- Không batch — 1000 viewers = 1000 Redis calls

---

### Option B: Redis INCR/DECR + Lua Script Batch

**Structure**:
```lua
-- Lua script: atomic batch update
local key = KEYS[1]
local delta = tonumber(ARGV[1])
local current = redis.call('INCRBY', key, delta)
redis.call('EXPIRE', key, 3600)
return current
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐⭐ | Atomic Lua script — guaranteed consistency |
| Performance | ⭐⭐⭐⭐⭐ | Batch reduce round trips |
| Scalability | ⭐⭐⭐⭐⭐ | Handles thundering herd |
| Simplicity | ⭐⭐⭐ | Lua script thêm complexity |
| Cost | ⭐⭐⭐⭐⭐ | Ít Redis calls hơn |

**Ưu điểm**:
- Atomic + TTL refresh trong 1 operation
- Batch updates giảm round trips

**Nhược điểm**:
- Lua script phức tạp hơn
- Debugging khó hơn

---

### Option C: Hangfire Job Sync từ DB mỗi 5 giây

**Structure**:
```
Hangfire recurring job (mỗi 5 giây):
  SELECT COUNT(*) FROM viewer_sessions WHERE room_id = ? AND left_at IS NULL
  → SET viewer_count:{roomId} = result
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐⭐ | Source of truth từ DB — luôn chính xác |
| Performance | ⭐⭐ | DB query mỗi 5 giây × N rooms = nhiều queries |
| Scalability | ⭐⭐ | N rooms × 1 DB query/5s = bottleneck |
| Simplicity | ⭐⭐⭐ | Đơn giản về logic nhưng cần Hangfire job |
| Cost | ⭐⭐ | DB load cao |

**Ưu điểm**:
- Luôn chính xác (từ DB)
- Tự heal nếu Redis restart

**Nhược điểm**:
- DB load cao khi nhiều rooms
- 5 giây lag — không real-time
- Không scale với nhiều rooms

---

### 🏆 Recommendation: **Option A — Redis INCR/DECR**

**Lý do**:
- **Simplicity**: 2 lines code, không cần Lua script cho MVP
- **Performance**: Redis INCR/DECR atomic — không race condition
- **Thundering herd**: Redis single-threaded → 1000 INCR/giây vẫn xử lý tuần tự, không conflict
- **Recovery**: Nếu Redis restart → resync từ DB khi room được query lần đầu (lazy resync)

**Recovery strategy**:
```csharp
public async Task<int> GetViewerCountAsync(Guid roomId) {
    var cached = await _redis.StringGetAsync($"viewer_count:{roomId}");
    if (cached.IsNull) {
        // Lazy resync từ DB
        var count = await _db.ViewerSessions
            .CountAsync(s => s.RoomId == roomId && s.LeftAt == null);
        await _redis.StringSetAsync($"viewer_count:{roomId}", count, TimeSpan.FromHours(1));
        return count;
    }
    return (int)cached;
}
```

---

## Question B1: Agora Token Refresh — Failure Handling

**Context**: Agora token TTL = 1 giờ. Client gọi refresh API khi token sắp hết hạn. Nếu Agora service down hoặc token generation fail → cần fallback strategy.

### Option A: Return Cached Token (nếu chưa hết hạn)

**Structure**:
```
GET /api/v1/livestream/rooms/{roomId}/token
  → Check: session/room still active?
  → Check: current token còn hạn > 5 phút?
    → YES: return current token (cached)
    → NO: generate new token
    → FAIL: return error
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐ | Graceful degradation — client tiếp tục với token cũ |
| Performance | ⭐⭐⭐⭐⭐ | Cache hit — không gọi Agora SDK |
| Scalability | ⭐⭐⭐⭐ | Giảm Agora API calls |
| Simplicity | ⭐⭐⭐⭐ | Cache logic đơn giản |
| Cost | ⭐⭐⭐⭐⭐ | Ít Agora API calls |

**Ưu điểm**:
- Graceful degradation — stream tiếp tục nếu Agora down tạm thời
- Giảm Agora API calls (cache hit)
- User experience tốt hơn

**Nhược điểm**:
- Cần cache token (Redis hoặc in-memory)
- Nếu token đã hết hạn → không có fallback

---

### Option B: Return Error → Client Retry

**Structure**:
```
GET /api/v1/livestream/rooms/{roomId}/token
  → Generate new token
  → FAIL: return HTTP 503 "Service Unavailable"
  → Client: show toast "Kết nối bị gián đoạn" + retry button
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐ | Stream bị gián đoạn khi Agora down |
| Performance | ⭐⭐⭐ | No caching benefit |
| Scalability | ⭐⭐⭐ | Simple |
| Simplicity | ⭐⭐⭐⭐⭐ | Đơn giản nhất |
| Cost | ⭐⭐⭐ | Retry tăng Agora API calls |

**Ưu điểm**: Đơn giản, honest về failure  
**Nhược điểm**: Stream bị gián đoạn — bad UX

---

### Option C: Extend Token TTL lên 4 giờ

**Structure**:
```
Token TTL: 4 giờ (thay vì 1 giờ)
Refresh frequency: mỗi 3.5 giờ (thay vì mỗi 55 phút)
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐⭐ | Ít refresh → ít failure points |
| Performance | ⭐⭐⭐⭐⭐ | Ít Agora API calls nhất |
| Scalability | ⭐⭐⭐⭐⭐ | Ít overhead |
| Simplicity | ⭐⭐⭐⭐⭐ | Đơn giản nhất |
| Cost | ⭐⭐⭐⭐⭐ | Ít Agora API calls |

**Ưu điểm**: Ít refresh → ít failure points, đơn giản  
**Nhược điểm**: Token tồn tại lâu hơn → security risk nếu token bị leak (revocation khó hơn)

---

### 🏆 Recommendation: **Option C — Extend TTL lên 4 giờ**

**Lý do**:
- **Reliability**: Ít refresh = ít failure points — quan trọng nhất cho real-time stream
- **Simplicity**: Không cần cache logic phức tạp
- **Security trade-off chấp nhận được**: Agora token chỉ cho phép join 1 channel cụ thể — risk thấp nếu leak
- **Agora Free Tier**: Ít API calls → tiết kiệm quota
- **Private call**: TTL 4 giờ >> max call duration thực tế → không cần refresh trong call

**Security mitigation**: Nếu stream end → server-side revoke channel (Agora API kick all users) → token vô dụng dù chưa expire.

---

## Question B2: Agora Free Tier — Quota Disable Threshold

**Context**: Agora Free Tier = 10,000 phút/tháng. Cần strategy khi gần hết quota để tránh unexpected charges.

### Option A: Disable khi đạt 90% (9,000 phút)

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐ | 1,000 phút buffer — đủ cho 1-2 ngày |
| Performance | ⭐⭐⭐⭐ | Ít false positives |
| Scalability | ⭐⭐⭐⭐ | Reasonable threshold |
| Simplicity | ⭐⭐⭐⭐ | 1 threshold |
| Cost | ⭐⭐⭐⭐ | Tận dụng tối đa quota |

**Ưu điểm**: Tận dụng 90% quota, buffer 1,000 phút  
**Nhược điểm**: Nếu spike traffic → 1,000 phút có thể hết trong vài giờ

---

### Option B: Disable khi đạt 80% (8,000 phút)

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐⭐ | 2,000 phút buffer — an toàn hơn |
| Performance | ⭐⭐⭐⭐ | Conservative nhưng safe |
| Scalability | ⭐⭐⭐⭐ | Good buffer |
| Simplicity | ⭐⭐⭐⭐ | 1 threshold |
| Cost | ⭐⭐⭐ | Lãng phí 2,000 phút/tháng |

**Ưu điểm**: Buffer lớn hơn, an toàn hơn  
**Nhược điểm**: Lãng phí 20% quota

---

### Option C: Không disable tự động — Chỉ alert admin

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐ | Phụ thuộc admin response time |
| Performance | ⭐⭐⭐⭐⭐ | Không giới hạn feature |
| Scalability | ⭐⭐ | Không scale — manual process |
| Simplicity | ⭐⭐⭐⭐⭐ | Đơn giản nhất |
| Cost | ⭐⭐ | Risk unexpected charges |

**Ưu điểm**: Không disable feature tự động  
**Nhược điểm**: Nếu admin không phản hồi kịp → unexpected Agora charges

---

### 🏆 Recommendation: **Option A — Disable tại 90%**

**Lý do**:
- **Balance**: Tận dụng 90% quota + buffer 1,000 phút (đủ cho ~16 giờ private call)
- **MVP scale**: 10K-100K users năm đầu → 10,000 phút/tháng đủ cho early stage
- **Alert at 80%**: Gửi alert admin tại 80% → admin có thể upgrade plan trước khi hit 90%
- **Feature flag**: Disable private call (tốn nhiều phút nhất) trước, public stream sau

**Implementation**:
```csharp
// Hangfire job hàng ngày check Agora usage
if (usageMinutes >= 9000) {
    await _featureFlags.DisableAsync("private-call");
    await _alertService.SendAsync("Agora quota 90% reached");
} else if (usageMinutes >= 8000) {
    await _alertService.SendAsync("Agora quota 80% — action needed");
}
```

---

## Question C1: Billing Tick Job — Failure Handling

**Context**: Hangfire job `ProcessBillingTick` chạy mỗi 10 giây per active call session. Nếu job fail (DB timeout, exception) → cần quyết định behavior để đảm bảo không charge sai.

### Option A: Retry 3 lần → Fail → End Call (Safe)

**Structure**:
```
ProcessBillingTick(sessionId):
  → Try deduct coins
  → FAIL: Retry 1 (1s delay)
  → FAIL: Retry 2 (2s delay)
  → FAIL: Retry 3 (4s delay)
  → FAIL: End call (EndedBy=System, reason=BillingError)
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐⭐ | Không bao giờ charge thiếu — safe |
| Performance | ⭐⭐⭐ | Call bị end khi billing fail |
| Scalability | ⭐⭐⭐⭐ | Predictable behavior |
| Simplicity | ⭐⭐⭐⭐ | Clear logic |
| Cost | ⭐⭐⭐⭐ | Không charge sai |

**Ưu điểm**:
- **Financial integrity**: Không bao giờ charge thiếu hoặc charge sai
- Predictable — user biết call sẽ end nếu billing fail
- Audit trail rõ ràng (EndedBy=System, reason=BillingError)

**Nhược điểm**:
- Call bị end khi DB tạm thời down → bad UX
- Transient errors (DB timeout 1 giây) → call bị end oan

---

### Option B: Retry 3 lần → Fail → Skip Tick (User-Friendly)

**Structure**:
```
ProcessBillingTick(sessionId):
  → Try deduct coins
  → FAIL after 3 retries: Skip tick (không charge)
  → Log warning, tiếp tục call
  → Next tick: try again bình thường
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐ | Có thể bị "free minutes" khi billing fail |
| Performance | ⭐⭐⭐⭐⭐ | Call không bị gián đoạn |
| Scalability | ⭐⭐⭐⭐ | Resilient |
| Simplicity | ⭐⭐⭐⭐ | Đơn giản |
| Cost | ⭐⭐ | Revenue loss khi billing fail |

**Ưu điểm**: UX tốt hơn — call không bị end  
**Nhược điểm**: Revenue loss — user được "free minutes" khi DB down

---

### Option C: Retry 3 lần → Fail → Log + Alert + Tiếp tục

**Structure**:
```
ProcessBillingTick(sessionId):
  → FAIL after 3 retries: Log error + CloudWatch alert
  → Tiếp tục call, manual investigation
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐ | Phụ thuộc manual investigation |
| Performance | ⭐⭐⭐⭐⭐ | Call không bị gián đoạn |
| Scalability | ⭐⭐ | Không scale — manual process |
| Simplicity | ⭐⭐⭐⭐⭐ | Đơn giản nhất |
| Cost | ⭐⭐ | Revenue loss + manual effort |

**Ưu điểm**: Đơn giản  
**Nhược điểm**: Revenue loss + manual investigation overhead

---

### 🏆 Recommendation: **Option A — Retry → End Call**

**Lý do**:
- **Financial integrity**: Đây là billing system — không charge sai quan trọng hơn UX
- **APPI compliance**: Audit trail rõ ràng cho mọi transaction
- **Transient errors**: 3 retries với exponential backoff (1s+2s+4s = 7 giây) đủ để handle transient DB issues
- **User communication**: Hiển thị rõ lý do "Cuộc gọi kết thúc do lỗi hệ thống" + không charge tick cuối

**Compensation logic**:
```csharp
// Nếu end call do billing error → không charge tick cuối
// Refund partial tick nếu đã charge một phần
if (endReason == CallEndReason.BillingError) {
    // Không charge, log incident
    await _incidentLog.RecordAsync(sessionId, "BillingTickFailed");
}
```

---

## Question C2: Billing Tick — Idempotency

**Context**: Hangfire có thể execute cùng 1 job 2 lần trong edge cases (server restart, network partition). Cần đảm bảo không charge coin 2 lần cho cùng 1 tick.

### Option A: TickNumber Unique Constraint

**Structure**:
```sql
CREATE UNIQUE INDEX idx_billing_ticks_session_tick 
ON billing_ticks(call_session_id, tick_number);
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐⭐ | DB constraint — guaranteed no duplicate |
| Performance | ⭐⭐⭐⭐⭐ | Index lookup O(log n) |
| Scalability | ⭐⭐⭐⭐⭐ | DB handles it |
| Simplicity | ⭐⭐⭐⭐⭐ | 1 index, no extra infrastructure |
| Cost | ⭐⭐⭐⭐⭐ | Minimal overhead |

**Ưu điểm**:
- DB-level guarantee — không thể bypass
- Đơn giản — chỉ cần unique index
- Idempotent insert: `INSERT ... ON CONFLICT DO NOTHING`

**Nhược điểm**:
- Cần handle `UniqueConstraintException` trong code
- Không prevent duplicate execution — chỉ prevent duplicate record

---

### Option B: Distributed Lock (Redis)

**Structure**:
```
Redis key: billing_lock:{sessionId}:{tickNumber}
TTL: 30 giây
Logic: SET NX → nếu fail → skip (already processing)
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐ | Prevent concurrent execution |
| Performance | ⭐⭐⭐⭐ | Redis SET NX nhanh |
| Scalability | ⭐⭐⭐⭐ | Redis handles it |
| Simplicity | ⭐⭐⭐ | Thêm Redis dependency cho billing |
| Cost | ⭐⭐⭐⭐ | Minimal Redis overhead |

**Ưu điểm**: Prevent concurrent execution (không chỉ duplicate record)  
**Nhược điểm**: Redis down → billing lock không hoạt động; thêm complexity

---

### Option C: Cả A và B (Defense in Depth)

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐⭐ | Double protection |
| Performance | ⭐⭐⭐⭐ | Slight overhead |
| Scalability | ⭐⭐⭐⭐ | Both scale well |
| Simplicity | ⭐⭐ | 2 mechanisms để maintain |
| Cost | ⭐⭐⭐ | Redis + DB overhead |

**Ưu điểm**: Maximum protection  
**Nhược điểm**: Over-engineering cho MVP

---

### 🏆 Recommendation: **Option A — TickNumber Unique Constraint**

**Lý do**:
- **Sufficient**: DB unique constraint đủ để prevent duplicate charge — đây là primary concern
- **Simplicity**: 1 index, không cần Redis lock
- **Hangfire behavior**: Hangfire không thực sự chạy duplicate jobs thường xuyên — chỉ trong edge cases
- **Idempotent pattern**: `INSERT ... ON CONFLICT (session_id, tick_number) DO NOTHING` → safe to retry

**Implementation**:
```csharp
try {
    await _db.BillingTicks.AddAsync(new BillingTick {
        CallSessionId = sessionId,
        TickNumber = tickNumber,
        CoinsCharged = coinsToCharge,
        // ...
    });
    await _db.SaveChangesAsync();
    // Deduct coins only after successful insert
    await _coinService.DeductCoinsAsync(viewerId, coinsToCharge, ...);
} catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation()) {
    // Already processed — skip silently
    _logger.LogWarning("Duplicate billing tick {TickNumber} for session {SessionId}", tickNumber, sessionId);
}
```

---

## Question D1: Redis Streams MAXLEN Strategy

**Context**: Room chat lưu trong Redis Stream `room:{roomId}:chat`. Cần giới hạn size để tránh memory exhaustion, đặc biệt với rooms có nhiều chat.

### Option A: MAXLEN 1000 với Approximate Trimming (~)

**Structure**:
```
XADD room:{roomId}:chat MAXLEN ~ 1000 * field value
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐ | Approximate — có thể có 1000-1100 entries |
| Performance | ⭐⭐⭐⭐⭐ | `~` = lazy trimming, không block |
| Scalability | ⭐⭐⭐⭐⭐ | Redis optimized cho approximate trimming |
| Simplicity | ⭐⭐⭐⭐⭐ | 1 flag thêm vào XADD |
| Cost | ⭐⭐⭐⭐⭐ | Minimal overhead |

**Ưu điểm**:
- Redis documentation khuyến nghị `~` cho production
- Không block XADD operation
- Memory bound — không bao giờ vượt quá ~1100 entries

**Nhược điểm**:
- Không chính xác tuyệt đối (có thể 1000-1100 entries)
- Không quan trọng cho use case này

---

### Option B: MAXLEN 1000 Exact Trimming

**Structure**:
```
XADD room:{roomId}:chat MAXLEN 1000 * field value
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐⭐ | Chính xác tuyệt đối — luôn ≤ 1000 |
| Performance | ⭐⭐⭐ | Exact trimming có thể block briefly |
| Scalability | ⭐⭐⭐ | Overhead cao hơn với high-traffic rooms |
| Simplicity | ⭐⭐⭐⭐⭐ | Đơn giản |
| Cost | ⭐⭐⭐ | Slightly higher CPU |

**Ưu điểm**: Chính xác tuyệt đối  
**Nhược điểm**: Redis docs không khuyến nghị cho production — có thể gây latency spike

---

### Option C: Không giới hạn MAXLEN — Chỉ dựa vào TTL

**Structure**:
```
XADD room:{roomId}:chat * field value
EXPIRE room:{roomId}:chat 604800  # 7 ngày
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐ | Memory không bounded — risk OOM |
| Performance | ⭐⭐⭐⭐⭐ | Không trim overhead |
| Scalability | ⭐⭐ | Viral room có thể có 100K+ messages |
| Simplicity | ⭐⭐⭐⭐⭐ | Đơn giản nhất |
| Cost | ⭐⭐ | Memory risk |

**Ưu điểm**: Đơn giản, không mất messages  
**Nhược điểm**: Viral room (100K messages × 200 bytes = 20MB per room) → memory risk

---

### 🏆 Recommendation: **Option A — MAXLEN ~ 1000 (Approximate)**

**Lý do**:
- **Redis best practice**: Redis documentation explicitly recommends `~` for production
- **Performance**: Không block XADD — quan trọng cho real-time chat
- **Memory bound**: ~1000 entries × 200 bytes = ~200KB per room → safe
- **Use case fit**: Chat history không cần chính xác tuyệt đối — approximate là đủ

**Note**: Q-C4 đã quyết định viewer mới join không thấy history → MAXLEN chỉ để bound memory, không phải để serve history.

---

## Question D2: ExportRoomChatToS3 — Failure Handling

**Context**: Hangfire job export chat từ Redis Streams lên S3 hàng ngày. Nếu S3 upload fail → cần strategy để không mất data (Redis TTL 7 ngày).

### Option A: Retry 3 lần → Fail → Log + Skip

**Structure**:
```
ExportRoomChatToS3(roomId, date):
  → Read Redis Stream
  → Upload to S3
  → FAIL after 3 retries: Log error, skip
  → Data vẫn trong Redis cho đến khi TTL expire
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐ | Data có thể mất nếu TTL expire trước retry |
| Performance | ⭐⭐⭐⭐ | Đơn giản |
| Scalability | ⭐⭐⭐⭐ | Stateless |
| Simplicity | ⭐⭐⭐⭐⭐ | Đơn giản nhất |
| Cost | ⭐⭐⭐⭐⭐ | Minimal |

**Ưu điểm**: Đơn giản  
**Nhược điểm**: Nếu S3 down > 7 ngày → data mất (unlikely nhưng possible)

---

### Option B: Retry 3 lần → Fail → Alert + Extend Redis TTL

**Structure**:
```
ExportRoomChatToS3(roomId, date):
  → FAIL after 3 retries:
    → EXPIRE room:{roomId}:chat 172800  # Extend thêm 2 ngày
    → Alert admin
    → Schedule retry sau 24 giờ
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐⭐ | Data được bảo vệ thêm 2 ngày |
| Performance | ⭐⭐⭐⭐ | Thêm Redis EXPIRE call |
| Scalability | ⭐⭐⭐⭐ | Manageable |
| Simplicity | ⭐⭐⭐ | Phức tạp hơn Option A |
| Cost | ⭐⭐⭐⭐ | Thêm Redis memory tạm thời |

**Ưu điểm**: Data được bảo vệ khi S3 down  
**Nhược điểm**: Phức tạp hơn, cần manage TTL extension

---

### Option C: Retry 3 lần → Fail → Dead Letter Queue (Hangfire Failed Jobs)

**Structure**:
```
ExportRoomChatToS3(roomId, date):
  → FAIL after 3 retries:
    → Job moves to Hangfire "Failed" queue
    → Admin có thể manually retry từ Hangfire Dashboard
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐ | Manual retry available |
| Performance | ⭐⭐⭐⭐ | Hangfire handles it |
| Scalability | ⭐⭐⭐⭐ | Hangfire Dashboard |
| Simplicity | ⭐⭐⭐⭐⭐ | Hangfire built-in behavior |
| Cost | ⭐⭐⭐⭐⭐ | Minimal |

**Ưu điểm**: Hangfire Dashboard cho phép manual retry  
**Nhược điểm**: Phụ thuộc admin action; data vẫn có thể mất nếu TTL expire

---

### 🏆 Recommendation: **Option C — Dead Letter Queue (Hangfire Failed Jobs)**

**Lý do**:
- **Simplicity**: Hangfire built-in behavior — không cần code thêm
- **Visibility**: Hangfire Dashboard hiển thị failed jobs rõ ràng
- **Manual retry**: Admin có thể retry từ Dashboard khi S3 available
- **TTL safety**: S3 outage > 7 ngày là extremely unlikely (AWS SLA 99.99%)
- **Archive purpose**: Chat export là "nice to have" archive — không phải critical data path

**Note**: Nếu cần higher reliability → upgrade lên Option B sau khi có production data về S3 failure frequency.

---

## Question E1: PostgreSQL Partition Creation Strategy

**Context**: `direct_messages` và `conversations` dùng PostgreSQL range partitioning theo `SentAt` (monthly). Partition phải tồn tại trước khi INSERT — nếu không có partition phù hợp → PostgreSQL throw error.

### Option A: Tạo Partition khi Startup (EF Core Migration)

**Structure**:
```csharp
// Trong DbContext OnModelCreating hoặc migration
// Tạo partition cho tháng hiện tại + 1 tháng tới
protected override void Up(MigrationBuilder migrationBuilder) {
    migrationBuilder.Sql(@"
        CREATE TABLE IF NOT EXISTS direct_messages_2026_03 
        PARTITION OF direct_messages
        FOR VALUES FROM ('2026-03-01') TO ('2026-04-01');
        
        CREATE TABLE IF NOT EXISTS direct_messages_2026_04 
        PARTITION OF direct_messages
        FOR VALUES FROM ('2026-04-01') TO ('2026-05-01');
    ");
}
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐ | Chỉ tạo 2 partitions — nếu deploy trễ sang tháng mới → thiếu partition |
| Performance | ⭐⭐⭐⭐⭐ | Partition sẵn sàng ngay khi app start |
| Scalability | ⭐⭐ | Cần deploy mỗi tháng để tạo partition mới |
| Simplicity | ⭐⭐⭐ | Migration phức tạp, cần update hàng tháng |
| Cost | ⭐⭐⭐⭐⭐ | Không overhead |

**Ưu điểm**:
- Partition sẵn sàng ngay khi app start
- Không cần background job

**Nhược điểm**:
- Cần tạo migration mới mỗi tháng → operational overhead
- Nếu quên deploy → INSERT fail vào đầu tháng mới

---

### Option B: Hangfire Job Tạo Partition Hàng Tháng

**Structure**:
```csharp
// Hangfire recurring job — chạy ngày 25 hàng tháng lúc 00:00
[AutomaticRetry(Attempts = 3)]
public async Task CreateNextMonthPartitionAsync() {
    var nextMonth = DateTime.UtcNow.AddMonths(1);
    var partitionName = $"direct_messages_{nextMonth:yyyy_MM}";
    var startDate = new DateTime(nextMonth.Year, nextMonth.Month, 1);
    var endDate = startDate.AddMonths(1);
    
    await _db.Database.ExecuteSqlRawAsync($@"
        CREATE TABLE IF NOT EXISTS {partitionName}
        PARTITION OF direct_messages
        FOR VALUES FROM ('{startDate:yyyy-MM-dd}') TO ('{endDate:yyyy-MM-dd}')
    ");
}
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐ | Tự động — không cần manual deploy |
| Performance | ⭐⭐⭐⭐⭐ | Partition tạo trước 5-6 ngày |
| Scalability | ⭐⭐⭐⭐⭐ | Tự động mãi mãi |
| Simplicity | ⭐⭐⭐⭐ | 1 Hangfire job, không cần migration hàng tháng |
| Cost | ⭐⭐⭐⭐⭐ | Minimal overhead |

**Ưu điểm**:
- Tự động — không cần operational intervention
- Tạo trước 5-6 ngày → buffer đủ lớn
- Scale mãi mãi không cần code change

**Nhược điểm**:
- Nếu Hangfire job fail → partition không được tạo
- Cần ensure job chạy đúng giờ

---

### Option C: Cả A và B (Startup Check + Monthly Job)

**Structure**:
```csharp
// Startup: check và tạo partition nếu thiếu (self-healing)
public async Task EnsurePartitionsExistAsync() {
    var months = new[] { DateTime.UtcNow, DateTime.UtcNow.AddMonths(1) };
    foreach (var month in months) {
        await CreatePartitionIfNotExistsAsync(month);
    }
}

// Hangfire job ngày 25 hàng tháng: tạo partition tháng tới
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐⭐ | Self-healing: startup fix nếu job fail |
| Performance | ⭐⭐⭐⭐⭐ | Partition luôn sẵn sàng |
| Scalability | ⭐⭐⭐⭐⭐ | Tự động + self-healing |
| Simplicity | ⭐⭐⭐ | 2 mechanisms nhưng đơn giản |
| Cost | ⭐⭐⭐⭐⭐ | Minimal overhead |

**Ưu điểm**:
- Defense in depth: nếu Hangfire job fail → startup tự heal
- Không bao giờ thiếu partition
- Không cần migration hàng tháng

**Nhược điểm**:
- 2 code paths để maintain (nhưng đơn giản)

---

### 🏆 Recommendation: **Option C — Startup Check + Monthly Job**

**Lý do**:
- **Reliability**: Self-healing — startup check đảm bảo partition luôn tồn tại dù Hangfire job fail
- **Zero operational overhead**: Không cần deploy hàng tháng như Option A
- **Defense in depth**: 2 mechanisms bảo vệ lẫn nhau
- **Startup cost**: Check partition khi startup là O(1) query — không ảnh hưởng performance

**Implementation note**:
```csharp
// IHostedService chạy khi startup
public class PartitionMaintenanceService : IHostedService {
    public async Task StartAsync(CancellationToken ct) {
        // Tạo partition cho tháng hiện tại + 2 tháng tới
        for (int i = 0; i <= 2; i++) {
            await EnsurePartitionExistsAsync(DateTime.UtcNow.AddMonths(i));
        }
    }
}
```

---

## Question E2: Partition Query Safeguard

**Context**: PostgreSQL partition pruning chỉ hoạt động khi query có `WHERE SentAt BETWEEN ...`. Nếu developer quên filter → full table scan across all partitions → performance disaster.

### Option A: EF Core Interceptor Tự Động Thêm Filter

**Structure**:
```csharp
public class PartitionSafeguardInterceptor : DbCommandInterceptor {
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, CommandEventData eventData, 
        InterceptionResult<DbDataReader> result) {
        
        if (command.CommandText.Contains("direct_messages") && 
            !command.CommandText.Contains("sent_at")) {
            // Tự động thêm filter 30 ngày
            command.CommandText += " AND sent_at >= NOW() - INTERVAL '30 days'";
        }
        return result;
    }
}
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐ | Interceptor có thể miss edge cases |
| Performance | ⭐⭐⭐⭐⭐ | Tự động protect — không bao giờ full scan |
| Scalability | ⭐⭐⭐⭐ | Transparent |
| Simplicity | ⭐⭐ | Interceptor phức tạp, khó debug |
| Cost | ⭐⭐⭐⭐⭐ | Minimal overhead |

**Ưu điểm**:
- Transparent — developer không cần nhớ
- Tự động protect production

**Nhược điểm**:
- Interceptor logic phức tạp, dễ có false positives
- Khó debug khi query bị modify ngầm
- Có thể miss edge cases (subqueries, CTEs)

---

### Option B: Throw Exception nếu Không Có Filter (Fail-Fast)

**Structure**:
```csharp
// Repository method enforce filter
public async Task<List<DirectMessage>> GetMessagesAsync(
    Guid conversationId, 
    DateTimeOffset? from = null,  // Required in practice
    DateTimeOffset? to = null) {
    
    if (from == null) {
        throw new InvalidOperationException(
            "DirectMessage queries MUST include SentAt filter for partition pruning. " +
            "Use GetRecentMessagesAsync() for default 30-day window.");
    }
    // ...
}
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐⭐ | Fail-fast — catch trong dev/test |
| Performance | ⭐⭐⭐⭐⭐ | Không bao giờ full scan trong production |
| Scalability | ⭐⭐⭐⭐⭐ | Enforce at code level |
| Simplicity | ⭐⭐⭐⭐⭐ | Đơn giản — throw exception |
| Cost | ⭐⭐⭐⭐⭐ | Zero overhead |

**Ưu điểm**:
- Catch lỗi sớm trong development
- Không có hidden behavior
- Đơn giản và rõ ràng

**Nhược điểm**:
- Developer phải biết về constraint này
- Cần document rõ

---

### Option C: Chỉ Document trong Coding Standards

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐ | Phụ thuộc developer discipline |
| Performance | ⭐⭐ | Không enforce — full scan có thể xảy ra |
| Scalability | ⭐⭐ | Không scale với team lớn |
| Simplicity | ⭐⭐⭐⭐⭐ | Đơn giản nhất |
| Cost | ⭐⭐⭐⭐⭐ | Zero overhead |

**Ưu điểm**: Đơn giản  
**Nhược điểm**: Không enforce — production incident có thể xảy ra

---

### 🏆 Recommendation: **Option B — Throw Exception (Fail-Fast)**

**Lý do**:
- **Simplicity**: Đơn giản hơn Option A (interceptor) rất nhiều
- **Visibility**: Lỗi rõ ràng — developer biết ngay vấn đề
- **Catch early**: Fail trong dev/test, không phải production
- **No hidden behavior**: Không modify query ngầm như Option A
- **Complement với coding standards**: Document + enforce = best practice

**Implementation**: Repository pattern enforce filter tại method signature level — `GetRecentMessagesAsync(conversationId, from, to)` với `from` required.

---

## Question F1: SignalR Message Latency Target

**Context**: Room chat messages được broadcast qua SignalR từ server đến tất cả viewers trong room. Latency target ảnh hưởng đến infrastructure sizing và SLA.

### Option A: < 500ms p95

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐⭐ | Achievable với current stack |
| Performance | ⭐⭐⭐⭐ | Tốt cho real-time chat |
| Scalability | ⭐⭐⭐⭐ | Không cần over-engineer |
| Simplicity | ⭐⭐⭐⭐⭐ | Standard target |
| Cost | ⭐⭐⭐⭐⭐ | Không cần premium infrastructure |

**Ưu điểm**: Achievable, không cần over-engineer  
**Nhược điểm**: Có thể cảm nhận được lag trong chat nhanh

---

### Option B: < 200ms p95

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐ | Aggressive — cần optimize nhiều |
| Performance | ⭐⭐⭐⭐⭐ | Excellent UX |
| Scalability | ⭐⭐⭐ | Cần premium infrastructure |
| Simplicity | ⭐⭐⭐ | Cần profiling và optimization |
| Cost | ⭐⭐⭐ | Cần larger ECS tasks |

**Ưu điểm**: UX tốt nhất  
**Nhược điểm**: Khó đạt được consistently với Redis backplane overhead

---

### Option C: < 1000ms p95

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐⭐ | Dễ đạt |
| Performance | ⭐⭐⭐ | Acceptable nhưng noticeable lag |
| Scalability | ⭐⭐⭐⭐⭐ | Rất dễ đạt |
| Simplicity | ⭐⭐⭐⭐⭐ | Không cần optimize |
| Cost | ⭐⭐⭐⭐⭐ | Minimal infrastructure |

**Ưu điểm**: Dễ đạt, không cần optimize  
**Nhược điểm**: 1 giây lag trong chat là noticeable — bad UX

---

### 🏆 Recommendation: **Option A — < 500ms p95**

**Lý do**:
- **Achievable**: SignalR + Redis backplane trong cùng VPC → latency thực tế ~50-100ms → 500ms p95 là conservative và achievable
- **Good UX**: 500ms không noticeable trong chat context (khác với gaming)
- **No over-engineering**: Không cần premium infrastructure hay complex optimization
- **Consistent với Unit 1**: Unit 1 đã set 500ms p95 cho API responses

**Measurement**: CloudWatch custom metric `SignalR.MessageLatency` từ server send đến client ACK.

---

## Question F2: Billing Tick Latency SLA

**Context**: Hangfire job `ProcessBillingTick` chạy mỗi 10 giây. Coin deduction xảy ra trong job execution. Cần xác định acceptable latency để set Hangfire queue priority và monitoring alerts.

### Option A: 10 giây ± 2 giây (Tight SLA)

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐⭐ | Predictable billing |
| Performance | ⭐⭐⭐⭐ | Cần dedicated Hangfire queue |
| Scalability | ⭐⭐⭐⭐ | Manageable |
| Simplicity | ⭐⭐⭐ | Cần queue priority config |
| Cost | ⭐⭐⭐⭐ | Dedicated queue overhead |

**Ưu điểm**: Predictable — user thấy coin deduct đúng giờ  
**Nhược điểm**: Cần dedicated Hangfire queue với high priority

---

### Option B: 10 giây ± 5 giây (Relaxed SLA)

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐ | Acceptable variance |
| Performance | ⭐⭐⭐⭐⭐ | Hangfire default behavior |
| Scalability | ⭐⭐⭐⭐⭐ | No special config |
| Simplicity | ⭐⭐⭐⭐⭐ | Hangfire default |
| Cost | ⭐⭐⭐⭐⭐ | No overhead |

**Ưu điểm**: Đơn giản — Hangfire default behavior  
**Nhược điểm**: ±5 giây variance — coin deduct có thể trễ 15 giây

---

### Option C: Không có SLA — Best Effort

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐ | Không predictable |
| Performance | ⭐⭐⭐⭐⭐ | No constraint |
| Scalability | ⭐⭐⭐⭐⭐ | No constraint |
| Simplicity | ⭐⭐⭐⭐⭐ | Đơn giản nhất |
| Cost | ⭐⭐⭐⭐⭐ | Minimal |

**Ưu điểm**: Đơn giản  
**Nhược điểm**: Không có SLA → không có monitoring threshold → khó detect issues

---

### 🏆 Recommendation: **Option B — 10 giây ± 5 giây**

**Lý do**:
- **Practical**: Hangfire default polling interval là 15 giây → ±5 giây là realistic
- **User impact**: Coin deduct trễ 5 giây không ảnh hưởng UX đáng kể (user không nhìn đồng hồ đếm giây)
- **Simplicity**: Không cần dedicated queue hay special config
- **Monitoring**: Set CloudWatch alert nếu billing tick latency > 30 giây (3× expected) → detect real issues

**Note**: Nếu cần tighter SLA → configure Hangfire polling interval xuống 5 giây cho billing queue.

---

## Question F3: Direct Message Delivery Latency

**Context**: DirectChat messages gửi qua SignalR đến recipient. Khác với room chat (broadcast 1→N), DirectChat là 1→1 → latency thường thấp hơn.

### Option A: < 500ms p95

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐⭐ | Achievable, consistent với room chat |
| Performance | ⭐⭐⭐⭐ | Good UX |
| Scalability | ⭐⭐⭐⭐ | Standard |
| Simplicity | ⭐⭐⭐⭐⭐ | Same target as room chat |
| Cost | ⭐⭐⭐⭐⭐ | No extra infrastructure |

**Ưu điểm**: Consistent với room chat SLA, achievable  
**Nhược điểm**: Có thể conservative cho 1→1 messaging

---

### Option B: < 1000ms p95

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐⭐⭐ | Rất dễ đạt |
| Performance | ⭐⭐⭐ | Noticeable lag trong chat |
| Scalability | ⭐⭐⭐⭐⭐ | Dễ đạt |
| Simplicity | ⭐⭐⭐⭐⭐ | Không cần optimize |
| Cost | ⭐⭐⭐⭐⭐ | Minimal |

**Ưu điểm**: Dễ đạt  
**Nhược điểm**: 1 giây lag trong 1→1 chat là noticeable — bad UX

---

### Option C: Không có SLA — Best Effort

| Tiêu chí | Score | Rationale |
|---|---|---|
| Reliability | ⭐⭐⭐ | Không predictable |
| Performance | ⭐⭐⭐ | Không có target |
| Scalability | ⭐⭐⭐ | Không có baseline |
| Simplicity | ⭐⭐⭐⭐⭐ | Đơn giản nhất |
| Cost | ⭐⭐⭐⭐⭐ | Minimal |

**Ưu điểm**: Đơn giản  
**Nhược điểm**: Không có monitoring threshold → khó detect degradation

---

### 🏆 Recommendation: **Option A — < 500ms p95**

**Lý do**:
- **Consistency**: Cùng target với room chat → 1 SLA cho toàn bộ SignalR messaging
- **Achievable**: DirectChat 1→1 thực tế sẽ đạt ~50-150ms → 500ms là conservative
- **UX**: 500ms không noticeable trong messaging context
- **Monitoring**: Cùng CloudWatch metric với room chat → đơn giản hơn

---

## Summary — All Recommendations

| Question | Topic | Recommendation | Rationale |
|---|---|---|---|
| **A1** | SignalR Backplane | Option A — Shared Redis | MVP simplicity, migration path rõ ràng |
| **A2** | SignalR Connection Limit | Option B — 5,000 conn/task | Memory safe, proactive scaling |
| **A3** | Viewer Count Cache | Option A — Redis INCR/DECR | Atomic, đơn giản, đủ cho MVP |
| **B1** | Agora Token Refresh | Option C — TTL 4 giờ | Ít failure points, security risk thấp |
| **B2** | Agora Quota Threshold | Option A — Disable tại 90% | Balance quota usage + buffer |
| **C1** | Billing Tick Failure | Option A — Retry → End Call | Financial integrity ưu tiên |
| **C2** | Billing Idempotency | Option A — Unique Constraint | Đủ, đơn giản, DB-level guarantee |
| **D1** | Redis Streams MAXLEN | Option A — MAXLEN ~ 1000 | Redis best practice, non-blocking |
| **D2** | S3 Export Failure | Option C — Dead Letter Queue | Hangfire built-in, visibility tốt |
| **E1** | Partition Creation | Option C — Startup + Monthly Job | Self-healing, zero operational overhead |
| **E2** | Partition Query Safeguard | Option B — Throw Exception | Fail-fast, đơn giản, catch early |
| **F1** | SignalR Chat Latency | Option A — < 500ms p95 | Achievable, good UX |
| **F2** | Billing Tick Latency | Option B — ±5 giây | Hangfire default, practical |
| **F3** | DirectMessage Latency | Option A — < 500ms p95 | Consistent với room chat SLA |
