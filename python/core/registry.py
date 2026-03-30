"""
Registry — Trung tâm đăng ký module.
Upgraded: EventBus, Middleware pipeline, Lifecycle hooks.
"""
import asyncio
from .module import IModule
from .eventbus import EventBus
from .lifecycle import IModuleLifecycle
from .middleware import IMiddleware, ModuleContext


class ModuleRegistry:
    """Registry quản lý và dispatch commands tới modules qua middleware pipeline."""

    def __init__(self) -> None:
        self._modules: dict[str, IModule] = {}
        self._middlewares: list[IMiddleware] = []
        self.event_bus = EventBus()

    def register(self, module: IModule) -> None:
        """Đăng ký 1 module. Trùng tên → raise."""
        key = module.name.lower()
        if key in self._modules:
            raise ValueError(f"Module '{module.name}' already registered.")
        self._modules[key] = module

    async def register_async(self, module: IModule) -> None:
        """Đăng ký module với lifecycle hooks."""
        if isinstance(module, IModuleLifecycle):
            await module.on_initializing()
        self.register(module)
        if isinstance(module, IModuleLifecycle):
            await module.on_initialized()

    def add_middleware(self, mw: IMiddleware) -> None:
        """Thêm middleware vào pipeline. FIFO."""
        self._middlewares.append(mw)

    def dispatch(self, module_name: str, command: str, args: list[str]) -> str:
        """Dispatch command qua middleware pipeline (sync)."""
        module = self._modules.get(module_name.lower())
        if module is None:
            available = ", ".join(self._modules.keys())
            raise KeyError(f"Module '{module_name}' not found. Available: {available}")

        if not self._middlewares:
            return module.execute(command, args)

        context = ModuleContext(
            module_name=module_name,
            command=command,
            args=args,
        )
        self._run_pipeline(context, module)
        return context.result if context.result is not None else module.execute(command, args)

    async def dispatch_async(self, module_name: str, command: str, args: list[str]) -> str:
        """Dispatch command (async). Cho phép module hỗ trợ async execution."""
        module = self._modules.get(module_name.lower())
        if module is None:
            available = ", ".join(self._modules.keys())
            raise KeyError(f"Module '{module_name}' not found. Available: {available}")

        if not self._middlewares:
            return module.execute(command, args)

        context = ModuleContext(
            module_name=module_name,
            command=command,
            args=args,
        )
        self._run_pipeline(context, module)
        return context.result if context.result is not None else module.execute(command, args)

    def _run_pipeline(self, context: ModuleContext, module: IModule) -> None:
        index = [-1]  # mutable counter

        def next_fn() -> None:
            index[0] += 1
            if index[0] < len(self._middlewares):
                self._middlewares[index[0]].invoke(context, next_fn)
            else:
                context.result = module.execute(context.command, context.args)

        next_fn()

    async def shutdown(self) -> None:
        """Shutdown tất cả modules có lifecycle."""
        for module in self._modules.values():
            if isinstance(module, IModuleLifecycle):
                await module.on_shutting_down()
                await module.on_shutdown()

    def get_all(self) -> dict[str, IModule]:
        return dict(self._modules)

    @property
    def count(self) -> int:
        return len(self._modules)

    @property
    def middleware_count(self) -> int:
        return len(self._middlewares)
