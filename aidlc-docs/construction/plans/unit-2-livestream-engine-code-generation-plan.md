# Code Generation Plan — Unit 2: Livestream Engine

**Unit**: Unit 2 — Livestream Engine  
**Ngày tạo**: 2026-03-22  
**Trạng thái**: ✅ COMPLETE

---

## Unit Context

### Stories Implemented (10 stories)
- US-03-01: Xem danh sách livestream đang hoạt động
- US-03-02: Tham gia và xem livestream
- US-03-03: Host bắt đầu và kết thúc livestream
- US-03-04: Chat trong phòng livestream
- US-03-05: Gửi gift cho Host
- US-04-01: Viewer gửi yêu cầu private call
- US-04-02: Host chấp nhận / từ chối private call
- US-04-03: Thực hiện private video call (Agora)
- US-05-01: Nhắn tin trực tiếp (DirectChat)
- US-05-02: Block / Unblock người dùng

### Modules in Scope
- `LivestreamApp.Livestream` — Livestream rooms, viewer sessions, private calls, billing
- `LivestreamApp.RoomChat` — Room chat via Redis Streams
- `LivestreamApp.DirectChat` — 1-1 messaging, conversations, blocks
- `LivestreamApp.API` — New controllers, hubs, EF configs, Hangfire jobs
- Frontend PWA — Livestream, PrivateCall, DirectChat features (FSD)

### Dependencies
- Unit 1: `LivestreamApp.Shared`, `LivestreamApp.Auth` (User entity), `LivestreamApp.API` (AppDbContext, middleware)

### Code Location
- Backend: `app/backend/`
- Frontend: `app/frontend/pwa/src/features/`
- Tests: `app/tests/`
- Docs: `aidlc-docs/construction/unit-2-livestream-engine/code/`

---

## Execution Sequence

### Phase 1: Shared — Enums & Domain Events (Unit 2)
- [x] Step 1: Create `app/backend/LivestreamApp.Shared/Domain/Enums/RoomCategory.cs`
- [x] Step 2: Create `app/backend/LivestreamApp.Shared/Domain/Enums/RoomStatus.cs`
- [x] Step 3: Create `app/backend/LivestreamApp.Shared/Domain/Enums/CallRequestStatus.cs`
- [x] Step 4: Create `app/backend/LivestreamApp.Shared/Domain/Enums/CallSessionStatus.cs`
- [x] Step 5: Create `app/backend/LivestreamApp.Shared/Domain/Enums/MessageType.cs`
- [x] Step 6: Create `app/backend/LivestreamApp.Shared/Domain/Events/StreamStartedEvent.cs`
- [x] Step 7: Create `app/backend/LivestreamApp.Shared/Domain/Events/StreamEndedEvent.cs`
- [x] Step 8: Create `app/backend/LivestreamApp.Shared/Domain/Events/CallAcceptedEvent.cs`
- [x] Step 9: Create `app/backend/LivestreamApp.Shared/Domain/Events/CallEndedEvent.cs`
- [x] Step 10: Create `app/backend/LivestreamApp.Shared/Domain/Events/DirectMessageSentEvent.cs`

### Phase 2: Livestream Module — Domain Entities
- [x] Step 11: Create `app/backend/LivestreamApp.Livestream/Domain/Entities/LivestreamRoom.cs`
- [x] Step 12: Create `app/backend/LivestreamApp.Livestream/Domain/Entities/ViewerSession.cs`
- [x] Step 13: Create `app/backend/LivestreamApp.Livestream/Domain/Entities/KickedViewer.cs`
- [x] Step 14: Create `app/backend/LivestreamApp.Livestream/Domain/Entities/PrivateCallRequest.cs`
- [x] Step 15: Create `app/backend/LivestreamApp.Livestream/Domain/Entities/CallSession.cs`
- [x] Step 16: Create `app/backend/LivestreamApp.Livestream/Domain/Entities/BillingTick.cs`
- [x] Step 17: Create `app/backend/LivestreamApp.Livestream/Domain/ValueObjects/AgoraToken.cs`
- [x] Step 18: Create `app/backend/LivestreamApp.Livestream/Domain/ValueObjects/CoinRate.cs`

### Phase 3: Livestream Module — Repositories (Interfaces)
- [x] Step 19: Create `app/backend/LivestreamApp.Livestream/Repositories/ILivestreamRoomRepository.cs`
- [x] Step 20: Create `app/backend/LivestreamApp.Livestream/Repositories/IViewerSessionRepository.cs`
- [x] Step 21: Create `app/backend/LivestreamApp.Livestream/Repositories/ICallSessionRepository.cs`
- [x] Step 22: Create `app/backend/LivestreamApp.Livestream/Repositories/IBillingTickRepository.cs`

### Phase 4: Livestream Module — Services (Interfaces)
- [x] Step 23: Create `app/backend/LivestreamApp.Livestream/Services/ILivestreamService.cs`
- [x] Step 24: Create `app/backend/LivestreamApp.Livestream/Services/IPrivateCallService.cs`
- [x] Step 25: Create `app/backend/LivestreamApp.Livestream/Services/IViewerCountService.cs`
- [x] Step 26: Create `app/backend/LivestreamApp.Livestream/Services/IAgoraTokenService.cs`
- [x] Step 27: Create `app/backend/LivestreamApp.Livestream/Services/IFeatureFlagService.cs`
- [x] Step 28: Create `app/backend/LivestreamApp.Livestream/Services/IBillingService.cs`

### Phase 5: Livestream Module — Service Implementations
- [x] Step 29: Create `app/backend/LivestreamApp.Livestream/Services/LivestreamService.cs`
  - `CreateRoomAsync`, `StartStreamAsync`, `EndStreamAsync`
  - `JoinRoomAsync`, `LeaveRoomAsync`, `GetActiveRoomsAsync`
  - `BanViewerAsync`, `GetViewerCountAsync`
- [x] Step 30: Create `app/backend/LivestreamApp.Livestream/Services/PrivateCallService.cs`
  - `RequestCallAsync`, `AcceptCallAsync`, `RejectCallAsync`, `EndCallAsync`
  - `GetAgoraTokenAsync`, `GetCallStatusAsync`
- [x] Step 31: Create `app/backend/LivestreamApp.Livestream/Services/ViewerCountService.cs`
  - Redis INCR/DECR, lazy resync từ DB, bulk get
- [x] Step 32: Create `app/backend/LivestreamApp.Livestream/Services/AgoraTokenService.cs`
  - Token generation (4h TTL), mock mode cho dev, quota check
- [x] Step 33: Create `app/backend/LivestreamApp.Livestream/Services/FeatureFlagService.cs`
  - Redis-backed, fail-open default
- [x] Step 34: Create `app/backend/LivestreamApp.Livestream/Services/BillingService.cs`
  - `ProcessBillingTickAsync` (idempotency via unique constraint)
  - `GetBalanceAsync`, `CheckSufficientBalanceAsync`, `FinalizeCallBillingAsync`

### Phase 6: Livestream Module — Hangfire Jobs
- [x] Step 35: Create `app/backend/LivestreamApp.Livestream/Jobs/ProcessBillingTickJob.cs`
  - Retry 3× (1s, 2s, 4s) → End call on failure
  - Idempotency: ON CONFLICT DO NOTHING
  - Broadcast BalanceUpdated + LowBalanceWarning
- [x] Step 36: Create `app/backend/LivestreamApp.Livestream/Jobs/AutoRejectCallRequestJob.cs`
  - 30s timeout → set status = TimedOut, notify viewer
- [x] Step 37: Create `app/backend/LivestreamApp.Livestream/Jobs/CheckAgoraQuotaJob.cs`
  - Daily: check usage, disable feature flag at 90%, alert at 80%
- [x] Step 38: Create `app/backend/LivestreamApp.Livestream/Jobs/CleanupEndedCallSessionsJob.cs`
  - Daily: cleanup Redis keys for ended sessions

### Phase 7: Livestream Module — Unit Tests
- [x] Step 39: Create `app/tests/LivestreamApp.Tests.Unit/Livestream/LivestreamServiceTests.cs`
  - Test: CreateRoom, StartStream, JoinRoom (eligible/ineligible), BanViewer
- [x] Step 40: Create `app/tests/LivestreamApp.Tests.Unit/Livestream/PrivateCallServiceTests.cs`
  - Test: RequestCall (1 pending limit), AcceptCall, RejectCall, EndCall
- [x] Step 41: Create `app/tests/LivestreamApp.Tests.Unit/Livestream/BillingServiceTests.cs`
  - Test: ProcessBillingTick (success, duplicate, insufficient balance, retry→end)

### Phase 8: Livestream Module — Summary Documentation
- [x] Step 42: Create `aidlc-docs/construction/unit-2-livestream-engine/code/livestream-module-summary.md`

### Phase 9: RoomChat Module — Domain & Service
- [x] Step 43: Create `app/backend/LivestreamApp.RoomChat/Domain/RoomChatMessage.cs` (record)
- [x] Step 44: Create `app/backend/LivestreamApp.RoomChat/Services/IRoomChatService.cs`
- [x] Step 45: Create `app/backend/LivestreamApp.RoomChat/Services/RoomChatService.cs`
  - `SendMessageAsync` (profanity filter, rate limit check, XADD)
  - `GetRecentMessagesAsync` (XRANGE, last N messages)
- [x] Step 46: Create `app/backend/LivestreamApp.RoomChat/Services/IChatRateLimitService.cs`
- [x] Step 47: Create `app/backend/LivestreamApp.RoomChat/Services/ChatRateLimitService.cs`
  - In-memory SlidingWindowRateLimiter: 3 msg/s per user per room
- [x] Step 48: Create `app/backend/LivestreamApp.RoomChat/Jobs/ExportRoomChatToS3Job.cs`
  - Daily: XRANGE → JSON Lines → S3 PutObject → Dead Letter on failure

### Phase 10: RoomChat Module — Unit Tests
- [x] Step 49: Create `app/tests/LivestreamApp.Tests.Unit/RoomChat/RoomChatServiceTests.cs`
  - Test: SendMessage (success, rate limited, banned user, profanity)

### Phase 11: RoomChat Module — Summary Documentation
- [x] Step 50: Create `aidlc-docs/construction/unit-2-livestream-engine/code/roomchat-module-summary.md`

### Phase 12: DirectChat Module — Domain Entities
- [x] Step 51: Create `app/backend/LivestreamApp.DirectChat/Domain/Entities/Conversation.cs`
- [x] Step 52: Create `app/backend/LivestreamApp.DirectChat/Domain/Entities/DirectMessage.cs`
- [x] Step 53: Create `app/backend/LivestreamApp.DirectChat/Domain/Entities/Block.cs`

### Phase 13: DirectChat Module — Services
- [x] Step 54: Create `app/backend/LivestreamApp.DirectChat/Services/IDirectChatService.cs`
- [x] Step 55: Create `app/backend/LivestreamApp.DirectChat/Services/DirectChatService.cs`
  - `GetOrCreateConversationAsync` (block check)
  - `GetConversationsAsync` (filter hidden)
  - `GetMessagesAsync` (required `from` param — partition safeguard)
  - `SendMessageAsync` (rate limit, online check, push fallback)
  - `MarkAsReadAsync`, `BlockUserAsync`, `UnblockUserAsync`

### Phase 14: DirectChat Module — Unit Tests
- [x] Step 56: Create `app/tests/LivestreamApp.Tests.Unit/DirectChat/DirectChatServiceTests.cs`
  - Test: SendMessage (success, blocked, rate limited), BlockUser (hide both sides)

### Phase 15: DirectChat Module — Summary Documentation
- [x] Step 57: Create `aidlc-docs/construction/unit-2-livestream-engine/code/directchat-module-summary.md`

### Phase 16: API — EF Core Configurations
- [x] Step 58: Create `app/backend/LivestreamApp.API/Infrastructure/Configurations/LivestreamRoomConfiguration.cs`
- [x] Step 59: Create `app/backend/LivestreamApp.API/Infrastructure/Configurations/ViewerSessionConfiguration.cs`
- [x] Step 60: Create `app/backend/LivestreamApp.API/Infrastructure/Configurations/KickedViewerConfiguration.cs`
- [x] Step 61: Create `app/backend/LivestreamApp.API/Infrastructure/Configurations/PrivateCallRequestConfiguration.cs`
- [x] Step 62: Create `app/backend/LivestreamApp.API/Infrastructure/Configurations/CallSessionConfiguration.cs`
- [x] Step 63: Create `app/backend/LivestreamApp.API/Infrastructure/Configurations/BillingTickConfiguration.cs`
  - UNIQUE INDEX on (call_session_id, tick_number)
- [x] Step 64: Create `app/backend/LivestreamApp.API/Infrastructure/Configurations/ConversationConfiguration.cs`
- [x] Step 65: Create `app/backend/LivestreamApp.API/Infrastructure/Configurations/DirectMessageConfiguration.cs`
  - Partition by range (sent_at), index on (conversation_id, sent_at)
- [x] Step 66: Create `app/backend/LivestreamApp.API/Infrastructure/Configurations/BlockConfiguration.cs`
- [x] Step 67: Update `app/backend/LivestreamApp.API/Infrastructure/AppDbContext.cs`
  - Add DbSets for all Unit 2 entities
- [x] Step 68: Generate EF Core migration `dotnet ef migrations add Unit2LivestreamEngine`

### Phase 17: API — SignalR Hubs
- [x] Step 69: Create `app/backend/LivestreamApp.API/Hubs/LivestreamHub.cs`
  - `OnConnectedAsync`: JWT validate, join room group + personal group
  - `OnDisconnectedAsync`: leave groups, DECR viewer count, update session
  - `JoinRoom`, `LeaveRoom`, `SendGift`
  - Server→Client: `ViewerJoined`, `ViewerLeft`, `ViewerCountUpdated`, `StreamEnded`, `ViewerBanned`, `GiftReceived`
- [x] Step 70: Create `app/backend/LivestreamApp.API/Hubs/ChatHub.cs`
  - `OnConnectedAsync`: join personal group `direct:{userId}`
  - `SendRoomMessage`, `SendDirectMessage`, `MarkConversationRead`
  - Server→Client: `RoomMessageReceived`, `DirectMessageReceived`, `CallRequest`, `CallAccepted`, `CallRejected`, `CallEnded`, `BalanceUpdated`, `LowBalanceWarning`
- [x] Step 71: Create `app/backend/LivestreamApp.API/Hubs/IConnectionTracker.cs`
- [x] Step 72: Create `app/backend/LivestreamApp.API/Hubs/ConnectionTracker.cs`
  - ConcurrentDictionary: connectionId → (userId, roomId)
  - `GetConnectionCount()` for CloudWatch metric

### Phase 18: API — IHostedServices
- [x] Step 73: Create `app/backend/LivestreamApp.API/HostedServices/PartitionMaintenanceService.cs`
  - Startup: EnsurePartitionExists for current + 2 months ahead
- [x] Step 74: Create `app/backend/LivestreamApp.API/HostedServices/ViewerCountBroadcastService.cs`
  - Every 5s: read viewer counts → broadcast to active room groups
- [x] Step 75: Create `app/backend/LivestreamApp.API/HostedServices/MetricsPublisherService.cs`
  - Every 30s: publish SignalR.ConnectionCount, Agora.UsageMinutes to CloudWatch

### Phase 19: API — REST Controllers
- [x] Step 76: Create `app/backend/LivestreamApp.API/Controllers/V1/LivestreamController.cs`
  - `POST /rooms`, `GET /rooms`, `GET /rooms/{id}`
  - `POST /rooms/{id}/start`, `POST /rooms/{id}/end`
  - `POST /rooms/{id}/join`, `POST /rooms/{id}/leave`
  - `POST /rooms/{id}/ban/{userId}`, `GET /rooms/{id}/viewers`
- [x] Step 77: Create `app/backend/LivestreamApp.API/Controllers/V1/PrivateCallController.cs`
  - `POST /calls/request`, `POST /calls/{id}/accept`, `POST /calls/{id}/reject`
  - `POST /calls/{id}/end`, `GET /calls/{id}/token`, `GET /calls/{id}/status`
- [x] Step 78: Create `app/backend/LivestreamApp.API/Controllers/V1/DirectChatController.cs`
  - `GET /direct-chat/conversations`, `GET /direct-chat/conversations/{id}`
  - `GET /direct-chat/conversations/{id}/messages`
  - `POST /direct-chat/conversations/{id}/read`
  - `POST /direct-chat/block/{userId}`, `DELETE /direct-chat/block/{userId}`

### Phase 20: API — DTOs & Validators
- [x] Step 79: Create `app/backend/LivestreamApp.API/DTOs/Livestream/CreateRoomRequest.cs` + validator
- [x] Step 80: Create `app/backend/LivestreamApp.API/DTOs/Livestream/GetRoomsQuery.cs` (filter, pagination)
- [x] Step 81: Create `app/backend/LivestreamApp.API/DTOs/Livestream/RoomResponse.cs`
- [x] Step 82: Create `app/backend/LivestreamApp.API/DTOs/PrivateCall/CallRequestDto.cs` + validator
- [x] Step 83: Create `app/backend/LivestreamApp.API/DTOs/DirectChat/SendMessageRequest.cs` + validator
- [x] Step 84: Create `app/backend/LivestreamApp.API/DTOs/DirectChat/GetMessagesQuery.cs` (from, to, page)
- [x] Step 85: Create `app/backend/LivestreamApp.API/DTOs/DirectChat/ConversationResponse.cs`

### Phase 21: API — appsettings Updates
- [x] Step 86: Update `app/backend/LivestreamApp.API/appsettings.json`
  - Add: `Agora`, `SignalR`, `Hangfire.BillingQueue` sections
- [x] Step 87: Update `app/backend/LivestreamApp.API/appsettings.Development.json`
  - Add: `Agora.MockMode: true`, SignalR dev config
- [x] Step 88: Update `app/backend/LivestreamApp.API/Extensions/ServiceCollectionExtensions.cs`
  - Register: SignalR + Redis backplane, Hangfire billing queue
  - Register: ILivestreamService, IPrivateCallService, IDirectChatService
  - Register: IBillingService, IAgoraTokenService, IFeatureFlagService
  - Register: IViewerCountService, IChatRateLimitService, IConnectionTracker
  - Register: IHostedServices (Partition, ViewerCount, Metrics)

### Phase 22: API — Integration Tests
- [x] Step 89: Create `app/tests/LivestreamApp.Tests.Integration/Livestream/LivestreamFlowTests.cs`
  - Test: Create room → Start stream → Join → Chat → Leave → End stream
- [x] Step 90: Create `app/tests/LivestreamApp.Tests.Integration/DirectChat/DirectChatFlowTests.cs`
  - Test: Send message → Receive → Mark read → Block → Verify hidden

### Phase 23: API — Summary Documentation
- [x] Step 91: Create `aidlc-docs/construction/unit-2-livestream-engine/code/api-module-summary.md`

### Phase 24: Frontend PWA — SignalR Client Setup
- [x] Step 92: Create `app/frontend/pwa/src/lib/signalr/livestreamHub.ts`
  - HubConnection builder, reconnect policy, event handlers
- [x] Step 93: Create `app/frontend/pwa/src/lib/signalr/chatHub.ts`
  - HubConnection builder, DirectChat + RoomChat event handlers

### Phase 25: Frontend PWA — Livestream Feature (FSD)
- [x] Step 94: Create `app/frontend/pwa/src/features/livestream/model/types.ts`
- [x] Step 95: Create `app/frontend/pwa/src/features/livestream/model/useLivestreamStore.ts`
  - Zustand: activeRoom, viewerCount, chatMessages, isHost
- [x] Step 96: Create `app/frontend/pwa/src/features/livestream/api/livestreamApi.ts`
- [x] Step 97: Create `app/frontend/pwa/src/features/livestream/ui/LivestreamGrid.tsx`
  - Grid layout, sort by viewer count (Q-E1)
- [x] Step 98: Create `app/frontend/pwa/src/features/livestream/ui/LivestreamRoom.tsx`
  - Room view container, video player placeholder
- [x] Step 99: Create `app/frontend/pwa/src/features/livestream/ui/RoomChatPanel.tsx`
  - Chat sidebar, message list, input
- [x] Step 100: Create `app/frontend/pwa/src/features/livestream/ui/ViewerCountBadge.tsx`
- [x] Step 101: Create `app/frontend/pwa/src/features/livestream/ui/GiftPanel.tsx`
- [x] Step 102: Create `app/frontend/pwa/src/features/livestream/ui/HostControls.tsx`
  - End stream, ban viewer controls

### Phase 26: Frontend PWA — Livestream Pages
- [x] Step 103: Create `app/frontend/pwa/src/app/[locale]/livestream/page.tsx`
  - Active rooms grid (Q-E5: tab bottom nav)
- [x] Step 104: Create `app/frontend/pwa/src/app/[locale]/livestream/[roomId]/page.tsx`
  - Room view page, SignalR connection lifecycle

### Phase 27: Frontend PWA — Private Call Feature (FSD)
- [x] Step 105: Create `app/frontend/pwa/src/features/private-call/model/types.ts`
- [x] Step 106: Create `app/frontend/pwa/src/features/private-call/model/usePrivateCallStore.ts`
  - Zustand: callStatus, balance, elapsedSeconds
- [x] Step 107: Create `app/frontend/pwa/src/features/private-call/api/privateCallApi.ts`
- [x] Step 108: Create `app/frontend/pwa/src/features/private-call/api/agoraClient.ts`
  - Agora RTC SDK wrapper, mock mode for dev
- [x] Step 109: Create `app/frontend/pwa/src/features/private-call/ui/CallRequestModal.tsx`
  - Incoming call request (host view), accept/reject
- [x] Step 110: Create `app/frontend/pwa/src/features/private-call/ui/CallScreen.tsx`
  - Active call: video streams, controls (Q-E2: volume, fullscreen, quality, chat toggle, gift)
- [x] Step 111: Create `app/frontend/pwa/src/features/private-call/ui/CallTimer.tsx`
  - Duration + cost per minute display
- [x] Step 112: Create `app/frontend/pwa/src/features/private-call/ui/BalanceDisplay.tsx`
  - Real-time balance, highlight when < 100 coins (Q-E4)
- [x] Step 113: Create `app/frontend/pwa/src/features/private-call/ui/CallEndSummary.tsx`

### Phase 28: Frontend PWA — DirectChat Feature (FSD)
- [x] Step 114: Create `app/frontend/pwa/src/features/direct-chat/model/types.ts`
- [x] Step 115: Create `app/frontend/pwa/src/features/direct-chat/model/useDirectChatStore.ts`
  - Zustand: conversations, messages, unread counts
- [x] Step 116: Create `app/frontend/pwa/src/features/direct-chat/api/directChatApi.ts`
- [x] Step 117: Create `app/frontend/pwa/src/features/direct-chat/ui/ConversationList.tsx`
- [x] Step 118: Create `app/frontend/pwa/src/features/direct-chat/ui/ConversationThread.tsx`
  - Message thread, infinite scroll (load older messages)
- [x] Step 119: Create `app/frontend/pwa/src/features/direct-chat/ui/MessageInput.tsx`
  - Text + emoji reactions (Q-D2)

### Phase 29: Frontend PWA — DirectChat Pages
- [x] Step 120: Create `app/frontend/pwa/src/app/[locale]/messages/page.tsx`
  - Conversation list (Q-E5: accessible từ bottom nav)
- [x] Step 121: Create `app/frontend/pwa/src/app/[locale]/messages/[conversationId]/page.tsx`
  - Message thread page

### Phase 30: Frontend PWA — i18n Updates
- [x] Step 122: Update `app/frontend/pwa/src/i18n/locales/ja.json`
  - Add keys: livestream, privateCall, directChat, gifts
- [x] Step 123: Update `app/frontend/pwa/src/i18n/locales/en.json`
  - Add same keys in English

### Phase 31: Frontend PWA — Unit Tests
- [x] Step 124: Create `app/frontend/pwa/src/__tests__/features/livestream/LivestreamGrid.test.tsx`
- [x] Step 125: Create `app/frontend/pwa/src/__tests__/features/direct-chat/ConversationThread.test.tsx`

### Phase 32: Frontend PWA — Summary Documentation
- [x] Step 126: Create `aidlc-docs/construction/unit-2-livestream-engine/code/pwa-summary.md`

### Phase 33: Infrastructure — docker-compose Updates
- [x] Step 127: Update `docker-compose.yml`
  - Add env vars: `Agora__AppId`, `Agora__AppCertificate`, `Agora__MockMode`
  - Add env vars: `SignalR__MaxConnections`
- [x] Step 128: Update `.env.example`
  - Add: `AGORA_APP_ID`, `AGORA_APP_CERTIFICATE`

### Phase 34: Documentation — API Reference
- [x] Step 129: Create `aidlc-docs/construction/unit-2-livestream-engine/code/api-reference.md`
  - All Unit 2 REST endpoints + SignalR hub events

### Phase 35: Final Verification
- [x] Step 130: Verify all 10 stories implemented
- [x] Step 131: Verify SignalR hubs accessible in dev
- [x] Step 132: Verify Hangfire billing job registered
- [x] Step 133: Verify EF Core migration runs cleanly
- [x] Step 134: Create `aidlc-docs/construction/unit-2-livestream-engine/code/unit-2-summary.md`

---

## Story Traceability

| Story ID | Implemented In | Steps |
|---|---|---|
| US-03-01 (Xem danh sách livestream) | LivestreamService.GetActiveRoomsAsync | 29, 76, 80-81, 97, 103 |
| US-03-02 (Tham gia xem livestream) | LivestreamService.JoinRoomAsync + LivestreamHub | 29, 69, 76, 98, 104 |
| US-03-03 (Host bắt đầu/kết thúc) | LivestreamService.StartStreamAsync/EndStreamAsync | 29, 69, 76, 102, 104 |
| US-03-04 (Chat trong phòng) | RoomChatService + ChatHub | 45, 70, 99 |
| US-03-05 (Gửi gift) | LivestreamHub.SendGift | 69, 101 |
| US-04-01 (Viewer gửi call request) | PrivateCallService.RequestCallAsync | 30, 77, 82, 109 |
| US-04-02 (Host accept/reject call) | PrivateCallService.AcceptCallAsync/RejectCallAsync | 30, 70, 77, 109 |
| US-04-03 (Private video call) | AgoraTokenService + CallScreen | 32, 77, 108, 110 |
| US-05-01 (DirectChat) | DirectChatService + ChatHub | 55, 70, 78, 117-119 |
| US-05-02 (Block/Unblock) | DirectChatService.BlockUserAsync | 55, 78, 119 |

---

## Definition of Done Checklist

- [x] Viewer có thể join room và nhận viewer count real-time qua SignalR
- [x] Room chat hoạt động với rate limiting (3 msg/s)
- [x] Private call flow: request → accept → Agora connect → billing ticks → end
- [x] Billing tick idempotency: không charge 2 lần cho cùng 1 tick
- [x] DirectChat: gửi/nhận tin nhắn, block ẩn conversation cả 2 phía
- [x] PostgreSQL partitions tạo tự động khi startup
- [x] Hangfire billing job chạy mỗi 10 giây per active call
- [x] Unit tests cho Livestream, RoomChat, DirectChat modules
- [x] Docker Compose khởi động với Agora mock mode

---

## Estimated Scope

- **Total Steps**: 134
- **Backend Files**: ~60 files (entities, services, hubs, jobs, configs)
- **Frontend Files**: ~35 files (features, pages, stores)
- **Test Files**: ~10 files
- **Infrastructure Files**: ~3 files (docker-compose, env)
- **Documentation Files**: ~6 files

---

## Key Technical Notes

- **SignalR**: Dùng `[Authorize]` attribute trên cả 2 hubs — JWT required
- **Agora MockMode**: `appsettings.Development.json` → `Agora.MockMode: true` → return fake token
- **Billing idempotency**: `INSERT ... ON CONFLICT (call_session_id, tick_number) DO NOTHING`
- **Partition safeguard**: `DirectMessageRepository.GetMessagesAsync(from: required)` — throw nếu thiếu
- **Redis Streams**: `XADD room:{roomId}:chat MAXLEN ~ 1000 *` — approximate trimming
- **Feature flags**: Redis key `feature_flag:private-call` — fail-open (null = enabled)
- **ConnectionTracker**: `ConcurrentDictionary` in-memory per ECS task — không persist
