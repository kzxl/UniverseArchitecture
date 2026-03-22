package shared

import (
	"fmt"
	"strings"
)

// PrintHeader in header đẹp cho output.
func PrintHeader(title string) {
	line := strings.Repeat("═", 60)
	fmt.Println()
	fmt.Println(line)
	fmt.Printf("  🌌 %s\n", title)
	fmt.Println(line)
}

// PrintResult in kết quả 1 command.
func PrintResult(module, command string, args []string, result string) {
	fmt.Printf("  ▸ %s %s %s\n", module, command, strings.Join(args, " "))
	fmt.Printf("    → %s\n", result)
}

// PrintBenchmark in benchmark result.
func PrintBenchmark(label string, iterations int, elapsedMs float64) {
	opsPerSec := float64(iterations) / (elapsedMs / 1000.0)
	fmt.Printf("  ⚡ %s: %d ops in %.1fms (%.0f ops/sec)\n", label, iterations, elapsedMs, opsPerSec)
}
