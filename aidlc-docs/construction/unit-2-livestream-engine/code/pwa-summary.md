# PWA Module Summary — Unit 2: Livestream Engine

## Tổng Quan

Frontend PWA (Next.js 14 App Router + FSD) cho Unit 2 bổ sung 3 feature domains:
- **Livestream**: Xem và tham gia phòng live, chat realtime, gửi gift
- **Private Call**: Video call 1-1 có tính phí qua Agora RTC
- **DirectChat**: Nhắn tin 1-1 giữa users

---

## Cấu Trúc FSD

```
src/
├── app/[locale]/
│   ├── livestream/
│   │   ├── page.tsx                    # Livestream grid (danh sách phòng)
│   │   └── [roomId]/page.tsx           # Livestream room viewer
│   └── messages/
│       ├── page.tsx                    # Conversation list
│       └── [conversationId]/page.tsx   # Conversation thread
│
├── features/
│   ├── livestream/
│   │   ├── api/livestreamApi.ts        # REST API calls
│   │   ├── model/
│   │   │   ├── types.ts                # Domain types
│   │   │   └── useLivestreamStore.ts   # Zustand store
│   │   └── ui/
│   │       ├── LivestreamGrid.tsx      # Room grid với viewer count
│   │       ├── LivestreamRoom.tsx      # Room viewer (chat + gift + controls)
│   │       ├── RoomChatPanel.tsx       # Chat panel trong room
│   │       ├── ViewerCountBadge.tsx    # Badge hiển thị số viewer
│   │       ├── GiftPanel.tsx           # Gift selection + send
│   │       ├── HostControls.tsx        # Host-only controls (end stream)
│   │       └── CallEndSummary.tsx      # (shared với private-call)
│   │
│   ├── private-call/
│   │   ├── api/privateCallApi.ts       # REST API calls
│   │   ├── model/
│   │   │   ├── types.ts                # Domain types
│   │   │   └── usePrivateCallStore.ts  # Zustand store
│   │   └── ui/
│   │       ├── CallRequestModal.tsx    # Incoming/outgoing call modal
│   │       ├── CallScreen.tsx          # Active call screen (Agora video)
│   │       ├── CallTimer.tsx           # Call duration timer
│   │       ├── BalanceDisplay.tsx      # Coin balance display
│   │       └── CallEndSummary.tsx      # Post-call summary (duration + coins)
│   │
│   └── direct-chat/
│       ├── api/directChatApi.ts        # REST API calls
│       ├── model/
│       │   ├── types.ts                # Domain types
│       │   └── useDirectChatStore.ts   # Zustand store
│       └── ui/
│           ├── ConversationList.tsx    # List of conversations
│           ├── ConversationThread.tsx  # Message thread
│           └── MessageInput.tsx        # Message input + emoji
│
├── lib/signalr/
│   ├── livestreamHub.ts                # SignalR hub client (Livestream)
│   └── chatHub.ts                      # SignalR hub client (Chat)
│
└── i18n/locales/
    ├── en.json                         # Keys: livestream, privateCall, directChat
    └── ja.json                         # Keys: livestream, privateCall, directChat
```

---

## SignalR Integration

### LivestreamHub (`/hubs/livestream`)

| Event (Server → Client) | Handler |
|---|---|
| `ViewerCountUpdated` | Cập nhật `viewerCount` trong store |
| `NewChatMessage` | Append message vào `chatMessages` |
| `GiftReceived` | Trigger gift animation |
| `StreamEnded` | Redirect viewer về grid |

### ChatHub (`/hubs/chat`)

| Event (Server → Client) | Handler |
|---|---|
| `NewDirectMessage` | Append message vào conversation thread |
| `MessageRead` | Mark messages as read |

---

## State Management

### `useLivestreamStore` (Zustand)
```ts
interface LivestreamState {
  rooms: LivestreamRoomDto[]
  currentRoom: LivestreamRoomDto | null
  chatMessages: RoomChatMessageDto[]
  viewerCount: number
}
```

### `usePrivateCallStore` (Zustand)
```ts
interface PrivateCallState {
  incomingRequest: CallRequestDto | null
  activeCall: CallSessionDto | null
  callStatus: 'idle' | 'ringing' | 'active' | 'ended'
  agoraToken: string | null
}
```

### `useDirectChatStore` (Zustand)
```ts
interface DirectChatState {
  conversations: ConversationDto[]
  messages: Record<string, DirectMessageDto[]>
  unreadCounts: Record<string, number>
}
```

---

## Agora RTC Integration

File: `src/features/private-call/api/agoraClient.ts`

- Sử dụng `agora-rtc-sdk-ng` (Web SDK)
- Token lấy từ backend (`GET /api/v1/private-calls/{callId}/agora-token`)
- Channel name = `callSessionId`
- Auto cleanup khi call kết thúc hoặc component unmount

---

## Pages

### `/[locale]/livestream`
- Server Component
- Render `<LivestreamGrid />` — danh sách phòng live đang hoạt động
- Filter theo category (query param)

### `/[locale]/livestream/[roomId]`
- Server Component (shell) + Client Components (chat, gift, controls)
- Join room qua SignalR khi mount
- Leave room khi unmount

### `/[locale]/messages`
- Server Component
- Render `<ConversationList />` — danh sách conversations

### `/[locale]/messages/[conversationId]`
- Server Component (shell) + Client Components (thread, input)
- Load messages, mark as read khi mount

---

## Tests

| File | Coverage |
|---|---|
| `__tests__/features/livestream/LivestreamGrid.test.tsx` | Empty state, room cards, category filter, sort by viewers |
| `__tests__/features/direct-chat/ConversationThread.test.tsx` | Render messages, alignment, markAsRead, load older |

---

## i18n Keys

Namespace `livestream`: `title`, `noActiveRooms`, `chatPlaceholder`, `send`, `sendGift`, `endStream`, `ending`, `confirmEndStream`

Namespace `privateCall`: `incomingCall`, `callFrom`, `accept`, `reject`, `endCall`, `callEnded`, `duration`, `coinsCharged`, `close`, `balance`, `coins`, `lowBalance`

Namespace `directChat`: `title`, `noConversations`, `messagePlaceholder`, `send`, `emojiReactions`, `toggleEmoji`
