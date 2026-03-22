---
inclusion: fileMatch
fileMatchPattern: "app/frontend/**"
---

# Frontend UI/UX & Architecture Standards

## Tài Liệu Tham Chiếu

Khi implement bất kỳ màn hình frontend nào, PHẢI tuân thủ 2 tài liệu cross-cutting sau:

- **UI/UX Design**: `aidlc-docs/construction/cross-cutting/frontend-uiux-design.md`
- **Component Architecture**: `aidlc-docs/construction/cross-cutting/frontend-component-architecture.md`

---

## Quy Tắc Bắt Buộc

### 1. Feature-Sliced Design (FSD)

Tất cả code frontend PHẢI tuân thủ FSD layer hierarchy:

```
app/ → src/views/ → src/widgets/ → src/features/ → src/entities/ → src/shared/
```

> ⚠️ **QUAN TRỌNG**: FSD composition layer PHẢI đặt tên là `src/views/`, **KHÔNG PHẢI** `src/pages/`. Next.js tự động scan tất cả thư mục tên `pages/` và conflict với App Router, gây lỗi "Conflicting app and page file" khi build.

- Layer trên chỉ import từ layer dưới — KHÔNG BAO GIỜ ngược lại
- `features/` KHÔNG import từ `widgets/`
- `entities/` KHÔNG import từ `features/`
- `shared/` KHÔNG import từ bất kỳ layer nào khác
- Mỗi slice PHẢI có `index.ts` làm public API

### 2. Server vs Client Components

- Default: Server Component
- Chỉ thêm `"use client"` khi cần: event handlers, browser APIs, React hooks (useState/useEffect/useRef)
- Giữ client components ở vị trí "lá" trong component tree

### 3. UI Components — shadcn/ui

- Dùng shadcn/ui components từ `src/shared/ui/` (copy-paste model)
- KHÔNG dùng MUI, Mantine, hoặc component library khác
- Customize qua Tailwind CSS và CSS variables — KHÔNG override inline styles

### 4. Design Tokens — Bắt Buộc

**PWA (user-facing)**:
- Primary: pink-500 (`#ec4899`) / gradient `from-pink-400 to-rose-400`
- LINE accent: `#06C755` (exact — không thay đổi)
- Error: rose-500 (KHÔNG dùng red-500 — quá aggressive cho JP market)
- Font: Noto Sans JP, line-height 1.7

**Admin**:
- Neutral slate palette
- Không dùng pink/gradient

### 5. Touch Targets

- Tối thiểu 44×44px cho tất cả interactive elements (UX-05-5)
- Button: `h-11` minimum
- Input: `h-11` minimum

### 6. i18n

→ Xem quy tắc đầy đủ tại `coding-standards-frontend.md` — Section "i18n Rules".

### 7. State Management

- Server state (API data): TanStack Query
- Client/UI state: Zustand stores
- Real-time state: SignalR → Zustand bridge qua `useSignalRSubscription`
- KHÔNG dùng Redux, Context API cho global state

### 8. Loading & Error States

- Mọi data-fetching screen PHẢI có skeleton loading (shadcn/ui Skeleton)
- KHÔNG để blank screen hoặc chỉ spinner
- Error messages bằng tiếng Nhật, tone lịch sự (です/ます form)

### 9. Orchestration Hook Pattern

Với widgets phức tạp (LivestreamViewer, PrivateCallView, ChatRoom):
- Tạo orchestration hook trong `widgets/{name}/model/use{Name}.ts`
- Hook chỉ wire các pieces lại — KHÔNG chứa business logic
- Component chỉ render, hook chỉ orchestrate

### 10. HTTP Error Handling trong Store

→ Xem quy tắc đầy đủ tại `coding-standards-frontend.md` — Section "HTTP Error Handling trong store".

### 11. ⚠️ Fix Quan Trọng — Tailwind CSS Content Glob

Sau khi setup FSD, **LUÔN** cập nhật `tailwind.config.js` để scan toàn bộ `src/`. Nếu không, Tailwind sẽ không generate classes cho các FSD layers mới → layout bị vỡ hoàn toàn.

```js
// tailwind.config.js — ĐÚNG
module.exports = {
  content: [
    './src/**/*.{js,ts,jsx,tsx,mdx}',  // Scan toàn bộ src/ — bao gồm views/, features/, entities/, shared/
    './app/**/*.{js,ts,jsx,tsx,mdx}',
  ],
}
```

Xem chi tiết tại: `aidlc-docs/construction/cross-cutting/frontend-component-architecture.md` — Section 13.

---

## Cấu Trúc Mỗi Widget Slice

```
widgets/{name}/
├── ui/
│   └── {WidgetName}.tsx       # Composed from features/entities
├── model/
│   └── use{WidgetName}.ts     # Orchestration hook — wire pieces, no business logic
└── index.ts                   # Public API
```

---

## Cấu Trúc Mỗi Feature Slice

```
features/{domain}/{feature-name}/
├── ui/
│   └── {FeatureName}.tsx     # "use client" nếu cần
├── model/
│   ├── {feature}.schema.ts   # Zod schema
│   └── use{Feature}.ts       # Hook
├── api/
│   └── {feature}.action.ts   # Server Action
└── index.ts                  # Public API — chỉ export những gì cần
```

---

## Cấu Trúc Mỗi Entity Slice

```
entities/{entity}/
├── model/
│   └── types.ts              # Domain types/interfaces
├── api/
│   └── {entity}.queries.ts   # TanStack Query functions
├── ui/
│   └── {EntityComponent}.tsx # Presentational components
└── index.ts                  # Public API
```

---

## Animation Guidelines

- Page transitions, micro-interactions: Motion (Framer Motion)
- Gift animations: Lottie (`@lottiefiles/dotlottie-react`), JSON files trong `public/lottie/`
- Floating reactions: Custom `<FloatingReaction>` component (Motion + CSS)
- Button press feedback: scale 0.97 → 1.0
- KHÔNG dùng CSS animations cho complex sequences — dùng Motion

---

## `src/views/` vs `app/` — Phân Chia Trách Nhiệm

- `app/[locale]/page.tsx` — Next.js route entry point: chỉ import View component, không chứa logic
- `src/views/{PageName}/` — FSD composition layer: orchestrate widgets/features cho một màn hình
- Logic, state, data fetching đặt trong `views/` hoặc `features/` — KHÔNG đặt trong `app/`

```tsx
// app/[locale]/profile/page.tsx — ĐÚNG: chỉ là entry point
export default function ProfilePage() {
  return <ProfileView />
}

// SAIÂ — đặt logic trực tiếp trong app/
export default function ProfilePage() {
  const { data } = useQuery(...)
  return <div>...</div>
}
```

---

## Accessibility Checklist

Trước khi hoàn thành bất kỳ screen nào:
- [ ] Tất cả interactive elements có `aria-label` bằng tiếng Nhật
- [ ] Form inputs có `<label>` liên kết
- [ ] Images có `alt` attributes
- [ ] Touch targets ≥ 44×44px
- [ ] Color contrast ≥ 4.5:1 cho text
- [ ] Focus visible rings cho keyboard navigation
- [ ] Semantic HTML (`<button>`, `<nav>`, `<main>`, `<section>`)
