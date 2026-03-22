# AI-DLC Case Study Report — Day 02
# Dự Án: App Livestream Hẹn Hò - Thị Trường Nhật Bản

**Ngày tạo**: 2026-03-22  
**Phiên bản**: 1.0  
**Mục đích**: Tài liệu chi tiết về quá trình Code Generation Unit 1 và debugging thực tế

---

## Executive Summary

Day 02 đánh dấu bước chuyển quan trọng: từ **design artifacts** sang **running application**. Toàn bộ Unit 1 Code Generation (~160 files) được thực thi, app chạy thành công trong development environment, và 13 issues được phát hiện + resolved trong cùng ngày.

**Thành tựu chính**:
- ✅ ~160 files generated (~15K-20K LOC)
- ✅ 64/64 tests pass (47 unit + 17 integration)
- ✅ App running end-to-end trong dev environment
- ✅ 13 issues resolved (9 frontend + 4 backend/infra)
- ✅ 5 lessons learned documented với actionable rules

---

## Table of Contents

1. [Code Generation Execution](#1-code-generation-execution)
2. [Technical Issues & Resolutions](#2-technical-issues--resolutions)
3. [Frontend Debugging Deep Dive](#3-frontend-debugging-deep-dive)
4. [Backend Issues & Fixes](#4-backend-issues--fixes)
5. [Tool Limitations Discovered](#5-tool-limitations-discovered)
6. [Coding Standards Evolution](#6-coding-standards-evolution)
7. [DO's and DON'Ts](#7-dos-and-donts)
8. [Lessons Learned](#8-lessons-learned)
9. [Metrics & Outcomes](#9-metrics--outcomes)
10. [Appendix](#10-appendix)

---

## 1. Code Generation Execution

### 1.1 Scope

Code Generation Unit 1 bao gồm 7 user stories (US-01-01 đến US-02-02) và 5 modules:

| Module | Scope | Files |
|---|---|---|
| LivestreamApp.Shared | Domain primitives, interfaces, base classes | ~15 files |
| LivestreamApp.Auth | Authentication, JWT, OTP, LINE Login | ~25 files |
| LivestreamApp.Profiles | User profiles, photos, host verification | ~20 files |
| LivestreamApp.API | ASP.NET Core host, controllers, middleware | ~30 files |
| LivestreamApp.MockServices | Stripe + LINE Pay mock servers | ~15 files |
| Frontend PWA | Next.js pages, components, stores, tests | ~35 files |
| Frontend Admin | Next.js admin pages | ~10 files |
| Infrastructure | Docker Compose, LocalStack, scripts | ~10 files |
| **Total** | | **~160 files** |

### 1.2 Execution Process

Code generation được thực thi theo 160-step plan từ Day 01:

**Phase 1-5: Project Setup**
- Solution structure, NuGet packages, project references
- Docker Compose với PostgreSQL + Redis + LocalStack + MailHog
- Environment configuration (appsettings.json, .env files)

**Phase 6-15: Shared Module**
- Domain primitives (Result, Error, ValueObject base classes)
- Interfaces (IRepository, ICacheService, IStorageService, IEmailService)
- Domain events infrastructure
- Shared DTOs và response models

**Phase 16-30: Auth Module**
- Domain entities (User, RefreshToken, OtpCode, LoginAttempt)
- Business rules implementation
- Services (AuthService, TokenService, OtpService, LineLoginService)
- Repository interfaces và implementations
- EF Core DbContext + migrations

**Phase 31-40: Profiles Module**
- Domain entities (UserProfile, HostProfile, UserPhoto)
- Services (ProfileService, PhotoService, HostVerificationService)
- S3 integration (presigned URLs)
- Repository implementations

**Phase 41-55: API Module**
- Controllers (AuthController, ProfilesController, AdminController)
- Middleware (ExceptionHandling, RateLimiting, Logging)
- Service registration (ServiceCollectionExtensions)
- Health checks, API versioning

**Phase 56-65: MockServices**
- Stripe Mock Controller (payment intents, webhooks)
- LINE Pay Mock Controller (payment requests, confirmations)
- Mock data generators

**Phase 66-80: Frontend PWA**
- Next.js setup với next-intl, Tailwind, Zustand
- Auth pages (Login, Register, OTP Verification, Reset Password)
- Profile pages (View, Edit, Photos)
- Shared components (AuthForm, OtpInput, LanguageSwitcher)
- API client (axios instance, interceptors)
- State stores (authStore, profileStore)

**Phase 81-90: Frontend Admin**
- Admin login page
- Dashboard skeleton
- Admin API client

**Phase 91-100: Tests**
- Unit tests (47 tests): Auth services, Profile services, MockServices
- Integration tests (17 tests): Auth flow, Profile flow, Docker infrastructure

### 1.3 Kết Quả

```
Solution: LivestreamApp.sln
├── app/backend/
│   ├── LivestreamApp.Shared/          ✅ Complete
│   ├── LivestreamApp.Auth/            ✅ Complete
│   ├── LivestreamApp.Profiles/        ✅ Complete
│   └── LivestreamApp.API/             ✅ Complete
├── app/mock/
│   └── LivestreamApp.MockServices/    ✅ Complete
├── app/frontend/
│   ├── pwa/                           ✅ Complete (with fixes)
│   └── admin/                         ✅ Complete (with fixes)
├── app/tests/
│   ├── LivestreamApp.Tests.Unit/      ✅ 47/47 pass
│   └── LivestreamApp.Tests.Integration/ ✅ 17/17 pass
└── app/infra/
    ├── docker-compose.yml             ✅ Complete
    └── localstack-init/               ✅ Complete
```

---

## 2. Technical Issues & Resolutions

### 2.1 Issue Summary

| # | Issue | Severity | Root Cause | Resolution |
|---|---|---|---|---|
| 1 | Migration USING clause fail | High | PostgreSQL subquery restriction | Manual migration rewrite |
| 2 | CORS error | High | Missing AllowCredentials() | Added CORS policy |
| 3 | i18n routing 404 | High | Missing next-intl middleware | Added middleware + config |
| 4 | Profile 404 treated as error | Medium | Store error handling | Catch 404 separately |
| 5 | Redis cache deserialization | High | Domain entity private constructor | Removed entity caching |
| 6 | `[locale]/layout.tsx` empty | High | Kiro fsWrite glob issue | PowerShell -LiteralPath |
| 7 | i18n content hardcoded | Medium | Pages not using useTranslations | Updated to use hook |
| 8 | Base URL config wrong | Medium | Config mismatch | Fixed appsettings |
| 9 | Reset-password page error | Medium | URL params handling | Fixed params |
| 10 | Profile pages not loading | Medium | Store initialization | Fixed store |
| 11 | AuthForm navigation | Medium | next/navigation vs next-intl | Used navigation wrapper |
| 12 | AdminLoginPage redirect | Medium | Redirect URL wrong | Fixed redirect |
| 13 | Verify URL params missing | Medium | Missing query params | Added params |

### 2.2 Resolution Rate

- **Total issues**: 13
- **Resolved same day**: 13 (100%)
- **Blocking issues remaining**: 0

---

## 3. Frontend Debugging Deep Dive

### 3.1 i18n Routing Architecture

**Problem**: Next.js App Router với `[locale]` dynamic segment cần middleware để:
1. Detect user's preferred locale
2. Redirect `/` → `/ja` hoặc `/en`
3. Validate locale trong URL
4. Pass locale context xuống components

**Solution Architecture**:

```
Request: /profile
    ↓
middleware.ts (next-intl)
    ↓ detect locale (cookie/header/default)
    ↓ redirect to /ja/profile
    ↓
app/[locale]/layout.tsx
    ↓ load messages for locale
    ↓ provide NextIntlClientProvider
    ↓
app/[locale]/profile/page.tsx
    ↓ useTranslations('profile')
    ↓ render with correct locale
```

**Files Created**:
```
src/
├── middleware.ts                    # next-intl routing middleware
├── i18n/
│   ├── request.ts                  # Server-side i18n config
│   └── locales/
│       ├── ja.json                 # Japanese translations
│       └── en.json                 # English translations
├── lib/
│   └── navigation.ts               # next-intl navigation wrapper
└── app/[locale]/
    └── layout.tsx                  # Locale-aware root layout
```

**Key Rule**: Không import từ `next/navigation` trực tiếp. Luôn dùng `@/lib/navigation`.

### 3.2 Store HTTP Error Handling Pattern

**Problem**: `profileStore.fetchProfile()` set `error` state khi nhận 404 → UI hiển thị error message thay vì "create profile" flow.

**Before (Wrong)**:
```typescript
async fetchProfile() {
  try {
    const profile = await profileApi.getProfile();
    set({ profile, error: null });
  } catch (err) {
    set({ error: 'Failed to load profile' }); // ❌ 404 treated as error
  }
}
```

**After (Correct)**:
```typescript
async fetchProfile() {
  try {
    const profile = await profileApi.getProfile();
    set({ profile, error: null });
  } catch (err) {
    if (axios.isAxiosError(err) && err.response?.status === 404) {
      set({ profile: null, error: null }); // ✅ 404 = not found, not error
    } else {
      set({ error: 'Failed to load profile' }); // ✅ Real errors only
    }
  }
}
```

**Rule**: HTTP status codes có semantic meaning:
- `404` = Resource not found (expected state, handle gracefully)
- `401` = Unauthorized (redirect to login)
- `403` = Forbidden (show permission error)
- `5xx` = Server error (show error message)
- Network error = Show connection error

### 3.3 Translation Key Management

**Problem**: Pages hardcode tiếng Nhật thay vì dùng `useTranslations` hook.

**Before (Wrong)**:
```tsx
// profile/page.tsx
export default function ProfilePage() {
  return <h1>プロフィール</h1>; // ❌ Hardcoded Japanese
}
```

**After (Correct)**:
```tsx
// profile/page.tsx
export default function ProfilePage() {
  const t = useTranslations('profile');
  return <h1>{t('title')}</h1>; // ✅ Uses translation key
}
```

**Translation files**:
```json
// ja.json
{ "profile": { "title": "プロフィール" } }

// en.json
{ "profile": { "title": "Profile" } }
```

---

## 4. Backend Issues & Fixes

### 4.1 EF Core Migration — PostgreSQL Incompatibility

**Problem**: EF Core generate migration:
```sql
ALTER COLUMN "Interests" TYPE text[] USING ARRAY(SELECT unnest("Interests"::text[]))
```

PostgreSQL không cho phép subquery trong `USING` clause của `ALTER COLUMN`.

**Fix — Manual Migration**:
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Step 1: Add new column
    migrationBuilder.AddColumn<string[]>(
        name: "Interests_New",
        table: "UserProfiles",
        type: "text[]",
        nullable: false,
        defaultValue: Array.Empty<string>());

    // Step 2: Populate from old column
    migrationBuilder.Sql(@"
        UPDATE ""UserProfiles"" 
        SET ""Interests_New"" = string_to_array(""Interests"", ',')
        WHERE ""Interests"" IS NOT NULL AND ""Interests"" != ''
    ");

    // Step 3: Drop old column
    migrationBuilder.DropColumn(name: "Interests", table: "UserProfiles");

    // Step 4: Rename new column
    migrationBuilder.RenameColumn(
        name: "Interests_New",
        table: "UserProfiles",
        newName: "Interests");
}
```

**Lesson**: Luôn test EF Core migrations trên PostgreSQL thực tế. Generated SQL không luôn valid.

### 4.2 Redis Cache — Domain Entity Deserialization

**Problem**: `ProfileService` cache `UserProfile` entity:
```csharp
// ❌ Wrong: Caching domain entity
var profile = await _dbContext.UserProfiles.FindAsync(userId);
await _cacheService.SetAsync($"profile:{userId}", profile); // Fails on read
```

`UserProfile` dùng DDD pattern với private constructor:
```csharp
public class UserProfile
{
    private UserProfile() { } // EF Core constructor
    
    public static UserProfile Create(UserId userId, ...) { ... }
}
```

`System.Text.Json` không thể deserialize vì không có public parameterless constructor → `NotSupportedException`.

**Fix**:
```csharp
// ✅ Correct: No caching, direct DB query
public async Task<UserProfile?> GetProfileAsync(UserId userId)
{
    return await _dbContext.UserProfiles
        .Include(p => p.Photos)
        .FirstOrDefaultAsync(p => p.UserId == userId);
}
```

**Rule**: Chỉ cache plain DTOs/records. Không bao giờ cache domain entities.

### 4.3 CORS Configuration

**Problem**: Frontend không thể gọi API do browser block CORS request với credentials.

**Root Cause**: httpOnly cookie flow yêu cầu `AllowCredentials()` trong CORS policy, nhưng `AllowCredentials()` không compatible với `AllowAnyOrigin()`.

**Fix**:
```csharp
services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",  // PWA
                "http://localhost:3001"   // Admin
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Required for httpOnly cookies
    });
});
```

**Rule**: `AllowCredentials()` yêu cầu explicit origins (không dùng `AllowAnyOrigin()`).

---

## 5. Tool Limitations Discovered

### 5.1 Kiro `fsWrite` Glob Pattern Issue

**Discovery**: Khi write file vào path chứa `[` hoặc `]`, Kiro's `fsWrite` tool interpret chúng như glob character class.

**Example**:
- Path: `app/frontend/pwa/src/app/[locale]/layout.tsx`
- `[locale]` bị interpret là character class matching `l`, `o`, `c`, `a`, `e`
- Result: File được tạo nhưng empty, hoặc write vào wrong path

**Symptom**: `layout.tsx` liên tục empty sau mỗi lần write → "The default export is not a React Component" error trên tất cả pages.

**Workaround**:
```powershell
# Dùng PowerShell -LiteralPath để bypass glob interpretation
$content = @"
// file content here
"@
Set-Content -LiteralPath "app/frontend/pwa/src/app/[locale]/layout.tsx" -Value $content
```

**Affected Tools**: `fsWrite`, `strReplace` — bất kỳ tool nào dùng path matching.

**Rule**: Với files trong thư mục có tên chứa `[` hoặc `]`, LUÔN dùng PowerShell `-LiteralPath`.

### 5.2 Impact Assessment

| Tool | Affected? | Workaround |
|---|---|---|
| `fsWrite` | ✅ Yes | PowerShell `-LiteralPath` |
| `strReplace` | ✅ Yes | PowerShell `-LiteralPath` |
| `readFile` | ❌ No | N/A |
| `getDiagnostics` | ❌ No | N/A |
| `executePwsh` | ❌ No | Use `-LiteralPath` flag |

---

## 6. Coding Standards Evolution

### 6.1 New Rules Added to `coding-standards-frontend.md`

**Section: i18n (Internationalization)**

```markdown
## i18n Rules

### Navigation
- NEVER import from `next/navigation` directly
- ALWAYS use `@/lib/navigation` (next-intl wrapper)
- This ensures locale prefix is preserved on navigation

### Translations
- NEVER hardcode text in any language in components
- ALWAYS use `useTranslations(namespace)` hook
- Add keys to BOTH `ja.json` and `en.json`
- Namespace matches feature folder name

### Routing
- Locale-aware pages live in `app/[locale]/`
- Files in `[locale]/` must be written with PowerShell -LiteralPath
- middleware.ts handles locale detection and redirect
```

**Section: HTTP Error Handling trong Store**

```markdown
## HTTP Error Handling

### Status Code Semantics
- 404: Resource not found — set { data: null }, NOT error state
- 401: Unauthorized — redirect to login
- 403: Forbidden — set error "permission denied"
- 5xx: Server error — set error message
- Network error: Set error "connection failed"

### Pattern
```typescript
} catch (err) {
  if (axios.isAxiosError(err)) {
    if (err.response?.status === 404) {
      set({ data: null, error: null }); // Expected state
    } else {
      set({ error: getErrorMessage(err) }); // Real error
    }
  }
}
```
```

### 6.2 Rationale

Coding standards được update ngay khi phát hiện pattern — không defer đến "sau". Điều này đảm bảo:
- Team members không mắc cùng lỗi
- Standards phản ánh thực tế (không chỉ lý thuyết)
- Lessons learned được institutionalize

---

## 7. DO's and DON'Ts

### Code Generation

**DO's**:
- ✅ Chạy app ngay sau code generation để phát hiện issues sớm
- ✅ Fix issues trong cùng ngày (không defer)
- ✅ Document lessons learned ngay khi phát hiện
- ✅ Update coding standards khi phát hiện pattern mới
- ✅ Test migrations trên real database

**DON'Ts**:
- ❌ Không assume generated code chạy ngay (luôn verify)
- ❌ Không cache domain entities (chỉ cache DTOs)
- ❌ Không dùng `AllowAnyOrigin()` với `AllowCredentials()`
- ❌ Không hardcode text trong components (dùng i18n)
- ❌ Không dùng `next/navigation` trong i18n project

### Tool Usage

**DO's**:
- ✅ Dùng PowerShell `-LiteralPath` cho files trong `[locale]/`
- ✅ Verify file content sau khi write (đặc biệt với special chars)
- ✅ Dùng `getDiagnostics` để check errors thay vì bash commands

**DON'Ts**:
- ❌ Không dùng `fsWrite` cho paths với `[` hoặc `]`
- ❌ Không assume file write thành công (verify)

### Frontend

**DO's**:
- ✅ Dùng `@/lib/navigation` cho tất cả navigation
- ✅ Dùng `useTranslations` cho tất cả text
- ✅ Phân biệt 404 vs real errors trong store
- ✅ Test i18n routing với cả `/ja/` và `/en/` prefixes

**DON'Ts**:
- ❌ Không import từ `next/navigation` trực tiếp
- ❌ Không hardcode text trong bất kỳ ngôn ngữ nào
- ❌ Không treat 404 như error trong store

---

## 8. Lessons Learned

### LL-01: Tool Limitations Phải Được Document Ngay

**Bài học**: Khi phát hiện tool limitation (Kiro `fsWrite` glob issue), document ngay và tạo rule. Không để "biết nhưng không ghi lại".

**Tại sao quan trọng**: Tool limitations tái xuất hiện. Nếu không document, sẽ mất thời gian debug lại.

**Action**: Thêm vào coding standards: "Files trong `[locale]/` phải dùng PowerShell `-LiteralPath`".

### LL-02: DDD Patterns Có Trade-offs Với Infrastructure

**Bài học**: DDD private constructors (tốt cho domain integrity) conflict với JSON serializers (cần public constructor). Phải chọn: domain purity vs infrastructure compatibility.

**Resolution**: Domain entities giữ private constructors. Infrastructure layer dùng DTOs riêng khi cần serialize.

**Rule**: Không bao giờ serialize/cache domain entities trực tiếp.

### LL-03: HTTP Semantics Phải Được Encode Trong Store

**Bài học**: HTTP status codes có semantic meaning. Store phải encode semantics này, không chỉ "success vs failure".

**Pattern**:
- 200: Success → set data
- 404: Not found → set null (not error)
- 401: Unauthorized → trigger logout
- 5xx: Server error → set error message

### LL-04: i18n Là Cross-Cutting Concern

**Bài học**: i18n không phải "add later" feature. Nó ảnh hưởng routing, navigation, component structure, và testing. Phải plan từ đầu.

**Impact**: Nếu i18n được include trong code generation plan từ đầu, sẽ tiết kiệm ~1 giờ debug.

**Action**: Thêm i18n setup vào code generation plan template cho future projects.

### LL-05: "First Run Checklist" Tiết Kiệm Thời Gian

**Bài học**: Sau code generation, có một set predictable issues thường xảy ra (CORS, env config, routing, seed data). Nếu có checklist, có thể verify nhanh thay vì debug từng cái.

**Proposed Checklist**:
```
□ Docker Compose starts successfully
□ Backend API responds on expected port
□ Frontend loads without console errors
□ CORS configured for frontend origins
□ Database migrations applied
□ Seed data created
□ i18n routing works (/ja/ and /en/)
□ Login flow works end-to-end
□ All unit tests pass
□ All integration tests pass
```

---

## 9. Metrics & Outcomes

### 9.1 Time Breakdown

| Activity | Time | % of Day |
|---|---|---|
| Code Generation | 4 giờ | 44% |
| Debugging & Fixes | 4 giờ | 44% |
| Standards & Review | 1 giờ | 12% |
| **Total** | **9 giờ** | **100%** |

### 9.2 Code Metrics

| Metric | Value |
|---|---|
| Files generated | ~160 |
| LOC written | ~15,000-20,000 |
| Backend files | ~80 |
| Frontend files | ~50 |
| Test files | ~20 |
| Infrastructure files | ~10 |

### 9.3 Quality Metrics

| Metric | Value |
|---|---|
| Unit tests | 47/47 pass (100%) |
| Integration tests | 17/17 pass (100%) |
| Issues found | 13 |
| Issues resolved | 13 (100%) |
| Blocking issues remaining | 0 |

### 9.4 Cumulative Progress (Day 01 + Day 02)

| Phase | Status |
|---|---|
| Inception Phase | ✅ 100% Complete |
| Construction — Functional Design | ✅ 100% Complete |
| Construction — NFR Requirements | ✅ 100% Complete |
| Construction — NFR Design | ✅ 100% Complete |
| Construction — Infrastructure Design | ✅ 100% Complete |
| Construction — Code Generation | ✅ 100% Complete |
| Construction — Build and Test | ⏳ Pending (Day 03) |

---

## 10. Appendix

### 10.1 Files Modified for Bug Fixes

**Backend**:
- `app/backend/LivestreamApp.API/Migrations/20260322050211_ChangeInterestsToTextArray.cs`
- `app/backend/LivestreamApp.API/Extensions/ServiceCollectionExtensions.cs`
- `app/backend/LivestreamApp.API/Program.cs`
- `app/backend/LivestreamApp.API/appsettings.Development.json`
- `app/backend/LivestreamApp.Profiles/Services/ProfileService.cs`

**Frontend**:
- `app/frontend/pwa/src/middleware.ts`
- `app/frontend/pwa/src/app/[locale]/layout.tsx`
- `app/frontend/pwa/src/i18n/request.ts`
- `app/frontend/pwa/src/lib/navigation.ts`
- `app/frontend/pwa/src/store/profileStore.ts`
- `app/frontend/pwa/src/app/[locale]/profile/page.tsx`
- `app/frontend/pwa/src/app/[locale]/profile/edit/page.tsx`
- `app/frontend/pwa/src/app/[locale]/profile/photos/page.tsx`
- `app/frontend/pwa/src/features/profile/photos/ui/PhotoUploader.tsx`
- `app/frontend/pwa/src/i18n/locales/ja.json`
- `app/frontend/pwa/src/i18n/locales/en.json`

**Standards**:
- `.kiro/steering/coding-standards-frontend.md`

### 10.2 Test Results

```
Unit Tests (47 total):
  LivestreamApp.Tests.Unit
  ├── Auth/
  │   ├── AuthServiceTests          ✅ 12 tests
  │   ├── TokenServiceTests         ✅ 8 tests
  │   └── OtpServiceTests           ✅ 6 tests
  ├── Profiles/
  │   ├── ProfileServiceTests       ✅ 8 tests
  │   ├── PhotoServiceTests         ✅ 7 tests
  │   └── HostVerificationServiceTests ✅ 6 tests
  └── MockServices/
      ├── StripeMockTests           ✅ 5 tests (updated)
      └── LinePayMockTests          ✅ 5 tests (updated)

Integration Tests (17 total):
  LivestreamApp.Tests.Integration
  ├── Auth/
  │   └── AuthFlowTests             ✅ 9 tests
  └── Profiles/
      └── ProfileFlowTests          ✅ 8 tests
```

### 10.3 Development Environment Setup

```bash
# Start infrastructure
docker-compose up -d postgres redis localstack mailhog

# Run backend
cd app/backend/LivestreamApp.API
dotnet run --launch-profile Development
# → http://localhost:5174

# Run PWA
cd app/frontend/pwa
npm run dev
# → http://localhost:3000

# Run Admin
cd app/frontend/admin
npm run dev
# → http://localhost:3001

# Seed accounts
# viewer@demo.com / Demo123! (Role=0)
# admin@demo.com / Demo123! (Role=2)
```

---

## 11. Frontend Architecture Change — Feature-Sliced Design

### 11.1 Vấn Đề Phát Hiện

Sau khi generate code Unit 1 và chạy app thực tế, phát hiện kiến trúc frontend ban đầu không phù hợp với quy mô dự án:

**Kiến trúc cũ (flat structure)**:
```
src/
├── components/          # Tất cả components gộp chung
│   ├── AuthForm.tsx
│   ├── OtpInput.tsx
│   └── ...
├── store/               # Tất cả stores gộp chung
│   ├── authStore.ts
│   └── profileStore.ts
└── lib/api/             # Tất cả API clients gộp chung
    ├── auth.ts
    └── profiles.ts
```

**Vấn đề**:
- 84+ API endpoints → `auth.ts` sẽ có 30+ functions, không manageable
- 5 units, 5-10 developers → merge conflicts thường xuyên
- Không có enforced boundaries → features import lẫn nhau tự do
- Không scale cho Unit 2-5 (Livestream, Payment, Chat, Moderation)

### 11.2 DO: Quyết Định Kiến Trúc Từ Inception

**Vấn đề cốt lõi**: Kiến trúc frontend không được thảo luận đủ sâu trong:
- **Inception — Application Design**: Chỉ define backend modules, không define frontend architecture pattern
- **Construction — Functional Design**: Chỉ list frontend components, không define layer structure

**Rule**: Trong **Application Design stage** (Inception), phải xác định rõ:
1. Frontend architecture pattern (FSD, Atomic Design, Feature-based)
2. Layer structure và dependency rules
3. State management strategy (per-entity vs centralized)
4. API client organization (per-domain vs centralized)

Nếu quyết định này đến muộn (sau code generation), chi phí là recreate toàn bộ frontend.

### 11.3 DO: Tạo Trade-off Analysis File

Khi có architectural decision phức tạp, tạo file phân tích trade-off riêng thay vì thảo luận inline.

**Format chuẩn** (từ `tradeoff-analysis.md`):

```markdown
## Question N: [Decision Title]

**Context**: [Tại sao cần quyết định này]

### Option A: [Tên option]
| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | ... |
| Maintainability | ⭐⭐⭐⭐ | ... |
| Concurrent Dev | ⭐⭐⭐ | ... |
| Scalability | ⭐⭐⭐⭐ | ... |
| Simplicity | ⭐⭐ | ... |

### 🏆 Recommendation: Option A
**Lý do**: ...
```

**Kết quả thực tế**: 14 architectural questions được resolve trong 1 session thay vì nhiều vòng thảo luận. Người phụ trách chỉ cần đọc Summary table và approve/override.

**Artifacts tạo ra**:
- `aidlc-docs/construction/frontend-recreation/tradeoff-analysis.md` — 14 questions, scoring matrix
- `aidlc-docs/construction/frontend-recreation/design.md` — FSD structure chi tiết
- `aidlc-docs/construction/cross-cutting/frontend-component-architecture.md` — Cross-cutting standard
- `.kiro/steering/coding-standards-frontend-uiux.md` — Coding standards theo FSD

### 11.4 Ưu Điểm Kiến Trúc FSD Đã Chọn

**Feature-Sliced Design (FSD)** — kiến trúc được chọn sau phân tích trade-off:

```
app/ → src/views/ → src/widgets/ → src/features/ → src/entities/ → src/shared/
```

**Ưu điểm 1: Parallel Development**
- Mỗi feature slice hoàn toàn độc lập
- 2-3 developers làm song song không conflict
- Ví dụ: Dev A làm `features/auth/login-email/`, Dev B làm `features/profile/edit-profile/` — zero overlap

**Ưu điểm 2: Clear Ownership**
- Bug trong auth flow → chỉ look vào `features/auth/` và `entities/user/`
- Không cần trace qua toàn bộ codebase
- Onboarding nhanh: developer mới chỉ cần hiểu layer rules

**Ưu điểm 3: Enforced Boundaries**
```javascript
// eslint-plugin-boundaries enforce tự động
{ "from": "features", "allow": ["entities", "shared"] }
// features KHÔNG THỂ import từ widgets hoặc views
// Compiler error ngay khi vi phạm
```

**Ưu điểm 4: Scalability cho 84+ API Endpoints**
```
Flat structure:  auth.ts (30+ functions) → unmanageable
FSD structure:   entities/user/api/user.queries.ts (10 functions)
                 entities/profile/api/profile.queries.ts (8 functions)
                 entities/livestream/api/livestream.queries.ts (12 functions)
                 → manageable, domain-driven
```

**Ưu điểm 5: Future-Proof cho Unit 2-5**
- Thêm feature mới = thêm feature slice mới
- Không refactor existing code
- Unit 2 (Livestream): thêm `features/livestream/`, `entities/livestream/`, `widgets/livestream-viewer/`
- Unit 3 (Payment): thêm `features/payment/`, `entities/coin/`
- Existing Unit 1 code không bị ảnh hưởng

**Ưu điểm 6: Server/Client Component Optimization**
- FSD layers map tự nhiên vào Next.js Server/Client components
- `src/views/` → Server Components (data fetching, SEO)
- `src/features/` → Client Components (interactivity)
- Giảm JavaScript bundle size, cải thiện performance

### 11.5 Summary Table — 14 Architectural Decisions

| Question | Quyết định | Lý do chính |
|---|---|---|
| Composition layer | `src/views/` (không `pages/`) | Next.js conflict với `pages/` folder |
| Widgets layer | Tạo ngay | Chuẩn bị cho Unit 2+ (LivestreamViewer, GiftPanel) |
| Recreate strategy | Incremental (feature by feature) | Low risk, test từng feature |
| AuthForm | Duplicate LoginForm/RegisterForm | UI sẽ diverge (LINE button vs terms checkbox) |
| OtpInput | Shared UI primitive | Reusable, semantic correct |
| API clients | `entities/{domain}/api/` | 84 endpoints → domain-driven files |
| Zustand stores | `entities/{domain}/model/` | Domain state, natural sharing |
| Test strategy | Hybrid (reuse scenarios, recreate code) | Preserve coverage + clean implementation |
| i18n | Centralized với namespace | next-intl native, organized |
| Reusable assets | Reuse shadcn/ui, translations | Efficiency, proven code |
| Package.json | Audit + selective update | Security + stability balance |
| Admin scope | Chỉ PWA (Admin sau) | Focus, manageable scope |
| Old codebase | Backup branch | Safety net, clean workspace |
| Code generation | AI generate toàn bộ | Speed + consistency + review process |

---

**Document Version**: 1.1  
**Generated**: 2026-03-22  
**Status**: Complete  
**Next Update**: Day 03 (Build and Test)
