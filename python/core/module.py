"""
Quy luật vật lý (Physics Law) — Abstract Base Class bất biến mà MỌI module phải tuân thủ.
Giống như lực hấp dẫn tác động lên mọi vật thể trong vũ trụ,
IModule định nghĩa "hợp đồng" mà mọi module phải ký.
"""
from abc import ABC, abstractmethod


class IModule(ABC):
    """Interface bất biến cho mọi module trong Universe."""

    @property
    @abstractmethod
    def name(self) -> str:
        """Tên duy nhất của module."""
        ...

    @property
    @abstractmethod
    def description(self) -> str:
        """Mô tả ngắn."""
        ...

    @property
    @abstractmethod
    def commands(self) -> list[str]:
        """Danh sách commands hỗ trợ."""
        ...

    @abstractmethod
    def execute(self, command: str, args: list[str]) -> str:
        """Thực thi command với args, trả về kết quả."""
        ...
