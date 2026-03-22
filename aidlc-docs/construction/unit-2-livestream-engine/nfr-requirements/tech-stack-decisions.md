# Tech Stack Decisions — Unit 2: Livestream Engine

**Ngày tạo**: 2026-03-22  
**Unit**: Unit 2 — Livestream Engine  
**Kế thừa từ**: Unit 1 Tech Stack (tất cả decisions vẫn áp dụng)

---

## Kế thừa từ Unit 1 (KHÔNG thay đổi)

| Layer | Technology | Version |
|---|---|---|
| Backend framework | ASP.NET Core | 8.0 |
| ORM | Entity Framework Core | 8.0 |
| Database | PostgreSQL | 15 (RDS) |
| Cache | Redis | 7.x (ElastiCache) |
| Background jobs | Hangfire | 1.8.x |
| Logging | Serilog | 3.x |
| Auth | JWT httpOnly Cookie | — |
| Container | Docker + ECS Fargate | — |
| Frontend PWA | Next.js 14 | App Router |
| Frontend Admin | Next.js 14 | App Router |
| State management | Zustand | 4.x |
| UI components | shadcn/ui + Tailwind CSS | — |
| i18n | next-intl | — |

---

## Unit 2 — New Technology Decisions

### TD-U2-01: Real-time Communication — ASP.NET Core SignalR

**Decision**: Dùng ASP.NET Core SignalR cho tất cả real-time features (room chat, viewer count, DirectChat, call signaling).

**Alternatives considered**:
- WebSocket raw: Phức tạp hơn, không có built-in group management
- Socket.IO: Node.js ecosystem, không phù hợp với .NET backend
- Server-Sent Events: Chỉ server→client, không bidirectional

**Rationale**:
- Built-in vào ASP.NET Core — không cần thêm dependency
- Automatic fallback (WebSocket → Long Polling)
- Built-in group management cho rooms
- Redis backplane scale-out đơn giản

**Configuration**:
```csharp
builder.Services.AddSignalR(options => {
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 32 * 1024; // 32KB max message
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
}).AddStackExchangeRedis(config["Redis:ConnectionString"], options => {
    options.Configuration.ChannelPrefix = RedisChannel.Literal("livestream");
});
```

**Hubs**:
- `LivestreamHub`: `/hubs/livestream` — room events, viewer count, gifts
- `ChatHub`: `/hubs/chat` — room chat + DirectChat messages

---

### TD-U2-02: Video Call — Agora RTC SDK

**Decision**: Dùng Agora RTC cho private video call (1-1 có tính phí).

**Alternatives considered**:
- WebRTC raw: Cần STUN/TURN server, phức tạp, không có billing integration
- Twilio Video: Đắt hơn, không có free tier đủ lớn
- Daily.co: Ít documentation tiếng Nhật, nhỏ hơn Agora
- Zoom SDK: Overkill, không phù hợp cho embedded call

**Rationale**:
- Free tier 10,000 phút/tháng — đủ cho MVP
- SDK cho Web (React) và Mobile (React Native future)
- Token-based auth — server-side control
- Low latency (<400ms) — phù hợp cho dating app
- Agora có data center tại Nhật Bản (ap-northeast-1)

**SDK versions**:
- Backend: `Agora.Rtc.Token` NuGet package (token generation)
- Frontend PWA: `agora-rtc-react` npm package

**Token generation**:
```csharp
// Server-side token generation (không expose App Secret)
var tokenBuilder = new RtcTokenBuilder();
var token = tokenBuilder.BuildTokenWithUid(
    appId: config["Agora:AppId"],
    appCertificate: config["Agora:AppCertificate"],
    channelName: $"call_{sessionId}",
    uid: userId.GetHashCode(),
    role: RtcRole.Publisher,
    privilegeExpiredTs: DateTimeOffset.UtcNow.AddHours(4).ToUnixTimeSeconds()
);
```

---

### TD-U2-03: Redis Streams — StackExchange.Redis

**Decision**: Dùng Redis Streams (via StackExchange.Redis) cho room chat history.

**Alternatives considered**:
- PostgreSQL table: Không real-time, write-heavy cho chat
- In-memory (SignalR): Mất khi restart, không persist
- Kafka: Overkill cho MVP, phức tạp
- MongoDB: Thêm database, không cần thiết

**Rationale**:
- Redis đã có trong stack (Unit 1) — không thêm dependency
- Redis Streams: append-only, ordered, consumer groups
- MAXLEN ~ 1000: Memory bounded
- TTL 7 ngày: Tự cleanup
- XADD O(1): Không ảnh hưởng performance

**Key patterns**:
```
room:{roomId}:chat          # Redis Stream, MAXLEN ~ 1000, TTL 7 ngày
viewer_count:{roomId}       # String (INCR/DECR), TTL 1 giờ
call_session:{sessionId}    # Hash (call state), TTL 4 giờ
agora_quota:current_month   # String (minutes used), TTL đến cuối tháng
```

---

### TD-U2-04: PostgreSQL Partitioning — Range Partitioning

**Decision**: Dùng PostgreSQL native range partitioning cho `direct_messages` table.

**Alternatives considered**:
- TimescaleDB: Hypertable tự động, nhưng thêm extension dependency
- Application-level sharding: Phức tạp, không cần thiết
- No partitioning: Full table scan khi data lớn

**Rationale**:
- PostgreSQL 15 native partitioning — không cần extension
- Monthly partitions: Phù hợp với query pattern (recent messages)
- Partition pruning: Query với `SentAt` filter chỉ scan 1-2 partitions
- EF Core support: `HasPartitionByRange` trong model config

**Partition naming**: `direct_messages_YYYY_MM`

**Maintenance**:
- `IHostedService` startup: Tạo partition cho tháng hiện tại + 2 tháng tới
- Hangfire job ngày 25: Tạo partition tháng tới
- Safeguard: Repository throw `InvalidOperationException` nếu query không có `SentAt` filter

---

### TD-U2-05: S3 Chat Archive — AWS SDK for .NET

**Decision**: Dùng AWS SDK for .NET (đã có từ Unit 1 S3 storage) để export chat lên S3.

**S3 key pattern**:
```
chat-archive/
  {year}/
    {month}/
      {roomId}/
        {date}.json
```

**Format**: JSON Lines (1 message per line) — dễ query với Athena sau này.

---

### TD-U2-06: Feature Flags — Custom Implementation

**Decision**: Dùng custom feature flag service (Redis-backed) cho Agora quota disable.

**Alternatives considered**:
- AWS AppConfig: Overkill cho MVP
- LaunchDarkly: Tốn phí
- Environment variables: Không dynamic

**Rationale**:
- Chỉ cần 1-2 flags (`private-call`, `public-stream`)
- Redis đã có — lưu flag state trong Redis
- Dynamic update không cần restart

**Implementation**:
```csharp
public interface IFeatureFlagService {
    Task<bool> IsEnabledAsync(string feature);
    Task SetAsync(string feature, bool enabled);
}
// Redis key: feature_flag:{feature} = "true"/"false"
```

---

## Dependency Summary — Unit 2 Additions

### Backend NuGet Packages

| Package | Version | Purpose |
|---|---|---|
| `Microsoft.AspNetCore.SignalR` | 8.0.x | Real-time hubs (included in ASP.NET Core) |
| `Microsoft.AspNetCore.SignalR.StackExchangeRedis` | 8.0.x | SignalR Redis backplane |
| `Agora.Rtc.Token` | 2.x | Agora token generation |
| `StackExchange.Redis` | 2.7.x | Redis Streams + feature flags (đã có Unit 1) |

### Frontend NPM Packages

| Package | Version | Purpose |
|---|---|---|
| `@microsoft/signalr` | 8.x | SignalR client |
| `agora-rtc-react` | 2.x | Agora RTC React hooks |
| `agora-rtc-sdk-ng` | 4.x | Agora RTC core SDK |

---

## Environment Variables — Unit 2 Additions

```env
# Agora
AGORA_APP_ID=your_agora_app_id
AGORA_APP_CERTIFICATE=your_agora_app_certificate

# Feature Flags (Redis-backed, không cần env var)
# Managed via IFeatureFlagService

# S3 Chat Archive (dùng chung bucket từ Unit 1)
S3_CHAT_ARCHIVE_PREFIX=chat-archive/
```

**`.env.example` updates required**:
```env
AGORA_APP_ID=your_agora_app_id_here
AGORA_APP_CERTIFICATE=your_agora_app_certificate_here
```
