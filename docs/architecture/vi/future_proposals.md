# Kế hoạch phát triển v4.0 (Future Proposals)
**Dự án**: Universe Architecture

Nhằm đưa Universe Architecture bứt phá giới hạn hiện tại, đây là các đề xuất chuyên sâu (Advanced Topics) cho tương lai.

## 1. Zero-Trust Network & gRPC Service Mesh
- **Vấn đề**: Việc viết Proxy Module thủ công ở Level 3 (Microservices) là tốn thời gian.
- **Đề xuất**: Xây dựng tool sinh code (Code Generator). Tự động quét cấu trúc `IModule` và sinh ra giao thức gRPC. Các instance của Universe sẽ tự động peer-to-peer (giao tiếp Mesh) tạo thành cụm Vũ Trụ liên thông.

## 2. Temporal Workflows (Saga Pattern)
- **Vấn đề**: Các quá trình chạy lâu (Long-running process) như đặt hàng, chờ thanh toán, trừ kho. Nếu sập Server giữa chừng sẽ mất Data.
- **Đề xuất**: Định nghĩa `IWorkflowModule` với File lưu State (Event Sourcing). Cho phép khôi phục tiến trình chính xác vị trí bị đứt gãy giữa các Module.

## 3. Web UI Composition (Vũ trụ Frontend)
- **Vấn đề**: Backend đã chia module cực đẹp, nhưng Frontend (React/Vue) vẫn là đống mã cứng liên kết chéo.
- **Đề xuất**: Ứng dụng Micro-Frontends (Module Federation). Core UI sẽ gọi lệnh tới Backend hỏi: "Module A có giao diện nào đăng ký không?", Backend trả về URL CDN của file `.js`, Frontend tự động load giao diện đó lên menu.

## 4. Hot-Reloading / Dynamic DLL (SO) Loading
- **Vấn đề**: Đang chạy Production phải stop app để chèn chức năng mới vào?
- **Đề xuất**: `ModuleRegistry.LoadPlugin("path/to/addon.dll")`. Tự động scan Memory và nạp lệnh mới mà ứng dụng chính không hề gián đoạn (Zero-Downtime Deployment).
