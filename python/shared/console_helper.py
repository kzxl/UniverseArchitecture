"""
Shared Infrastructure — "Không-thời gian" mà modules sống trên.
Console output helpers, không chứa business logic.
"""
import sys

# Fix unicode output on Windows (cp1252 → utf-8)
sys.stdout.reconfigure(encoding="utf-8")


def print_header(title: str) -> None:
    line = "═" * 60
    print()
    print(line)
    print(f"  🌌 {title}")
    print(line)


def print_result(module: str, command: str, args: list[str], result: str) -> None:
    print(f"  ▸ {module} {command} {' '.join(args)}")
    print(f"    → {result}")


def print_benchmark(label: str, iterations: int, elapsed_ms: float) -> None:
    ops_per_sec = int(iterations / (elapsed_ms / 1000))
    print(f"  ⚡ {label}: {iterations:,} ops in {elapsed_ms:.1f}ms ({ops_per_sec:,} ops/sec)")
