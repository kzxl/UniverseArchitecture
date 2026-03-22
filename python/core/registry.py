"""
Registry — Trung tâm đăng ký module.
Module tự đăng ký, Core không biết chi tiết module.
Dict-based lookup, case-insensitive.
"""
from .module import IModule


class ModuleRegistry:
    """Registry quản lý và dispatch commands tới modules."""

    def __init__(self) -> None:
        self._modules: dict[str, IModule] = {}

    def register(self, module: IModule) -> None:
        """Đăng ký 1 module. Trùng tên → raise."""
        key = module.name.lower()
        if key in self._modules:
            raise ValueError(f"Module '{module.name}' already registered.")
        self._modules[key] = module

    def dispatch(self, module_name: str, command: str, args: list[str]) -> str:
        """Dispatch command tới module phù hợp."""
        module = self._modules.get(module_name.lower())
        if module is None:
            available = ", ".join(self._modules.keys())
            raise KeyError(f"Module '{module_name}' not found. Available: {available}")
        return module.execute(command, args)

    def get_all(self) -> dict[str, IModule]:
        """Lấy tất cả modules."""
        return dict(self._modules)

    @property
    def count(self) -> int:
        """Số lượng modules."""
        return len(self._modules)
