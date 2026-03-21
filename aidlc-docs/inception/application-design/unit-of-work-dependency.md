# Unit of Work — Dependency Matrix
# Dự Án: App Livestream Hẹn Hò - Thị Trường Nhật Bản

---

## Dependency Matrix

| Unit | Depends On | Blocking? |
|---|---|---|
| Unit 1: Core Foundation | — (không phụ thuộc) | N/A |
| Unit 2: Livestream Engine | Unit 1 | ✅ Blocking |
| Unit 3: Coin & Payment | Unit 1 | ✅ Blocking |
| Unit 4: Social & Discovery | Unit 1, 2, 3 | ✅ Blocking |
| Unit 5: Admin & Moderation | Unit 1, 2, 3, 4 | ✅ Blocking |

---

## Thứ Tự Phát Triển

```
Unit 1: Core Foundation
    |
    +----------+----------+
    |                     |
Unit 2: Livestream    Unit 3: Coin & Payment
(có thể song song)    (có thể song song)
    |                     |
    +----------+----------+
               |
    Unit 4: Social & Discovery
               |
    Unit 5: Admin & Moderation
               |
           Complete
```

**Lưu ý**: Unit 2 và Unit 3 có thể phát triển **song song** sau khi Unit 1 hoàn thành, vì chúng không phụ thuộc lẫn nhau. Tuy nhiên, integration test của Unit 3 (gift trong livestream) cần Unit 2 hoàn thành.

---

## Integration Points

| Điểm tích hợp | Unit A | Unit B | Mô tả |
|---|---|---|---|
| JWT Auth | Unit 1 | Unit 2, 3, 4, 5 | Tất cả API calls cần JWT từ Auth module |
| User entity | Unit 1 | Unit 2, 3, 4, 5 | Tất cả modules reference UserId từ Profiles |
| CoinService | Unit 3 | Unit 2 | Livestream billing deduct coins từ Payment module |
| GiftSent event | Unit 3 | Unit 2, 4 | Gift animation (Unit 2 SignalR) + Leaderboard score (Unit 4) |
| StreamStarted event | Unit 2 | Unit 4 | Notification gửi đến followers (Unit 4) |
| UserFollowed event | Unit 4 | Unit 1 | Follow data từ Profiles (Unit 1) |
| LeaderboardService | Unit 4 | Unit 2, 3 | Record gift scores từ Unit 3, stream data từ Unit 2 |
| ModerationService | Unit 5 | Unit 2 | Force end stream qua LivestreamService (Unit 2) |
| AdminKickViewer | Unit 5 | Unit 2 | Kick viewer qua LivestreamHub (Unit 2) |
| ReportService | Unit 5 | Unit 1, 2, 3, 4 | Reports có thể liên quan đến bất kỳ module nào |
| **RoomChat → Livestream** | Unit 2 | Unit 2 (internal) | RoomChat validate roomId với LivestreamModule |
| **DirectChat → Profiles** | Unit 2 | Unit 1 | DirectChat check block list với ProfilesModule |

---

## Parallel Development Opportunities

### Unit 2 + Unit 3 (song song sau Unit 1)

| Team A — Unit 2 | Team B — Unit 3 |
|---|---|
| Livestream module | Payment module |
| RoomChat module (Redis Streams) | Coin system |
| DirectChat module (PostgreSQL partitioned) | Stripe Mock integration |
| SignalR LivestreamHub + ChatHub (shared) | LINE Pay Mock integration |
| Agora integration | PWA: Payment screens |
| PWA: Livestream + Chat screens | |

**Điều kiện**: Cần thống nhất interface `ICoinService` từ Unit 1 trước khi bắt đầu song song.

---

## Rollback Strategy

| Unit | Rollback Plan |
|---|---|
| Unit 1 | Drop database, xóa S3 bucket — không ảnh hưởng production |
| Unit 2 | Disable Agora channels, drop Livestream/Chat tables |
| Unit 3 | Disable payment endpoints, refund pending transactions thủ công |
| Unit 4 | Disable recommendation + notification — core features vẫn hoạt động |
| Unit 5 | Disable admin endpoints — không ảnh hưởng user-facing features |

---

## Shared Contracts (cần thống nhất trước khi song song)

Trước khi Unit 2 và Unit 3 bắt đầu song song, cần thống nhất các interfaces sau từ Unit 1:

```csharp
// Từ LivestreamApp.Shared — phải finalize trong Unit 1
public interface ICoinService
{
    Task<bool> HasSufficientCoinsAsync(Guid userId, int requiredAmount);
    Task<CoinBalance> DeductCoinsAsync(Guid userId, int amount, string reason, Guid? referenceId = null);
    Task<CoinBalance> AddCoinsAsync(Guid userId, int amount, string reason, Guid? referenceId = null);
}

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId);
    Task<bool> ExistsAsync(Guid userId);
}

// Dùng chung cho RoomChat và DirectChat
public interface IChatMessageFilter
{
    Task<bool> IsAllowedAsync(string content);
}

// Domain Events — phải finalize trong Unit 1
public record GiftSent(Guid SenderId, Guid HostId, Guid RoomId, Guid GiftId, int CoinAmount, DateTime OccurredAt);
public record StreamStarted(Guid HostId, Guid RoomId, DateTime OccurredAt);
public record UserFollowed(Guid FollowerId, Guid FolloweeId, DateTime OccurredAt);
public record DirectMessageSent(Guid SenderId, Guid RecipientId, Guid ConversationId, DateTime OccurredAt);
```
