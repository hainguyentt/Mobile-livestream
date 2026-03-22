# Daily Summary — 2026-03-22 (Day 02)

**Date**: 2026-03-22  
**Day**: Day 02  
**Work Hours**: 09:00 - 18:00 (9 giờ)  
**Phase**: Construction Phase Unit 1 — Code Generation + Frontend Debugging  
**Status**: ✅ On Track — Unit 1 Code Generation Complete

---

## 🎯 Today's Goals
- [x] Execute Code Generation (Unit 1 — ~160 files)
- [x] Run và fix integration tests
- [x] Start và verify app trong development environment
- [x] Debug và fix frontend issues (i18n, routing, profile, CORS)
- [ ] Build and Test (full suite) — deferred to Day 03
- [ ] Documentation updates — deferred to Day 03

---

## ✅ Completed Tasks

### 1. Execute Code Generation — Unit 1 (4 giờ)
**Description**: Thực thi 160-step code generation plan cho Unit 1 (Shared, Auth, Profiles, API, MockServices).  
**Outcome**:
- Generated ~160 files (~15K-20K LOC)
- Backend: Shared, Auth, Profiles, API modules
- Frontend: PWA pages (Login, Register, OTP, Profile, Photos), Admin Login
- Infrastructure: Docker Compose, LocalStack setup
- Tests: Unit tests (47 tests), Integration tests (17 tests)
**Artifacts**:
- `app/backend/` — toàn bộ backend modules
- `app/frontend/pwa/` — PWA Next.js app
- `app/frontend/admin/` — Admin Next.js app
- `app/mock/` — MockServices (Stripe + LINE Pay)
- `app/infra/` — Docker Compose, LocalStack scripts

### 2. Fix Migration — ChangeInterestsToTextArray
**Description**: Migration dùng `ALTER COLUMN ... USING ARRAY(SELECT ...)` fail vì PostgreSQL không cho phép subquery trong USING clause.  
**Root Cause**: EF Core generate migration syntax không tương thích với PostgreSQL constraint.  
**Fix**: Add column mới → populate → drop cũ → rename.  
**Outcome**: 17/17 integration tests pass, 47/47 unit tests pass.  
**Artifacts**: `app/backend/LivestreamApp.API/Migrations/20260322050211_ChangeInterestsToTextArray.cs`

### 3. Run App trong Development Environment
**Description**: Start infrastructure (postgres + redis via docker-compose), backend (port 5174), PWA (port 3000), Admin (port 3001).  
**Outcome**: Tất cả services khởi động thành công, app accessible trên browser.

### 4. Fix Frontend Issues (9 issues)
**Description**: Kiểm tra và fix các lỗi frontend sau khi chạy app lần đầu.  
**Issues fixed**:
- Base URL config sai
- Reset-password page lỗi
- Profile pages không load
- AuthForm navigation không giữ locale
- AdminLoginPage redirect sai
- Verify-email/phone URL params thiếu
- i18n routing 404 sau login
- Profile 404 không phân biệt với lỗi thực
- 500 khi change profile (Redis cache issue)
**Artifacts**: Nhiều files trong `app/frontend/pwa/src/`

### 5. Tạo Seed Accounts
**Description**: Tạo tài khoản demo cho testing.  
**Accounts**:
- `viewer@demo.com` / `Demo123!` (Role=0 — Viewer)
- `admin@demo.com` / `Demo123!` (Role=2 — Admin)
**Outcome**: Login flow hoạt động end-to-end.

### 6. Fix CORS Error
**Description**: Frontend không thể gọi API do thiếu CORS config với `AllowCredentials()` cho httpOnly cookie flow.  
**Fix**: Thêm CORS policy với `AllowCredentials()`, `AllowAnyHeader()`, `AllowAnyMethod()`.  
**Artifacts**: `ServiceCollectionExtensions.cs`, `Program.cs`, `appsettings.Development.json`

### 7. Fix i18n Routing — 404 sau khi login
**Description**: Next.js App Router với `[locale]` dynamic segment cần next-intl middleware để handle routing.  
**Fix**: Tạo next-intl middleware, layout, i18n request config, navigation helper.  
**Artifacts**: `middleware.ts`, `[locale]/layout.tsx`, `i18n/request.ts`, `lib/navigation.ts`

### 8. Fix Profile 404 — profileStore không phân biệt 404
**Description**: `profileStore.fetchProfile()` throw error khi nhận 404 → store set `error` state → UI hiển thị error thay vì "create profile" flow.  
**Fix**: Catch `AxiosError` 404 → set `{ profile: null }` không set error. Chỉ set `error` cho 5xx/network errors.  
**Artifacts**: `store/profileStore.ts`

### 9. Fix 500 khi Change Profile — Redis Cache Deserialization
**Description**: `ProfileService.GetProfileAsync` cache `UserProfile` entity (private constructors) vào Redis → `System.Text.Json` không deserialize được → `NotSupportedException`.  
**Root Cause**: Domain entity dùng DDD private constructor pattern không compatible với JSON serializer.  
**Fix**: Bỏ cache entity domain, query thẳng từ DB. Remove `ICacheService` dependency khỏi `ProfileService`.  
**Artifacts**: `ProfileService.cs`, `UserProfileCacheDto.cs`, `ProfileServiceTests.cs`

### 10. Fix i18n — `/en/profile` hiển thị tiếng Nhật
**Description**: Pages hardcode tiếng Nhật thay vì dùng `useTranslations` hook.  
**Fix**: Cập nhật `profile/page.tsx`, `profile/edit/page.tsx`, `profile/photos/page.tsx`, `PhotoUploader.tsx`. Thêm translation keys vào `ja.json` và `en.json`.  
**Artifacts**: `app/frontend/pwa/src/i18n/locales/en.json`, `ja.json`, các page files

### 11. Update Frontend Coding Standard
**Description**: Cập nhật `.kiro/steering/coding-standards-frontend.md` — thêm section i18n đầy đủ và store HTTP error handling rules.  
**Outcome**: Coding standard phản ánh lessons learned từ Day 02.  
**Artifacts**: `.kiro/steering/coding-standards-frontend.md`

### 12. Self-Review Frontend theo Coding Standards
**Description**: Kiểm tra toàn bộ frontend components và pages theo coding standards.  
**Fixed**: `AuthForm.tsx`, `OtpInput.tsx`, `reset-password/page.tsx`, `LanguageSwitcher.tsx`, test files, translation keys.  
**Outcome**: Frontend compliant với coding standards.

---

## 🔑 Key Decisions

### Decision 1: Bỏ Cache Domain Entity trong ProfileService
**Context**: `ProfileService` cache `UserProfile` entity vào Redis gây `NotSupportedException` khi deserialize.  
**Options Considered**:
- A — Tạo `UserProfileCacheDto` riêng để cache
- B — Bỏ cache hoàn toàn, query thẳng DB
- C — Dùng custom JSON converter cho private constructor  
**Chosen**: Option B (bỏ cache)  
**Rationale**: Profile data không hot-path, DB query đủ nhanh. Tránh complexity của DTO mapping chỉ để cache.  
**Impact**: `ProfileService` đơn giản hơn, không có `ICacheService` dependency.

### Decision 2: Dùng PowerShell `-LiteralPath` cho files trong `[locale]/`
**Context**: Kiro `fsWrite` interpret `[locale]` như glob pattern → file bị empty.  
**Options Considered**:
- A — Dùng `fsWrite` (broken)
- B — Dùng PowerShell `Set-Content -LiteralPath`  
**Chosen**: Option B  
**Rationale**: `-LiteralPath` không interpret special characters, đảm bảo write đúng file.  
**Impact**: Tất cả files trong `[locale]/` phải dùng PowerShell để write.

### Decision 3: next-intl Navigation Wrapper bắt buộc
**Context**: Dùng `useRouter` từ `next/navigation` → navigate mất locale prefix → 404.  
**Chosen**: Tạo `lib/navigation.ts` re-export từ `next-intl/navigation`.  
**Rationale**: Centralize navigation logic, tất cả components dùng cùng một source.  
**Impact**: Rule mới trong coding standard: không dùng `next/navigation` trực tiếp.

---

## 💡 Lessons Learned

### LL-01: Kiro `fsWrite` không thể write vào path có ký tự `[` `]`
**Context**: Next.js App Router dùng folder `[locale]` cho dynamic routing. Kiro's `fsWrite` tool interpret `[locale]` như glob pattern → file bị tạo ra nhưng empty hoặc không tồn tại.  
**Symptom**: File `app/frontend/pwa/src/app/[locale]/layout.tsx` liên tục bị empty sau mỗi lần write bằng `fsWrite`. Dẫn đến lỗi "The default export is not a React Component" trên tất cả pages trong `[locale]/`.  
**Root Cause**: Kiro tools dùng glob path matching — `[locale]` bị interpret là character class `[l, o, c, a, e]` thay vì literal string.  
**Fix**: Dùng PowerShell với `-LiteralPath` flag:
```powershell
Set-Content -LiteralPath "app/frontend/pwa/src/app/[locale]/layout.tsx" -Value $content
```
**Rule**: Với bất kỳ file nào trong thư mục có tên chứa `[` hoặc `]`, LUÔN dùng PowerShell `-LiteralPath` để write.

### LL-02: Cache domain entity với private constructor gây lỗi deserialization
**Context**: `ProfileService` cache `UserProfile` entity vào Redis. Entity dùng private constructor (DDD pattern) → `System.Text.Json` không thể deserialize → `NotSupportedException` khi đọc cache.  
**Fix**: Không cache domain entities. Query thẳng DB nếu không cần cache, hoặc tạo DTO riêng nếu cần.  
**Rule**: Chỉ cache plain DTOs/records với public constructor. Không bao giờ cache domain entities.

### LL-03: profileStore phải phân biệt HTTP 404 với lỗi thực sự
**Context**: `fetchProfile()` throw error khi nhận 404 → store set `error` state → UI hiển thị error thay vì "create profile" flow.  
**Fix**: Catch `AxiosError` với `status === 404` → set `{ profile: null }` không set `error`. Chỉ set `error` cho các lỗi thực sự (5xx, network error).  
**Rule**: HTTP 404 = "resource not found" (expected state), không phải lỗi. Xử lý riêng trong store.

### LL-04: next-intl navigation wrapper bắt buộc để giữ locale prefix
**Context**: Dùng `useRouter` từ `next/navigation` → navigate mất locale prefix → 404.  
**Fix**: Tạo `lib/navigation.ts` re-export từ `next-intl/navigation`. Tất cả components dùng `@/lib/navigation`.  
**Rule**: Không import trực tiếp từ `next/navigation` trong project có i18n routing.

### LL-05: EF Core migration syntax phải test với target DB
**Context**: EF Core generate migration dùng `ALTER COLUMN ... USING ARRAY(SELECT ...)` — valid SQL nhưng PostgreSQL không cho phép subquery trong USING clause.  
**Fix**: Viết migration thủ công: add column → populate → drop old → rename.  
**Rule**: Luôn test migration trên PostgreSQL thực tế, không chỉ trust EF Core generated SQL.

### LL-06: Thay đổi kiến trúc Frontend phải được quyết định từ Inception và Application Design
**Context**: Trong quá trình Code Generation Unit 1, phát hiện kiến trúc frontend ban đầu (flat `components/`, `store/`, `lib/api/`) không phù hợp với quy mô dự án (5 units, 84+ API endpoints, 5-10 developers). Quyết định chuyển sang **Feature-Sliced Design (FSD)** — nhưng quyết định này đến muộn (sau khi đã generate code), dẫn đến phải recreate toàn bộ frontend.

**Vấn đề cốt lõi**: Kiến trúc frontend không được thảo luận đủ sâu trong Inception (Application Design stage) và Construction (Functional Design stage). Chỉ đến khi code được generate và chạy thực tế mới nhận ra vấn đề scalability.

**DO — Yêu cầu quyết định kiến trúc frontend từ sớm**:
- Trong **Inception — Application Design**: Phải xác định rõ frontend architecture pattern (FSD, Atomic Design, Feature-based, v.v.) cùng với backend modules
- Trong **Construction — Functional Design**: Phải thiết kế FSD layer structure (views, widgets, features, entities, shared) trước khi code generation
- Không để "kiến trúc frontend" là afterthought sau khi code đã được generate

**DO — Tạo file trade-off analysis để đẩy nhanh quyết định**:
- Khi có architectural decision phức tạp, tạo file phân tích trade-off riêng (như `tradeoff-analysis.md`) với format chuẩn: Options → Scoring → Recommendation
- File này giúp người phụ trách review nhanh và quyết định mà không cần đọc toàn bộ context
- Mỗi option được đánh giá theo tiêu chí rõ ràng (FSD Compliance, Maintainability, Concurrent Dev, Scalability, Simplicity)
- Kết quả: 14 architectural questions được resolve trong 1 session thay vì nhiều vòng thảo luận

**Ưu điểm của kiến trúc FSD đã chọn**:
- **Parallel development**: Mỗi feature slice độc lập — 2-3 developers làm song song không conflict
- **Clear ownership**: Bug trong auth flow → chỉ cần look vào `features/auth/` và `entities/user/`
- **Enforced boundaries**: `eslint-plugin-boundaries` enforce dependency rules tự động — không thể import sai layer
- **Scalability**: 84 API endpoints → 6-7 entity files thay vì 5-6 file khổng lồ
- **Future-proof**: Unit 2-5 thêm feature slices mới, không refactor existing code
- **Onboarding**: Developer mới chỉ cần hiểu FSD layer rules + đọc public APIs qua `index.ts`

**Artifacts tạo ra**:
- `aidlc-docs/construction/frontend-recreation/tradeoff-analysis.md` — 14 questions, scoring matrix, recommendations
- `aidlc-docs/construction/frontend-recreation/design.md` — FSD structure chi tiết cho PWA và Admin
- `aidlc-docs/construction/cross-cutting/frontend-component-architecture.md` — Cross-cutting standard cho tất cả units
- `.kiro/steering/coding-standards-frontend-uiux.md` — Coding standards cập nhật theo FSD

---

## ⚠️ Blockers/Issues

### Issue 1: `[locale]/layout.tsx` bị empty do Kiro `fsWrite` glob issue
**Status**: 🟢 Resolved  
**Resolution**: Dùng PowerShell `-LiteralPath` để write file.

### Issue 2: `OtpInput.test.tsx` bị empty
**Status**: 🟢 Resolved  
**Resolution**: Restored file content.

### Issue 3: Redis cache deserialization error (ProfileService)
**Status**: 🟢 Resolved  
**Resolution**: Removed entity caching từ ProfileService.

### Issue 4: EF Core migration fail trên PostgreSQL
**Status**: 🟢 Resolved  
**Resolution**: Viết migration thủ công với add/populate/drop/rename pattern.

### Issue 5: Kiến trúc Frontend không phù hợp với quy mô dự án
**Status**: 🟢 Resolved (FSD architecture adopted)  
**Resolution**: Phân tích trade-off 14 questions → chọn Feature-Sliced Design → tạo design artifacts → recreate frontend theo FSD.

---

## 📈 Metrics

### Time Metrics
- **Total work time**: 9 giờ
- **Code Generation**: 4 giờ
- **Debugging & Fixes**: 4 giờ
- **Standards & Review**: 1 giờ

### Productivity Metrics
- **Files generated**: ~160 files
- **Bugs fixed**: 9 frontend issues + 4 backend/infra issues
- **Tests passing**: 47 unit + 17 integration = 64 total
- **Coding standards updated**: 1 file

### Quality Metrics
- **Blocking issues**: 0 (tất cả resolved)
- **Unit tests**: 47/47 pass ✅
- **Integration tests**: 17/17 pass ✅
- **Lessons learned documented**: 6

### Code Metrics
- **LOC generated**: ~15,000-20,000 lines
- **Backend modules**: 5 (Shared, Auth, Profiles, API, MockServices)
- **Frontend pages**: 10 PWA + 2 Admin
- **Test files**: ~20 files

---

## 🚀 Tomorrow's Plan (Day 03)

### High Priority
- [ ] **Build and Test — Full Suite** (2-3 giờ)
  - Run all unit tests với coverage report
  - Run integration tests với Docker
  - Verify Docker Compose startup end-to-end
  - Check code coverage (target ≥80%)
- [ ] **Documentation Updates** (1 giờ)
  - Update README.md với setup instructions
  - Finalize deployment guide
  - Document API endpoints

### Medium Priority
- [ ] **Self-Review Backend** (1 giờ)
  - Kiểm tra coding standards compliance
  - Review error handling patterns
  - Verify logging configuration
- [ ] **Staging Deployment** (2 giờ, nếu còn thời gian)
  - Build Docker images
  - Deploy to staging environment
  - Run smoke tests

### Low Priority
- [ ] **Start Unit 2 Planning** (nếu Unit 1 complete)
  - Review Unit 2 scope (Livestream Engine)
  - Identify dependencies từ Unit 1

### Stretch Goals
- [ ] Deploy to staging environment
- [ ] Start Unit 2 Functional Design

---

## 📝 Notes

### What Went Well
- ✅ Code generation hoàn thành đúng scope (~160 files)
- ✅ Tất cả tests pass sau khi fix migration
- ✅ App chạy được end-to-end trong dev environment
- ✅ 9 frontend issues identified và fixed trong cùng ngày
- ✅ Lessons learned documented ngay khi phát hiện

### What Could Be Improved
- ⚠️ Kiro `fsWrite` glob issue mất ~1 giờ debug → cần rule rõ ràng hơn từ đầu
- ⚠️ Redis cache strategy nên được review kỹ hơn trong Functional Design stage
- ⚠️ i18n setup nên được include trong code generation plan (không phải afterthought)
- 💡 Nên có checklist "first run verification" sau code generation

### Action Items for Future
- [ ] Thêm rule về PowerShell `-LiteralPath` vào coding standards
- [ ] Thêm i18n setup vào code generation plan template
- [ ] Tạo "first run checklist" cho sau code generation
- [ ] Review cache strategy trong Functional Design (không chỉ NFR Design)

---

**Report Generated**: 2026-03-22 23:00:00  
**Next Update**: 2026-03-23 (Day 03)  
**Status**: ✅ Day 02 Complete — Unit 1 Code Generation Done, Ready for Build & Test
