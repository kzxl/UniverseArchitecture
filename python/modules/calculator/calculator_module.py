"""
Calculator Module — Một "ngôi sao" trong vũ trụ.
Tự chứa, tuân thủ IModule interface.
Xóa folder này → app vẫn chạy (Acceptance Test #1).
"""
from core.module import IModule


class CalculatorModule(IModule):

    @property
    def name(self) -> str:
        return "calculator"

    @property
    def description(self) -> str:
        return "Basic arithmetic operations (add, sub, mul, div)"

    @property
    def commands(self) -> list[str]:
        return ["add", "sub", "mul", "div"]

    def execute(self, command: str, args: list[str]) -> str:
        if len(args) < 2:
            return "Error: need 2 numbers. Usage: calculator add 1 2"

        try:
            a, b = float(args[0]), float(args[1])
        except ValueError:
            return f"Error: invalid numbers '{args[0]}' '{args[1]}'"

        match command.lower():
            case "add": return f"{a:g} + {b:g} = {a + b:g}"
            case "sub": return f"{a:g} - {b:g} = {a - b:g}"
            case "mul": return f"{a:g} * {b:g} = {a * b:g}"
            case "div":
                if b == 0:
                    return "Error: division by zero"
                return f"{a:g} / {b:g} = {a / b:g}"
            case _:
                return f"Unknown command '{command}'. Available: {', '.join(self.commands)}"
