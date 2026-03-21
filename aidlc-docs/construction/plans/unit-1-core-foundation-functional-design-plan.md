# Functional Design Plan — Unit 1: Core Foundation

**Unit**: Unit 1 — Core Foundation  
**Stage**: Functional Design  
**Ngày tạo**: 2026-03-21  
**Trạng thái**: Artifacts generated — Chờ user approval

---

## Phạm Vi Unit 1

**Stories được giao**:
- US-01-01: Đăng ký tài khoản bằng email
- US-01-02: Đăng nhập bằng email/password
- US-01-03: Đăng nhập bằng LINE Login
- US-01-04: Xác minh số điện thoại
- US-01-05: Đặt lại mật khẩu
- US-02-01: Tạo và chỉnh sửa hồ sơ
- US-02-02: Huy hiệu xác minh cho Host (Should Have)

**Modules liên quan**: `LivestreamApp.Shared`, `LivestreamApp.Auth`, `LivestreamApp.Profiles`, `LivestreamApp.API`, `LivestreamApp.MockServices`

---

## Checklist Thực Thi

- [x] Step 1: Phân tích Unit 1 context (unit-of-work.md + story map)
- [x] Step 2: Tạo plan với questions
- [x] Step 3: Thu thập câu trả lời từ user
- [x] Step 4: Generate `domain-entities.md`
- [x] Step 5: Generate `business-rules.md`
- [x] Step 6: Generate `business-logic-model.md`
- [x] Step 7: Generate `frontend-components.md`
- [ ] Step 8: Present completion message
- [ ] Step 9: Chờ user approval

---

## Câu Hỏi Làm Rõ

### Nhóm A — Domain Model & Entities

**Q-A1: Cấu trúc User entity**

Hệ thống có 2 loại người dùng (Viewer và Host) với hành vi khác nhau. Cách tổ chức entity nào phù hợp nhất?

A. Single `User` table với `UserRole` enum (Viewer/Host/Admin) — đơn giản, dễ query  
B. Single `User` table + `HostProfile` extension table (1-1 optional) — Host có thêm thông tin riêng  
C. Separate tables: `Users` + `HostProfiles` (inheritance) — tách biệt hoàn toàn  
D. Single `User` table với `IsHost` flag + JSON column cho host-specific data  

[Answer]: B

---

**Q-A2: Quản lý ảnh hồ sơ**

Requirements nói tối đa 6 ảnh. Ảnh đầu tiên là avatar. Cần làm rõ:

A. Ảnh được lưu theo thứ tự cố định (index 0-5), user có thể reorder  
B. Ảnh có `IsPrimary` flag, không có thứ tự cụ thể  
C. Ảnh có `DisplayOrder` (1-6), user có thể drag-and-drop reorder  
D. Chỉ 1 avatar + tối đa 5 ảnh phụ (2 loại riêng biệt)  

[Answer]: A

---

**Q-A3: Verified Badge cho Host**

US-02-02 yêu cầu huy hiệu xác minh cho Host. Quy trình xác minh là gì?

A. Admin manually approve — Host submit request → Admin review → Grant badge  
B. Tự động khi đủ điều kiện (ví dụ: phone verified + profile complete + X ngày hoạt động)  
C. Kết hợp: Tự động check điều kiện cơ bản → Admin final approval  
D. Chỉ cần phone verification là đủ để có verified badge  

[Answer]: A

---

### Nhóm B — Business Logic & Workflows

**Q-B1: OTP Email Registration Flow**

Khi đăng ký bằng email, flow OTP cụ thể như thế nào?

A. Submit email → Gửi OTP (6 số, 10 phút) → Verify OTP → Tạo account → Redirect login  
B. Submit email + password → Gửi OTP → Verify OTP → Account active (không cần login lại)  
C. Submit email → Gửi magic link (không dùng OTP số) → Click link → Tạo password  
D. Submit email + password + profile cơ bản → Gửi OTP → Verify → Account active  

[Answer]: B

---

**Q-B2: LINE Login Integration**

Khi user đăng nhập bằng LINE lần đầu tiên, hệ thống xử lý thế nào?

A. Tự động tạo account mới từ LINE profile (displayName, avatar) — không cần thêm bước  
B. Tạo account từ LINE profile → Bắt buộc user nhập thêm email để liên kết  
C. Tạo account từ LINE profile → Hiển thị onboarding screen để hoàn thiện profile  
D. Kiểm tra email LINE có trùng với account hiện có → Merge nếu trùng, tạo mới nếu không  

[Answer]: D

---

**Q-B3: Phone Verification — Mục đích và Timing**

Requirements nói phone verification để "mở khóa tính năng nâng cao (age verification)". Cụ thể:

A. Phone verification bắt buộc ngay khi đăng ký (blocking — không thể dùng app nếu chưa verify)  
B. Phone verification optional khi đăng ký, nhưng bắt buộc trước khi dùng livestream/payment  
C. Phone verification optional hoàn toàn, chỉ hiển thị badge "Verified" nếu có  
D. Phone verification bắt buộc trong 7 ngày đầu (grace period), sau đó lock account nếu chưa verify  

[Answer]: C

---

**Q-B4: Brute-force Protection Strategy**

Cần làm rõ ngưỡng và hành động khi phát hiện brute-force:

A. 5 lần sai → Lock 15 phút → 10 lần sai → Lock 1 giờ → 20 lần sai → Lock vĩnh viễn (cần admin unlock)  
B. 5 lần sai → CAPTCHA bắt buộc → 10 lần sai → Lock 24 giờ  
C. 3 lần sai → Lock 5 phút (exponential backoff: 5min → 30min → 2h → 24h)  
D. 10 lần sai trong 1 giờ → Lock 1 giờ, reset sau mỗi giờ  

[Answer]: B

---

**Q-B5: Password Reset Flow**

A. Gửi link reset qua email (token 1 giờ) → Click link → Nhập password mới → Invalidate tất cả refresh tokens  
B. Gửi OTP 6 số qua email (10 phút) → Nhập OTP + password mới trong 1 form  
C. Gửi link reset qua email → Click link → Nhập OTP thứ 2 qua SMS → Nhập password mới (2FA reset)  
D. Gửi link reset qua email (token 24 giờ) → Click link → Nhập password mới (không invalidate sessions khác)  

[Answer]: A

---

### Nhóm C — Business Rules & Validation

**Q-C1: Profile Completeness Rules**

Những field nào là bắt buộc khi tạo profile lần đầu?

A. Chỉ cần `DisplayName` — tất cả field khác optional  
B. `DisplayName` + `DateOfBirth` (để verify 18+) — bắt buộc  
C. `DisplayName` + `DateOfBirth` + ít nhất 1 ảnh — bắt buộc  
D. `DisplayName` + `DateOfBirth` + `Gender` + ít nhất 1 ảnh — bắt buộc  

[Answer]: B

---

**Q-C2: Age Verification (18+)**

Hệ thống verify tuổi 18+ như thế nào?

A. Self-declare DateOfBirth khi tạo profile — không verify thêm (trust user)  
B. DateOfBirth + Phone verification = đủ để xác nhận 18+  
C. DateOfBirth + Phone verification + ID document upload (manual review)  
D. Chỉ cần Phone verification (số điện thoại Nhật yêu cầu ID thật)  

[Answer]: A

---

**Q-C3: Username / Display Name Rules**

A. `DisplayName` chỉ cần unique trong hệ thống, không có format restriction  
B. `DisplayName` unique + 2-20 ký tự + cho phép tiếng Nhật (Hiragana/Katakana/Kanji) + Latin  
C. `DisplayName` không cần unique (chỉ `UserId` là unique identifier), 2-30 ký tự  
D. `DisplayName` unique + 2-20 ký tự + chỉ Latin và số (không cho tiếng Nhật)  

[Answer]: A

---

### Nhóm D — MockServices Design

**Q-D1: Stripe Mock Server — Scenarios cần hỗ trợ**

Stripe Mock Server (~4 man-days) cần support những scenarios nào trong Unit 1?

A. Chỉ cần health check endpoint — payment scenarios sẽ implement ở Unit 3  
B. Implement đầy đủ ngay: Payment Intent (success/fail/pending), Webhook events, Refund  
C. Implement skeleton: Payment Intent success + basic webhook — Unit 3 sẽ mở rộng  
D. Implement theo nhu cầu Unit 3 hoàn toàn — Unit 1 chỉ setup project structure  

[Answer]: C

---

**Q-D2: LINE Pay Mock Server — Tương tự**

A. Chỉ setup project structure trong Unit 1, implement ở Unit 3  
B. Implement đầy đủ: Request API, Confirm API, Webhook callback  
C. Implement skeleton: Request + Confirm success path — Unit 3 mở rộng  
D. Implement theo nhu cầu Unit 3 hoàn toàn  

[Answer]: C

---

### Nhóm E — Frontend Components (PWA + Admin)

**Q-E1: Authentication UI Flow**

Sau khi đăng nhập thành công, user được redirect đến đâu?

A. Luôn redirect về Home/Discovery page  
B. Redirect về trang trước đó (intended destination) nếu có, không thì Home  
C. Redirect về Profile completion screen nếu profile chưa đầy đủ, không thì Home  
D. Redirect về Onboarding flow (multi-step) cho user mới, Home cho user cũ  

[Answer]: D

---

**Q-E2: Token Storage Strategy (PWA)**

JWT + Refresh Token lưu ở đâu trên client?

A. `localStorage` — đơn giản, persist qua sessions  
B. `httpOnly Cookie` — secure hơn, không accessible bởi JS  
C. `sessionStorage` — mất khi đóng tab  
D. Memory only (Zustand/Redux) + `httpOnly Cookie` cho refresh token  

[Answer]: B

---

**Q-E3: LINE Login Button — Placement**

LINE Login button xuất hiện ở đâu trong auth flow?

A. Chỉ trên Login screen  
B. Cả Login screen và Register screen  
C. Login screen + Register screen + một nút "Connect LINE" trong Profile settings  
D. Chỉ trên Register screen (first-time only)  

[Answer]: C

