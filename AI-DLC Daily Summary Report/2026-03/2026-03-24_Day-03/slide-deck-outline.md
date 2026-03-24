# Slide Deck Outline — Day 03
# AI-DLC: App Livestream Hẹn Hò - Thị Trường Nhật Bản

**Ngày**: 2026-03-24  
**Audience**: Team, Stakeholders  
**Duration**: 30-45 phút  
**Format**: 20 slides

---

## Slide 1: Title Slide

**Title**: AI-DLC Day 03 — Unit 2 Complete & Frontend FSD Architecture  
**Subtitle**: App Livestream Hẹn Hò | Thị Trường Nhật Bản  
**Date**: 2026-03-24  
**Visual**: Project logo + progress bar (2/5 units complete)

---

## Slide 2: Day 03 At a Glance

**Title**: Tổng Quan Day 03

**Key Numbers**:
- ✅ Unit 2: Livestream Engine — COMPLETE
- 🔄 Frontend FSD Recreation — IN PROGRESS
- 14 architectural decisions resolved
- ~140 files generated
- 0 blocking issues

**Visual**: Dashboard với 4 KPI cards

---

## Slide 3: Recap — Where We Are

**Title**: Tiến Độ Tổng Thể (Day 01-03)

**Progress Table**:
| Phase | Status |
|---|---|
| Inception Phase | ✅ 100% |
| Unit 1: Core Foundation | ✅ 100% |
| Unit 2: Livestream Engine | ✅ 100% |
| Frontend FSD | 🔄 80% |
| Unit 3-5 | ⏳ Planned |

**Visual**: Gantt chart hoặc progress bars

---

## Slide 4: Unit 2 — What We Built

**Title**: Unit 2: Livestream Engine — Scope

**3 Modules**:
1. **LivestreamApp.Livestream** — Rooms, viewer sessions, private calls, billing
2. **LivestreamApp.RoomChat** — Real-time chat trong livestream room
3. **LivestreamApp.DirectChat** — Text chat 1-1 giữa users

**Visual**: Architecture diagram với 3 modules + API layer

---

## Slide 5: Unit 2 — Technical Highlights

**Title**: Unit 2 — Key Technical Components

**SignalR Hubs**:
- `LivestreamHub` — Join/Leave room, Send gift, Kick viewer
- `ChatHub` — Room messages, Direct messages, Mark as read

**Background Jobs**:
- `ProcessBillingTickJob` — Trừ coin mỗi 10 giây
- `AutoRejectCallRequestJob` — Auto reject sau 30 giây
- `ExportRoomChatToS3Job` — Export chat history

**Visual**: Flow diagram: User → SignalR Hub → Service → DB/Redis

---

## Slide 6: Unit 2 — Frontend (FSD)

**Title**: Unit 2 Frontend — Feature Slices

**3 Feature Slices**:
```
features/livestream/    → LivestreamGrid, LivestreamRoom, RoomChatPanel
features/private-call/  → CallRequestModal, CallScreen, CallTimer
features/direct-chat/   → ConversationList, ConversationThread
```

**Visual**: Screenshot mockup của 3 screens

---

## Slide 7: The Problem — Flat Structure Doesn't Scale

**Title**: Vấn Đề Phát Hiện: Kiến Trúc Flat Không Scale

**Before (Flat)**:
```
src/
├── components/  ← Tất cả gộp chung
├── store/       ← Tất cả gộp chung
└── lib/api/     ← Tất cả gộp chung
```

**Problems**:
- 84+ API endpoints → 1 file khổng lồ
- 5-10 developers → merge conflicts
- Không có enforced boundaries
- Không scale cho Unit 3-5

**Visual**: "Before" diagram với red warning icons

---

## Slide 8: The Solution — Feature-Sliced Design

**Title**: Giải Pháp: Feature-Sliced Design (FSD)

**FSD Layers**:
```
views/     → Page composition (Server Components)
widgets/   → Complex UI blocks
features/  → User interactions
entities/  → Business entities
shared/    → Reusable primitives
```

**Dependency Rule**: Chỉ import từ layer thấp hơn (enforced by ESLint)

**Visual**: FSD pyramid diagram với arrows chỉ chiều dependency

---

## Slide 9: FSD vs Alternatives — Decision Matrix

**Title**: Tại Sao Chọn FSD?

**Scoring Matrix**:
| Tiêu chí | Flat | Atomic | Feature | FSD |
|---|---|---|---|---|
| Maintainability | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| Concurrent Dev | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| Scalability | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| Simplicity | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐ |

**Winner**: FSD — thắng 3/4 tiêu chí quan trọng

**Visual**: Bảng với highlight row FSD

---

## Slide 10: Trade-off Analysis Pattern

**Title**: Pattern Mới: Trade-off Analysis File

**Problem**: Architectural decisions thảo luận inline → nhiều vòng → không documented

**Solution**: File phân tích trade-off với format chuẩn:
```
Question N: [Title]
Context: [Why needed]
Option A: [Scoring matrix]
Option B: [Scoring matrix]
🏆 Recommendation: Option X
```

**Result**: 14 questions → 1 session (vs ước tính 3-4 sessions)

**Visual**: Before/After comparison

---

## Slide 11: 14 Architectural Decisions

**Title**: 14 Decisions Resolved

**Summary Table** (key decisions):
| Decision | Choice | Rationale |
|---|---|---|
| Architecture | FSD | Scalability + boundaries |
| Execution order | Admin first | Test ground |
| API clients | `entities/*/api/` | Domain-driven |
| Stores | `entities/*/model/` | Domain state |
| Test strategy | Hybrid | Preserve coverage |

**Visual**: Checklist với 14 items, tất cả checked

---

## Slide 12: FSD + Next.js = Natural Fit

**Title**: FSD Layers Map Vào Next.js Server/Client Components

**Mapping**:
```
views/    → Server Components (data fetching, SEO)
features/ → Client Components (interactivity)
entities/ → Mixed (API server-side, stores client-side)
shared/   → Mixed (UI primitives, utilities)
```

**Benefit**: Giảm JavaScript bundle size tự nhiên

**Visual**: Diagram với Server/Client component icons

---

## Slide 13: ESLint Boundaries — Automated Enforcement

**Title**: Tự Động Enforce FSD Rules

**Config**:
```javascript
{ "from": "features", "allow": ["entities", "shared"] }
// features KHÔNG THỂ import từ widgets
// → Compiler error ngay lập tức
```

**Benefit**: Không cần code review để catch violations

**Visual**: Screenshot ESLint error khi vi phạm

---

## Slide 14: Lesson Learned — Architecture First

**Title**: LL-07: Frontend Architecture = First-class Citizen

**Problem**: Kiến trúc flat phát hiện muộn → recreate toàn bộ frontend

**Cost**: ~1 ngày delay + rewrite tất cả Unit 1 frontend code

**Prevention**:
- Inception — Application Design: Thêm câu hỏi bắt buộc về frontend architecture
- Construction — Functional Design: Thiết kế FSD layers trước code generation

**Rule**: "Frontend architecture không phải afterthought"

**Visual**: Timeline showing "early decision" vs "late decision" cost

---

## Slide 15: Lesson Learned — Requirements Safety

**Title**: LL-10: Không Bao Giờ Overwrite Requirements Gốc

**Incident**: Tạo requirements cho frontend recreation → accidentally overwrite `requirements.md` gốc

**Fix**: `git checkout HEAD -- requirements.md`

**Rule**:
- Change requests → file riêng với ID prefix riêng (FRR, NFRR, ADR)
- Commit thường xuyên = safety net

**Visual**: Git history diagram

---

## Slide 16: Pending Items

**Title**: Việc Cần Làm (Day 04)

**High Priority**:
1. EF Core Migration Unit 2 (`dotnet ef migrations add`)
2. Install npm packages (`agora-rtc-sdk-ng`, `@microsoft/signalr`)
3. Build and Test — Full Suite (Unit 1 + Unit 2)

**Medium Priority**:
4. Verify Frontend FSD (chạy app, check ESLint)
5. Start Unit 3: Coin & Payment — Functional Design

**Visual**: Priority matrix (2x2: Urgency vs Impact)

---

## Slide 17: Cumulative Metrics (Day 01-03)

**Title**: Metrics Tổng Hợp 3 Ngày

| Metric | Value |
|---|---|
| Total files generated | ~400 files |
| Total LOC | ~30,000-40,000 |
| Tests written | ~70 tests |
| Lessons learned | 10 documented |
| Architectural decisions | 14 resolved |
| Coding standards updates | 3 times |
| Blocking issues | 0 |

**Visual**: Bar chart hoặc infographic

---

## Slide 18: Architecture Overview (Current State)

**Title**: Kiến Trúc Hiện Tại (Sau Day 03)

**Backend (Modular Monolith)**:
```
Unit 1: Shared, Auth, Profiles, API, MockServices
Unit 2: Livestream, RoomChat, DirectChat
Unit 3-5: Planned
```

**Frontend (FSD)**:
```
PWA: views/widgets/features/entities/shared
Admin: Planned (Day 04)
```

**Visual**: Full architecture diagram

---

## Slide 19: Roadmap

**Title**: Roadmap Tiếp Theo

**Day 04**:
- Build and Test (Unit 1 + 2)
- Unit 3: Coin & Payment — Functional Design → Code Generation

**Day 05-06**:
- Unit 4: Social & Discovery
- Frontend Admin Recreation

**Day 07+**:
- Unit 5: Admin & Moderation
- Staging Deployment
- User Acceptance Testing

**Visual**: Timeline với milestones

---

## Slide 20: Q&A

**Title**: Câu Hỏi & Thảo Luận

**Key Takeaways**:
1. Unit 2 complete — Livestream Engine ready
2. FSD architecture established — scalable foundation
3. Trade-off analysis pattern — decision accelerator
4. 0 blocking issues — on track

**Contact**: [Team contact info]

**Visual**: Thank you slide với project logo

---

## Speaker Notes

### Slide 7 (Flat Structure Problem)
"Khi chạy app lần đầu sau code generation, chúng ta nhận ra rằng với 84+ API endpoints và 5-10 developers, kiến trúc flat sẽ tạo ra bottleneck. Quyết định recreate sớm tốt hơn là phát hiện muộn khi đã có Unit 3-5."

### Slide 10 (Trade-off Analysis)
"Pattern này tiết kiệm đáng kể thời gian thảo luận. Thay vì back-and-forth trong chat, chúng ta tạo file phân tích với scoring matrix rõ ràng. Người phụ trách chỉ cần đọc Summary table và approve/override."

### Slide 14 (Architecture First)
"Đây là lesson quan trọng nhất của Day 03. Chi phí quyết định kiến trúc muộn = recreate toàn bộ frontend. Từ Day 04 trở đi, frontend architecture sẽ là câu hỏi bắt buộc trong Application Design stage."

---

**Slide Count**: 20 slides  
**Estimated Duration**: 30-45 phút  
**Generated**: 2026-03-24
