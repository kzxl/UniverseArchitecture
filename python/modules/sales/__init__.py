"""
SalesModule — Tạo đơn hàng, publish OrderCreatedEvent.
KHÔNG import InventoryModule — chỉ publish event (Principle #5).
"""
from core.module import IModule


class SalesModule(IModule):
    """🛒 Sales order management — publishes OrderCreatedEvent."""

    def __init__(self, event_bus) -> None:
        self._event_bus = event_bus
        self._orders: list[dict] = []
        self._order_counter = 0

    @property
    def name(self) -> str:
        return "sales"

    @property
    def description(self) -> str:
        return "🛒 Sales order management"

    @property
    def commands(self) -> list[str]:
        return ["create-order", "list-orders"]

    def execute(self, command: str, args: list[str]) -> str:
        match command.lower():
            case "create-order":
                return self._create_order(args)
            case "list-orders":
                return self._list_orders()
            case _:
                return f"❓ Unknown command: {command}"

    def _create_order(self, args: list[str]) -> str:
        if len(args) < 2:
            return "Error: usage: sales create-order <productId> <quantity>"
        pid = args[0].upper()
        try:
            qty = int(args[1])
        except ValueError:
            return f"Error: invalid quantity '{args[1]}'"

        self._order_counter += 1
        order_id = f"ORD-{self._order_counter:04d}"
        self._orders.append({"order_id": order_id, "product_id": pid, "qty": qty})

        # Publish event — InventoryModule và NotifierModule sẽ react
        self._event_bus.publish("OrderCreated", {
            "order_id": order_id,
            "product_id": pid,
            "quantity": qty,
        })

        return f"🛒 Order {order_id} created: {qty}x {pid}"

    def _list_orders(self) -> str:
        if not self._orders:
            return "📭 No orders yet."
        lines = [f"  {i+1}. {o['order_id']}: {o['qty']}x {o['product_id']}"
                 for i, o in enumerate(self._orders)]
        return "\n".join(lines)
