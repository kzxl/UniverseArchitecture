using Xunit;
using UniverseDemo.Core;
using UniverseDemo.Core.Middleware;
using UniverseDemo.Modules.Calculator;
using UniverseDemo.Modules.Greeter;
using UniverseDemo.Modules.Inventory;
using UniverseDemo.Modules.Sales;

namespace UniverseDemo.Tests;

/// <summary>
/// Conformance Tests — Behavioral parity checks aligned with conformance/spec.yaml.
/// These tests verify that C# implementation matches the cross-language specification.
/// </summary>
public class ConformanceTests
{
    // ═══════════════ Registry Core ═══════════════

    [Fact]
    public void Calculator_Add()
    {
        var registry = CreateRegistry();
        var result = registry.Dispatch("calculator", "add", ["10", "25"]);
        Assert.Contains("10 + 25 = 35", result);
    }

    [Fact]
    public void Calculator_Sub()
    {
        var registry = CreateRegistry();
        var result = registry.Dispatch("calculator", "sub", ["100", "37"]);
        Assert.Contains("100 - 37 = 63", result);
    }

    [Fact]
    public void Calculator_Mul()
    {
        var registry = CreateRegistry();
        var result = registry.Dispatch("calculator", "mul", ["7", "8"]);
        Assert.Contains("7 * 8 = 56", result);
    }

    [Fact]
    public void Calculator_Div()
    {
        var registry = CreateRegistry();
        var result = registry.Dispatch("calculator", "div", ["22", "7"]);
        Assert.Contains("22 / 7", result);
    }

    [Fact]
    public void Calculator_DivByZero()
    {
        var registry = CreateRegistry();
        var result = registry.Dispatch("calculator", "div", ["10", "0"]);
        Assert.Contains("division by zero", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Greeter_Hello()
    {
        var registry = CreateRegistry();
        var result = registry.Dispatch("greeter", "hello", ["Universe"]);
        Assert.Contains("Universe", result);
    }

    [Fact]
    public void Duplicate_Registration_Throws()
    {
        var registry = new ModuleRegistry();
        registry.Register(new CalculatorModule());
        Assert.Throws<InvalidOperationException>(() => registry.Register(new CalculatorModule()));
    }

    [Fact]
    public void CaseInsensitive_ModuleLookup()
    {
        var registry = CreateRegistry();
        var result = registry.Dispatch("CALCULATOR", "add", ["1", "2"]);
        Assert.Contains("1 + 2 = 3", result);
    }

    [Fact]
    public void Unknown_Module_Throws()
    {
        var registry = CreateRegistry();
        Assert.Throws<KeyNotFoundException>(() => registry.Dispatch("nonexistent", "x", []));
    }

    // ═══════════════ EventBus ═══════════════

    [Fact]
    public void EventBus_PublishSubscribe()
    {
        var bus = new EventBus();
        string? received = null;
        bus.Subscribe<string>(e => received = e);
        bus.Publish("hello");
        Assert.Equal("hello", received);
    }

    [Fact]
    public void EventBus_Unsubscribe_StopsDelivery()
    {
        var bus = new EventBus();
        var count = 0;
        void Handler(string _) => count++;
        bus.Subscribe<string>(Handler);
        bus.Publish("first");
        Assert.Equal(1, count);

        bus.Unsubscribe<string>(Handler);
        bus.Publish("second");
        Assert.Equal(1, count); // Should NOT increment
    }

    // ═══════════════ Middleware Pipeline ═══════════════

    [Fact]
    public void Middleware_Pipeline_ExecutesInOrder()
    {
        var registry = new ModuleRegistry();
        registry.Register(new CalculatorModule());
        var loggingMw = new LoggingMiddleware();
        var timingMw = new TimingMiddleware();
        registry.AddMiddleware(loggingMw);
        registry.AddMiddleware(timingMw);

        var result = registry.Dispatch("calculator", "add", ["1", "1"]);

        Assert.Contains("1 + 1 = 2", result);
        Assert.NotEmpty(loggingMw.Logs);
    }

    [Fact]
    public void ErrorMiddleware_CatchesExceptions()
    {
        var registry = new ModuleRegistry();
        registry.Register(new CalculatorModule());
        registry.AddMiddleware(new ErrorHandlingMiddleware());

        // This should not throw — error middleware catches it
        var result = registry.Dispatch("calculator", "add", []);
        Assert.NotNull(result);
    }

    // ═══════════════ Enterprise Scenario ═══════════════

    [Fact]
    public void Sales_CreateOrder_ReturnsOrderId()
    {
        var bus = new EventBus();
        var registry = new ModuleRegistry();
        registry.Register(new SalesModule(bus));
        var result = registry.Dispatch("sales", "create-order", ["PROD-001", "5"]);
        Assert.Contains("ORD-", result);
    }

    [Fact]
    public void Inventory_CheckStock()
    {
        var bus = new EventBus();
        var registry = new ModuleRegistry();
        registry.Register(new InventoryModule(bus));
        var result = registry.Dispatch("inventory", "check", ["PROD-001"]);
        Assert.Contains("PROD-001", result);
    }

    [Fact]
    public void Inventory_DeductInsufficientStock()
    {
        var bus = new EventBus();
        var registry = new ModuleRegistry();
        registry.Register(new InventoryModule(bus));
        var result = registry.Dispatch("inventory", "deduct", ["PROD-001", "999"]);
        Assert.Contains("Insufficient", result);
    }

    [Fact]
    public void Inventory_ListProducts()
    {
        var bus = new EventBus();
        var registry = new ModuleRegistry();
        registry.Register(new InventoryModule(bus));
        var result = registry.Dispatch("inventory", "list", []);
        Assert.Contains("PROD-001", result);
    }

    [Fact]
    public async Task Enterprise_Flow_SalesDeductsInventory()
    {
        var registry = new ModuleRegistry();
        var inventory = new InventoryModule(registry.EventBus);
        await registry.RegisterAsync(inventory);
        registry.Register(new SalesModule(registry.EventBus));

        // Check initial stock
        var before = registry.Dispatch("inventory", "check", ["PROD-001"]);
        Assert.Contains("100", before);

        // Create order → EventBus → Inventory auto-deducts
        registry.Dispatch("sales", "create-order", ["PROD-001", "5"]);

        // Verify stock deducted
        var after = registry.Dispatch("inventory", "check", ["PROD-001"]);
        Assert.Contains("95", after);
    }

    // ═══════════════ Helpers ═══════════════

    private static ModuleRegistry CreateRegistry()
    {
        var registry = new ModuleRegistry();
        registry.Register(new CalculatorModule());
        registry.Register(new GreeterModule());
        return registry;
    }
}
