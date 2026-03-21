# Unit of Work
# Dự Án: App Livestream Hẹn Hò - Thị Trường Nhật Bản

**Kiến trúc**: Modular Monolith  
**Tổng số units**: 5  
**Thứ tự phát triển**: Backend API trước → Frontend PWA sau (trong mỗi unit)

---

## UNIT 1: Core Foundation

**Mục tiêu**: Xây dựng nền tảng kỹ thuật cho toàn bộ hệ thống — authentication, user profiles, infrastructure setup, và mock services.

### Scope — Backend
| Module | Nội dung |
|---|---|
| `LivestreamApp.Shared` | Domain primitives, base interfaces, domain events, error codes, APPI helpers |
| `LivestreamApp.Auth` | Đăng ký/đăng nhập email+OTP, LINE OAuth, JWT+refresh token, phone verification, password reset, brute-force protection |
| `LivestreamApp.Profiles` | CRUD hồ sơ, upload ảnh (S3), verified badge |
| `LivestreamApp.API` | ASP.NET Core host, middleware pipeline (auth, rate limiting, CORS, error handling), health checks |
| `LivestreamApp.MockServices` | Stripe Mock Server (~4 man-days), LINE Pay Mock Server, configurable scenarios |

### Scope — Infrastructure
| Item | Nội dung |
|---|---|
| Database | PostgreSQL schema: Users, RefreshTokens, Profiles, Photos, BlockList |
| Docker | `docker-compose.yml`: PostgreSQL, Redis, LocalStack (S3, SES, SNS, SQS), MockServices |
| LocalStack init | Auto-create S3 bucket, SES identity, SNS topics |
| CI/CD skeleton | Dockerfile cho API project |

### Scope — Frontend (sau backend)
| App | Nội dung |
|---|---|
| PWA (`pwa/`) | Next.js project setup, i18n (JP/EN), Đăng ký, Đăng nhập (email + LINE Login), Hồ sơ cá nhân, Upload ảnh |
| Admin (`admin/`) | Next.js project setup, Login page (admin only) |

### Deliverables
- REST API: `/api/auth/*`, `/api/profiles/*`
- MockServices chạy trên port 5001
- Docker Compose hoạt động đầy đủ (1 lệnh `docker-compose up`)
- PWA: Auth flows + Profile screens
- Admin: Login screen

### Definition of Done
- [ ] Đăng ký, đăng nhập, LINE Login hoạt động end-to-end
- [ ] JWT refresh token rotation hoạt động
- [ ] Phone verification flow hoạt động
- [ ] Upload ảnh lên S3 (LocalStack) hoạt động
- [ ] MockServices trả về response đúng cho Stripe và LINE Pay
- [ ] Docker Compose khởi động toàn bộ stack thành công
- [ ] Unit tests cho Auth và Profiles modules (≥80% coverage)

---

## UNIT 2: Livestream Engine

**Mục tiêu**: Xây dựng core livestream functionality — public stream, private call 1-1, real-time chat, và SignalR infrastructure.

**Dependency**: Unit 1 phải hoàn thành trước.

### Scope — Backend
| Module | Nội dung |
|---|---|
| `LivestreamApp.Livestream` | Public stream (start/end/join/leave), Private call (request/accept/reject/end), Agora token generation, Billing service (per-minute), Kick viewer (host + admin), Session stats |
| `LivestreamApp.RoomChat` | Chat real-time trong phòng livestream (Redis Streams, TTL 7 ngày — không persist PostgreSQL), validate roomId với Livestream module, content filter |
| `LivestreamApp.DirectChat` | Chat 1-1 persistent (PostgreSQL partitioned by month, retention 12 tháng), Conversations, read status, block list check với Profiles module |
| SignalR Hubs | `LivestreamHub` (/hubs/livestream), `ChatHub` (/hubs/chat) — full implementation |
| Hangfire Jobs | `ProcessBillingTicks` (mỗi 10s per session), `AnalyzeVideoFrames` placeholder, **`ExportRoomChatToS3` (hàng ngày 02:00 JST)**, **`DropExpiredDirectChatPartitions` (ngày 1 hàng tháng)** |

### Scope — Infrastructure
| Item | Nội dung |
|---|---|
| Database | Schema: Rooms, ViewerSessions, PrivateCallRequests, CallSessions, BillingSessions; **`direct_messages` (PostgreSQL partitioned by month — DirectChat only)**; **`Conversations`** |
| Agora | Agora Free Tier account setup, RTC token generation |
| Redis | SignalR backplane (ElastiCache local via Docker); **Redis Streams `room:{roomId}:chat` (TTL 7 ngày) — RoomChat module** |

### Scope — Frontend (sau backend)
| App | Nội dung |
|---|---|
| PWA | Discovery page (danh sách rooms), Xem livestream public, Gửi chat trong room, Private call UI (request/accept/reject), Video call screen (Agora SDK), Billing ticker UI, Coin warning dialog |

### Deliverables
- REST API: `/api/livestream/*`, `/api/roomchat/*`, `/api/directchat/*`
- SignalR: `LivestreamHub`, `ChatHub` (dùng chung cho RoomChat + DirectChat) fully operational
- Agora integration hoạt động (video/audio)
- Per-minute billing hoạt động
- Room chat (RoomChat module) qua Redis Streams hoạt động (TTL 7 ngày)
- Direct chat (DirectChat module) qua PostgreSQL partitioned hoạt động
- PWA: Livestream screens + Chat

### Definition of Done
- [ ] Host có thể bắt đầu/kết thúc public stream
- [ ] Viewer có thể join/leave room, xem video
- [ ] Chat real-time hoạt động trong phòng (<500ms) — qua Redis Streams + SignalR
- [ ] Private chat 1-1 lưu PostgreSQL partitioned, query đúng partition
- [ ] Room chat TTL 7 ngày hoạt động (Redis EXPIRE)
- [ ] ExportRoomChatToS3 job chạy thành công
- [ ] Private call flow hoạt động end-to-end (request → accept → billing → end)
- [ ] Coin warning và auto-end khi hết coin hoạt động
- [ ] Host kick viewer hoạt động
- [ ] SignalR scale-out qua Redis backplane hoạt động
- [ ] Unit + integration tests (≥80% coverage)

---

## UNIT 3: Coin & Payment

**Mục tiêu**: Xây dựng hệ thống coin, thanh toán (Stripe + LINE Pay), và virtual gifts.

**Dependency**: Unit 1 phải hoàn thành trước. Unit 2 cần hoàn thành để test gift trong livestream.

### Scope — Backend
| Module | Nội dung |
|---|---|
| `LivestreamApp.Payment` | Coin balance management, Nạp coin Stripe (Payment Intent + webhook), Nạp coin LINE Pay (request + confirm + callback), Gói coin (500¥/1000¥/3000¥/5000¥), Virtual gifts (CRUD + send), Transaction history, Idempotency keys, Withdrawal requests (Could Have) |
| Webhook handlers | `/webhooks/stripe` (Stripe-Signature verification), `/webhooks/linepay` |
| Hangfire Jobs | `RetryFailedWebhooks` (mỗi 5 phút) |

### Scope — Infrastructure
| Item | Nội dung |
|---|---|
| Database | Schema: CoinBalances, CoinTransactions, CoinPackages, VirtualGifts, PaymentIntents, WithdrawalRequests |
| MockServices | Stripe Mock + LINE Pay Mock đã có từ Unit 1 — verify integration |

### Scope — Frontend (sau backend)
| App | Nội dung |
|---|---|
| PWA | Coin wallet screen, Nạp coin flow (Stripe Checkout + LINE Pay), Gift panel trong livestream (chọn quà + animation), Transaction history |

### Deliverables
- REST API: `/api/payment/*`, `/api/coins/*`
- Webhook endpoints hoạt động với MockServices
- Gift animation broadcast qua SignalR `LivestreamHub`
- PWA: Payment + Gift screens

### Definition of Done
- [ ] Nạp coin qua Stripe Mock hoạt động end-to-end (bao gồm webhook)
- [ ] Nạp coin qua LINE Pay Mock hoạt động end-to-end
- [ ] Gửi quà trong livestream trừ coin và hiển thị animation
- [ ] Transaction history hiển thị đúng
- [ ] Idempotency: double-click không tạo 2 giao dịch
- [ ] Webhook retry hoạt động khi webhook fail
- [ ] Unit + integration tests (≥80% coverage)

---

## UNIT 4: Social & Discovery

**Mục tiêu**: Xây dựng matching algorithm, leaderboard, notification system, và social features.

**Dependency**: Unit 1, 2, 3 phải hoàn thành trước.

### Scope — Backend
| Module | Nội dung |
|---|---|
| `LivestreamApp.Profiles` (mở rộng) | Matching algorithm (recommendation engine), Search & filter users, Like/Follow/Unfollow |
| `LivestreamApp.Leaderboard` | Host ranking (daily/weekly/monthly), Top gifters per session, Rank badges, Redis sorted sets, Scheduled reset jobs, Admin override |
| `LivestreamApp.Notification` | FCM push notifications, Device token management, Notification settings, In-app via `NotificationHub`, Notification history |
| SignalR Hubs | `NotificationHub` (/hubs/notification) — full implementation |
| Hangfire Jobs | `ResetDailyLeaderboard`, `ResetWeeklyLeaderboard`, `ResetMonthlyLeaderboard` |

### Scope — Infrastructure
| Item | Nội dung |
|---|---|
| Database | Schema: Follows, Likes, NotificationSettings, DeviceTokens, NotificationHistory, LeaderboardSnapshots, RankBadges |
| Redis | Leaderboard sorted sets (daily/weekly/monthly per host) |
| FCM | Firebase project setup, Admin SDK integration |

### Scope — Frontend (sau backend)
| App | Nội dung |
|---|---|
| PWA | Discovery/Recommendation screen, Search & filter UI, Follow/Like buttons, Leaderboard page (3 tabs: daily/weekly/monthly), Push notification permission + handling, Notification settings, In-app notification bell |

### Deliverables
- REST API: `/api/matching/*`, `/api/leaderboard/*`, `/api/notifications/*`
- SignalR: `NotificationHub` fully operational
- Leaderboard reset jobs hoạt động đúng timezone JST
- PWA: Discovery + Leaderboard + Notification screens

### Definition of Done
- [ ] Recommendation algorithm trả về kết quả phù hợp
- [ ] Search & filter hoạt động với các tiêu chí
- [ ] Follow/unfollow và notification khi host livestream
- [ ] Leaderboard hiển thị đúng theo daily/weekly/monthly
- [ ] Top gifters hiển thị real-time trong phòng livestream
- [ ] Push notification gửi thành công qua FCM
- [ ] Leaderboard reset đúng giờ JST
- [ ] Unit + integration tests (≥80% coverage)

---

## UNIT 5: Admin & Moderation

**Mục tiêu**: Xây dựng admin dashboard, content moderation system, và hoàn thiện toàn bộ hệ thống.

**Dependency**: Unit 1, 2, 3, 4 phải hoàn thành trước.

### Scope — Backend
| Module | Nội dung |
|---|---|
| `LivestreamApp.Moderation` | Report submission, AI content analysis (AWS Rekognition/LocalStack), Violation handling (auto-stop stream), Moderation actions (warn/suspend/ban), Audit log, Spam detection |
| `LivestreamApp.Admin` | Admin user management (search/lock/unlock), Admin kick viewer, Force end stream, Financial management (revenue stats, withdrawal approval), Analytics (DAU/MAU, top hosts, CSV export), Virtual gift management |
| Hangfire Jobs | `AnalyzeVideoFrames` (full implementation, mỗi 30s per active stream) |

### Scope — Infrastructure
| Item | Nội dung |
|---|---|
| Database | Schema: Reports, ModerationActions, ModerationAuditLog, AdminActionLog |
| LocalStack | Rekognition mock configuration |

### Scope — Frontend (sau backend)
| App | Nội dung |
|---|---|
| Admin Dashboard (`admin/`) | User management table, Moderation queue (reports list + action panel), Active livestreams monitor + kick viewer, Financial dashboard (revenue charts, withdrawal requests), Analytics charts (DAU/MAU, top hosts), Virtual gift management |

### Deliverables
- REST API: `/api/admin/*`, `/api/moderation/*`
- AI moderation pipeline hoạt động (LocalStack Rekognition)
- Admin Dashboard fully functional
- Toàn bộ hệ thống integrated và tested

### Definition of Done
- [ ] Report submission và moderation queue hoạt động
- [ ] AI frame analysis trigger đúng (mỗi 30s per stream)
- [ ] Auto-stop stream khi phát hiện HIGH violation
- [ ] Admin kick viewer hoạt động real-time qua SignalR
- [ ] Admin lock/unlock account hoạt động
- [ ] Revenue stats và withdrawal approval hoạt động
- [ ] Admin Dashboard fully functional
- [ ] End-to-end integration tests toàn hệ thống
- [ ] Tất cả 15 SECURITY rules verified compliant
