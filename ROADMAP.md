# 🌌 Universe Architecture — Roadmap

> Cập nhật: 2026-03-30 | Version hiện tại: v4.0

## ✅ Đã hoàn thành (v4.0)

- [x] **Enterprise Scenario Demo** — Sales→Inventory→Notification flow (4 ngôn ngữ)
- [x] **Conformance Test Suite** — `spec.yaml` + runners C#/Go/Python/TypeScript (63 tests, ALL PASS)
- [x] **Technical Fixes** — Sync fast-path, pipeline consistency, CancellationToken, async dispatch Python

---

## 📋 Backlog (xếp theo ưu tiên)

### 🔴 P1 — High Impact, nên làm sớm

#### Feature Parity — Port Advanced Features sang Go/TS/Python
> Hiện tại C# "ahead" quá xa → claim "cross-platform" yếu.

| Feature | C# | Go | TS | Python | Effort |
|---------|:--:|:--:|:--:|:------:|:------:|
| INestedModule (Fractal Universe) | ✅ | ❌ | ❌ | ❌ | Medium |
| ISecureModule + ACL Middleware | ✅ | ❌ | ❌ | ❌ | Medium |
| ServiceContainer (DI) | ✅ | ❌ | ❌ | ❌ | Low |
| IAsyncModule | ✅ | ❌ | ❌ | ❌ | Low |
| IWorkflowModule + Saga | ✅ | ❌ | ❌ | ❌ | High |
| PluginLoader (hot-reload) | ✅ | ❌ | ❌ | ❌ | High |

**Ưu tiên**: NestedModule + ISecureModule là 2 "unique selling points" — port trước.

---

#### Migration Cookbook — Chuyển Đổi Dự Án Thực
> Step-by-step guide chuyển monolith cũ sang Universe Architecture.

- Phase 1: Identify Boundaries (vẽ dependency graph, xác định God Classes)
- Phase 2: Extract Core (tạo IModule, ModuleRegistry, wrap existing code — Strangler Fig)
- Phase 3: Decouple Communication (thay direct references bằng EventBus)
- Phase 4: Add Middleware (extract auth/logging khỏi modules)
- Phase 5: Validate (Acceptance + Fitness Functions)
- Kèm case study thực từ MDS/RAF

**Giá trị**: Rất cao cho adoption, nhưng cần case study thực tế.

---

### 🟡 P2 — Medium Impact

#### `universe` CLI Tool — Scaffolding Generator
> CLI tự động tạo module, middleware, workflow theo template chuẩn.

```bash
universe new module --name inventory --lang csharp
universe new middleware --name rate-limiter --lang go
universe init --name my-erp --lang csharp --layers core,workflows
universe check --path ./src   # Fitness Functions
```

**Deliverables**:
- [ ] CLI framework (Go hoặc Node)
- [ ] Template files cho 4 ngôn ngữ
- [ ] `universe check` — static analysis scanner
- [ ] README + install guide

---

#### Architecture Fitness Dashboard
> Hiện thực hóa concept "Fitness Functions" (Section 9.1 trong docs) thành CI/CD tool.

| Metric | Cách đo | Ngôn ngữ |
|--------|---------|----------|
| Module Coupling Score | Scan import/using → detect cross-module references | Roslyn (C#), AST (Go/TS/Py) |
| Feature Isolation | Xóa 1 module folder → build pass? | CI script |
| Data Sovereignty | Grep cross-module DB access | Regex |
| God Class Detection | Lines per class > threshold | Script |
| Circular Dependency | Dependency graph cycle detection | Custom |

**Output**: JSON report → terminal dashboard hoặc web UI.

---

#### Package Publishing
> Publish Universe.Core thành package chính thức.

| Package | Registry | Command |
|---------|----------|---------|
| `Universe.Core` | NuGet | `dotnet add package Universe.Core` |
| `@universe-arch/core` | npm | `npm install @universe-arch/core` |
| `universe-arch` | PyPI | `pip install universe-arch` |
| `github.com/kzxl/universe-arch/core` | Go Modules | `go get ...` |

**Package layering** (opt-in):
- `Universe.Core` — bắt buộc
- `Universe.Workflows` — opt-in
- `Universe.Plugins` — opt-in (.NET only)
- `Universe.Distributed` — opt-in

---

### 🟢 P3 — Nice to Have

#### Interactive Playground Website
> Web app cho phép "chơi" với Universe Architecture ngay trên browser.

- **Live sandbox** — Viết module, register, dispatch → output realtime (TS client-side)
- **Visual Registry Graph** — Module tree dạng Galaxy map (D3.js/Three.js)
- **Middleware Pipeline Visualizer** — Animation request đi qua từng middleware
- **Benchmark Runner** — Chạy benchmark trên browser

**Tech**: Vite + TypeScript version + D3.js | Effort: High

---

#### Module Dependency Graph Visualization
> Auto-generate Mermaid/D3 graph từ codebase thực.

- Scan source code → detect module registrations + EventBus subscriptions
- Generate interactive diagram: modules = nodes, events = edges
- Violations (direct Module→Module import) = red arrows
- Integrate vào `universe check` CLI

---

## 🔧 Technical Debt

| Item | Mô tả | Priority |
|------|--------|:--------:|
| Go EventBus `reflect` | Có thể refactor sang Go Generics pattern compile-safe hơn | Low |
| C# `Dispatch()` sync fallback | Vẫn `GetAwaiter().GetResult()` khi có middleware — document rõ limitation | Low |
| Pre-existing test failure | `CalculatorModule.Execute_UnknownCommand_ReturnsError` — args check trước switch | Low |
| TS middleware sync-only | TS middleware `invoke()` đồng bộ, không hỗ trợ async middleware | Medium |
| Python middleware sync-only | Tương tự TS — cần async middleware support | Medium |

---

## 📊 Metrics hiện tại

| Metric | Value |
|--------|-------|
| Core interfaces (C#) | 13 |
| Enterprise modules | 5 (Calculator, Greeter, Inventory, Sales, Notifier) |
| Languages supported | 4 (C#, Go, TypeScript, Python) |
| Conformance tests | 63 (18+14+15+16) |
| Benchmark phases | 6 |
