"""
EventBus — Sóng hấp dẫn (Gravitational Waves).
Giao tiếp gián tiếp giữa modules. Principle #5.
"""
from typing import Any, Callable


class EventBus:
    """In-process event bus. String-keyed, type-safe via convention."""

    def __init__(self) -> None:
        self._handlers: dict[str, list[Callable]] = {}

    def publish(self, event_type: str, event: Any) -> None:
        """Publish event to all subscribers."""
        for handler in list(self._handlers.get(event_type, [])):
            handler(event)

    def subscribe(self, event_type: str, handler: Callable) -> None:
        """Subscribe to event type."""
        self._handlers.setdefault(event_type, []).append(handler)

    def unsubscribe(self, event_type: str, handler: Callable) -> None:
        """Remove subscription."""
        handlers = self._handlers.get(event_type)
        if handlers and handler in handlers:
            handlers.remove(handler)

    @property
    def type_count(self) -> int:
        return len(self._handlers)

    @property
    def handler_count(self) -> int:
        return sum(len(h) for h in self._handlers.values())
