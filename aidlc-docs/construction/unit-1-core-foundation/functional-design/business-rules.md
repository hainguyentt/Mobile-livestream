# Business Rules — Unit 1: Core Foundation

**Unit**: Unit 1 — Core Foundation  
**Ngày tạo**: 2026-03-21

---

## BR-AUTH: Authentication Rules

### BR-AUTH-01: Email Registration
| ID | Rule |
|---|---|
| BR-AUTH-01-1 | Email phải unique trong hệ thống (case-insensitive) |
| BR-AUTH-01-2 | Email được normalize về lowercase trước khi lưu |
| BR-AUTH-01-3 | Password tối thiểu 8 ký tự, phải có ít nhất 1 chữ hoa, 1 chữ thường, 1 số |
| BR-AUTH-01-4 | OTP gồm 6 chữ số, hết hạn sau 10 phút |
| BR-AUTH-01-5 | OTP chỉ được dùng 1 lần (IsUsed = true sau khi verify) |
| BR-AUTH-01-6 | Tối đa 3 lần nhập OTP sai → OTP bị invalidate, phải request OTP mới |
| BR-AUTH-01-7 | Account ở trạng thái `PendingVerification` cho đến khi verify email OTP |
| BR-AUTH-01-8 | Không thể đăng nhập khi account ở trạng thái `PendingVerification` |
| BR-AUTH-01-9 | Sau khi verify email → Account chuyển sang `Active`, tự động issue JWT + Refresh Token |

### BR-AUTH-02: Email/Password Login
| ID | Rule |
|---|---|
| BR-AUTH-02-1 | Sau 5 lần đăng nhập sai → `RequiresCaptcha = true`, trả về lỗi yêu cầu CAPTCHA |
| BR-AUTH-02-2 | Sau 10 lần đăng nhập sai → `LockoutUntil = now + 24h`, trả về lỗi account locked |
| BR-AUTH-02-3 | Khi account bị lock → Gửi email thông báo cho user |
| BR-AUTH-02-4 | `FailedLoginCount` reset về 0 sau khi đăng nhập thành công |
| BR-AUTH-02-5 | Không thể đăng nhập khi `Status = Suspended` hoặc `Status = Banned` |
| BR-AUTH-02-6 | Không thể đăng nhập khi `LockoutUntil > now` |
| BR-AUTH-02-7 | Mỗi lần đăng nhập thành công → Ghi `LoginAttempt` (IsSuccess=true) + cập nhật `LastLoginAt` |

### BR-AUTH-03: LINE Login
| ID | Rule |
|---|---|
| BR-AUTH-03-1 | Sau khi nhận LINE callback, kiểm tra `ProviderEmail` từ LINE có trùng với email đã đăng ký không |
| BR-AUTH-03-2 | Nếu email trùng → Link LINE account vào User hiện có (tạo `ExternalLogin` record), issue JWT |
| BR-AUTH-03-3 | Nếu email không trùng hoặc LINE không cung cấp email → Tạo User mới với `PasswordHash = null` |
| BR-AUTH-03-4 | User tạo qua LINE không có password → Không thể dùng email/password login cho đến khi set password |
| BR-AUTH-03-5 | Một LINE account chỉ được link với 1 User (unique constraint trên `ProviderUserId`) |
| BR-AUTH-03-6 | User đã có LINE link → Đăng nhập LINE lại chỉ refresh token, không tạo ExternalLogin mới |

### BR-AUTH-04: JWT & Refresh Token
| ID | Rule |
|---|---|
| BR-AUTH-04-1 | Access Token (JWT) hết hạn sau 15 phút |
| BR-AUTH-04-2 | Refresh Token hết hạn sau 30 ngày |
| BR-AUTH-04-3 | Refresh Token Rotation: mỗi lần dùng refresh token → Issue token mới + revoke token cũ |
| BR-AUTH-04-4 | Nếu refresh token đã bị revoke được dùng lại → Revoke toàn bộ token chain của user (reuse detection) |
| BR-AUTH-04-5 | Lưu `TokenHash` (SHA-256) thay vì raw token |
| BR-AUTH-04-6 | Khi reset password → Revoke tất cả refresh tokens của user |
| BR-AUTH-04-7 | Khi admin ban/suspend user → Revoke tất cả refresh tokens |
| BR-AUTH-04-8 | JWT payload chứa: `sub` (UserId), `role`, `email`, `iat`, `exp` |

### BR-AUTH-05: Password Reset
| ID | Rule |
|---|---|
| BR-AUTH-05-1 | Reset link chứa signed token (HMAC-SHA256), hết hạn sau 1 giờ |
| BR-AUTH-05-2 | Reset token chỉ dùng được 1 lần |
| BR-AUTH-05-3 | Sau khi reset password thành công → Invalidate tất cả refresh tokens |
| BR-AUTH-05-4 | Nếu email không tồn tại → Vẫn trả về success (không leak thông tin) |
| BR-AUTH-05-5 | Password mới phải khác password cũ |
| BR-AUTH-05-6 | Tối đa 3 request reset password trong 1 giờ per email (rate limit) |

### BR-AUTH-06: Phone Verification
| ID | Rule |
|---|---|
| BR-AUTH-06-1 | Phone verification hoàn toàn optional — không blocking bất kỳ tính năng nào |
| BR-AUTH-06-2 | Sau khi verify phone → `IsPhoneVerified = true`, hiển thị badge "Verified" trên profile |
| BR-AUTH-06-3 | Mỗi số điện thoại chỉ được verify cho 1 account |
| BR-AUTH-06-4 | OTP phone: 6 số, hết hạn 10 phút, tối đa 3 lần nhập sai |
| BR-AUTH-06-5 | Tối đa 3 request OTP phone trong 1 giờ per số điện thoại (rate limit chống spam SMS) |

---

## BR-PROFILE: Profile Rules

### BR-PROFILE-01: Profile Creation
| ID | Rule |
|---|---|
| BR-PROFILE-01-1 | `DisplayName` bắt buộc khi tạo profile |
| BR-PROFILE-01-2 | `DateOfBirth` bắt buộc khi tạo profile |
| BR-PROFILE-01-3 | User phải đủ 18 tuổi (tính từ `DateOfBirth`) — self-declare, không verify thêm |
| BR-PROFILE-01-4 | `DisplayName` unique trong hệ thống (case-insensitive, trim whitespace) |
| BR-PROFILE-01-5 | `IsProfileComplete = true` khi có đủ `DisplayName` + `DateOfBirth` |
| BR-PROFILE-01-6 | Profile được tạo tự động sau khi account active (có thể rỗng ban đầu) |

### BR-PROFILE-02: Photo Management
| ID | Rule |
|---|---|
| BR-PROFILE-02-1 | Tối đa 6 ảnh per user (DisplayIndex 0-5) |
| BR-PROFILE-02-2 | `DisplayIndex = 0` là avatar chính, hiển thị ở tất cả nơi |
| BR-PROFILE-02-3 | Khi upload ảnh mới vào index đã có → Xóa ảnh cũ khỏi S3, thay bằng ảnh mới |
| BR-PROFILE-02-4 | Khi reorder ảnh → Chỉ cập nhật `DisplayIndex`, không upload lại S3 |
| BR-PROFILE-02-5 | Định dạng cho phép: JPEG, PNG, WebP |
| BR-PROFILE-02-6 | Kích thước tối đa: 10MB per ảnh |
| BR-PROFILE-02-7 | Ảnh được resize về 1080x1080 (max) trước khi lưu S3 |
| BR-PROFILE-02-8 | Khi xóa ảnh → Xóa khỏi S3 + xóa record, các ảnh có index cao hơn không tự động fill vào |

### BR-PROFILE-03: Host Verified Badge
| ID | Rule |
|---|---|
| BR-PROFILE-03-1 | Chỉ User có `Role = Host` mới có thể submit verification request |
| BR-PROFILE-03-2 | Chỉ được submit 1 request tại một thời điểm (không submit khi đang `Pending`) |
| BR-PROFILE-03-3 | Admin approve → `HostProfile.IsVerified = true`, `VerificationStatus = Approved`, ghi `VerifiedAt` + `VerifiedByAdminId` |
| BR-PROFILE-03-4 | Admin reject → `VerificationStatus = Rejected`, ghi `VerificationNote`, Host có thể submit lại |
| BR-PROFILE-03-5 | Verified badge hiển thị trên profile và trong danh sách livestream |

---

## BR-MOCK: MockServices Rules

### BR-MOCK-01: Stripe Mock Server
| ID | Rule |
|---|---|
| BR-MOCK-01-1 | Chạy trên port 5001, base URL: `http://localhost:5001/stripe` |
| BR-MOCK-01-2 | Implement skeleton: `POST /v1/payment_intents` (success path) |
| BR-MOCK-01-3 | Implement basic webhook: `POST /webhooks/stripe` với event `payment_intent.succeeded` |
| BR-MOCK-01-4 | Response format giống Stripe API thật (để Unit 3 chỉ cần đổi base URL) |
| BR-MOCK-01-5 | Configurable via `appsettings.json`: `MockMode = true/false` |

### BR-MOCK-02: LINE Pay Mock Server
| ID | Rule |
|---|---|
| BR-MOCK-02-1 | Chạy trên port 5001, base URL: `http://localhost:5001/linepay` |
| BR-MOCK-02-2 | Implement skeleton: `POST /v3/payments/request` (success path) |
| BR-MOCK-02-3 | Implement: `POST /v3/payments/{transactionId}/confirm` (success path) |
| BR-MOCK-02-4 | Response format giống LINE Pay API thật |
| BR-MOCK-02-5 | Unit 3 sẽ mở rộng thêm: error scenarios, webhook callbacks |

---

## BR-SECURITY: Security Rules (từ Security Baseline Extension)

| ID | Rule |
|---|---|
| BR-SEC-01 | Tất cả passwords phải được hash bằng BCrypt (cost factor ≥ 12) |
| BR-SEC-02 | Không log raw passwords, OTP codes, hoặc tokens |
| BR-SEC-03 | Access Token và Refresh Token không được lưu trong `localStorage` — dùng `httpOnly Cookie` |
| BR-SEC-04 | `httpOnly Cookie` phải có flags: `HttpOnly`, `Secure`, `SameSite=Strict` |
| BR-SEC-05 | External Login tokens (LINE access token) phải được encrypt trước khi lưu DB |
| BR-SEC-06 | Rate limiting áp dụng cho: `/auth/register`, `/auth/login`, `/auth/otp/*`, `/auth/reset-password` |
| BR-SEC-07 | CORS chỉ cho phép origins được whitelist (PWA domain + Admin domain) |
| BR-SEC-08 | Tất cả API endpoints (trừ auth) phải require valid JWT |
| BR-SEC-09 | S3 presigned URL cho upload ảnh — không expose S3 credentials cho client |
| BR-SEC-10 | Input validation trên tất cả endpoints (FluentValidation) |
