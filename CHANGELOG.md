# Changelog

All notable changes to the Universe Architecture project will be documented in this file.

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
