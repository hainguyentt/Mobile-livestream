# Unit of Work — Story Map
# Dự Án: App Livestream Hẹn Hò - Thị Trường Nhật Bản

**Tổng stories**: 37 | **Must Have**: 31 | **Should Have**: 5 | **Could Have**: 1

---

## Unit 1: Core Foundation

| Story ID | Tên Story | Priority | Notes |
|---|---|---|---|
| US-01-01 | Đăng ký tài khoản bằng email | Must Have | |
| US-01-02 | Đăng nhập bằng email/password | Must Have | |
| US-01-03 | Đăng nhập bằng LINE Login | Must Have | |
| US-01-04 | Xác minh số điện thoại | Must Have | |
| US-01-05 | Đặt lại mật khẩu | Must Have | |
| US-02-01 | Tạo và chỉnh sửa hồ sơ | Must Have | |
| US-02-02 | Huy hiệu xác minh cho Host | Should Have | |

**Subtotal**: 7 stories (6 Must Have + 1 Should Have)

---

## Unit 2: Livestream Engine

| Story ID | Tên Story | Priority | Notes |
|---|---|---|---|
| US-04-01 | Host bắt đầu livestream public | Must Have | |
| US-04-02 | Viewer tham gia phòng livestream | Must Have | |
| US-04-03 | Chat real-time trong phòng livestream | Must Have | |
| US-04-05 | Host kick viewer vi phạm | Should Have | |
| US-05-01 | Viewer gửi yêu cầu private call | Must Have | |
| US-05-02 | Host chấp nhận/từ chối private call | Must Have | |
| US-05-03 | Video call 1-1 và tính phí theo phút | Must Have | |
| US-06-01 | Chat text 1-1 | Must Have | |
| US-06-02 | Chặn người dùng | Must Have | |

**Subtotal**: 9 stories (8 Must Have + 1 Should Have)

---

## Unit 3: Coin & Payment

| Story ID | Tên Story | Priority | Notes |
|---|---|---|---|
| US-04-04 | Gửi quà ảo trong livestream | Must Have | Cần Unit 2 (SignalR animation) |
| US-07-01 | Nạp coin qua Stripe | Must Have | Dùng Stripe Mock từ Unit 1 |
| US-07-02 | Nạp coin qua LINE Pay | Must Have | Dùng LINE Pay Mock từ Unit 1 |
| US-07-03 | Xem lịch sử giao dịch coin | Must Have | |
| US-07-04 | Host rút tiền từ coin nhận được | Could Have | Out of MVP scope |

**Subtotal**: 5 stories (4 Must Have + 1 Could Have)

---

## Unit 4: Social & Discovery

| Story ID | Tên Story | Priority | Notes |
|---|---|---|---|
| US-03-01 | Xem gợi ý người dùng từ thuật toán | Must Have | |
| US-03-02 | Tìm kiếm và lọc host | Must Have | |
| US-03-03 | Like và Follow host | Must Have | |
| US-08-01 | Push notification khi host livestream | Must Have | |
| US-08-02 | Push notification cho tin nhắn và tương tác | Must Have | |
| US-11-01 | Viewer xem bảng xếp hạng host | Must Have | |
| US-11-02 | Host theo dõi thứ hạng của mình | Must Have | |
| US-11-03 | Hiển thị top gifters trong phòng livestream | Must Have | Cần Unit 3 (gift data) |
| US-11-04 | Admin quản lý leaderboard | Should Have | |

**Subtotal**: 9 stories (8 Must Have + 1 Should Have)

---

## Unit 5: Admin & Moderation

| Story ID | Tên Story | Priority | Notes |
|---|---|---|---|
| US-09-01 | Báo cáo người dùng/nội dung vi phạm | Must Have | |
| US-09-02 | AI tự động phát hiện nội dung vi phạm | Must Have | Dùng LocalStack Rekognition |
| US-09-03 | Moderator xử lý báo cáo vi phạm | Must Have | |
| US-10-01 | Admin quản lý người dùng | Must Have | |
| US-10-02 | Admin remove viewer vi phạm khỏi livestream | Must Have | Cần Unit 2 (SignalR kick) |
| US-10-03 | Admin quản lý tài chính và yêu cầu rút tiền | Must Have | |
| US-10-04 | Admin xem báo cáo thống kê | Should Have | |

**Subtotal**: 7 stories (6 Must Have + 1 Should Have)

---

## Tổng Kết

| Unit | Stories | Must Have | Should Have | Could Have |
|---|---|---|---|---|
| Unit 1: Core Foundation | 7 | 6 | 1 | 0 |
| Unit 2: Livestream Engine | 9 | 8 | 1 | 0 |
| Unit 3: Coin & Payment | 5 | 4 | 0 | 1 |
| Unit 4: Social & Discovery | 9 | 8 | 1 | 0 |
| Unit 5: Admin & Moderation | 7 | 6 | 1 | 0 |
| **Tổng** | **37** | **31** | **4** | **1** |

> **Lưu ý**: US-11-04 (Admin quản lý leaderboard) được tính ở Unit 4, không phải Unit 5.

---

## Cross-Unit Story Dependencies

| Story | Unit | Phụ thuộc vào |
|---|---|---|
| US-04-04 (Gửi quà ảo) | Unit 3 | Unit 2 — cần `LivestreamHub.GiftReceived()` để broadcast animation |
| US-11-03 (Top gifters) | Unit 4 | Unit 3 — cần gift transaction data |
| US-10-02 (Admin kick viewer) | Unit 5 | Unit 2 — cần `LivestreamHub` để kick real-time |
| US-05-03 (Private call billing) | Unit 2 | Unit 3 — cần `ICoinService` interface (finalized trong Unit 1) |
