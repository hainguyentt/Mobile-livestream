# Services
# Dự Án: App Livestream Hẹn Hò - Thị Trường Nhật Bản

**Mô hình**: Mỗi Module có Service layer riêng. Orchestration phức tạp qua Domain Events.

---

## Kiến Trúc Service Layer

```
Controller / SignalR Hub
        |
        v
  Application Service   <-- orchestrates use cases
        |
   +---------+
   |         |
Domain    Infrastructure
Service    Service
(business  (external:
 logic)     DB, S3, Agora, etc.)
```

---

## SVC-01: AuthOrchestrationService

**Module**: Auth  
**Mục đích**: Điều phối luồng đăng ký, đăng nhập, OAuth

| Method | Orchestration |
|---|---|
| `RegisterAsync` | EmailOtpService → UserRepository → TokenService → DomainEvent(UserRegistered) |
| `LoginWithLineAsync` | LineOAuthClient → UserRepository (upsert) → TokenService |
| `RefreshTokenAsync` | TokenRepository → TokenService → TokenRepository (rotate) |
| `VerifyPhoneOtpAsync` | SmsOtpService → UserRepository (update verified) → DomainEvent(PhoneVerified) |

---

## SVC-02: ProfileOrchestrationService

**Module**: Profiles  
**Mục đích**: Điều phối profile management và matching

| Method | Orchestration |
|---|---|
| `UploadProfilePhotoAsync` | ImageProcessor → S3Service → ProfileRepository |
| `GetRecommendationsAsync` | RecommendationEngine → BlocklistRepository (filter) → ProfileRepository |
| `FollowUserAsync` | FollowRepository → DomainEvent(UserFollowed) → NotificationService |
| `BlockUserAsync` | BlockRepository → ChatService (archive conversations) |

---

## SVC-03: LivestreamOrchestrationService

**Module**: Livestream  
**Mục đích**: Điều phối vòng đời livestream và billing

| Method | Orchestration |
|---|---|
| `StartPublicStreamAsync` | RoomRepository → AgoraTokenService → DomainEvent(StreamStarted) → NotificationService (notify followers) |
| `JoinRoomAsync` | CoinService (check balance if paid room) → RoomRepository → AgoraTokenService → LivestreamHub (broadcast ViewerJoined) |
| `EndStreamAsync` | BillingService (finalize) → RoomRepository → LeaderboardService (record) → DomainEvent(StreamEnded) |
| `AcceptCallAsync` | CoinService (check balance) → SessionRepository → AgoraTokenService → LivestreamHub (notify both parties) |
| `EndCallAsync` | BillingService (calculate final cost) → CoinService (deduct) → LeaderboardService (record) → SessionRepository |

---

## SVC-04: BillingService

**Module**: Livestream (shared với Payment)  
**Mục đích**: Tính phí real-time cho private call và paid public rooms

| Method | Mô tả |
|---|---|
| `StartBillingSessionAsync` | Khởi tạo billing session, lưu start time và rate/phút |
| `ProcessBillingTickAsync` | Gọi mỗi 10 giây: tính coin tiêu, deduct, broadcast `CallBillingTick` |
| `CheckCoinWarningAsync` | Kiểm tra nếu coin còn < 2 phút → broadcast `CoinWarning` |
| `FinalizeBillingAsync` | Tính tổng cuối, deduct coin còn lại, tạo transaction record |

---

## SVC-05: PaymentOrchestrationService

**Module**: Payment  
**Mục đích**: Điều phối luồng nạp coin end-to-end

| Method | Orchestration |
|---|---|
| `InitiateStripeTopUpAsync` | CoinPackageRepository → StripePaymentService (create intent) → PendingTransactionRepository |
| `HandleStripeWebhookAsync` | WebhookValidator → StripePaymentService → CoinService (add coins) → NotificationService → DomainEvent(CoinsAdded) |
| `InitiateLinePayTopUpAsync` | CoinPackageRepository → LinePayService (request) → PendingTransactionRepository |
| `HandleLinePayCallbackAsync` | LinePayService (confirm) → CoinService (add coins) → NotificationService |
| `SendGiftAsync` | CoinService (check + deduct) → GiftRepository → DomainEvent(GiftSent) → LeaderboardService (record) → LivestreamHub (broadcast animation) |

---

## SVC-06: NotificationOrchestrationService

**Module**: Notification  
**Mục đích**: Điều phối gửi thông báo đa kênh (push + in-app)

| Trigger Event | Action |
|---|---|
| `StreamStarted` | Lấy danh sách followers → FCM push notification (batch) |
| `UserFollowed` | Push notification cho host |
| `PrivateCallRequest` | Push notification + SignalR `NotificationHub` (in-app) |
| `MessageReceived` | Push notification (nếu app đóng) hoặc SignalR (nếu app mở) |
| `CoinsAdded` | SignalR `NotificationHub` → `CoinBalanceUpdated` |
| `RankChanged` | SignalR `NotificationHub` → `LeaderboardRankChanged` |

---

## SVC-07: ModerationOrchestrationService

**Module**: Moderation  
**Mục đích**: Điều phối content moderation workflow

| Method | Orchestration |
|---|---|
| `ProcessVideoFrameAsync` | ContentAnalysisService (Rekognition) → ViolationHandler (nếu vi phạm) |
| `ViolationHandler (HIGH)` | LivestreamService (force end stream) → ModerationService (auto action) → NotificationService (alert moderator) |
| `ViolationHandler (MEDIUM)` | ReportRepository (create auto-report) → NotificationService (alert moderator) |
| `SubmitReportAsync` | ReportRepository → EscalationChecker (≥5 reports → high priority) → NotificationService (alert moderator) |
| `TakeActionAsync` | UserService (apply action) → AuditLogRepository → NotificationService (notify user) |

---

## SVC-08: LeaderboardService

**Module**: Leaderboard  
**Mục đích**: Quản lý ranking với Redis cache

| Method | Mô tả |
|---|---|
| `RecordGiftAsync` | Cập nhật Redis sorted set (host score) + DB record |
| `GetHostLeaderboardAsync` | Đọc từ Redis cache (TTL 5 phút), fallback DB nếu cache miss |
| `ResetLeaderboardAsync` | Scheduled job (Hangfire): archive current period → reset Redis + DB |
| `AssignBadgesAsync` | Sau reset: tính rank → assign badges → DomainEvent(RankChanged) |

---

## Domain Events (Shared Kernel)

| Event | Publisher | Subscribers |
|---|---|---|
| `UserRegistered` | AuthService | NotificationService (welcome email) |
| `PhoneVerified` | AuthService | ProfileService (unlock features) |
| `StreamStarted` | LivestreamService | NotificationService (notify followers) |
| `StreamEnded` | LivestreamService | LeaderboardService (finalize scores) |
| `GiftSent` | PaymentService | LeaderboardService (record), LivestreamHub (animation) |
| `CoinsAdded` | PaymentService | NotificationService (balance update) |
| `UserFollowed` | ProfileService | NotificationService (notify host) |
| `ViolationDetected` | ModerationService | LivestreamService (if HIGH), NotificationService |
| `RankChanged` | LeaderboardService | NotificationService (notify host) |

---

## Background Jobs (Hangfire)

| Job | Schedule | Service |
|---|---|---|
| `ResetDailyLeaderboard` | Mỗi ngày 00:00 JST | LeaderboardService |
| `ResetWeeklyLeaderboard` | Thứ 2 00:00 JST | LeaderboardService |
| `ResetMonthlyLeaderboard` | Ngày 1 hàng tháng 00:00 JST | LeaderboardService |
| `ProcessBillingTicks` | Mỗi 10 giây (per active session) | BillingService |
| `CleanupExpiredTokens` | Mỗi giờ | AuthService |
| `RetryFailedWebhooks` | Mỗi 5 phút | PaymentService |
| `AnalyzeVideoFrames` | Mỗi 30 giây (per active stream) | ModerationService |
