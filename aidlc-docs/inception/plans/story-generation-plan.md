# Story Generation Plan
# Dự Án: App Livestream Hẹn Hò - Thị Trường Nhật Bản

---

## PHẦN A: CÂU HỎI LÀM RÕ TRƯỚC KHI SINH STORIES

Vui lòng trả lời các câu hỏi dưới đây để tôi tạo user stories phù hợp nhất.

---

### Câu hỏi 1
Cách tổ chức (breakdown) user stories nào phù hợp nhất với dự án này?

A) Feature-Based — stories nhóm theo tính năng (Livestream, Matching, Chat, Payment, v.v.)
B) Persona-Based — stories nhóm theo loại người dùng (Viewer stories, Host stories, Admin stories)
C) Epic-Based — stories phân cấp: Epic → Story → Sub-task (ví dụ: Epic "Livestream" → Story "Xem livestream" → Sub-task)
D) User Journey-Based — stories theo luồng hành trình người dùng từ đầu đến cuối
E) Other (please describe after [Answer]: tag below)

[Answer]: A

---

### Câu hỏi 2
Mức độ chi tiết của Acceptance Criteria (tiêu chí chấp nhận) cho mỗi story?

A) Ngắn gọn — 2-3 bullet points Given/When/Then cho mỗi story
B) Trung bình — 4-6 bullet points, bao gồm happy path và 1-2 edge cases quan trọng
C) Chi tiết — đầy đủ happy path, edge cases, error scenarios, và validation rules
D) Other (please describe after [Answer]: tag below)

[Answer]: B

---

### Câu hỏi 3
Ngôn ngữ viết user stories là gì?

A) Tiếng Việt hoàn toàn
B) Tiếng Anh hoàn toàn
C) Tiếng Việt cho mô tả, tiếng Anh cho technical terms và acceptance criteria
D) Other (please describe after [Answer]: tag below)

[Answer]: C

---

### Câu hỏi 4
Phạm vi stories cần tạo — tập trung vào đâu?

A) Chỉ MVP features (Must Have) — những gì cần thiết để ra mắt
B) MVP + Should Have features — đầy đủ hơn cho roadmap gần
C) Toàn bộ requirements bao gồm cả Could Have — bức tranh toàn cảnh
D) Other (please describe after [Answer]: tag below)

[Answer]: B

---

### Câu hỏi 5
Với tính năng **Leaderboard & Ranking** (FR-11), stories cần bao gồm những góc nhìn nào?

A) Chỉ góc nhìn Host — host xem rank của mình, nhận thông báo thay đổi rank
B) Chỉ góc nhìn Viewer — viewer xem bảng xếp hạng để chọn host nổi tiếng
C) Cả hai: Host + Viewer + Admin quản lý leaderboard
D) Other (please describe after [Answer]: tag below)

[Answer]: C

---

### Câu hỏi 6
Với tính năng **Pay-per-use coin**, story nào quan trọng nhất cần có acceptance criteria chi tiết nhất?

A) Luồng nạp coin (top-up) — từ chọn gói đến thanh toán thành công/thất bại
B) Luồng tiêu coin trong private call — tính phí theo phút, cảnh báo hết coin, tự động kết thúc
C) Luồng gửi quà ảo trong livestream — chọn quà, trừ coin, hiển thị animation
D) Tất cả ba luồng trên đều cần chi tiết như nhau
E) Other (please describe after [Answer]: tag below)

[Answer]: A

---

### Câu hỏi 7
Personas (chân dung người dùng) cần được mô tả ở mức độ nào?

A) Cơ bản — tên, độ tuổi, mục tiêu chính, pain points
B) Trung bình — thêm hành vi sử dụng app, thiết bị, thói quen
C) Chi tiết — thêm câu chuyện cá nhân (backstory), scenario điển hình, quote đặc trưng
D) Other (please describe after [Answer]: tag below)

[Answer]: B

---

## PHẦN B: KẾ HOẠCH THỰC HIỆN (Sau khi nhận câu trả lời)

### Bước 1: Tạo Personas
- [x] Xác định và mô tả persona Viewer (nam giới 18-70 tuổi, Nhật Bản)
- [x] Xác định và mô tả persona Host/Streamer
- [x] Xác định và mô tả persona Admin/Moderator
- [x] Lưu vào `aidlc-docs/inception/user-stories/personas.md`

### Bước 2: Tạo User Stories - Authentication & Profile
- [x] Stories cho FR-01: Đăng ký, đăng nhập, LINE Login, xác minh số điện thoại
- [x] Stories cho FR-02: Tạo/chỉnh sửa hồ sơ, upload ảnh

### Bước 3: Tạo User Stories - Matching & Discovery
- [x] Stories cho FR-03: Algorithm matching, tìm kiếm, like/follow

### Bước 4: Tạo User Stories - Livestream Public
- [x] Stories cho FR-04: Host bắt đầu stream, viewer tham gia, chat, gửi quà, tính phí

### Bước 5: Tạo User Stories - Livestream Private 1-1
- [x] Stories cho FR-05: Gửi/nhận yêu cầu call, video call, tính phí theo phút, cảnh báo coin

### Bước 6: Tạo User Stories - Chat & Notifications
- [x] Stories cho FR-06: Chat 1-1, emoji, trạng thái đọc, chặn
- [x] Stories cho FR-08: Push notifications

### Bước 7: Tạo User Stories - Coin & Payment
- [x] Stories cho FR-07: Nạp coin (Stripe, LINE Pay), gói coin, lịch sử, rút tiền host

### Bước 8: Tạo User Stories - Leaderboard & Ranking
- [x] Stories cho FR-11: Bảng xếp hạng host, top gifters, rank badge

### Bước 9: Tạo User Stories - Content Moderation & Admin
- [x] Stories cho FR-09: AI filter, báo cáo vi phạm, xử lý moderator
- [x] Stories cho FR-10: Admin dashboard, quản lý người dùng, remove viewer vi phạm

### Bước 10: Review & Finalize
- [x] Kiểm tra tất cả stories tuân thủ INVEST criteria
- [x] Đảm bảo mỗi story có acceptance criteria đầy đủ
- [x] Map personas với stories liên quan
- [x] Lưu vào `aidlc-docs/inception/user-stories/stories.md`

---

*Vui lòng trả lời các câu hỏi ở Phần A và thông báo khi hoàn thành.*
