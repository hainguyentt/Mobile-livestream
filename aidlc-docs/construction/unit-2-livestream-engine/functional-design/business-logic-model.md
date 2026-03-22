# Business Logic Model — Unit 2: Livestream Engine

**Unit**: Unit 2 — Livestream Engine  
**Ngày tạo**: 2026-03-22

---

## 1. LivestreamModule — Public Stream

### 1.1 Start Stream Flow

```
Host → StartPublicStream(title, category)
  │
  ├── [Guard] Check: Host không có stream đang Live → Error nếu có
  ├── [Guard] Check: Host role = Host → Error nếu Viewer
  ├── Create LivestreamRoom { Status=Live, AgoraChannelName=generated }
  ├── Generate Agora RTC Token (role=Publisher, TTL=1h)
  ├── Publish StreamStartedEvent
  └── Return { RoomId, AgoraToken, ChannelName }
```

### 1.2 Join Room Flow

```
Viewer → JoinRoom(roomId)
  │
  ├── [Guard] Room.Status = Live → Error nếu Ended
  ├── [Guard] Viewer.DateOfBirth → Age ≥ 18 → Error nếu < 18
  ├── [Guard] KickedViewers.Contains(viewerId) → Error nếu bị kick
  ├── [Guard] Host.BlockList.Contains(viewerId) → Error nếu bị block
  ├── [Guard] Room.ViewerCount < 1000 → Error nếu đầy
  ├── Create ViewerSession { JoinedAt=now }
  ├── Increment ViewerCount (cached, Redis)
  ├── Generate Agora RTC Token (role=Subscriber, TTL=1h)
  ├── Publish ViewerJoinedEvent → SignalR broadcast
  └── Return { AgoraToken, ChannelName }
```

### 1.3 End Stream Flow

```
Host → EndStream(roomId)
  │
  ├── [Guard] Room.HostId = requestingUserId
  ├── Set Room.Status = Ended, Room.EndedAt = now
  ├── Close all active ViewerSessions (LeftAt = now)
  ├── Publish StreamEndedEvent
  ├── SignalR broadcast StreamEnded → all viewers in room group
  │     └── Viewers có 30 giây trước khi bị force-disconnect
  ├── Schedule Hangfire: ExportRoomChatToS3 (nếu có messages)
  └── Return stats { TotalViewers, PeakViewers, Duration }
```

### 1.4 Viewer Count Cache Strategy

```
Redis key: viewer_count:{roomId}
TTL: auto-expire khi stream ended + 1 giờ

Increment: khi ViewerJoined
Decrement: khi ViewerLeft / ViewerKicked
Sync to DB: mỗi 5 giây (Hangfire recurring job hoặc background timer)
```

### 1.5 Kick Viewer Flow

```
Host/Admin → KickViewer(roomId, viewerId, reason?)
  │
  ├── [Guard] Requester = Host của room HOẶC Admin role
  ├── [Guard] Viewer đang trong room (active ViewerSession exists)
  ├── Create KickedViewer record
  ├── Close ViewerSession (LeftAt = now, IsKicked = true)
  ├── Decrement ViewerCount
  ├── Publish ViewerKickedEvent
  └── SignalR: send ViewerKicked event đến viewer bị kick → force disconnect
```

---

## 2. PrivateCallModule — Private Call 1-1

### 2.1 Send Call Request Flow

```
Viewer → SendCallRequest(hostId)
  │
  ├── [Guard] Viewer.Role = Viewer (không phải Host)
  ├── [Guard] Host.Role = Host
  ├── [Guard] Host không bị block bởi Viewer (và ngược lại)
  ├── [Guard] Host không có Pending request khác
  │     └── Nếu có → Return Error "Host đang bận"
  ├── Lookup CoinRate cho private call (config từ system settings)
  ├── [Guard] Viewer.CoinBalance ≥ CoinRate.CoinsPerTick (đủ cho ít nhất 1 tick)
  ├── Create PrivateCallRequest { Status=Pending, ExpiresAt=now+30s }
  ├── Publish CallRequestedEvent
  ├── SignalR: send PrivateCallRequest event đến Host
  ├── Schedule Hangfire: CheckCallRequestTimeout (sau 30 giây)
  └── Return { RequestId, ExpiresAt }
```

### 2.2 Accept Call Flow

```
Host → AcceptCall(requestId)
  │
  ├── [Guard] Request.HostId = requestingUserId
  ├── [Guard] Request.Status = Pending (chưa timeout/cancelled)
  ├── [Guard] Request.ExpiresAt > now
  ├── Set Request.Status = Accepted
  ├── Create CallSession { Status=Active, AgoraChannelName=generated }
  ├── Generate Agora Tokens cho cả Host (Publisher) và Viewer (Publisher)
  │     └── Private call dùng COMMUNICATION mode — cả 2 đều publish
  ├── Publish CallAcceptedEvent
  ├── SignalR: send CallAccepted event đến Viewer (kèm AgoraToken + ChannelName)
  ├── Start Hangfire recurring job: ProcessBillingTicks (mỗi 10 giây)
  └── Return { SessionId, AgoraToken, ChannelName }
```

### 2.3 Reject / Timeout Flow

```
Host → RejectCall(requestId) HOẶC Hangfire timeout job
  │
  ├── Set Request.Status = Rejected / TimedOut
  ├── Publish CallRejectedEvent
  └── SignalR: send CallRejected event đến Viewer
```

### 2.4 Billing Tick Flow (Hangfire, mỗi 10 giây)

```
ProcessBillingTick(sessionId)
  │
  ├── Load CallSession (Status = Active)
  ├── Load Viewer.CoinBalance
  ├── [Check] Balance ≥ CoinRatePerTick?
  │     ├── YES:
  │     │   ├── Deduct coins từ Viewer (via ICoinService)
  │     │   ├── Create BillingTick record
  │     │   ├── Update CallSession.TotalCoinsCharged, TotalTicks
  │     │   ├── SignalR: CallBillingTick event → cả Viewer và Host
  │     │   └── [Check] Balance sau khi trừ < 100?
  │     │         └── YES → SignalR: CoinWarning event → Viewer
  │     │               └── Hiển thị coin balance real-time trong UI
  │     └── NO (hết coin):
  │         ├── Set CallSession.Status = Ended, EndedBy = System
  │         ├── Publish CallEndedEvent
  │         ├── SignalR: CallEnded event → cả Viewer và Host
  │         └── Cancel Hangfire recurring job
```

### 2.5 End Call Flow (Manual)

```
Viewer/Host → EndCall(sessionId)
  │
  ├── [Guard] userId = Session.ViewerId HOẶC Session.HostId
  ├── Set CallSession.Status = Ended, EndedAt = now
  ├── Set EndedBy = Viewer / Host
  ├── Cancel Hangfire billing job
  ├── Publish CallEndedEvent
  ├── SignalR: CallEnded event → cả 2 phía
  └── Return CallBillingSummary { Duration, TotalCoins }
```

### 2.6 Public Stream + Private Call Concurrency

```
Khi Host accept private call trong khi đang public stream:
  - Public stream KHÔNG bị pause/end
  - Host's Agora video/audio track tắt với public viewers
  - SignalR: broadcast "HostAway" event đến room viewers
  - Viewers thấy placeholder "Host đang bận" thay vì video
  - Khi call kết thúc: Host's video/audio tự động resume
```

---

## 3. RoomChatModule — Chat trong phòng livestream

### 3.1 Send Room Chat Message Flow

```
Viewer → SendRoomMessage(roomId, content)
  │
  ├── [Guard] Viewer đang trong room (active ViewerSession)
  ├── [Guard] Viewer không bị kick (KickedViewers check)
  ├── [Rate Limit] Max 3 messages/giây per user (Redis counter)
  │     └── Nếu vượt → Error "Gửi quá nhanh"
  ├── [Content Filter] Profanity check (danh sách từ cấm)
  │     └── Nếu vi phạm → Error "Nội dung không phù hợp"
  ├── Trim content (max 200 chars)
  ├── Write to Redis Stream: XADD room:{roomId}:chat * ...
  ├── Set/refresh TTL: EXPIRE room:{roomId}:chat 604800 (7 ngày)
  ├── Publish RoomChatMessageSentEvent
  └── SignalR: broadcast RoomMessageReceived → tất cả viewers trong room group
```

### 3.2 Get Recent Messages (Viewer Join)

```
Viewer → GetRecentMessages(roomId)
  │
  ├── [Guard] Viewer đang trong room
  ├── XREVRANGE room:{roomId}:chat + - COUNT 0
  │     └── Q-C4: Không có history — return empty list
  └── Return []
```

> **Quyết định từ Q-C4**: Viewer mới join không thấy tin nhắn cũ. Chỉ thấy tin nhắn từ khi join.

### 3.3 Rate Limit Implementation

```
Redis key: chat_rate:{userId}:{roomId}
TTL: 1 giây (sliding window)
Logic:
  - INCR chat_rate:{userId}:{roomId}
  - EXPIRE chat_rate:{userId}:{roomId} 1
  - Nếu count > 3 → reject
```

### 3.4 Export Room Chat to S3 (Hangfire Job)

```
ExportRoomChatToS3(roomId, date)
  │
  ├── XRANGE room:{roomId}:chat - + (đọc toàn bộ)
  ├── Serialize thành JSON Lines format
  ├── Upload lên S3: archives/room-chat/{date}/{roomId}.jsonl.gz
  └── Log kết quả (không xóa Redis — TTL tự xử lý)
```

---

## 4. DirectChatModule — Chat 1-1

### 4.1 Send Direct Message Flow

```
Viewer → SendDirectMessage(hostId, content, messageType)
  │
  ├── [Guard] Sender.Role = Viewer (Q-D1: chỉ Viewer nhắn Host)
  ├── [Guard] Host.Role = Host
  ├── [Guard] Block check: Host không block Viewer VÀ Viewer không block Host
  │     └── Nếu bị block → Error "Không thể gửi tin nhắn"
  ├── Find or Create Conversation { ViewerId, HostId }
  │     └── Nếu conversation bị hidden → unhide khi gửi tin mới? NO
  │         └── Nếu IsHidden = true → Error "Conversation không khả dụng"
  ├── Create DirectMessage (insert vào đúng partition theo tháng)
  ├── Update Conversation: LastMessageAt, LastMessagePreview, HostUnreadCount++
  ├── Publish DirectMessageSentEvent
  ├── SignalR: send DirectMessageReceived → Host (nếu online)
  └── Push Notification: gửi FCM notification → Host (Q-D4: mỗi tin nhắn mới)
```

### 4.2 Get Conversations Flow

```
Viewer/Host → GetConversations(userId, paging)
  │
  ├── Query conversations WHERE (ViewerId = userId OR HostId = userId)
  │     AND IsHiddenBy{Role} = false
  ├── Order by LastMessageAt DESC
  └── Return PagedResult<Conversation>
```

### 4.3 Get Messages Flow

```
User → GetMessages(conversationId, paging)
  │
  ├── [Guard] User là participant của conversation
  ├── [Guard] Conversation không bị hidden với user này
  ├── Query direct_messages WHERE ConversationId = ?
  │     AND SentAt >= [partition start] (query hint để đúng partition)
  │     AND IsDeletedBySender = false (nếu sender là current user)
  ├── Order by SentAt DESC
  └── Return PagedResult<DirectMessage>
```

### 4.4 Block User → Hide Conversation Flow

```
User A → BlockUser(userBId)  [xử lý trong Profiles module]
  │
  └── Publish UserBlockedEvent { BlockerId=A, BlockedId=B }
        │
        └── DirectChat handler:
              ├── Find Conversation { ViewerId=A, HostId=B } OR { ViewerId=B, HostId=A }
              ├── Set IsHiddenByA = true (Q-D3: ẩn cả 2 phía)
              ├── Set IsHiddenByB = true
              └── Publish ConversationHiddenEvent
```

### 4.5 Mark As Read Flow

```
User → MarkAsRead(conversationId)
  │
  ├── Update DirectMessages: IsRead=true, ReadAt=now WHERE ConversationId=? AND SenderId≠userId
  ├── Reset UnreadCount cho user trong Conversation
  ├── SignalR: send MessageRead event → sender (để update read receipt UI)
  └── Return
```

### 4.6 Partition Query Strategy

```
Khi query messages, luôn include SentAt range để PostgreSQL dùng partition pruning:

// Lấy messages trong 30 ngày gần nhất
var cutoff = DateTime.UtcNow.AddDays(-30);
var messages = await context.DirectMessages
    .Where(m => m.ConversationId == conversationId 
             && m.SentAt >= cutoff)  // ← partition pruning hint
    .OrderByDescending(m => m.SentAt)
    .Take(pageSize)
    .ToListAsync();
```

---

## 5. Agora Integration Model

### 5.1 Token Generation

```
GenerateAgoraToken(channelName, userId, role, ttlSeconds=3600)
  │
  ├── Load AgoraAppId, AgoraAppCertificate từ config
  ├── Build RtcTokenBuilder:
  │     - channelName: string
  │     - uid: userId.GetHashCode() (uint32)
  │     - role: Publisher (1) / Subscriber (2)
  │     - privilegeExpiredTs: now + ttlSeconds
  └── Return token string
```

### 5.2 Channel Naming Convention

```
Public stream:  "room-{roomId:N}"      // e.g. "room-550e8400e29b41d4a716446655440000"
Private call:   "call-{sessionId:N}"   // e.g. "call-6ba7b810-9dad-11d1-80b4-00c04fd430c8"
```

### 5.3 Token Refresh

```
Client-side: Agora SDK callback onTokenPrivilegeWillExpire
  → Client calls GET /api/livestream/rooms/{roomId}/token (refresh)
  → Server generates new token với TTL=1h
  → Client calls renewToken(newToken) trên Agora SDK
```
