# Record of Changes - Requirements Document

---

| Version | Ngày thay đổi | Loại thay đổi | Section | Tóm tắt thay đổi |
|---|---|---|---|---|
| 1.0 | 2026-03-21 | 🆕 Tạo mới | Tất cả | Tạo mới tài liệu yêu cầu ban đầu từ kết quả phân tích 15 câu hỏi |
| 1.1 | 2026-03-21 | ✏️ Chỉnh sửa | FR-01 | FR-01-3: Đổi LINE Login từ Should Have → Must Have; tách Google/Apple thành FR-01-3b (Should Have) |
| 1.1 | 2026-03-21 | ➕ Bổ sung | FR-11 (mới) | Thêm mới section FR-11: Leaderboard & Ranking cơ bản cho host (daily/weekly/monthly, top gifters, rank badge) |
| 1.1 | 2026-03-21 | ➕ Bổ sung | FR-10 | Thêm FR-10-6: Admin/Moderator có thể remove (kick) người xem vi phạm chính sách khỏi phòng livestream |
| 1.1 | 2026-03-21 | ✅ Quyết định | Section 5.2 | Xác nhận quyết định Payment: Stripe (primary) + LINE Pay (secondary) — không còn là đề xuất |
| 1.1 | 2026-03-21 | 🔄 Thay thế | Section 6.2 | Đổi backend từ Node.js/NestJS sang .NET 8 / ASP.NET Core + EF Core 8 + Hangfire |
| 1.1 | 2026-03-21 | 🔄 Thay thế | Section 6.1 | Cập nhật frontend real-time client từ Socket.io → SignalR client (`@microsoft/signalr`) |
| 1.1 | 2026-03-21 | ➕ Bổ sung | Section 6.6 (mới) | Thêm phân tích so sánh SignalR vs Socket.io; quyết định dùng ASP.NET Core SignalR |
| 1.1 | 2026-03-21 | ✏️ Chỉnh sửa | Section 8 | Cập nhật Out of Scope: Leaderboard nâng cao (Phase 2 mở rộng từ FR-11 MVP) |
| 1.2 | 2026-03-21 | ✏️ Chỉnh sửa | FR-07 | FR-07-6: Đổi ưu tiên Host rút tiền từ Must Have → Could Have, không nằm trong scope MVP |
| 1.3 | 2026-03-21 | ➕ Bổ sung | NFR-03 | Thêm NFR-03-4: Chat message retention policy — room chat dùng Redis Streams (TTL 7 ngày, không persist PostgreSQL); direct chat dùng PostgreSQL partitioning theo tháng, retention 12 tháng |
| 1.3 | 2026-03-21 | 🔄 Thay thế | Section 6.3 | Cập nhật Database strategy: Chat module tách thành 2 storage — Redis Streams cho room chat, PostgreSQL (partitioned) cho direct chat |
| 1.3 | 2026-03-21 | 🔄 Thay thế | Application Design | Tách MOD-05 LivestreamApp.Chat thành 2 modules riêng: MOD-05 LivestreamApp.RoomChat (Redis Streams) + MOD-06 LivestreamApp.DirectChat (PostgreSQL partitioned). Tổng: 12 backend modules. ChatHub giữ chung tại API layer. IChatMessageFilter đưa vào Shared. ||

---

**Chú thích loại thay đổi:**

| Ký hiệu | Loại | Mô tả |
|---|---|---|
| 🆕 Tạo mới | New | Tạo tài liệu hoặc section hoàn toàn mới |
| ➕ Bổ sung | Addition | Thêm yêu cầu, tính năng, hoặc nội dung mới vào section đã có |
| ✏️ Chỉnh sửa | Update | Sửa đổi nội dung đã có (ưu tiên, mô tả, phạm vi) |
| 🔄 Thay thế | Replace | Thay thế hoàn toàn một quyết định hoặc công nghệ bằng lựa chọn khác |
| ✅ Quyết định | Decision | Chốt một lựa chọn đang ở trạng thái đề xuất/chờ xác nhận |
| 🗑️ Xóa bỏ | Removal | Loại bỏ yêu cầu hoặc nội dung khỏi phạm vi |
