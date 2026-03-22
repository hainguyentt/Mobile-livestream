# Component Architecture Tổng Thể & Theo Nhóm Màn Hình
## App Livestream Hẹn Hò — Thị Trường Nhật Bản

---

## PHẦN 1: KIẾN TRÚC TỔNG THỂ

### 1.1 Phương Pháp Luận: Feature-Sliced Design (FSD) Áp Dụng Cho Next.js App Router

Ethan, tôi đề xuất áp dụng **Feature-Sliced Design (FSD)** — một phương pháp luận kiến trúc frontend được thiết kế cho ứng dụng quy mô lớn, hiện đang là approach được đánh giá cao nhất cho Next.js App Router tính đến thời điểm hiện tại (tham khảo: feature-sliced.design/blog/nextjs-app-router-guide, publish Jan 2026).

**Tại sao FSD phù hợp với dự án này hơn các approach khác:**

Dự án có nhiều domain rõ ràng (Auth, Livestream, Chat, Payment, Matching, Moderation) và sẽ phát triển từ MVP sang Phase 2+ (group livestream, subscription, speed dating). Approach truyền thống (nhóm file theo loại: `components/`, `hooks/`, `services/`) sẽ nhanh chóng thành "spaghetti code" khi có 5–10 developer cùng làm việc. FSD giải quyết điều này bằng 3 nguyên tắc cốt lõi: nhóm code theo business domain (không phải theo loại file kỹ thuật), enforced dependency direction giữa các layers (layer trên chỉ import từ layer dưới, không bao giờ ngược lại), và mỗi module có public API rõ ràng qua `index.ts` — chỉ export những gì consumer cần.

So sánh nhanh với các alternatives: MVC/MVVM truyền thống tạo global `services/` bucket trở thành "dependency magnet", mọi feature đều import vào tạo coupling ngầm. Atomic Design tốt cho design system nhưng không trả lời câu hỏi "domain logic đặt ở đâu" và "làm sao ngăn cross-feature imports". FSD kết hợp ưu điểm của cả hai: có structure cho domain logic (features, entities) và có chỗ cho UI kit (shared layer), đồng thời có quy tắc dependency rõ ràng.

### 1.2 Layer Hierarchy — Ánh Xạ Vào Dự Án

FSD định nghĩa 6 layers, sắp xếp từ cao xuống thấp. Quy tắc bất biến: **layer trên chỉ được import từ layer dưới hoặc cùng level, không bao giờ ngược lại**.

**Layer `app`** (cao nhất) — Application shell, global providers, root layout. Trong context Next.js App Router, đây là thư mục `app/` chứa routing files và global wiring. Layer này chỉ làm "orchestration": import và compose từ các layer dưới, không chứa business logic.

**Layer `pages`** (composition layer) — Mỗi "page" là một Server Component lắp ráp widgets và features thành một screen hoàn chỉnh. Route file `app/**/page.tsx` chỉ đơn giản render `<SomePageComponent />` từ layer này. Cần phân biệt rõ: "pages" ở đây là FSD layer concept, không phải Next.js Pages Router.

**Layer `widgets`** — Các khối UI lớn, composite, gồm nhiều features/entities kết hợp. Ví dụ: `LivestreamViewer` (chứa video player + chat panel + gift panel + viewer list), `BottomNavigation`, `Header`, `LeaderboardPanel`. Widget thường là ranh giới tự nhiên cho `loading.tsx` / `error.tsx` boundaries.

**Layer `features`** — User interaction scenarios — đây là trái tim của ứng dụng. Mỗi feature là một "hành động" cụ thể của user: `send-gift`, `start-livestream`, `request-private-call`, `top-up-coin`, `send-message`, `login-with-line`. Feature chứa UI + model (hooks, state) + API (server actions, API calls) liên quan đến hành động đó.

**Layer `entities`** — Domain models và domain UI primitives. Ví dụ: `user` (UserAvatar, UserCard, user types), `livestream` (LiveRoomCard, room types), `coin` (CoinBalance display, transaction types), `gift` (GiftItem component, gift types), `message` (MessageBubble, message types). Entity không chứa business logic phức tạp, chỉ chứa data types, basic queries, và presentational components.

**Layer `shared`** (thấp nhất) — Business-agnostic code: UI kit (shadcn/ui components), utilities (`date-fns` wrappers, format helpers), config (env vars, constants), API client primitives (fetch wrappers, SignalR connection builder), types (shared DTOs). Layer này **không được import từ bất kỳ layer nào khác**.

### 1.3 Directory Structure Cụ Thể

```
project-root/
├── app/                              # Next.js App Router — ROUTING ONLY
│   ├── (main)/                       # Route group: authenticated user screens
│   │   ├── layout.tsx                # MainLayout: BottomNav + auth guard
│   │   ├── page.tsx                  # Home → renders <HomePage />
│   │   ├── discover/
│   │   │   └── page.tsx              # → renders <DiscoverPage />
│   │   ├── live/
│   │   │   ├── page.tsx              # Live list → renders <LiveListPage />
│   │   │   └── [roomId]/
│   │   │       ├── page.tsx          # Livestream viewer → renders <LiveRoomPage />
│   │   │       ├── loading.tsx       # Skeleton while Agora connects
│   │   │       └── error.tsx         # Error boundary for room
│   │   ├── call/
│   │   │   └── [callId]/
│   │   │       └── page.tsx          # Private call → renders <PrivateCallPage />
│   │   ├── messages/
│   │   │   ├── page.tsx              # Chat list → renders <MessagesPage />
│   │   │   └── [conversationId]/
│   │   │       └── page.tsx          # Chat detail → renders <ChatDetailPage />
│   │   ├── profile/
│   │   │   ├── page.tsx              # My profile → renders <MyProfilePage />
│   │   │   └── [userId]/
│   │   │       └── page.tsx          # Other user profile → renders <UserProfilePage />
│   │   ├── wallet/
│   │   │   ├── page.tsx              # Coin wallet → renders <WalletPage />
│   │   │   └── history/
│   │   │       └── page.tsx          # Transaction history
│   │   ├── leaderboard/
│   │   │   └── page.tsx              # Ranking → renders <LeaderboardPage />
│   │   └── settings/
│   │       └── page.tsx              # Settings → renders <SettingsPage />
│   │
│   ├── (auth)/                       # Route group: unauthenticated screens
│   │   ├── layout.tsx                # AuthLayout: no nav, centered
│   │   ├── login/
│   │   │   └── page.tsx
│   │   ├── register/
│   │   │   └── page.tsx
│   │   └── forgot-password/
│   │       └── page.tsx
│   │
│   ├── (onboarding)/                 # Route group: post-register setup
│   │   ├── layout.tsx                # OnboardingLayout: progress bar
│   │   ├── profile-setup/
│   │   │   └── page.tsx
│   │   └── phone-verify/
│   │       └── page.tsx
│   │
│   ├── admin/                        # Route group: admin dashboard (separate layout)
│   │   ├── layout.tsx                # AdminLayout: sidebar nav
│   │   ├── page.tsx                  # Dashboard overview
│   │   ├── users/
│   │   │   └── page.tsx
│   │   ├── livestreams/
│   │   │   └── page.tsx
│   │   ├── moderation/
│   │   │   └── page.tsx
│   │   ├── finance/
│   │   │   └── page.tsx
│   │   └── gifts/
│   │       └── page.tsx
│   │
│   ├── api/                          # Route Handlers (webhooks, callbacks)
│   │   ├── webhooks/
│   │   │   ├── stripe/route.ts
│   │   │   └── line-pay/route.ts
│   │   └── agora/
│   │       └── token/route.ts        # Agora token generation
│   │
│   ├── _providers/
│   │   └── Providers.tsx             # Global providers composition
│   ├── layout.tsx                    # Root layout: html, body, fonts, providers
│   ├── not-found.tsx
│   └── manifest.ts                   # PWA manifest
│
├── src/                              # FSD LAYERS — BUSINESS CODE
│   ├── pages/                        # FSD "pages" layer (composition)
│   │   ├── home/
│   │   │   ├── ui/
│   │   │   │   └── HomePage.tsx      # Server Component: compose widgets
│   │   │   └── index.ts              # Public API
│   │   ├── live-room/
│   │   │   ├── ui/
│   │   │   │   └── LiveRoomPage.tsx
│   │   │   └── index.ts
│   │   ├── private-call/
│   │   │   ├── ui/
│   │   │   │   └── PrivateCallPage.tsx
│   │   │   └── index.ts
│   │   ├── messages/
│   │   ├── chat-detail/
│   │   ├── discover/
│   │   ├── wallet/
│   │   ├── leaderboard/
│   │   ├── my-profile/
│   │   ├── user-profile/
│   │   ├── settings/
│   │   ├── login/
│   │   ├── register/
│   │   └── admin-dashboard/
│   │       ├── ui/
│   │       │   └── AdminDashboardPage.tsx
│   │       └── index.ts
│   │
│   ├── widgets/                      # Large composite UI blocks
│   │   ├── bottom-navigation/
│   │   │   ├── ui/
│   │   │   │   └── BottomNavigation.tsx    # "use client"
│   │   │   └── index.ts
│   │   ├── livestream-viewer/
│   │   │   ├── ui/
│   │   │   │   ├── LivestreamViewer.tsx    # "use client" — main container
│   │   │   │   ├── VideoOverlay.tsx
│   │   │   │   ├── ViewerHeader.tsx
│   │   │   │   └── ChatOverlay.tsx
│   │   │   ├── model/
│   │   │   │   └── useLivestreamRoom.ts    # Orchestration hook
│   │   │   └── index.ts
│   │   ├── livestream-broadcaster/
│   │   │   ├── ui/
│   │   │   │   ├── BroadcasterView.tsx     # "use client"
│   │   │   │   └── PreLiveSetup.tsx
│   │   │   └── index.ts
│   │   ├── private-call-view/
│   │   │   ├── ui/
│   │   │   │   └── PrivateCallView.tsx     # "use client"
│   │   │   └── index.ts
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
│   ├── features/                     # User interaction scenarios
│   │   ├── auth/
│   │   │   ├── login-email/
│   │   │   │   ├── ui/
│   │   │   │   │   └── LoginEmailForm.tsx    # "use client"
│   │   │   │   ├── model/
│   │   │   │   │   ├── login.schema.ts       # Zod schema
│   │   │   │   │   └── useLoginEmail.ts
│   │   │   │   ├── api/
│   │   │   │   │   └── loginEmail.action.ts  # Server Action
│   │   │   │   └── index.ts
│   │   │   ├── login-line/
│   │   │   │   ├── ui/
│   │   │   │   │   └── LoginLineButton.tsx
│   │   │   │   └── index.ts
│   │   │   ├── login-social/                 # Google, Apple
│   │   │   ├── register/
│   │   │   ├── logout/
│   │   │   ├── forgot-password/
│   │   │   ├── verify-phone/
│   │   │   └── delete-account/
│   │   │
│   │   ├── livestream/
│   │   │   ├── start-livestream/
│   │   │   │   ├── ui/
│   │   │   │   │   └── StartLiveButton.tsx
│   │   │   │   ├── model/
│   │   │   │   │   └── useStartLivestream.ts
│   │   │   │   ├── api/
│   │   │   │   │   └── startLivestream.action.ts
│   │   │   │   └── index.ts
│   │   │   ├── end-livestream/
│   │   │   ├── join-room/
│   │   │   ├── leave-room/
│   │   │   ├── send-room-chat/
│   │   │   └── kick-viewer/
│   │   │
│   │   ├── gift/
│   │   │   ├── send-gift/
│   │   │   │   ├── ui/
│   │   │   │   │   ├── SendGiftButton.tsx
│   │   │   │   │   └── GiftConfirmSheet.tsx
│   │   │   │   ├── model/
│   │   │   │   │   └── useSendGift.ts
│   │   │   │   ├── api/
│   │   │   │   │   └── sendGift.action.ts
│   │   │   │   └── index.ts
│   │   │   └── gift-animation/
│   │   │       ├── ui/
│   │   │       │   ├── GiftAnimationLayer.tsx
│   │   │       │   └── FloatingReaction.tsx
│   │   │       ├── model/
│   │   │       │   └── useGiftAnimation.ts
│   │   │       └── index.ts
│   │   │
│   │   ├── private-call/
│   │   │   ├── request-call/
│   │   │   ├── accept-call/
│   │   │   ├── decline-call/
│   │   │   └── end-call/
│   │   │
│   │   ├── matching/
│   │   │   ├── like-user/
│   │   │   ├── follow-user/
│   │   │   ├── search-users/
│   │   │   └── filter-users/
│   │   │
│   │   ├── chat/
│   │   │   ├── send-message/
│   │   │   ├── send-sticker/
│   │   │   ├── block-user/
│   │   │   └── delete-message/
│   │   │
│   │   ├── payment/
│   │   │   ├── top-up-stripe/
│   │   │   ├── top-up-line-pay/
│   │   │   └── withdraw-request/      # Host rút tiền
│   │   │
│   │   ├── notification/
│   │   │   ├── push-permission/
│   │   │   └── notification-settings/
│   │   │
│   │   ├── profile/
│   │   │   ├── edit-profile/
│   │   │   └── upload-photos/
│   │   │
│   │   ├── moderation/
│   │   │   ├── report-user/
│   │   │   ├── report-content/
│   │   │   └── admin-ban-user/         # Admin-specific
│   │   │
│   │   └── leaderboard/
│   │       ├── view-ranking/
│   │       └── room-top-gifters/
│   │
│   ├── entities/                     # Domain models + domain UI
│   │   ├── user/
│   │   │   ├── model/
│   │   │   │   ├── types.ts            # User, Host, UserProfile interfaces
│   │   │   │   └── session.ts          # Session helpers (get current user)
│   │   │   ├── api/
│   │   │   │   └── user.queries.ts     # getUserById, searchUsers
│   │   │   ├── ui/
│   │   │   │   ├── UserAvatar.tsx      # Reusable avatar with online indicator
│   │   │   │   ├── UserCard.tsx        # Card in feed/search results
│   │   │   │   ├── UserBadge.tsx       # Verified badge, rank badge
│   │   │   │   └── OnlineStatus.tsx
│   │   │   └── index.ts
│   │   │
│   │   ├── livestream/
│   │   │   ├── model/
│   │   │   │   └── types.ts            # LiveRoom, LiveSession, ViewerInfo
│   │   │   ├── api/
│   │   │   │   └── livestream.queries.ts
│   │   │   ├── ui/
│   │   │   │   ├── LiveRoomCard.tsx     # Card in live list
│   │   │   │   ├── LiveBadge.tsx        # "LIVE" pulsing badge
│   │   │   │   └── ViewerCount.tsx
│   │   │   └── index.ts
│   │   │
│   │   ├── coin/
│   │   │   ├── model/
│   │   │   │   └── types.ts            # CoinBalance, Transaction, TopUpPackage
│   │   │   ├── api/
│   │   │   │   └── coin.queries.ts
│   │   │   ├── ui/
│   │   │   │   ├── CoinBalance.tsx      # Display balance with icon
│   │   │   │   ├── CoinAmount.tsx       # Formatted coin number
│   │   │   │   └── TransactionRow.tsx
│   │   │   └── index.ts
│   │   │
│   │   ├── gift/
│   │   │   ├── model/
│   │   │   │   └── types.ts            # Gift, GiftCategory
│   │   │   ├── api/
│   │   │   │   └── gift.queries.ts
│   │   │   ├── ui/
│   │   │   │   └── GiftItem.tsx         # Gift icon + name + price
│   │   │   └── index.ts
│   │   │
│   │   ├── message/
│   │   │   ├── model/
│   │   │   │   └── types.ts            # Message, Conversation
│   │   │   ├── ui/
│   │   │   │   ├── MessageBubble.tsx
│   │   │   │   ├── ConversationRow.tsx
│   │   │   │   └── ReadReceipt.tsx      # "既読" display
│   │   │   └── index.ts
│   │   │
│   │   └── notification/
│   │       ├── model/
│   │       │   └── types.ts
│   │       ├── ui/
│   │       │   └── NotificationBadge.tsx
│   │       └── index.ts
│   │
│   └── shared/                       # Business-agnostic, reusable
│       ├── ui/                       # shadcn/ui components + custom primitives
│       │   ├── button.tsx             # shadcn Button (copied)
│       │   ├── dialog.tsx
│       │   ├── sheet.tsx              # Bottom sheet
│       │   ├── tabs.tsx
│       │   ├── input.tsx
│       │   ├── avatar.tsx
│       │   ├── badge.tsx
│       │   ├── card.tsx
│       │   ├── skeleton.tsx
│       │   ├── toast.tsx              # sonner integration
│       │   ├── scroll-area.tsx
│       │   ├── dropdown-menu.tsx
│       │   └── ...                    # Other shadcn components
│       ├── lib/
│       │   ├── api-client.ts          # Fetch wrapper with auth headers
│       │   ├── signalr/
│       │   │   ├── connection.ts       # HubConnectionBuilder factory
│       │   │   ├── useSignalR.ts       # Core SignalR hook
│       │   │   └── types.ts
│       │   ├── agora/
│       │   │   ├── client.ts           # Agora client factory
│       │   │   ├── useAgoraClient.ts   # Core Agora hook
│       │   │   └── types.ts
│       │   ├── format/
│       │   │   ├── date.ts             # date-fns/locale/ja wrappers
│       │   │   ├── currency.ts         # ¥ formatting
│       │   │   └── number.ts
│       │   ├── cache/
│       │   │   └── tags.ts             # Centralized cache tag taxonomy
│       │   └── utils.ts               # cn(), clsx helpers
│       ├── config/
│       │   ├── env.ts                 # Type-safe env vars
│       │   ├── constants.ts           # App-wide constants
│       │   └── routes.ts             # Type-safe route paths
│       ├── hooks/
│       │   ├── useMediaQuery.ts
│       │   ├── useDebounce.ts
│       │   └── useIntersection.ts
│       └── types/
│           ├── api.ts                 # Generic API response types
│           └── common.ts              # Pagination, SortOrder, etc.
│
├── messages/                         # i18n (next-intl)
│   ├── ja.json
│   └── en.json
│
├── public/
│   ├── icons/                        # PWA icons
│   ├── lottie/                       # Lottie animation JSON files
│   │   ├── gift-heart.json
│   │   ├── gift-firework.json
│   │   ├── gift-rose.json
│   │   └── ...
│   └── stickers/                     # Sticker assets
│
└── [config files...]                 # tailwind.config.ts, next.config.ts, etc.
```

### 1.4 Server Component vs Client Component — Chiến Lược Ranh Giới

Đây là quyết định kiến trúc quan trọng nhất trong Next.js App Router. Nguyên tắc cốt lõi: **default everything to Server Component, chỉ đánh dấu `"use client"` khi buộc phải có interactivity**.

**Server Components** (không `"use client"`): Tất cả `app/**/page.tsx` và `app/**/layout.tsx`. Tất cả FSD `pages/` layer components (chúng là composition nodes, fetch data, render widgets). Tất cả `entities/*/api/*.queries.ts` (server-side data fetching). Tất cả `entities/*/ui/*` presentational components **nếu không có state/event handlers** (ví dụ: `UserCard` chỉ nhận props và render → Server Component). Toàn bộ admin dashboard pages (data-heavy, ít interactivity).

**Client Components** (`"use client"`): Widgets có interactivity: `LivestreamViewer`, `BottomNavigation`, `ChatRoom`, `GiftPanel`, `PrivateCallView`. Tất cả features có user interaction: `LoginEmailForm`, `SendGiftButton`, `StartLiveButton`, mọi form component. Components dùng browser APIs: Agora SDK, SignalR connection, Vibration API, Geolocation. Components dùng React hooks: `useState`, `useEffect`, `useRef`, event handlers (`onClick`, `onChange`).

**Ranh giới trong thực tế — "leaf-like" client components**: Giữ client components ở vị trí "lá" trong component tree — càng sâu càng tốt. Ví dụ: `LiveRoomPage` (Server Component) fetch room data → pass props vào `LivestreamViewer` (Client Component widget). Bên trong `LivestreamViewer`, nó là Client Component root, tất cả children tự động là client. Điều này đảm bảo Server Component xử lý data fetching + SEO + initial render, Client Component chỉ xử lý interactivity.

### 1.5 State Management Architecture

Thiết kế state management theo nguyên tắc phân tách rõ ràng **server state** vs **client state** vs **real-time state**:

**Server State (TanStack Query)** — dữ liệu có nguồn gốc từ backend API: user profiles, transaction history, leaderboard data, gift catalog, search results, conversation list. TanStack Query quản lý fetching, caching, background refetching, pagination (infinite scroll). Mỗi entity slice định nghĩa query keys và query functions riêng trong `entities/*/api/*.queries.ts`.

**Client State (Zustand stores)** — UI state và transient state không persist lên server:

`useAuthStore`: current user session, JWT tokens, refresh token rotation state. Persistent qua `zustand/middleware` persist to localStorage.

`useLiveRoomStore`: current room context khi đang trong livestream — room ID, host info, is-muted, is-camera-on, elapsed time, cost accumulated. Reset khi leave room.

`useCallStore`: private call state — call ID, partner info, timer, coin consumption rate. Reset khi end call.

`useUIStore`: theme mode (dark/light), bottom sheet open state, modal stack, toast queue. Không persist.

**Real-time State (SignalR → Zustand bridge)** — dữ liệu đẩy từ server qua SignalR, cần reactive update trên UI:

`useChatStore`: messages trong room chat hiện tại (append khi nhận message từ SignalR), typing indicators.

`usePresenceStore`: online/offline status của users đang follow, viewer list của room hiện tại (add/remove khi SignalR push join/leave events).

`useCoinBalanceStore`: coin balance real-time (update khi gift sent/received, top-up success). Đây là critical state — phải luôn chính xác.

`useNotificationStore`: unread counts cho chat, likes, follows. Update khi SignalR push notification events.

Pattern kết nối SignalR → Zustand: custom hook `useSignalRSubscription` trong `shared/lib/signalr/` nhận hub events và dispatch vào Zustand store tương ứng. Hook này được mount một lần tại `_providers/Providers.tsx` (global) cho presence & notification, và mount tại widget level cho room-specific events (chat messages, gift events).

### 1.6 Data Flow Pattern — End-to-End Cho Một Action Tiêu Biểu

Để minh họa cách tất cả layers phối hợp, tôi sẽ trace qua luồng **"User gửi gift trong livestream"**:

**Bước 1 — User tap nút gift trên UI**:
`widgets/livestream-viewer/ui/LivestreamViewer.tsx` (Client Component) → render `features/gift/send-gift/ui/SendGiftButton.tsx`. User taps button → mở `shared/ui/sheet.tsx` (Bottom Sheet) chứa gift grid.

**Bước 2 — Render gift catalog**:
`features/gift/send-gift/ui/GiftConfirmSheet.tsx` sử dụng TanStack Query call `entities/gift/api/gift.queries.ts` → `getGiftCatalog()` (cached, stale-while-revalidate). Render grid of `entities/gift/ui/GiftItem.tsx`. Hiển thị `entities/coin/ui/CoinBalance.tsx` (đọc từ `useCoinBalanceStore`).

**Bước 3 — User chọn gift, confirm**:
`features/gift/send-gift/model/useSendGift.ts` hook → validate coin balance đủ → call backend API `POST /api/gifts/send` (qua `shared/lib/api-client.ts` với auth header).

**Bước 4 — Backend xử lý → push SignalR event**:
Backend trừ coin, ghi transaction, push SignalR event `GiftReceived` tới tất cả viewers trong room + push `CoinBalanceUpdated` tới sender.

**Bước 5 — Frontend nhận SignalR events**:
`shared/lib/signalr/useSignalR.ts` nhận `GiftReceived` → dispatch tới `useChatStore` (hiển thị gift message trong chat) + dispatch tới `features/gift/gift-animation/model/useGiftAnimation.ts` (trigger animation). Nhận `CoinBalanceUpdated` → update `useCoinBalanceStore`.

**Bước 6 — Animation render**:
`features/gift/gift-animation/ui/GiftAnimationLayer.tsx` (luôn mounted trong `LivestreamViewer` widget) nhận gift event từ hook → load Lottie JSON từ `public/lottie/` → render animation overlay. Motion (Framer Motion) handle entrance/exit animations.

**Bước 7 — Cache invalidation**:
TanStack Query `invalidateQueries(['transactions'])` để background refetch transaction history. TanStack Query `invalidateQueries(['leaderboard', roomId])` để update room-level leaderboard.

---

## PHẦN 2: COMPONENT ARCHITECTURE THEO TỪNG NHÓM MÀN HÌNH

### 2.1 Nhóm Auth & Onboarding

```
app/(auth)/login/page.tsx                    ← Server Component (route entry)
  └─ src/views/login/ui/LoginPage.tsx        ← Server Component (composition)
      ├─ Logo + App title                    ← Static, server-rendered
      ├─ features/auth/login-line/ui/
      │   └─ LoginLineButton.tsx             ← "use client" — LINE OAuth redirect
      ├─ features/auth/login-social/ui/
      │   └─ SocialLoginButtons.tsx          ← "use client" — Google, Apple
      ├─ shared/ui/separator.tsx             ← Visual divider "または" (or)
      └─ features/auth/login-email/ui/
          └─ LoginEmailForm.tsx              ← "use client" — form + validation
              ├─ shared/ui/input.tsx
              ├─ shared/ui/button.tsx
              └─ model/login.schema.ts       ← Zod validation schema
```

**Đặc điểm kiến trúc nhóm Auth**: `LoginPage` là Server Component chỉ làm layout composition — không fetch data (login page không cần data). Các form/button là Client Components vì cần event handlers. `LoginLineButton` là component riêng biệt (không gộp với social login) vì LINE là Must Have priority và có flow OAuth riêng biệt — separate feature slice giúp team develop/test độc lập.

**Onboarding Wizard**: `ProfileSetupPage` sử dụng pattern multi-step form — một Client Component wrapper (`ProfileSetupWizard`) chứa internal step state (`useState` cho current step), render 3 step components. Mỗi step là feature slice riêng: `features/profile/upload-photos` (step 1), `features/profile/edit-profile` (step 2 — name, interests), và một inline step cho bio text. Cuối cùng submit tất cả cùng lúc qua single server action.

**Phone Verification** (`features/auth/verify-phone`): Component architecture tách biệt hoàn toàn — có UI riêng (`PhoneInput` + `OTPInput`), model riêng (`useVerifyPhone` hook quản lý countdown timer, resend cooldown), API riêng (server action gọi Twilio/AWS SNS). Sẽ được reuse ở Settings page nếu user muốn đổi số điện thoại.

### 2.2 Nhóm Home & Discovery

```
app/(main)/page.tsx                              ← Server Component
  └─ src/views/home/ui/HomePage.tsx              ← Server Component
      ├─ widgets/live-carousel/ui/
      │   └─ LiveCarousel.tsx                    ← "use client" — horizontal scroll, real-time
      │       ├─ entities/livestream/ui/
      │       │   └─ LiveRoomCard.tsx             ← Thumbnail + LiveBadge + ViewerCount
      │       ├─ entities/livestream/ui/
      │       │   └─ LiveBadge.tsx                ← Pulsing "LIVE" indicator
      │       └─ SignalR subscription             ← Real-time room list updates
      │
      ├─ widgets/user-feed/ui/
      │   └─ UserFeed.tsx                        ← "use client" — infinite scroll
      │       ├─ entities/user/ui/
      │       │   └─ UserCard.tsx                 ← Avatar + name + age + interest tags
      │       ├─ features/matching/like-user/ui/
      │       │   └─ LikeButton.tsx               ← Heart icon, optimistic update
      │       ├─ features/matching/follow-user/ui/
      │       │   └─ FollowButton.tsx
      │       └─ @tanstack/react-virtual          ← Virtualized list for performance
      │
      └─ widgets/leaderboard-panel/ui/
          └─ LeaderboardMini.tsx                 ← Top 3 hosts compact view
              └─ entities/user/ui/UserAvatar.tsx
```

**Đặc điểm kiến trúc nhóm Home**: `HomePage` là Server Component — nó fetch initial data (featured hosts, suggested users, live rooms) trên server, pass serialized data xuống Client Component widgets. `LiveCarousel` là Client Component vì cần: horizontal scroll gesture, real-time update khi host start/stop live (SignalR subscription), animation khi new room appears. `UserFeed` là Client Component vì cần: infinite scroll (`useInfiniteQuery` from TanStack Query), like/follow button interactions, virtualized rendering (performance cho danh sách dài).

**Discover Page** (`DiscoverPage`) có architecture tương tự nhưng bổ sung `features/matching/search-users` và `features/matching/filter-users` — cả hai là Client Components form-based. Filter panel dùng `shared/ui/sheet.tsx` (Bottom Sheet) trên mobile, sidebar trên desktop. Search input debounced 300ms (`shared/hooks/useDebounce.ts`) trước khi trigger TanStack Query refetch.

### 2.3 Nhóm Livestream — Public (Critical Path)

Đây là nhóm phức tạp nhất, tôi sẽ chi tiết hóa component tree:

```
app/(main)/live/[roomId]/page.tsx                     ← Server Component
  └─ src/views/live-room/ui/LiveRoomPage.tsx          ← Server Component
      │  (fetch room metadata, host info, validate access)
      │  (generate Agora token via server-side call)
      │
      └─ widgets/livestream-viewer/ui/
          └─ LivestreamViewer.tsx                      ← "use client" — ROOT CLIENT BOUNDARY
              │
              ├─ [Agora Layer]
              │   └─ shared/lib/agora/useAgoraClient.ts
              │       → AgoraRTCProvider                ← Agora React SDK context
              │       → RemoteVideoTrack                ← Host's video stream
              │       → Connection state management
              │
              ├─ [Video Overlay — Top]
              │   └─ ViewerHeader.tsx
              │       ├─ entities/user/ui/UserAvatar.tsx     ← Host avatar
              │       ├─ features/matching/follow-user/ui/
              │       │   └─ FollowButton.tsx                ← Follow host
              │       ├─ entities/livestream/ui/ViewerCount.tsx ← Real-time count
              │       ├─ ViewerAvatarStack.tsx                ← Horizontal avatar stack
              │       └─ CloseButton                          ← Navigate back
              │
              ├─ [Video Overlay — Middle]
              │   └─ features/gift/gift-animation/ui/
              │       └─ GiftAnimationLayer.tsx               ← Lottie animations overlay
              │           ├─ LottiePlayer (dotlottie-react)
              │           ├─ FloatingReaction.tsx              ← Hearts floating up
              │           └─ model/useGiftAnimation.ts        ← Queue + dequeue animations
              │
              ├─ [Video Overlay — Bottom]
              │   ├─ ChatOverlay.tsx
              │   │   ├─ ChatMessageList.tsx                  ← Virtualized, transparent
              │   │   │   ├─ entities/message/ui/
              │   │   │   │   └─ RoomChatBubble.tsx           ← Username + text, semi-transparent
              │   │   │   └─ Gift notification inline         ← "🎁 田中さん sent Rose"
              │   │   │
              │   │   └─ ChatInputBar.tsx
              │   │       ├─ shared/ui/input.tsx
              │   │       ├─ features/livestream/send-room-chat/
              │   │       │   └─ SendRoomChatAction            ← SignalR send
              │   │       └─ features/gift/send-gift/ui/
              │   │           └─ OpenGiftPanelButton.tsx        ← Icon button → open sheet
              │   │
              │   └─ PayPerMinuteBar.tsx                       ← Timer + cost display
              │       └─ entities/coin/ui/CoinAmount.tsx
              │
              ├─ [Bottom Sheet — Gift Panel]
              │   └─ widgets/gift-panel/ui/
              │       └─ GiftPanel.tsx                         ← shared/ui/sheet.tsx
              │           ├─ entities/coin/ui/CoinBalance.tsx  ← Current balance
              │           ├─ GiftGrid.tsx
              │           │   └─ entities/gift/ui/GiftItem.tsx ← Per gift (icon + price)
              │           ├─ features/gift/send-gift/ui/
              │           │   └─ GiftConfirmSheet.tsx           ← Confirm + send
              │           └─ features/payment/top-up-stripe/ui/
              │               └─ QuickTopUpLink.tsx             ← "コイン不足" prompt
              │
              ├─ [Bottom Sheet — Leaderboard]
              │   └─ widgets/leaderboard-panel/ui/
              │       └─ RoomLeaderboard.tsx
              │           ├─ features/leaderboard/room-top-gifters/
              │           │   └─ TopGifterList.tsx
              │           └─ entities/user/ui/UserAvatar.tsx
              │
              └─ [Orchestration Hook]
                  └─ widgets/livestream-viewer/model/
                      └─ useLivestreamRoom.ts
                          ├─ useAgoraClient()        ← Video connection
                          ├─ useSignalR('LiveRoom')   ← Chat + gift events
                          ├─ useLiveRoomStore()       ← Zustand room state
                          ├─ useChatStore()           ← Messages state
                          ├─ useGiftAnimation()       ← Animation queue
                          ├─ usePayPerMinute()        ← Timer + cost calculation
                          └─ Cleanup on unmount       ← Leave room, disconnect
```

**Đặc điểm kiến trúc nhóm Livestream**:

**Orchestration Hook (`useLivestreamRoom`)**: Đây là design pattern quan trọng nhất trong app — một "orchestration hook" nằm ở widget level, kết nối tất cả feature hooks và entity stores thành một luồng thống nhất. Hook này KHÔNG chứa business logic, nó chỉ wire các pieces lại với nhau. Lý do tách thành hook riêng thay vì viết trực tiếp trong component: testable (mock individual hooks), readable (component chỉ render, hook chỉ orchestrate), reusable (nếu Phase 2 có layout khác cho tablet/desktop, cùng hook, khác UI).

**Agora Integration Pattern**: `shared/lib/agora/useAgoraClient.ts` là thin wrapper quanh Agora React SDK, cung cấp simplified interface: `join(roomId, token, role)`, `leave()`, `remoteVideoTrack`, `connectionState`. Nó KHÔNG chứa business logic — chỉ encapsulate Agora SDK API. Feature `join-room` gọi hook này và xử lý business rules (check coin balance, increment viewer count, etc.).

**SignalR Multi-Hub Pattern**: Trong LivestreamViewer, cần subscribe 2 hub groups đồng thời: `LiveRoom:{roomId}` (chat messages, gift events, viewer join/leave) và `Presence` (global online status). Hook `useSignalR` trong `shared/lib/signalr/` quản lý connection lifecycle: connect on mount, auto-reconnect with exponential backoff, rejoin groups after reconnect, cleanup on unmount. Events được dispatch vào Zustand stores tương ứng — component chỉ subscribe store.

**Gift Animation Queue**: `useGiftAnimation` hook duy trì một queue (array of pending animations). Khi SignalR push gift event → enqueue. Component dequeue từng animation, render Lottie, đợi complete → dequeue next. Điều này prevent animation overlap và đảm bảo mỗi gift đều được hiển thị, kể cả khi nhiều gifts đến cùng lúc. High-value gifts (trên threshold) được ưu tiên và render full-screen.

**PayPerMinuteBar**: Component riêng track elapsed time và calculated cost. Dùng `useEffect` + `setInterval` (1 giây) để update timer. Tính cost = elapsed_minutes × rate_per_minute. So sánh với `useCoinBalanceStore` — khi remaining_minutes < 5 → show amber warning, < 2 → show red warning + modal prompt nạp coin. Khi balance = 0 → auto-leave room (call `features/livestream/leave-room`).

### 2.4 Nhóm Livestream — Private Call 1-1

```
app/(main)/call/[callId]/page.tsx                      ← Server Component
  └─ src/views/private-call/ui/PrivateCallPage.tsx     ← Server Component
      │  (fetch call metadata, validate both parties, generate Agora token)
      │
      └─ widgets/private-call-view/ui/
          └─ PrivateCallView.tsx                        ← "use client"
              │
              ├─ [Agora Layer — COMMUNICATION mode]
              │   ├─ LocalVideoTrack                    ← User's own camera
              │   ├─ RemoteVideoTrack                   ← Partner's video
              │   └─ Layout: PiP (small self, large partner)
              │
              ├─ CallHeader.tsx
              │   ├─ entities/user/ui/UserAvatar.tsx    ← Partner info
              │   ├─ CallTimer.tsx                      ← Elapsed time
              │   └─ entities/coin/ui/CoinAmount.tsx    ← Running cost
              │
              ├─ CallControls.tsx
              │   ├─ MuteButton                         ← Toggle mic
              │   ├─ CameraToggleButton                 ← Toggle camera
              │   ├─ features/gift/send-gift/ui/
              │   │   └─ SendGiftButton.tsx              ← Gift trong call
              │   └─ features/private-call/end-call/ui/
              │       └─ EndCallButton.tsx               ← Kết thúc (nút đỏ lớn)
              │
              ├─ CoinWarningModal.tsx                   ← Khi coin sắp hết
              │   └─ features/payment/top-up-stripe/ui/
              │       └─ QuickTopUpButton.tsx
              │
              └─ [Orchestration Hook]
                  └─ usePrivateCall.ts
                      ├─ useAgoraClient(COMMUNICATION)
                      ├─ useSignalR('PrivateCall')
                      ├─ useCallStore()
                      ├─ useCoinBalanceStore()
                      └─ useCallTimer()                 ← Interval + cost calc
```

**Đặc điểm kiến trúc**: Architecture gần giống Public Livestream nhưng đơn giản hơn — không có chat overlay, không có viewer list, không có leaderboard. Agora mode là `COMMUNICATION` (bidirectional) thay vì `LIVE_BROADCASTING` (unidirectional). Layout video dùng PiP pattern: partner video full-screen, self video thumbnail nhỏ góc phải trên (draggable via Motion gesture). `EndCallButton` trigger server action → backend emit SignalR event tới cả hai bên → cả hai unmount `PrivateCallView` → navigate back.

**Request/Accept Flow** (trước khi vào call page): `features/private-call/request-call` chứa `RequestCallSheet.tsx` — bottom sheet hiển thị ở profile page hoặc trong livestream. `features/private-call/accept-call` chứa `IncomingCallModal.tsx` — modal overlay hiện khi host nhận SignalR event `PrivateCallRequest`. Modal có 30s countdown (auto-decline), Accept/Decline buttons. Accept → backend tạo call session, generate Agora tokens cho cả hai, push `CallAccepted` event → cả hai navigate tới `/call/[callId]`.

### 2.5 Nhóm Chat & Messaging

```
app/(main)/messages/page.tsx                              ← Server Component
  └─ src/views/messages/ui/MessagesPage.tsx               ← Server Component
      │  (fetch conversation list server-side)
      │
      └─ widgets/conversation-list/ui/
          └─ ConversationList.tsx                          ← "use client"
              ├─ SearchInput                               ← Filter conversations
              ├─ VirtualizedList (@tanstack/react-virtual)
              │   └─ entities/message/ui/
              │       └─ ConversationRow.tsx                ← Avatar + name + preview + time + badge
              │           ├─ entities/user/ui/UserAvatar.tsx
              │           ├─ entities/user/ui/OnlineStatus.tsx
              │           └─ entities/notification/ui/NotificationBadge.tsx
              └─ SwipeActions (swipe left → Block, Delete)


app/(main)/messages/[conversationId]/page.tsx             ← Server Component
  └─ src/views/chat-detail/ui/ChatDetailPage.tsx          ← Server Component
      │  (fetch initial messages server-side, pass to client)
      │
      └─ widgets/chat-room/ui/
          └─ ChatRoom.tsx                                  ← "use client"
              │
              ├─ ChatHeader.tsx
              │   ├─ entities/user/ui/UserAvatar.tsx
              │   ├─ entities/user/ui/OnlineStatus.tsx
              │   └─ BackButton
              │
              ├─ MessageList.tsx
              │   ├─ @tanstack/react-virtual               ← Virtualized for performance
              │   ├─ useInfiniteQuery (load older messages)
              │   ├─ entities/message/ui/MessageBubble.tsx ← Right (self) / Left (other)
              │   ├─ entities/message/ui/ReadReceipt.tsx   ← "既読"
              │   └─ DateSeparator.tsx                     ← "3月22日"
              │
              ├─ ChatInputArea.tsx
              │   ├─ shared/ui/input.tsx
              │   ├─ features/chat/send-message/
              │   │   └─ SendMessageAction               ← SignalR send + optimistic update
              │   ├─ EmojiButton → @emoji-mart/react
              │   └─ features/chat/send-sticker/ui/
              │       └─ StickerButton → StickerPanel     ← Bottom sheet grid
              │
              └─ [Orchestration Hook]
                  └─ useChatRoom.ts
                      ├─ useSignalR('Chat:{conversationId}')
                      ├─ useChatStore()
                      ├─ useInfiniteQuery (messages history)
                      └─ Optimistic message handling
```

**Đặc điểm kiến trúc nhóm Chat**:

**Optimistic Updates**: Khi user gửi message, `features/chat/send-message` ngay lập tức append message vào `useChatStore` với status `sending` (hiển thị với opacity thấp hơn). Đồng thời gửi qua SignalR. Khi server confirm → update status thành `sent`. Khi đối phương đọc → server push `MessageRead` event → update status thành `read` → hiển thị "既読". Nếu send fail → update status thành `failed` + hiển thị retry button.

**Virtualized List**: Chat history có thể rất dài. `@tanstack/react-virtual` render chỉ visible items + buffer, giữ DOM node count thấp. Kết hợp `useInfiniteQuery` load thêm messages cũ khi scroll lên (reverse infinite scroll — scroll up to load more). Initial load: 50 messages gần nhất (server-side fetch trong `ChatDetailPage` Server Component, pass as `initialData` vào TanStack Query).

**Sticker Panel**: `features/chat/send-sticker` chứa `StickerPanel.tsx` — bottom sheet grid giống LINE sticker UX. Sticker assets load lazy từ CDN (CloudFront). Sticker metadata (id, category, thumbnail URL) cached trong TanStack Query. Gửi sticker = gửi message type `sticker` với sticker_id — render phía nhận sẽ map sticker_id → asset URL.

### 2.6 Nhóm Profile & Wallet

```
app/(main)/profile/page.tsx                               ← Server Component
  └─ src/views/my-profile/ui/MyProfilePage.tsx            ← Server Component
      │  (fetch current user profile server-side)
      │
      ├─ ProfileHeader.tsx                                 ← Static render
      │   ├─ entities/user/ui/UserAvatar.tsx (large)
      │   ├─ entities/user/ui/UserBadge.tsx
      │   ├─ Stats row (followers, following, gifts received)
      │   └─ features/profile/edit-profile/ui/
      │       └─ EditProfileButton.tsx                     ← Navigate to edit
      │
      ├─ PhotoGrid.tsx                                     ← 6 photo slots
      │   └─ features/profile/upload-photos/ui/
      │       └─ PhotoUploader.tsx                         ← "use client" — crop, upload
      │           └─ react-easy-crop
      │
      ├─ InterestTags.tsx                                  ← Chip list
      ├─ BioSection.tsx
      │
      └─ ProfileActions.tsx
          ├─ Coin Balance quick view → navigate to /wallet
          ├─ Settings link
          └─ features/auth/logout/ui/LogoutButton.tsx


app/(main)/wallet/page.tsx                                 ← Server Component
  └─ src/views/wallet/ui/WalletPage.tsx                   ← Server Component
      │
      ├─ BalanceHeader.tsx
      │   └─ entities/coin/ui/CoinBalance.tsx              ← Large display
      │
      ├─ TopUpPackageGrid.tsx                              ← "use client"
      │   ├─ PackageCard.tsx × N                           ← Price + coins + "人気" badge
      │   ├─ features/payment/top-up-stripe/ui/
      │   │   └─ StripeCheckoutButton.tsx                  ← @stripe/react-stripe-js
      │   └─ features/payment/top-up-line-pay/ui/
      │       └─ LinePayButton.tsx                          ← Server-side redirect
      │
      └─ RecentTransactions.tsx                            ← "use client"
          └─ entities/coin/ui/TransactionRow.tsx × N
              └─ Infinite scroll (TanStack Query)
```

**Đặc điểm kiến trúc**: Profile pages phần lớn là server-rendered static content — tốt cho initial load performance. Chỉ có interactive elements (`PhotoUploader`, `TopUpPackageGrid`) là Client Components. `PhotoUploader` sử dụng `react-easy-crop` cho crop UI, output crop coordinates, gửi lên backend xử lý resize/optimize, upload S3 → trả về CloudFront URL.

**Stripe Integration Pattern**: `StripeCheckoutButton` sử dụng `@stripe/react-stripe-js` → `Elements` provider → `useStripe` hook. Flow: user chọn package → `createPaymentIntent` server action → return client_secret → Stripe.js confirm payment → redirect tới success/fail page. Backend Stripe webhook (`app/api/webhooks/stripe/route.ts`) confirm payment → credit coins → push SignalR `CoinBalanceUpdated`.

**LINE Pay Integration Pattern**: Khác với Stripe (client-side SDK), LINE Pay dùng server-side redirect flow. `LinePayButton` gọi server action → backend tạo LINE Pay payment request → return redirect URL → `router.push(linePayUrl)` → user complete payment on LINE → LINE callback URL → backend verify → credit coins.

### 2.7 Nhóm Admin Dashboard

```
app/admin/layout.tsx                                       ← Server Component
  └─ AdminLayout
      ├─ widgets/admin-sidebar/ui/
      │   └─ AdminSidebar.tsx                              ← "use client" — collapsible
      │       └─ Navigation items with active state
      │
      └─ children (page content)


app/admin/page.tsx                                         ← Server Component
  └─ src/views/admin-dashboard/ui/AdminDashboardPage.tsx   ← Server Component
      │  (fetch KPIs server-side)
      │
      ├─ KPICards.tsx                                      ← DAU, MAU, Revenue, Active hosts
      │
      ├─ widgets/admin-revenue-chart/ui/
      │   └─ RevenueChart.tsx                              ← "use client" — Recharts
      │       └─ recharts (LineChart, BarChart)
      │
      └─ widgets/admin-user-table/ui/
          └─ RecentUsersTable.tsx                           ← "use client" — TanStack Table
              └─ @tanstack/react-table + shadcn DataTable


app/admin/moderation/page.tsx                              ← Server Component
  └─ src/views/admin-moderation/ui/AdminModerationPage.tsx
      │
      └─ widgets/admin-moderation-queue/ui/
          └─ ModerationQueue.tsx                           ← "use client"
              ├─ Report list (TanStack Table, sortable, filterable)
              ├─ ReportDetailPanel.tsx                     ← Slide-over panel
              │   ├─ Report content (screenshot, user info, timestamp)
              │   ├─ Linked livestream recording (if available)
              │   └─ Action buttons:
              │       ├─ features/moderation/admin-ban-user/ui/
              │       │   └─ BanUserButton.tsx
              │       ├─ DismissReportButton.tsx
              │       └─ WarnUserButton.tsx
              └─ features/livestream/kick-viewer/ui/
                  └─ KickViewerButton.tsx                  ← For active room monitoring
```

**Đặc điểm kiến trúc nhóm Admin**: Admin dashboard tách biệt hoàn toàn qua route group `app/admin/` với layout riêng (sidebar thay vì bottom nav). Phần lớn là data tables + charts → `TanStack Table` + `Recharts` là hai dependencies chính. Server-side data fetching cho KPIs (cached, revalidate mỗi 60 giây). Client-side cho interactive tables (sort, filter, pagination, inline actions).

Admin và main app share chung `entities/` layer (cùng user types, livestream types, coin types) và `shared/` layer (cùng UI kit, utilities). Nhưng `features/moderation/admin-ban-user` chỉ available cho admin — access control enforced ở cả route-level (middleware check admin role) và API-level (backend authorize).

---

## PHẦN 3: CROSS-CUTTING CONCERNS

### 3.1 Provider Composition (Global)

```typescript
// app/_providers/Providers.tsx — "use client"
// Composition root cho tất cả global providers

<QueryClientProvider>          // TanStack Query
  <ThemeProvider>              // Dark/light mode (next-themes)
    <SignalRProvider>           // Global SignalR connection (presence, notifications)
      <AuthProvider>            // Auth state from useAuthStore
        <IntlProvider>          // next-intl (locale messages)
          {children}
        </IntlProvider>
      </AuthProvider>
    </SignalRProvider>
  </ThemeProvider>
</QueryClientProvider>
```

`SignalRProvider` quản lý global connection lifecycle: connect sau khi user authenticated, auto-reconnect, expose connection instance qua Context. Room-specific subscriptions (LiveRoom, PrivateCall, Chat) được quản lý tại widget level — subscribe on mount, unsubscribe on unmount.

### 3.2 Error Handling Pattern

**3 tầng error boundaries**: Route-level (`app/**/error.tsx`) catch unhandled errors cho toàn bộ page. Widget-level: mỗi widget quan trọng (LivestreamViewer, ChatRoom) wrap trong custom ErrorBoundary — khi widget crash, hiển thị inline error + retry button thay vì crash toàn bộ page. Feature-level: từng feature action dùng try/catch, hiển thị toast error qua `sonner`.

### 3.3 Shared SignalR Hub Architecture

Frontend subscribe các SignalR hubs theo context:

**Global hubs** (connect khi authenticated, disconnect khi logout): `PresenceHub` — user online/offline events, typing indicators. `NotificationHub` — new message, like, follow, livestream start notifications.

**Scoped hubs** (connect/disconnect theo screen lifecycle): `LiveRoomHub:{roomId}` — chat messages, gift events, viewer join/leave, host end stream, kick events. `PrivateCallHub:{callId}` — call state changes, coin warning, end call signal. `ChatHub:{conversationId}` — message received, read receipt, typing indicator.

Mỗi hub connection được quản lý bởi `useSignalR(hubName, options)` hook trong `shared/lib/signalr/`. Hook return: `{ connection, connectionState, invoke, on, off }`. Events được bridge vào Zustand stores qua callback registration.

### 3.4 Cache Tag Taxonomy

```typescript
// shared/lib/cache/tags.ts
// Centralized cache tag definitions — shared between Server Components and Server Actions

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
} as const;
```

Khi feature slice thực hiện mutation (ví dụ: `send-gift` action), nó `revalidateTag(CacheTags.roomLeaderboard(roomId))` và `revalidateTag(CacheTags.transactions(userId))` — đảm bảo affected server-rendered data được refresh.

---

## PHẦN 4: TỔNG KẾT KIẾN TRÚC

### 4.1 Tóm Tắt Dependency Flow

```
app/ (routing) ──imports──→ src/views/ (composition)  ⚠️ KHÔNG dùng src/views/ — Next.js conflict
                                │
                          ──imports──→ src/widgets/ (composite UI blocks)
                                          │
                                    ──imports──→ src/features/ (user actions)
                                                    │
                                              ──imports──→ src/entities/ (domain models)
                                                              │
                                                        ──imports──→ src/shared/ (generic)
```

Hướng mũi tên là hướng dependency duy nhất được phép. `features/` KHÔNG import từ `widgets/`. `entities/` KHÔNG import từ `features/`. Vi phạm quy tắc này → ESLint rule cảnh báo (có thể dùng `eslint-plugin-import` hoặc `eslint-plugin-boundaries` để enforce tự động).

### 4.2 Lợi Ích Với ODC Team

Cấu trúc này mang lại 4 lợi ích chính cho team offshore development. Thứ nhất, **parallel development**: mỗi feature slice hoàn toàn independent — 2-3 developer có thể cùng lúc làm `send-gift`, `request-call`, `login-line` mà không conflict. Thứ hai, **clear ownership**: mỗi slice có public API rõ ràng — khi bug xảy ra trong gift flow, chỉ cần look vào `features/gift/` và `entities/gift/`. Thứ ba, **onboarding speed**: developer mới chỉ cần hiểu FSD layer rules + đọc public APIs, không cần navigate toàn bộ codebase. Thứ tư, **Phase 2 scalability**: thêm feature mới (group livestream, subscription) = thêm feature slices mới, không refactor existing code.

### 4.3 Enforce Conventions — Tooling

Để đảm bảo team tuân thủ architecture rules, đề xuất cài đặt: `eslint-plugin-boundaries` (enforce layer dependency rules — feature không import widget, entity không import feature), barrel file convention (mỗi slice PHẢI có `index.ts`, import từ bên ngoài slice PHẢI qua `index.ts`), và template generator (script `pnpm gen:feature <name>` tạo scaffold folder structure cho new feature slice — đảm bảo consistency).

---

Đây là component architecture đầy đủ, nhấn mạnh:

Kiến trúc này designed cho **team 5-10 developer làm việc parallel** — mỗi người claim feature slices riêng, merge không conflict vì boundary rõ ràng. `useLivestreamRoom` orchestration hook là pattern quan trọng nhất — nó là "glue" giữa Agora, SignalR, và Zustand, nhưng bản thân nó không chứa business logic, chỉ wire things together.
