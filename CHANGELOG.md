# Changelog

All notable changes to the Universe Architecture project will be documented in this file.

## [4.0.0] - 2026-03-27

### Added
- **Hot-Reload Plugin System**: `PluginLoader` (isolated `AssemblyLoadContext`), `PluginWatcher` (auto-detect DLL via `FileSystemWatcher`), `LoadPlugin()`/`UnloadPlugin()` on `ModuleRegistry`.
- **Temporal Workflows (Saga Pattern)**: `IWorkflowModule`, `WorkflowEngine` with JSON checkpoint persistence, resume after crash, and backward compensation (rollback).
- **Package Layering Guide**: Documentation clarifying that Monolith projects only need `Universe.Core`; `Workflows`, `Plugins`, `Distributed` layers are fully opt-in.
- **SamplePlugin**: External `WeatherModule` DLL demonstrating hot-reload capability.
- **OrderWorkflow**: Demo workflow (CreateOrder → ChargePayment → ShipItem) with Saga compensation.

### Changed
- `ModuleRegistry` now supports `UnregisterAsync()` for graceful module removal with lifecycle hooks.
- `RabbitMqEventBusAdapter` updated to match full `IEventBus` interface (`Subscribe`/`Unsubscribe`).
- `.csproj` restructured to exclude `Plugins/` and `tests/` subdirectories from main compilation.

## [3.0.0] - 2026-03-26

### Added
- **Event Bus (`IEventBus`)**: Implement in-process Pub/Sub for indirect module communication (Principle #5).
- **Service Container (`IServiceContainer`)**: Added lightweight Dependency Injection container embedded into the Registry.
- **Middleware Pipeline (`IMiddleware`)**: Implemented Chain of Responsibility pattern for cross-cutting aspect injection (Logging, Timing, ErrorHandling).
- **Module Lifecycle (`IModuleLifecycle`)**: Standardized hooks (`OnInitializing`, `OnInitialized`, `OnShuttingDown`, `OnShutdown`) for graceful resource management.
- **Async Execution**: Support for `IAsyncModule` providing asynchronous task execution natively.
- **Nested Registries (Fractal Universe)**: Added `INestedModule` allowing modules to contain their own sub-registries for massive scaling.
- **Sandboxing & ACL**: Implemented `ISecureModule` and `AccessControlMiddleware` to secure command dispatching.
- **Enterprise Documentation**: Added Architecture Decision Records (ADR), Security Model, Scaling Topology, and deep Benchmark Reports.
- **Multi-language Support**: Upgraded Go, TypeScript, and Python implementations to reach parity with v3 C# conceptual models.

### Changed
- `ModuleRegistry` routing algorithm rewritten to support `.` namespace parsing (e.g., `hr.employee.salary`).
- Replaced traditional monolithic `Execute` with pipeline-wrapped interceptors.
- Major updates to `universe-plugin.md` and `universe-plugin.vi.md` adding new principles mapping.
