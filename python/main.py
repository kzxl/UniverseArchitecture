"""
═══════════════════════════════════════════════════════════
 🌌 Universe Architecture — Python Demo
 Full demo: Registry + EventBus + Middleware + Lifecycle
═══════════════════════════════════════════════════════════
"""
import asyncio
import time
import statistics

from core import ModuleRegistry, IModule
from core.eventbus import EventBus
from core.middleware import LoggingMiddleware, TimingMiddleware, ErrorHandlingMiddleware
from modules import CalculatorModule, GreeterModule
from modules.notifier import NotifierModule, CalculationPerformedEvent, GreetingEvent
from shared import print_header, print_result


async def main():
    registry = ModuleRegistry()

    # ── Middleware pipeline (Gravity) ──
    logging_mw = LoggingMiddleware()
    registry.add_middleware(ErrorHandlingMiddleware())
    registry.add_middleware(TimingMiddleware())
    registry.add_middleware(logging_mw)

    # ── Register modules ──
    registry.register(CalculatorModule())
    registry.register(GreeterModule())

    # Register NotifierModule with lifecycle
    notifier = NotifierModule(registry.event_bus)
    await registry.register_async(notifier)

    # ══════════════════ Info ══════════════════
    print_header("Universe Architecture — Python")

    print(f"\n  📦 Registered modules: {registry.count}")
    print(f"  🔗 Middleware pipeline: {registry.middleware_count} handlers")
    print(f"  📡 EventBus: {registry.event_bus.type_count} event types, {registry.event_bus.handler_count} handlers")

    for name, mod in registry.get_all().items():
        print(f"     • {name} — {mod.description} [{', '.join(mod.commands)}]")

    # ══════════════════ Demo ══════════════════
    print_header("Demo Commands (via Middleware Pipeline)")

    demos = [
        ("calculator", "add", ["10", "25"]),
        ("calculator", "sub", ["100", "37"]),
        ("calculator", "mul", ["7", "8"]),
        ("calculator", "div", ["22", "7"]),
        ("greeter", "hello", ["Universe"]),
        ("greeter", "goodbye", ["Developer"]),
    ]

    for module, cmd, args in demos:
        result = registry.dispatch(module, cmd, args)
        print_result(module, cmd, args, result)

        # Publish events for EventBus demo
        if module == "calculator":
            registry.event_bus.publish("CalculationPerformed",
                CalculationPerformedEvent(operation=f"{cmd} {' '.join(args)}", result=result))
        elif module == "greeter":
            registry.event_bus.publish("Greeting",
                GreetingEvent(name=args[0] if args else "World", message=result))

    # ══════════════════ EventBus Demo ══════════════════
    print_header("EventBus — Indirect Communication")

    print("\n  📡 Notifier received events from other modules:")
    print(registry.dispatch("notifier", "history", []))
    print(f"\n  {registry.dispatch('notifier', 'count', [])}")

    # ══════════════════ Middleware Logs ══════════════════
    print_header("Middleware Pipeline — Gravity Logs")

    print(f"\n  📝 Logging middleware captured {len(logging_mw.logs)} dispatch(es):")
    for log in logging_mw.logs[:5]:
        print(f"     {log}")
    if len(logging_mw.logs) > 5:
        print(f"     ... and {len(logging_mw.logs) - 5} more")

    # ══════════════════ Lifecycle Demo ══════════════════
    print_header("Module Lifecycle — Star Lifecycle")

    print("\n  🌟 Shutting down all modules with lifecycle hooks...")
    await registry.shutdown()
    print("  ✅ All lifecycle modules shut down gracefully.")

    # ══════════════════════════════════════════════════════════
    #  ⚡ DETAILED BENCHMARK
    # ══════════════════════════════════════════════════════════
    print_header("Detailed Performance Benchmark")

    WARMUP = 10_000
    BENCH = 1_000_000
    LATENCY_SAMPLES = 10_000

    # Fresh registry without middleware for benchmark
    bench_registry = ModuleRegistry()
    bench_registry.register(CalculatorModule())
    bench_registry.register(GreeterModule())

    # ── Phase 1: Warmup ──
    print(f"\n  🔥 Phase 1: Warmup...")
    for _ in range(WARMUP):
        bench_registry.dispatch("calculator", "add", ["1", "2"])
        bench_registry.dispatch("greeter", "hello", ["World"])
    print(f"     {WARMUP:,} warmup iterations completed")

    # ── Phase 2: Throughput ──
    print("\n  📊 Phase 2: Throughput (1M iterations each)")
    print("  ┌────────────────────────────────┬────────────┬──────────────┐")
    print("  │ Scenario                       │ Time (ms)  │ Ops/sec      │")
    print("  ├────────────────────────────────┼────────────┼──────────────┤")

    run_throughput("calculator add 1 2", lambda: bench_registry.dispatch("calculator", "add", ["1", "2"]), BENCH)
    run_throughput("calculator mul 7 8", lambda: bench_registry.dispatch("calculator", "mul", ["7", "8"]), BENCH)
    run_throughput("calculator div 22 7", lambda: bench_registry.dispatch("calculator", "div", ["22", "7"]), BENCH)
    run_throughput("greeter hello World", lambda: bench_registry.dispatch("greeter", "hello", ["World"]), BENCH)
    run_throughput("greeter goodbye Dev", lambda: bench_registry.dispatch("greeter", "goodbye", ["Dev"]), BENCH)

    print("  └────────────────────────────────┴────────────┴──────────────┘")

    # ── Phase 3: Latency Distribution ──
    print(f"\n  📈 Phase 3: Latency Distribution ({LATENCY_SAMPLES:,} samples)")

    calc_lat = measure_latencies(lambda: bench_registry.dispatch("calculator", "add", ["1", "2"]), LATENCY_SAMPLES)
    greet_lat = measure_latencies(lambda: bench_registry.dispatch("greeter", "hello", ["World"]), LATENCY_SAMPLES)

    print("  ┌────────────────────┬──────────┬──────────┬──────────┬──────────┬──────────┐")
    print("  │ Scenario           │ Min (ns) │ Avg (ns) │ P50 (ns) │ P95 (ns) │ P99 (ns) │")
    print("  ├────────────────────┼──────────┼──────────┼──────────┼──────────┼──────────┤")
    print_latency_row("calculator add", calc_lat)
    print_latency_row("greeter hello", greet_lat)
    print("  └────────────────────┴──────────┴──────────┴──────────┴──────────┴──────────┘")

    # ── Phase 4: Scalability ──
    print("\n  🔬 Phase 4: Registry Scalability")
    print("  ┌──────────────┬──────────────┬──────────────┐")
    print("  │ # Modules    │ Dispatch/sec │ Overhead     │")
    print("  ├──────────────┼──────────────┼──────────────┤")

    base_ops = bench_ops(bench_registry, BENCH // 10)

    for n in [10, 40, 70, 100]:
        scaled = ModuleRegistry()
        scaled.register(CalculatorModule())
        for j in range(1, n):
            scaled.register(DummyModule(f"dummy_{j}"))

        ops = bench_ops(scaled, BENCH // 10)
        overhead = (base_ops / ops - 1) * 100
        oh_str = f"+{overhead:.1f}%" if overhead > 0.5 else "baseline"
        print(f"  │ {n:<12} │ {ops:>12,} │ {oh_str:<12} │")

    print("  └──────────────┴──────────────┴──────────────┘")

    # ── Phase 5: EventBus Throughput ──
    print("\n  📡 Phase 5: EventBus Throughput")
    bench_bus = EventBus()
    bench_bus.subscribe("BenchEvent", lambda _: None)

    for _ in range(WARMUP):
        bench_bus.publish("BenchEvent", {"v": 1})

    print("  ┌────────────────────────────────┬────────────┬──────────────┐")
    print("  │ Scenario                       │ Time (ms)  │ Ops/sec      │")
    print("  ├────────────────────────────────┼────────────┼──────────────┤")

    run_throughput("eventbus publish (1 sub)", lambda: bench_bus.publish("BenchEvent", {"v": 1}), BENCH)

    print("  └────────────────────────────────┴────────────┴──────────────┘")

    print()
    print("  ✅ All benchmarks completed!")
    print()


# ═══════════════ Helper Functions ═══════════════

def run_throughput(label: str, fn, iterations: int) -> None:
    start = time.perf_counter()
    for _ in range(iterations):
        fn()
    elapsed_ms = (time.perf_counter() - start) * 1000
    ops_per_sec = int(iterations / (elapsed_ms / 1000))
    print(f"  │ {label:<30} │ {elapsed_ms:>10.1f} │ {ops_per_sec:>12,} │")


def measure_latencies(fn, samples: int) -> list[float]:
    latencies = []
    for _ in range(samples):
        start = time.perf_counter_ns()
        fn()
        latencies.append(time.perf_counter_ns() - start)
    latencies.sort()
    return latencies


def print_latency_row(label: str, sorted_ns: list[float]) -> None:
    n = len(sorted_ns)
    mn = sorted_ns[0]
    avg = statistics.mean(sorted_ns)
    p50 = sorted_ns[int(n * 0.50)]
    p95 = sorted_ns[int(n * 0.95)]
    p99 = sorted_ns[int(n * 0.99)]
    print(f"  │ {label:<18} │ {mn:>8.0f} │ {avg:>8.0f} │ {p50:>8.0f} │ {p95:>8.0f} │ {p99:>8.0f} │")


class DummyModule(IModule):
    def __init__(self, id: str):
        self._id = id

    @property
    def name(self) -> str: return self._id
    @property
    def description(self) -> str: return "Dummy"
    @property
    def commands(self) -> list[str]: return ["noop"]
    def execute(self, command: str, args: list[str]) -> str: return "ok"


def bench_ops(reg: ModuleRegistry, iterations: int) -> int:
    start = time.perf_counter()
    for _ in range(iterations):
        reg.dispatch("calculator", "add", ["1", "2"])
    elapsed = time.perf_counter() - start
    return int(iterations / elapsed)


if __name__ == "__main__":
    asyncio.run(main())
