"""
═══════════════════════════════════════════════════════════
 🌌 Universe Architecture — Python Demo
 Module tự đăng ký → Registry dispatch → kết quả
═══════════════════════════════════════════════════════════
"""
import time

from core import ModuleRegistry
from modules import CalculatorModule, GreeterModule
from shared import print_header, print_result, print_benchmark

registry = ModuleRegistry()

# ── Register modules (thêm module mới = thêm 1 dòng) ──
registry.register(CalculatorModule())
registry.register(GreeterModule())

# ══════════════════ Info ══════════════════
print_header("Universe Architecture — Python")

print(f"\n  📦 Registered modules: {registry.count}")
for name, mod in registry.get_all().items():
    print(f"     • {name} — {mod.description} [{', '.join(mod.commands)}]")

# ══════════════════ Demo ══════════════════
print_header("Demo Commands")

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

# ══════════════════ Benchmark ══════════════════
print_header("Performance Benchmark")

iterations = 1_000_000

start = time.perf_counter()
for _ in range(iterations):
    registry.dispatch("calculator", "add", ["1", "2"])
elapsed = (time.perf_counter() - start) * 1000
print_benchmark("Registry Dispatch (calculator add 1 2)", iterations, elapsed)

start = time.perf_counter()
for _ in range(iterations):
    registry.dispatch("greeter", "hello", ["World"])
elapsed = (time.perf_counter() - start) * 1000
print_benchmark("Registry Dispatch (greeter hello World)", iterations, elapsed)

print()
print("  ✅ All demos completed successfully!")
print()
