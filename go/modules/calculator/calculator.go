package calculator

import (
	"fmt"
	"strconv"
	"strings"
)

// CalculatorModule — Một "ngôi sao" trong vũ trụ.
// Tự chứa, tuân thủ Module interface.
// Xóa folder này → app vẫn build (Acceptance Test #1).
type CalculatorModule struct{}

func (c *CalculatorModule) Name() string        { return "calculator" }
func (c *CalculatorModule) Description() string { return "Basic arithmetic operations (add, sub, mul, div)" }
func (c *CalculatorModule) Commands() []string  { return []string{"add", "sub", "mul", "div"} }

func (c *CalculatorModule) Execute(command string, args []string) string {
	if len(args) < 2 {
		return "Error: need 2 numbers. Usage: calculator add 1 2"
	}

	a, errA := strconv.ParseFloat(args[0], 64)
	b, errB := strconv.ParseFloat(args[1], 64)
	if errA != nil || errB != nil {
		return fmt.Sprintf("Error: invalid numbers '%s' '%s'", args[0], args[1])
	}

	switch strings.ToLower(command) {
	case "add":
		return fmt.Sprintf("%g + %g = %g", a, b, a+b)
	case "sub":
		return fmt.Sprintf("%g - %g = %g", a, b, a-b)
	case "mul":
		return fmt.Sprintf("%g * %g = %g", a, b, a*b)
	case "div":
		if b == 0 {
			return "Error: division by zero"
		}
		return fmt.Sprintf("%g / %g = %g", a, b, a/b)
	default:
		return fmt.Sprintf("Unknown command '%s'. Available: %s", command, strings.Join(c.Commands(), ", "))
	}
}
