# User Stories
# Dự Án: App Livestream Hẹn Hò - Thị Trường Nhật Bản

**Tổ chức**: Feature-Based  
**Phạm vi**: MVP (Must Have) + Should Have  
**Ngôn ngữ**: Tiếng Việt mô tả, tiếng Anh cho technical terms & Acceptance Criteria  
**Acceptance Criteria**: Mức trung bình (4-6 bullet, happy path + edge cases)

---

## EPIC 1: Authentication & Profile (FR-01, FR-02)

---

### US-01-01: Đăng ký tài khoản bằng email
**Là** người dùng mới,  
**Tôi muốn** đăng ký tài khoản bằng email,  
**Để** có thể truy cập ứng dụng.

**Acceptance Criteria:**
- Given người dùng nhập email hợp lệ và password (≥8 ký tự), When nhấn "Đăng ký", Then hệ thống gửi OTP 6 số về email trong vòng 60 giây
- Given người dùng nhập đúng OTP, When xác nhận, Then tài khoản được tạo và chuyển đến màn hình tạo hồ sơ
- Given email đã tồn tại trong hệ thống, When nhấn "Đăng ký", Then hiển thị lỗi "Email này đã được đăng ký"
- Given OTP hết hạn (sau 10 phút), When người dùng nhập OTP cũ, Then hiển thị lỗi và cho phép gửi lại OTP
- Given người dùng nhập password yếu (<8 ký tự), Then hiển thị validation error ngay lập tức

**Priority**: Must Have | **Persona**: Tanaka, Yamamoto

---

### US-01-02: Đăng nhập bằng email/password
**Là** người dùng đã có tài khoản,  
**Tôi muốn** đăng nhập bằng email và password,  
**Để** truy cập vào ứng dụng.

**Acceptance Criteria:**
- Given thông tin đăng nhập đúng, When nhấn "Đăng nhập", Then nhận JWT access token + refresh token, chuyển đến màn hình chính
- Given sai password, When nhấn "Đăng nhập", Then hiển thị lỗi chung "Email hoặc mật khẩu không đúng" (không tiết lộ field nào sai)
- Given đăng nhập sai 5 lần liên tiếp, Then tài khoản bị tạm khóa 15 phút và hiển thị thông báo
- Given JWT hết hạn, When thực hiện request, Then hệ thống tự động dùng refresh token để lấy token mới (silent refresh)

**Priority**: Must Have | **Persona**: Tanaka, Yamamoto, Suzuki

---

### US-01-03: Đăng nhập bằng LINE Login
**Là** người dùng Nhật Bản đang dùng LINE,  
**Tôi muốn** đăng nhập bằng tài khoản LINE của mình,  
**Để** không cần nhớ thêm email/password.

**Acceptance Criteria:**
- Given người dùng nhấn "Đăng nhập với LINE", When xác thực LINE OAuth thành công, Then tài khoản được tạo/liên kết và đăng nhập vào app
- Given người dùng LINE chưa có tài khoản app, When đăng nhập LINE lần đầu, Then chuyển đến màn hình tạo hồ sơ
- Given người dùng từ chối cấp quyền trên LINE, Then quay lại màn hình đăng nhập với thông báo phù hợp
- Given LINE OAuth thất bại (lỗi mạng), Then hiển thị thông báo lỗi và cho phép thử lại

**Priority**: Must Have | **Persona**: Tanaka, Yamamoto

---

### US-01-04: Xác minh số điện thoại
**Là** người dùng muốn dùng tính năng livestream và thanh toán,  
**Tôi muốn** xác minh số điện thoại của mình,  
**Để** mở khóa các tính năng nâng cao và xác nhận độ tuổi 18+.

**Acceptance Criteria:**
- Given người dùng nhập số điện thoại Nhật Bản hợp lệ (+81), When nhấn "Gửi OTP", Then nhận SMS OTP trong vòng 60 giây
- Given nhập đúng OTP, Then số điện thoại được xác minh, tính năng livestream và thanh toán được mở khóa
- Given số điện thoại đã được xác minh bởi tài khoản khác, Then hiển thị lỗi "Số điện thoại này đã được sử dụng"
- Given OTP sai 3 lần, Then yêu cầu chờ 5 phút trước khi thử lại

**Priority**: Must Have | **Persona**: Tanaka, Yamamoto

---

### US-01-05: Đặt lại mật khẩu
**Là** người dùng quên mật khẩu,  
**Tôi muốn** đặt lại mật khẩu qua email,  
**Để** lấy lại quyền truy cập tài khoản.

**Acceptance Criteria:**
- Given nhập email đã đăng ký, When nhấn "Gửi link đặt lại", Then nhận email với link reset (hết hạn sau 30 phút)
- Given nhấn link hợp lệ, Then chuyển đến form nhập mật khẩu mới (≥8 ký tự)
- Given link đã hết hạn hoặc đã dùng, Then hiển thị thông báo và cho phép yêu cầu link mới
- Given email không tồn tại, Then vẫn hiển thị thông báo "Nếu email tồn tại, bạn sẽ nhận được hướng dẫn" (tránh user enumeration)

**Priority**: Must Have | **Persona**: Tanaka, Yamamoto

---

### US-02-01: Tạo và chỉnh sửa hồ sơ
**Là** người dùng mới,  
**Tôi muốn** tạo hồ sơ cá nhân với ảnh và thông tin giới thiệu,  
**Để** người dùng khác có thể biết về tôi.

**Acceptance Criteria:**
- Given người dùng nhập tên hiển thị (2-20 ký tự), ngày sinh, giới thiệu (≤200 ký tự), When lưu, Then hồ sơ được tạo thành công
- Given upload ảnh đại diện (JPG/PNG, ≤5MB), Then ảnh được resize và lưu trên S3, hiển thị ngay
- Given người dùng upload tối đa 6 ảnh hồ sơ, Then tất cả hiển thị theo thứ tự có thể kéo thả để sắp xếp
- Given người dùng chưa điền đủ thông tin bắt buộc (tên, ngày sinh, ít nhất 1 ảnh), Then không thể truy cập tính năng matching và livestream
- Given chỉnh sửa hồ sơ, When lưu, Then thay đổi hiển thị ngay lập tức

**Priority**: Must Have | **Persona**: Tanaka, Yamamoto

---

### US-02-02: Huy hiệu xác minh cho Host
**Là** host đã xác minh danh tính,  
**Tôi muốn** có huy hiệu "Verified" trên hồ sơ,  
**Để** tăng độ tin cậy với viewer.

**Acceptance Criteria:**
- Given host đã xác minh số điện thoại và email, Then huy hiệu verified badge hiển thị trên hồ sơ và trong phòng livestream
- Given viewer xem hồ sơ host, Then có thể thấy rõ trạng thái verified
- Given admin thu hồi verified status, Then badge biến mất ngay lập tức

**Priority**: Should Have | **Persona**: Yamamoto

---

## EPIC 2: Matching & Discovery (FR-03)

---

### US-03-01: Xem gợi ý người dùng từ thuật toán
**Là** viewer,  
**Tôi muốn** xem danh sách host được gợi ý phù hợp với sở thích của tôi,  
**Để** dễ dàng tìm được người muốn kết nối.

**Acceptance Criteria:**
- Given người dùng đã hoàn thiện hồ sơ và có lịch sử tương tác, When mở màn hình Discovery, Then hiển thị danh sách host được sắp xếp theo relevance score
- Given người dùng mới chưa có lịch sử, Then hiển thị host theo popularity (số lượng follower, rating)
- Given host đang online/livestream, Then được ưu tiên hiển thị đầu danh sách với badge "LIVE"
- Given cuộn đến cuối danh sách, Then tự động load thêm (infinite scroll, page size 20)

**Priority**: Must Have | **Persona**: Tanaka

---

### US-03-02: Tìm kiếm và lọc host
**Là** viewer,  
**Tôi muốn** tìm kiếm host theo tên và lọc theo tiêu chí,  
**Để** tìm đúng người tôi muốn xem.

**Acceptance Criteria:**
- Given nhập từ khóa vào search bar, Then hiển thị kết quả real-time (debounce 300ms) theo tên hiển thị
- Given áp dụng filter (độ tuổi, sở thích, trạng thái online, có đang livestream), Then danh sách cập nhật ngay
- Given không có kết quả phù hợp, Then hiển thị thông báo "Không tìm thấy kết quả" và gợi ý mở rộng filter
- Given xóa tất cả filter, Then quay về danh sách gợi ý mặc định

**Priority**: Must Have | **Persona**: Tanaka

---

### US-03-03: Like và Follow host
**Là** viewer,  
**Tôi muốn** like và follow host yêu thích,  
**Để** nhận thông báo khi họ livestream và dễ tìm lại.

**Acceptance Criteria:**
- Given nhấn nút Follow trên hồ sơ host, Then host xuất hiện trong danh sách "Đang follow" và viewer nhận thông báo khi host livestream
- Given nhấn Like trên hồ sơ, Then số like tăng lên và hiển thị trên hồ sơ host
- Given nhấn Unfollow, Then host bị xóa khỏi danh sách following và không nhận thông báo nữa
- Given host nhận được like/follow, Then nhận push notification

**Priority**: Must Have | **Persona**: Tanaka, Yamamoto

---

## EPIC 3: Livestream Public (FR-04)

---

### US-04-01: Host bắt đầu livestream public
**Là** host,  
**Tôi muốn** bắt đầu phiên livestream public,  
**Để** giao lưu với nhiều viewer cùng lúc và nhận quà ảo.

**Acceptance Criteria:**
- Given host đã xác minh số điện thoại, When nhấn "Bắt đầu Livestream", Then phòng stream được tạo, Agora.io channel khởi tạo, và phòng hiển thị trong danh sách Discovery
- Given host cài đặt phòng miễn phí hoặc tính phí theo phút, Then setting được lưu và áp dụng cho toàn bộ phiên
- Given host nhấn "Kết thúc Livestream", Then phòng đóng, tất cả viewer bị disconnect, thống kê phiên được lưu
- Given kết nối mạng của host bị ngắt >30 giây, Then phòng tự động đóng và viewer nhận thông báo

**Priority**: Must Have | **Persona**: Yamamoto

---

### US-04-02: Viewer tham gia phòng livestream
**Là** viewer,  
**Tôi muốn** tham gia phòng livestream của host,  
**Để** xem và tương tác với host.

**Acceptance Criteria:**
- Given phòng livestream miễn phí, When viewer nhấn "Tham gia", Then kết nối Agora.io và xem stream ngay
- Given phòng tính phí theo phút, When viewer nhấn "Tham gia", Then hiển thị thông báo giá/phút và số coin hiện có, yêu cầu xác nhận trước khi vào
- Given phòng đã đủ 50 viewer, Then hiển thị thông báo "Phòng đã đầy" và không cho vào
- Given viewer không đủ coin để vào phòng tính phí, Then hiển thị thông báo và link đến trang nạp coin

**Priority**: Must Have | **Persona**: Tanaka

---

### US-04-03: Chat real-time trong phòng livestream
**Là** viewer trong phòng livestream,  
**Tôi muốn** gửi tin nhắn chat trong phòng,  
**Để** tương tác với host và các viewer khác.

**Acceptance Criteria:**
- Given viewer nhập tin nhắn (≤100 ký tự) và gửi, Then tin nhắn hiển thị cho tất cả người trong phòng qua SignalR trong <500ms
- Given tin nhắn chứa từ ngữ vi phạm (theo blacklist), Then bị filter tự động và không hiển thị, người gửi nhận cảnh báo
- Given host kick một viewer, Then viewer đó không thể gửi chat trong phòng này nữa
- Given phòng có nhiều tin nhắn, Then chat tự động scroll xuống tin mới nhất

**Priority**: Must Have | **Persona**: Tanaka, Yamamoto

---

### US-04-04: Gửi quà ảo trong livestream
**Là** viewer trong phòng livestream,  
**Tôi muốn** gửi quà ảo cho host,  
**Để** thể hiện sự ủng hộ và thu hút sự chú ý của host.

**Acceptance Criteria:**
- Given viewer mở gift panel, Then hiển thị danh sách quà với giá coin tương ứng và số dư coin hiện tại
- Given viewer chọn quà và xác nhận, Then coin bị trừ ngay lập tức, animation quà hiển thị nổi bật trên màn hình tất cả người trong phòng
- Given viewer không đủ coin, Then nút gửi bị disable và hiển thị gợi ý nạp thêm
- Given gửi quà thành công, Then tên viewer và quà hiển thị trong chat feed, host nhận notification

**Priority**: Must Have | **Persona**: Tanaka

---

### US-04-05: Host kick viewer vi phạm
**Là** host,  
**Tôi muốn** kick viewer có hành vi không phù hợp khỏi phòng stream của mình,  
**Để** duy trì môi trường lành mạnh trong phòng.

**Acceptance Criteria:**
- Given host nhấn vào tên viewer trong danh sách, Then hiển thị menu với tùy chọn "Kick khỏi phòng"
- Given host xác nhận kick, Then viewer bị disconnect khỏi phòng ngay lập tức và không thể tham gia lại phòng đó trong phiên hiện tại
- Given viewer bị kick, Then nhận thông báo "Bạn đã bị xóa khỏi phòng livestream"
- Given host kick, Then hành động được ghi log để admin có thể review nếu cần

**Priority**: Should Have | **Persona**: Yamamoto

---

## EPIC 4: Livestream Private 1-1 (FR-05)

---

### US-05-01: Viewer gửi yêu cầu private call
**Là** viewer,  
**Tôi muốn** gửi yêu cầu private call đến host,  
**Để** có cuộc trò chuyện riêng tư 1-1.

**Acceptance Criteria:**
- Given viewer nhấn "Private Call" trên hồ sơ host, Then hiển thị giá/phút và số coin hiện có, yêu cầu xác nhận
- Given viewer xác nhận, Then yêu cầu được gửi đến host qua SignalR, host nhận push notification
- Given host đang trong private call khác, Then hiển thị thông báo "Host đang bận, vui lòng thử lại sau"
- Given host không phản hồi trong 30 giây, Then yêu cầu tự động hủy và viewer nhận thông báo

**Priority**: Must Have | **Persona**: Tanaka

---

### US-05-02: Host chấp nhận/từ chối private call
**Là** host,  
**Tôi muốn** chấp nhận hoặc từ chối yêu cầu private call,  
**Để** kiểm soát được ai có thể gọi riêng cho mình.

**Acceptance Criteria:**
- Given nhận yêu cầu call, Then hiển thị popup với thông tin viewer (tên, ảnh, số lần đã call trước đây) và 2 nút "Chấp nhận" / "Từ chối"
- Given host chấp nhận, Then cả hai được kết nối vào Agora.io channel riêng, đồng hồ tính phí bắt đầu
- Given host từ chối, Then viewer nhận thông báo "Host đã từ chối cuộc gọi"
- Given host đang livestream public và nhận yêu cầu call, Then có thể chấp nhận (public stream tự động pause) hoặc từ chối

**Priority**: Must Have | **Persona**: Yamamoto

---

### US-05-03: Video call 1-1 và tính phí theo phút
**Là** viewer trong private call,  
**Tôi muốn** thấy thời gian và chi phí real-time trong khi gọi,  
**Để** kiểm soát được số coin tôi đang tiêu.

**Acceptance Criteria:**
- Given call đang diễn ra, Then hiển thị đồng hồ đếm thời gian và số coin đã tiêu real-time (cập nhật mỗi 10 giây)
- Given số coin còn lại đủ cho <2 phút nữa, Then hiển thị cảnh báo "Coin sắp hết" với tùy chọn kết thúc call
- Given coin về 0, Then call tự động kết thúc, cả hai nhận thông báo "Cuộc gọi kết thúc do hết coin"
- Given một trong hai bên nhấn "Kết thúc", Then call dừng ngay, tổng chi phí được tính và hiển thị màn hình tóm tắt
- Given mất kết nối mạng >10 giây, Then call tự động kết thúc, chỉ tính phí đến thời điểm mất kết nối

**Priority**: Must Have | **Persona**: Tanaka, Yamamoto

---

## EPIC 5: Chat & Notifications (FR-06, FR-08)

---

### US-06-01: Chat text 1-1
**Là** viewer đã follow một host,  
**Tôi muốn** gửi tin nhắn riêng cho host,  
**Để** trò chuyện ngoài phòng livestream.

**Acceptance Criteria:**
- Given viewer và host đã follow nhau (mutual follow), When mở chat, Then có thể gửi tin nhắn text
- Given gửi tin nhắn, Then hiển thị ngay trong conversation với trạng thái "Đã gửi", chuyển sang "Đã đọc" khi đối phương xem
- Given người dùng bị chặn, Then không thể gửi tin nhắn và không thấy trạng thái online của người kia
- Given tin nhắn chứa nội dung vi phạm, Then bị filter và người gửi nhận cảnh báo

**Priority**: Must Have | **Persona**: Tanaka, Yamamoto

---

### US-06-02: Chặn người dùng
**Là** người dùng,  
**Tôi muốn** chặn người dùng khác,  
**Để** không nhận tin nhắn và không thấy họ trong app.

**Acceptance Criteria:**
- Given nhấn "Chặn" trên hồ sơ hoặc trong chat, Then người đó bị chặn ngay lập tức
- Given người bị chặn, Then không thể gửi tin nhắn, không thể tham gia phòng livestream của người chặn, không xuất hiện trong gợi ý
- Given bỏ chặn, Then các hạn chế được gỡ bỏ

**Priority**: Must Have | **Persona**: Tanaka, Yamamoto

---

### US-08-01: Push notification khi host bắt đầu livestream
**Là** viewer đang follow một host,  
**Tôi muốn** nhận thông báo khi host bắt đầu livestream,  
**Để** không bỏ lỡ phiên stream của host yêu thích.

**Acceptance Criteria:**
- Given host bắt đầu livestream, Then tất cả follower nhận push notification trong vòng 30 giây
- Given viewer nhấn vào notification, Then mở thẳng vào phòng livestream của host
- Given viewer đã tắt thông báo cho host này, Then không nhận notification
- Given app đang mở, Then hiển thị in-app notification thay vì push notification

**Priority**: Must Have | **Persona**: Tanaka

---

### US-08-02: Push notification cho tin nhắn và tương tác
**Là** người dùng,  
**Tôi muốn** nhận thông báo khi có tin nhắn mới, like, follow, hoặc yêu cầu private call,  
**Để** phản hồi kịp thời.

**Acceptance Criteria:**
- Given nhận tin nhắn mới khi app đóng, Then push notification hiển thị tên người gửi và preview tin nhắn (≤50 ký tự)
- Given nhận yêu cầu private call, Then push notification có âm thanh riêng biệt và hiển thị ngay cả khi màn hình khóa
- Given người dùng tùy chỉnh cài đặt thông báo, Then chỉ nhận các loại thông báo đã bật

**Priority**: Must Have | **Persona**: Tanaka, Yamamoto

---

## EPIC 6: Coin & Payment (FR-07)

---

### US-07-01: Nạp coin qua Stripe
**Là** viewer muốn sử dụng tính năng trả phí,  
**Tôi muốn** nạp coin bằng thẻ tín dụng qua Stripe,  
**Để** có coin để xem livestream, gửi quà và gọi private call.

**Acceptance Criteria:**
- Given người dùng chọn gói coin (500¥/1000¥/3000¥/5000¥), When nhấn "Nạp ngay", Then chuyển đến Stripe Checkout với thông tin gói đã chọn
- Given thanh toán Stripe thành công (webhook xác nhận), Then số coin được cộng vào tài khoản trong vòng 10 giây, hiển thị thông báo thành công
- Given thanh toán thất bại (thẻ bị từ chối, hết hạn), Then hiển thị thông báo lỗi cụ thể từ Stripe và cho phép thử lại hoặc đổi phương thức
- Given thanh toán đang xử lý, Then hiển thị loading state và không cho phép nhấn nút nạp lần nữa (tránh double charge)
- Given nạp thành công, Then giao dịch được ghi vào lịch sử với timestamp, số tiền, số coin nhận được
- Given webhook Stripe bị delay, Then hệ thống retry tối đa 3 lần trước khi đánh dấu cần xử lý thủ công

**Priority**: Must Have | **Persona**: Tanaka

---

### US-07-02: Nạp coin qua LINE Pay
**Là** viewer người Nhật đang dùng LINE Pay,  
**Tôi muốn** nạp coin bằng LINE Pay,  
**Để** thanh toán tiện lợi mà không cần nhập thông tin thẻ.

**Acceptance Criteria:**
- Given người dùng chọn "LINE Pay" và chọn gói coin, When xác nhận, Then redirect đến LINE Pay checkout
- Given thanh toán LINE Pay thành công, Then coin được cộng vào tài khoản trong vòng 10 giây
- Given người dùng hủy thanh toán trên LINE Pay, Then quay về app với thông báo "Thanh toán đã bị hủy"
- Given LINE Pay không khả dụng (maintenance), Then hiển thị thông báo và gợi ý dùng Stripe

**Priority**: Must Have | **Persona**: Tanaka

---

### US-07-03: Xem lịch sử giao dịch coin
**Là** người dùng,  
**Tôi muốn** xem lịch sử tất cả giao dịch coin của mình,  
**Để** theo dõi chi tiêu và thu nhập.

**Acceptance Criteria:**
- Given mở trang lịch sử, Then hiển thị danh sách giao dịch theo thứ tự mới nhất, mỗi dòng gồm: loại (nạp/tiêu/nhận), số coin, mô tả, timestamp
- Given lọc theo loại giao dịch hoặc khoảng thời gian, Then danh sách cập nhật tương ứng
- Given cuộn đến cuối, Then load thêm giao dịch cũ hơn (pagination)

**Priority**: Must Have | **Persona**: Tanaka, Yamamoto

---

### US-07-04: Host rút tiền từ coin nhận được
**Là** host có coin tích lũy từ quà ảo và private call,  
**Tôi muốn** rút tiền về tài khoản ngân hàng Nhật Bản,  
**Để** nhận thu nhập thực tế từ hoạt động streaming.

**Acceptance Criteria:**
- Given host có số dư coin đủ ngưỡng tối thiểu (ví dụ: tương đương 3000¥), When gửi yêu cầu rút tiền, Then yêu cầu được tạo với trạng thái "Đang xử lý"
- Given host nhập thông tin tài khoản ngân hàng Nhật (tên ngân hàng, số tài khoản, tên chủ tài khoản), Then thông tin được lưu mã hóa
- Given admin phê duyệt yêu cầu rút tiền, Then host nhận thông báo và tiền được chuyển trong 3-5 ngày làm việc
- Given yêu cầu bị từ chối (thông tin sai, vi phạm policy), Then host nhận thông báo với lý do cụ thể

**Priority**: Could Have (Out of MVP scope) | **Persona**: Yamamoto

---

## EPIC 7: Leaderboard & Ranking (FR-11)

---

### US-11-01: Viewer xem bảng xếp hạng host
**Là** viewer,  
**Tôi muốn** xem bảng xếp hạng các host nổi tiếng nhất,  
**Để** dễ dàng tìm host chất lượng cao để xem.

**Acceptance Criteria:**
- Given mở trang Leaderboard, Then hiển thị top host theo 3 tab: Daily / Weekly / Monthly, sắp xếp theo tổng coin nhận được
- Given mỗi dòng trong leaderboard, Then hiển thị: hạng, ảnh đại diện, tên host, tổng coin, badge rank (nếu có), trạng thái online/LIVE
- Given nhấn vào host trong leaderboard, Then chuyển đến hồ sơ host
- Given leaderboard được cập nhật mỗi 5 phút, Then dữ liệu không quá cũ

**Priority**: Must Have | **Persona**: Tanaka

---

### US-11-02: Host theo dõi thứ hạng của mình
**Là** host,  
**Tôi muốn** xem thứ hạng hiện tại của mình trong leaderboard,  
**Để** biết mình đang đứng ở đâu so với các host khác và có động lực cải thiện.

**Acceptance Criteria:**
- Given host mở trang Leaderboard, Then thứ hạng của host được highlight và hiển thị ngay cả khi không nằm trong top hiển thị
- Given host có rank badge (Top 10, Top 50, Rising Star), Then badge hiển thị trên hồ sơ và trong phòng livestream
- Given thứ hạng thay đổi đáng kể (lên/xuống ≥10 bậc), Then host nhận in-app notification

**Priority**: Must Have | **Persona**: Yamamoto

---

### US-11-03: Hiển thị top gifters trong phòng livestream
**Là** viewer trong phòng livestream,  
**Tôi muốn** xem danh sách top người tặng quà nhiều nhất trong phiên,  
**Để** biết ai đang ủng hộ host nhiều nhất và có động lực cạnh tranh.

**Acceptance Criteria:**
- Given đang trong phòng livestream, Then hiển thị top 3 gifters của phiên hiện tại với tên và tổng coin đã tặng
- Given gửi quà, Then thứ hạng gifter cập nhật real-time
- Given phiên kết thúc, Then top gifters được lưu vào thống kê phiên

**Priority**: Must Have | **Persona**: Tanaka

---

### US-11-04: Admin quản lý leaderboard
**Là** admin,  
**Tôi muốn** có thể điều chỉnh leaderboard khi cần thiết,  
**Để** xử lý các trường hợp gian lận hoặc vi phạm chính sách.

**Acceptance Criteria:**
- Given admin phát hiện host gian lận coin, When xóa host khỏi leaderboard, Then host không xuất hiện trong bảng xếp hạng và mất badge
- Given admin reset leaderboard theo chu kỳ (daily/weekly/monthly), Then bảng xếp hạng được làm mới đúng thời điểm
- Given mọi thay đổi leaderboard bởi admin, Then được ghi log với lý do để audit

**Priority**: Should Have | **Persona**: Suzuki

---

## EPIC 8: Content Moderation (FR-09)

---

### US-09-01: Báo cáo người dùng/nội dung vi phạm
**Là** người dùng,  
**Tôi muốn** báo cáo người dùng hoặc nội dung vi phạm,  
**Để** giúp duy trì môi trường an toàn trên nền tảng.

**Acceptance Criteria:**
- Given nhấn "Báo cáo" trên hồ sơ, tin nhắn, hoặc trong phòng livestream, Then hiển thị form với danh sách lý do (nội dung không phù hợp, quấy rối, spam, lừa đảo, khác)
- Given gửi báo cáo, Then nhận xác nhận "Báo cáo đã được ghi nhận" và báo cáo vào queue của moderator
- Given cùng một nội dung nhận ≥5 báo cáo, Then tự động escalate lên moderator với priority cao
- Given người dùng báo cáo sai mục đích (spam báo cáo), Then hệ thống phát hiện và giới hạn số báo cáo/ngày

**Priority**: Must Have | **Persona**: Tanaka, Yamamoto

---

### US-09-02: AI tự động phát hiện nội dung vi phạm trong livestream
**Là** hệ thống moderation,  
**Tôi muốn** tự động phát hiện nội dung không phù hợp trong livestream,  
**Để** xử lý vi phạm nhanh hơn mà không cần moderator xem từng stream.

**Acceptance Criteria:**
- Given livestream đang diễn ra, Then AWS Rekognition phân tích video frame mỗi 30 giây
- Given phát hiện nội dung vi phạm mức độ cao (nudity, violence), Then tự động dừng livestream và gửi alert cho moderator
- Given phát hiện vi phạm mức độ trung bình, Then gửi cảnh báo cho moderator để review thủ công
- Given false positive (AI nhầm), Then moderator có thể override và khôi phục livestream

**Priority**: Must Have | **Persona**: Suzuki

---

### US-09-03: Moderator xử lý báo cáo vi phạm
**Là** moderator,  
**Tôi muốn** xem và xử lý các báo cáo vi phạm trong dashboard,  
**Để** bảo vệ cộng đồng khỏi nội dung và hành vi không phù hợp.

**Acceptance Criteria:**
- Given mở trang Reports trong admin dashboard, Then hiển thị danh sách báo cáo sắp xếp theo priority (AI-flagged > nhiều báo cáo > mới nhất)
- Given xem chi tiết báo cáo, Then thấy nội dung vi phạm, thông tin người bị báo cáo, lịch sử vi phạm trước đây
- Given moderator quyết định xử lý, Then có thể: cảnh báo, tạm khóa (1/3/7 ngày), khóa vĩnh viễn, hoặc bỏ qua (false positive)
- Given thực hiện hành động, Then người vi phạm nhận thông báo với lý do, hành động được ghi log

**Priority**: Must Have | **Persona**: Suzuki

---

## EPIC 9: Admin Dashboard (FR-10)

---

### US-10-01: Admin quản lý người dùng
**Là** admin,  
**Tôi muốn** tìm kiếm và quản lý tài khoản người dùng,  
**Để** xử lý các vấn đề tài khoản và đảm bảo tuân thủ chính sách.

**Acceptance Criteria:**
- Given tìm kiếm theo email, tên, hoặc ID, Then hiển thị kết quả với thông tin cơ bản và trạng thái tài khoản
- Given xem chi tiết người dùng, Then thấy: thông tin hồ sơ, lịch sử giao dịch, lịch sử vi phạm, trạng thái xác minh
- Given khóa tài khoản, Then người dùng bị đăng xuất ngay lập tức và không thể đăng nhập, nhận email thông báo
- Given mở khóa tài khoản, Then người dùng có thể đăng nhập lại bình thường

**Priority**: Must Have | **Persona**: Suzuki

---

### US-10-02: Admin remove viewer vi phạm khỏi livestream
**Là** admin/moderator,  
**Tôi muốn** remove (kick) người xem vi phạm chính sách khỏi phòng livestream đang diễn ra,  
**Để** can thiệp nhanh khi host không tự xử lý được.

**Acceptance Criteria:**
- Given admin xem danh sách livestream đang diễn ra, When chọn một phòng, Then thấy danh sách viewer đang trong phòng
- Given admin chọn viewer và nhấn "Remove khỏi phòng", Then viewer bị disconnect ngay lập tức và nhận thông báo
- Given admin có thể kèm theo lý do khi remove, Then lý do được ghi vào log và có thể dùng làm bằng chứng nếu cần
- Given hành động remove, Then được ghi log với: admin ID, viewer ID, phòng stream, timestamp, lý do

**Priority**: Must Have | **Persona**: Suzuki

---

### US-10-03: Admin quản lý tài chính và yêu cầu rút tiền
**Là** admin,  
**Tôi muốn** xem thống kê doanh thu và xử lý yêu cầu rút tiền của host,  
**Để** đảm bảo tài chính minh bạch và host nhận được thu nhập đúng hạn.

**Acceptance Criteria:**
- Given mở trang Finance, Then hiển thị tổng doanh thu (daily/weekly/monthly), số giao dịch, top spenders
- Given xem danh sách yêu cầu rút tiền, Then hiển thị: tên host, số tiền, thông tin ngân hàng, ngày yêu cầu, trạng thái
- Given phê duyệt yêu cầu rút tiền, Then trạng thái chuyển sang "Đang chuyển khoản" và host nhận thông báo
- Given từ chối yêu cầu, Then admin phải nhập lý do, host nhận thông báo với lý do cụ thể

**Priority**: Must Have | **Persona**: Suzuki

---

### US-10-04: Admin xem báo cáo thống kê
**Là** admin,  
**Tôi muốn** xem báo cáo DAU/MAU, doanh thu và top host,  
**Để** theo dõi sức khỏe của nền tảng và đưa ra quyết định kinh doanh.

**Acceptance Criteria:**
- Given mở trang Analytics, Then hiển thị biểu đồ DAU/MAU theo thời gian, doanh thu theo ngày/tuần/tháng
- Given xem top host, Then hiển thị top 10 host theo coin nhận được, số viewer, thời gian stream
- Given export báo cáo, Then tải về file CSV với dữ liệu đã lọc

**Priority**: Should Have | **Persona**: Suzuki

---

## Tổng Kết Stories

| Epic | Số Stories | Must Have | Should Have | Could Have |
|---|---|---|---|---|
| EPIC 1: Auth & Profile | 7 | 5 | 2 | 0 |
| EPIC 2: Matching | 3 | 3 | 0 | 0 |
| EPIC 3: Livestream Public | 5 | 4 | 1 | 0 |
| EPIC 4: Livestream Private | 3 | 3 | 0 | 0 |
| EPIC 5: Chat & Notifications | 4 | 4 | 0 | 0 |
| EPIC 6: Coin & Payment | 4 | 3 | 0 | 1 |
| EPIC 7: Leaderboard | 4 | 3 | 1 | 0 |
| EPIC 8: Content Moderation | 3 | 3 | 0 | 0 |
| EPIC 9: Admin Dashboard | 4 | 3 | 1 | 0 |
| **Tổng** | **37** | **31** | **5** | **1** |
