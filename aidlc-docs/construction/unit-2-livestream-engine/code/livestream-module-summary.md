# Livestream Module Summary — Unit 2

**Module**: `LivestreamApp.Livestream`  
**Ngày tạo**: 2026-03-22

## Phạm vi

Module xử lý toàn bộ lifecycle của livestream room, viewer sessions, private calls và billing.

## Cấu trúc

```
LivestreamApp.Livestream/
├── Domain/
│   ├── Entities/
│   │   ├── LivestreamRoom.cs       # Aggregate root
│   │   ├── ViewerSession.cs        # Viewer join/leave tracking
│   │   ├── KickedViewer.cs         # Ban list per stream
│   │   ├── PrivateCallRequest.cs   # Call request lifecycle
│   │   ├── CallSession.cs          # Active call session
│   │   └── BillingTick.cs          # Coin deduction audit
│   └── ValueObjects/
│       ├── AgoraToken.cs           # RTC token + expiry
│       └── CoinRate.cs             # Billing rate
├── Repositories/
│   ├── ILivestreamRoomRepository.cs
│   ├── IViewerSessionRepository.cs
│   ├── ICallSessionRepository.cs
│   └── IBillingTickRepository.cs
├── Services/
│   ├── ILivestreamService.cs / LivestreamService.cs
│   ├── IPrivateCallService.cs / PrivateCallService.cs
│   ├── IViewerCountService.cs / ViewerCountService.cs
│   ├── IAgoraTokenService.cs / AgoraTokenService.cs
│   ├── IFeatureFlagService.cs / FeatureFlagService.cs
│   ├── IBillingService.cs / BillingService.cs
│   └── ICoinWalletService.cs
└── Jobs/
    ├── ProcessBillingTickJob.cs
    ├── AutoRejectCallRequestJob.cs
    ├── CheckAgoraQuotaJob.cs
    └── CleanupEndedCallSessionsJob.cs
```

## Các quyết định thiết kế chính

- Entity pattern: private constructor + static `Create()` factory, EF Core private parameterless constructor
- `ViewerCountService`: Redis INCR/DECR atomic, lazy resync từ DB khi cache miss
- `AgoraTokenService`: MockMode cho dev (`Agora:MockMode: true`), TTL 4 giờ
- `FeatureFlagService`: fail-open (null = enabled), disable private-call khi quota >= 9,000 phút
- `BillingService`: idempotent via `ON CONFLICT DO NOTHING` trên `(call_session_id, tick_number)`
- `ProcessBillingTickJob`: end call khi insufficient balance, retry 3× trước khi fail

## Stories được implement

- US-03-01, US-03-02, US-03-03: Livestream room lifecycle
- US-04-01, US-04-02, US-04-03: Private call flow
