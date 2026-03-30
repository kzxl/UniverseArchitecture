package sales

import (
	"fmt"
	"strconv"
	"strings"

	"universe-demo/core"
)

// OrderCreatedEvent — published khi đơn hàng được tạo.
type OrderCreatedEvent struct {
	OrderID   string
	ProductID string
	Quantity  int
}

// SalesModule — Tạo đơn hàng, publish OrderCreatedEvent.
// KHÔNG import InventoryModule — chỉ publish event (Principle #5).
type SalesModule struct {
	eventBus     *core.EventBus
	orders       []order
	orderCounter int
}

type order struct {
	orderID   string
	productID string
	qty       int
}

func NewSalesModule(bus *core.EventBus) *SalesModule {
	return &SalesModule{eventBus: bus}
}

func (m *SalesModule) Name() string        { return "sales" }
func (m *SalesModule) Description() string { return "🛒 Sales order management" }
func (m *SalesModule) Commands() []string  { return []string{"create-order", "list-orders"} }

func (m *SalesModule) Execute(command string, args []string) string {
	switch strings.ToLower(command) {
	case "create-order":
		return m.createOrder(args)
	case "list-orders":
		return m.listOrders()
	default:
		return fmt.Sprintf("❓ Unknown command: %s", command)
	}
}

func (m *SalesModule) createOrder(args []string) string {
	if len(args) < 2 {
		return "Error: usage: sales create-order <productId> <quantity>"
	}
	pid := strings.ToUpper(args[0])
	qty, err := strconv.Atoi(args[1])
	if err != nil {
		return fmt.Sprintf("Error: invalid quantity '%s'", args[1])
	}

	m.orderCounter++
	orderID := fmt.Sprintf("ORD-%04d", m.orderCounter)
	m.orders = append(m.orders, order{orderID: orderID, productID: pid, qty: qty})

	// Publish event — InventoryModule và NotifierModule sẽ react
	core.Publish(m.eventBus, OrderCreatedEvent{
		OrderID:   orderID,
		ProductID: pid,
		Quantity:  qty,
	})

	return fmt.Sprintf("🛒 Order %s created: %dx %s", orderID, qty, pid)
}

func (m *SalesModule) listOrders() string {
	if len(m.orders) == 0 {
		return "📭 No orders yet."
	}
	var lines []string
	for i, o := range m.orders {
		lines = append(lines, fmt.Sprintf("  %d. %s: %dx %s", i+1, o.orderID, o.qty, o.productID))
	}
	return strings.Join(lines, "\n")
}
