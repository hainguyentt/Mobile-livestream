# API Reference — Unit 2: Livestream Engine

Base URL: `/api/v1`

Authentication: JWT Bearer token (cookie `access_token`) — required trừ khi ghi chú `[Public]`.

---

## Livestream Rooms

### POST /api/v1/livestream/rooms
Tạo phòng livestream mới (Host only).

**Request Body**
```json
{
  "title": "My Live Stream",
  "category": "music"
}
```

**Response** `201 Created`
```json
{
  "id": "uuid",
  "title": "My Live Stream",
  "category": "music",
  "hostId": "uuid",
  "hostName": "string",
  "status": "created",
  "viewerCount": 0,
  "agoraChannelName": "string",
  "startedAt": null,
  "endedAt": null
}
```

---

### GET /api/v1/livestream/rooms `[Public]`
Lấy danh sách phòng đang live.

**Query Parameters**
| Param | Type | Default | Description |
|---|---|---|---|
| `category` | string | null | Filter theo category |
| `page` | int | 1 | Trang hiện tại |
| `pageSize` | int | 20 | Số item mỗi trang |

**Response** `200 OK`
```json
{
  "items": [{ "id": "uuid", "title": "...", "viewerCount": 42, ... }],
  "total": 100,
  "page": 1,
  "pageSize": 20
}
```

---

### GET /api/v1/livestream/rooms/{id} `[Public]`
Lấy chi tiết một phòng.

**Response** `200 OK` — Room object | `404 Not Found`

---

### POST /api/v1/livestream/rooms/{id}/start
Host bắt đầu stream.

**Response** `204 No Content`

---

### POST /api/v1/livestream/rooms/{id}/end
Host kết thúc stream.

**Response** `204 No Content`

---

### POST /api/v1/livestream/rooms/{id}/join
Viewer tham gia phòng.

**Response** `200 OK`
```json
{
  "roomId": "uuid",
  "sessionId": "uuid",
  "agoraChannelName": "string"
}
```

---

### POST /api/v1/livestream/rooms/{id}/leave
Viewer rời phòng.

**Response** `204 No Content`

---

### POST /api/v1/livestream/rooms/{id}/ban/{userId}
Host ban viewer.

**Request Body** (optional)
```json
{ "reason": "Spam" }
```

**Response** `204 No Content`

---

### GET /api/v1/livestream/rooms/{id}/viewers
Lấy số viewer hiện tại.

**Response** `200 OK`
```json
{ "roomId": "uuid", "viewerCount": 42 }
```

---

## Private Calls

### POST /api/v1/livestream/calls/request
Viewer gửi yêu cầu private call tới host.

**Request Body**
```json
{ "hostId": "uuid" }
```

**Response** `201 Created`
```json
{
  "requestId": "uuid",
  "status": "pending",
  "expiresAt": "2026-03-22T10:00:00Z"
}
```

---

### POST /api/v1/livestream/calls/{id}/accept
Host chấp nhận call request.

**Response** `200 OK`
```json
{
  "sessionId": "uuid",
  "agoraChannelName": "string",
  "hostToken": { "token": "string", "expiresAt": "datetime" },
  "viewerToken": { "token": "string", "expiresAt": "datetime" }
}
```

---

### POST /api/v1/livestream/calls/{id}/reject
Host từ chối call request.

**Request Body** (optional)
```json
{ "reason": "Busy" }
```

**Response** `204 No Content`

---

### POST /api/v1/livestream/calls/{id}/end
Kết thúc call đang active.

**Response** `204 No Content`

---

### GET /api/v1/livestream/calls/{id}/token
Lấy/refresh Agora token cho call đang active.

**Response** `200 OK`
```json
{
  "token": "string",
  "channelName": "string",
  "expiresAt": "datetime"
}
```

---

### GET /api/v1/livestream/calls/{id}/status
Lấy trạng thái call session.

**Response** `200 OK`
```json
{
  "sessionId": "uuid",
  "status": "active | ended",
  "startedAt": "datetime",
  "endedAt": "datetime | null",
  "totalCoinsCharged": 120,
  "totalTicks": 12
}
```

---

## Direct Chat

### GET /api/v1/direct-chat/conversations
Lấy danh sách conversations của user hiện tại.

**Response** `200 OK`
```json
[
  {
    "id": "uuid",
    "otherUserId": "uuid",
    "otherUserName": "string",
    "lastMessage": "string | null",
    "lastMessageAt": "datetime | null",
    "unreadCount": 3
  }
]
```

---

### GET /api/v1/direct-chat/conversations/{id}
Lấy chi tiết conversation.

**Response** `200 OK` — Conversation object | `404 Not Found`

---

### GET /api/v1/direct-chat/conversations/{id}/messages
Lấy messages trong conversation (phân trang theo thời gian).

**Query Parameters**
| Param | Type | Required | Description |
|---|---|---|---|
| `from` | datetime | Yes | Lấy messages từ thời điểm này |
| `to` | datetime | No | Đến thời điểm này (default: now) |

**Response** `200 OK`
```json
[
  {
    "messageId": "uuid",
    "senderId": "uuid",
    "content": "Hello",
    "messageType": "text | emoji",
    "emojiCode": "string | null",
    "isRead": true,
    "sentAt": "datetime"
  }
]
```

---

### POST /api/v1/direct-chat/conversations/{id}/read
Đánh dấu conversation đã đọc.

**Response** `204 No Content`

---

### POST /api/v1/direct-chat/block/{userId}
Block một user.

**Response** `204 No Content`

---

### DELETE /api/v1/direct-chat/block/{userId}
Unblock một user.

**Response** `204 No Content`

---

## SignalR Hubs

### Hub: `/hubs/livestream`
Kết nối khi viewer/host vào phòng livestream.

**Client → Server**
| Method | Params | Description |
|---|---|---|
| `JoinRoom` | `roomId: string` | Join room group |
| `LeaveRoom` | `roomId: string` | Leave room group |
| `SendChatMessage` | `roomId, content` | Gửi chat message |
| `SendGift` | `roomId, giftType, quantity` | Gửi gift |

**Server → Client**
| Event | Payload | Description |
|---|---|---|
| `ViewerCountUpdated` | `{ roomId, count }` | Cập nhật viewer count |
| `NewChatMessage` | `{ roomId, senderId, content, sentAt }` | Chat message mới |
| `GiftReceived` | `{ roomId, senderId, giftType, quantity }` | Gift animation trigger |
| `StreamEnded` | `{ roomId }` | Stream kết thúc |

---

### Hub: `/hubs/chat`
Kết nối cho direct chat realtime.

**Client → Server**
| Method | Params | Description |
|---|---|---|
| `JoinConversation` | `conversationId: string` | Join conversation group |
| `SendMessage` | `conversationId, content, messageType` | Gửi message |
| `SendEmojiReaction` | `conversationId, emojiCode` | Gửi emoji reaction |

**Server → Client**
| Event | Payload | Description |
|---|---|---|
| `NewDirectMessage` | `{ conversationId, messageId, senderId, content, sentAt }` | Message mới |
| `MessageRead` | `{ conversationId, readAt }` | Messages đã được đọc |

---

## Error Responses

Tất cả endpoints trả về lỗi theo format chuẩn:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Error message",
  "traceId": "string"
}
```

| Status | Meaning |
|---|---|
| `400` | Validation error |
| `401` | Unauthenticated |
| `403` | Forbidden (không có quyền) |
| `404` | Resource không tồn tại |
| `409` | Conflict (e.g., room đã tồn tại) |
| `429` | Rate limited |
| `500` | Server error |
