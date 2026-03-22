# Frontend Component Architecture — Cross-Cutting Standard

**Phiên bản**: 1.0.0
**Ngày**: 2026-03-22
**Phạm vi**: Áp dụng cho tất cả Units (Unit 1 → Unit 5)
**Nguồn gốc**: Chuyển đổi từ `references/frontend/frontend-component-architecture.md` thành tài liệu chính thức

---

## 1. Phương Pháp Luận: Feature-Sliced Design (FSD)

Dự án áp dụng **Feature-Sliced Design (FSD)** — kiến trúc frontend cho ứng dụng quy mô lớn, phù hợp với Next.js App Router.

**Lý do chọn FSD**:
- Nhiều domain rõ ràng (Auth, Livestream, Chat, Payment, Matching, Moderation)
- Sẽ phát triển từ MVP → Phase 2+ (group livestream, subscription, speed dating)
- Team 5–10 developer làm việc parallel — cần boundary rõ ràng, tránh conflict

**3 nguyên tắc cốt lõi**:
1. Nhóm code theo business domain (không theo loại file kỹ thuật)
2. Enforced dependency direction — layer trên chỉ import từ layer dưới, không bao giờ ngược lại
3. Mỗi module có public API qua `index.ts` — chỉ export những gì consumer cần

---

## 2. Layer Hierarchy

Sắp xếp từ cao xuống thấp. **Quy tắc bất biến: layer trên chỉ import từ layer dưới, không bao giờ ngược lại.**

| Layer | Mô tả | Ví dụ |
|---|---|---|
| `app/` | Application shell, routing, global providers | `layout.tsx`, `page.tsx`, `Providers.tsx` |
| `src/views/` | Composition layer — lắp ráp widgets thành screen | `LoginPage.tsx`, `LiveRoomPage.tsx` |
| `src/widgets/` | Composite UI blocks lớn, nhiều features kết hợp | `LivestreamViewer`, `BottomNavigation`, `GiftPanel` |
| `src/features/` | User interaction scenarios — trái tim của app | `send-gift`, `start-livestream`, `login-with-line` |
| `src/entities/` | Domain models + domain UI primitives | `user`, `livestream`, `coin`, `gift`, `message` |
| `src/shared/` | Business-agnostic code — KHÔNG import từ layer nào khác | shadcn/ui components, utilities, API client |

**Dependency flow**:
```
app/ → src/views/ → src/widgets/ → src/features/ → src/entities/ → src/shared/
```

> ⚠️ **QUAN TRỌNG — Next.js Conflict**: KHÔNG đặt tên FSD composition layer là `src/pages/`. Next.js tự động scan tất cả thư mục tên `pages/` và coi đó là Pages Router, gây lỗi "Conflicting app and page file" với App Router. **Luôn dùng `src/views/` thay thế.**

---

## 3. Directory Structure

```
project-root/
├── app/                              # Next.js App Router — ROUTING ONLY
│   ├── (main)/                       # Authenticated user screens
│   │   ├── layout.tsx                # MainLayout: BottomNav + auth guard
│   │   ├── page.tsx                  # Home
│   │   ├── discover/page.tsx
│   │   ├── live/
│   │   │   ├── page.tsx              # Live list
│   │   │   └── [roomId]/page.tsx     # Livestream viewer
│   │   ├── call/[callId]/page.tsx    # Private call
│   │   ├── messages/
│   │   │   ├── page.tsx              # Chat list
│   │   │   └── [conversationId]/page.tsx
│   │   ├── profile/
│   │   │   ├── page.tsx              # My profile
│   │   │   └── [userId]/page.tsx     # Other user profile
│   │   ├── wallet/page.tsx
│   │   ├── leaderboard/page.tsx
│   │   └── settings/page.tsx
│   ├── (auth)/                       # Unauthenticated screens
│   │   ├── login/page.tsx
│   │   ├── register/page.tsx
│   │   └── forgot-password/page.tsx
│   ├── (onboarding)/                 # Post-register setup
│   │   ├── profile-setup/page.tsx
│   │   └── phone-verify/page.tsx
│   ├── admin/                        # Admin dashboard (separate layout)
│   │   ├── layout.tsx                # AdminLayout: sidebar nav
│   │   ├── page.tsx
│   │   ├── users/page.tsx
│   │   ├── livestreams/page.tsx
│   │   ├── moderation/page.tsx
│   │   ├── finance/page.tsx
│   │   └── gifts/page.tsx
│   ├── api/                          # Route Handlers
│   │   ├── webhooks/stripe/route.ts
│   │   ├── webhooks/line-pay/route.ts
│   │   └── agora/token/route.ts
│   ├── _providers/Providers.tsx
│   ├── layout.tsx
│   └── manifest.ts                   # PWA manifest
│
├── src/                              # FSD LAYERS
│   ├── views/                        # Composition layer (⚠️ KHÔNG dùng tên "pages/" — conflict với Next.js Pages Router)
│   │   ├── home/ui/HomePage.tsx
│   │   ├── live-room/ui/LiveRoomPage.tsx
│   │   ├── private-call/ui/PrivateCallPage.tsx
│   │   ├── messages/ui/MessagesPage.tsx
│   │   ├── chat-detail/ui/ChatDetailPage.tsx
│   │   ├── discover/ui/DiscoverPage.tsx
│   │   ├── wallet/ui/WalletPage.tsx
│   │   ├── leaderboard/ui/LeaderboardPage.tsx
│   │   ├── my-profile/ui/MyProfilePage.tsx
│   │   ├── user-profile/ui/UserProfilePage.tsx
│   │   ├── settings/ui/SettingsPage.tsx
│   │   ├── login/ui/LoginPage.tsx
│   │   ├── register/ui/RegisterPage.tsx
│   │   └── admin-dashboard/ui/AdminDashboardPage.tsx
│   │
│   ├── widgets/
│   │   ├── bottom-navigation/
│   │   ├── livestream-viewer/        # Critical: video + chat + gift + leaderboard
│   │   ├── livestream-broadcaster/
│   │   ├── private-call-view/
│   │   ├── conversation-list/
│   │   ├── chat-room/
│   │   ├── user-feed/
│   │   ├── live-carousel/
│   │   ├── gift-panel/
│   │   ├── leaderboard-panel/
│   │   ├── admin-sidebar/
│   │   ├── admin-user-table/
│   │   ├── admin-revenue-chart/
│   │   └── admin-moderation-queue/
│   │
│   ├── features/
│   │   ├── auth/
│   │   │   ├── login-email/
│   │   │   ├── login-line/           # Must Have — feature slice riêng
│   │   │   ├── login-social/
│   │   │   ├── register/
│   │   │   ├── logout/
│   │   │   ├── forgot-password/
│   │   │   ├── verify-phone/
│   │   │   └── delete-account/
│   │   ├── livestream/
│   │   │   ├── start-livestream/
│   │   │   ├── end-livestream/
│   │   │   ├── join-room/
│   │   │   ├── leave-room/
│   │   │   ├── send-room-chat/
│   │   │   └── kick-viewer/
│   │   ├── gift/
│   │   │   ├── send-gift/
│   │   │   └── gift-animation/
│   │   ├── private-call/
│   │   │   ├── request-call/
│   │   │   ├── accept-call/
│   │   │   ├── decline-call/
│   │   │   └── end-call/
│   │   ├── matching/
│   │   │   ├── like-user/
│   │   │   ├── follow-user/
│   │   │   ├── search-users/
│   │   │   └── filter-users/
│   │   ├── chat/
│   │   │   ├── send-message/
│   │   │   ├── send-sticker/
│   │   │   ├── block-user/
│   │   │   └── delete-message/
│   │   ├── payment/
│   │   │   ├── top-up-stripe/
│   │   │   ├── top-up-line-pay/
│   │   │   └── withdraw-request/
│   │   ├── notification/
│   │   │   ├── push-permission/
│   │   │   └── notification-settings/
│   │   ├── profile/
│   │   │   ├── edit-profile/
│   │   │   └── upload-photos/
│   │   ├── moderation/
│   │   │   ├── report-user/
│   │   │   ├── report-content/
│   │   │   └── admin-ban-user/
│   │   └── leaderboard/
│   │       ├── view-ranking/
│   │       └── room-top-gifters/
│   │
│   ├── entities/
│   │   ├── user/          # UserAvatar, UserCard, UserBadge, OnlineStatus
│   │   ├── livestream/    # LiveRoomCard, LiveBadge, ViewerCount
│   │   ├── coin/          # CoinBalance, CoinAmount, TransactionRow
│   │   ├── gift/          # GiftItem
│   │   ├── message/       # MessageBubble, ConversationRow, ReadReceipt
│   │   └── notification/  # NotificationBadge
│   │
│   └── shared/
│       ├── ui/            # shadcn/ui components (copied)
│       ├── lib/
│       │   ├── api-client.ts
│       │   ├── signalr/   # HubConnectionBuilder factory, useSignalR hook
│       │   ├── agora/     # Agora client factory, useAgoraClient hook
│       │   ├── format/    # date-fns/ja wrappers, ¥ formatting
│       │   ├── cache/     # Centralized cache tag taxonomy
│       │   └── utils.ts   # cn() helper
│       ├── config/
│       │   ├── env.ts     # Type-safe env vars
│       │   ├── constants.ts
│       │   └── routes.ts  # Type-safe route paths
│       ├── hooks/
│       │   ├── useMediaQuery.ts
│       │   ├── useDebounce.ts
│       │   └── useIntersection.ts
│       └── types/
│           ├── api.ts     # Generic API response types
│           └── common.ts  # Pagination, SortOrder, etc.
│
├── messages/
│   ├── ja.json            # Primary
│   └── en.json
│
└── public/
    ├── icons/             # PWA icons
    ├── lottie/            # Gift animation JSON files
    └── stickers/          # Sticker assets
```

---

## 4. Server Component vs Client Component

**Nguyên tắc**: Default Server Component, chỉ `"use client"` khi buộc phải có interactivity.

### Server Components (không `"use client"`)
- Tất cả `app/**/page.tsx` và `app/**/layout.tsx`
- Tất cả `src/views/` layer (composition, fetch data, render widgets)
- `entities/*/api/*.queries.ts` (server-side data fetching)
- `entities/*/ui/*` presentational components không có state/event handlers
- Toàn bộ admin dashboard pages

### Client Components (`"use client"`)
- Widgets có interactivity: `LivestreamViewer`, `BottomNavigation`, `ChatRoom`, `GiftPanel`, `PrivateCallView`
- Tất cả features có user interaction: forms, buttons, interactive UI
- Components dùng browser APIs: Agora SDK, SignalR, Vibration API
- Components dùng React hooks: `useState`, `useEffect`, `useRef`, event handlers

### Ranh giới "leaf-like"
Giữ client components ở vị trí "lá" trong component tree — càng sâu càng tốt.

Ví dụ: `LiveRoomPage` (Server) fetch room data → pass props vào `LivestreamViewer` (Client). Server xử lý data fetching + SEO + initial render, Client chỉ xử lý interactivity.

---

## 5. State Management Architecture

### Server State — TanStack Query
Dữ liệu từ backend API: user profiles, transaction history, leaderboard, gift catalog, search results, conversation list.

Query keys và query functions định nghĩa trong `entities/*/api/*.queries.ts`.

### Client State — Zustand Stores

| Store | Nội dung |
|---|---|
| `useAuthStore` | Current user session, JWT tokens. Persist to localStorage. |
| `useLiveRoomStore` | Current room context — room ID, host info, mute/camera state, elapsed time, cost. Reset khi leave. |
| `useCallStore` | Private call state — call ID, partner, timer, coin rate. Reset khi end. |
| `useUIStore` | Theme mode, bottom sheet state, modal stack. Không persist. |

### Real-time State — SignalR → Zustand Bridge

| Store | SignalR Events |
|---|---|
| `useChatStore` | Messages trong room chat (append khi nhận), typing indicators |
| `usePresenceStore` | Online/offline status, viewer list (join/leave events) |
| `useCoinBalanceStore` | Coin balance real-time (gift sent/received, top-up success) |
| `useNotificationStore` | Unread counts cho chat, likes, follows |

Pattern: `useSignalRSubscription` hook trong `shared/lib/signalr/` nhận hub events → dispatch vào Zustand stores. Mount global tại `_providers/Providers.tsx` cho presence & notification, mount tại widget level cho room-specific events.

---

## 6. Orchestration Hook Pattern

Pattern quan trọng nhất trong app — dùng cho các widget phức tạp (LivestreamViewer, PrivateCallView, ChatRoom).

**Nguyên tắc**: Widget có một "orchestration hook" kết nối tất cả feature hooks và entity stores. Hook KHÔNG chứa business logic — chỉ wire các pieces lại với nhau.

```typescript
// widgets/livestream-viewer/model/useLivestreamRoom.ts
export function useLivestreamRoom(roomId: string) {
  const agora = useAgoraClient()           // Video connection
  const signalR = useSignalR('LiveRoom')   // Chat + gift events
  const roomStore = useLiveRoomStore()     // Zustand room state
  const chatStore = useChatStore()         // Messages state
  const giftAnim = useGiftAnimation()      // Animation queue
  const payPerMin = usePayPerMinute()      // Timer + cost

  // Wire events → stores
  useEffect(() => {
    signalR.on('GiftReceived', giftAnim.enqueue)
    signalR.on('ChatMessage', chatStore.append)
    signalR.on('ViewerJoined', roomStore.addViewer)
    return () => signalR.off(...)
  }, [])

  // Cleanup on unmount
  useEffect(() => () => { agora.leave(); signalR.disconnect() }, [])

  return { agora, roomStore, chatStore, giftAnim, payPerMin }
}
```

**Lợi ích**: Testable (mock individual hooks), readable (component chỉ render), reusable (layout khác nhau dùng cùng hook).

---

## 7. Data Flow — End-to-End Example: "Gửi Gift"

1. User tap gift button → `SendGiftButton` (feature) mở Bottom Sheet
2. `GiftConfirmSheet` fetch gift catalog qua TanStack Query (`entities/gift/api`)
3. Hiển thị `CoinBalance` từ `useCoinBalanceStore`
4. User confirm → `useSendGift` hook validate balance → call `POST /api/gifts/send`
5. Backend trừ coin → push SignalR `GiftReceived` tới viewers + `CoinBalanceUpdated` tới sender
6. Frontend nhận `GiftReceived` → enqueue animation + append chat message
7. Frontend nhận `CoinBalanceUpdated` → update `useCoinBalanceStore`
8. `GiftAnimationLayer` dequeue → render Lottie animation
9. TanStack Query invalidate `['transactions']` + `['leaderboard', roomId]`

---

## 8. Provider Composition (Global)

```typescript
// app/_providers/Providers.tsx — "use client"
<QueryClientProvider>
  <ThemeProvider>
    <SignalRProvider>        // Global: presence + notifications
      <AuthProvider>
        <IntlProvider>      // next-intl
          {children}
        </IntlProvider>
      </AuthProvider>
    </SignalRProvider>
  </ThemeProvider>
</QueryClientProvider>
```

---

## 9. Error Handling — 3 Tầng

1. **Route-level** (`app/**/error.tsx`) — catch unhandled errors cho toàn page
2. **Widget-level** — custom ErrorBoundary trong mỗi widget quan trọng → inline error + retry button
3. **Feature-level** — try/catch trong mỗi action → toast error qua `sonner`

---

## 10. SignalR Hub Architecture

### Global Hubs (connect khi authenticated)
- `PresenceHub` — user online/offline, typing indicators
- `NotificationHub` — new message, like, follow, livestream start

### Scoped Hubs (connect/disconnect theo screen lifecycle)
- `LiveRoomHub:{roomId}` — chat, gift events, viewer join/leave, kick, end stream
- `PrivateCallHub:{callId}` — call state, coin warning, end call
- `ChatHub:{conversationId}` — message received, read receipt, typing

---

## 11. Cache Tag Taxonomy

```typescript
// shared/lib/cache/tags.ts
export const CacheTags = {
  user: (id: string) => `user:${id}`,
  userProfile: (id: string) => `user-profile:${id}`,
  liveRooms: () => 'live-rooms',
  liveRoom: (id: string) => `live-room:${id}`,
  giftCatalog: () => 'gift-catalog',
  leaderboard: (period: string) => `leaderboard:${period}`,
  roomLeaderboard: (roomId: string) => `room-leaderboard:${roomId}`,
  conversations: (userId: string) => `conversations:${userId}`,
  transactions: (userId: string) => `transactions:${userId}`,
  coinBalance: (userId: string) => `coin-balance:${userId}`,
} as const
```

---

## 12. Enforce Conventions — Tooling

- **`eslint-plugin-boundaries`** — enforce layer dependency rules (feature không import widget, entity không import feature)
- **Barrel file convention** — mỗi slice PHẢI có `index.ts`, import từ bên ngoài PHẢI qua `index.ts`
- **Template generator** — `pnpm gen:feature <name>` tạo scaffold folder structure cho new feature slice

---

## 13. ⚠️ Fix Quan Trọng — Tailwind CSS Content Glob

### Vấn đề
Khi dùng FSD với Next.js, `tailwind.config.js` mặc định thường chỉ scan các thư mục cũ như `src/pages/`, `src/components/`, `src/app/`. Sau khi chuyển sang FSD, các thư mục mới (`src/views/`, `src/features/`, `src/entities/`, `src/shared/`) **không được scan** → Tailwind không generate classes → toàn bộ layout bị vỡ (elements không có style, spacing sai, màu sắc không hiển thị).

### Triệu chứng
- Layout bị vỡ hoàn toàn sau khi refactor sang FSD
- Tailwind classes trong FSD layers không có effect
- Dev server chạy bình thường nhưng UI không có style

### Fix bắt buộc
Sau khi setup FSD, **LUÔN** cập nhật `tailwind.config.js` để scan toàn bộ `src/`:

```js
// tailwind.config.js
module.exports = {
  content: [
    './src/**/*.{js,ts,jsx,tsx,mdx}',  // ✅ Scan toàn bộ src/ — bao gồm tất cả FSD layers
    './app/**/*.{js,ts,jsx,tsx,mdx}',  // ✅ Scan Next.js app/ directory
  ],
  // ...
}
```

**KHÔNG dùng**:
```js
// ❌ Sẽ bỏ sót FSD layers
content: [
  './src/pages/**/*.{js,ts,jsx,tsx}',
  './src/components/**/*.{js,ts,jsx,tsx}',
  './src/app/**/*.{js,ts,jsx,tsx}',
]
```

### Áp dụng cho
- Tất cả Next.js projects dùng FSD trong workspace này
- Phải kiểm tra ngay sau khi setup FSD structure, trước khi viết bất kỳ component nào

---

## 13. Lợi Ích Cho Team

| Lợi ích | Mô tả |
|---|---|
| Parallel development | Mỗi feature slice independent — 2-3 developer cùng làm không conflict |
| Clear ownership | Bug trong gift flow → chỉ look vào `features/gift/` và `entities/gift/` |
| Onboarding speed | Developer mới chỉ cần hiểu FSD layer rules + đọc public APIs |
| Phase 2 scalability | Thêm feature mới = thêm feature slices, không refactor existing code |
