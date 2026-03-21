# NFR Design Plan — Unit 1: Core Foundation

**Unit**: Unit 1 — Core Foundation  
**Stage**: NFR Design  
**Ngày tạo**: 2026-03-21  
**Trạng thái**: Chờ trả lời câu hỏi

---

## Checklist Thực Thi

- [x] Step 1: Phân tích NFR requirements artifacts
- [x] Step 2: Tạo plan với questions
- [x] Step 3: Thu thập câu trả lời từ user
- [x] Step 4: Generate `nfr-design-patterns.md`
- [x] Step 5: Generate `logical-components.md`
- [ ] Step 6: Present completion message
- [ ] Step 7: Chờ user approval

---

## Patterns đã xác định (không cần hỏi)

Từ NFR requirements, các patterns sau đã rõ:
- **Cache-Aside Pattern** — Redis cho blacklisted tokens + user profile
- **Retry Pattern** — SES email retry 3 lần exponential backoff
- **Circuit Breaker** — không cần ở Unit 1 (không có external service critical path ngoài SES)
- **Token Rotation Pattern** — Refresh token rotation với reuse detection
- **Layered Rate Limiting** — Per-IP + Global (ASP.NET Core built-in)
- **Soft Delete + Anonymization** — APPI compliance
- **Health Check Pattern** — live/ready/startup probes

---

## Câu Hỏi Làm Rõ (Minimal)

### Q1: Cache Invalidation Strategy cho User Profile

Khi profile update, cache `user:profile:{userId}` cần invalidate. Ngoài ra còn trường hợp nào cần invalidate?

A. Chỉ invalidate khi user tự update profile (PUT /profiles/me)  
B. Invalidate khi: user update profile + admin lock/ban account + host verification status thay đổi  
C. Không cần invalidate thủ công — TTL 15 phút tự expire là đủ  

[Answer]: B

---

### Q2: Idempotency cho OTP Request

Nếu user click "Gửi lại OTP" nhiều lần liên tiếp (trước khi cooldown hết), hệ thống xử lý thế nào?

A. Tạo OTP mới mỗi lần request (invalidate OTP cũ) — đơn giản  
B. Trả về lỗi 429 nếu OTP hiện tại chưa hết hạn (không tạo OTP mới)  
C. Tạo OTP mới nhưng giữ OTP cũ vẫn valid cho đến khi hết hạn  

[Answer]: A

