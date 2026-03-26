# 🌌 Universe Architecture — Kiến Trúc Mở Rộng Không Giới Hạn

> Vũ trụ mở rộng vô hạn, phát hiện nền văn minh mới mà không phá vỡ quy luật vật lý.
> Áp dụng cho phần mềm: thêm module mới mà không phá vỡ kiến trúc hiện tại.

---

## 1. Triết Lý

### Vũ trụ mở rộng mà không vỡ — Tại sao?

4 lực cơ bản (Gravity, EM, Strong, Weak) = **Interface** của vũ trụ. Bất kỳ "module" nào (thiên hà, nền văn minh) đều tuân thủ _cùng quy luật_, nên vũ trụ mở rộng vô hạn mà không conflict.

```
┌──────────────────── VŨ TRỤ ──────────────────────┐
│                                                    │
│  QUY LUẬT VẬT LÝ  →  KHÔNG GIAN  →  NỀN VĂN MINH │
│  (Interfaces)         (Infra)        (Modules)     │
│   Bất biến            Chỗ sống       Tự hình thành │
│                                                    │
└────────────────────────────────────────────────────┘
```

| Vũ trụ | Phần mềm | Giải thích |
|--------|----------|-----------|
| Quy luật vật lý | **Core Interfaces** | Bất biến, mọi thứ phải tuân thủ |
| Không-thời gian | **Shared Infrastructure** | Nền tảng modules "sống" trên |
| Thiên hà | **Module Category** | Nhóm liên quan, độc lập |
| Ngôi sao | **Individual Module** | Đơn vị tự chứa |
| Hố đen | **Anti-pattern** (God class) | Nuốt chửng xung quanh |

### So với Plugin truyền thống

Plugin truyền thống dễ bị version mismatch, conflict shared resource.
**Universe model** vượt qua nhờ:
1. **Interface ổn định** — backward-compatible mãi mãi
2. **Module cô lập** — chạy "không gian" riêng
3. **Core không biết có bao nhiêu module** — không hard-code
4. **Shared infra = gravity** — kéo mọi thứ về đúng chỗ

---

## 2. Tám Nguyên Tắc

### #1: Core Stable — Modules Volatile
Core (interfaces, base classes, shared services) ít thay đổi. Modules thoải mái thêm/sửa/xoá.

> *Trong phiên bản 3.0, `IServiceContainer` đóng vai trò như Trạm Không Gian (Space Station) — một Dependency Injection container nhẹ nhàng dùng để phân giải dependency chuyên biệt cho module mà không bị gắn chặt vào thư viện DI bên thứ 3 (như Autofac hay MS.DI).*

### #2: Module Independence
```
✅ Module → Core/Shared  (OK)
❌ Module → Module       (CẤM — extract shared logic vào Core)
```

### #3: Contract-First
Mọi module tuân thủ cùng 1 interface (metadata + lifecycle + UI factory).

### #4: Registry — Đăng ký, không Hard-code
Module tự đăng ký. Core không biết chi tiết module.

### #5: Giao tiếp gián tiếp (EventBus)
Module cần "nói chuyện" → qua **Event Bus / Mediator**, không import trực tiếp.
> *Trong 3.0, Sóng hấp dẫn (Gravitational Waves) được cung cấp qua `IEventBus`.*

```csharp
// Module A: Publish event (không biết ai nghe)
registry.EventBus.Publish(new UserLoggedInEvent { UserId = 123 });

// Module B: Subscribe (không cần reference Module A)
public void OnInitialized() {
    registry.EventBus.Subscribe<UserLoggedInEvent>(this, @event => {
        RefreshDashboard(@event.UserId);
    });
}
```

### #6: Data Sovereignty — Module Sở Hữu Data Riêng
> *Mỗi thiên hà có hệ thống sao riêng — không xâm phạm không gian thiên hà khác.*

Module A **không truy cập trực tiếp** bảng/entity của Module B. Cần data từ module khác → gọi qua **Service interface**.

```
✅ OrderService gọi ICustomerService.GetById(id)     (OK — qua interface)
❌ OrderService query trực tiếp tbCAT_Customer         (CẤM — vi phạm sovereignty)
```

**Áp dụng thực tế theo nền tảng:**

| Nền tảng | Data Sovereignty |
|----------|------------------|
| Web API | Mỗi feature folder chứa DTOs riêng, Service chỉ query bảng thuộc feature |
| WinForms | Controller chỉ inject Service của feature mình, dữ liệu khác qua Shared Service |
| Microservices | Mỗi service có DB schema/database riêng |

### #7: Middleware = Lực Hấp Dẫn & Module Lifecycle (Vòng Đời Ngôi Sao)
> *Lực hấp dẫn tác động lên MỌI vật thể trong vũ trụ mà không cần vật thể biết về nó. Các ngôi sao cũng có vòng đời sinh ra và chết đi.*

#### A. Middleware Pipeline (Lực Hấp Dẫn)
Cross-cutting concerns (Auth, Logging, Compression, Error Handling) được xử lý ở **Middleware layer** — tự động tác động lên mọi request/action mà module không bị bẩn code.

```csharp
public interface IMiddleware {
    Task Invoke(ModuleContext context, Func<Task> next);
}

// Setup Pipeline in Core:
registry.AddMiddleware(new AuthenticationMiddleware());
registry.AddMiddleware(new LoggingMiddleware());
registry.AddMiddleware(new ErrorHandlingMiddleware());
// Tự động áp dụng cho mọi lệnh Dispatch().
```

**Key rules:**
- Module **KHÔNG chứa** auth check, logging boilerplate, compression logic.
- Middleware **KHÔNG chứa** business logic.
- Thêm middleware mới = thêm 1 dòng code config, không sửa module nào.

#### B. Module Lifecycle (Vòng Đời Ngôi Sao)
Module luôn cần quản lý tài nguyên (DB connections, background threads, socket listeners). `IModuleLifecycle` giúp module khởi tạo và dọn dẹp an toàn.

```csharp
public interface IModuleLifecycle {
    Task OnInitializing();      // Trước khi đăng ký
    Task OnInitialized();       // Sau khi đăng ký
    Task OnShuttingDown();      // Trước khi xoá/tắt
    Task OnShutdown();          // Sau khi tắt
}
```

### #8: Migration Path — Từ Thiên Hà Đến Vũ Trụ Con
> *Khi thiên hà quá lớn, nó có thể tách ra thành vũ trụ con độc lập.*

Module đạt đủ điều kiện có thể **tách thành service/process riêng** (microservice) mà không refactor lại kiến trúc.

**Điều kiện tách:**
1. ✅ Đạt Acceptance Test (xoá folder → vẫn build)
2. ✅ Data Sovereignty đã áp dụng (không query bảng module khác)
3. ✅ Giao tiếp qua Interface (không hard-code dependency)
4. ✅ Có independent deployment artifact (project/package riêng)

**Lộ trình 3 cấp:**
```
Level 1: Modular Monolith
  └── Modules trong cùng process, giao tiếp qua DI

Level 2: In-Process Event Bus  
  └── Modules giao tiếp qua MediatR / IEventAggregator
  └── Chuẩn bị tách nhưng chưa tách

Level 3: Distributed (Microservices)
  └── Modules chạy process riêng
  └── Giao tiếp qua Message Queue (RabbitMQ, Kafka)
  └── Mỗi module có DB riêng
```

---

## 3. Áp Dụng Theo Nền Tảng

### 3.1 WinForms (.NET Framework) — MDS/MLG2/RAF Pattern

**Constraint thực tế**:
- Legacy `.csproj` explicit file include
- LINQ to SQL (`dbMDSDataContext`)
- Autofac DI container
- BaseForm + RunAfterShown lifecycle
- View / Controller / Service 3-layer

**Universe mapping**:

| Universe Layer | WinForms Implementation |
|---------------|------------------------|
| **Quy luật (Interface)** | `IFeatureController` — contract cho mọi module |
| **Không gian (Infra)** | `BaseForm`, `Autofac`, `SharedLib`, `Themes` |
| **Module** | Feature folder: Controller + Service + Views |
| **Registry** | Autofac assembly scanning + `ShowUserControl` |
| **Giao tiếp** | `IEventAggregator` / delegate callbacks |

**Folder structure đúng kiểu vũ trụ**:
```
MDSManagement/
├── Shared/              ← Không-thời gian (infra)
│   ├── CustomControl/   BaseForm, BaseUserControl
│   ├── Extensions/      UI helpers, data extensions
│   └── Services/        Shared services
│
├── Features/            ← Các nền văn minh
│   ├── Sales/
│   │   ├── Order/
│   │   │   ├── OrderController.cs
│   │   │   ├── OrderController.Import.cs  (partial)
│   │   │   ├── OrderService.cs
│   │   │   ├── frmOrder.cs
│   │   │   └── ucOrderList.cs
│   │   ├── Invoice/
│   │   └── Customer/
│   ├── Production/
│   │   ├── Plan/
│   │   └── QA/
│   └── Inventory/
│       ├── Material/
│       └── Product/
│
├── Infrastructure/      ← Quy luật vật lý
│   ├── AutofacConfig.cs
│   ├── PermissionManager.cs
│   └── NavigationRegistry.cs
```

**Module registration (ShowUserControl)**:
```csharp
// Navigation tự discover modules — không hard-code!
// Core chỉ biết có "slots", không biết chi tiết module
panelMain.ShowUserControl<ucOrderList>();  // Autofac auto-resolve
```

**Key rules cho WinForms**:
- View chỉ `PopulateControls()` / `CollectData()` — không DB logic
- Controller thin pass-through → Service
- Service chứa business logic + DB access
- Partial class cho Controller lớn (`OrderController.Import.cs`)
- Guard `_isInitializing` trong `RunAfterShown`

---

### 3.2 ASP.NET Web API (.NET Framework) — MDS API Pattern

**Constraint thực tế**:
- .NET Framework 4.x (non-SDK csproj)
- OWIN / DelegatingHandler pipeline
- Autofac + `InstancePerRequest`
- `BaseApiController` + `TryAction/TryPaged` wrappers

**Universe mapping**:

| Universe Layer | Web API Implementation |
|---------------|----------------------|
| **Quy luật** | `BaseApiController`, `ApiResponse<T>`, `IGenericService<T>` |
| **Không gian** | `Infrastructure/` (Autofac, JWT, Swagger, GZip) |
| **Module** | `Features/{Module}/{Feature}/` (vertical slice) |
| **Registry** | Autofac assembly scanning (`EndsWith("Service")`) |
| **Giao tiếp** | Service injection — không cross-feature |

**Folder structure**:
```
MDS.API/
├── Infrastructure/           ← Quy luật vật lý
│   ├── AutofacConfig.cs      Assembly scanning
│   ├── WebApiConfig.cs       Route + handler pipeline
│   ├── SwaggerConfig.cs      NSwag generation
│   └── Security/
│       └── JwtSettings.cs
│
├── Features/                 ← Các nền văn minh (vertical slice)
│   ├── Sales/
│   │   ├── Customer/
│   │   │   ├── CustomerController.cs
│   │   │   ├── Services/
│   │   │   │   ├── ICustomerService.cs
│   │   │   │   └── CustomerService.cs
│   │   │   └── DTOs/
│   │   │       └── CustomerDto.cs
│   │   ├── Order/
│   │   └── Invoice/
│   ├── Production/
│   └── Technical/
│
├── Shared/                   ← Không-thời gian
│   ├── Models/               dbMDS.dbml, ApiResponse
│   ├── Extensions/           Paging, DataType
│   ├── Helpers/              CodeGenerator, PaymentDue
│   └── Constants/            StatusConstants
│
└── Middleware/               ← Lực hấp dẫn (cross-cutting)
    ├── JwtValidationHandler.cs
    ├── GZipCompressionHandler.cs
    └── NotFoundHandler.cs
```

**Acceptance test**: Xoá folder `Features/Sales/Customer/` → toàn bộ project vẫn build.

---

### 3.3 ASP.NET Core (.NET 6/8)

**Khác biệt với .NET Framework**:
- SDK-style `.csproj` — auto-include files
- Built-in DI (`IServiceCollection`)
- Middleware pipeline thay DelegatingHandler
- `Minimal API` hoặc `Controller-based`

**Universe mapping**:

```csharp
// Program.cs — Registry pattern
var builder = WebApplication.CreateBuilder(args);

// Auto-discover services (quy luật)
builder.Services.Scan(scan => scan
    .FromAssemblyOf<Program>()
    .AddClasses(c => c.Where(t => t.Name.EndsWith("Service")))
    .AsImplementedInterfaces()
    .WithScopedLifetime());

// Module registration
builder.Services.AddFeatureModule<SalesModule>();
builder.Services.AddFeatureModule<ProductionModule>();
// Thêm module mới → 1 dòng

var app = builder.Build();

// Module map routes
app.MapFeatureModules(); // Auto-discover & map
```

**Feature module interface**:
```csharp
public interface IFeatureModule
{
    string Name { get; }
    void RegisterServices(IServiceCollection services);
    void MapRoutes(WebApplication app);
}

public class SalesModule : IFeatureModule
{
    public string Name => "Sales";
    public void RegisterServices(IServiceCollection s) => s.AddScoped<IOrderService, OrderService>();
    public void MapRoutes(WebApplication app) => app.MapGroup("/api/sales").MapOrderEndpoints();
}
```

---

### 3.4 WPF (.NET 8) — NetTool Pattern

**Universe mapping**:

| Universe Layer | WPF Implementation |
|---------------|-------------------|
| **Quy luật** | `ITool` interface, `ToolViewModelBase` |
| **Không gian** | `Core/` (EngineRunner, ExportService, Themes) |
| **Module** | `Modules/{Name}/` (ViewModel + View + Models) |
| **Registry** | `ToolRegistry` → TabControl auto-populate |

**ITool interface**:
```csharp
public interface ITool {
    string Name { get; }
    string Icon { get; }
    int Order { get; }
    ToolViewModelBase ViewModel { get; }
    UserControl CreateView();
}
```

**ToolViewModelBase** (Template Method):
```csharp
public abstract class ToolViewModelBase : ViewModelBase, IDisposable {
    // Shared: IsRunning, StatusText, LogLines, Start/Stop commands
    protected abstract string GetEngineCommand();   // Module override
    protected abstract string BuildConfig();         // Module override
    protected abstract void OnEngineOutput(string line); // Module override
}
```

**Capability composition** (PoE gem links):
```csharp
public interface IExportable { IReadOnlyList<object> GetExportData(); }
public interface IChartable  { ISeries[] GetChartSeries(); }

// Core tự detect:
if (tool is IChartable c) ShowChart(c.GetChartSeries());
```

---

### 3.5 Go CLI — Multi-Command Engine

**Universe mapping**:

| Universe Layer | Go Implementation |
|---------------|------------------|
| **Quy luật** | `Command` interface (Name, Run) |
| **Không gian** | `shared/` (output writer, config loader) |
| **Module** | `cmd/<name>.go` — mỗi command 1 file |
| **Registry** | `init()` function auto-register |

```go
// shared/command.go — Interface (quy luật vật lý)
type Command interface {
    Name() string
    Description() string
    Run(ctx context.Context, configPath string) error
}

// cmd/registry.go — Registry
var commands = map[string]Command{}
func Register(c Command) { commands[c.Name()] = c }
func Dispatch(name, configPath string) error {
    c, ok := commands[name]
    if !ok { return fmt.Errorf("unknown: %s", name) }
    return c.Run(context.Background(), configPath)
}

// cmd/ping.go — Module (tự đăng ký)
func init() { Register(&PingCommand{}) }
type PingCommand struct{}
func (c *PingCommand) Name() string { return "ping" }
func (c *PingCommand) Run(ctx context.Context, configPath string) error { ... }
```

---

### 3.6 React + Node.js — HelpDesk Pattern

**Universe mapping**:

| Universe Layer | React Implementation |
|---------------|---------------------|
| **Quy luật** | `IPlugin` interface (TS), Redux slice contract |
| **Không gian** | Redux store, Axios instance, Socket.io, Theme |
| **Module** | `features/<name>/` (component + slice + API) |
| **Registry** | Route config array, dynamic UI generation |

**Frontend (React)**:
```typescript
// core/PluginRegistry.ts
interface IFeaturePlugin {
    id: string;
    name: string;
    icon: string;
    path: string;            // Route path
    component: React.FC;      // Page component
    navSection: string;       // Menu group
}

const plugins: IFeaturePlugin[] = [
    { id: 'tasks', name: 'Tasks', icon: '📋', path: '/tasks', component: TaskPage, navSection: 'Work' },
    { id: 'projects', name: 'Projects', icon: '📁', path: '/projects', component: ProjectPage, navSection: 'Work' },
    // Thêm module → 1 object
];
```

**Backend (Express.js)**:
```javascript
// core/moduleLoader.js
const fs = require('fs');
const path = require('path');

function loadModules(app) {
    const modulesDir = path.join(__dirname, '../features');
    fs.readdirSync(modulesDir).forEach(dir => {
        const routeFile = path.join(modulesDir, dir, 'routes.js');
        if (fs.existsSync(routeFile)) {
            const routes = require(routeFile);
            app.use(`/api/${dir}`, routes);  // Auto-mount
        }
    });
}
```

---

### 3.7 PHP — Legacy Web App

```
app/
├── core/               ← Quy luật
│   ├── Module.php       Interface
│   ├── Registry.php     Auto-discover
│   └── Database.php     Shared DB
├── modules/            ← Nền văn minh
│   ├── auth/
│   │   ├── AuthModule.php
│   │   ├── AuthController.php
│   │   └── views/
│   ├── inventory/
│   └── report/
└── shared/             ← Không gian
    ├── helpers.php
    └── config.php
```

---

## 4. Anti-Patterns & Giải Pháp

| ❌ Anti-Pattern | ✅ Giải pháp Universe | Nguyên tắc |
|----------------|---------------------|:---:|
| Module A import Module B | Event Bus / Mediator | #5 |
| Core chứa module logic | Delegate cho module | #1 |
| Hard-code module list | Registry auto-discover | #4 |
| God ViewModel/Controller (1000+ lines) | 1 VM/Controller per module + partial class | #2 |
| Shared mutable state | Immutable messages | #5 |
| `.csproj` thiếu file (legacy) | Kiểm tra `<Compile>` sau mỗi file mới | — |
| Service A query bảng của Module B | Gọi qua IServiceB interface | #6 |
| Auth/Log code rải rác trong modules | Tập trung vào Middleware pipeline | #7 |
| Monolith không tách được | Áp dụng #6 + #5 trước, tách dần theo Level | #8 |
| Module share DbContext chung | Mỗi module inject context riêng hoặc scoped | #6 |

---

## 5. Acceptance Test

### Test 1: Module Isolation (Cơ bản)
> **Xoá hoàn toàn 1 module folder → app vẫn build & chạy.**
> Nếu fail → còn coupling → cần refactor.

### Test 2: Data Sovereignty
> **Grep toàn bộ Service files trong Module A → không có reference đến Entity/Table của Module B.**
> Nếu fail → vi phạm Data Sovereignty → cần extract qua interface.

### Test 3: Middleware Independence
> **Xoá 1 middleware handler → app vẫn chạy** (chỉ mất cross-cutting feature đó).
> Nếu fail → middleware chứa business logic → cần tách.

### Test 4: Migration Readiness
> **Module có thể chạy như 1 project riêng** (với mock/stub các dependency).
> Nếu fail → chưa đủ điều kiện tách microservice.

---

## 6. Conversion Table Đầy Đủ

| Vũ trụ | Lớp Phần Mềm | WinForms / C# | Go | TypeScript | Python |
|--------|--------------|---------------|----|------------|--------|
| **Quy luật VL** | Core Interfaces | `IModule` | `Module` | `IModule` | `IModule` |
| **Không gian** | Shared Infra | `Program.cs` / `SharedLib` | `main.go` / `shared/` | `index.ts` | `main.py` |
| **Thiên hà** | Module Group | Feature folder | `modules/` | `src/modules/` | `modules/` |
| **Ngôi sao** | Module Unit | `NotifierModule` | `notifier` pkg | `NotifierModule` class | `NotifierModule` cls |
| **Registry** | Auto-discover | `ModuleRegistry` | `core.Registry` | `ModuleRegistry` | `ModuleRegistry` |
| **Lực hấp dẫn**| Middleware | `IMiddleware` | `Middleware` | `IMiddleware` | `IMiddleware` |
| **Vòng đời** | Lifecycle Hooks | `IModuleLifecycle` | `ModuleLifecycle`| `IModuleLifecycle` | `IModuleLifecycle` |
| **Sóng hấp dẫn**| Event Bus | `IEventBus` | `EventBus` | `EventBus` | `EventBus` |
| **Trạm K.Gian** | Service Container| `IServiceContainer` | N/A (Manual DI) | N/A (Manual DI) | N/A (Manual DI) |
| **Hệ sao riêng**| Data Sovereignty | Feature DB tables | pkg DB access | slice state | module schema |
| **Vũ trụ con** | Microservice | Tách project | Separate app | Micro-frontend | Separate app |

---

## 7. Module Communication Levels

> *Từ tín hiệu ánh sáng (cùng thiên hà) đến sóng hấp dẫn (xuyên vũ trụ).*

Khi module cần giao tiếp, chọn **level phù hợp** — không over-engineer:

```
┌─────────────────────────────────────────────────────────┐
│  Level 1: Direct Injection (cùng process)               │
│  ─────────────────────────────────────────               │
│  CustomerService inject IOrderService                   │
│  Pros: Đơn giản, debug dễ, type-safe                   │
│  Cons: Coupling tại compile-time                        │
│  Dùng khi: Modules LUÔN deploy cùng nhau               │
├─────────────────────────────────────────────────────────┤
│  Level 2: In-Process Event Bus                          │
│  ─────────────────────────────────────────               │
│  OrderService publish OrderCreatedEvent                  │
│  InventoryService subscribe → auto deduct stock          │
│  Pros: Loose coupling, mở rộng subscriber dễ            │
│  Cons: Debug khó hơn, event ordering                    │
│  Dùng khi: Modules CÓ THỂ tách trong tương lai         │
│  Tools: MediatR (.NET), IEventAggregator                │
├─────────────────────────────────────────────────────────┤
│  Level 3: Message Queue (cross-process)                 │
│  ─────────────────────────────────────────               │
│  OrderService → RabbitMQ → InventoryService             │
│  Pros: Chịu lỗi, scale độc lập, async                  │
│  Cons: Phức tạp, eventual consistency                   │
│  Dùng khi: Modules ĐÃ TÁCH thành service riêng         │
│  Tools: RabbitMQ, Kafka, Azure Service Bus              │
└─────────────────────────────────────────────────────────┘
```

**Quy tắc chọn Level:**
- Bắt đầu với **Level 1** — đơn giản nhất
- Chuyển lên **Level 2** khi 2+ modules cùng react theo 1 event
- Chuyển lên **Level 3** chỉ khi cần deploy/scale riêng

| Nền tảng | Level 1 | Level 2 | Level 3 |
|----------|---------|---------|--------|
| Web API (.NET) | Constructor injection | MediatR / domain events | RabbitMQ / Azure SB |
| WinForms | Autofac DI | IEventAggregator | Named Pipes / WCF |
| ASP.NET Core | `IServiceCollection` | MediatR | Kafka / gRPC |
| React | Props / Context | Redux dispatch | WebSocket / REST |
| Go | Function params | Channels | NATS / gRPC |

---

## 8. Nguồn Gốc & Tham Chiếu

Universe Architecture là tổng hợp thực dụng (practical synthesis) của các kiến trúc public:

| Nguyên tắc Universe | Nguồn gốc |
|:---|:---|
| #1 Core Stable, Modules Volatile | **Microkernel / Plugin Architecture** (POSA, 1996) |
| #2 Module Independence | **Modular Monolith** (Milan Jovanović) |
| #3 Contract-First | **Interface Segregation Principle** (SOLID) |
| #4 Registry | **Service Locator / Plugin Registry** (GoF, POSA) |
| #5 Giao tiếp gián tiếp | **Mediator Pattern** (GoF) + **Event-Driven Architecture** |
| #6 Data Sovereignty | **Bounded Context** (DDD, Eric Evans) |
| #7 Middleware | **Pipeline Pattern** + **Chain of Responsibility** (GoF) |
| #8 Migration Path | **Strangler Fig Pattern** (Martin Fowler) |
| Folder-per-Feature | **Vertical Slice Architecture** (Jimmy Bogard, 2018) |
| Dependency direction | **Clean Architecture** dependency rule (Uncle Bob, 2012) |

**Giá trị riêng của Universe Architecture:**
- 🌌 **Mental model vũ trụ** — biến concepts trừu tượng thành hình ảnh trực quan, dễ nhớ
- 🌐 **Cross-platform consistency** — cùng 1 mô hình cho WinForms, Web API, WPF, Go, React, PHP
- ✅ **Acceptance Test đơn giản** — "Xoá folder → vẫn build" dễ verify hơn mọi metric
- 📊 **Conversion Table** — practical cross-reference giữa 6+ platforms

---

## 9. 🚀 Breakthrough: Evolutionary & Performance-First

> *Vũ trụ không chỉ mở rộng — nó tự đo lường, tự tối ưu, tự tiến hóa.*
> Đây là phần **CHƯA CÓ kiến trúc public nào** tích hợp thành mental model thống nhất.

### 9.1 Architecture Fitness Functions — "Hằng Số Vũ Trụ"

> *Mỗi vũ trụ có hằng số vật lý (tốc độ ánh sáng, hằng số Planck). Thay đổi 1 hằng số → vũ trụ sụp đổ.*
> *Mỗi hệ thống phần mềm có "hằng số kiến trúc" — vi phạm → hệ thống suy thoái.*

**Fitness Function** = automated test đo lường sức khỏe kiến trúc liên tục, chạy trong CI/CD.

```
┌─────────────────────── FITNESS DASHBOARD ───────────────────────┐
│                                                                  │
│  🔴 Module Coupling Score    : 3/10  (target: ≤ 2)  ← ALERT    │
│  🟢 Feature Isolation        : 98%   (target: ≥ 95%)            │
│  🟢 Cyclomatic Complexity    : avg 8 (target: ≤ 15)             │
│  🟡 Data Sovereignty Violations: 2   (target: 0)    ← WARNING  │
│  🟢 Middleware Independence  : 100%  (target: 100%)             │
│  🟢 Build Time               : 12s   (target: ≤ 30s)           │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
```

**Mapping vũ trụ:**

| Hằng số vũ trụ | Fitness Function | Công cụ |
|:---|:---|:---|
| Tốc độ ánh sáng | **Response Time** ≤ 200ms | k6, JMeter |
| Lực hấp dẫn | **Coupling Score** ≤ threshold | NDepend, ArchUnit |
| Entropy | **Code Complexity** ≤ 15 | SonarQube |
| Tính đối xứng | **Data Sovereignty** = 0 violations | Custom grep/script |
| Hằng số Planck | **Module Size** ≤ max lines | Custom script |

**Cách implement:**
```csharp
// Fitness Function: Module Coupling chạy trong CI
[Test]
public void No_Feature_Should_Reference_Another_Feature()
{
    var types = typeof(Program).Assembly.GetTypes();
    var violations = types
        .Where(t => t.Namespace?.Contains("Features.Sales") == true)
        .SelectMany(t => t.GetReferencedAssemblies())
        .Where(r => r.Name.Contains("Features.Production"));
    
    Assert.IsEmpty(violations, "Sales → Production coupling detected!");
}
```

### 9.2 Performance Gravity — "Khối Lượng Quyết Định Quỹ Đạo"

> *Vật thể càng nặng → lực hấp dẫn càng lớn → chi phối chuyển động xung quanh.*
> *Module càng hot (nhiều request/data) → cần optimization strategy khác biệt.*

**Mỗi module có "khối lượng" (Performance Weight)** dựa trên:
- Request frequency (requests/s)
- Data volume (rows affected)
- Computation complexity (CPU time)

```
Module Performance Classification:

☀️ Sao (Star)     — High freq, High volume  → Cache + Async + Scale riêng
                     Ví dụ: Order, Inventory, Dashboard

🪐 Hành tinh      — Medium freq             → Standard optimization
                     Ví dụ: Customer, Product, Report

🌑 Mặt trăng      — Low freq, admin tasks   → Tối ưu code là đủ
                     Ví dụ: Admin, SystemConfig, Audit
```

**Performance Strategy theo Weight:**

| Classification | Strategy | Implementation |
|:---|:---|:---|
| ☀️ Sao | **Cache-First** + Async pipeline | Redis/MemoryCache, async controllers, read replica |
| ☀️ Sao | **Lazy Loading** cho sub-data | Load master first, detail on-demand |
| 🪐 Hành tinh | **Query Optimization** | Indexed queries, projection DTOs |
| 🌑 Mặt trăng | **Simple CRUD** | Standard service pattern đủ |

**Key insight**: Không phải mọi module cần cùng level optimization. Universe Architecture cho phép **vary strategies per module** mà vẫn tuân thủ cùng interface.

### 9.3 Self-Adaptive — "Tiến Hóa Tự Nhiên"

> *Vũ trụ tự tiến hóa — sao nhỏ trở thành sao khổng lồ rồi thành lỗ đen (nếu không kiểm soát).*
> *Module cũng tiến hóa — từ đơn giản → phức tạp. Cần cơ chế phát hiện sớm.*

**Module Lifecycle Stages:**
```
┌──────────┐    ┌───────────┐    ┌──────────┐    ┌─────────────┐
│ 🌱 Seed  │ →  │ 🌿 Growth │ →  │ ☀️ Mature │ →  │ 🕳️ Collapse │
│  < 200   │    │  200-500  │    │  500-1000│    │   > 1000    │
│  lines   │    │  lines    │    │  lines   │    │   lines     │
│          │    │           │    │          │    │  = GOD CLASS │
└──────────┘    └───────────┘    └──────────┘    └─────────────┘
                                      ↓ (phát hiện sớm)
                                 ┌──────────┐
                                 │ 🔀 Split │ Tách thành 2+ modules
                                 └──────────┘
```

**Auto-detection rules:**
```yaml
# universe-fitness.yml (chạy trong CI/CD)
rules:
  - name: "Module Size Warning"
    metric: lines_per_feature_folder
    warning: 500
    critical: 1000
    action: "Tách module hoặc extract sub-feature"
    
  - name: "Controller Method Count"
    metric: methods_per_controller
    warning: 10
    critical: 20
    action: "Tách thành partial class hoặc sub-controller"
    
  - name: "Cross-Feature Dependency"
    metric: import_between_features
    warning: 1
    critical: 3
    action: "Extract shared service hoặc dùng Event Bus"
    
  - name: "Response Time Degradation"
    metric: p95_response_ms
    warning: 500
    critical: 2000
    action: "Classify module lên ☀️ Star, áp dụng Cache-First"
```

### 9.4 Universe Topology — Multi-Client Architecture

> *Một thiên hà có thể được quan sát từ nhiều góc khác nhau — bằng kính viễn vọng, sóng radio, tia X.*
> *Cùng domain logic, nhiều client platform nhìn vào.*

**Đây là hướng đột phá thực sự**: cùng 1 Universe (domain), nhiều "lối nhìn" (client platforms):

```
                    ┌─────────────────────────────┐
                    │     🌌 UNIVERSE CORE         │
                    │     (Domain + Services)       │
                    │                               │
                    │  ┌─────────┐  ┌───────────┐  │
                    │  │ Sales   │  │Production │  │
                    │  │ Domain  │  │ Domain    │  │
                    │  └────┬────┘  └─────┬─────┘  │
                    │       │             │         │
                    └───────┼─────────────┼─────────┘
                            │             │
              ┌─────────────┼─────────────┼─────────────┐
              │             │             │             │
        ┌─────▼────┐  ┌────▼─────┐ ┌─────▼────┐ ┌─────▼────┐
        │ 🌐 API   │  │ 🖥️ WinFm │ │ 🎨 WPF  │ │ 🌍 Web   │
        │ Gateway  │  │  Client  │ │ Client   │ │ React    │
        └──────────┘  └──────────┘ └──────────┘ └──────────┘
```

**Solution Structure cho Enterprise:**
```
ERP.sln
├── Core/                          ← Quy luật vật lý (SHARED across ALL platforms)
│   ├── ERP.Domain/                Interfaces, Entities, Value Objects
│   ├── ERP.Services/              Business logic (platform-agnostic)
│   └── ERP.Shared/                DTOs, Constants, Extensions
│
├── Infrastructure/                ← Không-thời gian
│   ├── ERP.Persistence/           DbContext, Repositories, Migrations
│   └── ERP.Infrastructure/        Email, FileStorage, Cache
│
├── Clients/                       ← Các lối nhìn
│   ├── API/
│   │   ├── MDS.API/               REST API cho mobile/web consumers
│   │   └── HelpDesk.API/          REST API cho HelpDesk
│   ├── Desktop/
│   │   ├── MDS.WinForms/          Legacy desktop client
│   │   └── MDS.WPF/               Modern desktop client (optional)
│   └── Web/
│       └── MDS.Web/               React/Blazor SPA (optional)
│
└── Tests/                         ← Fitness Functions + Unit Tests
    ├── ERP.Tests.Unit/
    ├── ERP.Tests.Integration/
    └── ERP.Tests.Architecture/    Fitness function tests
```

**Key Rule — "Một vũ trụ, nhiều cách nhìn":**
```
✅ API Controller    → gọi ERP.Services   (OK — thin wrapper)
✅ WinForms Controller → gọi ERP.Services (OK — thin wrapper)
✅ WPF ViewModel     → gọi ERP.Services   (OK — thin wrapper)
❌ API chứa business logic riêng          (CẤM — duplicate!)
❌ WinForms chứa SQL queries              (CẤM — bypass Core!)
```

Mỗi client chỉ là **thin adapter** translate UI concepts thành Domain calls.

### 9.5 Fractal Universe — "Vũ Trụ Trong Vũ Trụ"

> *Zoom vào một thiên hà → thấy các hệ sao. Zoom vào hệ sao → thấy các hành tinh. Mỗi cấp đều tuân thủ cùng quy luật vật lý.*
> *Zoom vào hệ thống phần mềm cũng vậy. **Mỗi cấp là một vũ trụ** có cùng cấu trúc.*

Đây là nguyên lý **chưa có** ở bất kỳ architecture pattern public nào:
- DDD chỉ có 2 cấp (Bounded Context → Aggregate)
- Clean Architecture là 1 cấp (layers)
- Microkernel là 1 cấp (core + plugins)

**Fractal Universe scale vô hạn cấp**, mỗi cấp áp dụng **cùng quy tắc**:

```
🌌 System (Universe)
│   Ranh giới: Solution / Monorepo
│
├── 🌀 Application (Galaxy Cluster)
│   │   Ranh giới: Project / Package
│   │
│   ├── ⭐ Domain (Galaxy)
│   │   │   Ranh giới: Feature group folder + _Shared/
│   │   │
│   │   ├── 🪐 Feature (Solar System)
│   │   │   │   Ranh giới: Service / Controller / View / DTOs
│   │   │   │
│   │   │   ├── 🌍 Sub-feature (Planet)
│   │   │   │   Ranh giới: Partial class hoặc sub-folder
│   │   │   │
│   │   │   └── 🌍 Sub-feature (Planet)
│   │   │
│   │   └── 🪐 Feature (Solar System)
│   │
│   └── ⭐ Domain (Galaxy)
│
└── 🌀 Application (Galaxy Cluster)
```

**Quy tắc đồng dạng (Self-Similarity Rules) — áp dụng ở MỌI cấp:**

| Quy tắc | Mô tả |
|:---|:---|
| **Boundary** | Mỗi cấp có ranh giới rõ ràng (folder, namespace, project, service) |
| **Interface** | Giao tiếp qua contract, không truy cập nội bộ |
| **Data Sovereignty** | Mỗi cấp sở hữu data riêng, cấp khác không xâm phạm |
| **Acceptance Test** | Xoá một đơn vị ở bất kỳ cấp nào → cấp cha vẫn hoạt động |
| **Independence** | Thêm/xoá đơn vị mới → không sửa đơn vị cùng cấp |

**Cùng 1 acceptance test ở mọi cấp:**

```
Cấp System    : Xoá 1 Application  → System vẫn build (các app khác vẫn OK)
Cấp Domain    : Xoá 1 Feature group → App vẫn build (các domain khác vẫn OK)
Cấp Feature   : Xoá 1 Sub-feature   → Feature vẫn build (các sub khác vẫn OK)
```

**Khi nào zoom in (tách) vs zoom out (merge)?**

| Zoom In (tách sub-universe) | Zoom Out (merge) |
|:---|:---|
| Đơn vị > 500 lines | Đơn vị < 50 lines |
| Class > 10 public methods | 2 đơn vị share > 70% logic |
| Service trộn 2+ concerns | Folder chỉ có 1 file |
| Folder chứa > 10 files | Overhead quản lý > benefit |

### 9.6 Swappable Data Layer — "Thay Nhiên Liệu Không Đổi Tàu"

> *Tàu vũ trụ có thể thay đổi hệ thống nhiên liệu (từ hoá học → ion → fusion) mà không phải thiết kế lại toàn bộ tàu — miễn interface nạp nhiên liệu giữ nguyên.*

**Nguyên lý**: Business logic **không biết** data đến từ đâu (ORM, raw SQL, API, cache). Data access layer là **swappable** — đổi implementation mà không sửa logic.

```
┌─────────────────────────────────────────────────┐
│  Service Layer (Business Logic)                  │
│  ────────────────────────────                    │
│  Chỉ biết: "tôi cần data X" và "tôi trả kết quả Y" │
│  Không biết: data đến từ ORM, SQL, API, hay cache│
├─────────────────────────────────────────────────┤
│  Data Access Layer (Infrastructure)              │
│  ────────────────────────────                    │
│  Swappable: ORM ↔ Raw SQL ↔ API call ↔ In-memory│
│  Thay đổi ở đây → Service Layer không bị ảnh hưởng│
└─────────────────────────────────────────────────┘
```

**Cách tổ chức data access — tuỳ coding standards của từng project:**

| Cách | Ưu điểm | Nhược điểm |
|:---|:---|:---|
| ORM (EF, L2S, GORM) | Type-safe, navigation | Coupling ORM vendor |
| Raw SQL + mapping | Portable, tối ưu được | Thiếu compile-time check |
| Repository pattern | Abstract hoàn toàn | Thêm abstraction layer |
| Queries constants (partial/file riêng) | Tập trung, dễ tìm, dễ swap | Cần discipline |

> **uarch không bắt buộc** dùng cách nào — chỉ yêu cầu: data access **phải tách biệt** khỏi business logic, và **phải swappable** khi đổi infrastructure.

---

## 10. Tóm Tắt Hướng Đột Phá

| Concept | Kiến trúc hiện tại | Universe Architecture 2.0 |
|:---|:---|:---|
| **Đo lường** | Manual review | **Fitness Functions** tự động trong CI |
| **Optimization** | Cùng strategy cho mọi module | **Performance Gravity** — vary per module weight |
| **Prevention** | Fix bug sau khi xảy ra | **Self-Adaptive** — phát hiện sớm module bloat |
| **Multi-platform** | Mỗi platform thiết kế riêng | **Universe Topology** — shared core, thin clients |
| **Evolution** | Refactor lớn theo cycle | **Continuous Evolution** — tiến hóa tự nhiên |
| **Scale** | Flat structure, 1 cấp | **Fractal Universe** — self-similar ở mọi cấp |
| **Migration** | ORM lock-in | **Migration-Ready Queries** — swap data layer, giữ SQL |

> **Verdict**: Khi kết hợp Mental Model + Fitness Functions + Performance Gravity + Universe Topology, Universe Architecture trở thành framework **đo lường được, tự phát hiện vấn đề, tự hướng dẫn optimization** — vượt xa Microkernel, VSA, hay Clean Architecture vốn chỉ là static structural patterns.
