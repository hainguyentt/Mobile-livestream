# Unit 2: Livestream Engine — Code Generation Summary

**Status**: ✅ Complete  
**Date**: 2026-03-22

---

## Tổng Quan

Unit 2 bổ sung toàn bộ tính năng livestream, private call (video 1-1 có tính phí), và direct chat (nhắn tin 1-1) vào hệ thống LivestreamApp.

---

## Backend — .NET 8 Modules

### LivestreamApp.Livestream
| Layer | Files |
|---|---|
| Domain Entities | `LivestreamRoom`, `ViewerSession`, `KickedViewer`, `PrivateCallRequest`, `CallSession`, `BillingTick`, `AgoraToken`, `CoinRate` |
| Repository Interfaces | `ILivestreamRoomRepository`, `IViewerSessionRepository`, `ICallSessionRepository`, `IBillingTickRepository` |
| Repository Implementations | `LivestreamRoomRepository`, `ViewerSessionRepository`, `CallSessionRepository`, `BillingTickRepository` |
| Service Interfaces | `ILivestreamService`, `IPrivateCallService`, `IViewerCountService`, `IAgoraTokenService`, `IFeatureFlagService`, `IBillingService`, `ICoinWalletService` |
| Service Implementations | `LivestreamService`, `PrivateCallService`, `ViewerCountService`, `AgoraTokenService`, `FeatureFlagService`, `BillingService`, `CoinWalletService` |
| Hangfire Jobs | `ProcessBillingTickJob`, `AutoRejectCallRequestJob`, `CheckAgoraQuotaJob`, `CleanupEndedCallSessionsJob` |

### LivestreamApp.RoomChat
| Layer | Files |
|---|---|
| Domain Entities | `RoomChatMessage` |
| Service Interfaces | `IRoomChatService`, `IChatRateLimitService` |
| Service Implementations | `RoomChatService`, `ChatRateLimitService` |
| Hangfire Jobs | `ExportRoomChatToS3Job` |

### LivestreamApp.DirectChat
| Layer | Files |
|---|---|
| Domain Entities | `Conversation`, `DirectMessage`, `Block` |
| Repository Interfaces | `IConversationRepository`, `IDirectMessageRepository`, `IBlockRepository` |
| Repository Implementations | `ConversationRepository`, `DirectMessageRepository`, `BlockRepository` |
| Service Interfaces | `IDirectChatService` |
| Service Implementations | `DirectChatService` |

---

## Backend — API Layer

### EF Core
- 9 configuration files (Fluent API)
- `AppDbContext` updated với Unit 2 DbSets
- Migration: `Unit2LivestreamEngine` (cần chạy `dotnet ef migrations add`)

### SignalR Hubs
- `LivestreamHub` → `/hubs/livestream`
- `ChatHub` → `/hubs/chat`
- `IConnectionTracker` + `ConnectionTracker` (Redis-backed)

### IHostedServices
- `PartitionMaintenanceService` — dọn dẹp partition PostgreSQL
- `ViewerCountBroadcastService` — broadcast viewer count mỗi 5s
- `MetricsPublisherService` — publish metrics lên CloudWatch

### REST Controllers
- `LivestreamController` — `/api/v1/livestream/rooms`
- `PrivateCallController` — `/api/v1/livestream/calls`
- `DirectChatController` — `/api/v1/direct-chat`

### DTOs & Validators
`CreateRoomRequest`, `GetRoomsQuery`, `RoomResponse`, `CallRequestDto`, `SendMessageRequest`, `GetMessagesQuery`, `ConversationResponse`

### Configuration
- `appsettings.json` — Agora, SignalR, Hangfire sections
- `appsettings.Development.json` — MockMode: true
- `ServiceCollectionExtensions` — `AddSignalRServices()`, `AddLivestreamServices()`
- `Program.cs` — hub mapping, service registration

---

## Tests

### Unit Tests
| File | Module |
|---|---|
| `LivestreamServiceTests.cs` | Livestream |
| `PrivateCallServiceTests.cs` | Private Call |
| `BillingServiceTests.cs` | Billing |
| `RoomChatServiceTests.cs` | RoomChat |
| `DirectChatServiceTests.cs` | DirectChat |

### Integration Tests
| File | Coverage |
|---|---|
| `LivestreamFlowTests.cs` | Create room → join → chat → end |
| `DirectChatFlowTests.cs` | Send message → mark read → block |

---

## Frontend — Next.js 14 PWA (FSD)

### Features
| Feature | Files |
|---|---|
| `features/livestream` | `types.ts`, `useLivestreamStore.ts`, `livestreamApi.ts`, `LivestreamGrid.tsx`, `LivestreamRoom.tsx`, `RoomChatPanel.tsx`, `ViewerCountBadge.tsx`, `GiftPanel.tsx`, `HostControls.tsx` |
| `features/private-call` | `types.ts`, `usePrivateCallStore.ts`, `privateCallApi.ts`, `agoraClient.ts`, `CallRequestModal.tsx`, `CallScreen.tsx`, `CallTimer.tsx`, `BalanceDisplay.tsx`, `CallEndSummary.tsx` |
| `features/direct-chat` | `types.ts`, `useDirectChatStore.ts`, `directChatApi.ts`, `ConversationList.tsx`, `ConversationThread.tsx`, `MessageInput.tsx` |

### SignalR Clients
- `lib/signalr/livestreamHub.ts`
- `lib/signalr/chatHub.ts`

### Pages (App Router)
- `/[locale]/livestream/page.tsx` — Livestream grid
- `/[locale]/livestream/[roomId]/page.tsx` — Room viewer
- `/[locale]/messages/page.tsx` — Conversation list
- `/[locale]/messages/[conversationId]/page.tsx` — Thread

### i18n
- `en.json` — keys: `livestream`, `privateCall`, `directChat`
- `ja.json` — keys: `livestream`, `privateCall`, `directChat`

### Frontend Tests
- `__tests__/features/livestream/LivestreamGrid.test.tsx`
- `__tests__/features/direct-chat/ConversationThread.test.tsx`

---

## Infrastructure

### docker-compose.yml
- Agora env vars (`AGORA_APP_ID`, `AGORA_APP_CERTIFICATE`, `AGORA_MOCK_MODE`)

### .env.example
- `AGORA_APP_ID`, `AGORA_APP_CERTIFICATE`, `AGORA_MOCK_MODE`

---

## Documentation

| File | Description |
|---|---|
| `code/livestream-module-summary.md` | Livestream + Private Call module |
| `code/roomchat-module-summary.md` | RoomChat module |
| `code/directchat-module-summary.md` | DirectChat module |
| `code/api-module-summary.md` | API layer (hubs, controllers, DTOs) |
| `code/pwa-summary.md` | Frontend PWA |
| `code/api-reference.md` | REST + SignalR API reference |
| `code/unit-2-summary.md` | This file |

---

## Pending Actions (Manual)

1. **EF Core Migration**: `dotnet ef migrations add Unit2LivestreamEngine --project app/backend/LivestreamApp.API`
2. **Agora credentials**: Điền `AGORA_APP_ID` và `AGORA_APP_CERTIFICATE` vào `.env`
3. **Install Agora SDK**: `npm install agora-rtc-sdk-ng` trong `app/frontend/pwa`
4. **Install SignalR client**: `npm install @microsoft/signalr` trong `app/frontend/pwa`
