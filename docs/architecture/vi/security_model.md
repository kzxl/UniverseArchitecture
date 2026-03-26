# Security & Threat Model
**Dự án**: Universe Architecture v3.0

## 1. Mục tiêu bảo mật
Ngăn chặn hiệu ứng domino: "Một module lỗi/chứa mã độc không được lan sang module khác".

## 2. Bề mặt tấn công (Attack Surface)
- Gửi lệnh (Dispatch command) trái phép vào module nhạy cảm.
- Lắng nghe/Publish Event Bus chặn dữ liệu.
- Vượt mặt Middleware.

## 3. Sandboxing & Access Control List (ACL)
Universe Architecture giới thiệu `ISecureModule` định nghĩa chính quyền hạn.
Dùng `AccessControlMiddleware` kiểm tra xem `Principal` (người/vật gọi) có chứa đủ Claims so với `RequiredClaims` của module đích hay không. Chặn đứng lệnh từ ngoài vòng gửi xe.

Nhờ Registry con (Nested), chúng ta có thể chèn **Local Middleware** bảo vệ cục bộ cho một cụm Module (ví dụ cụm Tự Động Hóa chỉ chấp nhận lệnh nội mạng).

## 4. Bảo vệ Dữ Liệu (Data Sovereignty)
Nguyên tắc số #6 của Vũ Trụ: "Mỗi thiên hà có hệ thống sao riêng".
Module A **không** Reference DB Context của Module B.
Nếu tuân thủ, SQL Auth/Data leak của Module A là điểm chết cục bộ, không lan truyền toàn hệ thống.
