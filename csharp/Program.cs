using System.Diagnostics;
using UniverseDemo.Core;
using UniverseDemo.Modules.Calculator;
using UniverseDemo.Modules.Greeter;
using UniverseDemo.Shared;

// ═══════════════════════════════════════════════════════════
//  🌌 Universe Architecture — C# Demo
//  Module tự đăng ký → Registry dispatch → kết quả
// ═══════════════════════════════════════════════════════════

var registry = new ModuleRegistry();

// ── Register modules (thêm module mới = thêm 1 dòng) ──
registry.Register(new CalculatorModule());
registry.Register(new GreeterModule());

// ══════════════════ Demo ══════════════════
ConsoleHelper.PrintHeader("Universe Architecture — C# .NET 8");

Console.WriteLine($"\n  📦 Registered modules: {registry.Count}");
foreach (var (name, module) in registry.GetAll())
    Console.WriteLine($"     • {name} — {module.Description} [{string.Join(", ", module.Commands)}]");

// ── Demo commands ──
ConsoleHelper.PrintHeader("Demo Commands");

var demos = new (string module, string cmd, string[] parameters)[]
{
    ("calculator", "add", ["10", "25"]),
    ("calculator", "sub", ["100", "37"]),
    ("calculator", "mul", ["7", "8"]),
    ("calculator", "div", ["22", "7"]),
    ("greeter", "hello", ["Universe"]),
    ("greeter", "goodbye", ["Developer"]),
};

foreach (var (module, cmd, parameters) in demos)
{
    var result = registry.Dispatch(module, cmd, parameters);
    ConsoleHelper.PrintResult(module, cmd, parameters, result);
}

// ══════════════════ Benchmark ══════════════════
ConsoleHelper.PrintHeader("Performance Benchmark");

const int iterations = 1_000_000;
var sw = Stopwatch.StartNew();

for (int i = 0; i < iterations; i++)
    registry.Dispatch("calculator", "add", ["1", "2"]);

sw.Stop();
ConsoleHelper.PrintBenchmark("Registry Dispatch (calculator add 1 2)", iterations, sw.Elapsed.TotalMilliseconds);

sw.Restart();
for (int i = 0; i < iterations; i++)
    registry.Dispatch("greeter", "hello", ["World"]);

sw.Stop();
ConsoleHelper.PrintBenchmark("Registry Dispatch (greeter hello World)", iterations, sw.Elapsed.TotalMilliseconds);

Console.WriteLine();
Console.WriteLine("  ✅ All demos completed successfully!");
Console.WriteLine();
