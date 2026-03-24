# API Module Summary — Unit 2: Livestream Engine

**Module**: `LivestreamApp.API` (Unit 2 additions)  
**Ngày tạo**: 2026-03-22

## REST Controllers

| Controller | Route | Methods |
|---|---|---|
| `LivestreamController` | `/api/v1/livestream/rooms` | POST, GET, GET/{id}, POST/{id}/start, POST/{id}/end, POST/{id}/join, POST/{id}/leave, POST/{id}/ban/{userId}, GET/{id}/viewers |
| `PrivateCallController` | `/api/v1/livestream/calls` | POST/request, POST/{id}/accept, POST/{id}/reject, POST/{id}/end, GET/{id}/token, GET/{id}/status |
| `DirectChatController` | `/api/v1/direct-chat` | GET/conversations, GET/conversations/{id}, GET/conversations/{id}/messages, POST/conversations/{id}/read, POST/block/{userId}, DELETE/block/{userId} |

## SignalR Hubs

| Hub | Route | Events |
|---|---|---|
| `LivestreamHub` | `/hubs/livestream` | ViewerJoined, ViewerLeft, ViewerCountUpdated, StreamEnded, ViewerBanned, GiftReceived |
| `ChatHub` | `/hubs/chat` | RoomMessageReceived, DirectMessageReceived, CallRequest, CallAccepted, CallRejected, CallEnded, BalanceUpdated, LowBalanceWarning |

## EF Core Configurations

- `LivestreamRoomConfiguration` — partial unique index (host + Live status)
- `ViewerSessionConfiguration` — partial unique index (active session per viewer per room)
- `KickedViewerConfiguration` — unique (room, viewer)
- `PrivateCallRequestConfiguration` — partial unique index (host + Pending status)
- `CallSessionConfiguration` — unique (call_request_id)
- `BillingTickConfiguration` — unique (call_session_id, tick_number) — idempotency
- `ConversationConfiguration` — unique (viewer_id, host_id)
- `DirectMessageConfiguration` — composite PK (id, sent_at) for partitioned table
- `BlockConfiguration` — unique (blocker_id, blocked_id)

## IHostedServices

- `PartitionMaintenanceService` — startup: ensure direct_messages partitions exist
- `ViewerCountBroadcastService` — every 5s: broadcast viewer counts to room groups
- `MetricsPublisherService` — every 30s: publish CloudWatch metrics

## Hangfire Jobs

- `ProcessBillingTickJob` — every 10s per active call session
- `AutoRejectCallRequestJob` — 30s after call request created
- `CheckAgoraQuotaJob` — daily: check Agora usage, disable flag at 90%
- `CleanupEndedCallSessionsJob` — daily: cleanup Redis keys
- `ExportRoomChatToS3Job` — daily 02:00 UTC: export Redis Streams to S3
