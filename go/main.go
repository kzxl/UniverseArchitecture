package main

import (
	"fmt"
	"math"
	"sort"
	"time"

	"universe-demo/core"
	mw "universe-demo/core/middleware"
	"universe-demo/modules/calculator"
	"universe-demo/modules/greeter"
	"universe-demo/modules/notifier"
	"universe-demo/shared"
)

// ═══════════════════════════════════════════════════════════
//  🌌 Universe Architecture — Go Demo
//  Full demo: Registry + EventBus + Middleware + Lifecycle
// ═══════════════════════════════════════════════════════════

func main() {
	registry := core.NewRegistry()

	// ── Middleware pipeline (Gravity) ──
	loggingMw := &mw.LoggingMiddleware{}
	registry.AddMiddleware(&mw.ErrorHandlingMiddleware{})
	registry.AddMiddleware(&mw.TimingMiddleware{})
	registry.AddMiddleware(loggingMw)

	// ── Register modules ──
	registry.Register(&calculator.CalculatorModule{})
	registry.Register(&greeter.GreeterModule{})

	// Register NotifierModule with lifecycle
	not := notifier.NewNotifierModule(registry.EventBus)
	if err := registry.RegisterWithLifecycle(not); err != nil {
		panic(err)
	}

	// ══════════════════ Info ══════════════════
	shared.PrintHeader("Universe Architecture — Go")

	fmt.Printf("\n  📦 Registered modules: %d\n", registry.Count())
	fmt.Printf("  🔗 Middleware pipeline: %d handlers\n", registry.MiddlewareCount())
	fmt.Printf("  📡 EventBus: %d event types, %d handlers\n",
		registry.EventBus.TypeCount(), registry.EventBus.HandlerCount())

	for name, mod := range registry.GetAll() {
		fmt.Printf("     • %s — %s %v\n", name, mod.Description(), mod.Commands())
	}

	// ══════════════════ Demo ══════════════════
	shared.PrintHeader("Demo Commands (via Middleware Pipeline)")

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

		// Publish events for EventBus demo
		if d.module == "calculator" {
			core.Publish(registry.EventBus, notifier.CalculationPerformedEvent{
				Operation: fmt.Sprintf("%s %s", d.cmd, fmt.Sprint(d.args)),
				Result:    result,
			})
		} else if d.module == "greeter" {
			name := "World"
			if len(d.args) > 0 {
				name = d.args[0]
			}
			core.Publish(registry.EventBus, notifier.GreetingEvent{
				Name: name, Message: result,
			})
		}
	}

	// ══════════════════ EventBus Demo ══════════════════
	shared.PrintHeader("EventBus — Indirect Communication")

	fmt.Println("\n  📡 Notifier received events from other modules:")
	fmt.Println(registry.Dispatch("notifier", "history", nil))
	fmt.Println()
	fmt.Println("  " + registry.Dispatch("notifier", "count", nil))

	// ══════════════════ Middleware Logs ══════════════════
	shared.PrintHeader("Middleware Pipeline — Gravity Logs")

	fmt.Printf("\n  📝 Logging middleware captured %d dispatch(es):\n", len(loggingMw.Logs))
	for i, log := range loggingMw.Logs {
		if i >= 5 {
			fmt.Printf("     ... and %d more\n", len(loggingMw.Logs)-5)
			break
		}
		fmt.Printf("     %s\n", log)
	}

	// ══════════════════ Lifecycle Demo ══════════════════
	shared.PrintHeader("Module Lifecycle — Star Lifecycle")

	fmt.Println("\n  🌟 Shutting down all modules with lifecycle hooks...")
	if err := registry.Shutdown(); err != nil {
		fmt.Printf("  ❌ Shutdown error: %v\n", err)
	} else {
		fmt.Println("  ✅ All lifecycle modules shut down gracefully.")
	}

	// ══════════════════════════════════════════════════════════
	//  ⚡ DETAILED BENCHMARK
	// ══════════════════════════════════════════════════════════
	shared.PrintHeader("Detailed Performance Benchmark")

	const warmupIter = 10_000
	const benchIter = 1_000_000
	const latencySamples = 10_000

	// Fresh registry without middleware for fair benchmark
	benchRegistry := core.NewRegistry()
	benchRegistry.Register(&calculator.CalculatorModule{})
	benchRegistry.Register(&greeter.GreeterModule{})

	// ── Phase 1: Warmup ──
	fmt.Println("\n  🔥 Phase 1: Warmup...")
	for i := 0; i < warmupIter; i++ {
		benchRegistry.Dispatch("calculator", "add", []string{"1", "2"})
		benchRegistry.Dispatch("greeter", "hello", []string{"World"})
	}
	fmt.Printf("     %d warmup iterations completed\n", warmupIter)

	// ── Phase 2: Throughput ──
	fmt.Println("\n  📊 Phase 2: Throughput (1M iterations each)")
	fmt.Println("  ┌────────────────────────────────┬────────────┬──────────────┐")
	fmt.Println("  │ Scenario                       │ Time (ms)  │ Ops/sec      │")
	fmt.Println("  ├────────────────────────────────┼────────────┼──────────────┤")

	runThroughput("calculator add 1 2", func() { benchRegistry.Dispatch("calculator", "add", []string{"1", "2"}) }, benchIter)
	runThroughput("calculator mul 7 8", func() { benchRegistry.Dispatch("calculator", "mul", []string{"7", "8"}) }, benchIter)
	runThroughput("calculator div 22 7", func() { benchRegistry.Dispatch("calculator", "div", []string{"22", "7"}) }, benchIter)
	runThroughput("greeter hello World", func() { benchRegistry.Dispatch("greeter", "hello", []string{"World"}) }, benchIter)
	runThroughput("greeter goodbye Dev", func() { benchRegistry.Dispatch("greeter", "goodbye", []string{"Dev"}) }, benchIter)

	fmt.Println("  └────────────────────────────────┴────────────┴──────────────┘")

	// ── Phase 3: Latency Distribution ──
	fmt.Printf("\n  📈 Phase 3: Latency Distribution (%d samples)\n", latencySamples)

	calcLat := measureLatencies(func() { benchRegistry.Dispatch("calculator", "add", []string{"1", "2"}) }, latencySamples)
	greetLat := measureLatencies(func() { benchRegistry.Dispatch("greeter", "hello", []string{"World"}) }, latencySamples)

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

	baselineOps := benchmarkOps(benchRegistry, benchIter/10)

	for _, n := range []int{10, 40, 70, 100} {
		scaled := core.NewRegistry()
		scaled.Register(&calculator.CalculatorModule{})
		for j := 1; j < n; j++ {
			scaled.Register(&dummyModule{id: fmt.Sprintf("dummy_%d", j)})
		}
		ops := benchmarkOps(scaled, benchIter/10)
		overhead := (float64(baselineOps)/float64(ops) - 1) * 100
		if n == benchRegistry.Count() {
			fmt.Printf("  │ %-12d │ %12d │ %-12s │\n", n, ops, "baseline")
		} else {
			fmt.Printf("  │ %-12d │ %12d │ +%.1f%%%-7s │\n", n, ops, overhead, "")
		}
	}

	fmt.Println("  └──────────────┴──────────────┴──────────────┘")

	// ── Phase 5: EventBus Throughput ──
	fmt.Println("\n  📡 Phase 5: EventBus Throughput")
	benchBus := core.NewEventBus()
	core.Subscribe(benchBus, func(_ notifier.CalculationPerformedEvent) {})

	for i := 0; i < warmupIter; i++ {
		core.Publish(benchBus, notifier.CalculationPerformedEvent{Operation: "test", Result: "1"})
	}

	fmt.Println("  ┌────────────────────────────────┬────────────┬──────────────┐")
	fmt.Println("  │ Scenario                       │ Time (ms)  │ Ops/sec      │")
	fmt.Println("  ├────────────────────────────────┼────────────┼──────────────┤")

	runThroughput("eventbus publish (1 sub)", func() {
		core.Publish(benchBus, notifier.CalculationPerformedEvent{Operation: "bench", Result: "1"})
	}, benchIter)

	fmt.Println("  └────────────────────────────────┴────────────┴──────────────┘")

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

type dummyModule struct{ id string }

func (d *dummyModule) Name() string                        { return d.id }
func (d *dummyModule) Description() string                 { return "Dummy" }
func (d *dummyModule) Commands() []string                  { return []string{"noop"} }
func (d *dummyModule) Execute(_ string, _ []string) string { return "ok" }
