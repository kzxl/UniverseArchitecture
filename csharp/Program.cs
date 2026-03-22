using System.Diagnostics;
using UniverseDemo.Core;
using UniverseDemo.Modules.Calculator;
using UniverseDemo.Modules.Greeter;
using UniverseDemo.Shared;

// ═══════════════════════════════════════════════════════════
//  🌌 Universe Architecture — C# .NET 8 Demo
//  Module tự đăng ký → Registry dispatch → kết quả
// ═══════════════════════════════════════════════════════════

var registry = new ModuleRegistry();

// ── Register modules (thêm module mới = thêm 1 dòng) ──
registry.Register(new CalculatorModule());
registry.Register(new GreeterModule());

// ══════════════════ Info ══════════════════
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

// ══════════════════════════════════════════════════════════
//  ⚡ DETAILED BENCHMARK
// ══════════════════════════════════════════════════════════
ConsoleHelper.PrintHeader("Detailed Performance Benchmark");

const int warmupIterations = 10_000;
const int benchIterations = 1_000_000;
const int latencySamples = 10_000;

// ── Phase 1: Warmup (JIT compilation) ──
Console.WriteLine("\n  🔥 Phase 1: Warmup (JIT)...");
for (int i = 0; i < warmupIterations; i++)
{
    registry.Dispatch("calculator", "add", ["1", "2"]);
    registry.Dispatch("greeter", "hello", ["World"]);
}
Console.WriteLine($"     {warmupIterations:N0} warmup iterations completed");

// ── Phase 2: Throughput (ops/sec) ──
Console.WriteLine("\n  📊 Phase 2: Throughput (1M iterations each)");
Console.WriteLine("  ┌────────────────────────────────┬────────────┬──────────────┐");
Console.WriteLine("  │ Scenario                       │ Time (ms)  │ Ops/sec      │");
Console.WriteLine("  ├────────────────────────────────┼────────────┼──────────────┤");

RunThroughput("calculator add 1 2", () => registry.Dispatch("calculator", "add", ["1", "2"]), benchIterations);
RunThroughput("calculator mul 7 8", () => registry.Dispatch("calculator", "mul", ["7", "8"]), benchIterations);
RunThroughput("calculator div 22 7", () => registry.Dispatch("calculator", "div", ["22", "7"]), benchIterations);
RunThroughput("greeter hello World", () => registry.Dispatch("greeter", "hello", ["World"]), benchIterations);
RunThroughput("greeter goodbye Dev", () => registry.Dispatch("greeter", "goodbye", ["Dev"]), benchIterations);

// Registry overhead: lookup miss
RunThroughput("registry miss (error)", () =>
{
    try { registry.Dispatch("nonexistent", "x", []); } catch { }
}, benchIterations);

Console.WriteLine("  └────────────────────────────────┴────────────┴──────────────┘");

// ── Phase 3: Latency Distribution ──
Console.WriteLine($"\n  📈 Phase 3: Latency Distribution ({latencySamples:N0} samples)");

var calcLatencies = MeasureLatencies(() => registry.Dispatch("calculator", "add", ["1", "2"]), latencySamples);
var greetLatencies = MeasureLatencies(() => registry.Dispatch("greeter", "hello", ["World"]), latencySamples);

Console.WriteLine("  ┌────────────────────┬──────────┬──────────┬──────────┬──────────┬──────────┐");
Console.WriteLine("  │ Scenario           │ Min (ns) │ Avg (ns) │ P50 (ns) │ P95 (ns) │ P99 (ns) │");
Console.WriteLine("  ├────────────────────┼──────────┼──────────┼──────────┼──────────┼──────────┤");
PrintLatencyRow("calculator add", calcLatencies);
PrintLatencyRow("greeter hello", greetLatencies);
Console.WriteLine("  └────────────────────┴──────────┴──────────┴──────────┴──────────┴──────────┘");

// ── Phase 4: Scalability ──
Console.WriteLine("\n  🔬 Phase 4: Registry Scalability");
Console.WriteLine("  ┌──────────────┬──────────────┬──────────────┐");
Console.WriteLine("  │ # Modules    │ Dispatch/sec │ Overhead     │");
Console.WriteLine("  ├──────────────┼──────────────┼──────────────┤");

var baselineOps = BenchmarkOps(registry, benchIterations / 10);

for (int n = 10; n <= 100; n += 30)
{
    var scaledRegistry = new ModuleRegistry();
    scaledRegistry.Register(new CalculatorModule());
    for (int j = 1; j < n; j++)
        scaledRegistry.Register(new DummyModule($"dummy_{j}"));

    var ops = BenchmarkOps(scaledRegistry, benchIterations / 10);
    var overhead = ((double)baselineOps / ops - 1) * 100;
    Console.WriteLine($"  │ {n,-12} │ {ops,12:N0} │ {(overhead > 0 ? $"+{overhead:F1}%" : "baseline"),-12} │");
}

Console.WriteLine("  └──────────────┴──────────────┴──────────────┘");

// ── Phase 5: Memory ──
Console.WriteLine("\n  💾 Phase 5: Memory Usage");
var before = GC.GetTotalMemory(true);
var tempRegistry = new ModuleRegistry();
for (int i = 0; i < 1000; i++)
    tempRegistry.Register(new DummyModule($"mod_{i}"));
var after = GC.GetTotalMemory(true);
Console.WriteLine($"     Registry with 1,000 modules: ~{(after - before) / 1024.0:F1} KB");
Console.WriteLine($"     Per-module overhead: ~{(after - before) / 1000.0:F0} bytes");

Console.WriteLine();
Console.WriteLine("  ✅ All benchmarks completed!");
Console.WriteLine();

// ═══════════════ Helper Methods ═══════════════

static void RunThroughput(string label, Action action, int iterations)
{
    var sw = Stopwatch.StartNew();
    for (int i = 0; i < iterations; i++) action();
    sw.Stop();
    var opsPerSec = iterations / sw.Elapsed.TotalSeconds;
    Console.WriteLine($"  │ {label,-30} │ {sw.Elapsed.TotalMilliseconds,10:F1} │ {opsPerSec,12:N0} │");
}

static double[] MeasureLatencies(Action action, int samples)
{
    var latencies = new double[samples];
    for (int i = 0; i < samples; i++)
    {
        var start = Stopwatch.GetTimestamp();
        action();
        var end = Stopwatch.GetTimestamp();
        latencies[i] = (end - start) * 1_000_000_000.0 / Stopwatch.Frequency; // nanoseconds
    }
    Array.Sort(latencies);
    return latencies;
}

static void PrintLatencyRow(string label, double[] sorted)
{
    var min = sorted[0];
    var avg = sorted.Average();
    var p50 = sorted[(int)(sorted.Length * 0.50)];
    var p95 = sorted[(int)(sorted.Length * 0.95)];
    var p99 = sorted[(int)(sorted.Length * 0.99)];
    Console.WriteLine($"  │ {label,-18} │ {min,8:F0} │ {avg,8:F0} │ {p50,8:F0} │ {p95,8:F0} │ {p99,8:F0} │");
}

static long BenchmarkOps(ModuleRegistry reg, int iterations)
{
    var sw = Stopwatch.StartNew();
    for (int i = 0; i < iterations; i++)
        reg.Dispatch("calculator", "add", ["1", "2"]);
    sw.Stop();
    return (long)(iterations / sw.Elapsed.TotalSeconds);
}

// Dummy module for scalability test
class DummyModule(string id) : IModule
{
    public string Name => id;
    public string Description => "Dummy";
    public IReadOnlyList<string> Commands => ["noop"];
    public string Execute(string command, string[] args) => "ok";
}
