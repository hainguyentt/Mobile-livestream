# UI/UX Design System Summary — Unit 1 Core Foundation

**Ngày hoàn thành**: 2026-03-22
**Trạng thái**: COMPLETED

---

## Tổng Quan

UI/UX Design System đã được xây dựng và áp dụng cho toàn bộ màn hình Unit-1, bao gồm PWA (user-facing) và Admin dashboard. Hệ thống dựa trên shadcn/ui components với custom design tokens tối ưu cho thị trường Nhật Bản.

---

## Design Tokens

### Color Palette — PWA (Japanese Market)

| Token | Value | Mô tả |
|---|---|---|
| `primary-50` | `#fdf2f8` | Soft pink tint — background |
| `primary-100` | `#fce7f3` | Light pink — hover states |
| `primary-500` | `#ec4899` | Main pink — buttons, accents |
| `primary-600` | `#db2777` | Dark pink — hover/active |
| `accent-line` | `#06C755` | LINE brand green (exact) |
| `success` | `green-500` | Success states |
| `warning` | `amber-500` | Warning states |
| `error` | `rose-500` | Error (not red — JP market) |
| `info` | `blue-500` | Info states |

**Gradient**: `from-pink-400 to-rose-400` — soft pink → coral (login background)

### Color Palette — Admin (Neutral)

| Token | Value | Mô tả |
|---|---|---|
| `slate-50` → `slate-900` | Slate scale | Neutral admin palette |
| `primary` | `slate-900` | Admin primary actions |

### Typography

| Property | Value |
|---|---|
| Font family | Noto Sans JP (Google Fonts) |
| Fallback | Hiragino Kaku Gothic ProN, Yu Gothic, Meiryo, sans-serif |
| Base size | 16px (default), 14px (min) |
| Line height | 1.7 (Japanese text spacing) |
| Font weights | 400 (regular), 500 (medium), 700 (bold) |

### Spacing & Touch Targets

| Property | Value | Lý do |
|---|---|---|
| Min touch target | 44×44px | UX-05-5 — mobile accessibility |
| Button height | `h-11` (44px) | Touch target compliance |
| Input height | `h-11` (44px) | Touch target compliance |
| Border radius | `rounded-xl` (12px) | Soft, modern feel |
| Card radius | `rounded-2xl` (16px) | Card containers |

---

## Component Inventory

### PWA Components (`app/frontend/pwa/src/components/ui/`)

| Component | File | Variants |
|---|---|---|
| Button | `button.tsx` | default, secondary, outline, ghost, line, destructive |
| Input | `input.tsx` | default (with error state support) |
| Card | `card.tsx` | Card, CardHeader, CardContent, CardFooter, CardTitle, CardDescription |
| Badge | `badge.tsx` | default, secondary, outline, destructive |
| Skeleton | `skeleton.tsx` | Base skeleton (composable) |
| Separator | `separator.tsx` | horizontal / vertical |

**Utility**: `app/frontend/pwa/src/lib/utils.ts` — `cn()` helper (clsx + tailwind-merge)

### Admin Components (`app/frontend/admin/src/components/ui/`)

| Component | File | Variants |
|---|---|---|
| Button | `button.tsx` | default (slate), secondary, outline, ghost, destructive |
| Input | `input.tsx` | default (with error state support) |

**Utility**: `app/frontend/admin/src/lib/utils.ts` — `cn()` helper

---

## Screens Refactored

### PWA Screens

| Screen | File | Changes |
|---|---|---|
| Login | `app/[locale]/login/page.tsx` | Gradient background, LINE button first (UX-03-1), branding area |
| Auth Form | `components/AuthForm.tsx` | shadcn Button + Input, LINE button prominent |
| Reset Password | `app/[locale]/reset-password/page.tsx` | Design system layout |
| OTP Input | `components/OtpInput.tsx` | 44px touch targets, shadcn Input style |
| Verify Phone | `app/[locale]/verify-phone/page.tsx` | Design system layout |
| Profile Edit | `app/[locale]/profile/edit/page.tsx` | shadcn Card + Input + Button |
| Profile Photos | `app/[locale]/profile/photos/page.tsx` | Design system layout |

### Admin Screens

| Screen | File | Changes |
|---|---|---|
| Admin Login | `app/login/page.tsx` | Neutral slate palette, shadcn components |

---

## Dependencies

### PWA (`app/frontend/pwa/`)

```bash
npm install clsx tailwind-merge class-variance-authority @radix-ui/react-separator
```

| Package | Version | Mục đích |
|---|---|---|
| `clsx` | latest | Conditional class names |
| `tailwind-merge` | latest | Merge Tailwind classes without conflicts |
| `class-variance-authority` | latest | Component variant management (CVA) |
| `@radix-ui/react-separator` | latest | Accessible separator primitive |

### Admin (`app/frontend/admin/`)

```bash
npm install clsx tailwind-merge class-variance-authority
```

---

## Configuration Files Updated

| File | Changes |
|---|---|
| `app/frontend/pwa/tailwind.config.js` | Custom colors, Noto Sans JP font, animations |
| `app/frontend/pwa/src/app/globals.css` | CSS variables, Noto Sans JP import, base styles |
| `app/frontend/admin/tailwind.config.js` | Neutral slate palette, admin-specific tokens |
| `app/frontend/admin/src/app/globals.css` | CSS variables for admin theme |

---

## Design Decisions & Trade-offs

### Quyết định: shadcn/ui thay vì custom components
- **Lý do**: shadcn/ui cung cấp accessible, well-tested primitives; copy-paste model cho phép full customization
- **Trade-off**: Cần install Radix UI primitives; nhưng bundle size nhỏ hơn full component library

### Quyết định: Pink/Rose palette cho PWA
- **Lý do**: Thị trường Nhật Bản ưa màu pastel, ấm áp; pink phù hợp với dating app context
- **Trade-off**: Không dùng red (quá aggressive cho JP market); rose-500 cho error states

### Quyết định: LINE button first (UX-03-1)
- **Lý do**: LINE là social platform #1 tại Nhật; user quen dùng LINE login
- **Trade-off**: Email login vẫn available nhưng secondary

### Quyết định: 44px minimum touch targets
- **Lý do**: Apple HIG và Material Design đều recommend 44px; UX-05-5 requirement
- **Trade-off**: Buttons/inputs cao hơn một chút so với desktop convention

---

## Before/After Comparison

### Before (Code Generation Phase)
- Inline Tailwind classes không nhất quán
- Không có design tokens
- Button/Input styles khác nhau ở mỗi screen
- Không có LINE button styling
- Font mặc định (không có Noto Sans JP)

### After (UI/UX Design System)
- Consistent design tokens qua CSS variables
- shadcn/ui components với variants rõ ràng
- LINE button với brand color `#06C755`
- Noto Sans JP với line-height 1.7 cho Japanese text
- 44px touch targets trên tất cả interactive elements
- Gradient background trên login screen (Japanese market feel)

---

## Tài Liệu Tham Khảo

- `aidlc-docs/construction/unit-1-core-foundation/functional-design/uiux-design-system.md` — Full design spec
- `references/frontend/frontend-UIUX-requirements.md` — UX requirements (UX-01 → UX-07)
- `references/frontend/frontend-component-architecture.md` — Component architecture
