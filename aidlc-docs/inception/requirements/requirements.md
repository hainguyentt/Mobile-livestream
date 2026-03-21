# Tài Liệu Yêu Cầu
# Dự Án: App Livestream Hẹn Hò - Thị Trường Nhật Bản

**Phiên bản**: 1.3 — [Xem lịch sử thay đổi](./requirements.record-of-changes.md)  
**Ngày tạo**: 2026-03-21  
**Cập nhật**: 2026-03-21  
**Trạng thái**: Draft - Chờ phê duyệt

---

## 1. Tổng Quan Dự Án

### 1.1 Phân Tích Yêu Cầu Ban Đầu

| Thuộc tính | Giá trị |
|---|---|
| Loại yêu cầu | New Project (Greenfield) |
| Phạm vi | System-wide (PWA + Backend + Admin) |
| Độ phức tạp | Complex |
| Thị trường | Nhật Bản |

### 1.2 Mô Tả Dự Án

Xây dựng ứng dụng **Progressive Web App (PWA)** kết hợp tính năng **livestream** và **hẹn hò** dành cho thị trường Nhật Bản. Ứng dụng nhắm đến đối tượng **nam giới trưởng thành** (18-70 tuổi), thiên về giải trí và giao lưu xã hội. Mô hình kinh doanh dựa trên **Pay-per-use** thông qua hệ thống coin ảo.

---

## 2. Đối Tượng Người Dùng

### 2.1 Người Dùng Chính (Primary Users)
- **Đối tượng**: Nam giới trưởng thành, 18-70 tuổi, tại Nhật Bản
- **Mục đích**: Giải trí, giao lưu, kết bạn, hẹn hò
- **Hành vi**: Xem livestream, tương tác với host, gửi quà ảo, chat riêng

### 2.2 Host / Streamer
- **Đối tượng**: Người dùng (chủ yếu nữ giới) phát livestream
- **Mục đích**: Giao lưu, nhận quà ảo, kết nối với người xem
- **Hành vi**: Phát livestream public, nhận private call, quản lý hồ sơ

### 2.3 Admin / Moderator
- **Đối tượng**: Nhân viên vận hành
- **Mục đích**: Quản lý nội dung, xử lý vi phạm, quản lý người dùng

---

## 3. Yêu Cầu Chức Năng (Functional Requirements)

### FR-01: Xác Thực & Quản Lý Tài Khoản

| ID | Yêu cầu | Ưu tiên |
|---|---|---|
| FR-01-1 | Đăng ký tài khoản bằng email (xác minh OTP qua email) | Must Have |
| FR-01-2 | Đăng nhập bằng email/password | Must Have |
| FR-01-3 | Đăng nhập bằng Social Login LINE | Must Have |
| FR-01-3b | Đăng nhập bằng Social Login (Google, Apple) | Should Have |
| FR-01-4 | Xác minh số điện thoại để mở khóa tính năng nâng cao (age verification) | Must Have |
| FR-01-5 | Quên mật khẩu / đặt lại mật khẩu qua email | Must Have |
| FR-01-6 | Đăng xuất, xóa tài khoản | Must Have |

### FR-02: Hồ Sơ Người Dùng

| ID | Yêu cầu | Ưu tiên |
|---|---|---|
| FR-02-1 | Tạo và chỉnh sửa hồ sơ: ảnh đại diện, tên hiển thị, giới thiệu bản thân, sở thích | Must Have |
| FR-02-2 | Upload nhiều ảnh hồ sơ (tối đa 6 ảnh) | Must Have |
| FR-02-3 | Cài đặt quyền riêng tư hồ sơ | Should Have |
| FR-02-4 | Huy hiệu xác minh (verified badge) cho host | Should Have |
| FR-02-5 | Hiển thị lịch sử hoạt động, số lượng quà đã nhận | Should Have |

### FR-03: Hệ Thống Matching

| ID | Yêu cầu | Ưu tiên |
|---|---|---|
| FR-03-1 | Thuật toán gợi ý người dùng dựa trên sở thích, hành vi, lịch sử tương tác | Must Have |
| FR-03-2 | Tìm kiếm và lọc người dùng theo: độ tuổi, sở thích, khu vực, trạng thái online | Must Have |
| FR-03-3 | Like / Follow người dùng | Must Have |
| FR-03-4 | Thông báo khi được like / follow | Must Have |
| FR-03-5 | Danh sách người dùng đã like / đang follow | Should Have |

### FR-04: Livestream Public (1-N, N ≤ 50)

| ID | Yêu cầu | Ưu tiên |
|---|---|---|
| FR-04-1 | Host bắt đầu / kết thúc phiên livestream public | Must Have |
| FR-04-2 | Người xem tham gia phòng livestream (tối đa 50 người xem đồng thời) | Must Have |
| FR-04-3 | Chat real-time trong phòng livestream | Must Have |
| FR-04-4 | Gửi quà ảo (virtual gifts) trong livestream, hiển thị animation | Must Have |
| FR-04-5 | Hiển thị danh sách người xem, số lượng người xem real-time | Must Have |
| FR-04-6 | Host có thể kick người xem vi phạm | Should Have |
| FR-04-7 | Lưu lại thống kê phiên livestream (tổng quà, tổng người xem, thời lượng) | Should Have |
| FR-04-8 | Tính phí xem livestream theo phút (pay-per-use) hoặc miễn phí tùy host cài đặt | Must Have |

### FR-05: Livestream Private 1-1

| ID | Yêu cầu | Ưu tiên |
|---|---|---|
| FR-05-1 | Người dùng gửi yêu cầu private call đến host | Must Have |
| FR-05-2 | Host chấp nhận / từ chối yêu cầu private call | Must Have |
| FR-05-3 | Video call 1-1 real-time (sử dụng Agora.io) | Must Have |
| FR-05-4 | Tính phí theo phút cho private call (pay-per-use, trừ coin) | Must Have |
| FR-05-5 | Kết thúc call bởi một trong hai bên | Must Have |
| FR-05-6 | Hiển thị thời gian và chi phí real-time trong call | Must Have |

### FR-06: Chat & Nhắn Tin

| ID | Yêu cầu | Ưu tiên |
|---|---|---|
| FR-06-1 | Chat text 1-1 giữa người dùng đã match hoặc follow | Must Have |
| FR-06-2 | Gửi emoji, sticker trong chat | Should Have |
| FR-06-3 | Thông báo tin nhắn mới (push notification) | Must Have |
| FR-06-4 | Trạng thái đã đọc / chưa đọc | Should Have |
| FR-06-5 | Xóa tin nhắn, chặn người dùng | Must Have |

### FR-07: Hệ Thống Coin & Thanh Toán

| ID | Yêu cầu | Ưu tiên |
|---|---|---|
| FR-07-1 | Nạp coin qua Stripe (thẻ tín dụng/debit) | Must Have |
| FR-07-2 | Nạp coin qua LINE Pay | Must Have |
| FR-07-3 | Các gói nạp coin với giá trị khác nhau (ví dụ: 500¥, 1000¥, 3000¥, 5000¥) | Must Have |
| FR-07-4 | Hiển thị số dư coin real-time | Must Have |
| FR-07-5 | Lịch sử giao dịch coin (nạp, tiêu, nhận) | Must Have |
| FR-07-6 | Host rút tiền từ coin nhận được (qua chuyển khoản ngân hàng Nhật) | Could Have |
| FR-07-7 | Hệ thống quà ảo: danh sách quà với giá coin khác nhau | Must Have |
| FR-07-8 | Hóa đơn điện tử theo yêu cầu của Specified Commercial Transactions Act | Should Have |

### FR-08: Thông Báo (Notifications)

| ID | Yêu cầu | Ưu tiên |
|---|---|---|
| FR-08-1 | Push notification: host bắt đầu livestream | Must Have |
| FR-08-2 | Push notification: tin nhắn mới, like, follow | Must Have |
| FR-08-3 | Push notification: yêu cầu private call | Must Have |
| FR-08-4 | Cài đặt tùy chỉnh thông báo | Should Have |

### FR-09: Kiểm Duyệt Nội Dung

| ID | Yêu cầu | Ưu tiên |
|---|---|---|
| FR-09-1 | AI/ML tự động phát hiện nội dung không phù hợp trong livestream (nudity, violence) | Must Have |
| FR-09-2 | Hệ thống báo cáo (report) người dùng / nội dung vi phạm | Must Have |
| FR-09-3 | Admin dashboard để moderator xử lý báo cáo | Must Have |
| FR-09-4 | Tự động dừng livestream khi phát hiện vi phạm nghiêm trọng | Must Have |
| FR-09-5 | Hệ thống cảnh báo, tạm khóa, khóa vĩnh viễn tài khoản | Must Have |

### FR-11: Leaderboard & Ranking

| ID | Yêu cầu | Ưu tiên |
|---|---|---|
| FR-11-1 | Bảng xếp hạng host theo tổng coin nhận được (daily / weekly / monthly) | Must Have |
| FR-11-2 | Bảng xếp hạng top người tặng quà (top gifters) trong phòng livestream | Must Have |
| FR-11-3 | Hiển thị rank/badge trên hồ sơ host (ví dụ: Top 10, Top 50, Rising Star) | Should Have |
| FR-11-4 | Thông báo khi host lên/xuống hạng đáng kể | Could Have |

### FR-10: Admin Dashboard

| ID | Yêu cầu | Ưu tiên |
|---|---|---|
| FR-10-1 | Quản lý người dùng: xem, tìm kiếm, khóa/mở khóa tài khoản | Must Have |
| FR-10-2 | Quản lý nội dung: xem livestream đang diễn ra, lịch sử | Must Have |
| FR-10-3 | Quản lý tài chính: thống kê doanh thu, giao dịch, yêu cầu rút tiền | Must Have |
| FR-10-4 | Báo cáo & thống kê: DAU/MAU, doanh thu, top host | Should Have |
| FR-10-5 | Quản lý quà ảo: thêm/sửa/xóa danh sách quà | Should Have |
| FR-10-6 | Admin/Moderator có thể remove (kick) người xem vi phạm chính sách khỏi phòng livestream đang diễn ra | Must Have |

---

## 4. Yêu Cầu Phi Chức Năng (Non-Functional Requirements)

### NFR-01: Hiệu Năng (Performance)

| ID | Yêu cầu |
|---|---|
| NFR-01-1 | Độ trễ video call (latency) < 300ms (Agora.io SLA) |
| NFR-01-2 | API response time < 200ms cho 95% requests |
| NFR-01-3 | Hỗ trợ 10,000 concurrent users (năm đầu) |
| NFR-01-4 | PWA load time < 3 giây trên 4G |

### NFR-02: Khả Năng Mở Rộng (Scalability)

| ID | Yêu cầu |
|---|---|
| NFR-02-1 | Kiến trúc có thể scale từ 10K lên 100K users mà không cần refactor lớn |
| NFR-02-2 | Auto-scaling cho backend services trên AWS |
| NFR-02-3 | Database sharding/read replicas khi cần thiết |

### NFR-03: Độ Tin Cậy (Reliability)

| ID | Yêu cầu |
|---|---|
| NFR-03-1 | Uptime 99.9% (downtime < 8.7 giờ/năm) |
| NFR-03-2 | Tự động failover cho database |
| NFR-03-3 | Backup dữ liệu hàng ngày, retention 30 ngày |
| NFR-03-4 | Chat message retention policy: room chat lưu trong Redis Streams (TTL 7 ngày, không persist PostgreSQL); private chat lưu PostgreSQL với partitioning theo tháng, retention 12 tháng |

### NFR-04: Bảo Mật (Security)

| ID | Yêu cầu |
|---|---|
| NFR-04-1 | Tuân thủ APPI (Act on Protection of Personal Information) của Nhật Bản |
| NFR-04-2 | Mã hóa dữ liệu at-rest và in-transit (TLS 1.3) |
| NFR-04-3 | JWT authentication với refresh token rotation |
| NFR-04-4 | Rate limiting trên tất cả API endpoints |
| NFR-04-5 | Không lưu trữ thông tin thẻ tín dụng (delegate cho Stripe) |
| NFR-04-6 | GDPR-compatible data handling (cho người dùng EU nếu có) |

### NFR-05: Khả Năng Sử Dụng (Usability)

| ID | Yêu cầu |
|---|---|
| NFR-05-1 | Giao diện hỗ trợ tiếng Nhật (日本語) và tiếng Anh |
| NFR-05-2 | Hỗ trợ font chữ tiếng Nhật đầy đủ (Hiragana, Katakana, Kanji) |
| NFR-05-3 | PWA installable trên iOS Safari và Android Chrome |
| NFR-05-4 | Responsive design cho màn hình mobile (360px+) và desktop |
| NFR-05-5 | Dark mode support |

---

## 5. Phân Tích Payment Gateway

### 5.1 So Sánh Stripe vs LINE Pay cho Thị Trường Nhật Bản

| Tiêu chí | Stripe | LINE Pay |
|---|---|---|
| Phí giao dịch | 3.6% (Nhật Bản) | ~3.0-3.5% |
| Thẻ tín dụng | ✅ Visa, MC, Amex, JCB | ❌ Không hỗ trợ trực tiếp |
| Ví điện tử | Apple Pay, Google Pay | LINE Pay wallet |
| Độ phổ biến tại Nhật | Cao (developer-friendly) | Rất cao (LINE có 96M users tại Nhật) |
| Tích hợp | REST API, SDK đầy đủ | REST API, LINE Login integration |
| Hỗ trợ recurring | ✅ Subscription | ❌ Hạn chế |
| Thời gian tích hợp | Nhanh (1-2 tuần) | Trung bình (2-4 tuần, cần approval) |
| Phù hợp với Pay-per-use | ✅ Rất tốt | ✅ Tốt |

### 5.2 Quyết Định

**Sử dụng cả hai: Stripe làm primary, LINE Pay làm secondary**

- **Stripe**: Xử lý thẻ tín dụng/debit (Visa, MC, Amex, JCB), Apple Pay, Google Pay → phủ rộng nhất, tích hợp nhanh
- **LINE Pay**: LINE là super-app số 1 tại Nhật (96M users), tăng conversion rate đáng kể cho đối tượng người dùng Nhật, đồng thời tận dụng LINE Login (FR-01-3)

---

## 6. Tech Stack Đề Xuất

### 6.1 Frontend (PWA)
- **Framework**: Next.js 14+ (React) với App Router
- **Styling**: Tailwind CSS
- **State Management**: Zustand hoặc Redux Toolkit
- **PWA**: next-pwa
- **Real-time**: SignalR client (`@microsoft/signalr`) — xem phân tích 6.6
- **Video**: Agora.io Web SDK

### 6.2 Backend (.NET Stack)
- **Runtime**: .NET 8 (ASP.NET Core)
- **API**: REST (ASP.NET Core Web API) + Real-time (ASP.NET Core SignalR)
- **Authentication**: JWT + Refresh Token (ASP.NET Core Identity / custom)
- **ORM**: Entity Framework Core 8
- **Queue**: AWS SQS + Hangfire (background jobs)
- **Language**: C# 12

### 6.3 Database
- **Primary DB**: PostgreSQL (AWS RDS)
- **Cache**: Redis (AWS ElastiCache) — dùng StackExchange.Redis
- **Media Storage**: AWS S3 + CloudFront CDN

### 6.4 Infrastructure (AWS)
- **Compute**: AWS ECS Fargate (containerized, Docker)
- **CDN**: AWS CloudFront
- **Video Streaming**: Agora.io (third-party)
- **Push Notifications**: AWS SNS + Firebase Cloud Messaging
- **Email**: AWS SES
- **Monitoring**: AWS CloudWatch + Datadog

### 6.5 Third-party Services
- **Video/Audio**: Agora.io
- **Payment**: Stripe + LINE Pay
- **Content Moderation**: AWS Rekognition (AI) + Manual moderators
- **SMS/Phone Verification**: Twilio hoặc AWS SNS

### 6.6 Phân Tích Real-time: SignalR vs Socket.io

| Tiêu chí | ASP.NET Core SignalR | Socket.io |
|---|---|---|
| Tích hợp với .NET | ✅ Native, first-class support | ⚠️ Cần Node.js sidecar hoặc bridge |
| Transport | WebSocket, SSE, Long Polling (tự động fallback) | WebSocket, Long Polling (tự động fallback) |
| Client hỗ trợ | JS, .NET, Java, Swift, Python | JS, Python, Java, Swift, v.v. |
| Scale-out (multi-instance) | ✅ Azure SignalR Service hoặc Redis backplane | ✅ Redis adapter |
| Performance | Cao (tích hợp sâu với ASP.NET pipeline) | Cao (Node.js event loop) |
| Độ phức tạp tích hợp | Thấp (cùng solution .NET) | Cao (cần service riêng biệt) |
| Ecosystem | Microsoft-backed, long-term support | Open source, cộng đồng lớn |
| Phù hợp với stack này | ✅ Rất phù hợp | ❌ Không phù hợp (tạo thêm service Node.js) |

**Quyết định: Sử dụng ASP.NET Core SignalR**

Lý do: Backend đã chọn .NET 8, SignalR là giải pháp native, không cần thêm service Node.js riêng biệt, scale-out dễ dàng qua Redis backplane trên AWS ElastiCache, và Microsoft hỗ trợ dài hạn. Socket.io chỉ có lợi thế khi backend là Node.js.

---

## 7. Ràng Buộc & Giả Định

### 7.1 Ràng Buộc
- Ứng dụng phải tuân thủ APPI (Act on Protection of Personal Information) của Nhật Bản
- Nội dung phải phù hợp với luật pháp Nhật Bản (không có nội dung người lớn rõ ràng)
- Xác minh số điện thoại bắt buộc để sử dụng tính năng livestream và thanh toán
- Agora.io được chọn làm giải pháp video (không thay đổi)
- AWS là cloud provider chính

### 7.2 Giả Định
- Người dùng có kết nối internet ổn định (4G/WiFi) để sử dụng livestream
- Host là người dùng đã đăng ký và xác minh danh tính
- Tỷ lệ host/viewer dự kiến: 1 host : 20-50 viewers
- Đội ngũ moderator sẽ làm việc theo ca để xử lý báo cáo

---

## 8. Phạm Vi Ngoài MVP (Out of Scope - Phase 2)

- Tính năng group livestream (nhiều host cùng lúc)
- Tính năng story/reels ngắn
- Hệ thống subscription (gói tháng/năm)
- Tích hợp thêm payment: Convenience store, PayPay
- App native iOS/Android (nếu PWA không đủ)
- Tính năng video dating (speed dating online)
- Leaderboard và ranking system nâng cao (Phase 2 sẽ mở rộng từ FR-11 cơ bản trong MVP)

---

## 9. Tiêu Chí Thành Công (Success Criteria)

| Tiêu chí | Mục tiêu |
|---|---|
| Người dùng đăng ký trong 3 tháng đầu | 5,000+ |
| DAU/MAU ratio | > 20% |
| Thời gian trung bình mỗi phiên | > 15 phút |
| Tỷ lệ chuyển đổi (free → paying) | > 10% |
| Doanh thu tháng thứ 6 | Đủ bù chi phí vận hành |
| Uptime | > 99.9% |
