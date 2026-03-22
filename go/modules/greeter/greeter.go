package greeter

import (
	"fmt"
	"strings"
)

// GreeterModule — Module thứ 2 trong vũ trụ.
// Chứng minh thêm module mới = thêm 1 file + 1 dòng register.
type GreeterModule struct{}

func (g *GreeterModule) Name() string        { return "greeter" }
func (g *GreeterModule) Description() string { return "Greeting messages (hello, goodbye)" }
func (g *GreeterModule) Commands() []string  { return []string{"hello", "goodbye"} }

func (g *GreeterModule) Execute(command string, args []string) string {
	name := "World"
	if len(args) > 0 {
		name = strings.Join(args, " ")
	}

	switch strings.ToLower(command) {
	case "hello":
		return fmt.Sprintf("👋 Hello, %s! Welcome to the Universe!", name)
	case "goodbye":
		return fmt.Sprintf("🌙 Goodbye, %s! See you among the stars!", name)
	default:
		return fmt.Sprintf("Unknown command '%s'. Available: %s", command, strings.Join(g.Commands(), ", "))
	}
}
