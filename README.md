# 🌌 Universe Architecture

> **Vũ trụ mở rộng vô hạn, phát hiện nền văn minh mới mà không phá vỡ quy luật vật lý.**
> Áp dụng cho phần mềm: thêm module mới mà không phá vỡ kiến trúc hiện tại.

A modular, extensible software architecture pattern inspired by the universe's expansion model.
Demonstrated across **4 languages** with identical behavior and benchmarks.

---

## 📖 What is Universe Architecture?

Universe Architecture (UA) là một **mô hình tổ chức phần mềm** lấy cảm hứng từ cách vũ trụ vận hành:

| Vũ trụ | Phần mềm | Giải thích |
|--------|----------|-----------|
| 🔬 Quy luật vật lý | **Core Interfaces** | Bất biến, mọi thứ tuân thủ |
| 🌌 Không-thời gian | **Shared Infrastructure** | Nền tảng modules "sống" trên |
| 🌀 Thiên hà | **Module Category** | Nhóm liên quan, độc lập |
| ⭐ Ngôi sao | **Individual Module** | Đơn vị tự chứa |
| 🕳️ Hố đen | **Anti-pattern** (God class) | Nuốt chửng xung quanh |

### Why Not Plugin Architecture?

Plugin truyền thống dễ bị version mismatch, conflict shared resource. **Universe Architecture** vượt qua nhờ:

1. **Interface ổn định** — backward-compatible
2. **Module cô lập** — chạy "không gian" riêng
3. **Core không biết có bao nhiêu module** — không hard-code
4. **Shared infra = gravity** — kéo mọi thứ về đúng chỗ

---

## 🧠 8 Core Principles

```
┌──────────────────── VŨ TRỤ ──────────────────────┐
│                                                    │
│  QUY LUẬT VẬT LÝ  →  KHÔNG GIAN  →  NỀN VĂN MINH │
│  (Interfaces)         (Infra)        (Modules)     │
│   Bất biến            Chỗ sống       Tự hình thành │
│                                                    │
└────────────────────────────────────────────────────┘
```

| # | Principle | Description |
|:-:|-----------|-------------|
| 1 | **Core Stable — Modules Volatile** | Core interfaces ít thay đổi. Modules thoải mái thêm/sửa/xóa |
| 2 | **Module Independence** | `Module → Core ✅` · `Module → Module ❌` |
| 3 | **Contract-First** | Mọi module tuân thủ cùng 1 interface (metadata + lifecycle) |
| 4 | **Registry — Not Hard-coded** | Module tự đăng ký. Core không biết chi tiết module |
| 5 | **Indirect Communication** | Module giao tiếp qua Event Bus / Mediator, không import trực tiếp |
| 6 | **Data Sovereignty** | Mỗi module sở hữu data riêng, không xâm phạm module khác |
| 7 | **Middleware = Gravity** | Cross-cutting concerns (Auth, Log) tác động mọi module tự động |
| 8 | **Migration Path** | Module đủ lớn → tách thành service riêng, zero refactor |

---

## 📁 Project Structure (Tất Cả Ngôn Ngữ Giống Nhau)

```
{language}/
├── core/                    ← Quy luật vật lý (bất biến)
│   ├── IModule              Interface mọi module tuân thủ
│   └── ModuleRegistry       Trung tâm đăng ký & dispatch
│
├── modules/                 ← Các nền văn minh (tự chứa)
│   ├── calculator/          Module tính toán
│   └── greeter/             Module chào hỏi
│
├── shared/                  ← Không-thời gian (infrastructure)
│   └── ConsoleHelper        Output utilities
│
└── main / Program           ← Entry point + benchmark
```

**Thêm module mới = 1 folder + 1 dòng register. Zero changes to existing code.**

---

## 🚀 Quick Start

### C# (.NET 8)

```bash
cd csharp
dotnet run
```

### Go

```bash
cd go
go run .
```

### TypeScript (Node.js)

```bash
cd typescript
npm install
npx tsx src/index.ts
```

### Python (3.10+)

```bash
cd python
python main.py
```

---

## 🔍 Core Pattern Comparison

### IModule Interface

<table>
<tr><th>C#</th><th>Go</th></tr>
<tr>
<td>

```csharp
public interface IModule
{
    string Name { get; }
    string Description { get; }
    IReadOnlyList<string> Commands { get; }
    string Execute(string command, string[] args);
}
```

</td>
<td>

```go
type Module interface {
    Name() string
    Description() string
    Commands() []string
    Execute(command string, args []string) string
}
```

</td>
</tr>
<tr><th>TypeScript</th><th>Python</th></tr>
<tr>
<td>

```typescript
export interface IModule {
  readonly name: string;
  readonly description: string;
  readonly commands: readonly string[];
  execute(command: string, args: string[]): string;
}
```

</td>
<td>

```python
class IModule(ABC):
    @property
    @abstractmethod
    def name(self) -> str: ...
    @property
    @abstractmethod
    def description(self) -> str: ...
    @abstractmethod
    def execute(self, command: str,
                args: list[str]) -> str: ...
```

</td>
</tr>
</table>

### Module Registration (1 Line Each)

```csharp
// C#
registry.Register(new CalculatorModule());
registry.Register(new GreeterModule());
// Thêm module mới? → registry.Register(new YourModule());
```

```go
// Go
registry.Register(&calculator.CalculatorModule{})
registry.Register(&greeter.GreeterModule{})
```

```typescript
// TypeScript
registry.register(new CalculatorModule());
registry.register(new GreeterModule());
```

```python
# Python
registry.register(CalculatorModule())
registry.register(GreeterModule())
```

### Registry Dispatch (Zero Hard-Code)

```
registry.Dispatch("calculator", "add", ["10", "25"])
→ "10 + 25 = 35"

registry.Dispatch("greeter", "hello", ["Universe"])
→ "👋 Hello, Universe! Welcome to the Universe!"
```

---

## ⚡ Performance Benchmark

Benchmark: **1,000,000 registry dispatch operations** (calculator add 1 2)

| Language | Ops/sec (Calculator) | Ops/sec (Greeter) | Relative |
|----------|:--------------------:|:-----------------:|:--------:|
| **TypeScript** | ~22,000,000 | ~16,500,000 | 🥇 Fastest |
| **C# .NET 8** | ~1,600,000 | ~10,800,000 | 🥈 |
| **Go** | ~2,300,000 | ~5,400,000 | 🥉 |
| **Python** | ~735,000 | ~2,000,000 | 4th |

> **Note**: TypeScript's V8 JIT compiler aggressively optimizes hot loops, explaining its lead.
> C# and Go are comparable. Python is slower due to interpreter overhead but still handles 700K+ ops/sec.
> Benchmark chạy trên cùng 1 máy. Run each language's demo to see actual numbers on yours.

**Key insight**: Registry dispatch overhead là **negligible** ở mọi ngôn ngữ. Pattern không hy sinh performance — chi phí chính là string lookup trong Map/Dictionary, ~O(1).

---

## 🧪 Acceptance Test

### Test 1: Module Isolation
> **Xóa hoàn toàn 1 module folder → app vẫn build & chạy.**
> Nếu fail → còn coupling → cần refactor.

### Test 2: Zero Core Changes
> **Thêm module mới → không sửa code ở Core hay module khác.**
> Chỉ cần: 1 file module + 1 dòng register.

### Test 3: Consistent Behavior
> **Mọi ngôn ngữ cho cùng output** (trừ benchmark numbers).
> Chứng minh pattern là language-agnostic.

---

## 🏗️ Real-World Applications

Universe Architecture đã được áp dụng thành công trong:

| Project | Platform | Modules |
|---------|----------|:-------:|
| **NetTool** | WPF + Go | 11 network tools |
| **XTranslate** | WPF | Translation engines |
| **MDS Management** | WinForms | 16 business features |
| **MDS WebApi** | .NET Fx WebAPI | Vertical feature slices |
| **HelpDesk** | React + Express.js | Dynamic CRUD features |

### Adding a New Module (Real Example — NetTool)

```csharp
// 1. Create Modules/YourTool/YourTool.cs — implements ITool
// 2. Register in ShellViewModel.cs:
ToolRegistry.Register(new YourTool());
// Done! Tab appears automatically. Zero changes to other code.
```

---

## 📊 Pattern Comparison

| Feature | MVC | Plugin | Microservices | Universe Arch |
|---------|:---:|:------:|:-------------:|:------------:|
| Module independence | ⚠️ | ✅ | ✅ | ✅ |
| Zero-change extension | ❌ | ✅ | ✅ | ✅ |
| Single-process simplicity | ✅ | ✅ | ❌ | ✅ |
| Migration to microservice | ❌ | ⚠️ | ✅ | ✅ |
| Cross-platform consistency | ❌ | ❌ | ⚠️ | ✅ |
| Built-in acceptance test | ❌ | ❌ | ❌ | ✅ |
| Performance overhead | Low | Low | High | **Low** |

---

## 📚 Further Reading

- [Universe Architecture — Full Documentation](docs/universe-architecture.md)
- [Anti-Patterns & Solutions](docs/universe-architecture.md#anti-patterns)
- [Migration Path: Monolith → Microservices](docs/universe-architecture.md#migration-path)

---

## 📄 License

MIT

---

<p align="center">
  <strong>🌌 The Universe expands infinitely — so should your software.</strong>
</p>
