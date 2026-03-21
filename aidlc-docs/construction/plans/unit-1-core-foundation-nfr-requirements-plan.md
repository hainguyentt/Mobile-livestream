# NFR Requirements Plan — Unit 1: Core Foundation

**Unit**: Unit 1 — Core Foundation  
**Stage**: NFR Requirements  
**Ngày tạo**: 2026-03-21  
**Trạng thái**: Chờ trả lời câu hỏi

---

## Phạm Vi NFR Assessment

Unit 1 bao gồm: Auth, Profiles, MockServices, API host, Docker Compose, PWA auth/profile screens, Admin login.

Các NFR đã rõ từ requirements.md (không cần hỏi):
- API response time < 200ms (NFR-01-2)
- Uptime 99.9% (NFR-03-1)
- TLS 1.3, JWT rotation, rate limiting (NFR-04-x)
- APPI compliance (NFR-04-1)
- i18n JP/EN (NFR-05-1)
- BCrypt cost 12, httpOnly Cookie (từ functional design)

Các điểm cần làm rõ thêm:

---

## Checklist Thực Thi

- [x] Step 1: Phân tích functional design artifacts
- [x] Step 2: Tạo plan với questions
- [x] Step 3: Thu thập câu trả lời từ user
- [x] Step 4: Generate `nfr-requirements.md`
- [x] Step 5: Generate `tech-stack-decisions.md`
- [ ] Step 6: Present completion message
- [ ] Step 7: Chờ user approval

---

## Câu Hỏi Làm Rõ

### Nhóm A — Performance & Scalability

**Q-A1: Database Connection Pooling**

Unit 1 là nền tảng — cần quyết định connection pool size cho PostgreSQL ngay từ đầu:

A. Conservative: min=5, max=20 per instance — phù hợp dev/staging, scale sau  
B. Standard: min=10, max=50 per instance — balance giữa resource và performance  
C. Aggressive: min=20, max=100 per instance — optimize cho production ngay  
D. Để mặc định EF Core / Npgsql tự quản lý  

[Answer]: D

---

**Q-A2: Redis Cache Strategy cho Auth**

Refresh token và session data có nên cache Redis không?

A. Không cache — chỉ dùng PostgreSQL cho refresh tokens (đơn giản hơn)  
B. Cache blacklisted tokens trong Redis (revoked tokens) — giảm DB lookup khi validate  
C. Cache toàn bộ user session trong Redis, PostgreSQL chỉ là persistent backup  
D. Cache user profile data trong Redis (TTL 5 phút) để giảm DB reads  

[Answer]: Kết hợp B + D (có thay đổi về yêu cầu TTL)
B. Cache blacklisted tokens trong Redis (revoked tokens) — giảm DB lookup khi validate 
D. Cache user profile data trong Redis (TTL 15 phút, có thể thiết lập động) để giảm DB reads  

---

**Q-A3: S3 Image Upload — Direct vs Presigned**

Ảnh profile upload theo cách nào?

A. Client upload trực tiếp lên S3 qua presigned URL (server chỉ generate URL) — nhanh hơn, giảm tải server  
B. Client upload lên API server → Server upload lên S3 — đơn giản hơn, server kiểm soát hoàn toàn  
C. Client upload trực tiếp qua presigned URL + server verify sau khi upload xong  

[Answer]: C

---

### Nhóm B — Security & Compliance

**Q-B1: APPI Compliance — Data Retention cho Auth Logs**

APPI yêu cầu có chính sách lưu trữ dữ liệu rõ ràng. `LoginAttempts` table nên giữ bao lâu?

A. 30 ngày — đủ cho security audit, không giữ quá lâu  
B. 90 ngày — phù hợp với nhiều compliance frameworks  
C. 1 năm — cho phép phân tích pattern dài hạn  
D. Không giới hạn — giữ mãi (không khuyến nghị với APPI)  

[Answer]: B

---

**Q-B2: APPI — Right to Erasure (Xóa tài khoản)**

Khi user yêu cầu xóa tài khoản (FR-01-6), dữ liệu xử lý thế nào?

A. Hard delete — xóa hoàn toàn tất cả dữ liệu ngay lập tức  
B. Soft delete — đánh dấu `IsDeleted = true`, xóa thật sau 30 ngày (grace period)  
C. Anonymize — giữ record nhưng xóa PII (email → hash, tên → "Deleted User"), giữ transaction history  
D. Soft delete 30 ngày → Anonymize (không xóa hoàn toàn vì cần audit trail)  

[Answer]: D

---

**Q-B3: Rate Limiting — Granularity**

Rate limiting áp dụng theo cấp độ nào?

A. Per IP only — đơn giản, dễ implement  
B. Per IP + Per User (khi đã auth) — chính xác hơn  
C. Per IP + Per Email (cho auth endpoints) + Per User (cho protected endpoints)  
D. Global rate limit per endpoint (không phân biệt IP/User)  

[Answer]: A+D (Per-IP + Global, với note nâng cấp Per-User cho protected endpoints sau).

---

**Q-B4: Audit Log cho Admin Actions**

Admin approve/reject verified badge cần audit log không?

A. Có — lưu vào `AdminActionLog` table (ai làm gì, lúc nào, với ai)  
B. Không cần riêng — thông tin đã có trong `HostProfile.VerifiedByAdminId` + `VerifiedAt`  
C. Có — nhưng chỉ log vào application log (không cần DB table riêng)  

[Answer]: A

---

### Nhóm C — Reliability & Availability

**Q-C1: Email Service Fallback**

Nếu AWS SES fail khi gửi OTP email, hệ thống xử lý thế nào?

A. Retry 3 lần với exponential backoff → Trả lỗi cho user nếu vẫn fail  
B. Retry qua SES → Fallback sang SMTP provider khác (SendGrid/Mailgun)  
C. Queue vào AWS SQS → Background job retry (user nhận email chậm hơn nhưng không mất)  
D. Retry 3 lần → Fail silently (user không biết, phải request lại)  

[Answer]: A

---

**Q-C2: Health Check Endpoints**

API cần expose health check endpoints nào?

A. Chỉ `/health` — basic liveness check  
B. `/health/live` (liveness) + `/health/ready` (readiness — check DB + Redis)  
C. `/health/live` + `/health/ready` + `/health/startup` (startup probe cho ECS)  
D. Không cần — ECS tự handle  

[Answer]: C

---

### Nhóm D — Tech Stack Confirmation

**Q-D1: ORM Strategy — EF Core Migrations**

Quản lý database schema migrations như thế nào?

A. EF Core Code-First Migrations — tự động generate migration files từ entity changes  
B. EF Core + Fluent Migrator — migration files viết tay, kiểm soát tốt hơn  
C. EF Core Code-First + DbUp — migration scripts SQL thuần, chạy khi startup  
D. EF Core Code-First Migrations + tự động apply khi startup (không cần manual migrate)  

[Answer]: D

---

**Q-D2: Logging Framework**

Logging strategy cho backend:

A. Serilog → AWS CloudWatch Logs (structured logging, JSON format)  
B. Microsoft.Extensions.Logging built-in → CloudWatch  
C. Serilog → CloudWatch + Datadog (dual sink)  
D. NLog → CloudWatch  

[Answer]: A variant (Serilog → CloudWatch cho production, File + Console cho dev/test).

---

**Q-D3: API Versioning**

API versioning strategy:

A. URL path versioning: `/api/v1/auth/login` — rõ ràng, dễ test  
B. Header versioning: `Api-Version: 1` — URL sạch hơn  
C. Không cần versioning ở MVP — thêm sau khi cần  
D. Query string: `/api/auth/login?version=1`  

[Answer]: A

