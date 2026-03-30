"""
Conformance Tests — Behavioral parity checks aligned with conformance/spec.yaml.
Run: python -m unittest conformance.test_conformance -v
"""
import sys
import os
import unittest

# Add parent to path for imports
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from core.registry import ModuleRegistry
from core.eventbus import EventBus
from modules.calculator import CalculatorModule
from modules.greeter import GreeterModule
from modules.inventory import InventoryModule
from modules.sales import SalesModule


def create_registry():
    r = ModuleRegistry()
    r.register(CalculatorModule())
    r.register(GreeterModule())
    return r


class RegistryCoreTests(unittest.TestCase):
    """═══════════════ Registry Core ═══════════════"""

    def test_calculator_add(self):
        r = create_registry()
        result = r.dispatch("calculator", "add", ["10", "25"])
        self.assertIn("10 + 25 = 35", result)

    def test_calculator_sub(self):
        r = create_registry()
        result = r.dispatch("calculator", "sub", ["100", "37"])
        self.assertIn("100 - 37 = 63", result)

    def test_calculator_mul(self):
        r = create_registry()
        result = r.dispatch("calculator", "mul", ["7", "8"])
        self.assertIn("7 * 8 = 56", result)

    def test_calculator_div(self):
        r = create_registry()
        result = r.dispatch("calculator", "div", ["22", "7"])
        self.assertIn("22 / 7", result)

    def test_calculator_div_by_zero(self):
        r = create_registry()
        result = r.dispatch("calculator", "div", ["10", "0"])
        self.assertIn("division by zero", result.lower())

    def test_greeter_hello(self):
        r = create_registry()
        result = r.dispatch("greeter", "hello", ["Universe"])
        self.assertIn("Universe", result)

    def test_duplicate_registration_raises(self):
        r = ModuleRegistry()
        r.register(CalculatorModule())
        with self.assertRaises(ValueError):
            r.register(CalculatorModule())

    def test_case_insensitive_module_lookup(self):
        r = create_registry()
        result = r.dispatch("CALCULATOR", "add", ["1", "2"])
        self.assertIn("1 + 2 = 3", result)

    def test_unknown_module_raises(self):
        r = create_registry()
        with self.assertRaises(KeyError):
            r.dispatch("nonexistent", "x", [])


class EventBusTests(unittest.TestCase):
    """═══════════════ EventBus ═══════════════"""

    def test_publish_subscribe(self):
        bus = EventBus()
        received = []
        bus.subscribe("TestEvent", lambda e: received.append(e))
        bus.publish("TestEvent", {"message": "hello"})
        self.assertEqual(len(received), 1)
        self.assertEqual(received[0]["message"], "hello")

    def test_unsubscribe(self):
        bus = EventBus()
        count = [0]
        handler = lambda _: count.__setitem__(0, count[0] + 1)
        bus.subscribe("TestEvent", handler)
        bus.publish("TestEvent", "first")
        self.assertEqual(count[0], 1)
        bus.unsubscribe("TestEvent", handler)
        bus.publish("TestEvent", "second")
        self.assertEqual(count[0], 1)  # Should NOT increment


class EnterpriseScenarioTests(unittest.TestCase):
    """═══════════════ Enterprise Scenario ═══════════════"""

    def test_sales_create_order(self):
        bus = EventBus()
        r = ModuleRegistry()
        r.register(SalesModule(bus))
        result = r.dispatch("sales", "create-order", ["PROD-001", "5"])
        self.assertIn("ORD-", result)

    def test_inventory_check_stock(self):
        bus = EventBus()
        r = ModuleRegistry()
        r.register(InventoryModule(bus))
        result = r.dispatch("inventory", "check", ["PROD-001"])
        self.assertIn("PROD-001", result)

    def test_inventory_deduct_insufficient(self):
        bus = EventBus()
        r = ModuleRegistry()
        r.register(InventoryModule(bus))
        result = r.dispatch("inventory", "deduct", ["PROD-001", "999"])
        self.assertIn("Insufficient", result)

    def test_inventory_list(self):
        bus = EventBus()
        r = ModuleRegistry()
        r.register(InventoryModule(bus))
        result = r.dispatch("inventory", "list", [])
        self.assertIn("PROD-001", result)


if __name__ == "__main__":
    unittest.main()
