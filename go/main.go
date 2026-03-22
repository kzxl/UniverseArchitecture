package main

import (
	"fmt"
	"math"
	"sort"
	"time"

	"universe-demo/core"
	"universe-demo/modules/calculator"
	"universe-demo/modules/greeter"
	"universe-demo/shared"
)

// ═══════════════════════════════════════════════════════════
//  🌌 Universe Architecture — Go Demo
//  Module tự đăng ký → Registry dispatch → kết quả
// ═══════════════════════════════════════════════════════════

func main() {
	registry := core.NewRegistry()

	// ── Register modules (thêm module mới = thêm 1 dòng) ──
	registry.Register(&calculator.CalculatorModule{})
	registry.Register(&greeter.GreeterModule{})

	// ══════════════════ Info ══════════════════
	shared.PrintHeader("Universe Architecture — Go")

	fmt.Printf("\n  📦 Registered modules: %d\n", registry.Count())
	for name, mod := range registry.GetAll() {
		fmt.Printf("     • %s — %s %v\n", name, mod.Description(), mod.Commands())
	}

	// ══════════════════ Demo ══════════════════
	shared.PrintHeader("Demo Commands")

	demos := []struct {
		module, cmd string
		args        []string
	}{
		{"calculator", "add", []string{"10", "25"}},
		{"calculator", "sub", []string{"100", "37"}},
		{"calculator", "mul", []string{"7", "8"}},
		{"calculator", "div", []string{"22", "7"}},
		{"greeter", "hello", []string{"Universe"}},
		{"greeter", "goodbye", []string{"Developer"}},
	}

	for _, d := range demos {
		result := registry.Dispatch(d.module, d.cmd, d.args)
		shared.PrintResult(d.module, d.cmd, d.args, result)
	}

	// ══════════════════════════════════════════════════════════
	//  ⚡ DETAILED BENCHMARK
	// ══════════════════════════════════════════════════════════
	shared.PrintHeader("Detailed Performance Benchmark")

	const warmupIter = 10_000
	const benchIter = 1_000_000
	const latencySamples = 10_000

	// ── Phase 1: Warmup ──
	fmt.Println("\n  🔥 Phase 1: Warmup...")
	for i := 0; i < warmupIter; i++ {
		registry.Dispatch("calculator", "add", []string{"1", "2"})
		registry.Dispatch("greeter", "hello", []string{"World"})
	}
	fmt.Printf("     %d warmup iterations completed\n", warmupIter)

	// ── Phase 2: Throughput ──
	fmt.Println("\n  📊 Phase 2: Throughput (1M iterations each)")
	fmt.Println("  ┌────────────────────────────────┬────────────┬──────────────┐")
	fmt.Println("  │ Scenario                       │ Time (ms)  │ Ops/sec      │")
	fmt.Println("  ├────────────────────────────────┼────────────┼──────────────┤")

	runThroughput("calculator add 1 2", func() { registry.Dispatch("calculator", "add", []string{"1", "2"}) }, benchIter)
	runThroughput("calculator mul 7 8", func() { registry.Dispatch("calculator", "mul", []string{"7", "8"}) }, benchIter)
	runThroughput("calculator div 22 7", func() { registry.Dispatch("calculator", "div", []string{"22", "7"}) }, benchIter)
	runThroughput("greeter hello World", func() { registry.Dispatch("greeter", "hello", []string{"World"}) }, benchIter)
	runThroughput("greeter goodbye Dev", func() { registry.Dispatch("greeter", "goodbye", []string{"Dev"}) }, benchIter)

	fmt.Println("  └────────────────────────────────┴────────────┴──────────────┘")

	// ── Phase 3: Latency Distribution ──
	fmt.Printf("\n  📈 Phase 3: Latency Distribution (%d samples)\n", latencySamples)

	calcLat := measureLatencies(func() { registry.Dispatch("calculator", "add", []string{"1", "2"}) }, latencySamples)
	greetLat := measureLatencies(func() { registry.Dispatch("greeter", "hello", []string{"World"}) }, latencySamples)

	fmt.Println("  ┌────────────────────┬──────────┬──────────┬──────────┬──────────┬──────────┐")
	fmt.Println("  │ Scenario           │ Min (ns) │ Avg (ns) │ P50 (ns) │ P95 (ns) │ P99 (ns) │")
	fmt.Println("  ├────────────────────┼──────────┼──────────┼──────────┼──────────┼──────────┤")
	printLatencyRow("calculator add", calcLat)
	printLatencyRow("greeter hello", greetLat)
	fmt.Println("  └────────────────────┴──────────┴──────────┴──────────┴──────────┴──────────┘")

	// ── Phase 4: Scalability ──
	fmt.Println("\n  🔬 Phase 4: Registry Scalability")
	fmt.Println("  ┌──────────────┬──────────────┬──────────────┐")
	fmt.Println("  │ # Modules    │ Dispatch/sec │ Overhead     │")
	fmt.Println("  ├──────────────┼──────────────┼──────────────┤")

	baselineOps := benchmarkOps(registry, benchIter/10)

	for _, n := range []int{10, 40, 70, 100} {
		scaled := core.NewRegistry()
		scaled.Register(&calculator.CalculatorModule{})
		for j := 1; j < n; j++ {
			scaled.Register(&dummyModule{id: fmt.Sprintf("dummy_%d", j)})
		}
		ops := benchmarkOps(scaled, benchIter/10)
		overhead := (float64(baselineOps)/float64(ops) - 1) * 100
		if n == registry.Count() {
			fmt.Printf("  │ %-12d │ %12d │ %-12s │\n", n, ops, "baseline")
		} else {
			fmt.Printf("  │ %-12d │ %12d │ +%.1f%%%-7s │\n", n, ops, overhead, "")
		}
	}

	fmt.Println("  └──────────────┴──────────────┴──────────────┘")

	fmt.Println()
	fmt.Println("  ✅ All benchmarks completed!")
	fmt.Println()
}

// ═══════════════ Helper Functions ═══════════════

func runThroughput(label string, fn func(), iterations int) {
	start := time.Now()
	for i := 0; i < iterations; i++ {
		fn()
	}
	elapsed := time.Since(start)
	ms := float64(elapsed.Milliseconds())
	opsPerSec := float64(iterations) / elapsed.Seconds()
	fmt.Printf("  │ %-30s │ %10.1f │ %12.0f │\n", label, ms, opsPerSec)
}

func measureLatencies(fn func(), samples int) []float64 {
	latencies := make([]float64, samples)
	for i := 0; i < samples; i++ {
		start := time.Now()
		fn()
		latencies[i] = float64(time.Since(start).Nanoseconds())
	}
	sort.Float64s(latencies)
	return latencies
}

func printLatencyRow(label string, sorted []float64) {
	n := len(sorted)
	min := sorted[0]
	avg := 0.0
	for _, v := range sorted {
		avg += v
	}
	avg /= float64(n)
	p50 := sorted[int(math.Floor(float64(n)*0.50))]
	p95 := sorted[int(math.Floor(float64(n)*0.95))]
	p99 := sorted[int(math.Floor(float64(n)*0.99))]
	fmt.Printf("  │ %-18s │ %8.0f │ %8.0f │ %8.0f │ %8.0f │ %8.0f │\n", label, min, avg, p50, p95, p99)
}

func benchmarkOps(reg *core.Registry, iterations int) int64 {
	start := time.Now()
	for i := 0; i < iterations; i++ {
		reg.Dispatch("calculator", "add", []string{"1", "2"})
	}
	elapsed := time.Since(start)
	return int64(float64(iterations) / elapsed.Seconds())
}

// dummyModule for scalability test
type dummyModule struct{ id string }

func (d *dummyModule) Name() string        { return d.id }
func (d *dummyModule) Description() string { return "Dummy" }
func (d *dummyModule) Commands() []string  { return []string{"noop"} }
func (d *dummyModule) Execute(_ string, _ []string) string { return "ok" }
