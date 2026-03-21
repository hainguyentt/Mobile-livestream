# Câu Hỏi Làm Rõ Yêu Cầu
# Dự Án: App Livestream Hẹn Hò - Thị Trường Nhật Bản

Vui lòng trả lời các câu hỏi dưới đây bằng cách điền chữ cái vào thẻ `[Answer]:`.
Nếu không có lựa chọn nào phù hợp, hãy chọn "Other" và mô tả thêm.

---

## PHẦN 1: PHẠM VI & MÔ HÌNH KINH DOANH

## Câu hỏi 1
Mô hình kinh doanh chính của ứng dụng là gì?

A) Freemium - miễn phí cơ bản, trả phí cho tính năng nâng cao (coins/gems để gửi quà, super like, v.v.)
B) Subscription - đăng ký hàng tháng/năm để dùng đầy đủ tính năng
C) Pay-per-use - trả tiền theo từng phiên livestream hoặc tính năng sử dụng
D) Kết hợp Freemium + Subscription
E) Other (please describe after [Answer]: tag below)

[Answer]: C

---

## Câu hỏi 2
Đối tượng người dùng mục tiêu tại Nhật Bản là ai?

A) Người độc thân 20-35 tuổi, tìm kiếm mối quan hệ nghiêm túc
B) Người độc thân 18-40 tuổi, tìm kiếm cả mối quan hệ nghiêm túc lẫn giao lưu bạn bè
C) Chủ yếu là người dùng trẻ 18-25 tuổi, thiên về giải trí và giao lưu
D) Người trưởng thành 25-45 tuổi, tìm kiếm mối quan hệ lâu dài
E) Other (please describe after [Answer]: tag below)

[Answer]: E
 - Người trưởng thành nam giới (có thể lên đến ~70 tuổi), thiên về giải trí và giao lưu

---

## Câu hỏi 3
Tính năng livestream trong app sẽ hoạt động theo mô hình nào?

A) 1-1 private livestream - chỉ 2 người kết nối riêng tư với nhau
B) 1-nhiều public livestream - một người phát, nhiều người xem và tương tác
C) Cả hai: vừa có 1-1 private vừa có public livestream
D) Group livestream - nhiều người cùng tham gia một phòng video
E) Other (please describe after [Answer]: tag below)

[Answer]: C

---

## Câu hỏi 4
Nền tảng triển khai ưu tiên là gì?

A) Web app (React/Next.js) + iOS + Android (3 nền tảng đầy đủ)
B) Web app + React Native (dùng chung codebase mobile)
C) Chỉ Mobile (iOS + Android) trước, web sau
D) Progressive Web App (PWA) - một codebase cho cả web và mobile
E) Other (please describe after [Answer]: tag below)

[Answer]: D

---

## PHẦN 2: TÍNH NĂNG CỐT LÕI

## Câu hỏi 5
Tính năng matching (ghép đôi) sẽ hoạt động như thế nào?

A) Swipe-based (kiểu Tinder) - vuốt trái/phải để like hoặc bỏ qua
B) Algorithm-based - hệ thống tự gợi ý dựa trên sở thích và hành vi
C) Search & Filter - người dùng tự tìm kiếm theo tiêu chí
D) Kết hợp: Swipe + Algorithm recommendation
E) Other (please describe after [Answer]: tag below)

[Answer]: E
 - Kết hợp Algorithm-based + Search & Filter

---

## Câu hỏi 6
Tính năng nào là BẮT BUỘC phải có trong phiên bản đầu tiên (MVP)?

A) Livestream 1-1, matching cơ bản, chat text, hồ sơ người dùng
B) Livestream 1-1, matching, chat text/voice, quà ảo (virtual gifts), hồ sơ người dùng
C) Tất cả tính năng trên + public livestream + hệ thống coins + leaderboard
D) Chỉ cần: đăng ký/đăng nhập, hồ sơ, matching, chat, livestream 1-1 cơ bản
E) Other (please describe after [Answer]: tag below)

[Answer]: E
- Livestream private 1-1 và public 1-N (N <= 50), matching cơ bản, chat text, hồ sơ người dùng
- Nạp tiền mua coin, quà ảo

---

## Câu hỏi 7
Hệ thống xác minh danh tính người dùng sẽ như thế nào?

A) Xác minh số điện thoại (SMS OTP) là đủ
B) Xác minh số điện thoại + email
C) Xác minh số điện thoại + ảnh selfie (face verification) để chống giả mạo
D) Xác minh đầy đủ: số điện thoại + email + ID card/My Number Card (thẻ căn cước Nhật)
E) Other (please describe after [Answer]: tag below)

[Answer]: E
 - Xác minh Email là đủ

---

## PHẦN 3: YÊU CẦU KỸ THUẬT & HẠ TẦNG

## Câu hỏi 8
Hạ tầng cloud ưu tiên là gì?

A) AWS (Amazon Web Services)
B) Google Cloud Platform (GCP)
C) Microsoft Azure
D) Chưa quyết định, để AI-DLC đề xuất phù hợp nhất
E) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Câu hỏi 9
Công nghệ livestream/video sẽ sử dụng giải pháp nào?

A) Tự xây dựng với WebRTC (open source, kiểm soát hoàn toàn)
B) Dùng dịch vụ bên thứ 3: Agora.io (phổ biến ở châu Á)
C) Dùng dịch vụ bên thứ 3: Amazon IVS hoặc AWS Chime
D) Dùng dịch vụ bên thứ 3: Twilio Video
E) Other (please describe after [Answer]: tag below)

[Answer]: B

---

## Câu hỏi 10
Quy mô người dùng dự kiến khi ra mắt và sau 1 năm là bao nhiêu?

A) Nhỏ: 0-10,000 người dùng (MVP/beta testing)
B) Vừa: 10,000 - 100,000 người dùng trong năm đầu
C) Lớn: 100,000 - 1,000,000 người dùng trong năm đầu
D) Rất lớn: 1,000,000+ người dùng, cần kiến trúc scale lớn ngay từ đầu
E) Other (please describe after [Answer]: tag below)

[Answer]: B

---

## PHẦN 4: YÊU CẦU ĐẶC THÙ THỊ TRƯỜNG NHẬT BẢN

## Câu hỏi 11
Yêu cầu tuân thủ pháp lý tại Nhật Bản nào cần ưu tiên?

A) Chỉ cần tuân thủ Act on Protection of Personal Information (APPI) - luật bảo vệ dữ liệu cá nhân
B) APPI + Specified Commercial Transactions Act (đăng ký dịch vụ thương mại điện tử)
C) APPI + Act on Prevention of Dating Violence (luật phòng chống bạo lực hẹn hò) + age verification
D) Tuân thủ đầy đủ tất cả: APPI + thương mại điện tử + age verification + nội dung phù hợp
E) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## Câu hỏi 12
Ngôn ngữ giao diện ứng dụng sẽ hỗ trợ những ngôn ngữ nào?

A) Chỉ tiếng Nhật (日本語)
B) Tiếng Nhật + Tiếng Anh
C) Tiếng Nhật + Tiếng Anh + Tiếng Việt (cho team phát triển)
D) Đa ngôn ngữ đầy đủ (Nhật, Anh, Hàn, Trung, v.v.)
E) Other (please describe after [Answer]: tag below)

[Answer]: B

---

## Câu hỏi 13
Phương thức thanh toán nào cần hỗ trợ cho thị trường Nhật Bản?

A) Chỉ thẻ tín dụng/debit quốc tế (Visa, Mastercard)
B) Thẻ tín dụng + Apple Pay/Google Pay
C) Thẻ tín dụng + Convenience store payment (コンビニ払い - thanh toán tại cửa hàng tiện lợi)
D) Đầy đủ: Thẻ tín dụng + Apple/Google Pay + Convenience store + PayPay/LINE Pay
E) Other (please describe after [Answer]: tag below)

[Answer]: E
 - Dự định: Payment gateway (stripe), LINE Pay 
 - Hãy phân tích dự định để tôi quyết định tôi

---

## PHẦN 5: AN TOÀN & BẢO MẬT

## Câu hỏi 14
Mức độ kiểm duyệt nội dung (content moderation) cần thiết là gì?

A) Tự động: AI/ML filter nội dung không phù hợp trong livestream và chat
B) Thủ công: Đội ngũ moderator người thật review nội dung bị báo cáo
C) Kết hợp: AI filter tự động + moderator xử lý các trường hợp phức tạp
D) Hệ thống đầy đủ: AI filter + moderator + hệ thống trust score người dùng
E) Other (please describe after [Answer]: tag below)

[Answer]: C

---

## Câu hỏi 15
Xác minh độ tuổi (age verification) sẽ được thực hiện như thế nào?

A) Tự khai báo ngày sinh (không xác minh)
B) Xác minh qua số điện thoại (18+ mới có SIM tại Nhật)
C) Xác minh qua thẻ tín dụng (chỉ người 18+ mới có)
D) Xác minh ID chính thức (My Number Card, bằng lái xe, hộ chiếu)
E) Other (please describe after [Answer]: tag below)

[Answer]: B

---

## Câu hỏi: Security Extensions
Có nên áp dụng các quy tắc bảo mật (Security Extension Rules) như là ràng buộc bắt buộc cho dự án này không?

A) Yes — áp dụng tất cả SECURITY rules như ràng buộc bắt buộc (khuyến nghị cho ứng dụng production)
B) No — bỏ qua SECURITY rules (phù hợp cho PoC, prototype, thử nghiệm)
X) Other (please describe after [Answer]: tag below)

[Answer]: A

---

*Vui lòng điền tất cả các câu trả lời và thông báo khi hoàn thành.*
