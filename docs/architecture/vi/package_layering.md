# Hướng dẫn Phân lớp Packages (Mono vs Distributed)
**Dự án**: Universe Architecture v4.0

## Triết lý cốt lõi: "Chỉ dùng những gì cần thiết"

Universe Architecture **KHÔNG** bắt buộc dùng microservices. Kiến trúc theo lớp (Layers), mỗi lớp là **opt-in** — dự án Monolith chỉ cần Core, còn các tính năng nâng cao chỉ khi thực sự cần.

## Biểu đồ phân lớp

```
┌─────────────────────────────────────────────────┐
│            Universe.Distributed (Tuỳ chọn)      │  ← Chỉ dùng khi cần Microservices
│  gRPC Mesh · RabbitMQ Adapter · Service Proxy   │
├─────────────────────────────────────────────────┤
│            Universe.Plugins (Tuỳ chọn)          │  ← Chỉ dùng khi cần Hot-Reload DLL
│  PluginLoader · PluginWatcher                   │
├─────────────────────────────────────────────────┤
│            Universe.Workflows (Tuỳ chọn)        │  ← Chỉ dùng khi có quy trình dài
│  IWorkflowModule · WorkflowEngine · Saga        │
├─────────────────────────────────────────────────┤
│            Universe.Core (BẮT BUỘC)             │  ← MỌI DỰ ÁN ĐỀU DÙNG
│  IModule · ModuleRegistry · EventBus (Memory)   │
│  Middleware · Lifecycle · ServiceContainer       │
│  IAsyncModule · INestedModule · ISecureModule    │
└─────────────────────────────────────────────────┘
```

## Hướng dẫn chọn lớp theo loại dự án

| Loại dự án | Core | Workflows | Plugins | Distributed |
|------------|:----:|:---------:|:-------:|:-----------:|
| **WinForms/WPF (Desktop)** | ✅ | ❌ | ⚡ Tuỳ | ❌ |
| **Web API (Monolith)** | ✅ | ⚡ Tuỳ | ❌ | ❌ |
| **Enterprise ERP** | ✅ | ✅ | ✅ | ❌ |
| **SaaS Multi-Tenant** | ✅ | ✅ | ✅ | ✅ |
| **CLI Tool** | ✅ | ❌ | ❌ | ❌ |

## Ví dụ cụ thể

### Dự án Monolith (Hệ thống MDS, ERP nội bộ)
```csharp
// Chỉ cần dùng Core — 0 thêm dependency
var registry = new ModuleRegistry();
registry.Register(new InventoryModule());
registry.Register(new SalesModule());
registry.AddMiddleware(new LoggingMiddleware());

// Đủ dùng. EventBus chạy in-memory. Không cần RabbitMQ.
registry.EventBus.Subscribe<OrderCreatedEvent>(e => ...);
```

### Dự án cần Scale (Tách Reporting ra server riêng)
```csharp
// Vẫn dùng y chang code trên, chỉ THÊM 1 dòng:
registry.Services.RegisterInstance<IEventBus>(new RabbitMqEventBusAdapter("amqp://..."));
// Toàn bộ business code KHÔNG SỬA GÌ.
```

## Kết luận
- **1 codebase duy nhất**, không cần chia branch.
- Dự án nhỏ chỉ import `Universe.Core`.
- Khi lớn lên, thêm `Universe.Workflows` hoặc `Universe.Distributed` mà không phải refactor.
- Đây chính là triết lý "Vũ trụ mở rộng tự nhiên" — thêm thiên hà mới mà không phá vỡ quy luật vật lý.
