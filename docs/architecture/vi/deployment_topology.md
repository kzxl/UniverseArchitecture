# Sơ đồ Triển khai (Deployment Topology)
**Dự án**: Universe Architecture v3.0

Hướng dẫn tổ chức kiến trúc từ lúc khởi điểm đến quy mô cực lớn.

## Cấp độ 1: Kiến trúc Đơn nguyên (Modular Monolith)
Trong giai đoạn đầu, không tách Microservices. Mọi Module compile chung 1 Exe.
- **Cơ chế**: ModuleRegistry gọi trực tiếp (In-Process). Event Bus chạy Memory.
- **Ưu điểm**: Triển khai nhanh, dễ Debug, Network Latency = 0.

## Cấp độ 2: Vũ trụ lồng nhau (Nested Universe)
Hệ thống lên > 100 chức năng. Nhóm chúng lại bằng **SubRegistry** (`hr.salary`, `hr.employee`).
- Buộc chia cơ sở dữ liệu (Data Sovereignty) theo từng nhóm. Ngăn chặn module đọc chéo DB.

## Cấp độ 3: Dải ngân hà (Distributed Microservices)
Một Core Module quá tải, cần tách ra API riêng.
- **Cách làm**:
  - Tách thư mục chức năng đó ra 1 WebAPI riêng.
  - Viết 1 Proxy Module ở App cũ, hứng lệnh và đẩy sang WebAPI kia qua gRPC/HTTP.
  - Sử dụng `RabbitMqEventBus` thay cho In-Memory.
- **Lợi ích**: Code gọi từ các module khác không cần sửa bất kỳ chữ nào vì Contract của Proxy vẫn y chang bản gốc. Phép mầu Zero-change Extension đã xảy ra.
