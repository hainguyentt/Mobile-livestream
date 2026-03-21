# Application Design — Tổng Hợp
# Dự Án: App Livestream Hẹn Hò - Thị Trường Nhật Bản

**Ngày tạo**: 2026-03-21  
**Kiến trúc**: Modular Monolith (.NET 8) + Next.js PWA + Next.js Admin

---

## 1. Tổng Quan Kiến Trúc

### 1.1 Quyết Định Kiến Trúc

| Quyết định | Lựa chọn | Lý do |
|---|---|---|
| Backend architecture | Modular Monolith | Đủ phức tạp để cần module boundaries, nhưng chưa cần overhead của microservices ở giai đoạn đầu |
| Frontend | PWA (Next.js) + Admin (Next.js riêng) | PWA cho user-facing, Admin app riêng để deploy độc lập và phân quyền rõ ràng |
| Real-time | ASP.NET Core SignalR (3 Hubs) | Native .NET, scale qua Redis backplane |
| Mock services | Project riêng trong solution | Dễ maintain, không ảnh hưởng production code |

### 1.2 Solution Overview

```
LivestreamApp.sln
├── src/
│   ├── LivestreamApp.API            # Host: ASP.NET Core + SignalR
│   ├── LivestreamApp.Auth           # Module: Authentication
│   ├── LivestreamApp.Profiles       # Module: Profiles & Matching
│   ├── LivestreamApp.Livestream     # Module: Livestream Public + Private
│   ├── LivestreamApp.Chat           # Module: Chat
│   ├── LivestreamApp.Payment        # Module: Coin, Stripe, LINE Pay, Gifts
│   ├── LivestreamApp.Notification   # Module: Push + In-app notifications
│   ├── LivestreamApp.Leaderboard    # Module: Ranking
│   ├── LivestreamApp.Moderation     # Module: Content moderation
│   ├── LivestreamApp.Admin          # Module: Admin API
│   └── LivestreamApp.Shared         # Shared kernel
├── mock/
│   └── LivestreamApp.MockServices   # Stripe Mock + LINE Pay Mock
├── frontend/
│   ├── pwa/                         # Next.js PWA (Viewer + Host)
│   └── admin/                       # Next.js Admin Dashboard
└── infra/
    └── docker-compose.yml           # LocalStack + Redis + PostgreSQL + MockServices
```

---

## 2. Components

Chi tiết: [`components.md`](./components.md)

| ID | Component | Loại | Trách nhiệm chính |
|---|---|---|---|
| MOD-01 | LivestreamApp.API | Entry Point | Host, middleware, DI, SignalR mount |
| MOD-02 | LivestreamApp.Auth | Domain Module | Auth, JWT, LINE OAuth, OTP |
| MOD-03 | LivestreamApp.Profiles | Domain Module | Profile, matching, follow, block |
| MOD-04 | LivestreamApp.Livestream | Domain Module | Public stream, private call, Agora, billing |
| MOD-05 | LivestreamApp.Chat | Domain Module | Room chat, private chat, SignalR |
| MOD-06 | LivestreamApp.Payment | Domain Module | Coin, Stripe, LINE Pay, gifts, withdrawal |
| MOD-07 | LivestreamApp.Notification | Domain Module | FCM push, in-app via SignalR |
| MOD-08 | LivestreamApp.Leaderboard | Domain Module | Ranking, badges, Redis cache |
| MOD-09 | LivestreamApp.Moderation | Domain Module | Reports, AI analysis, actions |
| MOD-10 | LivestreamApp.Admin | Domain Module | Admin API cho dashboard |
| MOD-11 | LivestreamApp.Shared | Shared Kernel | Primitives, interfaces, domain events |
| FE-01 | PWA | Frontend | Viewer + Host UI, PWA |
| FE-02 | Admin Dashboard | Frontend | Admin/Moderator UI |
| MOCK-01 | MockServices | Dev Tool | Stripe Mock + LINE Pay Mock |

---

## 3. SignalR Hubs

| Hub | Endpoint | Events |
|---|---|---|
| `LivestreamHub` | `/hubs/livestream` | ViewerJoined/Left/Kicked, GiftReceived, StreamEnded, CallRequest/Accept/Reject, BillingTick, CoinWarning |
| `ChatHub` | `/hubs/chat` | RoomMessageReceived, PrivateMessageReceived, MessageRead |
| `NotificationHub` | `/hubs/notification` | NotificationReceived, CoinBalanceUpdated, LeaderboardRankChanged |

---

## 4. Service Layer

Chi tiết: [`services.md`](./services.md)

| Service | Module | Pattern |
|---|---|---|
| AuthOrchestrationService | Auth | Orchestration |
| ProfileOrchestrationService | Profiles | Orchestration + Domain Events |
| LivestreamOrchestrationService | Livestream | Orchestration + SignalR |
| BillingService | Livestream | Stateful (per session) + Hangfire |
| PaymentOrchestrationService | Payment | Orchestration + Webhook handling |
| NotificationOrchestrationService | Notification | Event-driven |
| ModerationOrchestrationService | Moderation | Event-driven + Scheduled |
| LeaderboardService | Leaderboard | Redis + Scheduled reset |

**Domain Events**: 9 events kết nối các modules (xem `services.md`)  
**Background Jobs** (Hangfire): 7 jobs (leaderboard reset, billing ticks, cleanup, retry)

---

## 5. Component Dependencies

Chi tiết: [`component-dependency.md`](./component-dependency.md)

**Dependency rule**: Modules chỉ phụ thuộc vào `Shared`. Giao tiếp cross-module qua Domain Events hoặc direct call (trong cùng process). Không có circular dependencies.

**External services**: PostgreSQL, Redis, S3, SES, SNS, SQS, Rekognition (LocalStack khi dev), Agora.io (Free Tier), Stripe (Mock + Test Mode), LINE Pay (Mock), FCM.

---

## 6. Security Architecture

| Layer | Cơ chế |
|---|---|
| Authentication | JWT Bearer (access token 15 phút + refresh token 30 ngày) |
| Authorization | Role-based (User, Host, Admin, Moderator) + Resource-based (owner check) |
| SignalR | JWT token trong connection handshake |
| Webhooks | Signature verification (Stripe-Signature, LINE Pay HMAC) |
| Data | Encryption at rest (RDS + S3) + TLS 1.3 in transit |
| Rate limiting | ASP.NET Core rate limiting middleware (per endpoint) |

---

## 7. Mock Services Strategy

| Service | Dev/Test | Staging | Production |
|---|---|---|---|
| Stripe | MockServices (port 5001) | Stripe Test Mode | Stripe Live |
| LINE Pay | MockServices (port 5001) | LINE Pay Sandbox | LINE Pay Live |
| AWS services | LocalStack (port 4566) | AWS Dev account | AWS Production |
| Agora.io | Agora Free Tier (thật) | Agora Free/Paid | Agora Paid |
