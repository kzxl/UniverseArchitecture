"""
InventoryModule — Quản lý kho (Data Sovereignty).
Demo Principle #6: Data Sovereignty + #5: EventBus communication.
"""
from core.module import IModule


class InventoryModule(IModule):
    """📦 Inventory management — owns stock data exclusively."""

    def __init__(self, event_bus) -> None:
        self._event_bus = event_bus
        self._stock: dict[str, int] = {
            "PROD-001": 100,
            "PROD-002": 50,
            "PROD-003": 200,
        }

    @property
    def name(self) -> str:
        return "inventory"

    @property
    def description(self) -> str:
        return "📦 Inventory management (stock tracking)"

    @property
    def commands(self) -> list[str]:
        return ["check", "add", "deduct", "list"]

    def execute(self, command: str, args: list[str]) -> str:
        match command.lower():
            case "check":
                return self._check_stock(args)
            case "add":
                return self._add_stock(args)
            case "deduct":
                return self._deduct_stock(args)
            case "list":
                return self._list_stock()
            case _:
                return f"❓ Unknown command: {command}"

    def _check_stock(self, args: list[str]) -> str:
        if len(args) < 1:
            return "Error: usage: inventory check <productId>"
        pid = args[0].upper()
        qty = self._stock.get(pid)
        if qty is None:
            return f"❌ Product '{pid}' not found"
        return f"📦 {pid}: {qty} units in stock"

    def _add_stock(self, args: list[str]) -> str:
        if len(args) < 2:
            return "Error: usage: inventory add <productId> <quantity>"
        pid = args[0].upper()
        try:
            qty = int(args[1])
        except ValueError:
            return f"Error: invalid quantity '{args[1]}'"
        self._stock[pid] = self._stock.get(pid, 0) + qty
        self._event_bus.publish("StockChanged", {"product_id": pid, "quantity": qty, "action": "added"})
        return f"✅ Added {qty} units to {pid}. New stock: {self._stock[pid]}"

    def _deduct_stock(self, args: list[str]) -> str:
        if len(args) < 2:
            return "Error: usage: inventory deduct <productId> <quantity>"
        pid = args[0].upper()
        try:
            qty = int(args[1])
        except ValueError:
            return f"Error: invalid quantity '{args[1]}'"
        current = self._stock.get(pid, 0)
        if current < qty:
            return f"❌ Insufficient stock for {pid}. Available: {current}"
        self._stock[pid] = current - qty
        self._event_bus.publish("StockChanged", {"product_id": pid, "quantity": qty, "action": "deducted"})
        return f"📤 Deducted {qty} units from {pid}. Remaining: {self._stock[pid]}"

    def _list_stock(self) -> str:
        if not self._stock:
            return "📭 No products in stock."
        lines = [f"  {i+1}. {pid}: {qty} units" for i, (pid, qty) in enumerate(self._stock.items())]
        return "\n".join(lines)
