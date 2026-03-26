# Architecture Decision Records (ADRs)
**Dự án**: Universe Architecture v3.0

Tài liệu này ghi lại các quyết định thiết kế kiến trúc quan trọng, nhằm chứng minh Universe Architecture đáp ứng tiêu chuẩn Enterprise (Khả năng mở rộng, Bảo trì, Khả năng thay thế, và Hiệu năng).

## ADR-001: Sử dụng "Module Registry" thay vì mã hóa cứng (Hard-coded routes)
**Ngữ cảnh**: Kiến trúc truyền thống dùng Switch/If-Else khiến file Router bị thay đổi liên tục. Vi phạm Open-Closed Principle.
**Quyết định**: Dùng `ModuleRegistry` tự động đăng ký.
**Kết quả**: Tối đa hóa khả năng cắm thêm chức năng (Zero-change Extension). Lookup O(1) đảm bảo hiệu năng.

## ADR-002: Publish/Subscribe qua EventBus
**Ngữ cảnh**: Gọi API trực tiếp giữa module tạo ra kết dính cứng (Tight Coupling).
**Quyết định**: Áp dụng Sóng Hấp Dẫn (`IEventBus`) để liên lạc bất đồng bộ.
**Kết quả**: Sẵn sàng dễ dàng thay Core thành Kafka/RabbitMQ khi chuyển sang Kiến trúc Phân tán (Distributed Microservices).

## ADR-003: "Fractal Universe" - Registry lồng nhau
**Ngữ cảnh**: Hệ thống lớn lên hàng ngàn chức năng sẽ bị tràn bộ định tuyến phẳng.
**Quyết định**: Module có thể chứa Registry con (vd `hr.salary`).
**Kết quả**: Code chia nhỏ theo Domain-Driven Design (Bounded Context).

## ADR-004: Lực hấp dẫn "Middleware Pipeline"
**Ngữ cảnh**: Auth, Logging nằm rải rác từng API.
**Quyết định**: Quấn mọi lệnh gửi qua chuỗi Middleware.
**Kết quả**: Business Code thuần khiết. Các Cross-Cutting concerns tập trung 1 chỗ.
