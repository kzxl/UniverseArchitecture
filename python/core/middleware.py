"""
Middleware — Lực hấp dẫn (Gravity). Principle #7.
Chain of Responsibility pattern.
"""
from abc import ABC, abstractmethod
from dataclasses import dataclass, field
from typing import Any, Callable
import time


@dataclass
class ModuleContext:
    """Ngữ cảnh cho mỗi dispatch, đi qua middleware pipeline."""
    module_name: str
    command: str
    args: list[str]
    result: str | None = None
    items: dict[str, Any] = field(default_factory=dict)
    timestamp: float = field(default_factory=time.time)


class IMiddleware(ABC):
    """Middleware interface — Gravity."""

    @abstractmethod
    def invoke(self, context: ModuleContext, next_fn: Callable[[], None]) -> None:
        ...


class LoggingMiddleware(IMiddleware):
    """Ghi log mọi dispatch."""

    def __init__(self) -> None:
        self.logs: list[str] = []

    def invoke(self, context: ModuleContext, next_fn: Callable[[], None]) -> None:
        ts = time.strftime("%H:%M:%S", time.localtime(context.timestamp))
        log = f"[LOG] {ts} → {context.module_name}.{context.command}({', '.join(context.args)})"
        next_fn()
        log += f" → {context.result}"
        self.logs.append(log)


class TimingMiddleware(IMiddleware):
    """Đo thời gian dispatch."""

    def invoke(self, context: ModuleContext, next_fn: Callable[[], None]) -> None:
        start = time.perf_counter_ns()
        next_fn()
        context.items["elapsed_ns"] = time.perf_counter_ns() - start


class ErrorHandlingMiddleware(IMiddleware):
    """Bắt exception, trả error message."""

    def invoke(self, context: ModuleContext, next_fn: Callable[[], None]) -> None:
        try:
            next_fn()
        except Exception as e:
            context.result = f"❌ Error: {e}"
