# Personas
# Dự Án: App Livestream Hẹn Hò - Thị Trường Nhật Bản

---

## Persona 1: Tanaka Hiroshi — Viewer (Người Xem)

| Thuộc tính | Chi tiết |
|---|---|
| Tên | Tanaka Hiroshi (田中 浩) |
| Độ tuổi | 42 tuổi |
| Nghề nghiệp | Nhân viên văn phòng (công ty sản xuất tại Tokyo) |
| Thu nhập | ~5,000,000 ¥/năm (trung bình) |
| Thiết bị | iPhone 14, MacBook tại văn phòng |
| Kết nối | 5G mobile, WiFi tại nhà và văn phòng |

### Mục tiêu
- Giải trí sau giờ làm việc căng thẳng
- Giao lưu, trò chuyện với người mới mà không cần gặp trực tiếp
- Tìm kiếm kết nối cảm xúc trong cuộc sống bận rộn

### Pain Points
- Quá bận để tham gia các hoạt động xã hội trực tiếp
- Cảm thấy cô đơn nhưng ngại tiếp cận người lạ ngoài đời thực
- Lo ngại về quyền riêng tư khi dùng app hẹn hò thông thường

### Hành vi sử dụng app
- Dùng app chủ yếu vào buổi tối (21:00-23:00) và cuối tuần
- Thích xem livestream public trước khi quyết định private call
- Sẵn sàng chi tiêu nếu cảm thấy kết nối thật sự với host
- Thường nạp coin theo gói nhỏ (500¥-1000¥) trước, gói lớn hơn khi đã tin tưởng

### Thói quen
- Dùng LINE hàng ngày để liên lạc → ưu tiên LINE Login và LINE Pay
- Đọc review và xem rank trước khi chọn host để xem
- Hay gửi quà ảo nhỏ để thu hút sự chú ý của host trong livestream public

---

## Persona 2: Yamamoto Yuki — Host / Streamer

| Thuộc tính | Chi tiết |
|---|---|
| Tên | Yamamoto Yuki (山本 由紀) |
| Độ tuổi | 26 tuổi |
| Nghề nghiệp | Part-time barista + streamer bán thời gian |
| Thu nhập | ~2,500,000 ¥/năm (thấp, muốn tăng thu nhập qua app) |
| Thiết bị | Samsung Galaxy S23, tablet Android |
| Kết nối | WiFi tại nhà (ổn định), 4G khi di chuyển |

### Mục tiêu
- Kiếm thêm thu nhập từ coin/quà ảo nhận được
- Xây dựng cộng đồng fan trung thành
- Lên top leaderboard để thu hút nhiều viewer hơn

### Pain Points
- Khó biết mình đang ở vị trí nào so với các host khác
- Lo lắng về viewer có hành vi không phù hợp trong phòng stream
- Muốn kiểm soát được ai được phép private call mình

### Hành vi sử dụng app
- Stream đều đặn 3-4 buổi/tuần, mỗi buổi 1-2 tiếng
- Theo dõi leaderboard hàng ngày để biết thứ hạng
- Chủ động chấp nhận private call từ viewer quen, thận trọng với viewer mới
- Rút tiền về tài khoản ngân hàng mỗi tháng

### Thói quen
- Chuẩn bị background và ánh sáng trước khi stream
- Tương tác tích cực với top gifters để giữ chân họ
- Kick ngay viewer có comment không phù hợp

---

## Persona 3: Suzuki Kenji — Admin / Moderator

| Thuộc tính | Chi tiết |
|---|---|
| Tên | Suzuki Kenji (鈴木 健二) |
| Độ tuổi | 31 tuổi |
| Nghề nghiệp | Nhân viên vận hành nền tảng (full-time) |
| Thiết bị | Desktop Windows, màn hình kép |
| Kết nối | WiFi văn phòng tốc độ cao |

### Mục tiêu
- Đảm bảo nền tảng tuân thủ pháp luật Nhật Bản (APPI)
- Xử lý nhanh các báo cáo vi phạm để bảo vệ cộng đồng
- Quản lý tài chính minh bạch (doanh thu, rút tiền host)

### Pain Points
- Khối lượng báo cáo vi phạm lớn vào giờ cao điểm
- Cần can thiệp nhanh khi có vi phạm nghiêm trọng trong livestream đang diễn ra
- Phải cân bằng giữa bảo vệ cộng đồng và không ảnh hưởng đến trải nghiệm người dùng hợp lệ

### Hành vi sử dụng app
- Làm việc theo ca (sáng/chiều/tối) để cover 24/7
- Ưu tiên xử lý báo cáo theo mức độ nghiêm trọng
- Dùng dashboard để monitor livestream đang diễn ra real-time
- Xử lý yêu cầu rút tiền của host trong vòng 3-5 ngày làm việc

### Thói quen
- Kiểm tra AI moderation alerts đầu tiên khi bắt đầu ca
- Ghi chú lý do khi thực hiện hành động khóa tài khoản để audit trail
- Báo cáo tổng hợp cuối tuần cho management

---

## Mapping: Personas ↔ Features

| Feature | Tanaka (Viewer) | Yamamoto (Host) | Suzuki (Admin) |
|---|---|---|---|
| FR-01: Auth | ✅ | ✅ | ✅ |
| FR-02: Profile | ✅ | ✅ | — |
| FR-03: Matching | ✅ | ✅ | — |
| FR-04: Public Livestream | ✅ (xem) | ✅ (phát) | ✅ (monitor) |
| FR-05: Private Call | ✅ (gọi) | ✅ (nhận) | — |
| FR-06: Chat | ✅ | ✅ | — |
| FR-07: Coin & Payment | ✅ (nạp/tiêu) | ✅ (nhận/rút) | ✅ (quản lý) |
| FR-08: Notifications | ✅ | ✅ | — |
| FR-09: Moderation | ✅ (báo cáo) | ✅ (kick viewer) | ✅ (xử lý) |
| FR-10: Admin Dashboard | — | — | ✅ |
| FR-11: Leaderboard | ✅ (xem) | ✅ (theo dõi rank) | ✅ (quản lý) |
