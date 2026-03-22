# Frontend Unit 1 Recreation — Execution Plan

**Ngày tạo**: 2026-03-22
**Phạm vi**: Admin (Phase 1) → PWA (Phase 2)
**Chiến lược**: Incremental recreation với FSD architecture
**Trạng thái**: ✅ Phase 1 Complete | ✅ Phase 2 Complete

---

## Tổng Quan

Recreate toàn bộ frontend Unit 1 từ đầu với Feature-Sliced Design (FSD) architecture.
Thứ tự thực hiện: **Admin trước** (test ground, 1 screen) → **PWA sau** (8 screens).

---

## Artifacts Folder Structure

```
aidlc-docs/construction/frontend-recreation/
├── requirements.md              ✅ Change request & requirements
├── verification-questions.md    ✅ 14 answered questions
├── answer-clarifications.md     ✅ Clarifications
├── intent-analysis.md           ✅ Intent analysis
├── tradeoff-analysis.md         ✅ Tradeoff analysis
├── execution-plan.md            ✅ This file
├── design.md                    ✅ FSD structure design
└── code-generation-plan.md      ✅ Code generation checklist
```

---

## Key Decisions (từ 14 Verification Questions)

| # | Quyết định | Chi tiết |
|---|---|---|
| Q1 | `src/views/` layer riêng | Tách routing (app/) và composition (src/views/). ⚠️ Dùng `views/` thay `pages/` — Next.js conflict |
| Q2 | Tạo `src/widgets/` ngay | Chuẩn bị cho Unit 2+ |
| Q3 | Incremental recreation | Từng feature, test ngay |
| Q4 | LoginForm + RegisterForm riêng | Separate trong `features/auth/*/ui/` |
| Q5 | OtpInput trong `shared/ui/` | Shared UI primitive |
| Q6 | API clients trong `entities/*/api/` | Entity-based, domain-driven |
| Q7 | Stores trong `entities/*/model/` | Entity-based state |
| Q8 | Hybrid test strategy | Reuse scenarios, recreate code |
| Q9 | Hybrid i18n namespace | Centralized + namespace organization |
| Q10 | Reuse tất cả assets | shadcn/ui, translations, utilities |
| Q11 | Audit & selective update | Security vulnerabilities only |
| Q12 | **Admin trước, PWA sau** | CRITICAL execution order |
| Q13 | Archive `_archive/` folder | Safety net |
| Q14 | AI generate toàn bộ | Recreate from scratch |

---

## Phase 1: Admin Recreation ✅

**Mục tiêu**: Test ground cho FSD, 1 screen đơn giản
**Kết quả**: Hoàn thành — 12/12 tests pass

### Bước 1: Setup & Archive
- [x] Archive old admin code → `app/frontend/_archive/admin-old/`
- [x] Audit `package.json` dependencies
- [x] Update security vulnerabilities

### Bước 2: FSD Structure Setup (Admin)
- [x] Tạo FSD folder structure cho admin
- [x] Cấu hình `eslint-plugin-boundaries`
- [x] Cấu hình path aliases
- [x] Setup `shared/ui/` với shadcn/ui components

### Bước 3: Shared Layer (Admin)
- [x] `shared/lib/api/client.ts` — Axios instance
- [x] `shared/lib/utils.ts` — Utility functions
- [x] `shared/ui/` — Button, Input, Card

### Bước 4: Entity Layer (Admin)
- [x] `entities/admin/model/adminStore.ts` — Zustand store
- [x] `entities/admin/api/admin.queries.ts` — API client
- [x] `entities/admin/index.ts` — Public API

### Bước 5: Feature Layer (Admin)
- [x] `features/auth/login-admin/ui/AdminLoginForm.tsx`
- [x] `features/auth/login-admin/model/loginAdmin.schema.ts`
- [x] `features/auth/login-admin/index.ts`

### Bước 6: Views Layer (Admin)
- [x] `views/login/ui/AdminLoginPage.tsx` — Composition
- [x] `views/login/index.ts`

### Bước 7: App Layer (Admin)
- [x] `app/login/page.tsx` → import từ `@/views/login`

### Bước 8: Tests (Admin)
- [x] `AdminLoginForm.test.tsx` — Hybrid strategy
- [x] `adminStore.test.ts`
- [x] Run tests → 12/12 pass

### Bước 9: Review & Approve
- [x] TypeScript check: `tsc --noEmit` pass
- [x] ESLint check: `npm run lint` pass
- [x] Manual test: Admin login flow OK
- [x] Document lessons learned
- [x] User approved → proceed to Phase 2

---

## Phase 2: PWA Recreation ✅

**Mục tiêu**: Apply lessons từ Admin, recreate 8 screens
**Kết quả**: Hoàn thành — 8/8 screens verified e2e

### Bước 1: Setup & Archive
- [x] Tạo FSD folder structure cho PWA
- [x] Cấu hình `eslint-plugin-boundaries`
- [x] Cấu hình path aliases
- [x] Setup `shared/ui/` với shadcn/ui components

### Bước 2: Shared Layer (PWA)
- [x] `shared/lib/api/client.ts` — Axios instance + interceptors
- [x] `shared/lib/utils.ts` — `cn()`, `getErrorMessage()`
- [x] `shared/lib/navigation.ts` — next-intl `useRouter` wrapper
- [x] `shared/config/routes.ts` — Route constants
- [x] `shared/types/index.ts` — Global types
- [x] `shared/ui/` — Button, Input, Card, Skeleton, Separator, Badge, OtpInput

### Bước 3: Entity Layer (PWA)
- [x] `entities/user/model/authStore.ts` — Auth state
- [x] `entities/user/api/auth.ts` — Auth API
- [x] `entities/user/index.ts`
- [x] `entities/profile/model/profileStore.ts` — Profile state
- [x] `entities/profile/api/profiles.ts` — Profile API
- [x] `entities/profile/index.ts`

### Bước 4: Feature Layer — Auth (PWA)
- [x] `features/auth/login-email/ui/LoginEmailForm.tsx` — Email + password + LINE button
- [x] `features/auth/register/ui/RegisterForm.tsx`
- [x] `features/auth/verify-otp/ui/VerifyOtpForm.tsx` — Dùng chung cho email + phone
- [x] `features/auth/reset-password/ui/ResetPasswordForm.tsx`

### Bước 5: Feature Layer — Profile (PWA)
- [x] `features/profile/edit/ui/EditProfileForm.tsx`
- [x] `features/profile/photo-upload/ui/PhotoUploadGrid.tsx`

### Bước 6: Views Layer (PWA)
- [x] `views/login/ui/LoginView.tsx`
- [x] `views/register/ui/RegisterView.tsx`
- [x] `views/verify-email/ui/VerifyEmailView.tsx`
- [x] `views/verify-phone/ui/VerifyPhoneView.tsx`
- [x] `views/reset-password/ui/ResetPasswordView.tsx`
- [x] `views/profile/ui/ProfileView.tsx`
- [x] `views/profile-edit/ui/ProfileEditView.tsx`
- [x] `views/profile-photos/ui/ProfilePhotosView.tsx`

### Bước 7: App Layer (PWA)
- [x] `app/[locale]/login/page.tsx`
- [x] `app/[locale]/register/page.tsx`
- [x] `app/[locale]/verify-email/page.tsx`
- [x] `app/[locale]/verify-phone/page.tsx`
- [x] `app/[locale]/reset-password/page.tsx`
- [x] `app/[locale]/profile/page.tsx`
- [x] `app/[locale]/profile/edit/page.tsx`
- [x] `app/[locale]/profile/photos/page.tsx`

### Bước 8: i18n
- [x] `i18n/locales/ja.json` — 9 namespaces (auth, common, login, register, otp, verifyEmail, verifyPhone, resetPassword, profile)
- [x] `i18n/locales/en.json` — đồng bộ keys với ja.json
- [x] Verify không có hardcoded strings

### Bước 9: Tests (PWA)
- [x] `__tests__/store/authStore.test.ts` — 4 pass
- [x] `__tests__/components/AuthForm.test.tsx` — 3 pass
- [x] `__tests__/components/OtpInput.test.tsx` — 1 pass
- [x] Tổng: 8/8 pass

### Bước 10: Final Review
- [x] `npx next build` pass — Exit Code 0
- [x] `npm run lint` pass
- [x] Manual test: 8/8 screens verified e2e
- [x] FSD imports không vi phạm boundaries
- [x] `_archive/` đã xóa

---

## FSD Dependency Rules

```
app/          → chỉ import từ views/
views/        → import từ widgets/, features/, entities/, shared/
widgets/      → import từ features/, entities/, shared/
features/     → import từ entities/, shared/
entities/     → import từ shared/
shared/       → KHÔNG import từ layer nào khác
```

> ⚠️ **QUAN TRỌNG**: Dùng `src/views/` thay vì `src/pages/` — Next.js tự động scan `pages/` và conflict với App Router.

**Enforcement**: `eslint-plugin-boundaries` trong `.eslintrc`

---

## Reference Documents

- `requirements.md` — Change request & requirements
- `design.md` — FSD structure design chi tiết
- `code-generation-plan.md` — Code generation checklist + Lessons Learned
- `aidlc-docs/construction/cross-cutting/frontend-uiux-design.md` — UI/UX standards
- `aidlc-docs/construction/cross-cutting/frontend-component-architecture.md` — Architecture standards
