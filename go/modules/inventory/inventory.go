package inventory

import (
	"fmt"
	"strconv"
	"strings"

	"universe-demo/core"
)

// StockChangedEvent — published khi stock thay đổi.
type StockChangedEvent struct {
	ProductID string
	Quantity  int
	Action    string // "deducted", "added"
}

// InventoryModule — Quản lý kho (Data Sovereignty).
type InventoryModule struct {
	eventBus *core.EventBus
	stock    map[string]int
}

func NewInventoryModule(bus *core.EventBus) *InventoryModule {
	return &InventoryModule{
		eventBus: bus,
		stock: map[string]int{
			"PROD-001": 100,
			"PROD-002": 50,
			"PROD-003": 200,
		},
	}
}

func (m *InventoryModule) Name() string        { return "inventory" }
func (m *InventoryModule) Description() string { return "📦 Inventory management (stock tracking)" }
func (m *InventoryModule) Commands() []string  { return []string{"check", "add", "deduct", "list"} }

func (m *InventoryModule) Execute(command string, args []string) string {
	switch strings.ToLower(command) {
	case "check":
		return m.checkStock(args)
	case "add":
		return m.addStock(args)
	case "deduct":
		return m.DeductStock(args)
	case "list":
		return m.listStock()
	default:
		return fmt.Sprintf("❓ Unknown command: %s", command)
	}
}

func (m *InventoryModule) checkStock(args []string) string {
	if len(args) < 1 {
		return "Error: usage: inventory check <productId>"
	}
	pid := strings.ToUpper(args[0])
	qty, ok := m.stock[pid]
	if !ok {
		return fmt.Sprintf("❌ Product '%s' not found", pid)
	}
	return fmt.Sprintf("📦 %s: %d units in stock", pid, qty)
}

func (m *InventoryModule) addStock(args []string) string {
	if len(args) < 2 {
		return "Error: usage: inventory add <productId> <quantity>"
	}
	pid := strings.ToUpper(args[0])
	qty, err := strconv.Atoi(args[1])
	if err != nil {
		return fmt.Sprintf("Error: invalid quantity '%s'", args[1])
	}
	m.stock[pid] += qty
	core.Publish(m.eventBus, StockChangedEvent{ProductID: pid, Quantity: qty, Action: "added"})
	return fmt.Sprintf("✅ Added %d units to %s. New stock: %d", qty, pid, m.stock[pid])
}

// DeductStock — exported so other flows can reference it.
func (m *InventoryModule) DeductStock(args []string) string {
	if len(args) < 2 {
		return "Error: usage: inventory deduct <productId> <quantity>"
	}
	pid := strings.ToUpper(args[0])
	qty, err := strconv.Atoi(args[1])
	if err != nil {
		return fmt.Sprintf("Error: invalid quantity '%s'", args[1])
	}
	current, ok := m.stock[pid]
	if !ok || current < qty {
		return fmt.Sprintf("❌ Insufficient stock for %s. Available: %d", pid, current)
	}
	m.stock[pid] = current - qty
	core.Publish(m.eventBus, StockChangedEvent{ProductID: pid, Quantity: qty, Action: "deducted"})
	return fmt.Sprintf("📤 Deducted %d units from %s. Remaining: %d", qty, pid, m.stock[pid])
}

func (m *InventoryModule) listStock() string {
	if len(m.stock) == 0 {
		return "📭 No products in stock."
	}
	var lines []string
	i := 1
	for pid, qty := range m.stock {
		lines = append(lines, fmt.Sprintf("  %d. %s: %d units", i, pid, qty))
		i++
	}
	return strings.Join(lines, "\n")
}
