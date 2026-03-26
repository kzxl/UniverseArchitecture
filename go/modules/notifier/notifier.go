package notifier

import (
	"fmt"
	"strings"

	"universe-demo/core"
)

// Events
type CalculationPerformedEvent struct {
	Operation string
	Result    string
}

type GreetingEvent struct {
	Name    string
	Message string
}

// NotifierModule — Demo EventBus + Lifecycle.
type NotifierModule struct {
	eventBus      *core.EventBus
	notifications []string
}

func NewNotifierModule(bus *core.EventBus) *NotifierModule {
	return &NotifierModule{eventBus: bus}
}

func (m *NotifierModule) Name() string        { return "notifier" }
func (m *NotifierModule) Description() string { return "🔔 Listens to events from other modules" }
func (m *NotifierModule) Commands() []string  { return []string{"history", "count", "clear"} }

func (m *NotifierModule) Execute(command string, args []string) string {
	switch strings.ToLower(command) {
	case "history":
		if len(m.notifications) == 0 {
			return "📭 No notifications yet."
		}
		start := 0
		if len(m.notifications) > 10 {
			start = len(m.notifications) - 10
		}
		var lines []string
		for i, n := range m.notifications[start:] {
			lines = append(lines, fmt.Sprintf("  %d. %s", i+1, n))
		}
		return strings.Join(lines, "\n")
	case "count":
		return fmt.Sprintf("📬 %d notification(s)", len(m.notifications))
	case "clear":
		count := len(m.notifications)
		m.notifications = nil
		return fmt.Sprintf("🗑️ Cleared %d notification(s)", count)
	default:
		return fmt.Sprintf("❓ Unknown command: %s", command)
	}
}

// Lifecycle hooks
func (m *NotifierModule) OnInitializing() error { return nil }
func (m *NotifierModule) OnInitialized() error {
	core.Subscribe(m.eventBus, func(e CalculationPerformedEvent) {
		m.notifications = append(m.notifications, fmt.Sprintf("🧮 Calculation: %s = %s", e.Operation, e.Result))
	})
	core.Subscribe(m.eventBus, func(e GreetingEvent) {
		m.notifications = append(m.notifications, fmt.Sprintf("👋 Greeting: %s", e.Message))
	})
	return nil
}
func (m *NotifierModule) OnShuttingDown() error { return nil }
func (m *NotifierModule) OnShutdown() error     { return nil }
