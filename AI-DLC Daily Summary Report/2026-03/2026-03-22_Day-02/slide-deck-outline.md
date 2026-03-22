# AI-DLC Case Study — Slide Deck Outline
# App Livestream Hẹn Hò - Thị Trường Nhật Bản

**Ngày**: 2026-03-22 (Day 02)  
**Thời lượng**: 45 phút (Full version)  
**Đối tượng**: Team members, Stakeholders, Management

---

## Slide 1: Title Slide
**Title**: AI-DLC Case Study Day 02: Từ Design sang Running Application  
**Subtitle**: Code Generation, Debugging, và Lessons Learned trong 9 Giờ  
**Date**: 2026-03-22  
**Presenter**: [Your Name]

**Speaker Notes**:
- Day 02 focus: thực thi code generation và debug thực tế
- Highlight: app chạy được end-to-end sau 2 ngày
- Nhiều lessons learned thực tế từ debugging

---

## Slide 2: Agenda
**Content**:
1. Recap Day 01 & Today's Goals (3 phút)
2. Code Generation Execution (8 phút)
3. Technical Issues & Resolutions (12 phút)
4. Tool Limitations Discovered (5 phút)
5. Coding Standards Evolution (5 phút)
6. Lessons Learned & Metrics (7 phút)
7. Next Steps (5 phút)

---

## PART 1: RECAP & GOALS (3 phút)

### Slide 3: Day 01 Recap
**Title**: Day 01 → Day 02: Từ Design đến Code

**Content**:
**Day 01 Completed**:
- ✅ Inception Phase (100%) — 35 artifacts
- ✅ Construction Design Stages (100%) — Functional, NFR, Infrastructure
- ✅ Code Generation Plan — 160 steps

**Day 02 Goals**:
- Execute 160-step code generation plan
- Run app in development environment
- Fix issues found during first run
- Document lessons learned

**Visual**: Progress bar Day 01 → Day 02

**Speaker Notes**:
- Day 01 = planning và design
- Day 02 = execution và reality check
- "Reality check" là phần quan trọng nhất

---

### Slide 4: Day 02 Results Overview
**Title**: Day 02 Kết Quả: App Running End-to-End

**Content**:
| Metric | Value |
|---|---|
| Files Generated | ~160 files |
| LOC Written | ~15,000-20,000 |
| Tests Passing | 64/64 (100%) |
| Issues Found & Fixed | 13 |
| Lessons Learned | 5 |
| App Status | ✅ Running in Dev |

**Visual**: Dashboard với metrics

**Speaker Notes**:
- 9 giờ làm việc: 4h code gen + 4h debug + 1h standards
- 100% test pass rate sau khi fix issues
- App chạy được = milestone quan trọng

---

## PART 2: CODE GENERATION EXECUTION (8 phút)

### Slide 5: Code Generation Scope
**Title**: Code Generation: 5 Modules, ~160 Files

**Content**:
| Module | Files | Status |
|---|---|---|
| LivestreamApp.Shared | ~15 | ✅ |
| LivestreamApp.Auth | ~25 | ✅ |
| LivestreamApp.Profiles | ~20 | ✅ |
| LivestreamApp.API | ~30 | ✅ |
| LivestreamApp.MockServices | ~15 | ✅ |
| Frontend PWA | ~35 | ✅ |
| Frontend Admin | ~10 | ✅ |
| Infrastructure | ~10 | ✅ |
| **Total** | **~160** | **✅** |

**Visual**: File tree structure

**Speaker Notes**:
- 160 files = comprehensive Unit 1 implementation
- Mỗi module có domain entities, services, repositories, tests
- Infrastructure = Docker Compose + LocalStack scripts

---

### Slide 6: What Was Generated
**Title**: Generated Code: Backend Architecture

**Content**:
**Backend Structure**:
```
LivestreamApp.Auth/
├── Domain/Entities/     # User, RefreshToken, OtpCode
├── Services/            # AuthService, TokenService, OtpService
├── Repositories/        # IUserRepository, UserRepository
└── Domain/Events/       # UserRegistered, EmailVerified, etc.

LivestreamApp.Profiles/
├── Domain/Entities/     # UserProfile, HostProfile, UserPhoto
├── Services/            # ProfileService, PhotoService
└── Repositories/        # IProfileRepository

LivestreamApp.API/
├── Controllers/V1/      # AuthController, ProfilesController
├── Middleware/          # ExceptionHandling, RateLimiting
└── Extensions/          # ServiceCollectionExtensions
```

**Visual**: Architecture diagram

**Speaker Notes**:
- Clean architecture: Domain → Application → Infrastructure → API
- Middleware stack: Exception → RateLimit → Auth → Logging
- Controllers thin, business logic trong Services

---

### Slide 7: What Was Generated — Frontend
**Title**: Generated Code: Frontend PWA

**Content**:
**PWA Structure**:
```
app/[locale]/
├── login/page.tsx
├── register/page.tsx
├── verify-phone/page.tsx
├── reset-password/page.tsx
├── profile/
│   ├── page.tsx
│   ├── edit/page.tsx
│   └── photos/page.tsx
└── layout.tsx

features/
├── auth/ui/AuthForm.tsx
├── profile/edit/ui/EditProfileForm.tsx
└── profile/photos/ui/PhotoUploader.tsx

store/
├── authStore.ts
└── profileStore.ts
```

**Visual**: Page flow diagram

**Speaker Notes**:
- Next.js App Router với `[locale]` dynamic segment
- Feature-based organization (không page-based)
- Zustand stores cho state management

---

### Slide 8: Test Results
**Title**: Tests: 64/64 Pass (After Fixes)

**Content**:
**Unit Tests (47)**:
- Auth: 26 tests (AuthService, TokenService, OtpService)
- Profiles: 21 tests (ProfileService, PhotoService, HostVerification)
- MockServices: 10 tests (Stripe, LINE Pay)

**Integration Tests (17)**:
- Auth Flow: 9 tests (registration, login, OTP, password reset)
- Profile Flow: 8 tests (create, update, photos, host verification)

**Infrastructure**: Docker-based (PostgreSQL + Redis + LocalStack)

**Visual**: Test results dashboard

**Speaker Notes**:
- Tests fail ban đầu do migration issue
- Sau khi fix migration: 64/64 pass
- Integration tests dùng real Docker containers (không mock)

---

## PART 3: TECHNICAL ISSUES & RESOLUTIONS (12 phút)

### Slide 9: Issues Overview
**Title**: 13 Issues Found & Fixed in Same Day

**Content**:
| Category | Count | Status |
|---|---|---|
| Backend/Infrastructure | 4 | ✅ All resolved |
| Frontend i18n | 3 | ✅ All resolved |
| Frontend routing | 3 | ✅ All resolved |
| Frontend store | 2 | ✅ All resolved |
| Tool limitations | 1 | ✅ Documented + workaround |
| **Total** | **13** | **✅ 100% resolved** |

**Visual**: Issue breakdown chart

**Speaker Notes**:
- 13 issues trong ngày đầu chạy app = bình thường
- Quan trọng: tất cả resolved same day
- Không có blocking issues còn lại

---

### Slide 10: Issue #1 — EF Core Migration Fail
**Title**: Issue: PostgreSQL USING Clause Incompatibility

**Content**:
**Problem**:
```sql
-- EF Core generated (FAILS on PostgreSQL)
ALTER COLUMN "Interests" TYPE text[] 
USING ARRAY(SELECT unnest("Interests"::text[]))
-- ❌ PostgreSQL: subquery not allowed in USING clause
```

**Fix — Manual Migration**:
```
Step 1: Add new column (text[])
Step 2: Populate from old column
Step 3: Drop old column
Step 4: Rename new column
```

**Result**: 17/17 integration tests pass ✅

**Speaker Notes**:
- EF Core generate SQL không luôn valid trên target DB
- PostgreSQL có restrictions mà EF Core không biết
- Fix: viết migration thủ công với 4 steps
- Lesson: luôn test migrations trên real DB

---

### Slide 11: Issue #2 — Redis Cache Deserialization
**Title**: Issue: Domain Entity Không Deserialize Được Từ Redis

**Content**:
**Problem**:
```csharp
// ProfileService cache UserProfile entity
await _cacheService.SetAsync($"profile:{userId}", profile);
// Later: System.Text.Json cannot deserialize
// UserProfile has private constructor (DDD pattern)
// → NotSupportedException
```

**Fix**:
```csharp
// Remove caching, query directly from DB
public async Task<UserProfile?> GetProfileAsync(UserId userId)
{
    return await _dbContext.UserProfiles
        .Include(p => p.Photos)
        .FirstOrDefaultAsync(p => p.UserId == userId);
}
```

**Rule**: Chỉ cache plain DTOs. Không bao giờ cache domain entities.

**Speaker Notes**:
- DDD private constructors = good for domain integrity
- But: JSON serializers need public constructor
- Trade-off: domain purity vs infrastructure compatibility
- Resolution: domain entities không serialize, dùng DTOs nếu cần cache

---

### Slide 12: Issue #3 — i18n Routing Architecture
**Title**: Issue: 404 Sau Khi Login (i18n Routing)

**Content**:
**Problem**: Navigate sau login → mất locale prefix → 404

**Root Cause**: Thiếu next-intl middleware + dùng `next/navigation` thay vì `next-intl/navigation`

**Solution Architecture**:
```
Request → middleware.ts (detect locale)
        → redirect /profile → /ja/profile
        → app/[locale]/layout.tsx (provide context)
        → page.tsx (useTranslations)
```

**New Rule**: Không import từ `next/navigation`. Dùng `@/lib/navigation`.

**Visual**: Request flow diagram

**Speaker Notes**:
- next-intl middleware = bắt buộc cho i18n routing
- `next/navigation` không aware locale → mất prefix
- `lib/navigation.ts` wrapper = single source of truth
- Tất cả components dùng wrapper này

---

### Slide 13: Issue #4 — Store HTTP Error Handling
**Title**: Issue: Profile 404 Treated As Error

**Content**:
**Problem**: User chưa có profile → API trả 404 → store set error → UI hiển thị error thay vì "create profile"

**Before (Wrong)**:
```typescript
} catch (err) {
  set({ error: 'Failed to load profile' }); // ❌
}
```

**After (Correct)**:
```typescript
} catch (err) {
  if (axios.isAxiosError(err) && err.response?.status === 404) {
    set({ profile: null, error: null }); // ✅ Expected state
  } else {
    set({ error: getErrorMessage(err) }); // ✅ Real error
  }
}
```

**Rule**: HTTP 404 = "not found" (expected), không phải lỗi.

**Speaker Notes**:
- HTTP status codes có semantic meaning
- 404 = resource not found = expected state trong nhiều flows
- Store phải encode semantics này
- Pattern áp dụng cho tất cả stores

---

## PART 4: TOOL LIMITATIONS DISCOVERED (5 phút)

### Slide 14: Kiro `fsWrite` Glob Issue
**Title**: Tool Limitation: `fsWrite` Và Glob Pattern

**Content**:
**Discovery**: Kiro `fsWrite` interpret `[locale]` như glob character class

**Symptom**:
- Write `app/frontend/pwa/src/app/[locale]/layout.tsx`
- File được tạo nhưng **empty**
- Tất cả pages trong `[locale]/` bị lỗi

**Root Cause**: `[locale]` → glob `[l,o,c,a,e]` → wrong path

**Workaround**:
```powershell
Set-Content -LiteralPath "app/frontend/pwa/src/app/[locale]/layout.tsx" -Value $content
```

**Rule**: Files trong `[locale]/` → LUÔN dùng PowerShell `-LiteralPath`

**Visual**: Before/After diagram

**Speaker Notes**:
- Mất ~1 giờ debug vì không biết root cause
- Symptom misleading: file tồn tại nhưng empty
- Workaround đơn giản khi biết root cause
- Document ngay để không mất thời gian lần sau

---

### Slide 15: Tool Limitation Impact
**Title**: Impact Assessment & Mitigation

**Content**:
| Tool | Affected | Workaround |
|---|---|---|
| `fsWrite` | ✅ Yes | PowerShell `-LiteralPath` |
| `strReplace` | ✅ Yes | PowerShell `-LiteralPath` |
| `readFile` | ❌ No | N/A |
| `executePwsh` | ❌ No | Use `-LiteralPath` flag |

**Time Lost**: ~1 giờ debug

**Prevention**: Rule trong coding standards + checklist

**Speaker Notes**:
- Limitation ảnh hưởng write tools, không phải read tools
- Workaround reliable và simple
- Quan trọng: document ngay, không để "biết nhưng không ghi"

---

## PART 5: CODING STANDARDS EVOLUTION (5 phút)

### Slide 16: Standards Updated
**Title**: Coding Standards: 2 New Sections Added

**Content**:
**Section 1: i18n Rules**
- Navigation: dùng `@/lib/navigation` (không `next/navigation`)
- Translations: dùng `useTranslations` (không hardcode)
- Routing: middleware.ts bắt buộc
- Files: PowerShell `-LiteralPath` cho `[locale]/`

**Section 2: HTTP Error Handling**
- 404 → set null, không set error
- 401 → redirect to login
- 5xx → set error message
- Pattern code example included

**Visual**: Standards document screenshot

**Speaker Notes**:
- Standards update ngay khi phát hiện pattern
- Không defer đến "sau"
- Standards phản ánh thực tế, không chỉ lý thuyết
- Team members benefit từ lessons learned

---

### Slide 16b: Frontend Architecture Change — FSD
**Title**: Lesson #6: Kiến Trúc Frontend Phải Quyết Định Từ Inception

**Content**:
**Vấn đề phát hiện**:
- Kiến trúc flat (`components/`, `store/`, `lib/api/`) không scale
- 84+ API endpoints → `auth.ts` sẽ có 30+ functions
- 5 units, 5-10 developers → merge conflicts thường xuyên

**Quyết định**: Chuyển sang **Feature-Sliced Design (FSD)**

```
app/ → views/ → widgets/ → features/ → entities/ → shared/
```

**DO's**:
- ✅ Quyết định frontend architecture trong **Inception — Application Design**
- ✅ Tạo `tradeoff-analysis.md` với scoring matrix để đẩy nhanh quyết định
- ✅ 14 questions → resolved trong 1 session

**Visual**: FSD layer diagram

**Speaker Notes**:
- Kiến trúc flat OK cho prototype, không OK cho production
- FSD = enforced boundaries, parallel dev, clear ownership
- Trade-off analysis file = người phụ trách chỉ cần đọc Summary table
- Lesson: architecture decision phải đến sớm, không phải sau code generation

---

### Slide 16c: FSD — Ưu Điểm Thực Tế
**Title**: Feature-Sliced Design: 6 Ưu Điểm Chính

**Content**:
1. **Parallel Development** — 2-3 devs làm song song, zero conflict
2. **Clear Ownership** — Bug trong auth → chỉ look vào `features/auth/`
3. **Enforced Boundaries** — `eslint-plugin-boundaries` enforce tự động
4. **Scalability** — 84 endpoints → domain-driven files, manageable
5. **Future-Proof** — Unit 2-5 thêm slices mới, không refactor existing
6. **Server/Client Optimization** — `views/` = Server, `features/` = Client

**Trade-off accepted**: Phức tạp hơn flat → Đổi lại: scalability cho 5 units

**Visual**: FSD structure diagram với layer colors

**Speaker Notes**:
- Parallel development: Dev A làm login-email, Dev B làm profile-edit — zero overlap
- Enforced boundaries: compiler error ngay khi vi phạm dependency rules
- Server/Client: views/ = Server Components (SEO, data fetching), features/ = Client (interactivity)
- Accepted trade-off: learning curve FSD → đổi lại maintainability và concurrent dev

---

### Slide 17: Standards Evolution Philosophy
**Title**: Living Standards: Update As You Learn

**Content**:
**Principle**: Coding standards là living document

**Process**:
1. Encounter issue in practice
2. Identify root cause
3. Define rule to prevent recurrence
4. Add to coding standards immediately
5. Apply rule going forward

**Day 02 Example**:
- Issue: i18n navigation mất locale prefix
- Rule: "Dùng `@/lib/navigation`, không `next/navigation`"
- Added to: `coding-standards-frontend.md`

**Visual**: Feedback loop diagram

**Speaker Notes**:
- Standards không phải "write once, forget"
- Mỗi lesson learned → potential new rule
- Balance: không quá nhiều rules (paralysis) vs đủ rules (guidance)
- Day 02 thêm 2 sections = reasonable

---

## PART 6: LESSONS LEARNED & METRICS (7 phút)

### Slide 18: Top 5 Lessons Learned
**Title**: Lessons Learned: Day 02

**Content**:
1. **Tool Limitations Matter** — Biết tool limitations từ đầu tiết kiệm thời gian debug
2. **Domain Entity ≠ Cache Object** — DDD private constructors không compatible với JSON serializers
3. **HTTP Semantics in Store** — 404 = expected state, không phải lỗi
4. **i18n Is Cross-Cutting** — Plan i18n từ đầu, không phải afterthought
5. **First Run Checklist** — Predictable issues sau code gen → checklist tiết kiệm thời gian
6. **Frontend Architecture = Inception Decision** — FSD phải được chọn từ Application Design, không phải sau code generation

**Visual**: Lesson icons

**Speaker Notes**:
- 5 lessons = actionable insights, không chỉ observations
- Mỗi lesson có rule cụ thể
- Lessons được institutionalize trong coding standards
- Future projects benefit từ lessons này

---

### Slide 19: Proposed First Run Checklist
**Title**: Lesson #5: First Run Checklist

**Content**:
```
After Code Generation — First Run Checklist:
□ Docker Compose starts (postgres, redis, localstack)
□ Backend API responds on expected port
□ Frontend loads without console errors
□ CORS configured for frontend origins
□ Database migrations applied successfully
□ Seed data created (test accounts)
□ i18n routing works (/ja/ and /en/)
□ Login flow works end-to-end
□ All unit tests pass
□ All integration tests pass
```

**Estimated Time**: 30 phút (vs 4 giờ debug without checklist)

**Visual**: Checklist with checkboxes

**Speaker Notes**:
- 13 issues trong Day 02 = predictable pattern
- Checklist giúp verify nhanh thay vì debug từng cái
- Estimated 30 phút vs 4 giờ = 87% time saving
- Sẽ thêm vào code generation plan template

---

### Slide 20: Day 02 Metrics
**Title**: Metrics: Day 02 by the Numbers

**Content**:
**Time**:
- Code Generation: 4 giờ
- Debugging: 4 giờ
- Standards: 1 giờ
- **Total: 9 giờ**

**Code**:
- ~160 files generated
- ~15,000-20,000 LOC
- 64/64 tests pass

**Quality**:
- 13 issues found → 13 resolved (100%)
- 0 blocking issues remaining
- 5 lessons documented

**Visual**: Metrics dashboard

**Speaker Notes**:
- 9 giờ vs estimate 7.5 giờ = slightly over due to debugging
- Debugging time = investment (lessons learned, standards updated)
- 100% issue resolution rate = good quality
- App running = primary success metric

---

### Slide 21: Cumulative Progress
**Title**: 2-Day Progress: Design → Running App

**Content**:
| Phase | Day 01 | Day 02 | Status |
|---|---|---|---|
| Inception Phase | ✅ 100% | — | Complete |
| Functional Design | ✅ 100% | — | Complete |
| NFR Requirements | ✅ 100% | — | Complete |
| NFR Design | ✅ 100% | — | Complete |
| Infrastructure Design | ✅ 100% | — | Complete |
| Code Generation | ✅ Plan | ✅ Execute | Complete |
| Build and Test | — | — | Day 03 |

**Total Time**: 7.5h + 9h = 16.5 giờ

**Visual**: Progress timeline

**Speaker Notes**:
- 2 ngày = từ ý tưởng đến running application
- Traditional approach: 2-4 tuần cho cùng scope
- Build and Test còn lại cho Day 03
- Unit 1 essentially complete

---

## PART 7: NEXT STEPS (5 phút)

### Slide 22: Day 03 Plan
**Title**: Day 03: Build, Test, và Documentation

**Content**:
**High Priority**:
- Build and Test — full suite với coverage report
- Documentation updates (README, deployment guide, API docs)
- Backend self-review theo coding standards

**Medium Priority**:
- Staging deployment (Docker images + smoke tests)
- Unit 1 sign-off

**Stretch Goals**:
- Start Unit 2 planning (Livestream Engine)

**Visual**: Day 03 roadmap

**Speaker Notes**:
- Build and Test = formal verification của Unit 1
- Documentation = handoff-ready artifacts
- Staging = first external deployment
- Unit 2 = next phase nếu còn thời gian

---

### Slide 23: Unit 2 Preview
**Title**: What's Next: Unit 2 — Livestream Engine

**Content**:
**Unit 2 Scope**:
- 9 user stories (US-03-01 through US-04-03)
- Modules: Livestream, RoomChat, DirectChat
- Key features: Agora.io integration, SignalR hubs, Chat storage

**Dependencies**: Unit 1 (Auth, Profiles, API) ✅ Complete

**Parallel Opportunity**: Unit 2 + Unit 3 (Coin & Payment) có thể parallel

**Estimated Duration**: 2-3 ngày

**Visual**: Unit dependency graph

**Speaker Notes**:
- Unit 2 = most complex unit (real-time video + chat)
- Agora.io integration = new third-party
- SignalR hubs = LivestreamHub + ChatHub
- Parallel với Unit 3 nếu có 2 developers

---

### Slide 24: Q&A
**Title**: Questions & Discussion

**Content**:
- Questions về code generation process?
- Questions về debugging approach?
- Questions về tool limitations?
- Questions về lessons learned?

**Resources**:
- Case Study Report: `AI-DLC Daily Summary Report/2026-03/2026-03-22_Day-02/case-study-report.md`
- Executive Summary: `AI-DLC Daily Summary Report/2026-03/2026-03-22_Day-02/executive-summary.md`
- Coding Standards: `.kiro/steering/coding-standards-frontend.md`

---

### Slide 25: Thank You
**Title**: Thank You!

**Content**:
**Day 02 Key Takeaways**:
- Code generation: ~160 files trong 4 giờ
- Debugging: 13 issues → 13 resolved (100%)
- App running: end-to-end trong dev environment
- Lessons: 5 actionable lessons documented

**2-Day Summary**:
- 16.5 giờ total (vs 40-60h traditional)
- Running application với 64/64 tests passing
- Comprehensive documentation + coding standards

---

## Appendix Slides

### Slide 26: Development Environment
**Title**: Dev Environment Setup

**Content**:
```bash
# Infrastructure
docker-compose up -d postgres redis localstack mailhog

# Backend (port 5174)
cd app/backend/LivestreamApp.API
dotnet run --launch-profile Development

# PWA (port 3000)
cd app/frontend/pwa && npm run dev

# Admin (port 3001)
cd app/frontend/admin && npm run dev
```

**Seed Accounts**:
- `viewer@demo.com` / `Demo123!` (Viewer)
- `admin@demo.com` / `Demo123!` (Admin)

---

### Slide 27: All Issues Fixed
**Title**: Complete Issue List

**Content**:
| # | Issue | Category | Resolution |
|---|---|---|---|
| 1 | Migration USING clause | Backend | Manual migration |
| 2 | CORS AllowCredentials | Backend | Added policy |
| 3 | i18n routing 404 | Frontend | next-intl middleware |
| 4 | Profile 404 as error | Frontend | Store fix |
| 5 | Redis deserialization | Backend | Remove entity cache |
| 6 | `[locale]/layout.tsx` empty | Tool | PowerShell -LiteralPath |
| 7 | Hardcoded Japanese text | Frontend | useTranslations |
| 8 | Base URL wrong | Frontend | Config fix |
| 9 | Reset-password error | Frontend | URL params fix |
| 10 | Profile pages not loading | Frontend | Store init fix |
| 11 | AuthForm navigation | Frontend | Navigation wrapper |
| 12 | AdminLoginPage redirect | Frontend | Redirect fix |
| 13 | Verify URL params | Frontend | Added params |

---

**End of Slide Deck**

**Total Slides**: 29 (27 main + 2 appendix)  
**Estimated Duration**: 45 phút (full version)  
**Format**: PowerPoint, Google Slides, hoặc Keynote
