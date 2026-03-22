# Domain Entities — Unit 2: Livestream Engine

**Unit**: Unit 2 — Livestream Engine  
**Modules**: `LivestreamApp.Livestream` + `LivestreamApp.RoomChat` + `LivestreamApp.DirectChat`  
**Ngày tạo**: 2026-03-22

---

## 1. Tổng Quan Domain Model

```
LivestreamRoom (1) ──── (0..N) ViewerSessions
     │
     ├── (1) ──── (0..N) KickedViewers       [ban list per stream]
     └── (1) ──── (0..1) ActivePrivateCall   [host đang trong call]

PrivateCallRequest (1) ──── (0..1) CallSession
CallSession (1) ──── (0..N) BillingTicks

Conversation (1) ──── (0..N) DirectMessages  [partitioned by month]
```

---

## 2. LivestreamModule Entities

### 2.1 LivestreamRoom

**Module**: `LivestreamApp.Livestream`  
**Table**: `livestream_rooms`

| Field | Type | Nullable | Mô tả |
|---|---|---|---|
| `Id` | `Guid` | No | PK |
| `HostId` | `Guid` | No | FK → users.Id |
| `Title` | `string(100)` | No | Tên phòng, bắt buộc |
| `Category` | `RoomCategory` enum | No | Bắt buộc khi tạo |
| `Status` | `RoomStatus` enum | No | `Scheduled` / `Live` / `Ended` |
| `AgoraChannelName` | `string(64)` | No | Unique channel name cho Agora RTC |
| `ViewerCount` | `int` | No | Cached count, cập nhật mỗi 5 giây |
| `PeakViewerCount` | `int` | No | Max viewers đạt được trong session |
| `TotalViewerCount` | `int` | No | Unique viewers tham gia (tích lũy) |
| `StartedAt` | `DateTime?` | Yes | UTC, null khi chưa live |
| `EndedAt` | `DateTime?` | Yes | UTC, null khi đang live |
| `CreatedAt` | `DateTime` | No | UTC |

**Enums**:
```csharp
enum RoomCategory { Talk, Music, Game, Cooking, Study, Other }
enum RoomStatus   { Scheduled, Live, Ended }
```

**Constraints**:
- Mỗi Host chỉ có tối đa 1 room với `Status = Live` tại một thời điểm
- `AgoraChannelName` = `$"room-{Id:N}"` (generated, unique)
- Max 1000 viewers per room (MVP hard limit)

---

### 2.2 ViewerSession

**Module**: `LivestreamApp.Livestream`  
**Table**: `viewer_sessions`  
**Mục đích**: Track viewer join/leave, tính unique viewers, phục vụ stats

| Field | Type | Nullable | Mô tả |
|---|---|---|---|
| `Id` | `Guid` | No | PK |
| `RoomId` | `Guid` | No | FK → livestream_rooms.Id |
| `ViewerId` | `Guid` | No | FK → users.Id |
| `JoinedAt` | `DateTime` | No | UTC |
| `LeftAt` | `DateTime?` | Yes | Null nếu đang xem |
| `WatchDurationSeconds` | `int` | No | Tính khi leave |
| `IsKicked` | `bool` | No | Default: false |

**Constraints**:
- Unique active session: một viewer chỉ có 1 session với `LeftAt = null` per room
- Khi viewer rejoin sau khi leave: tạo session mới

---

### 2.3 KickedViewer

**Module**: `LivestreamApp.Livestream`  
**Table**: `kicked_viewers`  
**Mục đích**: Ban list per stream — viewer bị kick không thể rejoin cho đến khi stream kết thúc

| Field | Type | Nullable | Mô tả |
|---|---|---|---|
| `Id` | `Guid` | No | PK |
| `RoomId` | `Guid` | No | FK → livestream_rooms.Id |
| `ViewerId` | `Guid` | No | FK → users.Id |
| `KickedByUserId` | `Guid` | No | FK → users.Id (Host hoặc Admin) |
| `KickedByRole` | `string(20)` | No | `Host` / `Admin` |
| `Reason` | `string(500)` | Yes | Lý do kick (optional) |
| `KickedAt` | `DateTime` | No | UTC |

**Unique constraint**: `(RoomId, ViewerId)` — mỗi viewer chỉ có 1 kick record per room

---

### 2.4 PrivateCallRequest

**Module**: `LivestreamApp.Livestream`  
**Table**: `private_call_requests`

| Field | Type | Nullable | Mô tả |
|---|---|---|---|
| `Id` | `Guid` | No | PK |
| `ViewerId` | `Guid` | No | FK → users.Id |
| `HostId` | `Guid` | No | FK → users.Id |
| `Status` | `CallRequestStatus` enum | No | `Pending` / `Accepted` / `Rejected` / `Cancelled` / `TimedOut` |
| `CoinRatePerTick` | `int` | No | Coins per 10-second tick (set khi tạo request) |
| `RequestedAt` | `DateTime` | No | UTC |
| `RespondedAt` | `DateTime?` | Yes | UTC, null khi chưa phản hồi |
| `ExpiresAt` | `DateTime` | No | `RequestedAt + 30 seconds` |

**Enum**:
```csharp
enum CallRequestStatus { Pending, Accepted, Rejected, Cancelled, TimedOut }
```

**Business rules**:
- Host chỉ có tối đa 1 `Pending` request tại một thời điểm
- Nếu Host đã có pending request → auto reject request mới với status `Rejected` (reason: "Host busy")
- Hangfire job check expiry mỗi 10 giây → set `TimedOut` nếu quá 30 giây

---

### 2.5 CallSession

**Module**: `LivestreamApp.Livestream`  
**Table**: `call_sessions`

| Field | Type | Nullable | Mô tả |
|---|---|---|---|
| `Id` | `Guid` | No | PK |
| `CallRequestId` | `Guid` | No | FK → private_call_requests.Id (unique) |
| `ViewerId` | `Guid` | No | FK → users.Id |
| `HostId` | `Guid` | No | FK → users.Id |
| `AgoraChannelName` | `string(64)` | No | Unique channel cho private call |
| `Status` | `CallSessionStatus` enum | No | `Active` / `Ended` |
| `CoinRatePerTick` | `int` | No | Coins per 10-second tick |
| `TotalCoinsCharged` | `int` | No | Tổng coins đã trừ |
| `TotalTicks` | `int` | No | Số billing ticks đã xử lý |
| `StartedAt` | `DateTime` | No | UTC |
| `EndedAt` | `DateTime?` | Yes | UTC, null khi đang active |
| `EndedBy` | `string(20)?` | Yes | `Viewer` / `Host` / `System` (hết coin) |

**Enum**:
```csharp
enum CallSessionStatus { Active, Ended }
```

**Constraints**:
- `AgoraChannelName` = `$"call-{Id:N}"`
- Khi `EndedBy = System`: viewer hết coin → auto-end

---

### 2.6 BillingTick

**Module**: `LivestreamApp.Livestream`  
**Table**: `billing_ticks`  
**Mục đích**: Audit trail cho mỗi lần trừ coin trong call

| Field | Type | Nullable | Mô tả |
|---|---|---|---|
| `Id` | `Guid` | No | PK |
| `CallSessionId` | `Guid` | No | FK → call_sessions.Id |
| `TickNumber` | `int` | No | Thứ tự tick (1, 2, 3...) |
| `CoinsCharged` | `int` | No | Coins trừ trong tick này |
| `ViewerBalanceBefore` | `int` | No | Balance trước khi trừ |
| `ViewerBalanceAfter` | `int` | No | Balance sau khi trừ |
| `ProcessedAt` | `DateTime` | No | UTC |
| `IsSuccess` | `bool` | No | False nếu không đủ coin |

---

## 3. RoomChatModule Entities

### 3.1 RoomChatMessage (Redis Stream Entry)

**Module**: `LivestreamApp.RoomChat`  
**Storage**: Redis Stream `room:{roomId}:chat` (TTL 7 ngày)  
**Không có PostgreSQL table**

```csharp
record RoomChatMessage
{
    string MessageId { get; init; }   // Redis Stream entry ID (e.g. "1234567890123-0")
    Guid RoomId { get; init; }
    Guid SenderId { get; init; }
    string SenderDisplayName { get; init; }
    string Content { get; init; }     // Max 200 chars, profanity-filtered
    DateTime SentAt { get; init; }    // UTC
}
```

**Redis key pattern**: `room:{roomId}:chat`  
**TTL**: 7 ngày (EXPIRE set khi tạo stream, refresh khi có message mới)  
**Max length**: MAXLEN 1000 (trim oldest khi vượt quá)

---

## 4. DirectChatModule Entities

### 4.1 Conversation

**Module**: `LivestreamApp.DirectChat`  
**Table**: `conversations`

| Field | Type | Nullable | Mô tả |
|---|---|---|---|
| `Id` | `Guid` | No | PK |
| `ViewerId` | `Guid` | No | FK → users.Id (người gửi — luôn là Viewer) |
| `HostId` | `Guid` | No | FK → users.Id (người nhận — luôn là Host) |
| `LastMessageAt` | `DateTime?` | Yes | UTC, dùng để sort conversation list |
| `LastMessagePreview` | `string(100)?` | Yes | Preview text của tin nhắn cuối |
| `ViewerUnreadCount` | `int` | No | Default: 0 |
| `HostUnreadCount` | `int` | No | Default: 0 |
| `IsHiddenByViewer` | `bool` | No | Default: false — ẩn khi block |
| `IsHiddenByHost` | `bool` | No | Default: false — ẩn khi block |
| `CreatedAt` | `DateTime` | No | UTC |

**Unique constraint**: `(ViewerId, HostId)` — mỗi cặp Viewer-Host chỉ có 1 conversation

---

### 4.2 DirectMessage

**Module**: `LivestreamApp.DirectChat`  
**Table**: `direct_messages` (PostgreSQL partitioned by month)  
**Retention**: 12 tháng

| Field | Type | Nullable | Mô tả |
|---|---|---|---|
| `Id` | `Guid` | No | PK (part of partition key) |
| `ConversationId` | `Guid` | No | FK → conversations.Id |
| `SenderId` | `Guid` | No | FK → users.Id |
| `Content` | `string(1000)` | No | Text content |
| `MessageType` | `MessageType` enum | No | `Text` / `Emoji` |
| `EmojiCode` | `string(50)?` | Yes | Emoji code nếu type = Emoji |
| `IsRead` | `bool` | No | Default: false |
| `ReadAt` | `DateTime?` | Yes | UTC |
| `IsDeletedBySender` | `bool` | No | Default: false (soft delete) |
| `SentAt` | `DateTime` | No | UTC — **partition key** |

**Enum**:
```csharp
enum MessageType { Text, Emoji }
```

**Partitioning**:
```sql
CREATE TABLE direct_messages (
    ...
    sent_at TIMESTAMPTZ NOT NULL
) PARTITION BY RANGE (sent_at);

-- Tạo partition hàng tháng
CREATE TABLE direct_messages_2026_03 
    PARTITION OF direct_messages
    FOR VALUES FROM ('2026-03-01') TO ('2026-04-01');
```

**Hangfire job**: `DropExpiredDirectChatPartitions` — chạy ngày 1 hàng tháng, drop partition > 12 tháng

---

## 5. Value Objects

### 5.1 AgoraToken
```csharp
record AgoraToken(
    string Token,           // RTC token từ Agora SDK
    string ChannelName,     // Channel name
    AgoraRole Role,         // Publisher / Subscriber
    DateTime ExpiresAt      // UTC, default: now + 1 giờ
);

enum AgoraRole { Publisher, Subscriber }
```

### 5.2 CoinRate
```csharp
record CoinRate(int CoinsPerTick, int TickIntervalSeconds = 10)
{
    // CoinsPerMinute = CoinsPerTick * (60 / TickIntervalSeconds)
    int CoinsPerMinute => CoinsPerTick * (60 / TickIntervalSeconds);
}
```

---

## 6. Domain Events

| Event | Module | Trigger | Payload |
|---|---|---|---|
| `StreamStartedEvent` | Livestream | Host bắt đầu stream | `RoomId`, `HostId`, `Title`, `Category`, `StartedAt` |
| `StreamEndedEvent` | Livestream | Host end stream | `RoomId`, `HostId`, `EndedAt`, `TotalViewers`, `PeakViewers` |
| `ViewerJoinedEvent` | Livestream | Viewer join room | `RoomId`, `ViewerId`, `JoinedAt` |
| `ViewerLeftEvent` | Livestream | Viewer leave room | `RoomId`, `ViewerId`, `WatchDurationSeconds` |
| `ViewerKickedEvent` | Livestream | Host/Admin kick viewer | `RoomId`, `ViewerId`, `KickedBy`, `Reason` |
| `CallRequestedEvent` | Livestream | Viewer gửi call request | `RequestId`, `ViewerId`, `HostId`, `RequestedAt` |
| `CallAcceptedEvent` | Livestream | Host accept call | `RequestId`, `SessionId`, `HostId`, `ViewerId` |
| `CallRejectedEvent` | Livestream | Host reject / timeout | `RequestId`, `ViewerId`, `HostId`, `Reason` |
| `CallEndedEvent` | Livestream | Call kết thúc | `SessionId`, `EndedBy`, `TotalCoins`, `DurationSeconds` |
| `CoinWarningEvent` | Livestream | Balance < 100 coins | `SessionId`, `ViewerId`, `CurrentBalance` |
| `RoomChatMessageSentEvent` | RoomChat | Viewer gửi chat | `RoomId`, `SenderId`, `MessageId` |
| `DirectMessageSentEvent` | DirectChat | Viewer gửi DM | `ConversationId`, `MessageId`, `SenderId`, `HostId` |
| `ConversationHiddenEvent` | DirectChat | User block → hide | `ConversationId`, `HiddenByUserId` |

---

## 7. Aggregate Boundaries

| Aggregate Root | Entities thuộc về |
|---|---|
| `LivestreamRoom` | `ViewerSession[]`, `KickedViewer[]` |
| `PrivateCallRequest` | (standalone) |
| `CallSession` | `BillingTick[]` |
| `Conversation` | `DirectMessage[]` (via partition query) |
