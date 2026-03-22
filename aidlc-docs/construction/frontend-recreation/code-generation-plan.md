# Frontend Unit 1 Recreation — Code Generation Plan

**Ngày tạo**: 2026-03-22
**Chiến lược**: Incremental — từng feature, test ngay
**Thứ tự**: Phase 1 (Admin) → Phase 2 (PWA)

---

## Phase 1: Admin Recreation ✅

### P1-S1: Setup & Archive
- [x] Archive `app/frontend/admin/src/` (refactor in-place, không tạo _archive)
- [x] Tạo FSD folder structure

### P1-S2: Shared Layer
- [x] `shared/lib/api/client.ts` — Axios instance với interceptors
- [x] `shared/lib/utils.ts` — `cn()`, `getErrorMessage()`
- [x] `shared/ui/button.tsx` — shadcn/ui Button
- [x] `shared/ui/input.tsx` — shadcn/ui Input
- [x] `shared/ui/index.ts` — Re-export tất cả

### P1-S3: Entity Layer — Admin
- [x] `entities/admin/model/types.ts` — `AdminUser`, `AdminTokens` interfaces
- [x] `entities/admin/api/admin.queries.ts` — `adminLogin()`, `adminLogout()`
- [x] `entities/admin/model/adminStore.ts` — Zustand store
- [x] `entities/admin/index.ts` — Public API

### P1-S4: Feature Layer — Login Admin
- [x] `features/auth/login-admin/model/loginAdmin.schema.ts` — Zod schema
- [x] `features/auth/login-admin/ui/AdminLoginForm.tsx` — Form component
- [x] `features/auth/login-admin/index.ts` — Public API

### P1-S5: Views Layer
- [x] `views/login/ui/AdminLoginPage.tsx` — Composition
- [x] `views/login/index.ts`

### P1-S6: App Layer
- [x] `app/login/page.tsx` — Import từ `@/views/login`

### P1-S7: ESLint & TypeScript Config
- [x] Cấu hình `eslint-plugin-boundaries` trong `.eslintrc.js`
- [x] Cấu hình path aliases trong `tsconfig.json`
- [x] Verify: `npm run lint` pass
- [x] Verify: `tsc --noEmit` pass

### P1-S8: Tests
- [x] `__tests__/features/auth/AdminLoginForm.test.tsx`
- [x] `__tests__/entities/admin/adminStore.test.ts`
- [x] Chạy: `npm test -- --run` → 12/12 pass

### P1-S9: Manual Verification
- [x] Test admin login flow end-to-end (backend port 5174, admin port 3001)
- [x] Verify UI/UX: slate palette, no pink
- [x] Verify FSD imports không vi phạm boundaries
- [x] Document lessons learned

---

## Phase 2: PWA Recreation ✅

### P2-S1: Setup
- [x] Tạo FSD folder structure rỗng

### P2-S2: Shared Layer
- [x] `shared/lib/api/client.ts` — Axios instance + interceptors (skip auth endpoints khi retry 401)
- [x] `shared/lib/utils.ts` — `cn()`, `getErrorMessage()`
- [x] `shared/lib/navigation.ts` — next-intl `useRouter` wrapper
- [x] `shared/config/routes.ts` — Route constants
- [x] `shared/types/index.ts` — Global types (`OtpPurpose`, v.v.)
- [x] `shared/ui/button.tsx` — shadcn/ui Button
- [x] `shared/ui/input.tsx` — shadcn/ui Input
- [x] `shared/ui/card.tsx` — shadcn/ui Card
- [x] `shared/ui/skeleton.tsx` — shadcn/ui Skeleton
- [x] `shared/ui/separator.tsx` — shadcn/ui Separator
- [x] `shared/ui/badge.tsx` — shadcn/ui Badge
- [x] `shared/ui/otp-input.tsx` — Custom OTP primitive
- [x] `shared/ui/index.ts` — Re-export tất cả

### P2-S3: Entity Layer — User
- [x] `entities/user/model/types.ts` — `User`, `AuthTokens` interfaces
- [x] `entities/user/api/auth.ts` — All auth API functions
- [x] `entities/user/model/authStore.ts` — Zustand store (error codes thay vì raw message)
- [x] `entities/user/index.ts`

### P2-S4: Entity Layer — Profile
- [x] `entities/profile/model/types.ts` — `UserProfile`, `UserPhoto`, `PresignResponse`
- [x] `entities/profile/api/profiles.ts` — All profile API functions
- [x] `entities/profile/model/profileStore.ts` — Zustand store (404 → null, không phải error)
- [x] `entities/profile/index.ts`

### P2-S5: Widget Layer
- [ ] Planned cho Unit 2 — không thuộc scope Phase 2

### P2-S6: Feature — Login Email
- [x] `features/auth/login-email/ui/LoginEmailForm.tsx` — Email + password + LINE button
- [x] `features/auth/login-email/index.ts`
- [x] `views/login/ui/LoginView.tsx` + `index.ts`
- [x] `app/[locale]/login/page.tsx`
- [x] Test: `AuthForm.test.tsx` → 3 pass

### P2-S7: Feature — Register
- [x] `features/auth/register/ui/RegisterForm.tsx`
- [x] `features/auth/register/index.ts`
- [x] `views/register/ui/RegisterView.tsx` + `index.ts`
- [x] `app/[locale]/register/page.tsx`

### P2-S8: Feature — Verify Email
- [x] `features/auth/verify-otp/ui/VerifyOtpForm.tsx` — Dùng chung cho email + phone
- [x] `features/auth/verify-otp/index.ts`
- [x] `views/verify-email/ui/VerifyEmailView.tsx` + `index.ts`
- [x] `app/[locale]/verify-email/page.tsx`

### P2-S9: Feature — Verify Phone
- [x] `views/verify-phone/ui/VerifyPhoneView.tsx` + `index.ts`
- [x] `app/[locale]/verify-phone/page.tsx`

### P2-S10: Feature — Reset Password
- [x] `features/auth/reset-password/ui/ResetPasswordForm.tsx`
- [x] `features/auth/reset-password/index.ts`
- [x] `views/reset-password/ui/ResetPasswordView.tsx` + `index.ts`
- [x] `app/[locale]/reset-password/page.tsx`

### P2-S11: Feature — Profile View
- [x] `views/profile/ui/ProfileView.tsx` + `index.ts`
- [x] `app/[locale]/profile/page.tsx`

### P2-S12: Feature — Profile Edit
- [x] `features/profile/edit/ui/EditProfileForm.tsx`
- [x] `features/profile/edit/index.ts`
- [x] `views/profile-edit/ui/ProfileEditView.tsx` + `index.ts`
- [x] `app/[locale]/profile/edit/page.tsx`

### P2-S13: Feature — Photo Upload
- [x] `features/profile/photo-upload/ui/PhotoUploadGrid.tsx`
- [x] `features/profile/photo-upload/index.ts`
- [x] `views/profile-photos/ui/ProfilePhotosView.tsx` + `index.ts`
- [x] `app/[locale]/profile/photos/page.tsx`

### P2-S14: i18n
- [x] `i18n/locales/ja.json` — 9 namespaces: auth, common, login, register, otp, verifyEmail, verifyPhone, resetPassword, profile
- [x] `i18n/locales/en.json` — đồng bộ keys với ja.json
- [x] Verify tất cả keys đồng bộ
- [x] Verify không có hardcoded strings trong components

### P2-S15: Tests
- [x] `__tests__/store/authStore.test.ts` — 4 pass
- [x] `__tests__/components/AuthForm.test.tsx` — 3 pass
- [x] `__tests__/components/OtpInput.test.tsx` — 1 pass (placeholder)
- [x] Tổng: 8/8 pass

### P2-S16: ESLint & Build
- [x] Cấu hình `eslint-plugin-boundaries` trong `.eslintrc.js`
- [x] Cấu hình path aliases trong `tsconfig.json`
- [x] `npm run lint` pass
- [x] `npx next build` pass — Exit Code 0

### P2-S17: Final Verification
- [x] Manual test 8/8 screens end-to-end với backend
- [x] Fix: error message hiển thị HTTP code → translate qua `auth` namespace
- [x] Fix: text thừa trên Register screen (`loginLink` duplicate)
- [x] Fix: `requestDescription` key không tồn tại trên ResetPassword screen
- [x] Fix: JSON invalid trong locale files sau khi thêm `auth` namespace
- [x] Fix: legacy files (`src/store/`, `src/components/`) xóa sau FSD refactor

---

## Checklist Hoàn Thành

### Phase 1 ✅
- [x] Admin login hoạt động end-to-end
- [x] Tất cả tests pass (12/12)
- [x] `tsc --noEmit` pass
- [x] `npm run lint` pass
- [x] FSD structure đúng
- [x] User approved

### Phase 2 ✅
- [x] Tất cả 8 PWA screens verified e2e
- [x] Tất cả tests pass (8/8)
- [x] `npx next build` pass (0 errors)
- [x] `npm run lint` pass
- [x] FSD structure đúng — `src/views/` thay vì `src/pages/`
- [x] i18n không có hardcoded strings — `auth` namespace cho error codes

---

## Ghi Chú Quan Trọng

1. **Mỗi feature phải test ngay** trước khi sang feature tiếp theo
2. **Không skip tests** — hybrid strategy: reuse scenarios, recreate code
3. **Không thay đổi API contracts** — backend không thay đổi
4. **Không thay đổi UX** — preserve existing user experience
5. **FSD `pages/` layer phải đổi tên thành `views/`** trong Next.js projects — Next.js scan tất cả `pages/` directories và conflict với App Router routes

---

## Lessons Learned — Phase 1

1. **eslint-plugin-boundaries v6** đổi tên rule `element-types` → `dependencies` — dùng v5 syntax (string-based) vẫn hoạt động với v6, chỉ có deprecation warning
2. **zod** không có sẵn trong admin — cần `npm install zod` riêng
3. **jest** cần cài đầy đủ: `jest jest-environment-jsdom @testing-library/react @testing-library/user-event @testing-library/jest-dom ts-jest @types/jest`
4. **setupFilesAfterEnv** (không phải `setupFilesAfterFramework`) là key đúng trong jest.config.js
5. **Zustand store mock trong tests**: cần mock cả `getState()` static method khi component gọi `useAdminStore.getState()` sau submit
6. **⚠️ CRITICAL — Tailwind content glob**: Sau khi refactor sang FSD, phải đổi content thành `'./src/**/*.{js,ts,jsx,tsx,mdx}'` — nếu không layout bị vỡ hoàn toàn
7. **FSD `pages/` → `views/`**: Next.js conflict với App Router — luôn dùng `src/views/`

---

## Lessons Learned — Phase 2

1. **`clsx` + `tailwind-merge` không có sẵn** — cần `npm install clsx tailwind-merge`; xóa `.next` cache sau install
2. **i18n namespace `auth` cho error codes** — store trả về error code (`AUTH_INVALID_CREDENTIALS`), component translate qua `useTranslations('auth')` — giữ store locale-agnostic
3. **Axios interceptor conflict với login 401** — interceptor retry 401 phải skip auth endpoints (`/auth/login`, `/auth/register`)
4. **JSON locale files dễ bị corrupt khi dùng strReplace** — khi thêm namespace mới, phải viết lại toàn bộ file; luôn validate JSON sau khi edit
5. **Legacy files phải xóa sau FSD refactor** — `src/store/`, `src/components/AuthForm.tsx`, `src/components/OtpInput.tsx`, `src/components/PhotoUploader.tsx` còn sót gây ESLint `no-restricted-imports` error khi build
6. **i18n key duplicate trong link text** — pattern đúng: `{t('hasAccount')} <a>{t('loginLinkLabel')}</a>` — key chứa full sentence không dùng được làm link text riêng
7. **Missing i18n key gây crash runtime** — luôn verify key tồn tại trong locale files trước khi dùng trong component
8. **`VerifyOtpForm` dùng chung cho email + phone** — dùng `purpose: OtpPurpose` prop để phân biệt — giảm code duplication
