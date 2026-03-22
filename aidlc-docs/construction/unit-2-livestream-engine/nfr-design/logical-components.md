# Logical Components — Unit 2: Livestream Engine

**Unit**: Unit 2 — Livestream Engine  
**Ngày tạo**: 2026-03-22

---

## 1. Tổng Quan Logical Architecture

```
┌──────────────────────────────────────────────────────────────────────┐
│                         PWA (Next.js)                                │
│                                                                      │
│  LivestreamPages | DirectChatPages | PrivateCallPages                │
│  ┌─────────────────────┐  ┌──────────────────────────────────────┐  │
│  │  Zustand Stores     │  │  Agora RTC SDK (agora-rtc-react)     │  │
│  │  useLivestreamStore │  │  useRTCClient, useLocalMicrophoneTrack│  │
│  │  useDirectChatStore │  │  useLocalCameraTrack                 │  │
│  └─────────────────────┘  └──────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │  @microsoft/signalr — HubConnection                          │   │
│  │  LivestreamHub client | ChatHub client                       │   │
│  └──────────────────────────────────────────────────────────────┘   │
└──────────────────────────────┬───────────────────────────────────────┘
                               │ HTTPS + WSS (TLS 1.3)
┌──────────────────────────────▼───────────────────────────────────────┐
│                    AWS ALB / CloudFront                               │
│         (WebSocket upgrade support, sticky sessions for SignalR)      │
└──────────────────────────────┬───────────────────────────────────────┘
                               │
┌──────────────────────────────▼───────────────────────────────────────┐
│              LivestreamApp.API (ASP.NET Core 8)                       │
│                                                                       │
│  ┌──────────────────────────────────────────────────────────────┐    │
│  │                    Middleware Pipeline                        │    │
│  │  RateLimiter → Auth → CORS → ErrorHandler → Logging          │    │
│  └──────────────────────────────────────────────────────────────┘    │
│                                                                       │
│  ┌─────────────────────┐    ┌──────────────────────────────────────┐ │
│  │  REST Controllers   │    │         SignalR Hubs                 │ │
│  │  ├── Livestream     │    │  ├── LivestreamHub (/hubs/livestream)│ │
│  │  ├── PrivateCall    │    │  └── ChatHub (/hubs/chat)            │ │
│  │  └── DirectChat     │    └──────────────────────────────────────┘ │
│  └──────────┬──────────┘                                             │
│             │                                                         │
│  ┌──────────▼──────────────────────────────────────────────────────┐ │
│  │                    Application Services                          │ │
│  │  ILivestreamService | IPrivateCallService | IDirectChatService  │ │
│  │  IBillingService | IAgoraTokenService | IFeatureFlagService     │ │
│  │  IViewerCountService | IChatRateLimitService                    │ │
│  └──────────┬──────────────────────────────────────────────────────┘ │
│             │                                                         │
│  ┌──────────▼──────────────────────────────────────────────────────┐ │
│  │                    Infrastructure Layer                          │ │
│  │  AppDbContext (EF Core) | RedisService | S3StorageService       │ │
│  │  AgoraTokenGenerator | HangfireJobScheduler                     │ │
│  │  ConnectionTracker | MetricsPublisher                           │ │
│  └──────────────────────────────────────────────────────────────────┘ │
│                                                                       │
│  ┌──────────────────────────────────────────────────────────────────┐ │
│  │                    Background Services                           │ │
│  │  PartitionMaintenanceService (IHostedService)                   │ │
│  │  ViewerCountBroadcastService (IHostedService)                   │ │
│  │  MetricsPublisherService (IHostedService)                       │ │
│  └──────────────────────────────────────────────────────────────────┘ │
└──────────────────────────────┬───────────────────────────────────────┘
                               │
        ┌──────────────────────┼──────────────────────┐
        │                      │                      │
┌───────▼──────┐    ┌──────────▼──────┐    ┌──────────▼──────┐
│  PostgreSQL  │    │     Redis       │    │   AWS S3        │
│  (RDS)       │    │ (ElastiCache)   │    │                 │
│              │    │                 │    │ chat-archive/   │
│  rooms       │    │ viewer_count:*  │    │ {year}/{month}/ │
│  viewer_sess │    │ room:*:chat     │    │ {roomId}/       │
│  call_reqs   │    │ call_session:*  │    │ {date}.jsonl    │
│  call_sess   │    │ feature_flag:*  │    └─────────────────┘
│  billing_tks │    │ agora_quota:*   │
│  direct_msgs │    │ SignalR backpl. │    ┌─────────────────┐
│  convs       │    └─────────────────┘    │  Agora RTC      │
│  blocks      │                           │  (External)     │
│  gifts       │    ┌─────────────────┐    │                 │
└──────────────┘    │  Hangfire Jobs  │    │ Token gen       │
                    │                 │    │ Channel mgmt    │
                    │ ProcessBilling  │    │ Quota API       │
                    │ ExportChatToS3  │    └─────────────────┘
                    │ CreatePartition │
                    │ CheckAgoraQuota │
                    └─────────────────┘
```

---

## 2. Application Services

### 2.1 ILivestreamService

**Trách nhiệm**: Quản lý vòng đời livestream room

| Method | NFR Patterns Applied |
|---|---|
| `CreateRoomAsync(hostId, dto)` | BR-LS-02 (1 active stream), cache invalidation |
| `StartStreamAsync(hostId, roomId)` | Feature flag check, Agora channel create |
| `EndStreamAsync(hostId, roomId)` | Agora revoke, viewer sessions cleanup, export trigger |
| `GetActiveRoomsAsync(filter, page)` | Cache-Aside (Redis, TTL 30s), sort by viewer count |
| `GetRoomAsync(roomId)` | Cache-Aside (Redis, TTL 60s) |
| `JoinRoomAsync(viewerId, roomId)` | BR-LS-03 validation, viewer session insert, INCR |
| `LeaveRoomAsync(viewerId, roomId)` | Viewer session update, DECR |
| `GetViewerCountAsync(roomId)` | Redis INCR/DECR pattern, lazy resync |
| `BanViewerAsync(hostId, viewerId, roomId)` | BR-RC-03, SignalR kick |

### 2.2 IPrivateCallService

**Trách nhiệm**: Private call lifecycle — request, accept, reject, end, billing

| Method | NFR Patterns Applied |
|---|---|
| `RequestCallAsync(viewerId, hostId)` | Feature flag check, BR-PC-01 (1 pending), timeout job |
| `AcceptCallAsync(hostId, requestId)` | Agora token gen (4h TTL), billing tick schedule |
| `RejectCallAsync(hostId, requestId)` | Cancel timeout job, notify viewer |
| `EndCallAsync(userId, sessionId, reason)` | Agora revoke, billing finalize, Redis cleanup |
| `GetAgoraTokenAsync(userId, sessionId)` | Token validation, TTL check |
| `GetCallStatusAsync(sessionId)` | Redis call_session hash |

### 2.3 IDirectChatService

**Trách nhiệm**: 1-1 messaging — conversations, messages, blocks

| Method | NFR Patterns Applied |
|---|---|
| `GetOrCreateConversationAsync(userId, recipientId)` | Block check, visibility rules |
| `GetConversationsAsync(userId, page)` | Filter hidden conversations (BR-DC-03) |
| `GetMessagesAsync(conversationId, from, to)` | Partition safeguard (required `from`) |
| `SendMessageAsync(senderId, conversationId, text)` | Rate limit, online check, push fallback |
| `MarkAsReadAsync(userId, conversationId)` | Unread count reset |
| `BlockUserAsync(blockerId, blockedId)` | Hide conversation both sides (BR-DC-03) |
| `UnblockUserAsync(blockerId, blockedId)` | Restore conversation visibility |

### 2.4 IBillingService

**Trách nhiệm**: Coin deduction, idempotency, balance management

| Method | NFR Patterns Applied |
|---|---|
| `ProcessBillingTickAsync(sessionId, tickNumber)` | Unique constraint idempotency, retry → end call |
| `GetBalanceAsync(userId)` | Redis cache (TTL 30s) |
| `CheckSufficientBalanceAsync(userId, amount)` | Pre-call validation |
| `FinalizeCallBillingAsync(sessionId)` | Calculate total, generate receipt |

**Billing tick execution** (Hangfire job):
```csharp
[AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 1, 2, 4 })]
[Queue("billing")]
public async Task ProcessBillingTickAsync(Guid sessionId, int tickNumber) {
    // 1. Check session active
    // 2. Check balance
    // 3. INSERT billing_ticks ON CONFLICT DO NOTHING
    // 4. IF inserted → deduct coins
    // 5. Broadcast balance update
    // 6. Check low balance warning (< 100 coins)
}
```

### 2.5 IAgoraTokenService

**Trách nhiệm**: Server-side Agora token generation và quota management

```csharp
interface IAgoraTokenService {
    Task<string> GenerateCallTokenAsync(Guid sessionId, Guid userId, AgoraRole role);
    Task<AgoraQuotaStatus> GetQuotaStatusAsync();
    Task RevokeChannelAsync(string channelName);
}
```

**Token generation**:
```csharp
// channelName: "call_{sessionId}" — scoped per session
// TTL: 4 giờ (TD-U2-02)
// Role: Publisher cho cả host và viewer (cả 2 cần send/receive)
```

### 2.6 IFeatureFlagService

**Trách nhiệm**: Dynamic feature enable/disable — Redis-backed

```csharp
interface IFeatureFlagService {
    Task<bool> IsEnabledAsync(string feature);
    Task SetAsync(string feature, bool enabled);
}
// Redis key: feature_flag:{feature}
// Default: enabled (fail-open)
```

### 2.7 IViewerCountService

**Trách nhiệm**: Redis INCR/DECR + lazy resync + broadcast

```csharp
interface IViewerCountService {
    Task<int> IncrementAsync(Guid roomId);
    Task<int> DecrementAsync(Guid roomId);
    Task<int> GetAsync(Guid roomId);           // Lazy resync từ DB nếu cache miss
    Task<Dictionary<Guid, int>> GetBulkAsync(IEnumerable<Guid> roomIds);
}
```

### 2.8 IChatRateLimitService

**Trách nhiệm**: In-memory rate limiting cho chat messages

```csharp
interface IChatRateLimitService {
    bool TryAcquireRoomChat(Guid userId, Guid roomId);   // 3 msg/s
    bool TryAcquireDirectChat(Guid userId);               // 10 msg/min
}
// Implementation: System.Threading.RateLimiting.SlidingWindowRateLimiter
// In-memory per ECS task — không cần Redis (acceptable: per-task limit)
```

---

## 3. SignalR Hubs

### 3.1 LivestreamHub (`/hubs/livestream`)

**Auth**: JWT required (`[Authorize]`)

| Client → Server | Server → Client (Group) | Mô tả |
|---|---|---|
| `JoinRoom(roomId)` | `ViewerJoined` → `room:{roomId}` | Join room group |
| `LeaveRoom(roomId)` | `ViewerLeft` → `room:{roomId}` | Leave room group |
| `SendGift(roomId, giftId)` | `GiftReceived` → `room:{roomId}` | Gửi gift |
| — | `ViewerCountUpdated` → `room:{roomId}` | Broadcast mỗi 5 giây |
| — | `StreamEnded` → `room:{roomId}` | Host end stream |
| — | `ViewerBanned` → `direct:{viewerId}` | Viewer bị ban |

### 3.2 ChatHub (`/hubs/chat`)

**Auth**: JWT required (`[Authorize]`)

| Client → Server | Server → Client (Group) | Mô tả |
|---|---|---|
| `SendRoomMessage(roomId, text)` | `RoomMessageReceived` → `room:{roomId}` | Room chat |
| `SendDirectMessage(recipientId, text)` | `DirectMessageReceived` → `direct:{recipientId}` | 1-1 chat |
| `MarkConversationRead(conversationId)` | `ConversationRead` → `direct:{senderId}` | Read receipt |
| — | `CallRequest` → `direct:{hostId}` | Incoming call request |
| — | `CallAccepted` → `direct:{viewerId}` | Call accepted + Agora token |
| — | `CallRejected` → `direct:{viewerId}` | Call rejected |
| — | `CallEnded` → `call:{sessionId}` | Call ended |
| — | `BalanceUpdated` → `direct:{viewerId}` | Coin deducted |
| — | `LowBalanceWarning` → `direct:{viewerId}` | Balance < 100 coins |

**OnConnectedAsync**:
```csharp
// 1. Validate JWT
// 2. Add to personal group: direct:{userId}
// 3. IConnectionTracker.Add(connectionId, userId, null)
// 4. If query has roomId → add to room:{roomId}
```

---

## 4. Infrastructure Components

### 4.1 PostgreSQL — Unit 2 Tables

| Table | Partition | Key Indexes | Notes |
|---|---|---|---|
| `rooms` | None | `host_id`, `status`, `created_at` | Room metadata |
| `viewer_sessions` | None | `(room_id, user_id)`, `left_at IS NULL` | Active viewers |
| `call_requests` | None | `(viewer_id, status)`, `host_id` | Pending requests |
| `call_sessions` | None | `(host_id, viewer_id)`, `started_at` | Active/ended calls |
| `billing_ticks` | None | `UNIQUE(call_session_id, tick_number)` | Idempotency key |
| `conversations` | None | `(user1_id, user2_id)`, `last_message_at` | DirectChat threads |
| `direct_messages` | Monthly range on `sent_at` | `(conversation_id, sent_at)` | Partitioned |
| `blocks` | None | `UNIQUE(blocker_id, blocked_id)` | Block relationships |
| `gifts` | None | `room_id`, `sender_id` | Gift catalog + history |

**Critical indexes**:
```sql
-- Billing idempotency
CREATE UNIQUE INDEX idx_billing_ticks_session_tick
    ON billing_ticks(call_session_id, tick_number);

-- DirectMessage partition pruning
CREATE INDEX idx_direct_messages_conv_sent
    ON direct_messages(conversation_id, sent_at);

-- Active viewer sessions
CREATE INDEX idx_viewer_sessions_active
    ON viewer_sessions(room_id) WHERE left_at IS NULL;

-- Pending call requests
CREATE INDEX idx_call_requests_pending
    ON call_requests(viewer_id) WHERE status = 'Pending';
```

### 4.2 Redis — Unit 2 Key Patterns

| Key Pattern | Type | TTL | Mục đích |
|---|---|---|---|
| `viewer_count:{roomId}` | STRING | 1 giờ | Atomic viewer counter |
| `room:{roomId}:chat` | STREAM | 7 ngày | Room chat history (MAXLEN ~ 1000) |
| `call_session:{sessionId}` | HASH | 4 giờ | Active call state |
| `feature_flag:{feature}` | STRING | None | Dynamic feature flags |
| `agora_quota:current_month` | STRING | End of month | Agora usage minutes |
| `active_rooms` | SET | 5 phút | Cache of active room IDs |
| `room:info:{roomId}` | STRING (JSON) | 60s | Room metadata cache |

### 4.3 Hangfire Jobs — Unit 2

| Job Class | Queue | Schedule | Retry |
|---|---|---|---|
| `ProcessBillingTickJob` | `billing` | Per active session, mỗi 10s | 3× (1s, 2s, 4s) |
| `ExportRoomChatToS3Job` | `default` | Daily 02:00 UTC | 3× → Dead Letter |
| `CreateNextMonthPartitionJob` | `maintenance` | Ngày 25, 00:00 UTC | 3× |
| `CheckAgoraQuotaJob` | `default` | Daily 00:00 UTC | 3× |
| `AutoRejectCallRequestJob` | `default` | 30s sau khi tạo request | 1× |
| `CleanupEndedCallSessionsJob` | `maintenance` | Daily 03:00 UTC | 3× |

### 4.4 IHostedServices — Unit 2

| Service | Interval | Mục đích |
|---|---|---|
| `PartitionMaintenanceService` | Startup only | Tạo partitions cho current + 2 tháng tới |
| `ViewerCountBroadcastService` | Mỗi 5 giây | Broadcast viewer count đến active rooms |
| `MetricsPublisherService` | Mỗi 30 giây | Publish CloudWatch custom metrics |

---

## 5. REST API Endpoints — Unit 2

### Livestream

```
POST   /api/v1/livestream/rooms                    # Tạo room
GET    /api/v1/livestream/rooms                    # Danh sách active rooms (grid)
GET    /api/v1/livestream/rooms/{roomId}           # Room detail
POST   /api/v1/livestream/rooms/{roomId}/start     # Bắt đầu stream
POST   /api/v1/livestream/rooms/{roomId}/end       # Kết thúc stream
POST   /api/v1/livestream/rooms/{roomId}/join      # Viewer join
POST   /api/v1/livestream/rooms/{roomId}/leave     # Viewer leave
POST   /api/v1/livestream/rooms/{roomId}/ban/{userId}  # Ban viewer
GET    /api/v1/livestream/rooms/{roomId}/viewers   # Viewer list
```

### Private Call

```
POST   /api/v1/livestream/calls/request            # Viewer request call
POST   /api/v1/livestream/calls/{requestId}/accept # Host accept
POST   /api/v1/livestream/calls/{requestId}/reject # Host reject
POST   /api/v1/livestream/calls/{sessionId}/end    # End call
GET    /api/v1/livestream/calls/{sessionId}/token  # Get/refresh Agora token
GET    /api/v1/livestream/calls/{sessionId}/status # Call status
```

### DirectChat

```
GET    /api/v1/direct-chat/conversations           # List conversations
GET    /api/v1/direct-chat/conversations/{id}      # Conversation detail
GET    /api/v1/direct-chat/conversations/{id}/messages  # Messages (requires from param)
POST   /api/v1/direct-chat/conversations/{id}/read # Mark as read
POST   /api/v1/direct-chat/block/{userId}          # Block user
DELETE /api/v1/direct-chat/block/{userId}          # Unblock user
```

---

## 6. Frontend Logical Components (FSD)

### 6.1 Feature: `livestream`

```
src/features/livestream/
├── model/
│   ├── useLivestreamStore.ts      # Zustand: room state, viewer count, chat messages
│   └── types.ts                   # Room, ViewerSession, ChatMessage types
├── api/
│   ├── livestreamApi.ts           # REST calls (join, leave, ban)
│   └── livestreamHub.ts           # SignalR LivestreamHub client
└── ui/
    ├── LivestreamGrid.tsx          # Active rooms grid (Q-E1: sort by viewer count)
    ├── LivestreamRoom.tsx          # Room view container
    ├── VideoPlayer.tsx             # HLS/WebRTC player
    ├── RoomChatPanel.tsx           # Chat sidebar
    ├── ViewerCountBadge.tsx        # Real-time viewer count
    ├── GiftPanel.tsx               # Gift sending UI
    └── HostControls.tsx            # Host-only controls
```

### 6.2 Feature: `private-call`

```
src/features/private-call/
├── model/
│   ├── usePrivateCallStore.ts     # Zustand: call state, timer, balance
│   └── types.ts                   # CallSession, CallRequest types
├── api/
│   ├── privateCallApi.ts          # REST calls (request, accept, reject, end)
│   └── agoraClient.ts             # Agora RTC SDK wrapper
└── ui/
    ├── CallRequestModal.tsx        # Incoming call request (host view)
    ├── CallScreen.tsx              # Active call UI (video + controls)
    ├── CallTimer.tsx               # Duration + cost display
    ├── BalanceDisplay.tsx          # Real-time balance (Q-E4: show when < 100)
    └── CallEndSummary.tsx          # Post-call summary
```

### 6.3 Feature: `direct-chat`

```
src/features/direct-chat/
├── model/
│   ├── useDirectChatStore.ts      # Zustand: conversations, messages, unread counts
│   └── types.ts                   # Conversation, DirectMessage types
├── api/
│   ├── directChatApi.ts           # REST calls
│   └── chatHub.ts                 # SignalR ChatHub client (shared với room chat)
└── ui/
    ├── ConversationList.tsx        # List of conversations
    ├── ConversationThread.tsx      # Message thread view
    ├── MessageInput.tsx            # Text + emoji input
    └── BlockedConversation.tsx     # Blocked state UI
```

### 6.4 Zustand Stores

**useLivestreamStore**:
```typescript
interface LivestreamState {
    activeRoom: Room | null;
    viewerCount: number;
    chatMessages: ChatMessage[];
    isHost: boolean;
    // Actions
    joinRoom: (roomId: string) => Promise<void>;
    leaveRoom: () => Promise<void>;
    sendMessage: (text: string) => void;
    sendGift: (giftId: string) => void;
}
```

**usePrivateCallStore**:
```typescript
interface PrivateCallState {
    activeCall: CallSession | null;
    callStatus: 'idle' | 'requesting' | 'ringing' | 'active' | 'ended';
    balance: number;
    elapsedSeconds: number;
    // Actions
    requestCall: (hostId: string) => Promise<void>;
    acceptCall: (requestId: string) => Promise<void>;
    endCall: () => Promise<void>;
}
```

---

## 7. Security Components — Unit 2

### 7.1 Hub Authorization

```csharp
// LivestreamHub + ChatHub đều require JWT
[Authorize]
public class LivestreamHub : Hub {
    public override async Task OnConnectedAsync() {
        var userId = Context.UserIdentifier; // từ JWT sub claim
        var roomId = Context.GetHttpContext()?.Request.Query["roomId"];
        
        // Add to personal group
        await Groups.AddToGroupAsync(Context.ConnectionId, $"direct:{userId}");
        
        // Add to room group nếu có roomId
        if (roomId.HasValue) {
            await ValidateRoomAccessAsync(userId, roomId);
            await Groups.AddToGroupAsync(Context.ConnectionId, $"room:{roomId}");
        }
        
        _connectionTracker.Add(Context.ConnectionId, userId, roomId);
    }
}
```

### 7.2 Rate Limiting — Unit 2 Additions

```csharp
// Thêm vào RateLimiterMiddleware từ Unit 1
// Room chat: 3 messages/giây per user (in-memory, per hub connection)
// DirectChat: 10 messages/phút per user (in-memory)
// Call request: 1 pending per viewer (DB check, không cần rate limiter)
// Stream start: 1 active per host (DB check)
```

### 7.3 Agora Security

```csharp
// Token generation — server-side only
// App Secret KHÔNG bao giờ expose ra client
// channelName scoped per session: "call_{sessionId}"
// Revocation: Agora API kick all users khi call end
```
