# Code Generation Plan — UI/UX Design System (Unit 1 Additional Task)

**Unit**: Unit 1 — Core Foundation (Additional Task)
**Ngày tạo**: 2026-03-22
**Trạng thái**: COMPLETED

---

## Mục Tiêu

Xây dựng UI/UX Design System dùng chung cho toàn dự án, dựa trên:
- `references/frontend/frontend-UIUX-requirements.md` (UX-01 → UX-07)
- `references/frontend/frontend-component-architecture.md` (FSD architecture)
- shadcn/ui Figma Kit (https://www.figma.com/design/G0YfrabMF4aqahDuv8wvlW/...)

Sau đó áp dụng cho tất cả màn hình Unit-1 đã implement.

---

## Phạm Vi

### Part 1 — Design System Foundation (Documentation + Config)
Tạo tài liệu design system và cấu hình Tailwind/shadcn cho dự án.

### Part 2 — shadcn/ui Components Setup
Cài đặt và copy các shadcn/ui components cần thiết cho Unit-1.

### Part 3 — Apply to Unit-1 Screens (PWA)
Refactor tất cả màn hình PWA Unit-1 theo design system mới.

### Part 4 — Apply to Admin Login
Refactor admin login page theo design system.

---

## Execution Sequence

### Phase 1: Design System Documentation
- [x] Step 1: Tạo `aidlc-docs/construction/unit-1-core-foundation/functional-design/uiux-design-system.md`
  - Color tokens (Japanese market palette)
  - Typography scale (Noto Sans JP)
  - Spacing system
  - Component patterns
  - Dark mode strategy

### Phase 2: Tailwind Configuration
- [x] Step 2: Update `app/frontend/pwa/tailwind.config.ts` — thêm custom color tokens, font, animation
- [x] Step 3: Update `app/frontend/pwa/src/app/globals.css` — CSS variables cho design tokens

### Phase 3: shadcn/ui Components (PWA)
- [x] Step 4: Tạo `app/frontend/pwa/src/components/ui/button.tsx` (shadcn Button)
- [x] Step 5: Tạo `app/frontend/pwa/src/components/ui/input.tsx` (shadcn Input)
- [x] Step 6: Tạo `app/frontend/pwa/src/components/ui/card.tsx` (shadcn Card)
- [x] Step 7: Tạo `app/frontend/pwa/src/components/ui/badge.tsx` (shadcn Badge)
- [x] Step 8: Tạo `app/frontend/pwa/src/components/ui/skeleton.tsx` (shadcn Skeleton)
- [x] Step 9: Tạo `app/frontend/pwa/src/components/ui/separator.tsx` (shadcn Separator)
- [x] Step 10: Tạo `app/frontend/pwa/src/lib/utils.ts` (cn() helper)

### Phase 4: Refactor PWA Auth Screens
- [x] Step 11: Refactor `app/frontend/pwa/src/components/AuthForm.tsx`
  - Apply shadcn Button, Input
  - Apply Japanese market design (warm gradient, LINE button prominent)
  - Apply Noto Sans JP font class
- [x] Step 12: Refactor `app/frontend/pwa/src/app/[locale]/login/page.tsx`
  - Full-page layout với gradient background
  - App logo/branding area
  - LINE button first (UX-03-1)
- [x] Step 13: Refactor `app/frontend/pwa/src/app/[locale]/reset-password/page.tsx`
  - Apply design system

### Phase 5: Refactor PWA OTP Screen
- [x] Step 14: Refactor `app/frontend/pwa/src/components/OtpInput.tsx`
  - Apply shadcn Input style
  - Improved mobile UX (larger touch targets 44x44px)
- [x] Step 15: Refactor `app/frontend/pwa/src/app/[locale]/verify-phone/page.tsx`
  - Apply design system layout

### Phase 6: Refactor PWA Profile Screens
- [x] Step 16: Refactor `app/frontend/pwa/src/app/[locale]/profile/edit/page.tsx`
  - Apply shadcn Card, Input, Button
  - Improved form layout
- [x] Step 17: Refactor `app/frontend/pwa/src/app/[locale]/profile/photos/page.tsx`
  - Apply design system

### Phase 7: Refactor Admin Login
- [x] Step 18: Update `app/frontend/admin/tailwind.config.ts` — neutral palette (slate/zinc)
- [x] Step 19: Tạo `app/frontend/admin/src/components/ui/button.tsx` (shadcn Button — admin variant)
- [x] Step 20: Tạo `app/frontend/admin/src/components/ui/input.tsx`
- [x] Step 21: Refactor `app/frontend/admin/src/app/login/page.tsx`
  - Apply neutral admin design
  - Apply shadcn components

### Phase 8: Documentation
- [x] Step 22: Tạo `aidlc-docs/construction/unit-1-core-foundation/code/uiux-design-system-summary.md`
  - Tóm tắt design tokens
  - Component inventory
  - Usage guidelines
  - Before/After comparison

---

## Design Decisions

### Color Palette (Japanese Market)
```
Primary (PWA): 
  - primary-50: #fdf2f8  (soft pink tint)
  - primary-100: #fce7f3
  - primary-500: #ec4899  (pink-500 — warm, not aggressive)
  - primary-600: #db2777
  - gradient: from-pink-400 to-rose-400 (soft pink → coral)

Accent:
  - accent-line: #06C755  (LINE brand green — exact)

Neutral (Admin):
  - slate-50 → slate-900 palette

Semantic:
  - success: green-500
  - warning: amber-500  
  - error: rose-500 (NOT red — too aggressive for JP market)
  - info: blue-500
```

### Typography
```
Font: Noto Sans JP (Google Fonts)
Fallback: "Hiragino Kaku Gothic ProN", "Yu Gothic", "Meiryo", sans-serif
Base size: 14px (min) — 16px (default)
Line height: 1.7 (Japanese text needs more)
```

### Touch Targets
```
Minimum: 44x44px (UX-05-5)
Buttons: h-11 (44px) minimum
Inputs: h-11 (44px) minimum
```

### shadcn/ui Components Used in Unit-1
- Button (primary, secondary, ghost, line variants)
- Input (with label pattern)
- Card (auth card, profile card)
- Badge (verified badge)
- Skeleton (loading states)
- Separator

---

## Files to Create/Modify

### New Files
1. `aidlc-docs/construction/unit-1-core-foundation/functional-design/uiux-design-system.md`
2. `app/frontend/pwa/src/components/ui/button.tsx`
3. `app/frontend/pwa/src/components/ui/input.tsx`
4. `app/frontend/pwa/src/components/ui/card.tsx`
5. `app/frontend/pwa/src/components/ui/badge.tsx`
6. `app/frontend/pwa/src/components/ui/skeleton.tsx`
7. `app/frontend/pwa/src/components/ui/separator.tsx`
8. `app/frontend/pwa/src/lib/utils.ts`
9. `app/frontend/admin/src/components/ui/button.tsx`
10. `app/frontend/admin/src/components/ui/input.tsx`
11. `aidlc-docs/construction/unit-1-core-foundation/code/uiux-design-system-summary.md`

### Modified Files
1. `app/frontend/pwa/tailwind.config.ts` (hoặc .js)
2. `app/frontend/pwa/src/app/globals.css`
3. `app/frontend/pwa/src/components/AuthForm.tsx`
4. `app/frontend/pwa/src/app/[locale]/login/page.tsx`
5. `app/frontend/pwa/src/app/[locale]/reset-password/page.tsx`
6. `app/frontend/pwa/src/components/OtpInput.tsx`
7. `app/frontend/pwa/src/app/[locale]/verify-phone/page.tsx`
8. `app/frontend/pwa/src/app/[locale]/profile/edit/page.tsx`
9. `app/frontend/pwa/src/app/[locale]/profile/photos/page.tsx`
10. `app/frontend/admin/tailwind.config.ts`
11. `app/frontend/admin/src/app/login/page.tsx`

---

## Estimated Scope
- **New files**: 11
- **Modified files**: 11
- **Total steps**: 22
