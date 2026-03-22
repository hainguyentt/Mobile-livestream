# Frontend Unit 1 Recreate — Requirements Verification Questions

**Ngày tạo**: 2026-03-22
**Cập nhật**: 2026-03-22 (Updated to reflect "recreate from scratch" approach)
**Mục đích**: Clarify requirements cho việc recreate frontend Unit 1 với FSD architecture

Vui lòng trả lời các câu hỏi sau bằng cách điền chữ cái lựa chọn sau tag `[Answer]:`. Nếu không có option nào phù hợp, chọn option cuối (Other) và mô tả chi tiết.

---

## Question 1: FSD Layer — Composition Layer
Unit 1 có screens khá đơn giản (auth forms + profile). Bạn muốn tạo `src/pages/` composition layer riêng hay để composition logic trực tiếp trong `app/**/page.tsx`?

A) Tạo `src/pages/` layer riêng — tách biệt routing (app/) và composition (src/pages/)
B) Để composition logic trong `app/**/page.tsx` — đơn giản hơn cho Unit 1
C) Other (please describe after [Answer]: tag below)

[Answer]: A

> ⚠️ **Cập nhật sau implementation**: Câu trả lời A đã được implement nhưng với tên `src/views/` thay vì `src/pages/`. Lý do: Next.js tự động scan thư mục tên `pages/` và conflict với App Router (lỗi "Conflicting app and page file"). **Tất cả references đến `src/pages/` trong codebase đã được đổi thành `src/views/`.**

---

## Question 2: FSD Layer — Widgets Layer
Unit 1 không có complex composite UI blocks (không có livestream viewer, chat room). Bạn có muốn tạo `src/widgets/` layer ngay bây giờ không?

A) Tạo `src/widgets/` ngay — chuẩn bị sẵn cho Unit 2+
B) Skip widgets layer cho Unit 1 — chỉ tạo khi cần (Unit 2+)
C) Tạo 1-2 simple widgets cho Unit 1 (ví dụ: ProfileCard, AuthCard)
D) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 3: Recreate Strategy
Bạn muốn recreate như thế nào?

A) Big bang — recreate toàn bộ cùng lúc, test sau
B) Incremental — recreate từng feature một, test ngay
C) Other (please describe after [Answer]: tag below)

[Answer]: B

---

## Question 4: Component Classification — AuthForm
`AuthForm` component hiện tại được dùng cho cả login và register. Trong FSD, component này nên ở đâu?

A) `features/auth/login-email/ui/` — duplicate thành LoginForm và RegisterForm riêng
B) `shared/ui/` — giữ như shared component vì reusable
C) `widgets/auth-card/` — treat như widget composite
D) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 5: Component Classification — OtpInput
`OtpInput` component được dùng cho verify-email và verify-phone. Component này nên ở đâu?

A) `features/auth/verify-email/ui/` — duplicate cho mỗi feature
B) `shared/ui/` — giữ như shared UI primitive
C) `entities/otp/ui/` — treat như domain entity component
D) Other (please describe after [Answer]: tag below)

[Answer]: B

---

## Question 6: API Client Location
API clients hiện tại (`lib/api/auth.ts`, `lib/api/profiles.ts`) nên move vào đâu?

A) `entities/user/api/` và `entities/profile/api/` — theo domain entity
B) `features/auth/*/api/` — mỗi feature có API client riêng
C) `shared/lib/api/` — giữ centralized như hiện tại
D) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 7: Zustand Store Location
Zustand stores (`authStore`, `profileStore`) nên move vào đâu?

A) `features/auth/login-email/model/` — mỗi feature có store riêng
B) `entities/user/model/` và `entities/profile/model/` — theo domain entity
C) `shared/lib/store/` — giữ centralized
D) Other (please describe after [Answer]: tag below)

[Answer]: B

---

## Question 8: Test Strategy
Tests hiện tại (`AuthForm.test.tsx`, `OtpInput.test.tsx`, `authStore.test.ts`) — reuse hay recreate?

A) Reuse existing tests — Adapt to new structure
B) Recreate tests from scratch — Fresh test code
C) Hybrid — Reuse test scenarios, recreate test code
D) Other (please describe after [Answer]: tag below)

[Answer]: C

---

## Question 9: i18n Structure
Translation files (`i18n/locales/ja.json`, `en.json`) có cần restructure không?

A) Giữ nguyên structure hiện tại — không cần thay đổi
B) Restructure theo FSD — mỗi feature có translation file riêng
C) Hybrid — giữ centralized nhưng organize keys theo feature
D) Other (please describe after [Answer]: tag below)

[Answer]: c

---

## Question 10: Reusable Assets from Old Codebase
Assets nào từ old codebase có thể reuse?

A) Reuse tất cả assets có thể (shadcn/ui, translations, utilities, test scenarios)
B) Recreate toàn bộ — Fresh start, không reuse gì
C) Selective reuse — Chỉ reuse infrastructure code (client.ts, utilities)
D) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Question 11: Package.json Dependencies
`package.json` dependencies — keep, update, hay recreate?

A) Keep existing dependencies — Minimal changes
B) Update to latest versions — Get latest features và security patches
C) Audit và selective update — Update critical packages only
D) Other (please describe after [Answer]: tag below)

[Answer]: C

---

## Question 12: Admin Dashboard Scope
Admin dashboard (`app/frontend/admin/`) có recreate cùng lúc không?

A) Có — recreate cả PWA và Admin cùng lúc
B) Không — chỉ recreate PWA, Admin recreate riêng sau
C) Partial — chỉ recreate shared components dùng chung
D) Other (please describe after [Answer]: tag below)

[Answer]: D
Thực hiện recreate Admin trước, PWA sau.
Lý do: ưu tiên xong trước 1 app.

---

## Question 13: Old Codebase Handling
Sau khi recreate xong, xử lý old codebase như thế nào?

A) Delete old code ngay — Clean workspace
B) Keep old code trong backup branch — Safety net
C) Archive old code trong `_archive/` folder — Easy reference
D) Other (please describe after [Answer]: tag below)

[Answer]: C

---

## Question 14: Code Generation Approach
Sau khi có design, bạn muốn generate code như thế nào?

A) AI generate toàn bộ code mới — recreate from scratch
B) AI generate migration scripts — transform existing code
C) Manual migration — AI chỉ provide guidance
D) Hybrid — AI generate structure, manual migrate logic
E) Other (please describe after [Answer]: tag below)

[Answer]: A

---

**Hướng dẫn**: 
1. Điền chữ cái lựa chọn sau mỗi tag `[Answer]:`
2. Nếu chọn "Other", mô tả chi tiết sau tag `[Answer]:`
3. Reply "done" hoặc "completed" khi đã trả lời xong tất cả câu hỏi

