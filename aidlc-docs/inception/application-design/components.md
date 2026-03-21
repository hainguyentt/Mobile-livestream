# Components
# Dự Án: App Livestream Hẹn Hò - Thị Trường Nhật Bản

**Kiến trúc**: Modular Monolith — một ASP.NET Core solution, nhiều modules độc lập

---

## Solution Structure

```
LivestreamApp.sln
├── src/
│   ├── LivestreamApp.API/              # Entry point — ASP.NET Core Web API + SignalR
│   ├── LivestreamApp.Auth/             # Module: Authentication & Identity
│   ├── LivestreamApp.Profiles/         # Module: User Profiles & Matching
│   ├── LivestreamApp.Livestream/       # Module: Livestream (Public + Private)
│   ├── LivestreamApp.RoomChat/         # Module: Room chat trong livestream (Redis Streams)
│   ├── LivestreamApp.DirectChat/       # Module: Chat 1-1 giữa users (PostgreSQL partitioned)
│   ├── LivestreamApp.Payment/          # Module: Coin, Stripe, LINE Pay
│   ├── LivestreamApp.Notification/     # Module: Push Notifications
│   ├── LivestreamApp.Leaderboard/      # Module: Ranking & Leaderboard
│   ├── LivestreamApp.Moderation/       # Module: Content Moderation
│   ├── LivestreamApp.Admin/            # Module: Admin Dashboard API
│   └── LivestreamApp.Shared/           # Shared: Domain primitives, interfaces, events
├── mock/
│   └── LivestreamApp.MockServices/     # Mock: Stripe, LINE Pay mock servers
├── frontend/
│   ├── pwa/                            # Next.js PWA (Viewer + Host)
│   └── admin/                          # Next.js Admin Dashboard
└── infra/
    └── docker-compose.yml              # LocalStack, Redis, PostgreSQL, MockServices
```

---

## Backend Modules (Modular Monolith)

### MOD-01: LivestreamApp.API
**Loại**: Entry Point / Host  
**Trách nhiệm**:
- Khởi động ASP.NET Core application
- Đăng ký tất cả modules vào DI container
- Cấu hình middleware pipeline (auth, rate limiting, CORS, error handling)
- Mount SignalR Hubs (`/hubs/livestream`, `/hubs/chat`, `/hubs/notification`)
- Expose REST API endpoints từ tất cả modules
- Health check endpoints

**Interfaces**: HTTP/HTTPS, WebSocket (SignalR)

---

### MOD-02: LivestreamApp.Auth
**Loại**: Domain Module  
**Trách nhiệm**:
- Đăng ký / đăng nhập bằng email + OTP
- LINE OAuth 2.0 integration (LINE Login)
- Google / Apple OAuth (Should Have)
- JWT access token + refresh token generation & rotation
- Xác minh số điện thoại (SMS OTP via Twilio/AWS SNS)
- Password reset flow
- Brute-force protection (account lockout)
- Session management

**Interfaces**: REST `/api/auth/*`

---

### MOD-03: LivestreamApp.Profiles
**Loại**: Domain Module  
**Trách nhiệm**:
- CRUD hồ sơ người dùng (tên, ảnh, giới thiệu, sở thích)
- Upload và quản lý ảnh hồ sơ (S3)
- Matching algorithm (recommendation engine)
- Tìm kiếm và lọc người dùng
- Like / Follow / Unfollow
- Block / Unblock người dùng
- Verified badge management

**Interfaces**: REST `/api/profiles/*`, `/api/matching/*`

---

### MOD-04: LivestreamApp.Livestream
**Loại**: Domain Module  
**Trách nhiệm**:
- Quản lý phòng livestream public (tạo, đóng, trạng thái)
- Quản lý private call 1-1 (request, accept, reject, billing)
- Agora.io token generation (RTC tokens)
- Tính phí theo phút cho private call và public room có phí
- Quản lý danh sách viewer trong phòng
- Kick viewer khỏi phòng (host action)
- Lưu thống kê phiên (tổng quà, viewers, thời lượng)
- SignalR: `LivestreamHub` — join/leave room, viewer list updates

**Interfaces**: REST `/api/livestream/*`, SignalR `LivestreamHub`

---

### MOD-05: LivestreamApp.RoomChat
**Loại**: Domain Module  
**Trách nhiệm**:
- Chat text real-time trong phòng livestream public (gắn với Room lifecycle)
- Lưu trữ qua **Redis Streams** (`room:{roomId}:chat`, TTL 7 ngày) — không persist PostgreSQL
- Broadcast tin nhắn qua SignalR `ChatHub` đến tất cả viewers trong phòng
- Filter tin nhắn vi phạm (blacklist từ `Shared`)
- Background job: batch export sang S3 hàng ngày (analytics archive)
- Phụ thuộc `LivestreamApp.Livestream` để validate roomId còn active

**Interfaces**: REST `/api/roomchat/*`, SignalR `ChatHub` (dùng chung)

---

### MOD-06: LivestreamApp.DirectChat
**Loại**: Domain Module  
**Trách nhiệm**:
- Chat text 1-1 private giữa users đã match hoặc follow
- Lưu trữ qua **PostgreSQL** với table partitioning theo tháng, retention 12 tháng
- Broadcast tin nhắn qua SignalR `ChatHub` đến recipient (nếu online)
- Quản lý Conversations (thread giữa 2 users)
- Trạng thái đã đọc / chưa đọc
- Gửi emoji, sticker
- Xóa tin nhắn
- Filter tin nhắn vi phạm (blacklist từ `Shared`)
- Background job: drop partition cũ hơn 12 tháng
- Tích hợp với block list từ `LivestreamApp.Profiles` (không nhận tin từ user đã block)

**Interfaces**: REST `/api/directchat/*`, SignalR `ChatHub` (dùng chung)

---

### MOD-07: LivestreamApp.Payment
**Loại**: Domain Module  
**Trách nhiệm**:
- Quản lý số dư coin của người dùng
- Nạp coin qua Stripe (Payment Intent flow)
- Nạp coin qua LINE Pay
- Xử lý Stripe webhooks (payment_intent.succeeded, failed)
- Xử lý LINE Pay callbacks
- Gói coin (500¥, 1000¥, 3000¥, 5000¥)
- Trừ coin khi xem livestream, gửi quà, private call
- Lịch sử giao dịch coin
- Yêu cầu rút tiền của host (Could Have)
- Idempotency cho tất cả payment operations

**Interfaces**: REST `/api/payment/*`, `/api/coins/*`, Webhook `/webhooks/stripe`, `/webhooks/linepay`

---

### MOD-08: LivestreamApp.Notification
**Loại**: Domain Module  
**Trách nhiệm**:
- Gửi push notification qua FCM (Firebase Cloud Messaging)
- Quản lý device tokens (đăng ký, hủy đăng ký)
- Notification templates (livestream started, new message, private call request, like/follow)
- Cài đặt tùy chỉnh thông báo per-user
- In-app notification delivery qua SignalR: `NotificationHub`
- Notification history

**Interfaces**: REST `/api/notifications/*`, SignalR `NotificationHub`

---

### MOD-09: LivestreamApp.Leaderboard
**Loại**: Domain Module  
**Trách nhiệm**:
- Tính toán và lưu trữ ranking host (daily/weekly/monthly)
- Top gifters per livestream session
- Rank badge assignment (Top 10, Top 50, Rising Star)
- Leaderboard reset theo chu kỳ (scheduled job)
- Cache leaderboard data trong Redis (TTL 5 phút)
- Admin override (xóa host khỏi leaderboard)

**Interfaces**: REST `/api/leaderboard/*`

---

### MOD-10: LivestreamApp.Moderation
**Loại**: Domain Module  
**Trách nhiệm**:
- Nhận và lưu báo cáo vi phạm từ users
- Tích hợp AWS Rekognition để phân tích video frames
- Escalation logic (≥5 reports → high priority)
- Hành động moderation: cảnh báo, tạm khóa, khóa vĩnh viễn
- Audit log cho tất cả moderation actions
- Spam detection cho báo cáo sai mục đích

**Interfaces**: REST `/api/moderation/*` (internal + admin)

---

### MOD-11: LivestreamApp.Admin
**Loại**: Domain Module  
**Trách nhiệm**:
- Admin API cho Admin Dashboard (Next.js app riêng)
- Quản lý người dùng (search, view, lock/unlock)
- Monitor livestream đang diễn ra
- Remove viewer vi phạm khỏi phòng (admin kick)
- Quản lý tài chính (doanh thu, yêu cầu rút tiền)
- Báo cáo thống kê (DAU/MAU, top hosts, revenue)
- Quản lý quà ảo (CRUD)
- Quản lý leaderboard

**Interfaces**: REST `/api/admin/*` (protected by Admin role)

---

### MOD-12: LivestreamApp.Shared
**Loại**: Shared Kernel  
**Trách nhiệm**:
- Domain primitives (UserId, CoinAmount, StreamId, v.v.)
- Base interfaces (IRepository, IUnitOfWork, IDomainEvent)
- Domain events (UserRegistered, StreamStarted, CoinPurchased, GiftSent, v.v.)
- Common exceptions và error codes
- Extension methods, utilities
- APPI compliance helpers (data anonymization, consent tracking)
- **`IChatMessageFilter`**: interface blacklist filter dùng chung cho cả RoomChat và DirectChat

**Interfaces**: Không expose HTTP — chỉ dùng nội bộ

---

## Frontend Applications

### FE-01: PWA (pwa/)
**Framework**: Next.js 14+ (App Router)  
**Trách nhiệm**:
- Giao diện Viewer: Discovery, xem livestream, gửi quà, private call
- Giao diện Host: Bắt đầu/quản lý stream, nhận call, xem stats
- Authentication flows (email, LINE Login)
- Hồ sơ người dùng, matching, chat
- Coin wallet, nạp coin (Stripe/LINE Pay checkout)
- Leaderboard, notifications
- PWA: installable, offline support cơ bản, push notifications
- i18n: tiếng Nhật + tiếng Anh

### FE-02: Admin Dashboard (admin/)
**Framework**: Next.js 14+ (App Router)  
**Trách nhiệm**:
- Giao diện quản trị cho Admin/Moderator
- User management, content moderation queue
- Livestream monitoring, admin kick viewer
- Financial management, withdrawal approvals
- Analytics & reporting
- Chỉ accessible với Admin/Moderator role

---

## Mock Services

### MOCK-01: LivestreamApp.MockServices
**Loại**: Development/Test Tool  
**Trách nhiệm**:
- Stripe Mock Server: `/mock/stripe/v1/*` (payment intents, confirm, refund, webhook trigger)
- LINE Pay Mock Server: `/mock/linepay/v3/*` (request, confirm, cancel)
- Configurable scenarios: success, decline, timeout, network_error
- Chỉ chạy trong môi trường Development/Test
- Expose port riêng (ví dụ: 5001) để không conflict với backend (5000)
