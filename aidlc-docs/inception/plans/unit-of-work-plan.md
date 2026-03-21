# Unit of Work Plan
# Dự Án: App Livestream Hẹn Hò - Thị Trường Nhật Bản

---

## PHẦN A: CÂU HỎI LÀM RÕ

Dựa trên Application Design (Modular Monolith, 11 modules), tôi đã có đề xuất 5 units.
Chỉ cần xác nhận một số điểm trước khi generate artifacts.

---

### Câu hỏi 1
Execution plan đề xuất 5 units theo thứ tự sau. Bạn có đồng ý với cách phân chia này không?

```
Unit 1: Core Foundation     — Auth, Profiles, Shared, API base, DB schema, Docker setup
Unit 2: Livestream Engine   — Livestream (Public + Private), Chat, Agora, SignalR Hubs
Unit 3: Coin & Payment      — Payment (Coin, Stripe Mock, LINE Pay Mock), Gifts
Unit 4: Social & Discovery  — Matching algorithm, Leaderboard, Notifications
Unit 5: Admin & Moderation  — Admin module, Moderation, Content AI, Admin Dashboard (Next.js)
```

A) Đồng ý với 5 units này
B) Gộp một số units lại (ít units hơn, mỗi unit lớn hơn)
C) Tách thêm (nhiều units hơn, mỗi unit nhỏ hơn)
D) Other (please describe after [Answer]: tag below)

[Answer]: A

---

### Câu hỏi 2
PWA frontend (Next.js) sẽ được phát triển như thế nào so với backend?

A) Song song với backend — frontend và backend cùng unit, dev cùng lúc
B) Sau backend — mỗi unit hoàn thành backend API trước, rồi mới làm frontend tương ứng
C) Frontend là unit riêng biệt ở cuối — làm toàn bộ backend trước, frontend sau
D) Other (please describe after [Answer]: tag below)

[Answer]: B

---

### Câu hỏi 3
MockServices project (Stripe Mock + LINE Pay Mock) sẽ được phát triển ở unit nào?

A) Unit 1 — setup sớm để các unit sau có thể dùng ngay
B) Unit 3 — cùng với Payment module (khi cần thiết)
C) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## PHẦN B: KẾ HOẠCH THỰC HIỆN

### Bước 1: Tạo unit-of-work.md
- [x] Định nghĩa chi tiết từng unit (scope, modules, deliverables, dependencies)
- [x] Lưu vào `aidlc-docs/inception/application-design/unit-of-work.md`

### Bước 2: Tạo unit-of-work-dependency.md
- [x] Dependency matrix giữa các units
- [x] Xác định thứ tự phát triển và điểm tích hợp
- [x] Lưu vào `aidlc-docs/inception/application-design/unit-of-work-dependency.md`

### Bước 3: Tạo unit-of-work-story-map.md
- [x] Map tất cả 37 user stories vào từng unit
- [x] Lưu vào `aidlc-docs/inception/application-design/unit-of-work-story-map.md`

---

*Vui lòng điền câu trả lời ở Phần A và thông báo khi hoàn thành.*
