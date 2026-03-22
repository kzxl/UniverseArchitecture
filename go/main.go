package main

import (
	"fmt"
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

	// ══════════════════ Benchmark ══════════════════
	shared.PrintHeader("Performance Benchmark")

	const iterations = 1_000_000

	start := time.Now()
	for i := 0; i < iterations; i++ {
		registry.Dispatch("calculator", "add", []string{"1", "2"})
	}
	elapsed := time.Since(start)
	shared.PrintBenchmark("Registry Dispatch (calculator add 1 2)", iterations, float64(elapsed.Milliseconds()))

	start = time.Now()
	for i := 0; i < iterations; i++ {
		registry.Dispatch("greeter", "hello", []string{"World"})
	}
	elapsed = time.Since(start)
	shared.PrintBenchmark("Registry Dispatch (greeter hello World)", iterations, float64(elapsed.Milliseconds()))

	fmt.Println()
	fmt.Println("  ✅ All demos completed successfully!")
	fmt.Println()
}
