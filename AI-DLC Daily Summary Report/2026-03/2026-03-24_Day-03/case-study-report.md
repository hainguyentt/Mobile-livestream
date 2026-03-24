# AI-DLC Case Study Report — Day 03
# Dự Án: App Livestream Hẹn Hò - Thị Trường Nhật Bản

**Ngày tạo**: 2026-03-24  
**Phiên bản**: 1.0  
**Mục đích**: Tài liệu chi tiết về Unit 2 Code Generation và Frontend FSD Architecture Decision

---

## Executive Summary

Day 03 đánh dấu hai milestone quan trọng: hoàn thành **Unit 2: Livestream Engine** (134-step code generation) và thiết lập **Frontend FSD Architecture** cho toàn bộ dự án. Điểm đặc biệt là việc phát hiện và xử lý architectural debt sớm — thay vì tiếp tục với kiến trúc flat không scalable, team quyết định recreate frontend theo FSD trước khi tiến sang Unit 3.

---

## Table of Contents

1. [Unit 2: Livestream Engine — Code Generation](#1-unit-2-livestream-engine--code-generation)
2. [Frontend Architecture Decision — FSD](#2-frontend-architecture-decision--fsd)
3. [Trade-off Analysis Pattern](#3-trade-off-analysis-pattern)
4. [FSD Implementation Details](#4-fsd-implementation-details)
5. [Lessons Learned](#5-lessons-learned)
6. [DO's and DON'Ts](#6-dos-and-donts)
7. [Metrics & Outcomes](#7-metrics--outcomes)

---

## 1. Unit 2: Livestream Engine — Code Generation

### 1.1 Scope

Unit 2 bổ sung tính năng core của app: livestream, private video call, và direct chat.

| Module | Scope | Files |
|---|---|---|
| LivestreamApp.Livestream | Rooms, viewer sessions, private calls, billing | ~25 files |
| LivestreamApp.RoomChat | Real-time chat trong livestream room | ~10 files |
| LivestreamApp.DirectChat | Text chat 1-1 giữa users | ~15 files |
| API Layer | SignalR hubs, REST controllers, DTOs | ~20 files |
| Frontend PWA | Feature slices (FSD) | ~20 files |
| Tests | Unit + Integration | ~7 files |
| Documentation | Summary files | ~7 files |
| **Total** | | **~104 files** |

### 1.2 Key Technical Components

**SignalR Hubs**:
```
LivestreamHub (/hubs/livestream)
├── JoinRoom / LeaveRoom
├── SendGift
├── KickViewer (Host only)
└── ViewerCountUpdate (broadcast)

ChatHub (/hubs/chat)
├── JoinRoom / LeaveRoom
├── SendRoomMessage
├── SendDirectMessage
└── MarkAsRead
```

**Background Jobs (Hangfire)**:
```
ProcessBillingTickJob     — Trừ coin mỗi 10 giây trong private call
AutoRejectCallRequestJob  — Auto reject sau 30 giây timeout
CheckAgoraQuotaJob        — Monitor Agora free tier usage
CleanupEndedCallSessionsJob — Dọn dẹp sessions đã kết thúc
ExportRoomChatToS3Job     — Export chat history lên S3 sau khi stream kết thúc
```

**Domain Entities Unit 2**:
```
Livestream: LivestreamRoom, ViewerSession, KickedViewer, PrivateCallRequest, CallSession, BillingTick, AgoraToken, CoinRate
RoomChat: RoomChatMessage
DirectChat: Conversation, DirectMessage, Block
```

### 1.3 Pending Actions (Manual)

Sau code generation, các bước thủ công cần thực hiện:

```bash
# 1. EF Core Migration
dotnet ef migrations add Unit2LivestreamEngine \
  --project app/backend/LivestreamApp.API

# 2. Install npm packages
cd app/frontend/pwa
npm install agora-rtc-sdk-ng @microsoft/signalr

# 3. Run tests
dotnet test --collect:"XPlat Code Coverage"
```

**Lesson**: Nên có "post-code-generation checklist" để không bỏ sót các bước này.

---

## 2. Frontend Architecture Decision — FSD

### 2.1 Vấn Đề Phát Hiện

Sau khi generate code Unit 1 và chạy app thực tế, phát hiện kiến trúc frontend ban đầu không phù hợp:

**Kiến trúc cũ (flat structure)**:
```
src/
├── components/     # Tất cả components gộp chung
├── store/          # Tất cả stores gộp chung
└── lib/api/        # Tất cả API clients gộp chung
```

**Vấn đề cụ thể**:
- 84+ API endpoints → `auth.ts` sẽ có 30+ functions → unmanageable
- 5 units, 5-10 developers → merge conflicts thường xuyên
- Không có enforced boundaries → features import lẫn nhau tự do
- Không scale cho Unit 2-5 (Livestream, Payment, Chat, Moderation)

### 2.2 Quyết Định: Feature-Sliced Design (FSD)

**FSD Layer Structure**:
```
src/
├── views/          # Page composition (Server Components)
├── widgets/        # Complex UI blocks (LivestreamViewer, GiftPanel)
├── features/       # User interactions (login-email, photo-upload)
├── entities/       # Business entities (user, profile, livestream)
└── shared/         # Reusable primitives (ui, lib, config)
```

**Dependency Rules (enforced by eslint-plugin-boundaries)**:
```
views     → widgets, features, entities, shared
widgets   → features, entities, shared
features  → entities, shared
entities  → shared
shared    → (nothing)
```

**Ví dụ cụ thể**:
```
features/auth/login-email/
├── ui/
│   └── LoginEmailForm.tsx    # Client Component
├── model/
│   └── useLoginForm.ts       # Form state + validation
└── index.ts                  # Public API

entities/user/
├── api/
│   └── user.queries.ts       # API calls
├── model/
│   └── userStore.ts          # Zustand store
└── index.ts                  # Public API
```

### 2.3 Tại Sao FSD Thắng Các Alternatives

| Tiêu chí | Flat | Atomic Design | Feature-based | FSD |
|---|---|---|---|---|
| FSD Compliance | ❌ | ⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| Maintainability | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| Concurrent Dev | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| Scalability | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| Simplicity | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐ |

FSD thắng trên 4/5 tiêu chí quan trọng. Chỉ thua về Simplicity (learning curve cao hơn), nhưng đây là trade-off chấp nhận được cho dự án quy mô này.

---

## 3. Trade-off Analysis Pattern

### 3.1 Vấn Đề Trước Khi Có Pattern

Trước đây, architectural decisions được thảo luận inline trong chat → mất nhiều vòng → không documented → khó review.

### 3.2 Pattern Mới: Trade-off Analysis File

**Format chuẩn**:
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

**Kết quả thực tế**:
- 14 architectural questions resolved trong 1 session
- Người phụ trách chỉ cần đọc Summary table và approve/override
- Decisions documented rõ ràng với rationale
- Dễ revisit khi cần thay đổi

### 3.3 14 Decisions Resolved

| # | Question | Decision |
|---|---|---|
| 1 | Composition layer name | `src/views/` (tránh conflict với Next.js `pages/`) |
| 2 | Widgets layer | Tạo ngay (chuẩn bị cho Unit 2+ widgets) |
| 3 | Recreation strategy | Incremental (feature by feature) |
| 4 | AuthForm classification | Duplicate LoginForm/RegisterForm (UI sẽ diverge) |
| 5 | OtpInput classification | `shared/ui/` (reusable primitive) |
| 6 | API clients location | `entities/{domain}/api/` (domain-driven) |
| 7 | Zustand stores location | `entities/{domain}/model/` (domain state) |
| 8 | Test strategy | Hybrid (reuse scenarios, recreate code) |
| 9 | i18n structure | Centralized với namespace |
| 10 | Reusable assets | Reuse shadcn/ui, translations, utilities |
| 11 | Package.json | Audit + selective update |
| 12 | Execution order | Admin first → PWA second |
| 13 | Old codebase | Archive folder (safety net) |
| 14 | Code generation | AI generate toàn bộ |

---

## 4. FSD Implementation Details

### 4.1 PWA Structure (Unit 1 Screens)

```
app/frontend/pwa/src/
├── views/
│   ├── auth/
│   │   ├── LoginView.tsx
│   │   ├── RegisterView.tsx
│   │   ├── OtpVerificationView.tsx
│   │   └── ResetPasswordView.tsx
│   └── profile/
│       ├── ProfileView.tsx
│       ├── EditProfileView.tsx
│       └── PhotosView.tsx
├── features/
│   ├── auth/
│   │   ├── login-email/
│   │   │   ├── ui/LoginEmailForm.tsx
│   │   │   ├── model/useLoginForm.ts
│   │   │   └── index.ts
│   │   └── register/
│   │       ├── ui/RegisterForm.tsx
│   │       ├── model/useRegisterForm.ts
│   │       └── index.ts
│   └── profile/
│       ├── edit/
│       │   ├── ui/EditProfileForm.tsx
│       │   └── index.ts
│       └── photo-upload/
│           ├── ui/PhotoUploadGrid.tsx
│           └── index.ts
├── entities/
│   ├── user/
│   │   ├── api/user.queries.ts
│   │   ├── model/userStore.ts
│   │   └── index.ts
│   └── profile/
│       ├── api/profile.queries.ts
│       ├── model/profileStore.ts
│       └── index.ts
└── shared/
    ├── ui/
    │   ├── OtpInput.tsx
    │   └── LanguageSwitcher.tsx
    └── lib/
        └── api/client.ts
```

### 4.2 PWA Structure (Unit 2 Screens)

```
features/
├── livestream/
│   ├── ui/
│   │   ├── LivestreamGrid.tsx
│   │   ├── LivestreamRoom.tsx
│   │   ├── RoomChatPanel.tsx
│   │   ├── GiftPanel.tsx
│   │   └── HostControls.tsx
│   └── index.ts
├── private-call/
│   ├── ui/
│   │   ├── CallRequestModal.tsx
│   │   ├── CallScreen.tsx
│   │   └── CallEndSummary.tsx
│   └── index.ts
└── direct-chat/
    ├── ui/
    │   ├── ConversationList.tsx
    │   ├── ConversationThread.tsx
    │   └── MessageInput.tsx
    └── index.ts
```

### 4.3 ESLint Boundaries Configuration

```javascript
// .eslintrc.js
{
  "rules": {
    "boundaries/element-types": [2, {
      "default": "disallow",
      "rules": [
        { "from": "views",    "allow": ["widgets", "features", "entities", "shared"] },
        { "from": "widgets",  "allow": ["features", "entities", "shared"] },
        { "from": "features", "allow": ["entities", "shared"] },
        { "from": "entities", "allow": ["shared"] },
        { "from": "shared",   "allow": [] }
      ]
    }]
  }
}
```

Khi developer vi phạm dependency rule (ví dụ: `features` import từ `widgets`), ESLint báo lỗi ngay lập tức — không cần code review để catch.

---

## 5. Lessons Learned

### LL-07: Frontend Architecture Phải Là First-class Citizen Trong Inception

**Bài học**: Kiến trúc frontend không được thảo luận đủ sâu trong Inception → phát hiện muộn → recreate toàn bộ.

**Chi phí phát hiện muộn**:
- ~1 ngày để phân tích, design, và recreate
- Tất cả Unit 1 frontend code phải được viết lại
- Delay Unit 3 planning

**Phòng tránh**:
- Trong **Inception — Application Design**: Thêm câu hỏi bắt buộc về frontend architecture pattern
- Trong **Construction — Functional Design**: Thiết kế FSD layer structure trước code generation
- Không để "kiến trúc frontend" là afterthought

### LL-08: Trade-off Analysis File = Decision Accelerator

**Bài học**: Thay vì thảo luận inline, tạo file phân tích trade-off với format chuẩn.

**Kết quả đo được**:
- 14 questions → 1 session (vs ước tính 3-4 sessions nếu thảo luận inline)
- Decisions documented với rationale rõ ràng
- Dễ revisit và update khi cần

**Template**:
```markdown
## Question N: [Title]
**Context**: [Why needed]
### Option A: [Name]
| Criteria | Score | Rationale |
### 🏆 Recommendation: Option X
**Reason**: [Why]
```

### LL-09: FSD + Next.js App Router = Natural Fit

**Bài học**: FSD layers map tự nhiên vào Next.js Server/Client component model.

**Mapping**:
```
src/views/     → Server Components (data fetching, SEO, no JS bundle)
src/widgets/   → Mixed (Server shell + Client interactive parts)
src/features/  → Client Components (user interactions, state)
src/entities/  → Mixed (API calls server-side, stores client-side)
src/shared/    → Mixed (UI primitives, utilities)
```

**Benefit**: Giảm JavaScript bundle size tự nhiên vì Server Components không ship JS xuống client.

### LL-10: Không Bao Giờ Overwrite Requirements Gốc

**Bài học**: Khi tạo requirements cho change request, accidentally overwrite `requirements.md` gốc.

**Fix**: `git checkout HEAD -- aidlc-docs/inception/requirements/requirements.md`

**Rule**:
- Change requests → file riêng với ID prefix riêng (FRR, NFRR, ADR)
- Không bao giờ modify `requirements.md` gốc cho change requests
- `git` là safety net quan trọng — commit thường xuyên

---

## 6. DO's and DON'Ts

### Frontend Architecture

**DO's**:
- ✅ Quyết định frontend architecture pattern trong Inception (Application Design stage)
- ✅ Tạo trade-off analysis file cho architectural decisions phức tạp
- ✅ Dùng `eslint-plugin-boundaries` để enforce FSD dependency rules
- ✅ Map FSD layers vào Next.js Server/Client components từ đầu
- ✅ Tạo `index.ts` public API cho mỗi FSD slice

**DON'Ts**:
- ❌ Không để kiến trúc frontend là afterthought sau code generation
- ❌ Không import trực tiếp từ internal files của slice khác (dùng `index.ts`)
- ❌ Không vi phạm FSD dependency rules (features không import từ widgets)
- ❌ Không gộp tất cả API clients vào 1 file (dùng entity-based organization)

### Requirements Management

**DO's**:
- ✅ Change requests → file riêng với ID prefix riêng
- ✅ Commit thường xuyên để có safety net
- ✅ Dùng `git checkout HEAD` để rollback khi cần

**DON'Ts**:
- ❌ Không overwrite requirements gốc cho change requests
- ❌ Không modify `requirements.md` trực tiếp (tạo record-of-changes thay thế)

### Post-Code-Generation Checklist

**DO's** (sau mỗi code generation):
- ✅ Chạy `dotnet ef migrations add` ngay
- ✅ Install npm packages ngay
- ✅ Chạy `dotnet test` để verify
- ✅ Chạy `npm run dev` để verify frontend
- ✅ Check ESLint boundaries violations

**DON'Ts**:
- ❌ Không defer migration creation (gây integration test failures)
- ❌ Không defer npm install (gây build failures)

---

## 7. Metrics & Outcomes

### 7.1 Time Breakdown

| Activity | Time | % of Day |
|---|---|---|
| Unit 2 Code Generation (carry-over) | 2 giờ | 22% |
| Frontend FSD Planning & Design | 3 giờ | 33% |
| Frontend Code Generation (PWA) | 4 giờ | 45% |
| **Total** | **9 giờ** | **100%** |

### 7.2 Code Metrics

| Metric | Value |
|---|---|
| Unit 2 backend files | ~50 files |
| Unit 2 frontend files | ~20 files |
| Unit 2 test files | ~7 files |
| Frontend FSD files (PWA) | ~60 files |
| Architecture artifacts | 6 files |

### 7.3 Cumulative Progress (Day 01-03)

| Phase | Status | Notes |
|---|---|---|
| Inception Phase | ✅ 100% | 35 artifacts |
| Unit 1: Core Foundation | ✅ 100% | ~160 files |
| Unit 2: Livestream Engine | ✅ 100% | ~80 files |
| Frontend FSD (PWA) | 🔄 80% | Unit 1+2 done, Admin pending |
| Build and Test | ⏳ 0% | Pending Day 04 |
| Unit 3: Coin & Payment | ⏳ 0% | Starting Day 04 |

### 7.4 Architecture Artifacts Created

| File | Purpose |
|---|---|
| `construction/frontend-recreation/requirements.md` | FRR requirements |
| `construction/frontend-recreation/tradeoff-analysis.md` | 14 decisions |
| `construction/frontend-recreation/design.md` | FSD structure |
| `construction/frontend-recreation/execution-plan.md` | 7-phase plan |
| `construction/cross-cutting/frontend-component-architecture.md` | Cross-cutting standard |
| `.kiro/steering/coding-standards-frontend-uiux.md` | FSD coding standards |

---

**Document Version**: 1.0  
**Generated**: 2026-03-24  
**Status**: Complete  
**Next Update**: Day 04 (Build & Test + Unit 3)
