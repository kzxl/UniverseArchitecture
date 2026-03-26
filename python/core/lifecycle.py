"""
ModuleLifecycle — Vòng đời ngôi sao (Star Lifecycle).
Optional interface — module không implement thì bỏ qua.
"""
from abc import ABC, abstractmethod


class IModuleLifecycle(ABC):
    """Optional lifecycle hooks for modules."""

    @abstractmethod
    async def on_initializing(self) -> None:
        """Gọi TRƯỚC khi module được đăng ký."""
        ...

    @abstractmethod
    async def on_initialized(self) -> None:
        """Gọi SAU khi module đã đăng ký."""
        ...

    @abstractmethod
    async def on_shutting_down(self) -> None:
        """Gọi TRƯỚC khi shutdown."""
        ...

    @abstractmethod
    async def on_shutdown(self) -> None:
        """Gọi SAU khi đã shutdown."""
        ...
