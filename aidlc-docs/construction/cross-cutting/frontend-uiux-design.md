# Frontend UI/UX Design — Cross-Cutting Standard

**Phiên bản**: 1.3.1
**Ngày**: 2026-03-22
**Phạm vi**: Áp dụng cho tất cả Units (Unit 1 → Unit 5)
**Nguồn gốc**: Chuyển đổi từ `references/frontend/frontend-UIUX-requirements.md` thành tài liệu chính thức

---

## UX-01: Nguyên Tắc Thiết Kế Tổng Quan

### UX-01-1: Phong Cách Thiết Kế — Thị Trường Nhật Bản

**Tone màu**:
- PWA (user-facing): Gradient ấm áp — soft pink → coral (`from-pink-400 to-rose-400`)
- Admin dashboard: Neutral palette — slate/zinc
- Tránh màu đỏ chói (red = cảnh báo mạnh trong context Nhật)
- Tham khảo: Pairs (gradient hồng-cam), Tapple (gradient xanh dương-tím) — đều dùng pastel mềm mại

**Typography**:
- Font: `Noto Sans JP` (Google Fonts)
- Fallback: `"Hiragino Kaku Gothic ProN", "Yu Gothic", "Meiryo", sans-serif`
- Base font: tối thiểu 14px body text (target 18–70 tuổi)
- Line-height: 1.7 (tiếng Nhật cần cao hơn Latin text)

**Spacing & Layout**:
- Card-based layout, padding 16px–24px
- Thoáng nhưng không quá trống (dating/entertainment app, không phải e-commerce)
- Icon style: rounded, friendly — tránh geometric/angular

### UX-01-2: Mobile-First Responsive

| Breakpoint | Tên | Đặc điểm |
|---|---|---|
| 360px – 428px | Mobile (primary) | Layout mặc định, bottom navigation, full-width cards |
| 429px – 768px | Tablet | 2-column grid, side panel cho chat |
| 769px+ | Desktop | 3-column layout, persistent sidebar, PiP cho livestream |

Mobile users Nhật cho dating apps > 95% → toàn bộ interaction flow thiết kế mobile-first.

### UX-01-3: Dark Mode

- Hỗ trợ `prefers-color-scheme` + manual toggle
- Default dark: màn hình livestream viewer (giảm chói mắt, tập trung video)
- Default light: tất cả màn hình khác

---

## UX-02: Navigation & Screen Map

### UX-02-1: Bottom Navigation (Mobile PWA)

| Tab | Icon | Label JP | Label EN | Màn hình |
|---|---|---|---|---|
| 1 | Home | ホーム | Home | Feed livestream + Gợi ý matching |
| 2 | Search | さがす | Discover | Tìm kiếm/Lọc user + Host |
| 3 | Live (dot indicator) | ライブ | Live | Danh sách phòng live |
| 4 | Chat (badge count) | メッセージ | Messages | Danh sách hội thoại |
| 5 | User | マイページ | My Page | Hồ sơ, Cài đặt, Coin, Lịch sử |

Badge: Chat (unread count), Live (số host online trong follow list).

### UX-02-2: Screen Map — PWA

**Onboarding** (first-time): Splash → Login/Register → Profile Setup (wizard 3 bước) → Phone Verification → Home

**Authenticated user**:
- Home & Discovery: Home Feed, User Profile View, Search & Filter, Leaderboard
- Livestream: Live Room List, Public Viewer, Private Call Request, Private 1-1 Call, Host Broadcasting
- Messaging: Conversation List, Chat Detail, Block/Report Flow
- Profile & Settings: My Profile, Coin Wallet, Transaction History, Notification/Privacy Settings, Account Management

### UX-02-3: Admin Dashboard Navigation

Sidebar navigation (web app riêng biệt):
Dashboard (KPIs) → User Management → Livestream Monitor → Content Moderation → Financial Management → Gift Management → System Settings

---

## UX-03: Chi Tiết Thiết Kế Từng Module

### UX-03-1: Onboarding & Authentication

**Login/Register screen**:
- Logo app trung tâm phía trên
- Nút "LINEでログイン" nổi bật nhất — full-width, màu `#06C755` (LINE brand)
- Bên dưới: Google, Apple (iOS), link "メールで登録"
- Thứ tự ưu tiên phản ánh FR-01-3 (LINE = Must Have)

**Profile Setup Wizard** (3 bước, progress indicator):
- Step 1: Upload ảnh đại diện (crop circle) + display name (bắt buộc)
- Step 2: Chọn sở thích — chip selection, tối đa 10 tags (映画, 音楽, グルメ, 旅行, ゲーム...)
- Step 3: Bio textarea + gợi ý template
- Mỗi bước có nút "スキップ" cho optional fields

**Phone Verification**: Modal giải thích lý do → input số Nhật (+81) → OTP 6 số → auto-fill trên mobile

### UX-03-2: Home Feed

**Live Now Card** (ưu tiên trên cùng):
- Thumbnail 16:9 hoặc 3:4, overlay "LIVE" badge (đỏ, pulse animation)
- Tên host, viewer count, tag danh mục
- Horizontal scroll carousel ở đầu feed

**Suggested User Card**:
- Grid 2 cột, ảnh rounded-lg, tên, tuổi, tag sở thích chung (highlight)
- Like button (heart icon) overlay góc phải

Pull-to-refresh + infinite scroll.

### UX-03-3: Livestream Public Viewer (Critical Screen)

**Layout**: Full-screen video, tất cả UI overlay với semi-transparent background.

**Top overlay**: Host avatar + tên + follow button (trái); viewer count + viewer avatars stack max 5 (phải); Close (X) góc phải trên.

**Middle**: Vùng xem video. Gift animations bay trong vùng này. High-value gift → full-screen animation 2-3 giây.

**Bottom overlay**:
- Chat messages scrollable (transparent, chữ trắng + shadow, max 50 messages — Redis Streams TTL 7 ngày)
- Input bar: text input + nút gift (icon quà) + nút send

**Gift Panel** (Bottom Sheet):
- Grid quà ảo (icon + tên + giá coin)
- Số dư coin hiện tại ở trên
- Nút "Nạp thêm coin" khi không đủ
- Tap gift → confirm → gửi → animation

**Pay-per-minute bar**: ⏱ thời gian đã xem + 💰 coin đã tiêu. Cảnh báo amber khi < 5 phút remaining.

**Leaderboard Mini Panel**: Top gifters trong phòng, top 3 với crown/medal icon.

### UX-03-4: Private Call 1-1

**Request Flow**: Bottom sheet — giá coin/phút, estimated balance, nút "リクエスト送信". Waiting state với ripple animation.

**In-Call Screen**: Full-screen video. Top: timer + chi phí real-time. Bottom: Mute, Camera toggle, End Call (nút đỏ lớn). Gift button available. Cảnh báo modal khi coin < 2 phút: "コインが残りわずかです。チャージしますか？"

**Host nhận request**: Push notification + modal — avatar + tên, Accept (xanh) / Decline (xám). Timeout 30 giây → auto-decline.

### UX-03-5: Chat & Messaging

**Conversation List**: iMessage/LINE layout. Avatar, tên, last message preview, timestamp, unread badge. Swipe left → Block, Delete.

**Chat Detail**:
- Header: avatar + tên + online status (green dot)
- Bubbles: user phải (primary color), đối phương trái (gray)
- Sticker panel: bottom sheet grid, giống LINE UX
- "既読" (kidoku) dưới message gần nhất — convention LINE quen thuộc với người Nhật

### UX-03-6: Coin Wallet & Payment

**Wallet Screen**: Số dư coin lớn ở trung tâm. Grid gói nạp (giá ¥ + số coin, gói "popular" highlight). Hai nút payment: Stripe (thẻ/Apple Pay/Google Pay) + LINE Pay.

**Transaction History**: List — icon loại (nạp=green↑, tiêu=red↓, nhận=blue→), mô tả, số coin, timestamp. Filter theo loại + khoảng thời gian.

### UX-03-7: Leaderboard & Ranking

**Tabs**: 日間 / 週間 / 月間 (Daily/Weekly/Monthly)

**List**: Top 3 đặc biệt (avatar lớn, crown gold/silver/bronze). Từ vị trí 4: compact row với badge (Top 10, Top 50, Rising Star). Infinite scroll.

**Room Top Gifters**: Mini panel trong livestream viewer, real-time update.

### UX-03-8: Host Broadcasting Screen

**Pre-live**: Preview camera, title, category/tags, toggle free/paid (nếu paid → set coin/phút), nút "ライブ開始" (lớn, accent color).

**During Live**: Camera full-screen. Overlay: viewer join/leave toast, total gift + coin earned (real-time), nút kick viewer (long-press → context menu), nút end livestream.

**Post-live Summary**: Modal — tổng thời lượng, peak viewers, total gifts, total coin earned. Nút share hoặc close.

---

## UX-04: Open-Source Stack Chính Thức

### UI Components
- **shadcn/ui** (Radix UI + Tailwind CSS) — copy-paste model, full ownership
- Components: Button, Dialog, Sheet, Tabs, Avatar, Badge, Card, Input, Select, Toast, DropdownMenu, ScrollArea, Skeleton

### Animation & Motion
- **Motion (Framer Motion)** — page transitions, micro-interactions, gesture, spring physics
- **@lottiefiles/dotlottie-react** — gift animations (Lottie JSON, nhẹ hơn GIF 90%+)
- Custom `<FloatingReaction>` — floating hearts/reactions trong livestream (Motion + CSS)

### i18n
- **next-intl** — Next.js App Router native, ICU message syntax, server component support
- Cấu trúc: `messages/ja.json` (primary), `messages/en.json`. Default locale: `ja`
- Routing: `/ja/...` và `/en/...`

### PWA
- **@serwist/next** + **serwist** — successor của next-pwa, Workbox-based
- Offline fallback page, push notification, installable PWA manifest
- Caching: stale-while-revalidate cho profile data, network-first cho chat

### Real-time
- **@microsoft/signalr** — HubConnectionBuilder, auto-reconnect với exponential backoff
- Toast "接続中..." khi reconnecting

### Video
- **agora-rtc-sdk-ng** + **agora-rtc-react** — hooks: `useJoin`, `usePublish`, `useRemoteUsers`, `useLocalMicrophoneTrack`, `useLocalCameraTrack`
- Public livestream: `LIVE_BROADCASTING` mode
- Private call: `COMMUNICATION` mode

### State Management
- **Zustand** — stores: `useAuthStore`, `useCoinStore`, `useLiveStore`, `useNotificationStore`, `useChatStore`

### Forms & Validation
- **React Hook Form** + **Zod** — shadcn/ui Form component tích hợp sẵn

### Data Fetching
- **TanStack Query** — server state, caching, infinite scroll, optimistic updates

### Admin Dashboard
- **shadcn/ui** + **Recharts** (charts) + **TanStack Table** (data tables)

### Utilities

| Mục đích | Library |
|---|---|
| Date formatting (JP) | `date-fns` + `date-fns/locale/ja` |
| Image optimization | Next.js Image (built-in) |
| Image crop | `react-easy-crop` |
| Virtual list | `@tanstack/react-virtual` |
| Toast | `sonner` |
| Emoji picker | `@emoji-mart/react` |
| Icons | `lucide-react` |

---

## UX-05: Performance & Quality Requirements

### UX-05-1: Loading States
- Mọi data-fetching screen phải có skeleton loading (shadcn/ui Skeleton)
- Không bao giờ blank screen hoặc chỉ spinner
- Skeleton phản ánh đúng layout content sẽ hiển thị
- Livestream: black background + "接続中..." + loading indicator khi Agora connecting

### UX-05-2: Error States
- Mỗi screen cần: network error (retry button), empty state, permission denied
- Error messages bằng tiếng Nhật, tone lịch sự (です/ます form)
- Ví dụ empty state: "まだフォローしているホストはいません"

### UX-05-3: Micro-feedback
- Like: heart animation (scale up → down)
- Gift: confirmation haptic (Vibration API nếu device hỗ trợ)
- Button press: scale 0.97 → 1.0 (Motion)
- Pull-to-refresh: elastic overscroll effect

### UX-05-4: Offline Resilience
- PWA offline page: "インターネット接続を確認してください" + retry button
- Chat messages cached (TanStack Query persisted cache) — đọc được khi offline
- Coin balance cache: hiển thị "最終更新: HH:mm"

### UX-05-5: Accessibility
- shadcn/ui (Radix UI) cung cấp WCAG 2.1 AA cho primitive components
- Color contrast: tối thiểu 4.5:1 cho text, 3:1 cho large text và UI components
- Focus visible rings cho keyboard navigation
- Aria labels bằng tiếng Nhật cho tất cả interactive elements
- Touch target tối thiểu 44×44px (target audience đến 70 tuổi)
- Font size đủ lớn, không nhỏ hơn 14px

---

## UX-06: Dependencies Tổng Hợp (MVP)

```
# Core
next, react, react-dom, typescript

# UI
@radix-ui/*, tailwindcss, tailwind-merge, clsx, class-variance-authority

# Animation
motion, @lottiefiles/dotlottie-react

# Real-time & Video
@microsoft/signalr, agora-rtc-sdk-ng, agora-rtc-react

# State & Data
zustand, @tanstack/react-query, @tanstack/react-table, @tanstack/react-virtual

# Forms
react-hook-form, zod, @hookform/resolvers

# i18n
next-intl

# PWA
@serwist/next, serwist

# Payment
@stripe/stripe-js, @stripe/react-stripe-js

# Utilities
date-fns, react-easy-crop, sonner, @emoji-mart/react, recharts, lucide-react

# Font
next/font/google (Noto Sans JP)
```

---

## UX-07: Khuyến Nghị Áp Dụng Cho Các Units Tiếp Theo

### UX-07-1: Sticker Pack
- Tối thiểu 1 bộ sticker gốc (20–30 sticker) phù hợp văn hóa Nhật
- Ưu tiên Lottie animated stickers
- Sticker là phần quan trọng trong communication culture Nhật (tham khảo LINE sticker ecosystem)

### UX-07-2: Progressive Feature Unlocking
- Sau đăng ký email: user thấy feed + basic profile view
- Livestream, chat, gift bị mờ (disabled) với tooltip "電話番号認証が必要です"
- Tạo động lực verify mà không block hoàn toàn trải nghiệm ban đầu

### UX-07-3: Prototype Trước Khi Code (Unit 2+)
3 luồng cần prototype và user test trước khi full development:
1. Livestream viewer experience (join → chat → send gift)
2. Coin top-up flow (wallet → select package → pay → confirmation)
3. Private call flow (request → accept → in-call → end)
