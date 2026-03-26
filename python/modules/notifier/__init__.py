"""
NotifierModule — Demo EventBus + Lifecycle.
Subscribes to events without importing source modules.
"""
from dataclasses import dataclass
from core.module import IModule
from core.eventbus import EventBus
from core.lifecycle import IModuleLifecycle


@dataclass
class CalculationPerformedEvent:
    operation: str
    result: str


@dataclass
class GreetingEvent:
    name: str
    message: str


class NotifierModule(IModule, IModuleLifecycle):
    """🔔 Listens to events from other modules."""

    def __init__(self, event_bus: EventBus) -> None:
        self._event_bus = event_bus
        self._notifications: list[str] = []

    @property
    def name(self) -> str:
        return "notifier"

    @property
    def description(self) -> str:
        return "🔔 Listens to events from other modules"

    @property
    def commands(self) -> list[str]:
        return ["history", "count", "clear"]

    def execute(self, command: str, args: list[str]) -> str:
        match command.lower():
            case "history":
                if not self._notifications:
                    return "📭 No notifications yet."
                return "\n".join(
                    f"  {i+1}. {n}"
                    for i, n in enumerate(self._notifications[-10:])
                )
            case "count":
                return f"📬 {len(self._notifications)} notification(s)"
            case "clear":
                count = len(self._notifications)
                self._notifications.clear()
                return f"🗑️ Cleared {count} notification(s)"
            case _:
                return f"❓ Unknown command: {command}"

    # Lifecycle
    async def on_initializing(self) -> None:
        pass

    async def on_initialized(self) -> None:
        self._event_bus.subscribe("CalculationPerformed", self._on_calculation)
        self._event_bus.subscribe("Greeting", self._on_greeting)

    async def on_shutting_down(self) -> None:
        self._event_bus.unsubscribe("CalculationPerformed", self._on_calculation)
        self._event_bus.unsubscribe("Greeting", self._on_greeting)

    async def on_shutdown(self) -> None:
        pass

    def _on_calculation(self, event: CalculationPerformedEvent) -> None:
        self._notifications.append(f"🧮 Calculation: {event.operation} = {event.result}")

    def _on_greeting(self, event: GreetingEvent) -> None:
        self._notifications.append(f"👋 Greeting: {event.message}")
