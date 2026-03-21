# Application Design Plan
# Dự Án: App Livestream Hẹn Hò - Thị Trường Nhật Bản

---

## PHẦN A: CÂU HỎI LÀM RÕ THIẾT KẾ

Chỉ có một số điểm cần xác nhận trước khi thiết kế component. Vui lòng điền `[Answer]:`.

---

### Câu hỏi 1
Backend sẽ tổ chức theo kiến trúc nào?

A) Monolith — một ASP.NET Core project duy nhất, chia theo feature folders (Auth, Livestream, Payment, v.v.)
B) Modular Monolith — một solution nhưng chia thành nhiều projects/modules độc lập, deploy cùng nhau
C) Microservices — mỗi domain (Auth, Livestream, Payment) là một service riêng biệt, deploy độc lập
D) Other (please describe after [Answer]: tag below)

[Answer]: B

---

### Câu hỏi 2
Admin Dashboard sẽ là ứng dụng riêng hay tích hợp vào cùng PWA?

A) Tích hợp vào cùng PWA (Next.js) — route riêng `/admin`, protected by role
B) Ứng dụng web riêng biệt (Next.js project riêng) — deploy độc lập
C) Other (please describe after [Answer]: tag below)

[Answer]: B

---

### Câu hỏi 3
SignalR Hubs sẽ được tổ chức như thế nào?

A) Một Hub duy nhất xử lý tất cả real-time events (chat, livestream, notifications, coin)
B) Nhiều Hub chuyên biệt: `LivestreamHub`, `ChatHub`, `NotificationHub`
C) Other (please describe after [Answer]: tag below)

[Answer]: B

---

### Câu hỏi 4
Mock Services (Stripe Mock, LINE Pay Mock, LocalStack) sẽ được tổ chức ở đâu trong solution?

A) Cùng solution, project riêng `MockServices` — chạy song song với backend khi dev
B) Repository riêng biệt, deploy như một service độc lập
C) Inline trong backend, switch bằng environment variable (không có project riêng)
D) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## PHẦN B: KẾ HOẠCH THỰC HIỆN

### Bước 1: Xác định Components
- [x] Liệt kê tất cả components và responsibilities
- [x] Tạo `aidlc-docs/inception/application-design/components.md`

### Bước 2: Định nghĩa Component Methods
- [x] Method signatures cho từng component
- [x] Tạo `aidlc-docs/inception/application-design/component-methods.md`

### Bước 3: Thiết kế Service Layer
- [x] Service definitions và orchestration patterns
- [x] Tạo `aidlc-docs/inception/application-design/services.md`

### Bước 4: Component Dependencies
- [x] Dependency matrix và communication patterns
- [x] Tạo `aidlc-docs/inception/application-design/component-dependency.md`

### Bước 5: Consolidate
- [x] Tổng hợp tất cả vào `aidlc-docs/inception/application-design/application-design.md`

---

*Vui lòng điền câu trả lời ở Phần A và thông báo khi hoàn thành.*
