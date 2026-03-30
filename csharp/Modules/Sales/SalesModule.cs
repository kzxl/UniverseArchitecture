using UniverseDemo.Core;

namespace UniverseDemo.Modules.Sales;

// ── Events ──

/// <summary>Event khi đơn hàng được tạo — các module khác subscribe để react.</summary>
public sealed class OrderCreatedEvent
{
    public required string OrderId { get; init; }
    public required string ProductId { get; init; }
    public required int Quantity { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// SalesModule — Tạo đơn hàng, publish OrderCreatedEvent.
/// Demo Principle #5: Indirect Communication + #6: Data Sovereignty.
/// SalesModule KHÔNG import InventoryModule — chỉ publish event.
/// </summary>
public sealed class SalesModule : IModule
{
    private readonly IEventBus _eventBus;
    private readonly List<(string orderId, string productId, int qty)> _orders = [];
    private int _orderCounter;

    public SalesModule(IEventBus eventBus) => _eventBus = eventBus;

    public string Name => "sales";
    public string Description => "🛒 Sales order management";
    public IReadOnlyList<string> Commands => ["create-order", "list-orders"];

    public string Execute(string command, string[] args) => command.ToLower() switch
    {
        "create-order" => CreateOrder(args),
        "list-orders" => ListOrders(),
        _ => $"❓ Unknown command: {command}"
    };

    private string CreateOrder(string[] args)
    {
        if (args.Length < 2) return "Error: usage: sales create-order <productId> <quantity>";
        var productId = args[0].ToUpper();
        if (!int.TryParse(args[1], out var qty)) return $"Error: invalid quantity '{args[1]}'";

        _orderCounter++;
        var orderId = $"ORD-{_orderCounter:D4}";

        _orders.Add((orderId, productId, qty));

        // Publish event — InventoryModule và NotifierModule sẽ react
        // SalesModule KHÔNG biết ai sẽ nhận event này (Principle #5)
        _eventBus.Publish(new OrderCreatedEvent
        {
            OrderId = orderId,
            ProductId = productId,
            Quantity = qty
        });

        return $"🛒 Order {orderId} created: {qty}x {productId}";
    }

    private string ListOrders()
    {
        if (_orders.Count == 0) return "📭 No orders yet.";
        var lines = _orders.Select((o, i) => $"  {i + 1}. {o.orderId}: {o.qty}x {o.productId}");
        return string.Join("\n", lines);
    }
}
