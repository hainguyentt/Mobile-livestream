# Business Rules — Unit 2: Livestream Engine

**Unit**: Unit 2 — Livestream Engine  
**Ngày tạo**: 2026-03-22

---

## BR-LS: LivestreamModule Rules

### BR-LS-01: Host Stream Limit
- **Rule**: Mỗi Host chỉ được có **1 stream đang Live** tại một thời điểm
- **Check**: Trước khi tạo room mới, query `livestream_rooms WHERE host_id = ? AND status = 'Live'`
- **Violation**: HTTP 409 Conflict — "Bạn đang có một stream đang diễn ra"
- **Exception**: Không có — hard limit

### BR-LS-02: Host Role Requirement
- **Rule**: Chỉ user có `Role = Host` mới được bắt đầu stream
- **Violation**: HTTP 403 Forbidden — "Chỉ Host mới có thể phát livestream"

### BR-LS-03: Viewer Age Verification
- **Rule**: Viewer phải đủ 18 tuổi (tính từ `DateOfBirth`) để join phòng
- **Calculation**: `Age = (today - DateOfBirth).TotalDays / 365.25`
- **Violation**: HTTP 403 Forbidden — "Bạn phải đủ 18 tuổi để xem livestream"
- **Edge case**: Nếu `DateOfBirth` null → deny (yêu cầu cập nhật profile)

### BR-LS-04: Block Check on Join
- **Rule**: Viewer không thể join phòng nếu bị Host block (check `BlockList` trong Profiles module)
- **Violation**: HTTP 403 Forbidden — "Bạn không thể tham gia phòng này"
- **Note**: Không tiết lộ lý do cụ thể (privacy)

### BR-LS-05: Room Capacity Limit
- **Rule**: Tối đa **1000 viewers** đang active trong một phòng (MVP)
- **Check**: `Room.ViewerCount >= 1000` → reject join
- **Violation**: HTTP 409 Conflict — "Phòng đã đầy"

### BR-LS-06: Kicked Viewer Rejoin Ban
- **Rule**: Viewer bị kick không thể rejoin phòng đó **cho đến khi stream kết thúc**
- **Check**: Query `kicked_viewers WHERE room_id = ? AND viewer_id = ?`
- **Violation**: HTTP 403 Forbidden — "Bạn đã bị xóa khỏi phòng này"
- **Cleanup**: Record tự động vô hiệu khi `Room.Status = Ended`

### BR-LS-07: Kick Authorization
- **Rule**: Chỉ **Host của phòng** hoặc **Admin** mới được kick viewer
- **Violation**: HTTP 403 Forbidden

### BR-LS-08: Stream Title Validation
- **Rule**: Title bắt buộc, 1–100 ký tự, không được chỉ là whitespace
- **Violation**: HTTP 400 Bad Request

### BR-LS-09: Agora Token TTL
- **Rule**: Agora token có TTL **1 giờ**. Client phải refresh trước khi hết hạn
- **Refresh endpoint**: `GET /api/livestream/rooms/{roomId}/token`
- **Guard**: Chỉ refresh nếu viewer đang có active ViewerSession

---

## BR-PC: PrivateCallModule Rules

### BR-PC-01: Single Pending Request
- **Rule**: Host chỉ có thể có **1 pending call request** tại một thời điểm
- **Behavior**: Nếu Host đã có pending request → request mới bị **auto-reject** ngay lập tức
- **Response**: HTTP 409 — "Host đang bận, vui lòng thử lại sau"

### BR-PC-02: Call Request Timeout
- **Rule**: Call request tự động **timeout sau 30 giây** nếu Host không phản hồi
- **Implementation**: Hangfire delayed job `CheckCallRequestTimeout` scheduled tại `RequestedAt + 30s`
- **Behavior khi timeout**: `Status = TimedOut`, SignalR `CallRejected` event gửi đến Viewer

### BR-PC-03: Viewer Coin Pre-check
- **Rule**: Viewer phải có đủ coin cho **ít nhất 1 billing tick** trước khi gửi request
- **Check**: `CoinBalance >= CoinRatePerTick`
- **Violation**: HTTP 402 Payment Required — "Không đủ coin để thực hiện cuộc gọi"

### BR-PC-04: Billing Tick Interval
- **Rule**: Billing tick xảy ra **mỗi 10 giây** khi call đang active
- **Implementation**: Hangfire recurring job `ProcessBillingTicks` với interval 10 giây
- **Coin deduction**: Trừ từ Viewer's balance qua `ICoinService.DeductCoinsAsync`

### BR-PC-05: Auto-End on Insufficient Coins
- **Rule**: Call **tự động kết thúc ngay lập tức** khi Viewer không đủ coin cho tick tiếp theo
- **Behavior**: `CallSession.EndedBy = System`, SignalR `CallEnded` event gửi cả 2 phía
- **No grace period**: Không có 30 giây grace (Q-B5: A)

### BR-PC-06: Coin Warning Threshold
- **Rule**: Hiển thị cảnh báo khi `CoinBalance < 100` coins
- **Implementation**: Sau mỗi billing tick, check balance → nếu < 100 → SignalR `CoinWarning` event
- **UI behavior**: Hiển thị coin balance real-time trong call screen khi đang trong warning state

### BR-PC-07: Public Stream During Private Call
- **Rule**: Public stream **không bị pause** khi Host đang trong private call
- **Behavior**: Host's Agora video/audio track tắt với public viewers (Agora mute)
- **SignalR**: Broadcast `HostAway` event đến room → viewers thấy placeholder UI
- **Resume**: Khi call kết thúc → Host's track tự động resume

### BR-PC-08: Call Participant Authorization
- **Rule**: Chỉ Viewer và Host trong session mới được end call
- **Violation**: HTTP 403 Forbidden

### BR-PC-09: Agora Mode for Private Call
- **Rule**: Private call dùng Agora **COMMUNICATION mode** (không phải LIVE_BROADCASTING)
- **Reason**: Cả 2 phía đều publish video/audio (peer-to-peer)
- **Token role**: Cả Viewer và Host đều nhận `Publisher` role token

---

## BR-RC: RoomChatModule Rules

### BR-RC-01: Sender Must Be Active Viewer
- **Rule**: Chỉ viewer đang có **active ViewerSession** mới được gửi chat
- **Violation**: HTTP 403 Forbidden — "Bạn không ở trong phòng này"

### BR-RC-02: Kicked Viewer Cannot Chat
- **Rule**: Viewer bị kick không thể gửi chat (check `KickedViewers`)
- **Violation**: HTTP 403 Forbidden

### BR-RC-03: Rate Limit
- **Rule**: Tối đa **3 tin nhắn/giây** per user per room
- **Implementation**: Redis counter với TTL 1 giây (sliding window)
- **Violation**: HTTP 429 Too Many Requests — "Gửi quá nhanh, vui lòng chờ"

### BR-RC-04: Profanity Filter
- **Rule**: Nội dung chat phải qua profanity filter (danh sách từ cấm tiếng Nhật + tiếng Anh)
- **Implementation**: `IChatMessageFilter.FilterAsync(content)` trong Shared module
- **Behavior**: Reject message nếu phát hiện từ cấm
- **Violation**: HTTP 400 Bad Request — "Nội dung không phù hợp"

### BR-RC-05: Message Length
- **Rule**: Tối đa **200 ký tự** per message
- **Violation**: HTTP 400 Bad Request

### BR-RC-06: No Chat History on Join
- **Rule**: Viewer mới join **không thấy tin nhắn cũ** (Q-C4: C)
- **Behavior**: `GetRecentMessages` trả về empty list
- **Rationale**: Giảm tải Redis read, tránh context overload cho viewer mới

### BR-RC-07: Redis Stream TTL
- **Rule**: Redis Stream `room:{roomId}:chat` có TTL **7 ngày**
- **Refresh**: TTL được refresh mỗi khi có message mới (EXPIRE command)
- **Max length**: MAXLEN 1000 (trim oldest entries khi vượt quá)

### BR-RC-08: S3 Archive Job
- **Rule**: `ExportRoomChatToS3` job chạy **hàng ngày lúc 02:00 JST** cho các rooms đã ended
- **Format**: JSON Lines, gzip compressed
- **Path**: `archives/room-chat/{YYYY-MM-DD}/{roomId}.jsonl.gz`

---

## BR-DC: DirectChatModule Rules

### BR-DC-01: Viewer-to-Host Only
- **Rule**: Chỉ **Viewer mới được nhắn tin cho Host** (Q-D1: B)
- **Direction**: Viewer → Host (một chiều về khởi tạo)
- **Host reply**: Host có thể reply trong conversation đã tồn tại
- **Violation**: HTTP 403 Forbidden — "Chỉ viewer mới có thể bắt đầu cuộc trò chuyện"

### BR-DC-02: Block Check
- **Rule**: Không thể gửi tin nhắn nếu **một trong hai bên đã block bên kia**
- **Check**: `IMatchingService.IsBlockedAsync(senderId, recipientId)` (bidirectional)
- **Violation**: HTTP 403 Forbidden — "Không thể gửi tin nhắn"

### BR-DC-03: Block → Hide Conversation
- **Rule**: Khi user A block user B → conversation bị **ẩn với cả 2 phía** (Q-D3: A)
- **Implementation**: Subscribe `UserBlockedEvent` → set `IsHiddenByViewer = true` AND `IsHiddenByHost = true`
- **Cannot send**: Cả 2 không thể gửi tin nhắn mới vào conversation đã hidden

### BR-DC-04: Message Types
- **Rule**: Hỗ trợ **Text** và **Emoji** (Q-D2: B)
- **Text**: Max 1000 ký tự
- **Emoji**: Lưu `EmojiCode` (unicode emoji code, e.g. "U+1F600")
- **No images**: Không hỗ trợ image upload trong MVP

### BR-DC-05: Push Notification on Every Message
- **Rule**: Gửi FCM push notification **mỗi khi có tin nhắn mới** (Q-D4: A)
- **Condition**: Gửi bất kể recipient online hay offline
- **Implementation**: Publish `DirectMessageSentEvent` → Notification module handler

### BR-DC-06: Unique Conversation per Pair
- **Rule**: Mỗi cặp (ViewerId, HostId) chỉ có **1 conversation**
- **Implementation**: Unique constraint `(viewer_id, host_id)` trên `conversations` table
- **Behavior**: `FindOrCreate` pattern — nếu đã tồn tại thì dùng lại

### BR-DC-07: Partition Query Requirement
- **Rule**: Mọi query trên `direct_messages` **phải include `SentAt` range** để kích hoạt partition pruning
- **Minimum range**: Luôn filter `SentAt >= [cutoff]` (default: 30 ngày gần nhất)
- **Rationale**: Tránh full table scan trên partitioned table

### BR-DC-08: Partition Retention
- **Rule**: Partitions cũ hơn **12 tháng** bị drop bởi `DropExpiredDirectChatPartitions` job
- **Schedule**: Ngày 1 hàng tháng lúc 03:00 JST
- **Safety**: Chỉ drop partition nếu `partition_end_date < now - 12 months`

### BR-DC-09: Soft Delete
- **Rule**: Sender có thể xóa tin nhắn của mình (soft delete: `IsDeletedBySender = true`)
- **Visibility**: Tin nhắn bị xóa không hiển thị với sender, nhưng vẫn hiển thị với recipient
- **No hard delete**: Không xóa khỏi DB để đảm bảo audit trail

---

## BR-AGO: Agora Integration Rules

### BR-AGO-01: Free Tier Limits
- **Rule**: Agora Free Tier giới hạn **10,000 phút/tháng** (combined audio + video)
- **Monitoring**: Track usage qua Agora Console API
- **Alert**: Gửi alert khi đạt 80% quota
- **Fallback**: Disable private call feature nếu quota exhausted (feature flag)

### BR-AGO-02: Channel Name Uniqueness
- **Rule**: Channel name phải unique và không được tái sử dụng
- **Format**: `room-{Guid:N}` hoặc `call-{Guid:N}` — đảm bảo uniqueness qua GUID

### BR-AGO-03: Token Expiry Handling
- **Rule**: Client phải refresh Agora token trước khi hết hạn (TTL = 1 giờ)
- **Client responsibility**: Agora SDK callback `onTokenPrivilegeWillExpire` → call refresh API
- **Server**: Chỉ issue token mới nếu session/room vẫn active

---

## Validation Summary

| Module | Rule ID | Severity | HTTP Code |
|---|---|---|---|
| Livestream | BR-LS-01 | Error | 409 |
| Livestream | BR-LS-02 | Error | 403 |
| Livestream | BR-LS-03 | Error | 403 |
| Livestream | BR-LS-04 | Error | 403 |
| Livestream | BR-LS-05 | Error | 409 |
| Livestream | BR-LS-06 | Error | 403 |
| Livestream | BR-LS-07 | Error | 403 |
| Livestream | BR-LS-08 | Validation | 400 |
| PrivateCall | BR-PC-01 | Error | 409 |
| PrivateCall | BR-PC-03 | Error | 402 |
| PrivateCall | BR-PC-05 | System | — |
| PrivateCall | BR-PC-06 | Warning | — |
| RoomChat | BR-RC-03 | Rate Limit | 429 |
| RoomChat | BR-RC-04 | Validation | 400 |
| RoomChat | BR-RC-05 | Validation | 400 |
| DirectChat | BR-DC-01 | Error | 403 |
| DirectChat | BR-DC-02 | Error | 403 |
| DirectChat | BR-DC-04 | Validation | 400 |
