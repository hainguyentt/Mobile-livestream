# Functional Design Plan — Unit 2: Livestream Engine

**Status**: COMPLETED  
**Created**: 2026-03-22  
**Unit**: Unit 2 — Livestream Engine

---

## Stories Covered

| Story ID | Tên Story | Priority |
|---|---|---|
| US-04-01 | Host bắt đầu livestream public | Must Have |
| US-04-02 | Viewer tham gia phòng livestream | Must Have |
| US-04-03 | Chat real-time trong phòng livestream | Must Have |
| US-04-05 | Host kick viewer vi phạm | Should Have |
| US-05-01 | Viewer gửi yêu cầu private call | Must Have |
| US-05-02 | Host chấp nhận/từ chối private call | Must Have |
| US-05-03 | Video call 1-1 và tính phí theo phút | Must Have |
| US-06-01 | Chat text 1-1 | Must Have |
| US-06-02 | Chặn người dùng | Must Have |

---

## Execution Steps

- [x] Step 1: Phân tích Unit 2 context từ unit-of-work.md và story map
- [x] Step 2: Tạo câu hỏi làm rõ Functional Design
- [x] Step 3: Thu thập câu trả lời từ user
- [x] Step 4: Generate domain entities (domain-entities.md)
- [x] Step 5: Generate business logic model (business-logic-model.md)
- [x] Step 6: Generate business rules (business-rules.md)
- [x] Step 7: Generate frontend components (frontend-components.md)
- [ ] Step 8: Present completion message và chờ approval

---

## Clarifying Questions

### A. LivestreamModule — Public Stream

**Q-A1**: Khi Host bắt đầu stream, thông tin nào là bắt buộc?

- A. Chỉ cần title (tên phòng)
- B. Title + category/tag (ví dụ: "Trò chuyện", "Âm nhạc", "Game")
- C. Title + thumbnail (ảnh đại diện phòng)
- D. Title + category + thumbnail

[Answer]: B

---

**Q-A2**: Một Host có thể có bao nhiêu stream đang active cùng lúc?

- A. Chỉ 1 stream tại một thời điểm (hard limit)
- B. Tối đa 2 (1 public + 1 private call)
- C. Không giới hạn

[Answer]: A

---

**Q-A3**: Viewer join phòng — có cần xác thực gì không?

- A. Chỉ cần đăng nhập (authenticated user)
- B. Đăng nhập + đủ tuổi (18+, check DateOfBirth)
- C. Đăng nhập + không bị block bởi Host
- D. Đăng nhập + đủ tuổi + không bị block bởi Host

[Answer]: D

---

**Q-A4**: Khi Host end stream, điều gì xảy ra với viewers đang xem?

- A. Tất cả viewers nhận SignalR event `StreamEnded` và bị disconnect khỏi room
- B. Viewers nhận event, có 30 giây để xem replay trước khi disconnect
- C. Room chuyển sang trạng thái "Ended" — viewers vẫn ở trong room nhưng không có video

[Answer]: B

---

**Q-A5**: Viewer count — cách tính và hiển thị?

- A. Real-time count từ SignalR connection (số connections đang active trong room group)
- B. Cached count cập nhật mỗi 5 giây (tránh thundering herd)
- C. Persistent count lưu DB (ViewerSessions table, tính unique viewers)

[Answer]: B

---

**Q-A6**: Room có giới hạn số lượng viewers không?

- A. Không giới hạn (Agora Free Tier hỗ trợ tối đa 1M concurrent)
- B. Giới hạn 1000 viewers per room (MVP)
- C. Giới hạn theo plan của Host (Free: 100, Premium: unlimited)

[Answer]: B

---

### B. PrivateCallModule — Private Call 1-1

**Q-B1**: Viewer gửi call request — Host có thể nhận bao nhiêu pending requests cùng lúc?

- A. Chỉ 1 request tại một thời điểm (reject auto nếu đang có pending)
- B. Queue tối đa 5 requests, Host xử lý theo thứ tự
- C. Không giới hạn pending requests

[Answer]: A

---

**Q-B2**: Timeout cho call request — nếu Host không phản hồi?

- A. 30 giây → auto reject
- B. 60 giây → auto reject
- C. 120 giây → auto reject
- D. Không có timeout (request tồn tại cho đến khi Host xử lý hoặc Viewer cancel)

[Answer]: A

---

**Q-B3**: Billing model cho private call — tính phí như thế nào?

- A. Tính phí mỗi 10 giây (billing tick), trừ coin từ Viewer
- B. Tính phí mỗi 1 phút, trừ coin từ Viewer
- C. Tính phí mỗi 10 giây, trừ coin từ Viewer, Host nhận 70% (30% platform fee)
- D. Tính phí mỗi 1 phút, trừ coin từ Viewer, Host nhận 70%

[Answer]: A

---

**Q-B4**: Coin warning — khi nào hiển thị cảnh báo?

- A. Khi còn đủ coin cho 5 phút nữa
- B. Khi còn đủ coin cho 3 phút nữa
- C. Khi còn đủ coin cho 2 phút nữa
- D. Khi coin balance < 100 coins (fixed threshold)

[Answer]: D

---

**Q-B5**: Khi Viewer hết coin trong khi đang call?

- A. Auto-end call ngay lập tức
- B. Cảnh báo → 30 giây grace period → auto-end
- C. Cảnh báo → Host được thông báo → Host quyết định tiếp tục hay end

[Answer]: A

---

**Q-B6**: Trong khi Host đang có private call, Host có thể tiếp tục public stream không?

- A. Có — public stream vẫn chạy, Host chỉ tạm thời "away" (video/audio tắt với viewers)
- B. Không — public stream tự động pause khi Host accept private call
- C. Host phải chọn: end public stream hoặc từ chối private call

[Answer]: A

---

### C. RoomChatModule — Chat trong phòng livestream

**Q-C1**: Content filter cho room chat — filter những gì?

- A. Chỉ filter profanity (danh sách từ cấm cứng)
- B. Filter profanity + spam detection (gửi quá nhiều tin nhắn trong thời gian ngắn)
- C. Filter profanity + spam + URL/link (không cho phép gửi link)
- D. Không filter (moderation sau)

[Answer]: A

---

**Q-C2**: Rate limit cho room chat?

- A. Tối đa 3 tin nhắn/giây per user
- B. Tối đa 1 tin nhắn/giây per user
- C. Tối đa 5 tin nhắn/10 giây per user
- D. Không rate limit

[Answer]: A

---

**Q-C3**: Khi viewer bị kick khỏi phòng, họ có thể rejoin không?

- A. Không — bị ban khỏi phòng đó vĩnh viễn (cho đến khi Host unban)
- B. Không — bị ban cho đến khi stream kết thúc
- C. Có thể rejoin sau 10 phút
- D. Có thể rejoin ngay lập tức (kick chỉ là cảnh báo)

[Answer]: B

---

**Q-C4**: Room chat history — viewer mới join thấy bao nhiêu tin nhắn cũ?

- A. 50 tin nhắn gần nhất
- B. 100 tin nhắn gần nhất
- C. Không có history — chỉ thấy tin nhắn từ khi join

[Answer]: C

---

### D. DirectChatModule — Chat 1-1

**Q-D1**: Ai có thể bắt đầu conversation 1-1?

- A. Bất kỳ user nào (Viewer hoặc Host) có thể nhắn tin cho bất kỳ user nào
- B. Chỉ Viewer có thể nhắn tin cho Host (không phải ngược lại)
- C. Chỉ có thể nhắn tin nếu đã follow nhau (mutual follow)
- D. Bất kỳ user nào, nhưng recipient có thể cài đặt "chỉ nhận tin từ người follow"

[Answer]: B

---

**Q-D2**: Message types được hỗ trợ trong direct chat?

- A. Chỉ text
- B. Text + emoji reactions
- C. Text + image (upload ảnh)
- D. Text + emoji + image

[Answer]: B

---

**Q-D3**: Khi user A block user B — điều gì xảy ra với conversation hiện có?

- A. Conversation bị ẩn với cả 2 phía, không thể gửi tin nhắn mới
- B. Conversation bị ẩn chỉ với user A, user B vẫn thấy nhưng không gửi được
- C. Conversation vẫn hiển thị nhưng cả 2 không thể gửi tin nhắn mới
- D. Conversation bị xóa hoàn toàn

[Answer]: A

---

**Q-D4**: Notification cho direct message — khi nào gửi push notification?

- A. Mỗi tin nhắn mới (real-time)
- B. Chỉ khi user offline (không có active SignalR connection)
- C. Không gửi push notification cho direct chat (chỉ in-app)

[Answer]: A

---

### E. Frontend PWA — Livestream Screens

**Q-E1**: Discovery page (danh sách rooms) — layout và sorting?

- A. Grid layout, sort by viewer count (nhiều nhất lên đầu)
- B. Grid layout, sort by algorithm (personalized recommendations)
- C. List layout, sort by newest first
- D. Grid layout, tabs: "Đang live" (by viewer count) + "Gợi ý" (algorithm)

[Answer]: A

---

**Q-E2**: Video player trong phòng livestream — controls nào hiển thị?

- A. Chỉ volume control + fullscreen
- B. Volume + fullscreen + quality selector
- C. Volume + fullscreen + chat toggle (ẩn/hiện chat overlay)
- D. Volume + fullscreen + quality + chat toggle + gift button

[Answer]: D

---

**Q-E3**: Private call UI — flow từ phía Viewer?

- A. Nút "Gọi riêng" trên profile Host → Confirm dialog → Waiting screen → Call screen
- B. Nút "Gọi riêng" trong phòng livestream → Confirm dialog → Waiting screen → Call screen
- C. Cả A và B (có thể gọi từ cả 2 nơi)

[Answer]: A

---

**Q-E4**: Billing ticker UI — hiển thị như thế nào trong call screen?

- A. Hiển thị coin balance còn lại (cập nhật real-time)
- B. Hiển thị coin đã tiêu trong call này + balance còn lại
- C. Hiển thị thời gian call + coin rate (X coins/phút) + balance còn lại
- D. Chỉ hiển thị cảnh báo khi sắp hết coin

[Answer]: D
Hiển thị coin balance còn lại (cập nhật real-time) khi sắp hết coin

---

**Q-E5**: Direct chat UI — nằm ở đâu trong app?

- A. Tab riêng trong bottom navigation (icon chat)
- B. Accessible từ profile của user (nút "Nhắn tin")
- C. Cả A và B

[Answer]: C

---
