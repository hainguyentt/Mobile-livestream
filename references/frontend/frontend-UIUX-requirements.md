# Đề Xuất Chi Tiết Yêu Cầu UI/UX — Frontend
## App Livestream Hẹn Hò — Thị Trường Nhật Bản

**Phiên bản bổ sung**: 1.3.1  
**Ngày**: 2026-03-22  
**Mục đích**: Bổ sung section UI/UX Requirements vào tài liệu yêu cầu chính, kèm đề xuất open-source stack để tối ưu tốc độ ra MVP

---

## UX-01: Nguyên Tắc Thiết Kế Tổng Quan (Design Principles)

### UX-01-1: Phong Cách Thiết Kế Phù Hợp Thị Trường Nhật Bản

Người dùng Nhật Bản có kỳ vọng UI/UX khác biệt rõ rệt so với thị trường phương Tây. Các nghiên cứu về thiết kế ứng dụng Nhật (tham khảo Pairs, Tapple — hai dating app hàng đầu Nhật với hơn 20 triệu users mỗi app) cho thấy người dùng Nhật ưu tiên: cảm giác **an toàn và tin cậy** (信頼感), giao diện **sạch sẽ nhưng đủ thông tin** (không quá tối giản kiểu Western minimalist), và **tone màu ấm áp, mềm mại** thay vì quá nổi bật. Cụ thể, đề xuất thiết kế nên tuân thủ các nguyên tắc sau:

**Tone màu chính**: Gradient ấm áp (soft pink → coral hoặc warm purple → soft blue) cho giao diện user-facing; neutral palette (slate/zinc) cho admin dashboard. Tránh màu đỏ chói (red có nghĩa cảnh báo mạnh trong context Nhật). Nên tham khảo color palette của Pairs (gradient hồng-cam) và Tapple (gradient xanh dương-tím) — đều dùng màu pastel mềm mại.

**Typography**: Font hệ thống ưu tiên Noto Sans JP (Google Fonts, miễn phí, hỗ trợ đầy đủ Hiragana/Katakana/Kanji, tối ưu cho web rendering). Fallback chain: `"Noto Sans JP", "Hiragino Kaku Gothic ProN", "Yu Gothic", "Meiryo", sans-serif`. Kích thước base font tối thiểu 14px cho body text (người dùng 18–70 tuổi, cần đọc rõ), line-height 1.7 (tiếng Nhật cần line-height cao hơn Latin text).

**Spacing & Layout**: Đặc thù thiết kế Nhật thiên về "thông tin đủ" hơn là "negative space nhiều". Tuy nhiên, vì đây là app hẹn hò/giải trí (không phải e-commerce), nên giữ layout thoáng nhưng không quá trống. Card-based layout cho danh sách users/hosts, with comfortable padding (16px–24px).

**Iconography**: Sử dụng icon rounded, friendly style. Tránh icon quá geometric/angular — không phù hợp tone app hẹn hò Nhật.

### UX-01-2: Mobile-First Responsive Strategy

| Breakpoint | Tên | Đặc điểm |
|---|---|---|
| 360px – 428px | Mobile (primary) | Layout mặc định, bottom navigation, full-width cards |
| 429px – 768px | Tablet | 2-column grid cho danh sách, side panel cho chat |
| 769px+ | Desktop | 3-column layout, persistent sidebar, picture-in-picture cho livestream |

Theo requirement NFR-05-4, mobile 360px+ là target chính. Tỷ lệ mobile users ở Nhật cho dating apps đạt trên 95%, nên toàn bộ interaction flow phải được thiết kế mobile-first, desktop là phiên bản mở rộng.

### UX-01-3: Dark Mode

Hỗ trợ system preference detection (`prefers-color-scheme`) + manual toggle. Dark mode đặc biệt quan trọng cho trải nghiệm xem livestream — giảm chói mắt, tập trung vào video content. Đề xuất dark mode là default cho màn hình livestream viewer, light mode là default cho các màn hình khác.

---

## UX-02: Cấu Trúc Navigation & Luồng Màn Hình

### UX-02-1: Bottom Navigation (Mobile)

Bottom tab bar cố định gồm 5 tab chính, là mô hình chuẩn của các app hẹn hò/livestream Nhật hiện tại:

| Tab | Icon | Label (JP) | Label (EN) | Màn hình |
|---|---|---|---|---|
| 1 | Home icon | ホーム | Home | Feed livestream đang diễn ra + Gợi ý matching |
| 2 | Search icon | さがす | Discover | Tìm kiếm/Lọc user + Host |
| 3 | Live icon (có dot indicator) | ライブ | Live | Danh sách phòng live đang hoạt động |
| 4 | Chat icon (badge count) | メッセージ | Messages | Danh sách hội thoại |
| 5 | User icon | マイページ | My Page | Hồ sơ, Cài đặt, Coin, Lịch sử |

Badge notification hiển thị trên tab Chat (unread count) và tab Live (số host đang online trong danh sách follow).

### UX-02-2: Screen Map Tổng Quan

**Luồng Onboarding** (first-time user):
Splash Screen → Login/Register → Profile Setup (ảnh, tên, sở thích — wizard 3 bước) → Phone Verification → Home

**Luồng chính (authenticated user)**, tổ chức theo nhóm:

Nhóm Home & Discovery gồm: Home Feed (livestream cards + suggested users), User Profile View, Search & Filter, và Leaderboard (ranking host).

Nhóm Livestream gồm: Live Room List, Public Livestream Viewer (video + chat overlay + gift panel), Private Call Request, Private 1-1 Video Call, và Host Broadcasting Screen (chỉ cho host role).

Nhóm Messaging gồm: Conversation List, Chat Detail (1-1 text), và Block/Report Flow.

Nhóm Profile & Settings gồm: My Profile (edit mode), Coin Wallet (số dư + nạp tiền), Transaction History, Notification Settings, Privacy Settings, Language Toggle, và Account Management (password, delete account).

### UX-02-3: Admin Dashboard — Screen Map Riêng

Admin dashboard là ứng dụng web riêng biệt (cùng codebase Next.js nhưng route group tách biệt `/admin`), sử dụng sidebar navigation:

Sidebar gồm: Dashboard (overview KPIs), User Management (list, search, detail, ban), Livestream Monitor (đang live, lịch sử), Content Moderation (report queue, actions), Financial Management (revenue, transactions, withdrawal requests), Gift Management (CRUD gifts), và System Settings.

---

## UX-03: Chi Tiết Thiết Kế Từng Module

### UX-03-1: Onboarding & Authentication

**Màn hình Login/Register**: Layout đơn giản, logo app trung tâm phía trên, bên dưới là các nút đăng nhập. Ưu tiên hiển thị nút "LINEでログイン" (Login with LINE) nổi bật nhất (nút xanh LINE brand color #06C755, full-width) — vì LINE là super-app 96 triệu users tại Nhật, conversion rate cao nhất. Bên dưới là nút Google, Apple (nếu iOS), và cuối cùng là link "メールで登録" (Đăng ký bằng email). Đây phản ánh đúng thứ tự ưu tiên từ FR-01-3 (LINE là Must Have).

**Profile Setup Wizard**: 3 bước rõ ràng với progress indicator ở trên. Step 1: Upload ảnh đại diện (camera hoặc gallery, crop circle), nhập display name. Step 2: Chọn sở thích từ tag list (ví dụ: 映画/Phim, 音楽/Nhạc, グルメ/Ẩm thực, 旅行/Du lịch, ゲーム/Game...) — UI dạng chip selection, chọn tối đa 10 tags. Step 3: Giới thiệu bản thân (textarea, gợi ý template để giảm rào cản). Mỗi bước có nút "スキップ" (Skip) cho các field optional, nhưng ảnh đại diện và tên là bắt buộc.

**Phone Verification**: Sau khi hoàn thành profile setup, hiển thị modal giải thích lý do cần xác minh số điện thoại (an toàn, xác minh tuổi — theo FR-01-4). Input số điện thoại format Nhật (+81), gửi OTP 6 số qua SMS, auto-fill OTP trên mobile.

### UX-03-2: Home Feed

Layout dạng vertical scrollable feed, mỗi item là một card gồm hai loại xen kẽ:

**Live Now Card** (ưu tiên hiển thị trên cùng): Thumbnail lớn (aspect ratio 16:9 hoặc 3:4) với overlay "LIVE" badge (đỏ, pulse animation), tên host, viewer count icon + số, và tag danh mục. Tap vào để vào phòng livestream. Phần "Live Now" hiển thị dạng horizontal scroll carousel ở đầu feed, cho phép user swipe ngang xem các host đang live.

**Suggested User Card**: Card nhỏ hơn, dạng grid 2 cột. Gồm ảnh đại diện (rounded-lg), tên, tuổi, tag sở thích chung (highlight màu), nút Like (heart icon) overlay góc phải. Tap vào xem profile detail.

Pull-to-refresh ở trên cùng. Infinite scroll load thêm khi cuộn xuống.

### UX-03-3: Livestream Public Viewer (Critical Screen)

Đây là màn hình quan trọng nhất của ứng dụng, cần thiết kế đặc biệt kỹ lưỡng:

**Layout**: Full-screen video chiếm toàn bộ viewport. Tất cả UI elements overlay lên video với semi-transparent background.

**Top Area** (overlay, safe area top): Gồm host avatar + tên + follow button ở góc trái trên; viewer count + viewer avatars (horizontal stack, max 5 avatars hiển thị) ở góc phải trên; nút Close (X) góc phải trên cùng.

**Middle Area**: Vùng trống để xem video. Gift animations sẽ bay từ dưới lên hoặc từ trái sang phải trong vùng này. Khi có gift lớn (high-value), animation chiếm full-screen trong 2-3 giây.

**Bottom Area** (overlay): Chat messages scrollable (dạng transparent list, chữ trắng + shadow, giới hạn hiển thị 50 messages gần nhất theo NFR-03-4 với Redis Streams TTL 7 ngày). Bên dưới chat list là input bar: text input + nút gửi gift (icon quà), nút gửi message. Tap nút gift mở bottom sheet Gift Panel.

**Gift Panel** (Bottom Sheet): Kéo lên từ dưới, hiển thị grid quà ảo (icon + tên + giá coin). Mỗi quà có animation preview nhỏ. Hiển thị số dư coin hiện tại ở trên cùng panel. Nút "Nạp thêm coin" nếu không đủ. Tap gift → confirm → gửi → animation hiển thị trên livestream.

**Timer & Cost Display** (cho pay-per-minute rooms): Thanh ngang phía trên chat area, hiển thị: ⏱ thời gian đã xem + 💰 coin đã tiêu. Khi coin gần hết (dưới 5 phút remaining), hiển thị warning nhẹ (pulse amber).

**Leaderboard Mini Panel**: Bên cạnh viewer count, có nút nhỏ mở panel xem top gifters trong phòng hiện tại (FR-11-2). Hiển thị top 3 với crown/medal icon, scrollable list cho phần còn lại.

### UX-03-4: Private Call 1-1

**Request Flow**: Từ profile host hoặc trong livestream, user tap nút "プライベートコール" (Private Call). Hiển thị bottom sheet: giá coin/phút, estimated coin balance (bao nhiêu phút có thể call), nút "リクエスト送信" (Send Request). Sau khi gửi, hiển thị waiting state với animation (ripple effect trên avatar host).

**In-Call Screen**: Full-screen video call. Top overlay: timer đếm ngược + chi phí real-time (FR-05-6). Bottom overlay: nút Mute, Camera toggle, End Call (nút đỏ lớn). Gift button vẫn có thể gửi quà trong private call. Khi coin sắp hết (dưới 2 phút), hiển thị modal cảnh báo: "コインが残りわずかです。チャージしますか？" (Coin sắp hết. Nạp thêm?) với nút nạp nhanh.

**Host-side (nhận request)**: Push notification + in-app modal hiển thị: avatar + tên user gửi request, nút Accept (xanh) / Decline (xám). Timeout 30 giây nếu host không phản hồi → auto-decline.

### UX-03-5: Chat & Messaging

**Conversation List**: Giống iMessage/LINE layout. Mỗi row: avatar, tên, last message preview (truncate 1 dòng), timestamp, unread badge (dot hoặc count). Sort theo most recent. Swipe left để hiển thị actions: Block, Delete.

**Chat Detail**: Header: avatar + tên + online status indicator (green dot). Message bubbles: user bên phải (primary color), đối phương bên trái (gray). Hỗ trợ emoji và sticker (FR-06-2) — sticker panel dạng bottom sheet grid, tương tự LINE sticker UX (người dùng Nhật rất quen thuộc pattern này). Trạng thái đã đọc hiển thị "既読" (kidoku — read) dưới message gần nhất, đúng convention của LINE mà người Nhật quen thuộc (FR-06-4).

### UX-03-6: Coin Wallet & Payment

**Coin Wallet Screen**: Số dư coin lớn ở trung tâm trên (large font, accent color). Bên dưới: grid các gói nạp coin (FR-07-3) dạng card với giá ¥ + số coin nhận, gói "popular" được highlight. Bên dưới grid: hai nút payment method (Stripe — icon thẻ tín dụng/Apple Pay/Google Pay, và LINE Pay — icon LINE). Tap gói → chọn payment method → redirect payment flow → callback success/fail.

**Transaction History**: List view, mỗi row: icon loại giao dịch (nạp=green arrow up, tiêu=red arrow down, nhận=blue arrow in cho host), mô tả, số coin (+/-), timestamp. Filter theo loại giao dịch và khoảng thời gian.

### UX-03-7: Leaderboard & Ranking (FR-11)

**Tab trên Leaderboard Screen**: 3 toggle: 日間 (Daily) / 週間 (Weekly) / 月間 (Monthly).

**List Layout**: Numbered list, top 3 hiển thị đặc biệt (avatar lớn hơn, crown icon gold/silver/bronze). Từ vị trí 4 trở đi: compact list row với avatar nhỏ, tên, tổng coin, và badge nếu có (Top 10, Top 50, Rising Star theo FR-11-3). Infinite scroll cho full list.

**Top Gifters in Room** (FR-11-2): Mini leaderboard panel trong livestream viewer, chỉ hiển thị user gifters trong room hiện tại, cập nhật real-time.

### UX-03-8: Host Broadcasting Screen

**Pre-live Screen**: Preview camera, chọn title cho phiên live, chọn category/tags, toggle free/paid (nếu paid thì set coin/phút), nút "ライブ開始" (Start Live — nút lớn, accent color).

**During Live**: Camera viewfinder chiếm full-screen. Overlay tương tự viewer nhưng bổ sung: viewer join/leave notifications (toast nhỏ slide in từ trên), total gift count + coin earned (real-time), nút kick viewer (long-press avatar viewer → context menu), nút end livestream.

**Post-live Summary**: Modal hiển thị thống kê phiên (FR-04-7): tổng thời lượng, peak viewers, total gifts nhận, total coin earned. Nút share hoặc close.

---

## UX-04: Open-Source Stack Đề Xuất Cho MVP

Dựa trên tech stack đã chọn (Next.js 14+ / Tailwind CSS / Agora.io / SignalR) và nghiên cứu ecosystem hiện tại, đây là đề xuất open-source cụ thể cho từng layer:

### UX-04-1: UI Component Library — shadcn/ui (Primary)

| Tiêu chí | Đánh giá |
|---|---|
| **Library** | **shadcn/ui** (built on Radix UI + Tailwind CSS) |
| Lý do chọn | Copy-paste model → full ownership code, không dependency lock-in. Tích hợp native với Tailwind CSS (đã chọn). Next.js App Router first-class support. Active maintenance, community lớn nhất trong hệ Tailwind component libraries tính đến 2026. |
| Components sử dụng ngay | Button, Dialog, Sheet (bottom sheet), Tabs, Avatar, Badge, Card, Input, Select, Toast, DropdownMenu, Popover, ScrollArea, Skeleton (loading state) |
| Customization | Fork component code trực tiếp vào project → dễ customize theme (màu sắc, border-radius, spacing) cho phong cách Nhật Bản |
| Trade-off | Cần tự maintain code đã copy; nhưng với quy mô MVP đây là lợi thế vì team có toàn quyền kiểm soát |

**So sánh nhanh vs alternatives đã cân nhắc**:

MUI (Material UI) bị loại vì Material Design aesthetic quá "Google-style", khó customize cho tone ấm áp của dating app Nhật, và bundle size lớn. Mantine có component coverage tốt nhưng styling system riêng, không tận dụng được Tailwind CSS đã chọn. HeroUI (NextUI cũ) đẹp nhưng ecosystem component chưa đủ rộng cho tất cả use case cần thiết.

### UX-04-2: Animation & Motion

| Mục đích | Library | Lý do |
|---|---|---|
| Page transitions, micro-interactions, gesture | **Motion (Framer Motion)** `motion` | Library animation số 1 cho React. Declarative API. Hỗ trợ layout animations, gesture (drag, swipe), spring physics. Dùng cho: page transition, card animations, bottom sheet gesture, button feedback |
| Gift animations (quà ảo bay, hiệu ứng đặc biệt) | **Lottie React** `@lottiefiles/dotlottie-react` | Render After Effects animations dưới dạng JSON. File nhẹ hơn GIF 90%+. Dùng cho: gift effects (hoa, trái tim, pháo hoa...), celebration animations. Designer tạo animation trong After Effects → export Lottie JSON → frontend render |
| Floating hearts/reactions trong livestream | **Custom implementation** dùng Motion + CSS | Build custom `<FloatingReaction>` component: spawn element → animate translateY + opacity + random translateX → remove on complete. Pattern đã proven trong TikTok Live / Instagram Live |

### UX-04-3: Internationalization (i18n)

| Library | **next-intl** |
|---|---|
| Lý do | Designed specifically for Next.js App Router. Lightweight hơn i18next. Hỗ trợ ICU message syntax (xử lý tốt plural rules, gender, number formatting cho tiếng Nhật). Server Component support native. |
| Cấu trúc | `messages/ja.json` (primary), `messages/en.json` (secondary). Sử dụng locale-based routing: `/ja/...` và `/en/...`. Default locale: `ja`. |
| Lưu ý tiếng Nhật | Tiếng Nhật không có plural forms nhưng cần xử lý counter suffixes (助数詞), honorifics (敬語), và format date/number theo chuẩn Nhật (¥1,000, 2026年3月22日) |

### UX-04-4: PWA — Serwist (successor của next-pwa)

| Library | **@serwist/next** + **serwist** |
|---|---|
| Lý do | `next-pwa` đã ngừng phát triển. Serwist là fork chính thức, actively maintained, tương thích Next.js App Router, sử dụng Workbox dưới hood. Hỗ trợ offline fallback page, push notification integration, installable PWA manifest. |
| Cấu hình | Service worker cho precaching static assets, runtime caching cho API calls (stale-while-revalidate cho profile data, network-first cho chat messages), offline fallback page khi mất kết nối |

### UX-04-5: Real-time — SignalR Client

| Library | **@microsoft/signalr** |
|---|---|
| Đã xác nhận trong requirement | Section 6.6 đã chọn SignalR |
| Frontend usage | `HubConnectionBuilder` → connect tới ASP.NET Core SignalR hub. Dùng cho: livestream chat, viewer count real-time, gift event broadcast, notification push, coin balance update, private call signaling |
| Reconnection | Built-in automatic reconnection với exponential backoff. Hiển thị toast "接続中..." (Connecting...) khi đang reconnect |

### UX-04-6: Video — Agora.io

| Library | **agora-rtc-sdk-ng** (Agora Web SDK NG) + **agora-rtc-react** |
|---|---|
| Đã xác nhận trong requirement | Section 6.5 đã chọn Agora.io |
| Public Livestream | Agora `LIVE_BROADCASTING` mode: host là broadcaster, viewers là audience. Max 50 concurrent viewers (FR-04-2) |
| Private Call | Agora `COMMUNICATION` mode: 1-1 video call |
| Agora React SDK | Cung cấp React hooks (`useJoin`, `usePublish`, `useRemoteUsers`, `useLocalMicrophoneTrack`, `useLocalCameraTrack`) — giảm đáng kể boilerplate code |

### UX-04-7: State Management

| Library | **Zustand** (đã đề xuất trong requirement section 6.1) |
|---|---|
| Lý do | Lightweight (1.1kB gzipped), simple API, không boilerplate như Redux. TypeScript first-class. |
| Stores đề xuất | `useAuthStore` (user session, token), `useCoinStore` (balance, transactions), `useLiveStore` (current room state, viewers, chat messages), `useNotificationStore` (unread counts), `useChatStore` (conversations, messages) |

### UX-04-8: Form & Validation

| Library | **React Hook Form** + **Zod** |
|---|---|
| Lý do | React Hook Form: uncontrolled form approach, minimal re-renders, perfect cho mobile performance. Zod: TypeScript-first schema validation, share schema với backend nếu cần. shadcn/ui Form component đã built-in integration với React Hook Form + Zod. |
| Dùng cho | Registration form, profile edit, coin top-up amount, search filters, report form |

### UX-04-9: Data Fetching & Caching

| Library | **TanStack Query (React Query)** |
|---|---|
| Lý do | Server state management tách biệt khỏi client state (Zustand). Automatic caching, background refetch, optimistic updates, infinite scroll pagination. |
| Dùng cho | Fetch user profiles, conversation list, transaction history, leaderboard data, search results. Infinite scroll cho feed, chat history. |

### UX-04-10: Admin Dashboard

| Approach | **shadcn/ui + Recharts + TanStack Table** |
|---|---|
| Lý do | Thay vì dùng framework admin riêng (React-Admin), tận dụng cùng shadcn/ui component library cho consistency với main app. Giảm learning curve cho team. |
| Recharts | Charting library cho dashboard KPIs (revenue chart, DAU/MAU, top hosts). Lightweight, React-native, composable. |
| TanStack Table | Headless table library cho data tables (user list, transaction list, report queue). Sorting, filtering, pagination built-in. shadcn/ui đã có DataTable component built on TanStack Table. |

### UX-04-11: Additional Utilities

| Mục đích | Library | Ghi chú |
|---|---|---|
| Date formatting (JP) | **date-fns** + `date-fns/locale/ja` | Lightweight, tree-shakable. Format: "3月22日 14:30", "3時間前" (3 giờ trước) |
| Image optimization | **Next.js Image** (built-in) | Automatic WebP/AVIF, lazy loading, blur placeholder |
| Image crop (profile photo) | **react-easy-crop** | Lightweight crop UI, output crop area → backend process |
| Virtual list (chat, long lists) | **@tanstack/react-virtual** | Virtualized rendering cho danh sách chat messages dài, giảm DOM nodes |
| Toast notifications | **sonner** | Đã integrated với shadcn/ui. Stackable toasts, swipe to dismiss |
| Emoji picker | **@emoji-mart/react** | Full emoji picker, searchable, recent emojis. Hỗ trợ Japanese emoji conventions |

---

## UX-05: Performance & UX Quality Requirements (Frontend-Specific)

### UX-05-1: Loading States

Mọi data-fetching screen phải có skeleton loading UI (shadcn/ui Skeleton component). Không bao giờ hiển thị blank screen hoặc chỉ spinner. Skeleton nên phản ánh đúng layout của content sẽ hiển thị. Livestream room: hiển thị black background + "接続中..." text + loading indicator trong khi Agora SDK connecting.

### UX-05-2: Error States

Mỗi screen cần error state riêng: network error (hiển thị retry button), empty state (ví dụ: "まだフォローしているホストはいません" — Chưa follow host nào), permission denied. Error messages phải bằng tiếng Nhật (primary) với tone lịch sự (dùng です/ます form).

### UX-05-3: Haptic & Micro-feedback

Khi gửi Like: brief heart animation (scale up → scale down). Khi gửi gift: confirmation haptic (nếu device hỗ trợ via Vibration API). Button press: subtle scale feedback (0.97 → 1.0) via Motion. Pull-to-refresh: elastic overscroll effect.

### UX-05-4: Offline Resilience

PWA offline page hiển thị khi mất kết nối: thông báo "インターネット接続を確認してください" (Vui lòng kiểm tra kết nối internet) + retry button. Chat messages đã load được lưu trong local cache (TanStack Query persisted cache), user vẫn đọc được khi offline. Coin balance cuối cùng được cache hiển thị dưới dạng "最終更新: 14:30" (Cập nhật lần cuối: 14:30).

### UX-05-5: Accessibility (a11y)

shadcn/ui (built on Radix UI) đã cung cấp WCAG 2.1 AA compliance cho tất cả primitive components. Bổ sung: color contrast ratio tối thiểu 4.5:1 cho text, 3:1 cho large text và UI components. Focus visible rings cho keyboard navigation. Aria labels cho tất cả interactive elements, với Japanese text. Đặc biệt quan trọng khi target audience bao gồm người dùng đến 70 tuổi — font size lớn, touch target tối thiểu 44x44px.

---

## UX-06: Tổng Hợp Open-Source Dependencies (MVP)

Bảng tổng hợp tất cả frontend dependencies đề xuất, phân nhóm theo layer:

**Core Framework**: `next` (App Router), `react`, `react-dom`, `typescript`

**UI Components**: `shadcn/ui` (copy-paste, not installed), `@radix-ui/*` (auto-installed via shadcn), `tailwindcss`, `tailwind-merge`, `clsx`, `class-variance-authority`

**Animation**: `motion` (Framer Motion), `@lottiefiles/dotlottie-react`

**Real-time & Video**: `@microsoft/signalr`, `agora-rtc-sdk-ng`, `agora-rtc-react`

**State & Data**: `zustand`, `@tanstack/react-query`, `@tanstack/react-table`, `@tanstack/react-virtual`

**Forms**: `react-hook-form`, `zod`, `@hookform/resolvers`

**i18n**: `next-intl`

**PWA**: `@serwist/next`, `serwist`

**Payment**: `@stripe/stripe-js`, `@stripe/react-stripe-js` (LINE Pay uses server-side redirect — no frontend SDK needed)

**Utilities**: `date-fns`, `react-easy-crop`, `sonner`, `@emoji-mart/react`, `recharts` (admin), `lucide-react` (icons)

**Font**: `@fontsource/noto-sans-jp` hoặc `next/font/google` (Noto Sans JP)

---

## UX-07: Khuyến Nghị Bổ Sung

### UX-07-1: Figma Design System Trước Khi Code

Khuyến nghị mạnh mẽ: xây dựng Figma design system trước khi bắt đầu code, sử dụng shadcn/ui Figma kit (có sẵn miễn phí trên Figma Community) làm base, customize color tokens, typography scale, và spacing cho thị trường Nhật. Điều này giúp đảm bảo consistency giữa design và implementation, giảm back-and-forth giữa designer và developer.

**shadcn/ui Figma kit**: https://www.figma.com/design/G0YfrabMF4aqahDuv8wvlW/shadcn-ui--The-Ultimate-UI-Kit-for-Figma--Community-?node-id=1511-135261&t=83Ua2b8jfhTTZvD6-0

### UX-07-2: Prototype Key Flows Trước

Đề xuất prototype và user test 3 luồng quan trọng nhất trước khi full development: (1) Onboarding flow (register → profile setup → first live), (2) Livestream viewer experience (join → chat → send gift), và (3) Coin top-up flow (wallet → select package → pay → confirmation). Đây là 3 luồng trực tiếp ảnh hưởng đến conversion rate và doanh thu.

### UX-07-3: Sticker Pack Cho Thị Trường Nhật

Ngoài emoji chuẩn, cần đầu tư thiết kế ít nhất 1 bộ sticker gốc (20–30 sticker) phù hợp văn hóa Nhật. Sticker là phần cực kỳ quan trọng trong communication culture Nhật (tham khảo LINE sticker ecosystem). Có thể dùng Lottie animated stickers cho trải nghiệm tốt hơn.

### UX-07-4: Progressive Feature Unlocking

Để khuyến khích phone verification (FR-01-4), nên thiết kế UI theo mô hình "progressive unlock": sau đăng ký email, user chỉ thấy feed và basic profile view. Các tính năng livestream, chat, gift bị mờ (disabled state) với tooltip "電話番号認証が必要です" (Cần xác minh số điện thoại). Tạo động lực verify mà không block hoàn toàn trải nghiệm ban đầu.

---

Ethan, đây là đề xuất chi tiết về UI/UX frontend cho dự án. Mấy điểm tôi muốn highlight:

Về **open-source stack**, shadcn/ui + Motion + Lottie + TanStack Query là combo mạnh nhất hiện tại cho MVP trên Next.js + Tailwind. Toàn bộ đều free, actively maintained, và quan trọng nhất là **không tạo vendor lock-in** — team có full ownership code. Tổng estimated bundle size impact cho toàn bộ client-side dependencies khoảng ~150–180kB gzipped (trước tree-shaking), nằm trong mức chấp nhận được cho PWA target load time < 3 giây trên 4G (NFR-01-4).

Về **thị trường Nhật**, các quyết định UI (LINE-first login, "既読" read receipts, sticker culture, Noto Sans JP, tone lịch sự trong error messages) đều dựa trên hành vi thực tế của người dùng Nhật trong các app tương tự. Đây là những chi tiết nhỏ nhưng tạo khác biệt lớn về conversion và retention.

Nếu cần, tôi có thể đi sâu hơn vào bất kỳ module nào — ví dụ wireframe detail cho livestream screen, hoặc component architecture breakdown cho từng page.