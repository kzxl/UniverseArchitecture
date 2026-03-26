# 🌌 Universe Architecture — Infinite Extensibility Without Breaking Changes

> The universe expands infinitely, discovering new civilizations without breaking the laws of physics.
> Applied to software: add new modules without breaking existing architecture.

🇻🇳 [Bản Tiếng Việt](universe-plugin.vi.md)

---

## 1. Philosophy

### Why the Universe Expands Without Breaking

4 fundamental forces (Gravity, EM, Strong, Weak) = the universe's **Interfaces**. Any "module" (galaxy, civilization) obeys the _same laws_, so the universe expands infinitely without conflicts.

```
┌──────────────────── UNIVERSE ─────────────────────┐
│                                                    │
│  LAWS OF PHYSICS  →  SPACETIME    →  CIVILIZATIONS │
│  (Interfaces)        (Infra)         (Modules)     │
│   Immutable          Habitat         Self-forming  │
│                                                    │
└────────────────────────────────────────────────────┘
```

| Universe | Software | Explanation |
|----------|----------|-------------|
| Laws of physics | **Core Interfaces** | Immutable, everything obeys |
| Spacetime | **Shared Infrastructure** | Foundation modules "live" on |
| Galaxy | **Module Category** | Related group, independent |
| Star | **Individual Module** | Self-contained unit |
| Black hole | **Anti-pattern** (God class) | Swallows everything around |

### vs. Traditional Plugin Architecture

Traditional plugins suffer from version mismatch and shared resource conflicts.
**Universe model** overcomes this through:
1. **Stable interfaces** — backward-compatible forever
2. **Isolated modules** — runs in its own "space"
3. **Core doesn't know how many modules exist** — no hard-coding
4. **Shared infra = gravity** — pulls everything into place

---

## 2. Eight Core Principles

### #1: Core Stable — Modules Volatile
Core (interfaces, base classes, shared services) rarely changes. Modules are free to add/edit/delete.

> *In 3.0, the `IServiceContainer` acts as the Space Station — a lightweight Dependency Injection container that resolves module dependencies without tying them to a specific 3rd party framework (like Autofac or MS.DI).*

### #2: Module Independence
```
✅ Module → Core/Shared  (OK)
❌ Module → Module       (FORBIDDEN — extract shared logic into Core)
```

### #3: Contract-First
Every module obeys the same interface (metadata + lifecycle + UI factory).

### #4: Registry — Not Hard-coded
Modules self-register. Core doesn't know module details.

### #5: Indirect Communication (EventBus)
Modules communicate via **Event Bus / Mediator**, no direct imports.
> *In 3.0, Sóng hấp dẫn (Gravitational Waves) is implemented via `IEventBus`.*

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

### #6: Data Sovereignty — Modules Own Their Data
> *Each galaxy has its own star system — doesn't invade another galaxy's space.*

Module A **does not directly access** Module B's tables/entities. Need data from another module → call through **Service interface**.

```
✅ OrderService calls ICustomerService.GetById(id)     (OK — via interface)
❌ OrderService queries tbCAT_Customer directly         (FORBIDDEN — sovereignty violation)
```

**Applied by platform:**

| Platform | Data Sovereignty |
|----------|-----------------|
| Web API | Each feature folder has its own DTOs; Service only queries tables belonging to its feature |
| WinForms | Controller only injects its own feature's Service; other data via Shared Service |
| Microservices | Each service has its own DB schema/database |

### #7: Middleware = Gravity & Module Lifecycle (Star Lifecycle)
> *Gravity affects ALL objects in the universe without objects needing to know about it. Stars also have lifecycles (birth, death).*

#### A. Middleware Pipeline (Gravity)
Cross-cutting concerns (Auth, Logging, Compression, Error Handling) are handled in the **Middleware layer** — automatically applied to every request/action without polluting module business logic.

```csharp
public interface IMiddleware {
    Task Invoke(ModuleContext context, Func<Task> next);
}

// Setup Pipeline in Core:
registry.AddMiddleware(new AuthenticationMiddleware());
registry.AddMiddleware(new LoggingMiddleware());
registry.AddMiddleware(new ErrorHandlingMiddleware());
// Automatically applies to all Dispatch() calls.
```

**Key rules:**
- Module **MUST NOT contain** auth check, logging boilerplate, compression logic.
- Middleware **MUST NOT contain** business logic.
- Adding new middleware = 1 line of registration.

#### B. Module Lifecycle (Star Lifecycle)
Modules often need to manage resources (DB connections, background threads, socket listeners). The `IModuleLifecycle` allows modules to gracefully start up and shut down.

```csharp
public interface IModuleLifecycle {
    Task OnInitializing();      // Before registration
    Task OnInitialized();       // After registration
    Task OnShuttingDown();      // Before unload
    Task OnShutdown();          // After unload
}
```

### #8: Migration Path — From Galaxy to Sub-Universe
> *When a galaxy grows too large, it can split into an independent sub-universe.*

A module meeting conditions can **split into a separate service/process** (microservice) without architectural refactoring.

**Split conditions:**
1. ✅ Passes Acceptance Test (delete folder → still builds)
2. ✅ Data Sovereignty applied (no querying other module's tables)
3. ✅ Communication via Interface (no hard-coded dependency)
4. ✅ Has independent deployment artifact (separate project/package)

**3-Level roadmap:**
```
Level 1: Modular Monolith
  └── Modules in same process, communicate via DI

Level 2: In-Process Event Bus
  └── Modules communicate via MediatR / IEventAggregator
  └── Prepared to split but not yet separated

Level 3: Distributed (Microservices)
  └── Modules run in separate processes
  └── Communication via Message Queue (RabbitMQ, Kafka)
  └── Each module has its own DB
```

---

## 3. Platform Mapping

### 3.1 WinForms (.NET Framework)

| Universe Layer | WinForms Implementation |
|---------------|------------------------|
| **Laws (Interface)** | `IFeatureController` — contract for all modules |
| **Spacetime (Infra)** | `BaseForm`, DI Container, SharedLib, Themes |
| **Module** | Feature folder: Controller + Service + Views |
| **Registry** | DI assembly scanning + `ShowUserControl` |
| **Communication** | `IEventAggregator` / delegate callbacks |

### 3.2 ASP.NET Web API

| Universe Layer | Web API Implementation |
|---------------|----------------------|
| **Laws** | `BaseApiController`, `ApiResponse<T>`, `IGenericService<T>` |
| **Spacetime** | `Infrastructure/` (DI, JWT, Swagger, GZip) |
| **Module** | `Features/{Module}/{Feature}/` (vertical slice) |
| **Registry** | DI assembly scanning |
| **Communication** | Service injection — no cross-feature |

### 3.3 ASP.NET Core

```csharp
// Feature module interface
public interface IFeatureModule
{
    string Name { get; }
    void RegisterServices(IServiceCollection services);
    void MapRoutes(WebApplication app);
}
```

### 3.4 WPF (.NET 8)

| Universe Layer | WPF Implementation |
|---------------|-------------------|
| **Laws** | `ITool` interface, `ToolViewModelBase` |
| **Spacetime** | `Core/` (EngineRunner, ExportService, Themes) |
| **Module** | `Modules/{Name}/` (ViewModel + View + Models) |
| **Registry** | `ToolRegistry` → TabControl auto-populate |

### 3.5 Go CLI

| Universe Layer | Go Implementation |
|---------------|------------------|
| **Laws** | `Command` interface (Name, Run) |
| **Spacetime** | `shared/` (output writer, config loader) |
| **Module** | `cmd/<name>.go` — one file per command |
| **Registry** | `init()` function auto-register |

### 3.6 React + Node.js

| Universe Layer | React Implementation |
|---------------|---------------------|
| **Laws** | `IPlugin` interface (TS), Redux slice contract |
| **Spacetime** | Redux store, Axios, Socket.io, Theme |
| **Module** | `features/<name>/` (component + slice + API) |
| **Registry** | Route config array, dynamic UI generation |

### 3.7 PHP

```
app/
├── core/               ← Laws
│   ├── Module.php       Interface
│   ├── Registry.php     Auto-discover
│   └── Database.php     Shared DB
├── modules/            ← Civilizations
│   ├── auth/
│   ├── inventory/
│   └── report/
└── shared/             ← Spacetime
    ├── helpers.php
    └── config.php
```

---

## 4. Anti-Patterns & Solutions

| ❌ Anti-Pattern | ✅ Universe Solution | Principle |
|----------------|---------------------|:---------:|
| Module A imports Module B | Event Bus / Mediator | #5 |
| Core contains module logic | Delegate to module | #1 |
| Hard-coded module list | Registry auto-discover | #4 |
| God ViewModel/Controller (1000+ lines) | 1 VM/Controller per module + partial class | #2 |
| Shared mutable state | Immutable messages | #5 |
| Service A queries Module B's table | Call via IServiceB interface | #6 |
| Auth/Log code scattered in modules | Centralize in Middleware pipeline | #7 |
| Monolith can't be split | Apply #6 + #5 first, split gradually | #8 |

---

## 5. Acceptance Tests

### Test 1: Module Isolation (Basic)
> **Delete an entire module folder → app still builds & runs.**
> If fails → coupling exists → refactor needed.

### Test 2: Data Sovereignty
> **Grep all Service files in Module A → no reference to Module B's Entity/Table.**
> If fails → Data Sovereignty violation → extract via interface.

### Test 3: Middleware Independence
> **Delete 1 middleware handler → app still runs** (only loses that cross-cutting feature).
> If fails → middleware contains business logic → needs splitting.

### Test 4: Migration Readiness
> **Module can run as a separate project** (with mock/stub dependencies).
> If fails → not ready for microservice extraction.

---

## 6. Full Conversion Table

| Universe | Software Layer | WinForms / C# | Go | TypeScript | Python |
|----------|---------------|---------------|----|------------|--------|
| **Laws of Physics** | Core Interfaces | `IModule` | `Module` | `IModule` | `IModule` |
| **Spacetime** | Shared Infra | `Program.cs` / `SharedLib` | `main.go` / `shared/` | `index.ts` | `main.py` |
| **Galaxy** | Module Group | Feature folder | `modules/` | `src/modules/` | `modules/` |
| **Star** | Module Unit | `NotifierModule` | `notifier` pkg | `NotifierModule` class | `NotifierModule` cls |
| **Registry** | Auto-discover | `ModuleRegistry` | `core.Registry` | `ModuleRegistry` | `ModuleRegistry` |
| **Gravity** | Middleware | `IMiddleware` | `Middleware` | `IMiddleware` | `IMiddleware` |
| **Star Lifecycle** | Lifecycle Hooks | `IModuleLifecycle` | `ModuleLifecycle`| `IModuleLifecycle` | `IModuleLifecycle` |
| **Gravitational Waves**| Event Bus | `IEventBus` | `EventBus` | `EventBus` | `EventBus` |
| **Space Station** | Service Container| `IServiceContainer` | N/A (Manual DI) | N/A (Manual DI) | N/A (Manual DI) |
| **Own Star System** | Data Sovereignty | Feature DB tables | pkg DB access | slice state | module schema |
| **Sub-Universe** | Microservice | Separate project | Separate app | Micro-frontend | Separate app |

---

## 7. Module Communication Levels

> *From light signals (same galaxy) to gravitational waves (cross-universe).*

```
┌─────────────────────────────────────────────────────────┐
│  Level 1: Direct Injection (same process)               │
│  Simplest. CustomerService injects IOrderService.       │
│  Use when: Modules ALWAYS deploy together.              │
├─────────────────────────────────────────────────────────┤
│  Level 2: In-Process Event Bus                          │
│  OrderService publishes OrderCreatedEvent →             │
│  InventoryService subscribes → auto deduct stock.       │
│  Use when: Modules MAY split in the future.             │
├─────────────────────────────────────────────────────────┤
│  Level 3: Message Queue (cross-process)                 │
│  OrderService → RabbitMQ → InventoryService.            │
│  Use when: Modules ALREADY split into services.         │
└─────────────────────────────────────────────────────────┘
```

**Rule of thumb:**
- Start with **Level 1** — simplest
- Move to **Level 2** when 2+ modules react to the same event
- Move to **Level 3** only when independent deployment/scaling is needed

---

## 8. Origins & References

Universe Architecture is a practical synthesis of these public architectures:

| Universe Principle | Origin |
|:---|:---|
| #1 Core Stable, Modules Volatile | **Microkernel / Plugin Architecture** (POSA, 1996) |
| #2 Module Independence | **Modular Monolith** (Milan Jovanović) |
| #3 Contract-First | **Interface Segregation Principle** (SOLID) |
| #4 Registry | **Service Locator / Plugin Registry** (GoF, POSA) |
| #5 Indirect Communication | **Mediator Pattern** (GoF) + **Event-Driven Architecture** |
| #6 Data Sovereignty | **Bounded Context** (DDD, Eric Evans) |
| #7 Middleware | **Pipeline Pattern** + **Chain of Responsibility** (GoF) |
| #8 Migration Path | **Strangler Fig Pattern** (Martin Fowler) |
| Folder-per-Feature | **Vertical Slice Architecture** (Jimmy Bogard, 2018) |
| Dependency direction | **Clean Architecture** dependency rule (Uncle Bob, 2012) |

**Unique value of Universe Architecture:**
- 🌌 **Universe mental model** — turns abstract concepts into intuitive visuals
- 🌐 **Cross-platform consistency** — same model for WinForms, Web API, WPF, Go, React, PHP
- ✅ **Simple Acceptance Test** — "Delete folder → still builds" easier to verify than any metric
- 📊 **Conversion Table** — practical cross-reference across 6+ platforms

---

## 9. 🚀 Breakthrough: Evolutionary & Performance-First

> *The universe doesn't just expand — it self-measures, self-optimizes, self-evolves.*
> These are concepts **NO public architecture** integrates into a unified mental model.

### 9.1 Architecture Fitness Functions — "Universal Constants"

> *Every universe has physical constants (speed of light, Planck's constant). Change one constant → universe collapses.*
> *Every software system has "architecture constants" — violate them → system degrades.*

**Fitness Function** = automated test measuring architecture health continuously, running in CI/CD.

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

### 9.2 Performance Gravity — "Mass Determines Orbit"

> *Heavier objects → stronger gravity → dominates surrounding motion.*
> *Hotter modules (more requests/data) → need differentiated optimization.*

```
Module Performance Classification:

☀️ Star          — High freq, High volume  → Cache + Async + Scale independently
🪐 Planet        — Medium freq             → Standard optimization
🌑 Moon          — Low freq, admin tasks   → Code optimization is enough
```

**Key insight**: Not every module needs the same optimization level. Universe Architecture allows **varying strategies per module** while adhering to the same interface.

### 9.3 Self-Adaptive — "Natural Evolution"

> *The universe self-evolves — small stars become giants then black holes (if unchecked).*
> *Modules also evolve — from simple → complex. Need early detection.*

```
Module Lifecycle Stages:

┌──────────┐    ┌───────────┐    ┌──────────┐    ┌─────────────┐
│ 🌱 Seed  │ →  │ 🌿 Growth │ →  │ ☀️ Mature │ →  │ 🕳️ Collapse │
│  < 200   │    │  200-500  │    │  500-1000│    │   > 1000    │
│  lines   │    │  lines    │    │  lines   │    │   lines     │
│          │    │           │    │          │    │  = GOD CLASS │
└──────────┘    └───────────┘    └──────────┘    └─────────────┘
                                      ↓ (early detection)
                                 ┌──────────┐
                                 │ 🔀 Split │ Split into 2+ modules
                                 └──────────┘
```

### 9.4 Universe Topology — Multi-Client Architecture

> *A galaxy can be observed from many angles — telescope, radio waves, X-ray.*
> *Same domain logic, multiple client platforms looking in.*

```
                    ┌─────────────────────────────┐
                    │     🌌 UNIVERSE CORE         │
                    │     (Domain + Services)       │
                    └───────┬─────────────┬─────────┘
                            │             │
              ┌─────────────┼─────────────┼─────────────┐
              │             │             │             │
        ┌─────▼────┐  ┌────▼─────┐ ┌─────▼────┐ ┌─────▼────┐
        │ 🌐 API   │  │ 🖥️ Desk  │ │ 🎨 WPF  │ │ 🌍 Web   │
        │ Gateway  │  │  Client  │ │ Client   │ │ SPA      │
        └──────────┘  └──────────┘ └──────────┘ └──────────┘
```

Each client is just a **thin adapter** translating UI concepts into Domain calls.

### 9.5 Fractal Universe — "Universe Within Universe"

> *Zoom into a galaxy → see star systems. Zoom into a star system → see planets. Each level obeys the same laws of physics.*
> *Zoom into a software system the same way. **Each level is a universe** with the same structure.*

This is a principle **not found** in any public architecture pattern:
- DDD has only 2 levels (Bounded Context → Aggregate)
- Clean Architecture is 1 level (layers)
- Microkernel is 1 level (core + plugins)

**Fractal Universe scales to infinite levels**, each applying **the same rules**:

```
🌌 System (Universe)
│   Boundary: Solution / Monorepo
│
├── 🌀 Application (Galaxy Cluster)
│   │   Boundary: Project / Package
│   │
│   ├── ⭐ Domain (Galaxy)
│   │   │   Boundary: Feature group folder + _Shared/
│   │   │
│   │   ├── 🪐 Feature (Solar System)
│   │   │   │   Boundary: Service / Controller / View / DTOs
│   │   │   │
│   │   │   ├── 🌍 Sub-feature (Planet)
│   │   │   │   Boundary: Partial class or sub-folder
│   │   │   │
│   │   │   └── 🌍 Sub-feature (Planet)
│   │   │
│   │   └── 🪐 Feature (Solar System)
│   │
│   └── ⭐ Domain (Galaxy)
│
└── 🌀 Application (Galaxy Cluster)
```

**Self-Similarity Rules — apply at EVERY level:**

| Rule | Description |
|:---|:---|
| **Boundary** | Each level has clear boundaries (folder, namespace, project, service) |
| **Interface** | Communicate via contract, no internal access |
| **Data Sovereignty** | Each level owns its data, others don't invade |
| **Acceptance Test** | Delete a unit at any level → parent still works |
| **Independence** | Add/remove a new unit → no changes to sibling units |

**Same acceptance test at every level:**

```
System level  : Delete 1 Application   → System still builds (other apps OK)
Domain level  : Delete 1 Feature group  → App still builds (other domains OK)
Feature level : Delete 1 Sub-feature    → Feature still builds (other subs OK)
```

**When to zoom in (split) vs zoom out (merge)?**

| Zoom In (split sub-universe) | Zoom Out (merge) |
|:---|:---|
| Unit > 500 lines | Unit < 50 lines |
| Class > 10 public methods | 2 units share > 70% logic |
| Service mixes 2+ concerns | Folder has only 1 file |
| Folder contains > 10 files | Management overhead > benefit |

### 9.6 Swappable Data Layer — "Change Fuel Without Redesigning the Ship"

> *A spaceship can change its fuel system (chemical → ion → fusion) without redesigning the entire ship — as long as the fuel interface stays the same.*

**Principle**: Business logic **doesn't know** where data comes from (ORM, raw SQL, API, cache). Data access layer is **swappable** — change implementation without touching logic.

```
┌─────────────────────────────────────────────────┐
│  Service Layer (Business Logic)                  │
│  ────────────────────────────                    │
│  Only knows: "I need data X" and "I return Y"   │
│  Doesn't know: data comes from ORM, SQL, or API │
├─────────────────────────────────────────────────┤
│  Data Access Layer (Infrastructure)              │
│  ────────────────────────────                    │
│  Swappable: ORM ↔ Raw SQL ↔ API call ↔ In-memory│
│  Changes here → Service Layer unaffected         │
└─────────────────────────────────────────────────┘
```

**Data access organization — depends on each project's coding standards:**

| Approach | Pros | Cons |
|:---|:---|:---|
| ORM (EF, GORM, etc.) | Type-safe, navigation | Coupling to ORM vendor |
| Raw SQL + mapping | Portable, optimizable | No compile-time check |
| Repository pattern | Full abstraction | Extra abstraction layer |
| Query constants (partial/separate file) | Centralized, easy to find, easy to swap | Requires discipline |

> **uarch does not mandate** any specific approach — it only requires: data access **must be separated** from business logic, and **must be swappable** when changing infrastructure.

---

## 10. Summary of Breakthroughs

| Concept | Current Architecture | Universe Architecture 2.0 |
|:---|:---|:---|
| **Measurement** | Manual review | **Fitness Functions** automated in CI |
| **Optimization** | Same strategy for all modules | **Performance Gravity** — vary per module weight |
| **Prevention** | Fix bugs after they occur | **Self-Adaptive** — detect module bloat early |
| **Multi-platform** | Each platform designed separately | **Universe Topology** — shared core, thin clients |
| **Evolution** | Large refactor cycles | **Continuous Evolution** — natural progression |
| **Scale** | Flat structure, 1 level | **Fractal Universe** — self-similar at every level |
| **Migration** | ORM lock-in | **Swappable Data Layer** — swap infrastructure, keep logic |

> **Verdict**: When combining Mental Model + Fitness Functions + Performance Gravity + Universe Topology, Universe Architecture becomes a framework that is **measurable, self-detecting, self-guiding optimization** — far beyond Microkernel, VSA, or Clean Architecture which are merely static structural patterns.
