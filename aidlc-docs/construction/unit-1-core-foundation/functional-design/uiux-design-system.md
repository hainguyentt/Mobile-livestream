# UI/UX Design System — Livestream Dating App (Thị Trường Nhật Bản)

**Phiên bản**: 1.0.0  
**Ngày tạo**: 2026-03-22  
**Áp dụng cho**: Unit 1 Core Foundation + toàn dự án

---

## 1. Design Decisions & Trade-off Analysis

### 1.1 Responsive Strategy

**Quyết định**: Mobile-First với Adaptive Layout (không phải Fluid Responsive)

**Trade-off Analysis**:

| Approach | Ưu điểm | Nhược điểm |
|---|---|---|
| Fluid Responsive (% widths) | Đơn giản, ít breakpoint | Trải nghiệm desktop kém, layout "stretched" |
| Adaptive Layout (fixed breakpoints) | Tối ưu từng device class, UX tốt hơn | Cần thiết kế nhiều layout hơn |
| Mobile-Only (no desktop) | Nhanh nhất cho MVP | Bỏ lỡ 5% desktop users, admin cần desktop |

**Lý do chọn Adaptive Layout**:
- 95%+ users Nhật dùng mobile cho dating apps → mobile layout là primary
- Admin dashboard cần desktop layout riêng (sidebar nav, data tables)
- Breakpoints: 360px (mobile), 768px (tablet), 1024px (desktop)
- Tailwind breakpoints: `sm:768px`, `lg:1024px` — đủ cho scope hiện tại

### 1.2 Color System

**Quyết định**: Warm Pink Gradient (PWA) + Neutral Slate (Admin)

**Trade-off Analysis**:

| Palette | Phù hợp JP market | Brand differentiation | Accessibility |
|---|---|---|---|
| Pink/Rose gradient (Pairs-style) | Cao | Trung bình (phổ biến) | Tốt (pastel) |
| Purple/Blue gradient (Tapple-style) | Cao | Tốt | Tốt |
| Red/Orange (aggressive) | Thấp | Cao | Kém (JP context) |

**Lý do chọn Pink/Rose**:
- Tone ấm áp, an toàn — phù hợp tâm lý người dùng Nhật (UX-01-1)
- Tránh đỏ chói (cảnh báo trong context Nhật)
- Pastel tones dễ đạt contrast ratio WCAG AA

### 1.3 Dark Mode Strategy

**Quyết định**: System preference detection + manual toggle, dark mode default cho livestream screens

**Lý do**: Livestream viewer cần dark background để tập trung vào video (UX-01-3). Các màn hình khác default light.

### 1.4 Navigation Pattern

**Quyết định**: Bottom Tab Bar (5 tabs) cho mobile PWA, Sidebar cho Admin

**Lý do**: Bottom navigation là chuẩn của Pairs, Tapple, LINE — người dùng Nhật quen thuộc (UX-02-1). Thumb-friendly trên mobile.

### 1.5 Component Library

**Quyết định**: shadcn/ui (copy-paste model) + Radix UI primitives

**Lý do**: Full ownership code, không vendor lock-in, tích hợp native với Tailwind CSS, Next.js App Router first-class support (UX-04-1).

---

## 2. Color Tokens

### 2.1 PWA — Primary Palette (Warm Pink)

```css
/* Primary — Pink/Rose */
--color-primary-50:  #fdf2f8;
--color-primary-100: #fce7f3;
--color-primary-200: #fbcfe8;
--color-primary-300: #f9a8d4;
--color-primary-400: #f472b6;
--color-primary-500: #ec4899;   /* Main brand color */
--color-primary-600: #db2777;   /* Hover state */
--color-primary-700: #be185d;   /* Active/pressed */
--color-primary-800: #9d174d;
--color-primary-900: #831843;

/* Gradient */
--gradient-brand: linear-gradient(135deg, #f472b6 0%, #fb7185 100%);
/* pink-400 → rose-400 — soft, warm, not aggressive */

/* LINE Brand */
--color-line: #06C755;          /* Exact LINE brand green */
--color-line-hover: #05b34c;

/* Semantic */
--color-success: #22c55e;       /* green-500 */
--color-warning: #f59e0b;       /* amber-500 */
--color-error:   #f43f5e;       /* rose-500 — NOT red-500 */
--color-info:    #3b82f6;       /* blue-500 */
```

### 2.2 Admin — Neutral Palette (Slate)

```css
--color-admin-bg:      #f8fafc;  /* slate-50 */
--color-admin-surface: #ffffff;
--color-admin-border:  #e2e8f0;  /* slate-200 */
--color-admin-text:    #0f172a;  /* slate-900 */
--color-admin-muted:   #64748b;  /* slate-500 */
--color-admin-accent:  #0f172a;  /* slate-900 — buttons */
```

### 2.3 Dark Mode Tokens

```css
/* Applied via .dark class on <html> */
--color-bg-dark:      #0f0f0f;
--color-surface-dark: #1a1a1a;
--color-border-dark:  #2a2a2a;
--color-text-dark:    #f5f5f5;
--color-muted-dark:   #a3a3a3;
```

---

## 3. Typography

### 3.1 Font Stack

```css
/* Primary — Japanese market */
font-family: 'Noto Sans JP', 'Hiragino Kaku Gothic ProN', 'Yu Gothic', 'Meiryo', sans-serif;

/* Monospace (code, OTP digits) */
font-family: 'JetBrains Mono', 'Fira Code', monospace;
```

### 3.2 Type Scale

| Token | Size | Weight | Line Height | Usage |
|---|---|---|---|---|
| `text-xs` | 12px | 400 | 1.5 | Captions, timestamps |
| `text-sm` | 14px | 400 | 1.7 | Body text, labels (JP min) |
| `text-base` | 16px | 400 | 1.7 | Default body |
| `text-lg` | 18px | 500 | 1.6 | Subheadings |
| `text-xl` | 20px | 600 | 1.4 | Page titles |
| `text-2xl` | 24px | 700 | 1.3 | Hero headings |
| `text-3xl` | 30px | 700 | 1.2 | Large display (coin balance) |

**Lưu ý**: Line-height 1.7 cho body text — tiếng Nhật cần cao hơn Latin text (UX-01-1).

---

## 4. Spacing System

Dùng Tailwind default spacing scale (4px base unit). Các giá trị thường dùng:

| Token | Value | Usage |
|---|---|---|
| `p-3` | 12px | Compact padding (badges, chips) |
| `p-4` | 16px | Card padding mobile |
| `p-6` | 24px | Card padding desktop |
| `gap-2` | 8px | Tight element spacing |
| `gap-4` | 16px | Standard element spacing |
| `gap-6` | 24px | Section spacing |

---

## 5. Component Patterns

### 5.1 Button Variants

```
primary:   bg-primary-500, hover:bg-primary-600, text-white, h-11 (44px min)
secondary: border-primary-500, text-primary-600, bg-transparent
ghost:     text-gray-600, hover:bg-gray-100
line:      bg-[#06C755], text-white, hover:bg-[#05b34c]
danger:    bg-rose-500, text-white
```

**Touch target**: Tất cả buttons tối thiểu `h-11` (44px) theo UX-05-5.

### 5.2 Input Pattern

```
Base:    border border-gray-300, rounded-lg, h-11, px-3, text-sm
Focus:   ring-2 ring-primary-500 border-primary-500
Error:   border-rose-500 ring-rose-500
Disabled: opacity-50 cursor-not-allowed bg-gray-50
```

### 5.3 Card Pattern

```
Base:    bg-white rounded-xl shadow-sm border border-gray-100
Hover:   shadow-md transition-shadow
Padding: p-4 (mobile), p-6 (desktop)
```

### 5.4 Form Field Pattern

```
Label:   text-sm font-medium text-gray-700, mb-1
Input:   h-11, full-width
Helper:  text-xs text-gray-500, mt-1
Error:   text-xs text-rose-500, mt-1
```

---

## 6. Animation & Motion

### 6.1 Micro-interactions

```css
/* Button press feedback */
button:active { transform: scale(0.97); }
transition: all 150ms ease;

/* Page transitions */
/* Fade + slide up: opacity 0→1, translateY 8px→0 */
duration: 200ms, ease-out

/* Skeleton pulse */
@keyframes pulse { 0%,100% { opacity:1 } 50% { opacity:0.5 } }
```

### 6.2 Loading States

- Skeleton UI cho tất cả data-fetching screens (không dùng spinner đơn độc)
- Skeleton phản ánh đúng layout của content

---

## 7. Accessibility Standards

- Color contrast: tối thiểu 4.5:1 cho text, 3:1 cho UI components
- Touch targets: tối thiểu 44x44px (UX-05-5)
- Focus rings: visible, `ring-2 ring-primary-500 ring-offset-2`
- Aria labels: tất cả interactive elements
- Semantic HTML: `<button>`, `<nav>`, `<main>`, `<section>`, `<form>`
- Error messages: `role="alert"` cho screen readers

---

## 8. shadcn/ui Components — Unit 1 Inventory

| Component | File | Usage |
|---|---|---|
| Button | `components/ui/button.tsx` | Tất cả CTAs |
| Input | `components/ui/input.tsx` | Form fields |
| Card | `components/ui/card.tsx` | Auth card, profile card |
| Badge | `components/ui/badge.tsx` | Verified badge, status |
| Skeleton | `components/ui/skeleton.tsx` | Loading states |
| Separator | `components/ui/separator.tsx` | Visual dividers |

---

## 9. Dependencies Cần Cài Thêm

```bash
# PWA
npm install clsx tailwind-merge class-variance-authority

# Admin (same)
npm install clsx tailwind-merge class-variance-authority
```

**Lưu ý**: shadcn/ui dùng copy-paste model — không `npm install shadcn`. Các component được copy trực tiếp vào `src/components/ui/`.
