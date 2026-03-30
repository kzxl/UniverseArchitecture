using UniverseDemo.Core;

namespace UniverseDemo.Modules.Inventory;

// ── Events ──

/// <summary>Event khi stock thay đổi.</summary>
public sealed class StockChangedEvent
{
    public required string ProductId { get; init; }
    public required int Quantity { get; init; }
    public required string Action { get; init; } // "deducted", "added"
}

/// <summary>
/// InventoryModule — Quản lý kho (Data Sovereignty: chỉ module này access stock data).
/// Demo Principle #6: Data Sovereignty + #5: EventBus communication.
/// </summary>
public sealed class InventoryModule : IModule, IModuleLifecycle
{
    private readonly IEventBus _eventBus;

    // Simulated stock database (owned exclusively by this module)
    private readonly Dictionary<string, int> _stock = new()
    {
        ["PROD-001"] = 100,
        ["PROD-002"] = 50,
        ["PROD-003"] = 200,
    };

    public InventoryModule(IEventBus eventBus) => _eventBus = eventBus;

    public string Name => "inventory";
    public string Description => "📦 Inventory management (stock tracking)";
    public IReadOnlyList<string> Commands => ["check", "add", "deduct", "list"];

    public string Execute(string command, string[] args) => command.ToLower() switch
    {
        "check" => CheckStock(args),
        "add" => AddStock(args),
        "deduct" => DeductStock(args),
        "list" => ListStock(),
        _ => $"❓ Unknown command: {command}"
    };

    private string CheckStock(string[] args)
    {
        if (args.Length < 1) return "Error: usage: inventory check <productId>";
        var productId = args[0].ToUpper();
        return _stock.TryGetValue(productId, out var qty)
            ? $"📦 {productId}: {qty} units in stock"
            : $"❌ Product '{productId}' not found";
    }

    private string AddStock(string[] args)
    {
        if (args.Length < 2) return "Error: usage: inventory add <productId> <quantity>";
        var productId = args[0].ToUpper();
        if (!int.TryParse(args[1], out var qty)) return $"Error: invalid quantity '{args[1]}'";

        _stock[productId] = _stock.GetValueOrDefault(productId) + qty;
        _eventBus.Publish(new StockChangedEvent { ProductId = productId, Quantity = qty, Action = "added" });
        return $"✅ Added {qty} units to {productId}. New stock: {_stock[productId]}";
    }

    /// <summary>Deduct stock — called internally or via dispatch. Publishes StockChangedEvent.</summary>
    public string DeductStock(string[] args)
    {
        if (args.Length < 2) return "Error: usage: inventory deduct <productId> <quantity>";
        var productId = args[0].ToUpper();
        if (!int.TryParse(args[1], out var qty)) return $"Error: invalid quantity '{args[1]}'";

        if (!_stock.TryGetValue(productId, out var current) || current < qty)
            return $"❌ Insufficient stock for {productId}. Available: {current}";

        _stock[productId] = current - qty;
        _eventBus.Publish(new StockChangedEvent { ProductId = productId, Quantity = qty, Action = "deducted" });
        return $"📤 Deducted {qty} units from {productId}. Remaining: {_stock[productId]}";
    }

    private string ListStock()
    {
        if (_stock.Count == 0) return "📭 No products in stock.";
        var lines = _stock.Select((kv, i) => $"  {i + 1}. {kv.Key}: {kv.Value} units");
        return string.Join("\n", lines);
    }

    // ── Lifecycle ──
    public Task OnInitializing() => Task.CompletedTask;

    public Task OnInitialized()
    {
        // Subscribe OrderCreatedEvent — auto-deduct stock when order is placed
        // KHÔNG import SalesModule — chỉ biết event type (Principle #5)
        _eventBus.Subscribe<Sales.OrderCreatedEvent>(OnOrderCreated);
        return Task.CompletedTask;
    }

    public Task OnShuttingDown()
    {
        _eventBus.Unsubscribe<Sales.OrderCreatedEvent>(OnOrderCreated);
        return Task.CompletedTask;
    }

    public Task OnShutdown() => Task.CompletedTask;

    // ── Event Handlers ──

    private void OnOrderCreated(Sales.OrderCreatedEvent e)
    {
        // Auto-deduct stock — gọi qua internal method, không qua Registry dispatch
        DeductStock([e.ProductId, e.Quantity.ToString()]);
    }
}
