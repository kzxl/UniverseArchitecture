# Architecture Decision Records (ADRs)
**Project**: Universe Architecture v3.0

This document records the critical architectural decisions to prove Universe Architecture meets constraints.

## ADR-001: Module Registry vs Hard-coded routes
**Context**: Traditional architectures use switch/if-else logic, requiring constant modifications to the Router file. This violates the Open-Closed Principle.
**Decision**: Use `ModuleRegistry` for automatic discovery.
**Consequences**: Maximized extensibility (Zero-change Extension). O(1) string lookup guarantees consistent performance regardless of scale.

## ADR-002: In-Process Publish/Subscribe via EventBus
**Context**: Direct API calls between modules create tight coupling.
**Decision**: Apply an `IEventBus` (Gravitational Waves) for asynchronous communication.
**Consequences**: Perfect readiness for Distributed Architecture (Microservices) by simply swapping the EventBus implementation (e.g., Kafka).

## ADR-003: "Fractal Universe" - Nested Registries
**Context**: An enterprise system with thousands of functions will overflow a flat routing namespace.
**Decision**: Modules can contain their own SubRegistries via `INestedModule` (e.g., `hr.salary`).
**Consequences**: Natural Domain-Driven Design (Bounded Context) boundaries inside the monolith.

## ADR-004: "Gravity" Middleware Pipeline
**Context**: Cross-cutting concerns like Auth and Logging are scattered across APIs.
**Decision**: Wrap all dispatched commands in a Middleware Chain of Responsibility.
**Consequences**: Pure business code. Centralized aspect injection.
