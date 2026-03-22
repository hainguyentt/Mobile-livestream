# Frontend Components — Unit 2: Livestream Engine

**Unit**: Unit 2 — Livestream Engine  
**App**: PWA (Next.js 14, FSD Architecture)  
**Ngày tạo**: 2026-03-22

---

## 1. Pages & Routes

| Route | Page Component | Mô tả |
|---|---|---|
| `/[locale]/live` | `LiveListPage` | Discovery — danh sách rooms đang live |
| `/[locale]/live/[roomId]` | `LiveRoomPage` | Xem livestream public |
| `/[locale]/live/host` | `HostBroadcastPage` | Host phát stream |
| `/[locale]/call/[callId]` | `PrivateCallPage` | Private call screen |
| `/[locale]/messages` | `MessagesPage` | Danh sách conversations |
| `/[locale]/messages/[conversationId]` | `ChatDetailPage` | Chi tiết conversation |

---

## 2. Feature Structure (FSD)

```
src/features/
├── live-list/                    # Discovery page
│   ├── ui/LiveListPage.tsx
│   ├── ui/RoomCard.tsx
│   ├── model/useLiveList.ts
│   └── api/liveListApi.ts
│
├── live-room/                    # Viewer xem stream
│   ├── ui/LiveRoomPage.tsx
│   ├── ui/VideoPlayer.tsx
│   ├── ui/ChatOverlay.tsx
│   ├── ui/GiftPanel.tsx
│   ├── ui/ViewerCountBadge.tsx
│   ├── ui/RoomChatInput.tsx
│   ├── model/useLiveRoom.ts      # Orchestration hook
│   ├── model/useRoomChat.ts
│   ├── model/useRoomSignalR.ts
│   └── api/liveRoomApi.ts
│
├── host-broadcast/               # Host phát stream
│   ├── ui/HostBroadcastPage.tsx
│   ├── ui/StartStreamForm.tsx
│   ├── ui/BroadcastControls.tsx
│   ├── ui/IncomingCallModal.tsx
│   ├── model/useHostBroadcast.ts
│   └── api/hostBroadcastApi.ts
│
├── private-call/                 # Private call
│   ├── ui/PrivateCallPage.tsx
│   ├── ui/CallControls.tsx
│   ├── ui/BillingTicker.tsx
│   ├── ui/CoinWarningModal.tsx
│   ├── model/usePrivateCall.ts   # Orchestration hook
│   ├── model/useCallBilling.ts
│   ├── model/useCallSignalR.ts
│   └── api/privateCallApi.ts
│
├── messages/                     # Conversation list
│   ├── ui/MessagesPage.tsx
│   ├── ui/ConversationItem.tsx
│   ├── model/useMessages.ts
│   └── api/messagesApi.ts
│
└── chat-detail/                  # Chat 1-1
    ├── ui/ChatDetailPage.tsx
    ├── ui/MessageBubble.tsx
    ├── ui/ChatInput.tsx
    ├── model/useChatDetail.ts
    ├── model/useChatSignalR.ts
    └── api/chatDetailApi.ts
```

---

## 3. Component Specifications

### 3.1 LiveListPage

**Props**: none  
**State** (via `useLiveList`):
```typescript
{
  rooms: LivestreamRoomSummary[]
  isLoading: boolean
  hasMore: boolean
  fetchNextPage: () => void
}
```

**Layout**: Grid 2 columns (mobile), 3 columns (tablet+)  
**Sorting**: By `viewerCount` DESC (Q-E1: A)  
**Behavior**:
- Infinite scroll (load more khi scroll đến cuối)
- Pull-to-refresh
- Auto-refresh mỗi 30 giây (viewer count thay đổi)
- Empty state: "Chưa có stream nào đang diễn ra"

---

### 3.2 RoomCard

**Props**:
```typescript
{
  room: LivestreamRoomSummary  // { id, hostName, hostAvatar, title, category, viewerCount, thumbnailUrl? }
  onClick: (roomId: string) => void
}
```

**UI Elements**:
- Thumbnail (host avatar fallback nếu không có thumbnail)
- LIVE badge (đỏ, pulse animation)
- Viewer count (icon người + số)
- Host display name
- Room title
- Category tag

---

### 3.3 LiveRoomPage (Orchestration)

**Orchestration hook** `useLiveRoom`:
```typescript
{
  // Room state
  room: LivestreamRoom | null
  isLoading: boolean
  
  // Agora
  agoraToken: string | null
  channelName: string | null
  
  // Chat
  messages: RoomChatMessage[]
  sendMessage: (content: string) => Promise<void>
  isChatVisible: boolean
  toggleChat: () => void
  
  // Controls
  volume: number
  setVolume: (v: number) => void
  quality: VideoQuality
  setQuality: (q: VideoQuality) => void
  isFullscreen: boolean
  toggleFullscreen: () => void
  
  // Gift
  isGiftPanelOpen: boolean
  toggleGiftPanel: () => void
  
  // Actions
  leaveRoom: () => void
}
```

**Layout** (mobile-first):
```
┌─────────────────────────────┐
│  [← Back]  Room Title  [👥N] │  ← Header overlay
│                             │
│                             │
│      VIDEO PLAYER           │  ← Full screen video
│                             │
│                             │
│  [🔊] [⛶] [HD] [💬] [🎁]   │  ← Controls overlay (Q-E2: D)
├─────────────────────────────┤
│  Chat overlay (toggleable)  │  ← Slide up/down
│  [Message input...]  [Send] │
└─────────────────────────────┘
```

---

### 3.4 VideoPlayer

**Props**:
```typescript
{
  channelName: string
  agoraToken: string
  role: 'host' | 'viewer'
  onTokenExpiring: () => Promise<string>  // callback để refresh token
}
```

**Agora SDK usage**:
```typescript
// Viewer mode
const { isConnected } = useJoin({ appid, channel, token, uid })
const remoteUsers = useRemoteUsers()
// Render host's video track
```

**Controls** (Q-E2: D):
- Volume slider
- Fullscreen toggle
- Quality selector (Auto / 720p / 480p / 360p)
- Chat toggle (ẩn/hiện ChatOverlay)
- Gift button (mở GiftPanel)

---

### 3.5 ChatOverlay

**Props**:
```typescript
{
  messages: RoomChatMessage[]
  onSendMessage: (content: string) => Promise<void>
  isVisible: boolean
  currentUserId: string
}
```

**UI**:
- Scrollable message list (auto-scroll to bottom khi có message mới)
- Message bubble: `[Avatar] DisplayName: content`
- Input field + Send button
- Rate limit feedback: disable input 1 giây sau khi gửi quá nhanh

**Note**: Không hiển thị history khi join (Q-C4: C) — list bắt đầu empty

---

### 3.6 GiftPanel

**Props**:
```typescript
{
  roomId: string
  isOpen: boolean
  onClose: () => void
  onGiftSent: (gift: VirtualGift) => void
}
```

**UI**: Bottom sheet với grid các gift items  
**Note**: Gift data từ Unit 3 (Payment module) — placeholder trong Unit 2

---

### 3.7 HostBroadcastPage

**State** (via `useHostBroadcast`):
```typescript
{
  streamStatus: 'idle' | 'starting' | 'live' | 'ending'
  room: LivestreamRoom | null
  viewerCount: number
  
  // Form
  title: string
  category: RoomCategory
  
  // Agora (host mode)
  isMuted: boolean
  isCameraOff: boolean
  toggleMute: () => void
  toggleCamera: () => void
  
  // Actions
  startStream: (title: string, category: RoomCategory) => Promise<void>
  endStream: () => Promise<void>
  
  // Incoming call
  pendingCallRequest: CallRequest | null
  acceptCall: (requestId: string) => Promise<void>
  rejectCall: (requestId: string) => Promise<void>
}
```

**Agora SDK usage** (host mode):
```typescript
const { localMicrophoneTrack } = useLocalMicrophoneTrack()
const { localCameraTrack } = useLocalCameraTrack()
usePublish([localMicrophoneTrack, localCameraTrack])
```

**IncomingCallModal**: Hiển thị khi có `pendingCallRequest`
- Avatar + tên Viewer
- Nút Accept (xanh) / Decline (xám)
- Countdown timer 30 giây
- Auto-dismiss khi timeout

---

### 3.8 PrivateCallPage (Orchestration)

**Route**: `/[locale]/call/[callId]`  
**Orchestration hook** `usePrivateCall`:
```typescript
{
  session: CallSession | null
  
  // Agora
  agoraToken: string | null
  channelName: string | null
  
  // Billing
  coinBalance: number           // real-time từ SignalR
  isWarning: boolean            // balance < 100
  
  // Controls
  isMuted: boolean
  isCameraOff: boolean
  toggleMute: () => void
  toggleCamera: () => void
  
  // Actions
  endCall: () => Promise<void>
}
```

**Layout**:
```
┌─────────────────────────────┐
│  [Timer: 02:34]  [⚠️ 85 🪙] │  ← Top overlay: timer + coin warning
│                             │
│      FULL SCREEN VIDEO      │  ← Agora COMMUNICATION mode
│                             │
│  [🔇] [📷] [🎁]  [📞 End]  │  ← Bottom controls
└─────────────────────────────┘
```

**BillingTicker** (Q-E4: D + note):
- Bình thường: ẩn
- Khi `balance < 100`: hiển thị coin balance real-time (cập nhật mỗi tick)
- Animation: pulse/shake khi warning

**CoinWarningModal**:
- Trigger: `balance < 100`
- Content: "コインが残りわずかです" + current balance
- Actions: "チャージ" (nạp coin — navigate to payment) / "続ける" (tiếp tục)

---

### 3.9 MessagesPage

**State** (via `useMessages`):
```typescript
{
  conversations: Conversation[]
  isLoading: boolean
  hasMore: boolean
  fetchNextPage: () => void
}
```

**Layout**: List view, sort by `lastMessageAt` DESC  
**ConversationItem**:
- Avatar + DisplayName
- Last message preview (truncate 50 chars)
- Timestamp (relative: "2 phút trước")
- Unread badge (số tin chưa đọc)

**Bottom Navigation**: Tab "Messages" với unread count badge (Q-E5: C)

---

### 3.10 ChatDetailPage

**State** (via `useChatDetail` + `useChatSignalR`):
```typescript
{
  conversation: Conversation | null
  messages: DirectMessage[]
  isLoading: boolean
  hasMore: boolean
  
  // Input
  inputText: string
  setInputText: (text: string) => void
  sendMessage: () => Promise<void>
  
  // Emoji
  isEmojiPickerOpen: boolean
  toggleEmojiPicker: () => void
  sendEmoji: (emojiCode: string) => Promise<void>
  
  // Actions
  loadMoreMessages: () => void
  markAsRead: () => void
}
```

**Layout**:
```
┌─────────────────────────────┐
│  [← Back]  Host Name  [⋮]  │  ← Header
├─────────────────────────────┤
│                             │
│  [Message bubbles]          │  ← Scrollable, newest at bottom
│                             │
├─────────────────────────────┤
│  [😊] [Text input...] [Send]│  ← Input bar
└─────────────────────────────┘
```

**MessageBubble**:
- Sent (right-aligned, primary color)
- Received (left-aligned, surface color)
- Read receipt: checkmark icon khi `IsRead = true`
- Emoji message: large emoji display

**SignalR integration** (`useChatSignalR`):
- Connect to `ChatHub` khi mount
- Subscribe `DirectMessageReceived` → append to messages list
- Subscribe `MessageRead` → update read status
- Disconnect khi unmount

---

## 4. Zustand Stores

### 4.1 useLiveRoomStore
```typescript
interface LiveRoomStore {
  roomId: string | null
  hostId: string | null
  agoraToken: string | null
  channelName: string | null
  viewerCount: number
  isChatVisible: boolean
  isGiftPanelOpen: boolean
  
  setRoom: (room: Partial<LiveRoomStore>) => void
  updateViewerCount: (count: number) => void
  reset: () => void  // gọi khi leave room
}
```

### 4.2 useCallStore
```typescript
interface CallStore {
  callId: string | null
  sessionId: string | null
  partnerId: string | null
  partnerName: string | null
  agoraToken: string | null
  channelName: string | null
  coinBalance: number
  isWarning: boolean
  startedAt: Date | null
  
  setCall: (call: Partial<CallStore>) => void
  updateBalance: (balance: number) => void
  reset: () => void  // gọi khi end call
}
```

---

## 5. API Integration Points

| Feature | Endpoint | Method |
|---|---|---|
| Danh sách rooms | `/api/v1/livestream/rooms` | GET |
| Join room | `/api/v1/livestream/rooms/{roomId}/join` | POST |
| Leave room | `/api/v1/livestream/rooms/{roomId}/leave` | POST |
| Start stream | `/api/v1/livestream/rooms` | POST |
| End stream | `/api/v1/livestream/rooms/{roomId}/end` | POST |
| Refresh Agora token | `/api/v1/livestream/rooms/{roomId}/token` | GET |
| Kick viewer | `/api/v1/livestream/rooms/{roomId}/kick` | POST |
| Send call request | `/api/v1/livestream/calls/request` | POST |
| Accept call | `/api/v1/livestream/calls/{requestId}/accept` | POST |
| Reject call | `/api/v1/livestream/calls/{requestId}/reject` | POST |
| End call | `/api/v1/livestream/calls/{sessionId}/end` | POST |
| Send room chat | `/api/v1/roomchat/{roomId}/messages` | POST |
| Get conversations | `/api/v1/directchat/conversations` | GET |
| Get messages | `/api/v1/directchat/conversations/{id}/messages` | GET |
| Send direct message | `/api/v1/directchat/conversations/{id}/messages` | POST |
| Mark as read | `/api/v1/directchat/conversations/{id}/read` | POST |

---

## 6. SignalR Hub Subscriptions

### LivestreamHub (`/hubs/livestream`)
| Event | Handler | UI Action |
|---|---|---|
| `ViewerJoined` | `useRoomSignalR` | Update viewer count |
| `ViewerLeft` | `useRoomSignalR` | Update viewer count |
| `ViewerKicked` | `useRoomSignalR` | Force navigate away nếu là current user |
| `StreamEnded` | `useRoomSignalR` | Show "Stream ended" → 30s countdown → navigate away |
| `PrivateCallRequest` | `useHostBroadcast` | Show IncomingCallModal |
| `PrivateCallAccepted` | `usePrivateCall` | Navigate to `/call/{callId}` |
| `PrivateCallRejected` | `usePrivateCall` | Show toast "Host từ chối" |
| `CallBillingTick` | `useCallBilling` | Update coin balance in store |
| `CoinWarning` | `useCallBilling` | Set `isWarning = true`, show BillingTicker |

### ChatHub (`/hubs/chat`)
| Event | Handler | UI Action |
|---|---|---|
| `RoomMessageReceived` | `useRoomChat` | Append to chat list |
| `DirectMessageReceived` | `useChatSignalR` | Append to messages + update conversation preview |
| `MessageRead` | `useChatSignalR` | Update read receipt on message bubble |

---

## 7. Form Validation

### StartStreamForm
| Field | Validation |
|---|---|
| `title` | Required, 1–100 chars, not whitespace only |
| `category` | Required, must be valid `RoomCategory` enum value |

### ChatInput (Room)
| Field | Validation |
|---|---|
| `content` | Required, 1–200 chars, client-side trim |

### ChatInput (Direct)
| Field | Validation |
|---|---|
| `content` | Required, 1–1000 chars (text) hoặc valid emoji code |
