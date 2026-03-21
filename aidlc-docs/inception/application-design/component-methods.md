# Component Methods
# Dự Án: App Livestream Hẹn Hò - Thị Trường Nhật Bản

> **Lưu ý**: File này định nghĩa method signatures ở mức high-level.
> Detailed business logic và validation rules sẽ được định nghĩa trong **Functional Design** (Construction Phase).

---

## MOD-02: AuthModule

```csharp
public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request);
    Task<AuthResult> LoginAsync(LoginRequest request);
    Task<AuthResult> LoginWithLineAsync(string authorizationCode);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(Guid userId, string refreshToken);
    Task<bool> SendEmailOtpAsync(string email);
    Task<bool> VerifyEmailOtpAsync(string email, string otp);
    Task<bool> SendPhoneOtpAsync(Guid userId, string phoneNumber);
    Task<bool> VerifyPhoneOtpAsync(Guid userId, string otp);
    Task<bool> RequestPasswordResetAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
}
```

---

## MOD-03: ProfilesModule

```csharp
public interface IProfileService
{
    Task<UserProfile> GetProfileAsync(Guid userId);
    Task<UserProfile> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task<string> UploadProfilePhotoAsync(Guid userId, Stream photoStream, string contentType);
    Task DeleteProfilePhotoAsync(Guid userId, string photoId);
    Task<bool> SetVerifiedBadgeAsync(Guid userId, bool isVerified); // Admin only
}

public interface IMatchingService
{
    Task<PagedResult<UserProfile>> GetRecommendationsAsync(Guid userId, PagingRequest paging);
    Task<PagedResult<UserProfile>> SearchUsersAsync(SearchUsersRequest request, PagingRequest paging);
    Task LikeUserAsync(Guid fromUserId, Guid toUserId);
    Task FollowUserAsync(Guid followerId, Guid followeeId);
    Task UnfollowUserAsync(Guid followerId, Guid followeeId);
    Task BlockUserAsync(Guid blockerId, Guid blockedId);
    Task UnblockUserAsync(Guid blockerId, Guid blockedId);
    Task<bool> IsBlockedAsync(Guid userId, Guid targetId);
}
```

---

## MOD-04: LivestreamModule

```csharp
public interface ILivestreamService
{
    Task<LivestreamRoom> StartPublicStreamAsync(Guid hostId, StartStreamRequest request);
    Task EndStreamAsync(Guid hostId, Guid roomId);
    Task<AgoraToken> GetRtcTokenAsync(Guid userId, Guid roomId, AgoraRole role);
    Task<LivestreamRoom> GetRoomAsync(Guid roomId);
    Task<PagedResult<LivestreamRoom>> GetActiveRoomsAsync(PagingRequest paging);
    Task<JoinRoomResult> JoinRoomAsync(Guid viewerId, Guid roomId);
    Task LeaveRoomAsync(Guid viewerId, Guid roomId);
    Task KickViewerAsync(Guid hostId, Guid roomId, Guid viewerId); // Host action
    Task AdminKickViewerAsync(Guid adminId, Guid roomId, Guid viewerId, string reason); // Admin action
    Task<StreamStats> GetSessionStatsAsync(Guid roomId);
}

public interface IPrivateCallService
{
    Task<CallRequest> SendCallRequestAsync(Guid viewerId, Guid hostId);
    Task<CallSession> AcceptCallAsync(Guid hostId, Guid callRequestId);
    Task RejectCallAsync(Guid hostId, Guid callRequestId);
    Task EndCallAsync(Guid userId, Guid sessionId);
    Task<CallBillingSummary> GetCallSummaryAsync(Guid sessionId);
    Task<AgoraToken> GetPrivateCallTokenAsync(Guid userId, Guid sessionId);
}
```

---

## MOD-05: ChatModule

```csharp
public interface IRoomChatService
{
    Task<ChatMessage> SendRoomMessageAsync(Guid senderId, Guid roomId, string content);
    Task<PagedResult<ChatMessage>> GetRoomMessagesAsync(Guid roomId, PagingRequest paging);
}

public interface IPrivateChatService
{
    Task<ChatMessage> SendPrivateMessageAsync(Guid senderId, Guid recipientId, string content);
    Task<PagedResult<Conversation>> GetConversationsAsync(Guid userId, PagingRequest paging);
    Task<PagedResult<ChatMessage>> GetMessagesAsync(Guid userId, Guid conversationId, PagingRequest paging);
    Task MarkAsReadAsync(Guid userId, Guid conversationId);
    Task DeleteMessageAsync(Guid userId, Guid messageId);
}
```

---

## MOD-06: PaymentModule

```csharp
public interface ICoinService
{
    Task<CoinBalance> GetBalanceAsync(Guid userId);
    Task<CoinBalance> DeductCoinsAsync(Guid userId, int amount, string reason, Guid? referenceId = null);
    Task<CoinBalance> AddCoinsAsync(Guid userId, int amount, string reason, Guid? referenceId = null);
    Task<PagedResult<CoinTransaction>> GetTransactionHistoryAsync(Guid userId, PagingRequest paging);
    Task<bool> HasSufficientCoinsAsync(Guid userId, int requiredAmount);
}

public interface IStripePaymentService
{
    Task<CreatePaymentResult> CreatePaymentIntentAsync(Guid userId, CoinPackage package);
    Task<bool> HandleWebhookAsync(string payload, string signature);
    Task<RefundResult> RefundAsync(string paymentIntentId, int? amount = null);
}

public interface ILinePayService
{
    Task<LinePayRequestResult> RequestPaymentAsync(Guid userId, CoinPackage package);
    Task<bool> ConfirmPaymentAsync(string transactionId, string orderId);
    Task HandleCallbackAsync(LinePayCallbackDto callback);
}

public interface IGiftService
{
    Task<IEnumerable<VirtualGift>> GetAvailableGiftsAsync();
    Task<SendGiftResult> SendGiftAsync(Guid senderId, Guid roomId, Guid giftId);
}

public interface IWithdrawalService  // Could Have
{
    Task<WithdrawalRequest> RequestWithdrawalAsync(Guid hostId, WithdrawalRequestDto request);
    Task<WithdrawalRequest> ApproveWithdrawalAsync(Guid adminId, Guid requestId);
    Task<WithdrawalRequest> RejectWithdrawalAsync(Guid adminId, Guid requestId, string reason);
}
```

---

## MOD-07: NotificationModule

```csharp
public interface INotificationService
{
    Task SendPushAsync(Guid userId, PushNotificationPayload payload);
    Task SendPushToMultipleAsync(IEnumerable<Guid> userIds, PushNotificationPayload payload);
    Task RegisterDeviceTokenAsync(Guid userId, string deviceToken, DevicePlatform platform);
    Task UnregisterDeviceTokenAsync(Guid userId, string deviceToken);
    Task<NotificationSettings> GetSettingsAsync(Guid userId);
    Task UpdateSettingsAsync(Guid userId, UpdateNotificationSettingsRequest request);
}
```

---

## MOD-08: LeaderboardModule

```csharp
public interface ILeaderboardService
{
    Task<PagedResult<LeaderboardEntry>> GetHostLeaderboardAsync(LeaderboardPeriod period, PagingRequest paging);
    Task<LeaderboardEntry?> GetHostRankAsync(Guid hostId, LeaderboardPeriod period);
    Task<IEnumerable<GifterEntry>> GetTopGiftersAsync(Guid roomId, int topN = 3);
    Task RecordGiftAsync(Guid hostId, Guid gifterId, int coinAmount, Guid roomId);
    Task ResetLeaderboardAsync(LeaderboardPeriod period); // Scheduled job
    Task AdminRemoveFromLeaderboardAsync(Guid adminId, Guid hostId, string reason);
}
```

---

## MOD-09: ModerationModule

```csharp
public interface IModerationService
{
    Task<Report> SubmitReportAsync(Guid reporterId, SubmitReportRequest request);
    Task<ModerationAction> TakeActionAsync(Guid moderatorId, TakeActionRequest request);
    Task<PagedResult<Report>> GetPendingReportsAsync(PagingRequest paging);
    Task<PagedResult<ModerationAction>> GetActionHistoryAsync(Guid userId, PagingRequest paging);
}

public interface IContentAnalysisService
{
    Task<ContentAnalysisResult> AnalyzeVideoFrameAsync(byte[] frameBytes);
    Task HandleViolationAsync(Guid roomId, ViolationSeverity severity, ContentAnalysisResult result);
}
```

---

## MOD-10: AdminModule

```csharp
public interface IAdminUserService
{
    Task<PagedResult<AdminUserView>> SearchUsersAsync(AdminSearchRequest request, PagingRequest paging);
    Task<AdminUserDetail> GetUserDetailAsync(Guid userId);
    Task LockAccountAsync(Guid adminId, Guid userId, string reason, DateTime? until = null);
    Task UnlockAccountAsync(Guid adminId, Guid userId);
}

public interface IAdminLivestreamService
{
    Task<PagedResult<ActiveStreamView>> GetActiveStreamsAsync(PagingRequest paging);
    Task<IEnumerable<ViewerInfo>> GetRoomViewersAsync(Guid roomId);
    Task AdminKickViewerAsync(Guid adminId, Guid roomId, Guid viewerId, string reason);
    Task ForceEndStreamAsync(Guid adminId, Guid roomId, string reason);
}

public interface IAdminFinanceService
{
    Task<RevenueStats> GetRevenueStatsAsync(DateRange range);
    Task<PagedResult<WithdrawalRequest>> GetWithdrawalRequestsAsync(WithdrawalStatus? status, PagingRequest paging);
    Task ApproveWithdrawalAsync(Guid adminId, Guid requestId);
    Task RejectWithdrawalAsync(Guid adminId, Guid requestId, string reason);
}

public interface IAdminAnalyticsService
{
    Task<DauMauStats> GetDauMauAsync(DateRange range);
    Task<IEnumerable<TopHostEntry>> GetTopHostsAsync(int topN, DateRange range);
    Task<byte[]> ExportReportAsync(ReportType type, DateRange range); // CSV export
}
```

---

## SignalR Hubs

```csharp
// LivestreamHub — /hubs/livestream
public interface ILivestreamHubClient
{
    Task ViewerJoined(ViewerJoinedEvent evt);
    Task ViewerLeft(ViewerLeftEvent evt);
    Task ViewerKicked(ViewerKickedEvent evt);
    Task GiftReceived(GiftReceivedEvent evt);
    Task StreamEnded(StreamEndedEvent evt);
    Task PrivateCallRequest(CallRequestEvent evt);
    Task PrivateCallAccepted(CallAcceptedEvent evt);
    Task PrivateCallRejected(CallRejectedEvent evt);
    Task CallBillingTick(BillingTickEvent evt);  // mỗi 10 giây
    Task CoinWarning(CoinWarningEvent evt);       // coin sắp hết
}

// ChatHub — /hubs/chat
public interface IChatHubClient
{
    Task RoomMessageReceived(RoomChatMessage message);
    Task PrivateMessageReceived(PrivateChatMessage message);
    Task MessageRead(MessageReadEvent evt);
}

// NotificationHub — /hubs/notification
public interface INotificationHubClient
{
    Task NotificationReceived(InAppNotification notification);
    Task CoinBalanceUpdated(CoinBalanceEvent evt);
    Task LeaderboardRankChanged(RankChangedEvent evt);
}
```
