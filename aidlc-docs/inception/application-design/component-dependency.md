# Component Dependencies
# Dự Án: App Livestream Hẹn Hò - Thị Trường Nhật Bản

---

## Dependency Matrix

| Module | Depends On | Communication |
|---|---|---|
| API | Auth, Profiles, Livestream, Chat, Payment, Notification, Leaderboard, Moderation, Admin | DI injection |
| Auth | Shared | Direct call |
| Profiles | Auth, Shared | Direct call + Domain Events |
| Livestream | Auth, Profiles, Payment (Coin), Notification, Leaderboard, Shared | Direct call + Domain Events + SignalR |
| Chat | Auth, Profiles, Shared | Direct call + SignalR |
| Payment | Auth, Shared | Direct call + Domain Events |
| Notification | Auth, Shared | Domain Events + SignalR + FCM |
| Leaderboard | Auth, Payment, Shared | Domain Events + Redis |
| Moderation | Auth, Livestream, Shared | Direct call + Domain Events |
| Admin | Auth, Profiles, Livestream, Payment, Moderation, Leaderboard, Shared | Direct call |
| Shared | — | None (no dependencies) |
| MockServices | — | HTTP (standalone server) |

---

## Communication Patterns

```
+------------------+     REST API      +------------------+
|   PWA (Next.js)  | <---------------> |  LivestreamApp   |
|   pwa/           |                   |  .API            |
+------------------+     SignalR WS    +------------------+
                    <---------------->        |
+------------------+     REST API            |  DI + Domain Events
| Admin Dashboard  | <--------------->       |
|   admin/         |                   +-----+-----+
+------------------+                   |           |
                                   Modules    Shared Kernel
                                       |
                              +--------+--------+
                              |                 |
                         PostgreSQL           Redis
                         (AWS RDS)        (ElastiCache)
                              |
                         +----+----+
                         |         |
                        S3       Agora.io
                    (media)     (video/audio)
```

---

## Module Internal Structure (per module)

Mỗi module tuân theo cấu trúc Clean Architecture nhẹ:

```
LivestreamApp.{Module}/
├── Domain/
│   ├── Entities/          # Domain entities
│   ├── Events/            # Domain events
│   └── Interfaces/        # Repository interfaces
├── Application/
│   ├── Services/          # Application services (orchestration)
│   ├── DTOs/              # Request/Response DTOs
│   └── Validators/        # FluentValidation validators
├── Infrastructure/
│   ├── Repositories/      # EF Core implementations
│   ├── ExternalClients/   # Agora, Stripe, LINE Pay, FCM clients
│   └── Persistence/       # DbContext, migrations (per module)
└── Presentation/
    ├── Controllers/        # REST API controllers
    └── Hubs/              # SignalR Hubs (chỉ Livestream, Chat, Notification)
```

---

## External Dependencies

| External Service | Module(s) | Protocol | Mock Available |
|---|---|---|---|
| PostgreSQL (AWS RDS) | Tất cả modules | EF Core / TCP | Docker local |
| Redis (AWS ElastiCache) | Leaderboard, Notification, Auth (token store) | StackExchange.Redis | Docker local |
| AWS S3 | Profiles (ảnh), Moderation (frames) | AWS SDK | LocalStack |
| AWS SES | Auth (email OTP, password reset) | AWS SDK | LocalStack |
| AWS SNS | Notification (push fallback) | AWS SDK | LocalStack |
| AWS SQS | Background jobs queue | AWS SDK | LocalStack |
| AWS Rekognition | Moderation (video analysis) | AWS SDK | LocalStack |
| Agora.io | Livestream (video/audio) | Agora SDK | Agora Free Tier |
| Stripe | Payment | Stripe .NET SDK | MockServices |
| LINE Pay | Payment | HTTP REST | MockServices |
| LINE OAuth | Auth | OAuth 2.0 | LINE Dev account |
| FCM (Firebase) | Notification | Firebase Admin SDK | Firebase test project |
| Twilio / AWS SNS | Auth (SMS OTP) | HTTP / AWS SDK | LocalStack SNS |

---

## Data Flow: Luồng Gửi Quà Ảo (Critical Path)

```
Viewer (PWA)
    |
    | POST /api/payment/gifts/send
    v
PaymentController
    |
    v
PaymentOrchestrationService.SendGiftAsync()
    |
    +---> CoinService.HasSufficientCoinsAsync()  --> PostgreSQL
    |
    +---> CoinService.DeductCoinsAsync()          --> PostgreSQL
    |
    +---> GiftRepository.RecordAsync()            --> PostgreSQL
    |
    +---> DomainEvent: GiftSent
              |
              +---> LeaderboardService.RecordGiftAsync()  --> Redis (sorted set)
              |
              +---> LivestreamHub.GiftReceived()          --> SignalR (broadcast to room)
                        |
                        v
                All viewers in room (PWA) — hiển thị gift animation
```

---

## Data Flow: Luồng Private Call Billing (Critical Path)

```
Viewer accepts call
    |
    v
LivestreamOrchestrationService.AcceptCallAsync()
    |
    +---> CoinService.HasSufficientCoinsAsync()
    +---> AgoraTokenService.GetPrivateCallTokenAsync()
    +---> BillingService.StartBillingSessionAsync()
    +---> LivestreamHub.PrivateCallAccepted() --> both parties
    |
    | [Every 10 seconds - Hangfire job]
    v
BillingService.ProcessBillingTickAsync()
    |
    +---> CoinService.DeductCoinsAsync()
    +---> LivestreamHub.CallBillingTick() --> viewer (update UI)
    +---> BillingService.CheckCoinWarningAsync()
              |
              | [if coins < 2 min remaining]
              v
          LivestreamHub.CoinWarning() --> viewer
              |
              | [if coins = 0]
              v
          LivestreamOrchestrationService.EndCallAsync()
```

---

## Security Boundaries

| Boundary | Enforcement |
|---|---|
| PWA → API | JWT Bearer token (every request) |
| Admin Dashboard → API | JWT Bearer token + Admin role claim |
| SignalR connection | JWT token in query string / header |
| Stripe webhook | Stripe-Signature header verification |
| LINE Pay callback | LINE Pay signature verification |
| Module → Module | Direct in-process call (no network boundary in Modular Monolith) |
| API → MockServices | Chỉ trong Development environment (env check) |
