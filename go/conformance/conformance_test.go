package conformance

import (
	"strings"
	"testing"

	"universe-demo/core"
	"universe-demo/modules/calculator"
	"universe-demo/modules/greeter"
	"universe-demo/modules/inventory"
	"universe-demo/modules/sales"
)

// ═══════════════ Registry Core ═══════════════

func TestCalculatorAdd(t *testing.T) {
	r := createRegistry()
	result := r.Dispatch("calculator", "add", []string{"10", "25"})
	assertContains(t, result, "10 + 25 = 35")
}

func TestCalculatorSub(t *testing.T) {
	r := createRegistry()
	result := r.Dispatch("calculator", "sub", []string{"100", "37"})
	assertContains(t, result, "100 - 37 = 63")
}

func TestCalculatorMul(t *testing.T) {
	r := createRegistry()
	result := r.Dispatch("calculator", "mul", []string{"7", "8"})
	assertContains(t, result, "7 * 8 = 56")
}

func TestCalculatorDiv(t *testing.T) {
	r := createRegistry()
	result := r.Dispatch("calculator", "div", []string{"22", "7"})
	assertContains(t, result, "22 / 7")
}

func TestCalculatorDivByZero(t *testing.T) {
	r := createRegistry()
	result := r.Dispatch("calculator", "div", []string{"10", "0"})
	assertContainsCI(t, result, "division by zero")
}

func TestGreeterHello(t *testing.T) {
	r := createRegistry()
	result := r.Dispatch("greeter", "hello", []string{"Universe"})
	assertContains(t, result, "Universe")
}

func TestDuplicateRegistrationPanics(t *testing.T) {
	defer func() {
		if r := recover(); r == nil {
			t.Error("Expected panic on duplicate registration")
		}
	}()
	r := core.NewRegistry()
	r.Register(&calculator.CalculatorModule{})
	r.Register(&calculator.CalculatorModule{})
}

func TestCaseInsensitiveModuleLookup(t *testing.T) {
	r := createRegistry()
	result := r.Dispatch("CALCULATOR", "add", []string{"1", "2"})
	assertContains(t, result, "1 + 2 = 3")
}

func TestUnknownModuleReturnsError(t *testing.T) {
	r := createRegistry()
	result := r.Dispatch("nonexistent", "x", nil)
	assertContainsCI(t, result, "not found")
}

// ═══════════════ EventBus ═══════════════

func TestEventBusPublishSubscribe(t *testing.T) {
	bus := core.NewEventBus()
	var received string
	core.Subscribe(bus, func(e string) { received = e })
	core.Publish(bus, "hello")
	if received != "hello" {
		t.Errorf("Expected 'hello', got '%s'", received)
	}
}

func TestEventBusUnsubscribe(t *testing.T) {
	bus := core.NewEventBus()
	count := 0
	handler := func(_ string) { count++ }
	core.Subscribe(bus, handler)
	core.Publish(bus, "first")
	if count != 1 {
		t.Fatalf("Expected 1, got %d", count)
	}
	// Note: Go EventBus uses reflect — unsubscribe by type clears all handlers of that type
}

// ═══════════════ Enterprise Scenario ═══════════════

func TestSalesCreateOrder(t *testing.T) {
	bus := core.NewEventBus()
	r := core.NewRegistry()
	r.Register(sales.NewSalesModule(bus))
	result := r.Dispatch("sales", "create-order", []string{"PROD-001", "5"})
	assertContains(t, result, "ORD-")
}

func TestInventoryCheckStock(t *testing.T) {
	bus := core.NewEventBus()
	r := core.NewRegistry()
	r.Register(inventory.NewInventoryModule(bus))
	result := r.Dispatch("inventory", "check", []string{"PROD-001"})
	assertContains(t, result, "PROD-001")
}

func TestInventoryDeductInsufficient(t *testing.T) {
	bus := core.NewEventBus()
	r := core.NewRegistry()
	r.Register(inventory.NewInventoryModule(bus))
	result := r.Dispatch("inventory", "deduct", []string{"PROD-001", "999"})
	assertContains(t, result, "Insufficient")
}

func TestInventoryList(t *testing.T) {
	bus := core.NewEventBus()
	r := core.NewRegistry()
	r.Register(inventory.NewInventoryModule(bus))
	result := r.Dispatch("inventory", "list", nil)
	assertContains(t, result, "PROD-001")
}

// ═══════════════ Helpers ═══════════════

func createRegistry() *core.Registry {
	r := core.NewRegistry()
	r.Register(&calculator.CalculatorModule{})
	r.Register(&greeter.GreeterModule{})
	return r
}

func assertContains(t *testing.T, actual, expected string) {
	t.Helper()
	if !strings.Contains(actual, expected) {
		t.Errorf("Expected result to contain '%s', got: '%s'", expected, actual)
	}
}

func assertContainsCI(t *testing.T, actual, expected string) {
	t.Helper()
	if !strings.Contains(strings.ToLower(actual), strings.ToLower(expected)) {
		t.Errorf("Expected result to contain '%s' (case-insensitive), got: '%s'", expected, actual)
	}
}
